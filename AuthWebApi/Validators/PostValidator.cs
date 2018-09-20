using AuthWebApi.IValidators;
using AuthWebApi.ViewModels.Posts;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            if (post.Files == null || post.Files.Count == 0)
                return false;
            return true;
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
