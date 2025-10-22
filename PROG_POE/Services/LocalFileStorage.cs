namespace PROG_POE.Services;

public class LocalFileStorage : IFileStorage
{
    private readonly IWebHostEnvironment _env;
    private static readonly string[] _allowed = [".pdf", ".docx", ".xlsx"];
    private const long MaxSize = 5 * 1024 * 1024; // 5MB

    public LocalFileStorage(IWebHostEnvironment env) => _env = env;

    public async Task<string> SaveAsync(IFormFile file, string subFolder)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowed.Contains(ext)) throw new InvalidOperationException("Invalid file type.");
        if (file.Length > MaxSize) throw new InvalidOperationException("File too large.");

        var root = _env.WebRootPath ?? "wwwroot";
        var folder = Path.Combine(root, "uploads", subFolder);
        Directory.CreateDirectory(folder);

        var name = $"{Guid.NewGuid():N}{ext}";
        var path = Path.Combine(folder, name);
        using var stream = File.Create(path);
        await file.CopyToAsync(stream);

        return Path.Combine("uploads", subFolder, name).Replace("\\", "/");
    }

    public Task<bool> ExistsAsync(string path)
    {
        var full = Path.Combine(_env.WebRootPath ?? "wwwroot", path);
        return Task.FromResult(File.Exists(full));
    }
}
