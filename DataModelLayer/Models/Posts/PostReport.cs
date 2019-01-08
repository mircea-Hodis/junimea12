using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataModelLayer.Models.Posts
{
    [Table("PostReports")]
    public class PostReport
    {
        [Key]
        public int Id { get; set; }
        public int PostId { get; set; }
        public string ReportById { get; set; }
        public string Reason { get; set; }
        public DateTime ReportTime { get; set; }
    }

}
