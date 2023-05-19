using LanesBackend.Caching;
using LanesBackend.Hubs;
using LanesBackend.Interfaces;
using LanesBackend.Logic;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddCors();

builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IGameCodeService, GameCodeService>();
builder.Services.AddScoped<ILanesService, LanesService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IGameCache, GameCache>();
builder.Services.AddScoped<IPendingGameCache, PendingGameCache>();

var app = builder.Build();

app.MapHub<GameHub>("/game");

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

app.UseCors(builder =>
{
    builder.WithOrigins("http://localhost:4200", "https://localhost:4200", "http://chessofcards.com", "https://chessofcards.com")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
});

app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();