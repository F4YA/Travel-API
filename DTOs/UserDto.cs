using System;
using System.Collections;
using System.Collections.Generic;
using API.Models;

namespace API.DTOs
{
    public class UserDto
    {
        #nullable enable
        public string? Id { get; set; }
        public string? Name{ get; set; }
        public string? Email {get; set;}
        public DateTime? RegisteredAt { get; set; }
        public Location? Location { get; set; }
        //public IEnumerable<UserDto>? Followers{ get; set; }
        //public IEnumerable<UserDto>? Following{ get; set; }
        public int? FollowersCount { get; set; }
        public int? FollowingCount { get; set; }
        public int? PublicationCount { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? ProfileBannerUrl { get; set; }

    }
}