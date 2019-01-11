using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataModelLayer.Models.Posts;
using DataModelLayer.Models.Tikets;

namespace AuthWebApi.IUploadHelpers
{
    public interface IPostFilesUploadHelper
    {
        Task<Post> UploadFiles(List<IFormFile> formFiles, Post post);
        Task<List<PostFiles>> ReplacePostFiles(List<PostFiles> oldFiles, List<IFormFile> newFiles, int postId);
        void DeleteFiles(Post post);

    }
}
