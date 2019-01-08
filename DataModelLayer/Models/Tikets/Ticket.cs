using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace DataModelLayer.Models.Tikets
{
    [Table("Ticket")]
    public class Ticket
    {
        [Key]
        public int Id { get; set; }
        public string TicketIssuerUserId { get; set; }
        public string Message { get; set; }
        public bool IsAddressed { get; set; }
        public string AddressedMessage { get; set; }
        public string AddressedById { get; set; }
        public List<TicketFile> TicketFiles { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class TicketsViewModel
    {
        public string TicketIssuerUserId { get; set; }
        public string Message { get; set; }
        public List<IFormFile> TicketFiles { get; set; }
    }

    public class ReportEntity
    {
        [Key]
        public int Id { get; set; }
        public int EntityId { get; set; }
        public int ReportedEntityId { get; set; }
        [NotMapped]
        public ReportedEntityType ReportedEntityType { get; set; }
        public string Message { get; set; }
        public string ReportedByUserId { get; set; }
        public string AddressedMessage { get; set; }
        public string AddressedById { get; set; }
        public bool IsAddressed { get; set; }
        public List<ReportFile> ReportFiles { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ReportEntityViewModel
    {
        public int EntityId { get; set; }
        public string IssuerId { get; set; }
        public int ReportedEntityId { get; set; }
        public string Message { get; set; }
        public List<IFormFile> TicketFiles { get; set; }
    }

    public enum ReportedEntityType
    {
        Post = 0,
        Commnet = 1
    }

    [Table("TicketFile")]
    public class TicketFile
    {
        public TicketFile(string filetPath, int ticketId)
        {
            Url = filetPath;
            TicketId = ticketId;
        }

        public int Id { get; set; }
        public int TicketId { get; set; }
        public string Url { get; set; }
    }

    [Table("ReportFile")]
    public class ReportFile
    {
        public ReportFile(string filetPath, int reportEntityId)
        {
            Url = filetPath;
            ReportEntityId = reportEntityId;
        }

        public int Id { get; set; }
        public int ReportEntityId { get; set; }
        public string Url { get; set; }
    }
}
