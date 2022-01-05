using System.Collections.Generic;
using API.DTOs;
using API.DTOs.Responses;
using API.Models;

namespace API.Interfaces
{
    public interface IHomeServices
    {
       public SearchResultDto search(string query);
       public IEnumerable<PublicationDto> showsPublicationsAccordingUserInterests(string userId);
       public IEnumerable<PublicationDto> showsMostRecentPublicationsAccordingUserInterests(string userId);
       public IEnumerable<PublicationDto> showsMostPopularPublicationsAccordingUserFollowing(string userId);
       public IEnumerable<PublicationDto> showsMostRecentPublicationsAccordingUserFollowing(string userId);
       public IEnumerable<PublicationDto> showsMostPopularPublications();
       public EvaluatorDto showsMostRecentEvaluations(string userId);
       public CommentResponse showsMostRecentComments(string userId);
       public UserDto showsMostRecentFollowers(string userId);
    }
}