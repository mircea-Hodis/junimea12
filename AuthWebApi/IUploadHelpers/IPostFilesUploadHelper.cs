using AuthWebApi.Models.Posts;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuthWebApi.IUploadHelpers
{
    public interface IPostFilesUploadHelper
    {
        Task<Post> UploadFiles(List<IFormFile> formFiles, Post post);
    }
}
