using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace APFood.Services
{
    public class S3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3Service(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _bucketName = configuration["AWS:BucketName"];
        }

        public async Task<string> UploadFileAsync(Stream inputStream, string fileName)
        {
            var fileTransferUtility = new TransferUtility(_s3Client);

            var fileUploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = inputStream,
                Key = fileName,
                BucketName = _bucketName
            };

            await fileTransferUtility.UploadAsync(fileUploadRequest);

            return $"https://{_bucketName}.s3.amazonaws.com/{fileName}";
        }

        public string GeneratePreSignedURL(string fileName, TimeSpan expiration)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                Expires = DateTime.UtcNow.Add(expiration)
            };

            string url = _s3Client.GetPreSignedURL(request);
            return url;
        }
    }
}
