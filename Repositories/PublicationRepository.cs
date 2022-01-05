using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using API.DTOs;
using API.Interfaces;
using API.Models;
using Neo4jClient;

namespace API.Repositories
{
    public class PublicationRepository : IPublication
    {
        private readonly IGraphClient _neo4jConnection;

        public PublicationRepository(IGraphClient neo4jConnection)
        {
            _neo4jConnection = neo4jConnection;
        }
        public async Task<Publication> create(string userId, Publication publication)
        { 
            publication.PublishedAt = DateTime.Now;
            var images = JsonSerializer.Serialize(publication.Images);
            var result = await _neo4jConnection.Cypher
            .Create("(p:Publication {Id:'"+publication.Id+"',"+
                                    $"PublishedAt:'{publication.PublishedAt}',"+
                                    $"Rote:'{publication.Rote}',"+
                                    $"Business:'{publication.Business}',"+
                                    $"Title:'{publication.Title}',"+
                                    $"Content:'{publication.Content}',"+
                                    $"Images:{images}"+
                                    "})")
            .Return((p) => new Publication{
                Id = p.As<Publication>().Id,
                PublishedAt = p.As<Publication>().PublishedAt,
                Rote = p.As<Publication>().Rote,
                Business = p.As<Publication>().Business,
                Title = p.As<Publication>().Title,
                Content = p.As<Publication>().Content,
                Images = p.As<Publication>().Images
            }).ResultsAsync;
            createdBy(userId, result.First().Id);
            result.First().Locality = about(publication).Result;
            return result.First();

            
        }

        public async Task<string> about(Publication publication){
            var result = await _neo4jConnection.Cypher
            .Match("(p:Publication {Id:'"+publication.Id+"'}),(l:Locality {Name:'"+publication.Locality+"'})")
            .Create("(p)-[r:About]->(l)")
            .Return((l) => new LocalityDto{
                Name = l.As<LocalityDto>().Name
            }).ResultsAsync;
            return result.First().Name;
        }

        public async Task<string> getAbout(string id){
            var result = await _neo4jConnection.Cypher
            .Match("(p:Publication {Id:'"+id+"'})-[r:About]->(l:Locality)")
            .Return((l) => new LocalityDto{
                Name = l.As<LocalityDto>().Name
            }).ResultsAsync;

            return result.First().Name;
        }

        public async void createThread(string rootId, string branchId)
        {
            await _neo4jConnection.Cypher
            .Match("(r:Publication {Id:'"+rootId+"'}),(b:Publication {Id:'"+branchId+"'})")
            .Create("(b)-[c:Complements]->(r)").ExecuteWithoutResultsAsync();
        }

        public async void delete(string id)
        {
            await _neo4jConnection.Cypher
            .Match("(p:Publication {Id:'"+id+"'})")
            .DetachDelete("p").ExecuteWithoutResultsAsync();
        }

        public void deleteThread(string id){
            var complements = getComplements(id).Result;
            delete(id);
            for (int i = 0; i < complements.Count(); i++)
            {
                delete(complements.ElementAt(i).Id);
            }
        }

        public async Task<IEnumerable<PublicationDto>> getAll(string userId){
            var result = await _neo4jConnection.Cypher
            .Match("(u:User {Id:'"+userId+"'})-[c:Published]->(p:Publication)")
            .ReturnDistinct<PublicationDto>("p").ResultsAsync;

            for (int i = 0; i < result.Count(); i++){
                result.ElementAt(i).Evaluation = await getRate(result.ElementAt(i).Id);
                result.ElementAt(i).Locality = await getAbout(result.ElementAt(i).Id);
            }
            return result;

            // (p) => new PublicationDto{
            //     Id = p.As<PublicationDto>().Id,
            //     PublishedAt = p.As<PublicationDto>().PublishedAt,
            //     Rote = p.As<PublicationDto>().Rote,
            //     Business = p.As<PublicationDto>().Business,
            //     Title = p.As<PublicationDto>().Title,
            //     Content = p.As<PublicationDto>().Id,
            //     Images = p.As<PublicationDto>().Images
            // }
        }

