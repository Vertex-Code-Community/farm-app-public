using FarmApp.Shared.Enums;

namespace FarmApp.ViewModels.Users;

public class UserViewModel
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public int IconNumber { get; set; }
    public UserRole UserType { get; set; }
    public DateTime Created { get; set; }
}