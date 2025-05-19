using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.IRequest
{
    [Table("Comments")]
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        [Required(ErrorMessage = "Phải có nội dung")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
        [Display(Name = "Nội dung")]
        public string? Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [Required]
        public int? RequestId { get; set; }

        [ForeignKey("RequestId")]
        public Request Request { get; set; }

        [Required]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public AppUser User { get; set; }
    }
}