using eShop.WebApp.Components;
using eShop.ServiceDefaults;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.AddApplicationServices();

var app = builder.Build();

// -----------------------------------------------------------------------------
// 1. DÜZELTME: Forwarded Headers Ayarı (En üstte olmalı)
// -----------------------------------------------------------------------------
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};

// DİKKAT: 'KnownIPNetworks' değil, 'KnownNetworks' olmalı.
forwardedHeadersOptions.KnownNetworks.Clear(); 
forwardedHeadersOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeadersOptions);
// -----------------------------------------------------------------------------

app.MapDefaultEndpoints();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// -----------------------------------------------------------------------------
// 2. DÜZELTME: HTTPS Yönlendirmesi (Headers ayarından SONRA gelmeli)
// -----------------------------------------------------------------------------
// Not: K8s Ingress kullanıyorsanız bazen UseHttpsRedirection döngüye (loop)
// sebep olabilir. Eğer "Too many redirects" hatası alırsanız bu satırı yorum satırı yapın.
app.UseHttpsRedirection();

app.UseStaticFiles();

// -----------------------------------------------------------------------------
// 3. KRİTİK EKSİK: Authentication ve Authorization Middleware'leri
// -----------------------------------------------------------------------------
app.UseAntiforgery();

// Bu ikisi mutlaka eklenmeli, yoksa login olduktan sonra kullanıcı 
// hala "anonymous" (giriş yapmamış) görünür.
app.UseAuthentication(); 
app.UseAuthorization();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.MapForwarder("/product-images/{id}", "http://catalog-api", "/api/catalog/items/{id}/pic");

app.Run();
