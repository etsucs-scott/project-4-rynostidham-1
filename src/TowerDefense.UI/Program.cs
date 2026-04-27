using TowerDefense.Core.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Register game services. SaveGameService uses current directory for save files.
var saveDir = Path.Combine(Directory.GetCurrentDirectory(), "data");
Directory.CreateDirectory(saveDir);

builder.Services.AddSingleton(_ => new SaveGameService(saveDir));
builder.Services.AddSingleton(_ => new LeaderboardService(saveDir));
builder.Services.AddSingleton<PathFinder>();
builder.Services.AddSingleton<WaveManager>();

// GameEngine depends on the above, so register last
builder.Services.AddSingleton<GameEngine>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
