using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataModelLayer.Models.Entities
{
    public class Ban
    {
        [Key]
        public int BanId { get; set; }
        public string BannedUserId { get; set; }
        public string BannedById { get; set; }
        public DateTime BanStart { get; set; }
        public DateTime BanEnd { get; set; }
        public long? FacebookId { get; set; }
        public string BannedEmail { get; set; }
        public bool IsBanActive { get; set; }
        [NotMapped]
        public bool IsPermanentBan { get; set; }
    }

    public class BanDisplayModel
    {
        public int BanId { get; set; }
        public string BannedUserId { get; set; }
        public string BannedById { get; set; }
        public DateTime BanStart { get; set; }
        public DateTime BanEnd { get; set; }
        public long? FacebookId { get; set; }
        public string BannedEmail { get; set; }
        [NotMapped]
        public bool IsPermanentBan { get; set; }
        public string BannedByUserName { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
