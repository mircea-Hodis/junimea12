﻿using System.Threading.Tasks;
using AuthWebApi.IDataMappingHelpers;
using DataModelLayer.Models.Posts;
using DataModelLayer.ViewModels.Posts;

namespace AuthWebApi.DataMappingHelpers
{
    public class PostMappingHelper : IPostMappingHelper
    {
        public async Task<Post> MapPostViewModelToDataObject(PostViewModel postViewModel)
        {
            var post = new Post();
            post.Description = postViewModel.Description;
            post.PostTtile = postViewModel.PostTitle;

            return null;
        }

    }
}
