using System.Reflection;
using System.Text.Json.Serialization;
using Amazon.S3;
using ChessOfCards.Api.Features.Games;
using ChessOfCards.Application.Features.Games;
using ChessOfCards.DataAccess.Interfaces;
using ChessOfCards.DataAccess.Repositories;
using ClassroomGroups.Api.Features.Authentication;
using ClassroomGroups.Api.Features.Classrooms;
using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Behaviors.Shared;
using ClassroomGroups.Application.Features.Authentication;
using ClassroomGroups.Application.Features.Classrooms;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using Google.Apis.Auth.AspNetCore3;
using LanesBackendLauncher.Util;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder
  .Configuration.SetBasePath(builder.Environment.ContentRootPath)
  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
  .AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.json",
    optional: true,
    reloadOnChange: true
  )
  .AddEnvironmentVariables()
  .AddSystemsManager(builder.Configuration["AppSecrets:SystemsManagerPath"]);

builder.Logging.AddAWSProvider();

builder.Services.Configure<DatabaseBackupSettings>(
  builder.Configuration.GetSection("DatabaseBackup")
);

builder.Services.AddAWSService<IAmazonS3>();

builder.Services.AddHostedService<DatabaseBackupService>();

builder
  .Services.AddAuthentication(options =>
  {
    options.DefaultChallengeScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
    options.DefaultForbidScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
  })
  .AddCookie(options =>
  {
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = false;
    options.Cookie.IsEssential = true;
    options.Events.OnRedirectToLogin = context =>
    {
      context.Response.StatusCode = StatusCodes.Status403Forbidden;
      return Task.CompletedTask;
    };
  })
  .AddGoogleOpenIdConnect(options =>
  {
    options.ClientId =
      builder.Configuration["ClassroomGroups:Authentication:Google:ClientId"] ?? "";
    options.ClientSecret =
      builder.Configuration["ClassroomGroups:Authentication:Google:ClientSecret"] ?? "";

    options.CallbackPath = "/api/v1/authentication/login-with-google-response";

    options.Events.OnRedirectToIdentityProvider = context =>
    {
      var redirectUri = builder.Configuration["ClassroomGroups:LoginRedirectUrl"] ?? "";

      // Ensure HTTPS is enforced
      var uriBuilder = new UriBuilder(redirectUri)
      {
        Scheme = Uri.UriSchemeHttps,
        Port = -1 // Removes explicit port numbers if they exist
      };

      context.ProtocolMessage.RedirectUri = uriBuilder.ToString();
      return Task.CompletedTask;
    };

    options.SignedOutRedirectUri =
      builder.Configuration["ClassroomGroups:LoggedOutRedirectUrl"] ?? "";

    options.Events.OnTicketReceived = async context =>
    {
      context.ReturnUri = builder.Configuration["ClassroomGroups:LoggedInRedirectUrl"] ?? "";
      await Task.CompletedTask;
    };
  });

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();

// [MediatR EventHandler Registry]
builder.Services.AddMediatR(cfg =>
  cfg.RegisterServicesFromAssemblies(
    typeof(GameNameInvalidCommandHandler).GetTypeInfo().Assembly, // Represents the 'ChessOfCards.Api' project.
    typeof(CreatePendingGameCommandHandler).GetTypeInfo().Assembly, // Represents the 'ChessOfCards.Application' project.
    typeof(GetAccountRequest).GetTypeInfo().Assembly // Represents the 'ClassroomGroups.Application' project.
  )
);
builder
  .Services.AddControllers()
  .AddApplicationPart(typeof(AuthenticationController).Assembly) // Represents the 'ClassroomGroups.Application' project.
  .AddJsonOptions(options =>
  {
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
  });

builder.Services.AddEndpointsApiExplorer();

builder
  .Services.AddSignalR()
  .AddJsonProtocol(options =>
  {
    options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
  });

// [ChessOfCards Service Registry]
builder.Services.AddSingleton<IPendingGameRepository, PendingGameRepository>();
builder.Services.AddSingleton<IGameRepository, GameRepository>();
builder.Services.AddSingleton<IGameTimerService, GameTimerService>();

// [ClassroomGroups Service Regsitry]
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<AccountRequiredCache>();
builder.Services.AddScoped<AccountOptionalCache>();
builder.Services.AddScoped<IDetailService, DetailService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<IOrdinalService, OrdinalService>();

