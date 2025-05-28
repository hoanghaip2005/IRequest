using System;
using System.ComponentModel.DataAnnotations;

namespace App.Models.IRequest
{
    public class RequestApproval
    {
        [Key]
        public int Id { get; set; }
        
        public int RequestId { get; set; }
        public Request Request { get; set; }
        
        public string ApprovedByUserId { get; set; }
        public AppUser ApprovedByUser { get; set; }
        
        public DateTime ApprovedAt { get; set; }
        
        public string Note { get; set; }
    }
} 