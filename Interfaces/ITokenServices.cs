using System.Security.Claims;
using API.DTOs;
using API.Models;

namespace API.Interfaces
{
    public interface ITokenServices
    {
        public string generateToken(UserDto user, Login login);
        public string generateRefreshToken();
        public ClaimsPrincipal getPrincipalFromExpiredToken(string token);
        public void saveRefreshToken(string email, string refreshToken);
        public string getRefreshToken(string email);
        public void deleteRefreshToken(string email, string refreshToken);
    }
}