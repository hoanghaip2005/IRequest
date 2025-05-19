using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using App.Models.IRequest;
using Microsoft.AspNetCore.Identity;

namespace App.Models
{
    public class AppUser : IdentityUser
    {
        [Column(TypeName = "nvarchar")]
        [StringLength(400)]
        public string? HomeAdress { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }
        public int? DepartmentID { get; set; }
        [ForeignKey("DepartmentID")]
        public Department? Department { get; set; }

        public ICollection<App.Models.IRequest.Request> Requests { get; set; } = new HashSet<App.Models.IRequest.Request>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}