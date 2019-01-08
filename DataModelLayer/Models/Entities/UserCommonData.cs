using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataModelLayer.Models.Entities
{
    [Table("UserCommonData")]
    public class UserCommonData
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public long? FacebookId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public int UserLevel { get; set; }
    }

    public class EntityCommonData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long FacebookId { get; set; }
    }
}
