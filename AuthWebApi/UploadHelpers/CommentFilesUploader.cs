using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AuthWebApi.IUploadHelpers;
using DataModelLayer.Models.Comments;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace AuthWebApi.UploadHelpers
{
    public class CommentFilesUploader : ICommentFilesUploader
    {
        private readonly IHostingEnvironment _environment;

        public CommentFilesUploader(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<List<CommentFiles>> UploadFiles(List<IFormFile> commentFiles, long commentId)
        {
            var files = new List<CommentFiles>();
            foreach (var file in commentFiles)
            {
                var uploads = Path.Combine(_environment.ContentRootPath, "images");
                files.Add(new CommentFiles(commentId, Path.Combine(uploads, GetUniqueFileName(file.FileName))));
                var fileStream = new FileStream(files.Last().Url, FileMode.Create);
                await file.CopyToAsync(fileStream);
                fileStream.Close();
            }

            return files;
        }

        public void DeleteCommentFiles(List<CommentFiles> toBeDeletedCommetnFiles)
        {
            foreach (var file in toBeDeletedCommetnFiles)
            {
                File.Delete(file.Url);
            }
        }

        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                   + "_"
                   + Guid.NewGuid().ToString().Substring(0, 4)
                   + Path.GetExtension(fileName);
        }
    }
}
