using System.Threading.Tasks;

namespace GitSharp
{
    public interface ISourceControlApi
    {
        Task<string> CatFileAsync(string hash);
    }
}