using System.ComponentModel.DataAnnotations;
using FarmApp.Shared.Enums;

namespace FarmApp.ViewModels.Users;

public class UpdateUserModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "First Name is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "UserName must be 3-50 characters")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Last Name is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Last Name must be 3-50 characters")]
    public string LastName { get; set; } = string.Empty;
    
    [StringLength(50, MinimumLength = 8, ErrorMessage = "Password must be 8-50 characters")]
    public string? Password { get; set; }
}