using AuthWebApi.IUploadHelpers;
using AuthWebApi.Models.Posts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AuthWebApi.UploadHelpers
{
    public class PostFilesUploadHelper : IPostFilesUploadHelper
    {
        private readonly IHostingEnvironment _environment;

        public PostFilesUploadHelper(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<Post> UploadFiles(List<IFormFile> formFiles, Post post)
        {
            foreach (var formFile in formFiles)
            {
                var uploads = Path.Combine(_environment.ContentRootPath, "images");
                post.Files.Add(new PostFiles(post.Id, Path.Combine(uploads, GetUniqueFileName(formFile.FileName))));
                await formFile.CopyToAsync(new FileStream(post.Files.Last().Url, FileMode.Create));
            }

            return post;
        }

        private void AddFileToPost(Post post, string url)
        {
            post.Files.Add(new PostFiles(post.Id, url));
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
