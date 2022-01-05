using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Interfaces;
using API.Models;
using Neo4jClient;

namespace API.Repositories
{
    public class HomeRepository : IHome
    {
        private readonly IGraphClient _neo4jConnection;
        public HomeRepository(IGraphClient neo4jConnection)
        {
            _neo4jConnection = neo4jConnection;
        }
        public async Task<SearchResultDto> search(string query)
        {
            var publications = await _neo4jConnection.Cypher
            .Call("db.index.fulltext.queryNodes('PubIndex','"+query+"')").Yield("node")
            .Return<PublicationDto>("node").ResultsAsync;

            var users = await _neo4jConnection.Cypher
            .Call("db.index.fulltext.queryNodes('UserIndex','"+query+"')").Yield("node")
            .Return<UserDto>("node").ResultsAsync;

            var locations = await _neo4jConnection.Cypher
            .Call("db.index.fulltext.queryNodes('LocalityIndex','"+query+"')").Yield("node")
            .Match("(p:Publication)-[a:About]->(node)")
            .Return<PublicationDto>("p").ResultsAsync;

            publications.Concat(locations);

            return new SearchResultDto(){
                Users = users,
                Publications = publications
            };
        }

        public async Task<IEnumerable<PublicationDto>> showsPublicationsAccordingUserInterests(string userId){
            var publications = await _neo4jConnection.Cypher
            .Match("(u:User {Id:'"+userId+"'})-[i:Interested]->(l:Locality)<-[a:About]-(p:Publication)<-[e:Evaluated]-()").With("p, SUM(e.Rate)/COUNT(e) AS media").Return<PublicationDto>("p")
            .OrderBy("media DESC").Limit(10).ResultsAsync;
            
            return publications;
        }

        public async Task<IEnumerable<PublicationDto>> showsMostRecentPublicationsAccordingUserInterests(string userId){
            var publications = await _neo4jConnection.Cypher.Match("(u:User {Id:'"+userId+"'})-[i:Interested]->(l:Locality)<-[a:About]-(p:Publication)").ReturnDistinct<PublicationDto>("p").OrderBy("p.PublishedAt DESC").Limit(10).ResultsAsync;

            return publications;
        }

        public async Task<IEnumerable<PublicationDto>> showsMostRecentPublicationsAccordingUserFollowing(string userId){
            var publications = await _neo4jConnection.Cypher.Match("(user:User {Id:'"+userId+"'})-[f:Follows]->(following:User)-[r:Published]->(p:Publication)").ReturnDistinct<PublicationDto>("p")
            .OrderBy("p.PublishedAt DESC").Limit(10).ResultsAsync;

            return publications;
        }

        public async Task<IEnumerable<PublicationDto>> showsMostPopularPublicationsAccordingUserFollowing(string userId){
            var publications = await _neo4jConnection.Cypher.Match("(u:User {Id:'"+userId+"'})-[f:Follows]->(following:User)-[r:Published]->(p:Publication)<-[e:Evaluated]-()")
            .With("p, SUM(e.Rate)/COUNT(e) AS media").Return<PublicationDto>("p").OrderBy("media desc")
            .Limit(10).ResultsAsync;

            return publications;
        }

        public async Task<IEnumerable<PublicationDto>> showsMostPopularPublications(){
            var publications = await _neo4jConnection.Cypher.Match("(p:Publication)<-[e:Evaluated]-()")
            .With("p, SUM(e.Rate)/COUNT(e) AS media, COUNT(e) as c").Return<PublicationDto>("p")
            .OrderBy("media, c DESC").Limit(10).ResultsAsync;
            
            return publications;
        }

        public async Task<EvaluatorDto> showsMostRecentEvaluations(string userId)
        {
            var evaluations = await _neo4jConnection.Cypher
            .Match("(u:User {Id:'"+userId+"'})-[r:Published]->(p:Publication)<-[e:Evaluated]-(ev:User)")
            .Return((ev,e) => new EvaluatorDto{
                Id = ev.As<EvaluatorDto>().Id,
                Name = ev.As<EvaluatorDto>().Name,
                ProfileImageUrl = ev.As<EvaluatorDto>().ProfileImageUrl,
                Evaluation = e.As<Evaluation>()
            }).OrderBy("e.EvaluatedAt DESC").Limit(1).ResultsAsync;
            if(evaluations.Any()){
                return evaluations.First();
            }else{
                return null;
            }
        }

        public async Task<Comment> showsMostRecentComments(string userId)
        {
            var comments = await _neo4jConnection.Cypher
            .Match("(u:User {Id:'"+userId+"'})-[r:Published]->(p:Publication)<-[re:Reply]-(c:Comment)")
            .Return<Comment>("c").OrderBy("c.CommentedAt DESC").Limit(1).ResultsAsync;
            if(comments.Any()){
                var pub = await _neo4jConnection.Cypher
                .Match("(u:User)-[r:Published]->(p:Publication)<-[re:Reply]-(c:Comment {Id:'"+comments.First().Id+"'})").Return<PublicationDto>("p").ResultsAsync;

                var user = await _neo4jConnection.Cypher
                .Match("(c:Comment {Id:'"+comments.First().Id+"'})<-[r:Commented]-(u:User)").Return<UserDto>("u").ResultsAsync;
                comments.First().UserId = user.First().Id;
                comments.First().PubId = pub.First().Id;
                return comments.First();
            }else{
                return null;
            }
            
        }

        public async Task<UserDto> showsMostRecentFollowers(string userId)
        {
            var follower = await _neo4jConnection.Cypher
            .Match("(u:User {Id:'"+userId+"'})<-[f:Follows]-(follower:User)")
            .Return<UserDto>("follower").OrderBy("f.FollowedAt DESC").Limit(1).ResultsAsync;
            if(follower.Any()){
                return follower.First();
            }else{
                return null;
            }
        }
    }
}