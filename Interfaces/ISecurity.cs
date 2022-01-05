namespace API.Interfaces
{
    public interface ISecurity
    {
         public string hashPassword(string password);
         public bool comparePasswords(string hash, string password);
    }
}