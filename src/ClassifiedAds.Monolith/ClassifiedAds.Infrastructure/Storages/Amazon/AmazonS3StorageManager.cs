﻿using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using ClassifiedAds.Domain.Entities;
using ClassifiedAds.Domain.Infrastructure.Storages;
using System.IO;

namespace ClassifiedAds.Infrastructure.Storages.Amazon
{
    public class AmazonS3StorageManager : IFileStorageManager
    {
        private readonly IAmazonS3 _client;
        private readonly string _bucketName;

        public AmazonS3StorageManager(string awsAccessKeyId, string awsSecretAccessKey, string bucketName, string regionEndpoint)
        {
            _client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, RegionEndpoint.GetBySystemName(regionEndpoint));
            _bucketName = bucketName;
        }

        public void Create(FileEntry fileEntry, MemoryStream stream)
        {
            var fileTransferUtility = new TransferUtility(_client);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = fileEntry.Id.ToString(),
                BucketName = _bucketName,
                CannedACL = S3CannedACL.NoACL,
            };

            fileTransferUtility.UploadAsync(uploadRequest).Wait();

            fileEntry.FileLocation = fileEntry.Id.ToString();
        }

        public void Delete(FileEntry fileEntry)
        {
            _client.DeleteObjectAsync(_bucketName, fileEntry.FileLocation).Wait();
        }

        public byte[] Read(FileEntry fileEntry)
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = fileEntry.FileLocation,
            };

            using (var response = _client.GetObjectAsync(request).GetAwaiter().GetResult())
            using (var responseStream = response.ResponseStream)
            using (var reader = new MemoryStream())
            {
                responseStream.CopyTo(reader);
                return reader.ToArray();
            }
        }
    }
}
