using System.Collections.Generic;
using API.DTOs;
using API.DTOs.Responses;
using API.Models;

namespace API.Interfaces
{
    public interface IEvaluationServices
    {
         public CommentResponse createComment(Comment obj);
         public Evaluation createEvaluation(Evaluation obj);
         public IEnumerable<EvaluatorDto> getAllEvaluations(string publicationId);
         public IEnumerable<CommentResponse> getAllComments(string publicationId);
         public CommentResponse getOneComment(string id, string pubId);
         public EvaluatorDto getOneEvaluation(string userId, string pubId);
         public void deleteComment(string id);
         public void deleteEvaluation(string userId, string publicationId);
         public CommentResponse reply(Reply obj);
         public Evaluation updateEvaluation(Evaluation obj);
         public void upVote(Vote obj);
         public void deleteUpVote(Vote obj);
         public void downVote(Vote obj);
         public void deleteDownVote(Vote obj);
    }
}