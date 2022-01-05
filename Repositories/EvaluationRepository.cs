using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.DTOs.Responses;
using API.Interfaces;
using API.Models;
using Neo4jClient;

namespace API.Repositories
{
    public class EvaluationRepository : IEvaluation
    {
        private readonly IGraphClient _neo4jConnection;
        public EvaluationRepository(IGraphClient neo4jConnection)
        {
            _neo4jConnection = neo4jConnection;
        }

        public async Task<Evaluation> createEvaluation(Evaluation obj){
            obj.EvaluatedAt = DateTime.Now;
            var result = await _neo4jConnection.Cypher
            .Match("(u:User {Id:'"+obj.UserId+"'}),(p:Publication {Id:'"+obj.PubId+"'})")
            .Create("(u)-[e:Evaluated {EvaluatedAt:'"+obj.EvaluatedAt+"',Rate:"+obj.Rate+"}]->(p)")
            .Return((e) => new Evaluation{EvaluatedAt = e.As<Evaluation>().EvaluatedAt,
                                          Rate = e.As<Evaluation>().Rate
            }).ResultsAsync;

            return result.First();
        }

        public async Task<CommentResponse> createComment(Comment obj)
        {
            obj.CommentedAt = DateTime.Now;
            var result = await _neo4jConnection.Cypher
            .Match("(u:User {Id:'"+obj.UserId+"'}),(p:Publication {Id:'"+obj.PubId+"'})")
            .Create("(u)-[t:Commented]->(c:Comment {Id:'"+obj.Id+"',"+ 
                                                   $"CommentedAt:'{obj.CommentedAt}',"+
                                                   $"Content:'{obj.Content}'"+
            "})-[r:Reply]->(p)")
            .Return((c) => new CommentResponse{ Id = c.As<Comment>().Id,            
                                                CommentedAt = c.As<Comment>().CommentedAt,
                                                Content = c.As<Comment>().Content            
            }).ResultsAsync;
            var evaluator = getOneEvaluation(obj.UserId, obj.PubId).Result;
            result.First().UserId = evaluator.Id;
            result.First().Name = evaluator.Name;
            result.First().ProfileImageUrl = evaluator.ProfileImageUrl;
            result.First().Rate = evaluator.Evaluation.Rate;
            result.First().UpVotes = 0;
            result.First().DownVotes = 0;
            return result.First(); 
        }

        public async void deleteEvaluation(string userId, string publicationId){
            await _neo4jConnection.Cypher
            .Match("(u:User {Id:'"+userId+"'})-[e:Evaluated]->(p:Publication {Id:'"+publicationId+"'})")
            .Delete("e").ExecuteWithoutResultsAsync();
        }

        public async void deleteComment(string id){
            await _neo4jConnection.Cypher
            .Match("(c:Comment {Id:'"+id+"'})").DetachDelete("c").ExecuteWithoutResultsAsync();
        }

        public async Task<IEnumerable<EvaluatorDto>> getAllEvaluations(string publicationId){
            var evaluators = await _neo4jConnection.Cypher
            .Match("(u:User)-[e:Evaluated]->(p:Publication {Id:'"+publicationId+"'})")
            .ReturnDistinct<EvaluatorDto>("u").ResultsAsync;
            for (int i = 0; i < evaluators.Count(); i++)
            {
                var result = await _neo4jConnection.Cypher
                .Match("(u:User {Id'"+evaluators.ElementAt(i).Id+"'})-[e:Evaluated]->(p:Publication {Id:'"+publicationId+"'})")
                .Return<Evaluation>("e").ResultsAsync;
                evaluators.ElementAt(i).Evaluation = result.First();
            }
            return evaluators;

            // (u) => new EvaluatorDto{
            // Id = u.As<EvaluatorDto>().Id, 
            // Name = u.As<EvaluatorDto>().Name,
            // ProfileImageUrl = u.As<EvaluatorDto>().ProfileImageUrl}
            
            // (e) => new Evaluation{
            // EvaluatedAt = e.As<Evaluation>().EvaluatedAt,
            // Rate = e.As<Evaluation>().Rate
            // }
        }

        public async Task<EvaluatorDto> getOneEvaluation(string userId, string publicationId)
        {
            var evaluator = await _neo4jConnection.Cypher
            .Match("(u:User {Id:'"+userId+"'})-[e:Evaluated]->(p:Publication {Id:'"+publicationId+"'})")
            .Return<EvaluatorDto>("u").ResultsAsync;  
            var evaluation = await _neo4jConnection.Cypher
            .Match("(u:User {Id:'"+userId+"'})-[e:Evaluated]->(p:Publication {Id:'"+publicationId+"'})")
            .Return((e) => new Evaluation{EvaluatedAt = e.As<Evaluation>().EvaluatedAt,
                                                  Rate = e.As<Evaluation>().Rate
            }).ResultsAsync;
            if(evaluation.Any()){
                evaluator.First().Evaluation = evaluation.First();
                return evaluator.First();
            }else{
                return null;
            }
            
        }

