using ComandasVentas.Blazor.Components;
using ComandasVentas.Blazor.Services.Auth;
using ComandasVentas.Blazor.Services.Caja;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<AppSessionState>();
builder.Services.AddScoped<LoginDataService>();
builder.Services.AddScoped<LogoCacheService>();
builder.Services.AddScoped<CajaDataService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
