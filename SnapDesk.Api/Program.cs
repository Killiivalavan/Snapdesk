using SnapDesk.Core;
using SnapDesk.Core.Interfaces;
using SnapDesk.Core.Services;
using SnapDesk.Data.Configuration;
using SnapDesk.Data.Repositories;
using SnapDesk.Data.Services;
using SnapDesk.Platform;
using LiteDB;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging(logging => logging.SetMinimumLevel(LogLevel.Information));

var dbPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "SnapDesk", "snapdesk.db");
var dbConfig = DatabaseConfiguration.CreateForPath(dbPath);
builder.Services.AddSingleton(dbConfig);
builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
builder.Services.AddSingleton<ILayoutRepository, LayoutRepository>();
builder.Services.AddSingleton<IHotkeyRepository, HotkeyRepository>();
builder.Services.AddSingleton<IRepository<HotkeyInfo>, HotkeyRepository>();
builder.Services.AddSingleton<SnapDesk.Platform.Interfaces.IWindowApi>(_ => PlatformFactory.CreateWindowApi());
builder.Services.AddSingleton<SnapDesk.Platform.Interfaces.IHotkeyApi>(_ => PlatformFactory.CreateHotkeyApi());
builder.Services.AddSingleton<IWindowService, WindowService>();
builder.Services.AddSingleton<IHotkeyService, HotkeyService>();
builder.Services.AddSingleton<ILayoutService, LayoutService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var db = app.Services.GetRequiredService<IDatabaseService>();
await db.InitializeAsync();

var info = PlatformFactory.GetPlatformInfo();

static ObjectId ParseId(string id, out bool ok)
{
    try { ok = true; return new ObjectId(id); }
    catch { ok = false; return ObjectId.Empty; }
}

app.MapGet("/api/status", () => Results.Ok(new
{
    status = "running",
    platform = info.OperatingSystem,
    database = db.IsConnected ? "connected" : "disconnected",
    timestamp = DateTime.UtcNow
}));

app.MapGet("/api/layouts", async (ILayoutService s) =>
{
    var layouts = await s.GetAllLayoutsAsync();
    return Results.Ok(layouts);
});

app.MapGet("/api/layouts/{id}", async (string id, ILayoutService s) =>
{
    var oid = ParseId(id, out var ok);
    if (!ok) return Results.BadRequest("Invalid ID");
    var layout = await s.GetLayoutAsync(oid);
    return layout is not null ? Results.Ok(layout) : Results.NotFound();
});

app.MapPost("/api/layouts", async (SaveRequest req, ILayoutService s) =>
{
    var layout = await s.SaveCurrentLayoutAsync(req.Name, req.Description);
    return Results.Created($"/api/layouts/{layout.Id}", layout);
});

app.MapPost("/api/layouts/{id}/restore", async (string id, ILayoutService s) =>
{
    var oid = ParseId(id, out var ok);
    if (!ok) return Results.BadRequest("Invalid ID");
    var restored = await s.RestoreLayoutAsync(oid);
    return restored ? Results.Ok() : Results.NotFound();
});

app.MapDelete("/api/layouts/{id}", async (string id, ILayoutService s) =>
{
    var oid = ParseId(id, out var ok);
    if (!ok) return Results.BadRequest("Invalid ID");
    var deleted = await s.DeleteLayoutAsync(oid);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.MapGet("/api/hotkeys", async (IHotkeyService s) =>
{
    var hotkeys = await s.GetRegisteredHotkeysAsync();
    return Results.Ok(hotkeys);
});

app.MapPost("/api/hotkeys", async (HotkeyRequest req, IHotkeyService s) =>
{
    var hotkey = new HotkeyInfo(req.Keys, req.Action);
    var ok = await s.RegisterHotkeyAsync(hotkey, () => Task.CompletedTask);
    return ok ? Results.Created($"/api/hotkeys/{hotkey.Id}", hotkey) : Results.Conflict();
});

app.MapDelete("/api/hotkeys/{id}", async (string id, IHotkeyService s) =>
{
    var oid = ParseId(id, out var ok);
    if (!ok) return Results.BadRequest("Invalid ID");
    var deleted = await s.UnregisterHotkeyAsync(oid);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.MapGet("/api/windows", async (IWindowService s) =>
{
    try
    {
        var windows = await s.GetCurrentWindowsAsync();
        return Results.Ok(windows);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/api/monitors", async (IWindowService s) =>
{
    try
    {
        var monitors = await s.GetMonitorConfigurationAsync();
        return Results.Ok(monitors);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.Run();

internal record SaveRequest(string Name, string? Description = null);
internal record HotkeyRequest(string Keys, HotkeyAction Action);
