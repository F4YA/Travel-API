using System;
using System.Collections.Generic;
using API.DTOs;
using API.Interfaces;
using API.Models;

namespace API.Services
{
    public class PublicationServices : IPublicationServices
    {
        private readonly IPublication _publicationRepository;
        private readonly IUserServices _userServices;

        public PublicationServices(IPublication publicationRepository, IUserServices userServices)
        {
            _publicationRepository = publicationRepository;
            _userServices = userServices;
        }
        public Publication create(string userId, Publication publication)
        {
            if(_userServices.IsUserRegistered(userId)){
                publication.Id = setId(userId);   
                var result = _publicationRepository.create(userId, publication).Result;
                if(publication.Complement != null){
                    result.Complement = create(userId, publication.Complement);
                    if(!string.IsNullOrEmpty(result.Complement.Id)){
                        _publicationRepository.createThread(result.Id, result.Complement.Id);
                        return result;
                    }else{throw new Exception();}
                }else{return result;}
            }else{throw new Exception("404: Usuário não encontrado"){HResult = 404};}
        }

        public void deleteThread(string id)
        {
            if(IsPublicationExists(id)){
                try{
                    _publicationRepository.deleteThread(id);
                    if(IsPublicationExists(id)){
                        throw new Exception(){HResult = 500};
                    }
                }catch{
                    if(IsPublicationExists(id)){
                        throw new Exception(){HResult = 500};
                    }
                }
            }else{throw new Exception("404: Publicação não encontrada"){HResult = 404};}
        }

        public IEnumerable<PublicationDto> getAll(string userId)
        {
            if(_userServices.IsUserRegistered(userId)){
                return _publicationRepository.getAll(userId).Result;
            }else{
                throw new Exception("404: Usuario não encontrado"){HResult = 404};
            }
        }

        public PublicationDto getOne(string id)
        {
            if(_publicationRepository.IsPublicationExists(id).Result){
                return _publicationRepository.getOne(id).Result;
            }else{
                throw new Exception("404: Publicação não encontrada"){HResult = 404};
            }
        }

        public ThreadDto getThread(string userId, string id)
        {
            if(_publicationRepository.IsPublicationExists(id).Result){
                 return _publicationRepository.getThread(id).Result;
            }else{throw new Exception("404: Publicação não encontrada"){HResult = 404};}
        }

        public bool IsPublicationExists(string id)
        {
            if(!string.IsNullOrEmpty(id)){
                return _publicationRepository.IsPublicationExists(id).Result;
            }else{throw new Exception("400: Identificador fornecido é nulo"){HResult = 400};}
        }

        public IEnumerable<PublicationDto> searchPublications(string userId, string query)
        {
            if(_userServices.IsUserRegistered(userId)){
                return _publicationRepository.searchPublications(userId, query).Result;
            }else{throw new Exception("404: Usuario não encontrado"){HResult = 404};}
        }

        public string setId(string userId){
            string id;
            do{
                id = userId+Guid.NewGuid().ToString("N");
            }while (_publicationRepository.IsPublicationExists(id).Result);
            return id;
        }
    }
}