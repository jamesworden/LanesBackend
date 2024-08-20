using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClassroomGroups.Api.Features.Authentication;

[ApiController]
[Route("classroom-groups/api/v1/[controller]")]
public class AuthenticationController : ControllerBase
{
  [AllowAnonymous]
  [HttpPost("login-with-google")]
  public async Task LoginWithGoogle()
  {
    await HttpContext.ChallengeAsync(
      GoogleDefaults.AuthenticationScheme,
      new AuthenticationProperties { RedirectUri = Url.Action("LoginWithGoogleResponse") }
    );
  }

  [AllowAnonymous]
  [HttpGet("login-with-google-response")]
  public async Task<IActionResult> LoginWithGoogleResponse()
  {
    var result = await HttpContext.AuthenticateAsync(
      CookieAuthenticationDefaults.AuthenticationScheme
    );
    if (result is null || result.Succeeded == false || result.Principal == null)
    {
      return new EmptyResult();
    }
    var identity = result.Principal.Identities.FirstOrDefault();
    if (identity is null)
    {
      return new EmptyResult();
    }
    await Request.HttpContext.SignInAsync("Cookies", result.Principal);
    return Redirect("http://localhost:4200");
  }

  [Authorize]
  [HttpPost("logout")]
  public async Task<IActionResult> Logout()
  {
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return new EmptyResult();
  }
}
