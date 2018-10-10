﻿using System.Collections.Generic;
using System.Threading.Tasks;
using AuthWebApi.Models.Comments;
using Microsoft.AspNetCore.Http;

namespace AuthWebApi.IUploadHelpers
{
    public interface ICommentFilesUploadHelpers
    {
        Task<List<CommentFiles>> UploadFiles(List<IFormFile> commentFiles, long postId);
    }
}
