using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataManagement.Models
{
    public class PostComments
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int PostId { get; set; }
        public string Text { get; set; }
    }
}
