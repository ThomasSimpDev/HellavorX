using HellavorX.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace HellavorX.Services;

public class FileUploadService
{
    private readonly Cloudinary _cloudinary;

    public FileUploadService(IConfiguration configuration)
    {
        var cloudName = configuration["CLOUDINARY_CLOUD_NAME"];
        var apiKey = configuration["CLOUDINARY_API_KEY"];
        var apiSecret = configuration["CLOUDINARY_API_SECRET"];

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
    {
        // Reset stream position if needed
        if (fileStream.CanSeek)
        {
            fileStream.Position = 0;
        }

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            UniqueFilename = true,
            Overwrite = false,
            UseFilename = false,
            Folder = "hellavorx"
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
        {
            throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");
        }

        // Return the secure URL from Cloudinary
        return uploadResult.SecureUrl.ToString();
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
