using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Models;

namespace API.Interfaces
{
    public interface IUserServices 
    {
        public UserDto getUserById(string id);
        public UserDto getUserByEmail(string email);
        public IEnumerable<UserDto> getFollowers(string id);
        public IEnumerable<UserDto> getFolloweds(string id);
        public UserDto create(User user);
        public UserDto updateUserId(UserIdDto obj);
        public UserDto updateUserName(UserNameDto obj);
        public UserDto updateUserEmail(UserEmailDto obj);
        public UserDto updateUserPhone(UserPhoneDto obj);
        public UserDto updateUserPassword(UserPasswordDto obj);
        public void resetPassword(string email, string password);
        public void updateUserProfileImageUrl(string userId, string newImageUrl);
        public void updateUserBannerUrl(string userId, string newBannerUrl);
        public void deleteUser(DeleteUserDto obj);
        public void followUser(FollowDto obj);
        public void unfollowUser(FollowDto obj);
        public bool IsUserRegistered(string id);
        public bool IsEmailRegistered(string email);
        public bool IsPhoneRegistered(string phone);
        public UserDto login(Login obj); 
        public UserDto loginWithFacebook(Login obj);
        public UserDto loginWithGoogle(Login obj);
    }
}