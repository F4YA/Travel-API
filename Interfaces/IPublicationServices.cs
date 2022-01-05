using System.Collections.Generic;
using API.DTOs;
using API.Models;

namespace API.Interfaces
{
    public interface IPublicationServices
    {
        public IEnumerable<PublicationDto> getAll(string userId);
        public PublicationDto getOne(string id);
        public ThreadDto getThread(string userId, string id); 
        public Publication create(string userId, Publication publication);
        //  public Publication delete(string id);
        public void deleteThread(string id);
        public bool IsPublicationExists(string id);
        public string setId(string userId);
        public IEnumerable<PublicationDto> searchPublications(string userId, string query);
    }
}