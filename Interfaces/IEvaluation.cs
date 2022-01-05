using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.DTOs.Responses;
using API.Models;

namespace API.Interfaces
{
    public interface IEvaluation
    {
         public Task<CommentResponse> createComment(Comment obj);
         public Task<Evaluation> createEvaluation(Evaluation obj);
         public Task<IEnumerable<EvaluatorDto>> getAllEvaluations(string publicationId);
         public Task<EvaluatorDto> getOneEvaluation(string userId, string publicationId);
         public Task<IEnumerable<CommentResponse>> getAllComments(string publicationId);
         public Task<CommentResponse> getOneComment(string id, string pubId);
         public void deleteComment(string id);
         public void deleteEvaluation(string userId, string publicationId);
         public Task<CommentResponse> reply(Reply obj);
         public Task<Evaluation> updateEvaluation(Evaluation obj);
         public Task<bool> IsCommentExists(string id);
         public void upVote(Vote obj);
         public void deleteUpVote(Vote obj);
         public void downVote(Vote obj);
         public void deleteDownVote(Vote obj);
         //public Task<Comment> update(string userId, int publicationId, int id);
    }
}