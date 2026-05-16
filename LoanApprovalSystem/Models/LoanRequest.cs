using System.ComponentModel.DataAnnotations;

namespace LoanApprovalSystem.Models
{
    public class LoanRequest
    {
        [Key]
        public int Id { get; set; }

        public string LoanNumber { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public string Description { get; set; }

        public string AttachmentPath { get; set; }

        public string Status { get; set; }

        public string CurrentApproverRole { get; set; }

        public string CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
