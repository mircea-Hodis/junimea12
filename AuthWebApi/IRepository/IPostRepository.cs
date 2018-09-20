﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthWebApi.Models.Posts;

namespace AuthWebApi.IRepository
{
    public interface IPostRepository
    {
        Task<Post> CreateAsync(Post post);
        Task<List<Post>> GetList(DateTime startTime);
        Task<Post> GetPostById(int postId);
    }
}
