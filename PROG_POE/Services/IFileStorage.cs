namespace PROG_POE.Services;

public interface IFileStorage
{
    Task<string> SaveAsync(IFormFile file, string subFolder);
    Task<bool> ExistsAsync(string path);
}

