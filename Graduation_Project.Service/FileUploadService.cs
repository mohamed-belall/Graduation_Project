using Graduation_Project.Core.IServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Graduation_Project.Service
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public FileUploadService(IWebHostEnvironment webHostEnvironment , IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            this._configuration = configuration;
        }



        public async Task<(bool Success ,string Message , string? FilePath)> UploadFileAsync(IFormFile file, string folderName , ClaimsPrincipal? user, string? customFileName = null)
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
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", folderName);

                // Ensure the directory exists
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

               

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);


                // check if file exist delete it
                if(File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // saving the new file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }


                string relativePath = $"/uploads/{folderName}/{uniqueFileName}";
                return (true , "File uploaded successfully.", relativePath); // Return relative path
            }
            catch (Exception ex)
            {

                return (false, "An error occurred while uploading the file.", null);
            }

        }


        public async Task<(bool Success, string Message)> DeleteFile(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return (false, "Invalid file path.");

            try
            {
                // Build the absolute path
                string filePath = CombinePath(relativePath);

                if (!File.Exists(filePath))
                    return (false, "File does not exist.");

                File.Delete(filePath);
                return (true, "File deleted successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred while deleting the file. {ex.Message}");
            }
        }

        public string CombinePath(string relativePath)
        {
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

            return filePath;
        }

        public string getRelativePath(string absolutePath)
        {
            string baseUrl = _configuration["AzureStorageUrl"]!;
            absolutePath = absolutePath.Replace(baseUrl, "");

            return absolutePath;
        }
    }
}
