using System.Collections.Generic;
using System.Threading.Tasks;
using DataModelLayer.Models.Comments;
using Microsoft.AspNetCore.Http;

namespace AuthWebApi.IUploadHelpers
{
    public interface ICommentFilesUploader
    {
        Task<List<CommentFiles>> UploadFiles(List<IFormFile> commentFiles, long postId);
    }
}