// [ClassroomGroups PipelineBehavior Registry]
var pipelineBehaviors = new (Type request, Type response, Type[] behaviors)[]
{
  (
    typeof(CreateClassroomRequest),
    typeof(CreateClassroomResponse),
    [typeof(AccountRequiredBehavior<,>)]
  ),
  (typeof(GetAccountRequest), typeof(GetAccountResponse), [typeof(AccountOptionalBehavior<,>)]),
  (
    typeof(UpsertAccountRequest),
    typeof(UpsertAccountResponse),
    [typeof(AccountOptionalBehavior<,>)]
  ),
  (
    typeof(GetClassroomDetailsRequest),
    typeof(GetClassroomDetailsResponse),
    [typeof(AccountRequiredBehavior<,>)]
  ),
  (
    typeof(GetConfigurationDetailRequest),
    typeof(GetConfigurationDetailResponse),
    [typeof(AccountRequiredBehavior<,>)]
  ),
  (
    typeof(GetConfigurationsRequest),
    typeof(GetConfigurationsResponse),
    [typeof(AccountRequiredBehavior<,>)]
  ),
  (
    typeof(CreateConfigurationRequest),
    typeof(CreateConfigurationResponse),
    [typeof(AccountRequiredBehavior<,>)]
  ),
  (
    typeof(DeleteClassroomRequest),
    typeof(DeleteClassroomResponse),
    [typeof(AccountRequiredBehavior<,>)]
  ),
  (
    typeof(PatchConfigurationRequest),
    typeof(PatchConfigurationResponse),
    [typeof(AccountRequiredBehavior<,>)]
  ),
  (
    typeof(PatchClassroomRequest),
    typeof(PatchClassroomResponse),
    [typeof(AccountRequiredBehavior<,>)]
  ),
  (typeof(CreateGroupRequest), typeof(CreateGroupResponse), [typeof(AccountRequiredBehavior<,>)]),
  (typeof(DeleteGroupRequest), typeof(DeleteGroupResponse), [typeof(AccountRequiredBehavior<,>)]),
  (
    typeof(CreateStudentRequest),
    typeof(CreateStudentResponse),
    [typeof(AccountRequiredBehavior<,>)]
  ),
  (typeof(PatchGroupRequest), typeof(PatchGroupResponse), [typeof(AccountRequiredBehavior<,>)]),
  (
    typeof(DeleteConfigurationRequest),
    typeof(DeleteConfigurationResponse),
    [typeof(AccountRequiredBehavior<,>)]
  ),
  (typeof(CreateColumnRequest), typeof(CreateColumnResponse), [typeof(AccountRequiredBehavior<,>)]),
  (
    typeof(UpsertStudentFieldRequest),
    typeof(UpsertStudentFieldResponse),
    [typeof(AccountRequiredBehavior<,>)]
  ),
  (typeof(PatchFieldRequest), typeof(PatchFieldResponse), [typeof(AccountRequiredBehavior<,>)]),
  (
    typeof(DeleteStudentRequest),
    typeof(DeleteStudentResponse),
    [typeof(AccountRequiredBehavior<,>)]
  ),
  (typeof(SortGroupsRequest), typeof(SortGroupsResponse), [typeof(AccountRequiredBehavior<,>)]),
  (typeof(MoveStudentRequest), typeof(MoveStudentResponse), [typeof(AccountRequiredBehavior<,>)]),
  (typeof(MoveColumnRequest), typeof(MoveColumnResponse), [typeof(AccountRequiredBehavior<,>)]),
  (typeof(DeleteColumnRequest), typeof(DeleteColumnResponse), [typeof(AccountRequiredBehavior<,>)]),
  (typeof(LockGroupRequest), typeof(LockGroupResponse), [typeof(AccountRequiredBehavior<,>)]),
  (typeof(UnlockGroupRequest), typeof(UnlockGroupResponse), [typeof(AccountRequiredBehavior<,>)]),
  (
    typeof(GroupStudentsRequest),
    typeof(GroupStudentsResponse),
    [typeof(AccountRequiredBehavior<,>)]
  ),
};
foreach (var (request, response, behaviors) in pipelineBehaviors)
{
  foreach (var behavior in behaviors)
  {
    builder.Services.AddTransient(
      typeof(IPipelineBehavior<,>).MakeGenericType(request, response),
      behavior.MakeGenericType(request, response)
    );
  }
}

var connectionString = builder.Configuration["ClassroomGroups:ConnectionString"] ?? "";

builder.Services.AddDbContext<ClassroomGroupsContext>(options =>
  options.UseSqlite(connectionString, b => b.MigrationsAssembly("ClassroomGroups.DataAccess"))
);

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
          "http://www.classroomgroups.com",
          "https://www.classroomgroups.com",
          "http://classroomgroups.com",
          "https://classroomgroups.com"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    }
  );
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
  var dbContext = scope.ServiceProvider.GetRequiredService<ClassroomGroupsContext>();
  dbContext.Database.EnsureCreated();
}

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

app.UseCookiePolicy(new CookiePolicyOptions { Secure = CookieSecurePolicy.Always });

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
