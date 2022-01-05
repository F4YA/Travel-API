using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Interfaces
{
    public interface IUser
    {
        public Task<UserDto> getUserById(string id);
        public Task<UserDto> getUserByEmail(string email);
        public Task<string> getPassword(string id);
        public Task<string> getPasswordByEmail(string email);
        public Task<IEnumerable<UserDto>> getFollowers(string id);
        public Task<IEnumerable<UserDto>> getFolloweds(string id);
        //public Task<ActionResult> createUser([FromBody] User user);
        public Task<UserDto> create(User user);
        public Task<UserDto> updateUserId(UserIdDto obj);
        public Task<UserDto> updateUserName(UserNameDto obj);
        public Task<UserDto> updateUserEmail(UserEmailDto obj);
        public Task<UserDto> updateUserPhone(UserPhoneDto obj);
        public Task<UserDto> updateUserPassword(UserPasswordDto obj);
        public void resetUserPassword(string email, string password);
        public void updateUserProfileImageUrl(string userId, string newImageUrl);
        public void updateUserBannerUrl(string userId, string newBannerUrl);
        public void deleteUser(DeleteUserDto obj);
        public void followUser(FollowDto obj);
        public void unfollowUser(FollowDto obj);
        public Task<bool> IsUserRegistered(string id);
        public Task<bool> IsEmailRegistered(string email);
        public Task<bool> IsPhoneRegistered(string phone);
    }
}