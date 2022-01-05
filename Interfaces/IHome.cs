using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Models;

namespace API.Interfaces
{
    public interface IHome
    {
       public Task<SearchResultDto> search(string query);
       public Task<IEnumerable<PublicationDto>> showsPublicationsAccordingUserInterests(string userId);
       public Task<IEnumerable<PublicationDto>> showsMostRecentPublicationsAccordingUserInterests(string userId);
       public Task<IEnumerable<PublicationDto>> showsMostPopularPublicationsAccordingUserFollowing(string userId);
       public Task<IEnumerable<PublicationDto>> showsMostRecentPublicationsAccordingUserFollowing(string userId);
       public Task<IEnumerable<PublicationDto>> showsMostPopularPublications();
       public Task<EvaluatorDto> showsMostRecentEvaluations(string userId);
       public Task<Comment> showsMostRecentComments(string userId);
       public Task<UserDto> showsMostRecentFollowers(string userId);
    }
}