using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Interfaces;
using API.Models;
using Neo4jClient;

namespace API.Repositories
{
    public class UserRepository : IUser
    {
       private readonly IGraphClient _neo4jConnection;
        private readonly ILocation _locationRepository;
        private readonly IPublication _publicationRepository;
        public UserRepository(IGraphClient neo4jConnection, ILocation locationRepository, IPublication publicationRepository)
        {
            _neo4jConnection = neo4jConnection;
            _locationRepository = locationRepository;
            _publicationRepository = publicationRepository;
        }

        //CREATE METHODS
        public async Task<UserDto> create(User user)
        {
            user.RegisteredAt = DateTime.Now;
            var result = await _neo4jConnection.Cypher
                .Create("(user:User {Id:'"+user.Id+"',"+
                                    $"Name:'{user.Name}',"+
                                    $"Email:'{user.Email}',"+
                                    $"RegisteredAt:'{user.RegisteredAt}',"+
                                    //$"Phone:'{user.Phone}',"+
                                    $"Password:'{user.Password}'"+
                                    "})")
                .Return((user) => new UserDto{ 
                    Id = user.As<UserDto>().Id,
                    Name = user.As<UserDto>().Name,
                    RegisteredAt = user.As<UserDto>().RegisteredAt,
            }).ResultsAsync;
            result.First().FollowingCount = 0;
            result.First().FollowersCount = 0;
            result.First().PublicationCount = 0;
            result.First().Location = createLocation(user.Id, user.Location.Locality).Result;
            for (int i = 0; i < user.Interests.Count(); i++)
            {
                createRelationshipsWithCities(result.First().Id, user.Interests.ElementAt(i));
            }
            return result.First();

            
        }

        public async void followUser(FollowDto obj)
        {
            var date = DateTime.Now;
            await _neo4jConnection.Cypher
            .Match("(follower:User {Id:'"+obj.FollowerId+"'}),(followed:User {Id:'"+obj.FollowedId+"'})")
            .Create("(follower)-[f:Follows {FollowedAt:'"+date+"'}]->(followed)")
            .ExecuteWithoutResultsAsync();
        }

        public async Task<Location> createLocation(string userId, string locality){
            await _neo4jConnection.Cypher
            .Match("(u:User {Id:'"+userId+"'}),(l:Locality {Name:'"+locality+"'})")
            .Create("(u)-[f:IsFrom]->(l)").ExecuteWithoutResultsAsync();
            var location = await _locationRepository.getLocation(userId);
            return location;
        }
        public async void createRelationshipsWithCities(string userId, string cityName){
            await _neo4jConnection.Cypher.Match("(u:User {Id:'"+userId+"'}),(l:Locality {Name:'"+cityName+"'})")
            .Create("(u)-[r:Interested]->(l)").ExecuteWithoutResultsAsync();
        }

        //DELETE METHODS
        public async void deleteUser(DeleteUserDto obj)
        {
            await _neo4jConnection.Cypher
                .Match("(user:User {Id:'"+obj.Id+"'})").DetachDelete("user").ExecuteWithoutResultsAsync();
        }

        public async void unfollowUser(FollowDto obj)
        {
            await _neo4jConnection.Cypher
            .Match("(follower:User {Id:'"+obj.FollowerId+"'})-[f:Follows]->(followed:User {Id:'"+obj.FollowedId+"'})")
            .Delete("f")
            .ExecuteWithoutResultsAsync();
        }

        //GET METHODS
        public async Task<string> getPassword(string id)
        {
            var result = await _neo4jConnection.Cypher
            .Match("(user:User)").Where($"user.Id='{id}'")
            .Return((user) => new{user.As<User>().Password}).ResultsAsync;  
            return result.First().Password;
        }

        public async Task<string> getPasswordByEmail(string email)
        {
            var result = await _neo4jConnection.Cypher
            .Match("(user:User)").Where($"user.Email='{email}'")
            .Return((user) => new{user.As<User>().Password}).ResultsAsync;  
            return result.First().Password;
        }

