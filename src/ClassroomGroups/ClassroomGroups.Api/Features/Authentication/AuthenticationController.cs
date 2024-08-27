using ClassroomGroups.Application.Features.Accounts.Requests;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ClassroomGroups.Api.Features.Authentication;

[ApiController]
[Route("classroom-groups/api/v1/[controller]")]
public class AuthenticationController(IMediator mediator, IConfiguration configuration)
  : ControllerBase
{
  private readonly IMediator _mediator = mediator;

  private readonly IConfiguration _configuration = configuration;

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
    await _mediator.Send(new UpsertAccountRequest());
    return Redirect(_configuration["ClassroomGroups:LoggedInRedirectUrl"] ?? "");
  }

  [Authorize]
  [HttpPost("logout")]
  public async Task<IActionResult> Logout()
  {
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return new EmptyResult();
  }

  [Authorize]
  [HttpGet("get-account")]
  public async Task<Account?> GetAccount()
  {
    return await _mediator.Send(new GetAccountRequest());
  }
}
