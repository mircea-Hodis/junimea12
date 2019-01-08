using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataModelLayer.Models.Posts;

namespace DataAccessLayer.IRepository
{
    public interface IPostRepository
    {
        Task<Post> CreateAsync(Post post);
        Task<List<Post>> GetList(DateTime startTime, string userId);
        Task<Post> GetPostById(int postId);
        Task<List<Post>> GetUserPosts(string userId, DateTime startDate);
        Task<PostLike> LikePost(PostLike like);
        Task<DeletePostResponse> DeletePost(int postId, string callerId);
        Task<int> UpdatePostAsync(UpdatePost updatePost);
    }
}
