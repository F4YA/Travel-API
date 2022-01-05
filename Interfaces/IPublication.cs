using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Interfaces
{
    public interface IPublication
    {
         public Task<IEnumerable<PublicationDto>> getAll(string userId);
         public Task<PublicationDto> getOne(string id);
         public Task<ThreadDto> getThread(string id);
         public Task<Publication> create(string userId, Publication publication);
         public void createThread(string rootId, string branchId);
         public void delete(string id);
         public void deleteThread(string id);
         public Task<bool> IsPublicationExists(string id);
         public Task<IEnumerable<PublicationDto>> searchPublications(string userId, string query);
    }
}