        public async Task<PublicationDto> getOne(string id)
        {
            var result = await _neo4jConnection.Cypher
            .Match("(p:Publication {Id:'"+id+"'})")
            .Return<PublicationDto>("p").ResultsAsync;
            result.First().Evaluation = await getRate(result.First().Id);
            result.First().Author = await getAuthor(result.First().Id);
            result.First().Locality = await getAbout(result.First().Id);
            result.First().TotalComments = 0;
            result.First().TotalFiveStars = starCount(result.First().Evaluation.Rates, 5);
            result.First().TotalFourStars = starCount(result.First().Evaluation.Rates, 4);
            result.First().TotalThreeStars = starCount(result.First().Evaluation.Rates, 3);
            result.First().TotalTwoStars = starCount(result.First().Evaluation.Rates, 2);
            result.First().TotalOneStars = starCount(result.First().Evaluation.Rates, 1);
            return result.First();

            // (p) => new PublicationDto{
            //     Id = p.As<PublicationDto>().Id,
            //     PublishedAt = p.As<PublicationDto>().PublishedAt,
            //     Rote = p.As<PublicationDto>().Rote,
            //     Business = p.As<PublicationDto>().Business,
            //     Title = p.As<PublicationDto>().Title,
            //     Content = p.As<PublicationDto>().Content,
            //     Images = p.As<PublicationDto>().Images}
        }

        public async Task<ThreadDto> getThread(string id){
            ThreadDto thread = new ThreadDto();
            thread.Publication = await getOne(id);
            var complement = await getComplements(id);
            thread.TotalRate = thread.Publication.Evaluation.TotalRate;
            thread.TotalAvaliations = thread.Publication.Evaluation.EvaluationsCount;
            thread.TotalComments = 0;
            thread.TotalFiveStars = starCount(thread.Publication.Evaluation.Rates, 5);
            thread.TotalFourStars = starCount(thread.Publication.Evaluation.Rates, 4);
            thread.TotalThreeStars = starCount(thread.Publication.Evaluation.Rates, 3);
            thread.TotalTwoStars = starCount(thread.Publication.Evaluation.Rates, 2);
            thread.TotalOneStars = starCount(thread.Publication.Evaluation.Rates, 1);
            IEnumerable<int> rates = thread.Publication.Evaluation.Rates;
            try{
                if(complement.Any()){
                    rates = complement.First().Evaluation.Rates;
                    for (int i = 1; i < complement.Count(); i++){
                        if(complement.ElementAt(i).Evaluation.Rates != null){
                        rates.Concat(complement.ElementAt(i).Evaluation.Rates);
                        }
                        complement.ElementAt(i).Complement = complement.ElementAt(i);
                        thread.TotalRate += complement.ElementAt(i).Evaluation.TotalRate;
                        thread.TotalAvaliations += complement.ElementAt(i).Evaluation.EvaluationsCount;
                    }
                    thread.Publication.Complement = complement.First();
                    thread.TotalRate = thread.TotalRate/(complement.Count()+1);
                    thread.TotalFiveStars = starCount(rates, 5);
                    thread.TotalFourStars = starCount(rates, 4);
                    thread.TotalThreeStars = starCount(rates, 3);
                    thread.TotalTwoStars = starCount(rates, 2);
                    thread.TotalOneStars = starCount(rates, 1);
                    return thread;
                }else{return thread;}
            }catch{return thread;}
     
        }

        public int starCount(IEnumerable<int> rates, int num){
            try{
                int count = 0;
                for (int i = 0; i < rates.Count(); i++)
                {
                    if(rates.ElementAt(i) == num){count++;}
                }
                return count;
            }catch{
                return 0;
            }
        }

