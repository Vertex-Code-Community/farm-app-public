using FarmApp.Models.User;

namespace FarmApp.Models.Options;

public class UsersOptions
{
    public List<CredentialsModel> Users { get; init; } = [];
}