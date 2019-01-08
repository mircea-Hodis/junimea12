using AuthWebApi.IValidators;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using DataModelLayer.ViewModels.Posts;

namespace AuthWebApi.Validators
{
    public class PostValidator : IPostValidator
    {
        private readonly string[] _acceptedFormas;

        public PostValidator()
        {
            _acceptedFormas = new string[] { "jpg", "png" };
        }

        public bool ValidatePost(PostViewModel post)
        {
            return post.Files == null || post.Files.Count == 0 ? false : true;
        }

        public bool ValidateImage(IFormFile image)
        {
            var extension = image.ContentType;  
            return ValidateImageExtension(image.ContentType);
        }

        private bool ValidateImageExtension(string extension)
        {
            return _acceptedFormas.Any(format => string.Compare(format, extension, StringComparison.InvariantCulture) == 1);
        }
    }
}
