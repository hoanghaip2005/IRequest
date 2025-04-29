using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace App.Models.IRequest
{
    public class Users : IdentityUser
    {
        [Column(TypeName = "nvarchar")]
        [StringLength(400)]
        public string HomeAdress { get; set; }

        // [Required]       
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }
        public bool IsActive { get; set; } = true;

        public int? DepartmentID { get; set; }
        public Department? Department { get; set; }

        public ICollection<Request> Requests { get; set; }
    }
}