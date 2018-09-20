﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace AuthWebApi.ViewModels.Posts
{
    public class PostViewModel
    {
        public string PostTitle { get; set; }
        public string Description { get; set; }
        public List<IFormFile> Files { get; set; }
    }
}