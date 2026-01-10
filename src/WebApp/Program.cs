using eShop.WebApp.Components;
using eShop.ServiceDefaults;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.AddApplicationServices();

var app = builder.Build();

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownIPProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseAntiforgery();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.MapForwarder("/product-images/{id}", "http://catalog-api", "/api/catalog/items/{id}/pic");

app.Run();
