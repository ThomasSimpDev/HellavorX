using HellavorX.Models;
using Supabase;

namespace HellavorX.Services;

public class SupabaseStorageService
{
    private readonly Client? _supabase;
    private readonly string _bucketName = "hellavorx";
    private readonly ILogger<SupabaseStorageService>? _logger;
    private readonly bool _isConfigured;

    public SupabaseStorageService(IConfiguration configuration, ILogger<SupabaseStorageService>? logger = null)
    {
        _logger = logger;
        
        var supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL") 
            ?? configuration["SUPABASE_URL"];
        var supabaseKey = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY")
            ?? configuration["SUPABASE_ANON_KEY"];

        if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
        {
            _isConfigured = false;
            _logger?.LogWarning("Supabase storage is not configured. File uploads will be disabled.");
            return;
        }

        try
        {
            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = false
            };

            _supabase = new Client(supabaseUrl, supabaseKey, options);
            _isConfigured = true;
        }
        catch (Exception ex)
        {
            _isConfigured = false;
            _logger?.LogWarning(ex, "Failed to initialize Supabase client. File uploads will be disabled.");
        }
    }

    /// <summary>
    /// Checks if Supabase storage is properly configured and accessible
    /// </summary>
    public bool IsAvailable => _isConfigured && _supabase != null;

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
    {
        // Reset stream position if needed
        if (fileStream.CanSeek)
        {
            fileStream.Position = 0;
        }

        // Check if storage is configured
        if (!IsAvailable)
        {
            throw new InvalidOperationException("Supabase storage is not configured. Please set SUPABASE_URL and SUPABASE_ANON_KEY environment variables.");
        }

        // Generate unique filename
        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";

        // Convert stream to byte array
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();

        var bucket = _supabase.Storage.From(_bucketName);

        // Ensure bucket exists, create if not
        var bucketExists = await BucketExistsAsync(_bucketName);
        if (!bucketExists)
        {
            try
            {
                // Try to create the bucket with public access
                await _supabase.Storage.CreateBucket(_bucketName, new Supabase.Storage.BucketUpsertOptions { Public = true });
                _logger?.LogInformation("Bucket '{BucketName}' created successfully.", _bucketName);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Could not create bucket '{BucketName}'. File upload will fail.", _bucketName);
                // Continue anyway - the upload might still work if bucket was created by admin
            }
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

    private async Task<bool> BucketExistsAsync(string bucketName)
    {
        try
        {
            var buckets = await _supabase.Storage.ListBuckets();
            return buckets?.Any(b => b.Name == bucketName) ?? false;
        }
        catch
        {
            return false;
        }
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
