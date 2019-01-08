using System;
using System.Collections.Generic;
using DataModelLayer.Models.Posts;

namespace DataModelLayer.ViewModels.UserProfile
{
    public class UserProfile
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<Post> Posts { get; set; }
    }

    public class UserProfileRequest
    {
        public string UserId { get; set; }
        public DateTime StartDate { get; set; }
    }
}
