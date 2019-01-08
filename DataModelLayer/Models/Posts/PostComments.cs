namespace DataModelLayer.Models.Posts
{
    public class PostComments
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int PostId { get; set; }
        public string Text { get; set; }
    }
}