        public async Task<IEnumerable<PublicationDto>> getComplements(string id){
            var result = await _neo4jConnection.Cypher
            .Match("(b:Publication {Id:'"+id+"'})<-[d:Complements]-(c:Publication)")
            .Return<PublicationDto>("c").ResultsAsync;

            if(result.Any()){
                result.First().Evaluation = await getRate(result.First().Id);
                result.First().Locality = await getAbout(result.First().Id);
                var complements = getComplements(result.First().Id).Result;
                if(complements != null){
                    result.Concat(complements);
                }
                return result;
            }else{return null;} 

            // (c) => new PublicationDto{
            //     Id = c.As<PublicationDto>().Id,
            //     PublishedAt = c.As<PublicationDto>().PublishedAt,
            //     Rote = c.As<PublicationDto>().Rote,
            //     Business = c.As<PublicationDto>().Business,
            //     Title = c.As<PublicationDto>().Title,
            //     Content = c.As<PublicationDto>().Content,
            //     Images = c.As<PublicationDto>().Images
            // }
        }

        public async Task<bool> IsPublicationExists(string id)
        {
            var result = await _neo4jConnection.Cypher
            .Match("(p:Publication {Id:'"+id+"'})")
            .Return((p) => new Publication{
                Id = p.As<Publication>().Id
            }).ResultsAsync;
            
            return result.Any();
        }

        public async void createdBy(string userId, string pubId){
            await _neo4jConnection.Cypher
            .Match("(u:User {Id:'"+userId+"'}),(p:Publication {Id:'"+pubId+"'})")
            .Create("(u)-[c:Published]->(p)").ExecuteWithoutResultsAsync();
        }

        public async Task<UserDto> getAuthor(string pubId){
            var author = await _neo4jConnection.Cypher
            .Match("(u:User)-[c:Published]->(p:Publication {Id:'"+pubId+"'})")
            .Return<UserDto>("u").ResultsAsync;

            return author.First();
        }


        public async Task<RateDto> getRate(string publicationId){
            var evaluations =await _neo4jConnection.Cypher
            .Match("(u:User)-[e:Evaluated]->(p:Publication {Id:'"+publicationId+"'})")
            .ReturnDistinct((e) => new Evaluation{
                EvaluatedAt = e.As<Evaluation>().EvaluatedAt,
                Rate = e.As<Evaluation>().Rate
            }).ResultsAsync;
            var result = new RateDto();
            IEnumerable<int> rates = evaluations.Select((x) => x.Rate);
            if(evaluations.Any()){
                decimal rate = 0;
                result.Rates = rates;
                for (int i = 0; i < evaluations.Count(); i++){
                    rate += evaluations.ElementAt(i).Rate;
                    Console.WriteLine(result.Rates);
                    if(evaluations.ElementAt(i) != null){
                        result.Rates.Append(evaluations.ElementAt(i).Rate);
                    }
                }
                result.EvaluationsCount = evaluations.Count();
                decimal? totalRate = rate/result.EvaluationsCount;
                result.TotalRate = Math.Round((decimal)totalRate, 1);
                return result;
            }else{
                return new RateDto{
                    TotalRate = 0,
                    EvaluationsCount = 0,
                    Rates = null
                };
            }
        }

        public async Task<IEnumerable<PublicationDto>> searchPublications(string userId, string query){
            var result  = await _neo4jConnection.Cypher
            .Call("db.index.fulltext.queryNodes('PubIndex','"+query+"')").Yield("node")
            .Match("(u:User {Id:'"+userId+"'})-[r:Published]->(node)").Return<PublicationDto>("node").ResultsAsync;

            IEnumerable<PublicationDto> publications = Enumerable.Empty<PublicationDto>();
            PublicationDto[] pub = new PublicationDto[result.Count()];
            for (int i = 0; i < result.Count(); i++)
            {
                pub[i] = getOne(result.ElementAt(i).Id).Result;
            }
            publications = pub;
            return publications;
        }
    }
}