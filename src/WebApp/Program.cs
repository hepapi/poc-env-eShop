using eShop.WebApp.Components;
using eShop.ServiceDefaults;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.AddApplicationServices();

var app = builder.Build();

// -----------------------------------------------------------------------------
// 1. Forwarded Headers Ayarı (En üstte olması kritik)
// -----------------------------------------------------------------------------
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};

// .NET 10 ile gelen değişiklik: KnownNetworks yerine KnownIPNetworks kullanılır.
// Konteyner ortamında Load Balancer IP'si değişebileceği için listeyi temizliyoruz.
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeadersOptions);
// -----------------------------------------------------------------------------

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days.
    app.UseHsts();
}

app.UseAntiforgery();

app.UseHttpsRedirection();

app.UseStaticFiles();

// -----------------------------------------------------------------------------
// 2. EKLENEN KISIM: Authentication ve Authorization
// -----------------------------------------------------------------------------
// Bu satırlar olmadan IdentityServer'dan dönen token işlenemez 
// ve kullanıcı giriş yapmış sayılmaz.
app.UseAuthentication();
app.UseAuthorization();
// -----------------------------------------------------------------------------

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.MapForwarder("/product-images/{id}", "http://catalog-api", "/api/catalog/items/{id}/pic");

app.Run();
