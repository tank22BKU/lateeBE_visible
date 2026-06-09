using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using UserService.Application.Common.Interfaces;

namespace UserService.Infrastructure.Services;

public class S3Service : IFileStorageService
{
    private readonly IConfiguration _config;
    private readonly IAmazonS3 _s3Client;

    public S3Service(IConfiguration config)
    {
        _config = config;

        var accessKey = _config["AWS:AccessKey"];
        var secretKey = _config["AWS:SecretKey"];
        var region = _config["AWS:Region"] ?? "ap-southeast-2";

        _s3Client = new AmazonS3Client(
            accessKey,
            secretKey,
            RegionEndpoint.GetBySystemName(region)
        );
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var bucketName = _config["AWS:BucketName"];

        if (string.IsNullOrWhiteSpace(bucketName))
        {
            throw new InvalidOperationException("AWS:BucketName is not configured.");
        }

        var key = $"{Guid.NewGuid()}_{fileName}";

        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);

        var region = _config["AWS:Region"] ?? "ap-southeast-2";
        return $"https://{bucketName}.s3.{region}.amazonaws.com/{key}";
    }
}