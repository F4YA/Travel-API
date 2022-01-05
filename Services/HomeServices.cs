using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using API.DTOs;
using API.DTOs.Responses;
using API.Interfaces;
using API.Models;

namespace API.Services
{
    public class HomeServices : IHomeServices
    {
        private readonly IHome _homeRepository;
        private readonly IPublicationServices _publicationServices;
        private readonly IUserServices _userServices;
        private readonly IEvaluationServices _evaluationServices;
        public HomeServices(IHome homeRepository, IPublicationServices publicationServices, IUserServices userServices, IEvaluationServices evaluationServices)
        {
            _homeRepository = homeRepository;
            _publicationServices = publicationServices;
            _userServices = userServices;
            _evaluationServices = evaluationServices;
        }

        public SearchResultDto search(string query)
        {
            if(!string.IsNullOrEmpty(query)){
                var result = _homeRepository.search(query).Result;
                PublicationDto[] publications = new PublicationDto[result.Publications.Count()];
                UserDto[] users = new UserDto[result.Users.Count()];

                for (int i = 0; i < result.Publications.Count(); i++)
                {publications[i] = _publicationServices.getOne(result.Publications.ElementAt(i).Id);}
                for (int i = 0; i < result.Users.Count(); i++)
                {users[i] = _userServices.getUserById(result.Users.ElementAt(i).Id);}

                return new SearchResultDto{Publications = publications, Users = users};
            }else{throw new Exception("400: pesquisa nula"){HResult = 400};}
        }

        public IEnumerable<PublicationDto> showsMostPopularPublications()
        {
            var publications = _homeRepository.showsMostPopularPublications().Result;
            if(publications.Any()){
                var publicationsArray = publications.ToArray();
                for (int i = 0; i < publications.Count(); i++)
                {publicationsArray[i] = _publicationServices.getOne(publicationsArray[i].Id);}
                return publicationsArray;
            }else{
                return Enumerable.Empty<PublicationDto>();
            } 
        }

        public IEnumerable<PublicationDto> showsMostPopularPublicationsAccordingUserFollowing(string userId)
        {
            var publications = _homeRepository.showsMostPopularPublicationsAccordingUserFollowing(userId).Result;
            if(publications.Any()){
                var publicationsArray = publications.ToArray();
                for (int i = 0; i < publications.Count(); i++)
                {publicationsArray[i] = _publicationServices.getOne(publicationsArray[i].Id);}
                return publicationsArray;
            }else{
                return Enumerable.Empty<PublicationDto>();
            } 
        }

        public IEnumerable<PublicationDto> showsMostRecentPublicationsAccordingUserFollowing(string userId){
            var publications = _homeRepository.showsMostRecentPublicationsAccordingUserFollowing(userId).Result;
            
            // for (int i = 0; i < publications.Count(); i++)
            // {publications[i] = _publicationServices.getOne(publications[i].Id);}
            if(publications.Any()){
                var publicationsArray = publications.ToArray();
                for (int i = 0; i < publications.Count(); i++)
                {publicationsArray[i] = _publicationServices.getOne(publicationsArray[i].Id);}
                return publicationsArray;
            }else{
                return Enumerable.Empty<PublicationDto>();
            }
        }
        public IEnumerable<PublicationDto> showsMostRecentPublicationsAccordingUserInterests(string userId)
        {
            var publications = _homeRepository.showsMostRecentPublicationsAccordingUserInterests(userId).Result;
            if(publications.Any()){
                var publicationsArray = publications.ToArray();
                for (int i = 0; i < publications.Count(); i++)
                {publicationsArray[i] = _publicationServices.getOne(publicationsArray[i].Id);}
                return publicationsArray;
            }else{
                return Enumerable.Empty<PublicationDto>();
            }
        }

        public IEnumerable<PublicationDto> showsPublicationsAccordingUserInterests(string userId)
        {
            var publications = _homeRepository.showsPublicationsAccordingUserInterests(userId).Result;

            if(publications.Any()){
                var publicationsArray = publications.ToArray();
                for (int i = 0; i < publications.Count(); i++)
                {publicationsArray[i] = _publicationServices.getOne(publicationsArray[i].Id);}
                return publicationsArray;
            }else{
                return Enumerable.Empty<PublicationDto>();
            }
        }

        public CommentResponse showsMostRecentComments(string userId)
        {
            if(_userServices.IsUserRegistered(userId)){
                var comment = _homeRepository.showsMostRecentComments(userId).Result;
                if(comment != null){
                    return _evaluationServices.getOneComment(comment.Id, comment.PubId);
                }else{
                    return null;
                }
            }else{throw new Exception("404: Usuario não encontrado");}
        }

        public EvaluatorDto showsMostRecentEvaluations(string userId)
        {
            if(_userServices.IsUserRegistered(userId)){
                return _homeRepository.showsMostRecentEvaluations(userId).Result;
            }else{throw new Exception("404: Usuario não encontrado");}
        }

        public UserDto showsMostRecentFollowers(string userId)
        {
            if (_userServices.IsUserRegistered(userId)){
                return _homeRepository.showsMostRecentFollowers(userId).Result;
            }else{throw new Exception("404: Usuario não encontrado");}
        }

        
    }
}