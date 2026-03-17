using HellavorX.Models;

namespace HellavorX.Services;

public class FileUploadService
{
    private readonly IWebHostEnvironment _env;

    public FileUploadService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
    {
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsFolder);
        
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
        
        using var fileStreamOutput = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(fileStreamOutput);
        
        return $"/uploads/{uniqueFileName}";
    }

    public MediaType GetMediaType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        // categorize common extensions, defaulting to File for anything else
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp" or ".tiff" => MediaType.Image,
            ".mp3" or ".wav" or ".ogg" or ".m4a" or ".flac" => MediaType.Audio,
            ".mp4" or ".webm" or ".mov" or ".avi" or ".mkv" or ".wmv" => MediaType.Video,
            _ => MediaType.File
        };
    }
}
