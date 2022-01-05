using System;
using System.Collections.Generic;
using API.DTOs;
using API.DTOs.Responses;
using API.Interfaces;
using API.Models;

namespace API.Services
{
    public class EvaluationServices : IEvaluationServices
    {
        private readonly IEvaluation _evaluationRepository;
        private readonly IPublicationServices _publicationServices;
        private readonly IUserServices _userServices;
        public EvaluationServices(IEvaluation evaluationRepository, IPublicationServices publicationServices, IUserServices userServices)
        {
            _evaluationRepository = evaluationRepository;
            _publicationServices = publicationServices;
            _userServices = userServices;
        }

        public Evaluation createEvaluation(Evaluation obj)
        {
            if(_publicationServices.IsPublicationExists(obj.PubId) && _userServices.IsUserRegistered(obj.UserId)){
               return _evaluationRepository.createEvaluation(obj).Result;
            }else{
                throw new Exception("404: Usuario ou publicação não encontrados"){HResult = 404};
            }
        }

        public CommentResponse createComment(Comment obj)
        {
            if(_publicationServices.IsPublicationExists(obj.PubId) && _userServices.IsUserRegistered(obj.UserId)){
               obj.Id = setId(obj.UserId);
               return _evaluationRepository.createComment(obj).Result;
            }else{throw new Exception("404: Usuario ou publicação não encontrados"){HResult = 404};}
        }

        public void deleteEvaluation(string userId, string publicationId)
        {
            if(_publicationServices.IsPublicationExists(userId) && _userServices.IsUserRegistered(publicationId)){
                _evaluationRepository.deleteEvaluation(userId, publicationId);
            }else{throw new Exception("404: Usuario ou publicação não encontrados"){HResult = 404};}
        }

        public void deleteComment(string id)
        {
            if(_evaluationRepository.IsCommentExists(id).Result){
                _evaluationRepository.deleteComment(id);
            }else{throw new Exception("404: Comentário não encontrado"){HResult = 404};}
        }

        public IEnumerable<EvaluatorDto> getAllEvaluations(string publicationId)
        {
           if(_publicationServices.IsPublicationExists(publicationId)){
               return _evaluationRepository.getAllEvaluations(publicationId).Result;
           }else{throw new Exception("404: Publicação não encontrada"){HResult = 404};}
        }

        public IEnumerable<CommentResponse> getAllComments(string publicationId)
        {
            if(_publicationServices.IsPublicationExists(publicationId)){
               return _evaluationRepository.getAllComments(publicationId).Result;
           }else{throw new Exception("404: Publicação não encontrada"){HResult = 404};}
        }

        public CommentResponse getOneComment(string id, string pubId)
        {
            if(_evaluationRepository.IsCommentExists(id).Result){
                var result =_evaluationRepository.getOneComment(id, pubId).Result;
                return result;
            }else{throw new Exception("404: Comentário não encontrado"){HResult = 404};}
        }

        public CommentResponse reply(Reply obj)
        {
            if(_evaluationRepository.IsCommentExists(obj.CommentId).Result){
                if(_userServices.IsUserRegistered(obj.UserId)){
                    obj.Id = _publicationServices.setId(obj.UserId);
                    return _evaluationRepository.reply(obj).Result;
                }else{throw new Exception("404: Usuario não encontrado"){HResult = 404};}
            }else{throw new Exception("404: Comentário não encontrado"){HResult = 404};}
        }

        public Evaluation updateEvaluation(Evaluation obj)
        {
           if(_publicationServices.IsPublicationExists(obj.UserId) && _userServices.IsUserRegistered(obj.PubId)){
                    return _evaluationRepository.updateEvaluation(obj).Result;
            }else{throw new Exception("404: Usuario ou publicação não encontrados"){HResult = 404};}
        }

        public string setId(string userId){
            string id;
            do{
                id = userId+Guid.NewGuid().ToString("N");
            }while (_evaluationRepository.IsCommentExists(id).Result);
            return id;
        }

        public void upVote(Vote obj)
        {
            if(_userServices.IsUserRegistered(obj.UserId) && _evaluationRepository.IsCommentExists(obj.CommentId).Result){
                _evaluationRepository.deleteDownVote(obj);
                _evaluationRepository.upVote(obj);
            }else{throw new Exception("404: Usuario ou comentário não encontrados"){HResult = 404};}
        }

        public void deleteUpVote(Vote obj){
            if(_userServices.IsUserRegistered(obj.UserId) && _evaluationRepository.IsCommentExists(obj.CommentId).Result){
                _evaluationRepository.deleteUpVote(obj);
            }else{throw new Exception("404: Usuario ou comentário não encontrados"){HResult = 404};}
        }

        public void downVote(Vote obj)
        {
            if(_userServices.IsUserRegistered(obj.UserId) && _evaluationRepository.IsCommentExists(obj.CommentId).Result){
                _evaluationRepository.deleteUpVote(obj);
                _evaluationRepository.downVote(obj);
            }else{throw new Exception("404: Usuario ou comentário não encontrados"){HResult = 404};}
        }
        
        public void deleteDownVote(Vote obj){
            if(_userServices.IsUserRegistered(obj.UserId) && _evaluationRepository.IsCommentExists(obj.CommentId).Result){
                _evaluationRepository.deleteDownVote(obj);
            }else{throw new Exception("404: Usuario ou comentário não encontrados"){HResult = 404};}
        }

        public EvaluatorDto getOneEvaluation(string userId, string pubId)
        {
            if(_userServices.IsUserRegistered(userId) && _publicationServices.IsPublicationExists(pubId)){
                var result = _evaluationRepository.getOneEvaluation(userId, pubId).Result;
                if(result != null){
                    return result;
                }else{throw new Exception("404: Avaliação não encontrada"){HResult = 404};}
            }else{throw new Exception("404: Usuario ou publicação não encontrados"){HResult = 404};}
        }
    }
}