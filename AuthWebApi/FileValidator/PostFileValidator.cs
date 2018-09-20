using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AuthWebApi.FileValidator
{
    public class PostFileValidator
    {
        public async Task<bool> FileValidator(IFormFile postFile)
        {
            return false;
        }

    }
}
