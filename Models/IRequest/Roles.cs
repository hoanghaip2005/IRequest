using System.ComponentModel.DataAnnotations;
using App.Models.IRequest;
using Microsoft.AspNetCore.Identity;

public class Roles : IdentityRole
{

    public ICollection<Users> Users { get; set; }

    public ICollection<WorkflowStep> WorkflowSteps { get; set; }
}