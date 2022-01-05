
using API.Interfaces;

namespace API.Security
{
    public class SafetyValidations : ISecurity
    {
        public string hashPassword(string password)
        {
            int workFactor = 16; //2^(16) = 65536 iterations

            string salt = BCrypt.Net.BCrypt.GenerateSalt(workFactor);
            string hash = BCrypt.Net.BCrypt.HashPassword(password, salt);

            return hash;
        }

        public bool comparePasswords(string hash, string password)
        {
            return BCrypt.Net.BCrypt.Verify(text: password, hash: hash);
        }


    }
}