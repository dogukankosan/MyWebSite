using AspNetCoreRateLimit;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using MyWebSite.Business;
using MyWebSite.Classes;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews()
    .AddFluentValidation(fv =>
    {
        fv.DisableDataAnnotationsValidation = true;
        fv.RegisterValidatorsFromAssemblyContaining<ContactValidator>();
    });
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "__Host-AF";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "__Host-Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax; 
        options.LoginPath = "/AdminGiris/Panel";
        options.AccessDeniedPath = "/AdminGiris/Panel";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.RealIpHeader = "X-Forwarded-For";
    options.HttpStatusCode = StatusCodes.Status429TooManyRequests;
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "GET:/d/*",
            Period = "1m",
            Limit = 300 
        },
        new RateLimitRule
        {
            Endpoint = "POST:/d/*",
            Period = "1m",
            Limit = 40 
        },
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
            Limit = 1000
        }
    };
});
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = new[]
    {
        "text/plain", "text/css", "text/html", "text/xml",
        "application/json", "application/xml",
        "application/javascript", "text/javascript",
        "image/svg+xml"
    };
});
WebApplication app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts(); 
}
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
});
app.UseHttpsRedirection();
app.Use(async (ctx, next) =>
{
    ctx.Response.OnStarting(() =>
    {
        var h = ctx.Response.Headers;
        if (!h.ContainsKey("X-Content-Type-Options")) h["X-Content-Type-Options"] = "nosniff";
        if (!h.ContainsKey("Referrer-Policy")) h["Referrer-Policy"] = "no-referrer";
        if (!h.ContainsKey("Content-Security-Policy")) h["Content-Security-Policy"] = "frame-ancestors 'self'";
        if (!h.ContainsKey("Permissions-Policy")) h["Permissions-Policy"] = "geolocation=(), camera=(), microphone=()";
        return Task.CompletedTask;
    });
    await next();
});
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/d"))
    {
        ctx.Response.Headers["X-Robots-Tag"] = "noindex, nofollow, noarchive";
    }
    await next();
});
app.UseResponseCompression();
app.UseStaticFiles();
app.UseRouting();
app.UseIpRateLimiting();
app.UseStatusCodePagesWithReExecute("/Home/Error");
app.UseAuthentication();
app.UseAuthorization();
var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
SQLCrud.Configure(builder.Configuration, httpContextAccessor);
Logging.Configure(httpContextAccessor);
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();