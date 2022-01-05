using System.Threading.Tasks;
using API.Models;

namespace API.Interfaces
{
    public interface ILocation
    {
         public Task<Location> getLocation(string userId);
    }
}