using System.ComponentModel.DataAnnotations;
using App.Models;
using App.Models.IRequest;
using Microsoft.AspNetCore.Identity;

public class Roles : IdentityRole
{

    public ICollection<AppUser> Users { get; set; }

    public ICollection<WorkflowStep> WorkflowSteps { get; set; }
}