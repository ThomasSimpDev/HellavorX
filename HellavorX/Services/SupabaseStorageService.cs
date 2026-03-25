using HellavorX.Models;
using Supabase;

namespace HellavorX.Services;

public class SupabaseStorageService
{
    private readonly Client _supabase;
    private readonly string _bucketName = "hellavorx";

    public SupabaseStorageService(IConfiguration configuration)
    {
        var supabaseUrl = configuration["SUPABASE_URL"] ?? throw new ArgumentNullException("SUPABASE_URL configuration is missing");
        var supabaseKey = configuration["SUPABASE_ANON_KEY"] ?? throw new ArgumentNullException("SUPABASE_ANON_KEY configuration is missing");

        var options = new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = false
        };

        _supabase = new Client(supabaseUrl, supabaseKey, options);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
    {
        // Reset stream position if needed
        if (fileStream.CanSeek)
        {
            fileStream.Position = 0;
        }

        // Generate unique filename
        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";

        // Convert stream to byte array
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();

        var bucket = _supabase.Storage.From(_bucketName);

        // Ensure bucket exists, create if not
        try
        {
            await _supabase.Storage.CreateBucket(_bucketName);
        }
        catch (Exception)
        {
            // Bucket might already exist, continue with upload
        }

        // Upload the file
        var uploadResult = await bucket.Upload(fileBytes, uniqueFileName);

        if (uploadResult != null)
        {
            // Get the public URL
            var publicUrl = _supabase.Storage.From(_bucketName).GetPublicUrl(uniqueFileName);
            return publicUrl;
        }

        throw new Exception("Supabase upload failed: No response from storage");
    }

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            // Extract filename from URL
            var uri = new Uri(fileUrl);
            var fileName = Path.GetFileName(uri.LocalPath);

            var bucket = _supabase.Storage.From(_bucketName);
            await bucket.Remove(new List<string> { fileName });
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public MediaType GetMediaType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp" or ".tiff" => MediaType.Image,
            ".mp3" or ".wav" or ".ogg" or ".m4a" or ".flac" => MediaType.Audio,
            ".mp4" or ".webm" or ".mov" or ".avi" or ".mkv" or ".wmv" => MediaType.Video,
            _ => MediaType.File
        };
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".tiff" => "image/tiff",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".ogg" => "audio/ogg",
            ".m4a" => "audio/mp4",
            ".flac" => "audio/flac",
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            ".mov" => "video/quicktime",
            ".avi" => "video/x-msvideo",
            ".mkv" => "video/x-matroska",
            ".wmv" => "video/x-ms-wmv",
            _ => "application/octet-stream"
        };
    }
}