        public async Task<IEnumerable<CommentResponse>> getAllComments(string publicationId){
            var comments = await _neo4jConnection.Cypher
            .Match("(u:User)-[e:Commented]->(c:Comment)-[r:Reply]->(p:Publication {Id:'"+publicationId+"'})")
            .ReturnDistinct((u) => new CommentResponse{
                UserId = u.As<UserDto>().Id,
                Name = u.As<UserDto>().Name,
                ProfileImageUrl = u.As<UserDto>().ProfileImageUrl
            }).ResultsAsync;
            if(comments.Any()){
                for (int i = 0; i < comments.Count(); i++)
                {
                    var result = await _neo4jConnection.Cypher
                    .Match("(u:User {Id:'"+comments.ElementAt(i).UserId+"'})-[e:Commented]->(c:Comment)-[r:Reply]->(p:Publication {Id:'"+publicationId+"'})")
                    .Return((c) => new Comment{
                    Id = c.As<Comment>().Id, CommentedAt = c.As<Comment>().CommentedAt, Content = c.As<Comment>().Content
                    }).ResultsAsync;
                    var upVotes = await getUpVotes(result.First().Id);
                    var downVotes = await getDownVotes(result.First().Id);
                    var evaluation = await getOneEvaluation(comments.ElementAt(i).UserId, publicationId);
                    comments.ElementAt(i).Rate = evaluation.Evaluation.Rate;
                    comments.ElementAt(i).Id = result.First().Id;
                    comments.ElementAt(i).CommentedAt = result.First().CommentedAt;
                    comments.ElementAt(i).Content = result.First().Content;
                    comments.ElementAt(i).UpVotes = upVotes.Count();                    
                    comments.ElementAt(i).DownVotes = downVotes.Count();                    
                }
                return comments;
            }else{
                return comments;
            }
        }

        public async Task<CommentResponse> getOneComment(string id, string pubId){
            var result = await _neo4jConnection.Cypher
            .Match("(c:Comment {Id:'"+id+"'})")
            .Return((c) => new Comment{Id = c.As<Comment>().Id,
                                       CommentedAt = c.As<Comment>().CommentedAt,
                                       Content = c.As<Comment>().Content
            }).ResultsAsync;
            var user = getUser(result.First().Id).Result;
            var evaluator = await getOneEvaluation(user.Id, pubId);
            var upVotes = await getUpVotes(id);
            var downVotes = await getDownVotes(id); 
            var replies = await getReply(id, pubId);
            var response = new CommentResponse{
                UserId = user.Id,
                Name = evaluator.Name,
                ProfileImageUrl = evaluator.ProfileImageUrl,
                Rate = evaluator.Evaluation.Rate,
                Id = result.First().Id,
                CommentedAt = result.First().CommentedAt,
                Content = result.First().Content,
                UpVotes = upVotes.Count(),
                DownVotes = downVotes.Count(),
                Replies = replies
            };
            return response;
        }

        public async Task<CommentResponse> reply(Reply obj)
        {
            obj.CommentedAt = DateTime.Now;
            var result = await _neo4jConnection.Cypher
            .Match("(c:Comment {Id:'"+obj.CommentId+"'}),(u:User {Id:'"+obj.UserId+"'})")
            .Create("(u)-[r:Replied]->(a:Comment {"+
            $"Id:'{obj.Id}',CommentedAt:'{obj.CommentedAt}', Content:'{obj.Content}'"+
            "})-[p:Reply]->(c)")
            .Return((a) => new CommentResponse{Id = a.As<Comment>().Id,
                                                CommentedAt = a.As<Comment>().CommentedAt,
                                                Content = a.As<Comment>().Content
            }).ResultsAsync;
            var evaluator = getOneEvaluation(obj.UserId, obj.PubId).Result;
            result.First().UserId = evaluator.Id;
            result.First().Name = evaluator.Name;
            result.First().ProfileImageUrl = evaluator.ProfileImageUrl;
            result.First().Rate = evaluator.Evaluation.Rate;
            result.First().UpVotes = 0;
            result.First().DownVotes = 0;
            return result.First();
        }

        public async Task<IEnumerable<CommentResponse>> getReply(string commentId, string pubId)
        {
            var replies = await _neo4jConnection.Cypher
            .Match("(r:Comment)-[v:Reply]->(c:Comment {Id:'"+commentId+"'})")
            .ReturnDistinct<CommentResponse>("r").ResultsAsync;

            // (r) => new CommentResponse{
            //                             Id = r.As<Comment>().Id,
            //                             CommentedAt = r.As<Comment>().CommentedAt,
            //                             Content = r.As<Comment>().Content
            // }

            if(replies.Any()){
                for (int i = 0; i < replies.Count(); i++)
                {
                    var upVotes = await getUpVotes(replies.ElementAt(i).Id);
                    var downVotes = await getDownVotes(replies.ElementAt(i).Id);
                    var user = await getReplyUser(replies.ElementAt(i).Id);
                    var evaluator = getOneEvaluation(user.Id, pubId).Result;
                    replies.ElementAt(i).UserId = evaluator.Id;
                    replies.ElementAt(i).Name = evaluator.Name;
                    replies.ElementAt(i).ProfileImageUrl = evaluator.ProfileImageUrl;
                    replies.ElementAt(i).Rate = evaluator.Evaluation.Rate;
                    replies.ElementAt(i).UpVotes = upVotes.Count();
                    replies.ElementAt(i).DownVotes = downVotes.Count();
                }
                return replies;
            }else{
                return replies;
            }
        }

