using OpenAI;
using OpenAI.Files;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OxBotSvc
{
    public class FilesSvc
    {
        #region PROPERTIES

        public OpenAIClient Cli;

        public List<FileResponse> CachedFiles { get; private set; } = new();
        public bool IsCachedFilesEmpty => CachedFiles == null || CachedFiles.Count == 0;

        public List<FileResponse> DeletedFiles { get; private set; } = new List<FileResponse>();

        #endregion PROPERTIES

        #region CTOR AND INIT

        public FilesSvc(OpenAIClient client) { Cli = client; }

        #endregion CTOR AND INIT

        #region API FNS

        public async Task<List<FileResponse>> ListAsync(bool refresh = false)
        {
            if (refresh || IsCachedFilesEmpty)
            {
                CachedFiles = 
                    new List<FileResponse>(await Cli.FilesEndpoint.ListFilesAsync());
            }
            return CachedFiles;
        }

        // purpose:
        // - fine-tune = jsonl with training data - each line has "prompt" and "completion"
        // - fine-tune-results - Not implement yet...or i don't know how to use it
        // - assistants - For Assistants and Messages
        // - assistants_output - Not implemented yet...or i don't know how to use it
        public async Task<List<FileResponse>> UploadAsync(
            string[] filePaths,
            string purpose = "assistants")
        {
            // No upload files, no change, return current list
            if (filePaths == null || filePaths.Length == 0) { return CachedFiles; }

            foreach (var filePath in filePaths)
            {
                var fileResponse =
                    await Cli.FilesEndpoint.UploadFileAsync(filePath, purpose);
                Console.WriteLine($"{fileResponse.FileName} - {fileResponse.Id}");
            }

            return await ListAsync(true); // Refreshing the list after uploads
        }

        // purpose:
        // - fine-tune = jsonl with training data - each line has "prompt" and "completion"
        // - fine-tune-results - Not implement yet...or i don't know how to use it
        // - assistants - For Assistants and Messages
        // - assistants_output - Not implemented yet...or i don't know how to use it
        public async Task<FileResponse?> UploadAsync(
            string filePath,
            string purpose = "assistants")
        {
            if (filePath == null || filePath.Length == 0) { return null; }

            var fileResponse =
                await Cli.FilesEndpoint.UploadFileAsync(filePath, purpose);
            Console.WriteLine($"{fileResponse.FileName} - {fileResponse.Id}");
            await ListAsync(true); // Refreshing the list after uploads
            return fileResponse;
        }

        //public async Task<File> RetrieveByIdAsync(string fileId) { 
        //    throw new NotImplementedException();
        //}
        //public async Task<File> RetrieveByNameAsync(string fileId)
        //{
        //    throw new NotImplementedException();
        //}
        //public async Task DownloadAsync(string fileId, string savePath) { /* Implementation */ }

        public async Task<bool> DeleteByIdAsync(string id)
        {
            var isDeleted = await Cli.FilesEndpoint.DeleteFileAsync(id);
            if (isDeleted)
            {
                // Move to deleted files list
                var file = CachedFiles.FirstOrDefault(f => f.Id == id);
                if (file != null)
                {
                    DeletedFiles.Add(file);
                    CachedFiles.Remove(file);
                }
            }
            return isDeleted;
        }

        public async Task<bool> DeleteByNameAsync(string name,bool first = false)
        {
            bool anyDeleted = false;
            var matchedFiles = CachedFiles.Where(f => f.FileName == name).ToList();

            foreach (var file in matchedFiles)
            {
                anyDeleted |= await DeleteByIdAsync(file.Id);
                if (first) { break; }
            }

            return anyDeleted;
        }

        #endregion API FNS
    }
}