        public async Task<UserDto> getUserByEmail(string email)
        {
            var result = await _neo4jConnection.Cypher
            .Match("(user:User {Email:'"+email+"'})")
            .Return<UserDto>("user").ResultsAsync;

            result.First().FollowersCount = getFollowers(result.First().Id).Result.Count();
            result.First().FollowingCount = getFolloweds(result.First().Id).Result.Count();
            result.First().Location = _locationRepository.getLocation(result.First().Id).Result;
            result.First().PublicationCount = _publicationRepository.getAll(result.First().Id).Result.Count();

            return result.First();

            // (user) => new UserDto{
            // Id = user.As<User>().Id, 
            // Name = user.As<User>().Name,
            // RegisteredAt = user.As<UserDto>().RegisteredAt,
            // ProfileImageUrl = user.As<UserDto>().ProfileImageUrl,
            // ProfileBannerUrl = user.As<UserDto>().ProfileBannerUrl
            // }
        }

        public async Task<UserDto> getUserById(string id)
        {
            var result = await _neo4jConnection.Cypher
            .Match("(user:User {Id:'"+id+"'})")
            .Return<UserDto>("user").ResultsAsync;

            result.First().FollowersCount = getFollowers(id).Result.Count();
            result.First().FollowingCount = getFolloweds(id).Result.Count();
            result.First().Location = _locationRepository.getLocation(id).Result;
            result.First().PublicationCount = _publicationRepository.getAll(result.First().Id).Result.Count();

            return result.First();

            // (user) => new UserDto{
            // Id = user.As<UserDto>().Id, 
            // Name = user.As<UserDto>().Name,
            // RegisteredAt = user.As<UserDto>().RegisteredAt,
            // ProfileImageUrl = user.As<UserDto>().ProfileImageUrl,
            // ProfileBannerUrl = user.As<UserDto>().ProfileBannerUrl
            // }
        }

        public async Task<bool> IsUserRegistered(string id){
            var result = await _neo4jConnection.Cypher.Match("(user:User)")
            .Where($"user.Id='{id}'")
            .Return((user) => new User{Id = user.As<User>().Id,Name = user.As<User>().Name}).ResultsAsync;
            return result.Any();
        }
        public async Task<bool> IsEmailRegistered(string email){
            var result = await _neo4jConnection.Cypher.Match("(user:User)")
            .Where($"user.Email='{email}'")
            .Return((user) => new User{Id = user.As<User>().Id,Name = user.As<User>().Name}).ResultsAsync;
            return result.Any();
        }

        public async Task<bool> IsPhoneRegistered(string phone){
            var result = await _neo4jConnection.Cypher.Match("(user:User)")
            .Where($"user.Phone='{phone}'")
            .Return((user) => new User{Id = user.As<User>().Id,Name = user.As<User>().Name}).ResultsAsync;
            return result.Any();
        }

        public async Task<IEnumerable<UserDto>> getFollowers(string id){
            var result = await  _neo4jConnection.Cypher
            .Match("(user:User {Id:'"+id+"'})<-[f:Follows]-(b:User)")
            .ReturnDistinct((b) => new UserDto{
                Id = b.As<UserDto>().Id,
                Name = b.As<UserDto>().Name,
                ProfileImageUrl = b.As<UserDto>().ProfileImageUrl
                })
            .ResultsAsync;
            
            for (int i = 0; i < result.Count(); i++)
            {
                result.ElementAt(i).Location = _locationRepository.getLocation(result.ElementAt(i).Id).Result;
            }
            return result;
        }

