using LanesBackend.Caching;
using LanesBackend.Hubs;
using LanesBackend.Interfaces;
using LanesBackend.Interfaces.GameEngine;
using LanesBackend.Logic;
using LanesBackend.Logic.GameEngine;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IDeckService, DeckService>();
builder.Services.AddScoped<ILanesService, LanesService>();
builder.Services.AddScoped<IMoveChecksService, MoveChecksService>();
builder.Services.AddScoped<IGameEngineService, GameEngineService>();
builder.Services.AddScoped<IGameRuleService, GameRuleService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IGameCache, GameCache>();
builder.Services.AddScoped<IPendingGameCache, PendingGameCache>();
builder.Services.AddSignalR();
builder.Services.AddCors();

var app = builder.Build();

app.MapHub<GameHub>("/game");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors(builder =>
{
    builder.WithOrigins("http://localhost:4200", "http://chessofcards.com")
           .AllowAnyMethod()
           .AllowAnyHeader()
           .AllowCredentials();

    app.UseHttpsRedirection();
});

app.UseRouting();

app.UseAuthorization();

app.Run();