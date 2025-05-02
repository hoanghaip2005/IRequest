using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace App.Areas.Identity.Models.UserViewModels
{
  public class AddUserRoleModel
  {
    public AppUser user { get; set; }
    [Required(ErrorMessage = "Vui lòng chọn ít nhất một role")]

    [DisplayName("Các role gán cho user")]
    public string[] RoleNames { get; set; }

    public List<IdentityRoleClaim<string>> claimsInRole { get; set; }
    public List<IdentityUserClaim<string>> claimsInUserClaim { get; set; }

  }
}