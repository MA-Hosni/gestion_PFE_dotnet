using System;
using System.IO;
using System.Threading.Tasks;

namespace PfeManagement.Infrastructure.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(Stream fileStream, string fileName);
    }

    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _uploadDirectory = "Uploads";

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName)
        {
            if (!Directory.Exists(_uploadDirectory))
            {
                Directory.CreateDirectory(_uploadDirectory);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(_uploadDirectory, uniqueFileName);

            using var fileStreamToWrite = new FileStream(filePath, FileMode.Create);
            await fileStream.CopyToAsync(fileStreamToWrite);

            return filePath;
        }
    }
}
