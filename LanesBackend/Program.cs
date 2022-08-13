using LanesBackend.Hubs;

var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddSingleton<ICacheService, InMemoryCacheService>();
// builder.Services.AddMemoryCache();
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
    builder.WithOrigins("http://localhost:4200")
           .AllowAnyMethod()
           .AllowAnyHeader()
           .AllowCredentials();

    app.UseHttpsRedirection();
});

app.UseRouting();

app.UseAuthorization();

app.Run();