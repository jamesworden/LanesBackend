using ClassroomGroups.Application.Features.Authentication;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ClassroomGroups.Api.Features.Authentication;

[ApiController]
[Route("classroom-groups/api/v1/[controller]")]
public class AuthenticationController(IMediator mediator, IConfiguration configuration)
  : ControllerBase
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
    if (!result.Succeeded)
    {
      return Unauthorized();
    }
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
    await mediator.Send(new UpsertAccountRequest());
    return Redirect(configuration["ClassroomGroups:LoggedInRedirectUrl"] ?? "");
  }

  [AllowAnonymous]
  [HttpPost("logout")]
  public async Task<IActionResult> Logout()
  {
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Ok();
  }

  [AllowAnonymous]
  [HttpGet("account")]
  public async Task<GetAccountResponse> GetAccount()
  {
    return await mediator.Send(new GetAccountRequest());
  }
}
