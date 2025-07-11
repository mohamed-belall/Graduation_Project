using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Graduation_Project.Core.IServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Graduation_Project.Service
{
    public class AzureFileUploadService : IFileUploadService
    {
        private readonly IConfiguration _configuration;

        public AzureFileUploadService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string CombinePath(string relativePath)
        {
            throw new NotImplementedException();
        }

        public async Task<(bool Success, string Message)> DeleteFile(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return (false, "Invalid file path.");

            try
            {
                // Remove leading slashes and container path if needed
                // E.g. "/uploads/ClinicPicture/photo.jpg" -> "ClinicPicture/photo.jpg"
                string containerName = _configuration["AzureBlobStorage:ContainerName"];
                string blobName = relativePath
                    .Replace("/uploads/", "") // Adjust this if your blob naming is different
                    .TrimStart('/')
                    .Replace("\\", "/");

                var blobServiceClient = new BlobServiceClient(_configuration["AzureBlobStorage:ConnectionString"]);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                var blobClient = containerClient.GetBlobClient(blobName);

                bool deleted = await blobClient.DeleteIfExistsAsync();

                if (deleted)
                    return (true, "File deleted successfully.");
                else
                    return (false, "File does not exist in Azure.");
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred while deleting the file. {ex.Message}");
            }
        }

        public string getRelativePath(string absolutePath)
        {
            // Example: https://youraccount.blob.core.windows.net/images/ClinicPicture/user.jpg
            var uri = new Uri(absolutePath);
            return uri.AbsolutePath.TrimStart('/'); // returns "images/ClinicPicture/user.jpg"
        }

        public async Task<(bool Success, string Message, string? FilePath)> UploadFileAsync(IFormFile file, string folderName, ClaimsPrincipal? user, string? customFileName = null)
        {


            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return (false, "Invalid file format. Allowed formats: .jpg, .jpeg, .png, .gif, .bmp, .webp", null);

            // Validate file size (Max 5MB)
            const long maxFileSize = 5 * 1024 * 1024;
            if (file.Length > maxFileSize)
                return (false, "File size must be less than 5MB.", null);

            // Validate folder name
            if (string.IsNullOrWhiteSpace(folderName))
                return (false, "Folder name is required.", null);



            string uniqueFileName;

            if (folderName.Contains("ClinicPicture") || folderName.Contains("MedicalHistory"))
            {
                uniqueFileName = string.IsNullOrWhiteSpace(customFileName)
               ? $"{user.FindFirstValue(ClaimTypes.GivenName)!}-{Guid.NewGuid()}{extension}"
               : $"{customFileName}{extension}";
            }
            else
            {
                uniqueFileName = string.IsNullOrWhiteSpace(customFileName)
                ? $"{user.FindFirstValue(ClaimTypes.GivenName)!}-{user.FindFirstValue(ClaimTypes.NameIdentifier)!}{extension}"
                : $"{customFileName}{extension}";
            }

            //uniqueFileName = string.IsNullOrWhiteSpace(customFileName)
            //  ? $"{user.FindFirstValue(ClaimTypes.GivenName)!}-{Guid.NewGuid()}{extension}"
            //  : $"{customFileName}{extension}";

            try
            {
                // Simulate folder structure in blob name
                string blobName = $"{folderName}/{uniqueFileName}";

                var blobServiceClient = new BlobServiceClient(_configuration["AzureBlobStorage:ConnectionString"]);
                var containerClient = blobServiceClient.GetBlobContainerClient(_configuration["AzureBlobStorage:ContainerName"]);
                await containerClient.CreateIfNotExistsAsync();

                var blobClient = containerClient.GetBlobClient(blobName);

                // Delete existing blob if it exists
                await blobClient.DeleteIfExistsAsync();

                using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream,new BlobHttpHeaders
                {
                    ContentType = "image/jpeg"
                });

                string fileUrl = $"{containerClient.Name}/{blobName}";
                return (true, "File uploaded successfully.", fileUrl);
            }
            catch (Exception ex)
            {

                return (false, "An error occurred while uploading the file.", null);
            }

        }

    }
}
