using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AspNet.Security.OpenId.Steam;

namespace GLOKON.Baiters.Server.Controllers
{
    [AllowAnonymous]
    public class AuthenticationController : Controller
    {
        [HttpGet("/login")]
        public IActionResult Login()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/" },
                SteamAuthenticationDefaults.AuthenticationScheme);
        }

        [HttpGet("/logout")]
        public IActionResult Logout()
        {
            return SignOut(new AuthenticationProperties { RedirectUri = "/" },
                CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