        public async Task<Evaluation> updateEvaluation(Evaluation obj)
        {
           var result = await _neo4jConnection.Cypher
            .Match("(u:User {Id:'"+obj.UserId+"'})-[e:Evaluated]->(p:Publication {Id:'"+obj.PubId+"'})")
            .Set($"e.Rate = '{obj.Rate}'")
            .Return<Evaluation>("e").ResultsAsync;
            return result.First();

            // (e) => new Evaluation{EvaluatedAt = e.As<Evaluation>().EvaluatedAt,
            //                               Rate = e.As<Evaluation>().Rate
            // }
        }

        public async Task<UserDto> getUser(string id){
            var result = await _neo4jConnection.Cypher
            .Match("(u:User)-[e:Commented]->(c:Comment {Id:'"+id+"'})")
            .Return<UserDto>("u").ResultsAsync;
            return result.First();

            // (u) => new UserDto{
            //     Id = u.As<UserDto>().Id,
            //     Name = u.As<UserDto>().Name,
            //     ProfileImageUrl = u.As<UserDto>().ProfileImageUrl
            // }
        }

        public async Task<UserDto> getReplyUser(string id){
            var result = await _neo4jConnection.Cypher
            .Match("(u:User)-[e:Replied]->(c:Comment {Id:'"+id+"'})")
            .Return<UserDto>("u").ResultsAsync;
            return result.First();

            // (u) => new UserDto{
            //     Id = u.As<UserDto>().Id,
            //     Name = u.As<UserDto>().Name,
            //     ProfileImageUrl = u.As<UserDto>().ProfileImageUrl
            // }
        }

        public async Task<bool> IsCommentExists(string id)
        {
            var result = await _neo4jConnection.Cypher
            .Match("(c:Comment {Id:'"+id+"'})")
            .Return<Comment>("c").ResultsAsync;
            return result.Any();

            // (c) => new Comment{Id = c.As<Comment>().Id,
            //                            CommentedAt = c.As<Comment>().CommentedAt,
            //                            Content = c.As<Comment>().Content
            // }
        }

        public async void upVote(Vote obj){
            var date = DateTime.Now;
            await _neo4jConnection.Cypher
            .Match("(u:User {Id:'"+obj.UserId+"'}),(c:Comment {Id:'"+obj.CommentId+"'})")
            .Create("(u)-[v:UpVoted {Date:'"+date+"'}]->(c)").ExecuteWithoutResultsAsync();
        }    
        public async void downVote(Vote obj){
            var date = DateTime.Now;
            await _neo4jConnection.Cypher
            .Match("(u:User {Id:'"+obj.UserId+"'}),(c:Comment {Id:'"+obj.CommentId+"'})")
            .Create("(u)-[v:DownVoted {Date:'"+date+"'}]->(c)").ExecuteWithoutResultsAsync();
        }

        public async void deleteUpVote(Vote obj){
            await _neo4jConnection.Cypher
            .Match("(u:User {Id:'"+obj.UserId+"'})-[v:UpVoted]->(c:Comment {Id:'"+obj.CommentId+"'})")
            .Delete("v").ExecuteWithoutResultsAsync();
        }
        

        public async void deleteDownVote(Vote obj){
            await _neo4jConnection.Cypher
            .Match("(u:User {Id:'"+obj.UserId+"'})-[v:DownVoted]->(c:Comment {Id:'"+obj.CommentId+"'})")
            .Delete("v").ExecuteWithoutResultsAsync();
        }

        public async Task<IEnumerable<EvaluatorDto>> getUpVotes(string commentId){
            var result = await _neo4jConnection.Cypher
            .Match("(u:User)-[v:UpVoted]->(c:Comment {Id:'"+commentId+"'})")
            .ReturnDistinct<EvaluatorDto>("u").ResultsAsync;

            return result;

            // (u) => new EvaluatorDto{
            //     Id = u.As<EvaluatorDto>().Id,
            //     Name = u.As<EvaluatorDto>().Name,
            //     ProfileImageUrl = u.As<EvaluatorDto>().ProfileImageUrl
            // }
        }

        public async Task<IEnumerable<EvaluatorDto>> getDownVotes(string commentId){
            var result = await _neo4jConnection.Cypher
            .Match("(u:User)-[v:DownVoted]->(c:Comment {Id:'"+commentId+"'})")
            .ReturnDistinct<EvaluatorDto>("u").ResultsAsync;

            return result;

            // (u) => new EvaluatorDto{
            //     Id = u.As<EvaluatorDto>().Id,
            //     Name = u.As<EvaluatorDto>().Name,
            //     ProfileImageUrl = u.As<EvaluatorDto>().ProfileImageUrl
            // }
        }
    }
}