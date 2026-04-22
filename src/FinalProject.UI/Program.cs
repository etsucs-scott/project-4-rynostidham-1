using FinalProject.Core;

var builder = WebApplication.CreateBuilder(args);

var savePath = Path.Combine(builder.Environment.ContentRootPath, "Data", "save.json");
builder.Services.AddSingleton(new FileStorageService(savePath));
builder.Services.AddSingleton<GameState>();
builder.Services.AddSingleton<PathfindingService>();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();
