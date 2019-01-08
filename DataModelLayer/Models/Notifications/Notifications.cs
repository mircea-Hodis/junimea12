using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataModelLayer.Models.Notifications
{
    [Table("Notifications")]
    public class Notifications
    {
        [Key]
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
        public bool WasSeen { get; set; }
    }
}
