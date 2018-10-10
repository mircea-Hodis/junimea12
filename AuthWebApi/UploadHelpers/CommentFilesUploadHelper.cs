using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AuthWebApi.IUploadHelpers;
using AuthWebApi.Models.Comments;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace AuthWebApi.UploadHelpers
{
    public class CommentFilesUploadHelper : ICommentFilesUploadHelpers
    {
        private readonly IHostingEnvironment _environment;

        public CommentFilesUploadHelper(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<List<CommentFiles>> UploadFiles(List<IFormFile> commentFiles, long postId)
        {
            var files = new List<CommentFiles>();
            foreach (var file in commentFiles)
            {
                var uploads = Path.Combine(_environment.ContentRootPath, "images");
                files.Add(new CommentFiles(postId, Path.Combine(uploads, GetUniqueFileName(file.FileName))));
                await file.CopyToAsync(new FileStream(files.Last().Url, FileMode.Create));
            }

            return files;
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
