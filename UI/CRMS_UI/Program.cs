using CRMS_UI.Services.Implementation;
using CRMS_UI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var backendApiUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? throw new InvalidOperationException("BackendApiUrl not configured in appsettings.");
var signalRHubPath = builder.Configuration["ApiSettings:SignalRHub"] ?? "/telemetryHub"; 

builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true; 
    options.Cookie.IsEssential = true; 
});

builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    client.BaseAddress = new Uri(backendApiUrl);
    client.Timeout = TimeSpan.FromMinutes(60); 
});

builder.Services.AddSingleton(new SignalRConfig { HubPath = signalRHubPath });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Register}");

app.Run();

public class SignalRConfig
{
    public string HubPath { get; set; }
}
