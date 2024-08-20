﻿using System.Reflection;
using System.Text.Json.Serialization;
using ChessOfCards.Api.Features.Games;
using ChessOfCards.Application.Features.Games;
using ChessOfCards.DataAccess.Interfaces;
using ChessOfCards.DataAccess.Repositories;
using ClassroomGroups.Api.Features.ClassroomGroups;
using ClassroomGroups.Api.Features.Login;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Cors.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// App Settings
builder
  .Configuration.SetBasePath(builder.Environment.ContentRootPath)
  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
  .AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.json",
    optional: true,
    reloadOnChange: true
  )
  .AddEnvironmentVariables();

// Auth | Preemptively devising authentication schemes until we need them is overkill.
// The default one is for ClassroomGroups "sign in with google" redirection auth.
builder
  .Services.AddAuthentication(options =>
  {
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
  })
  .AddCookie(options =>
  {
    options.LoginPath = "/authentication/login";
    options.LogoutPath = "/authentication/logout";
  })
  .AddGoogle(
    GoogleDefaults.AuthenticationScheme,
    options =>
    {
      options.ClientId =
        builder.Configuration.GetSection("ClassroomGroups:Authentication:Google:ClientId").Value
        ?? "";
      options.ClientSecret =
        builder.Configuration.GetSection("ClassroomGroups:Authentication:Google:ClientSecret").Value
        ?? "";
    }
  );

builder.Services.AddAuthorization();

// Register event handlers from assemblies
builder.Services.AddMediatR(cfg =>
  cfg.RegisterServicesFromAssemblies(
    typeof(GameNameInvalidCommandHandler).GetTypeInfo().Assembly, // Represents the 'ChessOfCards.Api' project.
    typeof(CreatePendingGameCommandHandler).GetTypeInfo().Assembly // Represents the 'ChessOfCards.Application' project.
  )
);

// Register controllers from assemblies
builder.Services.AddControllers().AddApplicationPart(typeof(AuthenticationController).Assembly);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder
  .Services.AddSignalR()
  .AddJsonProtocol(options =>
  {
    // Helps ensure that enum values translate well when recieving websocket requests.
    options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
  });
builder.Services.AddCors();

builder.Services.AddSingleton<IPendingGameRepository, PendingGameRepository>();
builder.Services.AddSingleton<IGameRepository, GameRepository>();
builder.Services.AddSingleton<IGameTimerService, GameTimerService>();

builder.Services.AddSwaggerGen();

builder.Services.AddCors(Options =>
{
  Options.AddPolicy(
    "frontendApplications",
    CorsPolicyBuilder =>
    {
      CorsPolicyBuilder
        .WithOrigins(
          "http://localhost:4200",
          "https://localhost:4200",
          "http://chessofcards.com",
          "https://chessofcards.com",
          "http://classroomgroups.com",
          "https://classroomgroups.com"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    }
  );
});

var app = builder.Build();

app.MapHub<GameHub>("/game");
app.MapHub<ClassroomsHub>("/classroom-groups");

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}
else
{
  app.UseExceptionHandler("/Error");
  app.UseHsts();
}

app.UseCors("frontendApplications");

app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
