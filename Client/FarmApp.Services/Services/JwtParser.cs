using System.IdentityModel.Tokens.Jwt;

namespace FarmApp.Services.Services;
//
// public static class JwtParser
// {
//     public static bool TokenIsExpired(string token)
//     {
//         var tokenHandler = new JwtSecurityTokenHandler();
//
//         if (tokenHandler.CanReadToken(token))
//         {
//             var tokenInfo = tokenHandler.ReadJwtToken(token);
//             return !(tokenInfo.ValidTo.ToLocalTime() > DateTime.Now);
//         }
//
//         return true;
//     }
// }
