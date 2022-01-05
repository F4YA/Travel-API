using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API.DTOs;
using API.Interfaces;
using API.Models;
using Microsoft.IdentityModel.Tokens;

namespace API.Services 
{
    public class TokenServices : ITokenServices
    {
        private readonly byte[] _key;
        private readonly int _time;
        public TokenServices(byte[] key, int time)
        {
            _key = key;
            _time = time;
        }
        public string generateToken(UserDto user, Login login){
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = _key;
            var tokenDescriptor = new SecurityTokenDescriptor{
                Subject = new ClaimsIdentity(new Claim[]{
                    new Claim("id", user.Id.ToString()),
                    new Claim("name", user.Name.ToString()),
                    new Claim(ClaimTypes.Email, login.Email.ToString()),
                    new Claim("country", user.Location.Country.ToString()),
                    new Claim("country_code", user.Location.CountryCode.ToString()),
                    new Claim("locality", user.Location.Locality.ToString()),
                    new Claim(ClaimTypes.Expiration, DateTime.UtcNow.AddHours(_time).ToString()),
                }),
                Expires = DateTime.UtcNow.AddHours(_time),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_key),
                SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string generateToken(IEnumerable<Claim> claims){
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = _key;
            var tokenDescriptor = new SecurityTokenDescriptor{
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(_time),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_key),
                SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string generateRefreshToken(){
            var refreshToken = Guid.NewGuid().ToString();
            return refreshToken;
        }

        public ClaimsPrincipal getPrincipalFromExpiredToken(string token){
            var tokenValidationParameters= new TokenValidationParameters{
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(_key),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if(securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Token Invalido");

            return principal;
        }

        private List<(string, string)> _refreshTokens = new();

        public void saveRefreshToken(string email, string refreshToken){
            _refreshTokens.Add(new(email, refreshToken));
        }

        public string getRefreshToken(string email){
            return _refreshTokens.FirstOrDefault(x => x.Item1 == email).Item2;
        }

        public void deleteRefreshToken(string email, string refreshToken){
            var item = _refreshTokens.FirstOrDefault(x => x.Item1 == email && x.Item2 == refreshToken);
            _refreshTokens.Remove(item);
        }
    }
}