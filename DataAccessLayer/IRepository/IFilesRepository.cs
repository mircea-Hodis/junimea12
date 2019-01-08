using System.Collections.Generic;
using System.Threading.Tasks;
using DataModelLayer.Models.Posts;

namespace DataAccessLayer.IRepository
{
    public interface IFilesRepository
    {
        Task<List<PostFiles>> AddPostImagesAsync(List<PostFiles> post);
        Task<List<PostFiles>> UpdatePostImagesAsync(List<PostFiles> postFiles, int postId);
    }
}
