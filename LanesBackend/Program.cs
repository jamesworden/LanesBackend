using LanesBackend;
using LanesBackend.Hubs;
using LanesBackend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ICacheService, InMemoryCacheService>();
builder.Services.AddMemoryCache();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

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

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();