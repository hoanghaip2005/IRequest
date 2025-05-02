using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.IRequest
{
    
    public class Department
    {
        [Key]
        public int DepartmentID { get; set; }
        [Required(ErrorMessage = "Phải Nhập {0}"), StringLength(50)]
        [Column(TypeName = "nvarchar")]
        [Display(Name = "Tên Phòng Ban")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Phải nhập {0}")]
        [StringLength(255)]
        [Display(Name = "Mô Tả")]
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public ICollection<AppUser> Users { get; set; } = new HashSet<AppUser>();
    }
}
