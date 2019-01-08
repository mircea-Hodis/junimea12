using DataModelLayer.ViewModels.Posts;
using Microsoft.AspNetCore.Http;

namespace AuthWebApi.IValidators
{
    public interface IPostValidator
    {
       bool ValidatePost(PostViewModel post);
       bool ValidateImage(IFormFile image);
    }
}
