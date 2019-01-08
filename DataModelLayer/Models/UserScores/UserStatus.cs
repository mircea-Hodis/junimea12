using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataModelLayer.Models.UserScores
{
    [Table("UsersStatuses")]
    public class UserStatus
    {
        [Key]
        public string Id { get; set; }
        public bool IsEligebleForAdminProposal { get; set; }
    }

    [Table("AdminProposals")]
    public class AdminProposal
    {
        public string Id { get; set; }
        public DateTime ProposedDate { get; set; }
        public bool IsAccepted { get; set; }    
        public DateTime AcceptedDate { get; set; }
    }

    public class AcceptAdminProposalViewModel
    {
        public string UserId { get; set; }
        public bool Accepted { get; set; }
    }
}
