using AuthWebApi.IUploadHelpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using DataModelLayer.Models.Posts;
using Microsoft.CodeAnalysis.CSharp;

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
                var fileStream = new FileStream(post.Files.Last().Url, FileMode.Create);
                await formFile.CopyToAsync(fileStream);
                fileStream.Close();
            }

            return post;
        }

        public async Task<List<PostFiles>> ReplacePostFiles(List<PostFiles> oldFiles, List<IFormFile> newFiles, int postId)
        {
            var newFilesList = new List<PostFiles>();
            foreach (var file in oldFiles)
            {
                File.Delete(file.Url);
            }

            foreach (var newFile in newFiles)
            {
                var uploads = Path.Combine(_environment.ContentRootPath, "images");
                newFilesList.Add(new PostFiles(postId, Path.Combine(uploads, GetUniqueFileName(newFile.FileName))));
                await newFile.CopyToAsync(new FileStream(newFilesList.Last().Url, FileMode.Create));
            }

            return newFilesList;
        }

        public void DeleteFiles(Post post)
        {
            foreach (var file in post.Files)
            {
                File.Delete(file.Url);
            }

            foreach (var comment in post.Comments)
            {
                foreach (var file in comment.Files)
                {
                    File.Delete(file.Url);
                }
            }
        }

        private static string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);

            return Path.GetFileNameWithoutExtension(fileName)
                      + "_"
                      + Guid.NewGuid().ToString().Substring(0, 4)
                      + Path.GetExtension(fileName);
        }
    }
}
