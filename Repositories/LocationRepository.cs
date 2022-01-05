using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Interfaces;
using API.Models;
using Neo4jClient;

namespace API.Repositories
{
    public class LocationRepository : ILocation
    {
        private readonly IGraphClient _neo4jConnection;
        public LocationRepository(IGraphClient neo4jConnection)
        {
            _neo4jConnection = neo4jConnection;
        }

        public async Task<CountryDto> getCountry(string userId){
            var result = await _neo4jConnection.Cypher
            .Match("(c:Country)<-[o:IsIn]-(l:Locality)<-[f:IsFrom]-(u:User {Id:'"+userId+"'})")
            .Return<CountryDto>("c").ResultsAsync;

            return result.First();

            // (c) => new CountryDto{
            //     Name = c.As<CountryDto>().Name,
            //     Code = c.As<CountryDto>().Code
            // }
        }
        public async Task<LocalityDto> getLocality(string userId){
            var result = await _neo4jConnection.Cypher
            .Match("(l:Locality)<-[f:IsFrom]-(u:User {Id:'"+userId+"'})")
            .Return<LocalityDto>("l").ResultsAsync;

            return result.First();

            // (l) => new LocalityDto{
            //     Name = l.As<LocalityDto>().Name,
            // }
        }

        public async Task<Location> getLocation(string userId){
            var country = await getCountry(userId);
            var locality = await getLocality(userId);
            return new Location{
                Country = country.Name,
                CountryCode = country.Code,
                Locality = locality.Name
            };
        }
    }
}