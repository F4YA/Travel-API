using System;
using System.Collections.Generic;

namespace API.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime RegisteredAt {get; set;}

        #nullable enable
        public string? Email { get; set; }
        public string? Phone { get; set; }
        #nullable disable
        public IEnumerable<string> Interests{ get; set; }
        public Location Location { get; set; }

        public string Password { get; set; }

    }
}