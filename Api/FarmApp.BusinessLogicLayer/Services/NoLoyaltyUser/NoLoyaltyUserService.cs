using FarmApp.BusinessLogicLayer.Providers;
using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.DataAccessLayer.Repositories.Interfaces;

namespace FarmApp.BusinessLogicLayer.Services.NoLoyaltyUser;

public partial class NoLoyaltyUserService : INoLoyaltyUserService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtProvider _jwtProvider;
    private readonly IEmailService _emailService;

    public NoLoyaltyUserService(
        IUserRepository userRepository,
        JwtProvider jwtProvider,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _jwtProvider = jwtProvider;
        _emailService = emailService;
    }
}