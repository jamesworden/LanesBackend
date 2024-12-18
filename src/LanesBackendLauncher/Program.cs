using System.Reflection;
using System.Text.Json.Serialization;
using ChessOfCards.Api.Features.Games;
using ChessOfCards.Application.Features.Games;
using ChessOfCards.DataAccess.Interfaces;
using ChessOfCards.DataAccess.Repositories;
using ClassroomGroups.Api.Features.Authentication;
using ClassroomGroups.Api.Features.Classrooms;
using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Authentication;
using ClassroomGroups.Application.Features.Classrooms;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
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

builder
  .Services.AddAuthentication(options =>
  {
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
  })
  .AddCookie(options =>
  {
    options.Events.OnRedirectToLogin = context =>
    {
      context.Response.StatusCode = StatusCodes.Status403Forbidden;
      return Task.CompletedTask;
    };
  })
  .AddGoogle(
    GoogleDefaults.AuthenticationScheme,
    options =>
    {
      options.ClientId =
        builder.Configuration["ClassroomGroups:Authentication:Google:ClientId"] ?? "";
      options.ClientSecret =
        builder.Configuration["ClassroomGroups:Authentication:Google:ClientSecret"] ?? "";
    }
  );

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
builder.Services.AddCors();

// [ChessOfCards Service Registry]
builder.Services.AddSingleton<IPendingGameRepository, PendingGameRepository>();
builder.Services.AddSingleton<IGameRepository, GameRepository>();
builder.Services.AddSingleton<IGameTimerService, GameTimerService>();

// [ClassroomGroups Service Regsitry]
builder.Services.AddScoped<AuthBehaviorCache>();
builder.Services.AddScoped<IDetailService, DetailService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<IOrdinalService, OrdinalService>();

// [ClassroomGroups PipelineBehavior Registry]
var pipelineBehaviors = new (Type request, Type response, Type[] behaviors)[]
{
  (typeof(CreateClassroomRequest), typeof(CreateClassroomResponse), [typeof(AuthBehavior<,>)]),
  (typeof(GetAccountRequest), typeof(GetAccountResponse), [typeof(AuthBehavior<,>)]),
  (typeof(UpsertAccountRequest), typeof(UpsertAccountResponse), [typeof(AuthBehavior<,>)]),
  (typeof(GetClassroomsRequest), typeof(GetClassroomsResponse), [typeof(AuthBehavior<,>)]),
  (
    typeof(GetClassroomDetailsRequest),
    typeof(GetClassroomDetailsResponse),
    [typeof(AuthBehavior<,>)]
  ),
  (
    typeof(GetConfigurationDetailRequest),
    typeof(GetConfigurationDetailResponse),
    [typeof(AuthBehavior<,>)]
  ),
  (typeof(GetConfigurationsRequest), typeof(GetConfigurationsResponse), [typeof(AuthBehavior<,>)]),
  (
    typeof(CreateConfigurationRequest),
    typeof(CreateConfigurationResponse),
    [typeof(AuthBehavior<,>)]
  ),
  (typeof(DeleteClassroomRequest), typeof(DeleteClassroomResponse), [typeof(AuthBehavior<,>)]),
  (
    typeof(PatchConfigurationRequest),
    typeof(PatchConfigurationResponse),
    [typeof(AuthBehavior<,>)]
  ),
  (typeof(PatchClassroomRequest), typeof(PatchClassroomResponse), [typeof(AuthBehavior<,>)]),
  (typeof(CreateGroupRequest), typeof(CreateGroupResponse), [typeof(AuthBehavior<,>)]),
  (typeof(DeleteGroupRequest), typeof(DeleteGroupResponse), [typeof(AuthBehavior<,>)]),
  (typeof(CreateStudentRequest), typeof(CreateStudentResponse), [typeof(AuthBehavior<,>)]),
  (typeof(PatchGroupRequest), typeof(PatchGroupResponse), [typeof(AuthBehavior<,>)]),
  (
    typeof(DeleteConfigurationRequest),
    typeof(DeleteConfigurationResponse),
    [typeof(AuthBehavior<,>)]
  ),
  (typeof(CreateColumnRequest), typeof(CreateColumnResponse), [typeof(AuthBehavior<,>)]),
  (
    typeof(UpsertStudentFieldRequest),
    typeof(UpsertStudentFieldResponse),
    [typeof(AuthBehavior<,>)]
  ),
  (typeof(PatchFieldRequest), typeof(PatchFieldResponse), [typeof(AuthBehavior<,>)]),
  (typeof(DeleteStudentRequest), typeof(DeleteStudentResponse), [typeof(AuthBehavior<,>)]),
  (typeof(SortGroupsRequest), typeof(SortGroupsResponse), [typeof(AuthBehavior<,>)]),
  (typeof(MoveStudentRequest), typeof(MoveStudentResponse), [typeof(AuthBehavior<,>)]),
  (typeof(MoveColumnRequest), typeof(MoveColumnResponse), [typeof(AuthBehavior<,>)]),
  (typeof(DeleteColumnRequest), typeof(DeleteColumnResponse), [typeof(AuthBehavior<,>)]),
  (typeof(LockGroupRequest), typeof(LockGroupResponse), [typeof(AuthBehavior<,>)]),
  (typeof(UnlockGroupRequest), typeof(UnlockGroupResponse), [typeof(AuthBehavior<,>)]),
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
