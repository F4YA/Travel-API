using System.Collections.Generic;

namespace API.DTOs
{
    public class SearchResultDto
    {
        public IEnumerable<UserDto> Users { get; set; }
        public IEnumerable<PublicationDto> Publications { get; set; }
    }
}