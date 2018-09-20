using AuthWebApi.Models.Posts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuthWebApi.IRepository
{
    public interface IFilesRepository
    {
        Task<List<PostFiles>> AddPostImagesAsync(List<PostFiles> post);
    }
}