        public async Task<IEnumerable<UserDto>> getFolloweds(string id){
            var result = await  _neo4jConnection.Cypher
            .Match("(user:User {Id:'"+id+"'})-[f:Follows]->(b:User)")
            .ReturnDistinct((b) => new UserDto{
                Id = b.As<UserDto>().Id,
                Name = b.As<UserDto>().Name,
                ProfileImageUrl = b.As<UserDto>().ProfileImageUrl
                })
            .ResultsAsync;

            for (int i = 0; i < result.Count(); i++)
            {
                result.ElementAt(i).Location = _locationRepository.getLocation(result.ElementAt(i).Id).Result;
            }
            
            return result;
        }

        //UPDATE METHODS
        public async Task<UserDto> updateUserEmail(UserEmailDto obj)
        {
            var result = await _neo4jConnection.Cypher.Match("(user:User {Id:'"+obj.Id+"'})")
                    .Set($"user.Email ='{obj.NewEmail}'")
                    .Return((user) => new UserDto{ 
                        Id = user.As<UserDto>().Id,
                        Name = user.As<UserDto>().Name}).ResultsAsync;
            return result.First();
        }

        public async Task<UserDto> updateUserId(UserIdDto obj)
        {
            var result = await _neo4jConnection.Cypher.Match("(user:User {Id:'"+obj.CurrentId+"'})")
                .Set($"user.Id ='{obj.NewId}'").Return((user) => new UserDto{
                Id = user.As<UserDto>().Id,
                Name = user.As<UserDto>().Name,
                }).ResultsAsync;
            return result.First();
        }

        public async Task<UserDto> updateUserName(UserNameDto obj)
        {
            var result = await _neo4jConnection.Cypher.Match("(user:User {Id:'"+obj.Id+"'})")
                .Set($"user.Name ='{obj.NewName}'").Return((user) => new UserDto{
                Id = user.As<UserDto>().Id,
                Name = user.As<UserDto>().Name,
                }).ResultsAsync;
            return result.First();
        }

        public async Task<UserDto> updateUserPassword(UserPasswordDto obj)
        {
            var result = await _neo4jConnection.Cypher.Match("(user:User {Id:'"+obj.Id+"'})")
                    .Set($"user.Password ='{obj.NewPassword}'")
                    .Return((user) => new UserDto{ 
                        Id = user.As<UserDto>().Id,
                        Name = user.As<UserDto>().Name}).ResultsAsync;
            return result.First();
        }
        public async void resetUserPassword(string email, string password)
        {
            await _neo4jConnection.Cypher.Match("(user:User {Email:'"+email+"'})")
                    .Set($"user.Password ='{password}'")
                    .ExecuteWithoutResultsAsync();
        }

        public async Task<UserDto> updateUserPhone(UserPhoneDto obj)
        {
            var result = await _neo4jConnection.Cypher.Match("(user:User {Id:'"+obj.Id+"'})")
                    .Set($"user.Phone ='{obj.NewPhone}'")
                    .Return((user) => new UserDto{ 
                        Id = user.As<UserDto>().Id,
                        Name = user.As<UserDto>().Name}).ResultsAsync;
            return result.First();
        }

        public async Task<Location> updateUserLocality(string userId, string newLocality){
            await _neo4jConnection.Cypher
            .Match("(u:User {id:'"+userId+"'})-[f:From]->(l:Locality)")
            .Delete("f").ExecuteWithoutResultsAsync();
            return createLocation(userId, newLocality).Result;
        }

        public async void updateUserProfileImageUrl(string userId, string newImageUrl){
            await _neo4jConnection.Cypher.Match("(user:User {Id:'"+userId+"'})")
            .Set($"user.ProfileImageUrl ='{newImageUrl}'").ExecuteWithoutResultsAsync(); 
        }

        public async void updateUserBannerUrl(string userId, string newBannerUrl){
            await _neo4jConnection.Cypher.Match("(user:User {Id:'"+userId+"'})")
            .Set($"user.ProfileBannerUrl ='{newBannerUrl}'").ExecuteWithoutResultsAsync(); 
        } 
    }
}