using AspNetCoreRateLimit;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using MyWebSite.Business;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews().AddFluentValidation(fv => { fv.DisableDataAnnotationsValidation = true; fv.RegisterValidatorsFromAssemblyContaining<ContactValidator>(); });
builder.Services.AddControllersWithViews().AddFluentValidation(fv => { fv.DisableDataAnnotationsValidation = true; fv.RegisterValidatorsFromAssemblyContaining<AdminAboutValidator>(); });
builder.Services.AddControllersWithViews().AddFluentValidation(fv => { fv.DisableDataAnnotationsValidation = true; fv.RegisterValidatorsFromAssemblyContaining<AdminSocialMediaValidator>(); });
builder.Services.AddControllersWithViews().AddFluentValidation(fv => { fv.DisableDataAnnotationsValidation = true; fv.RegisterValidatorsFromAssemblyContaining<AdminLoginValidator>(); });
builder.Services.AddControllersWithViews().AddFluentValidation(fv => { fv.DisableDataAnnotationsValidation = true; fv.RegisterValidatorsFromAssemblyContaining<AdminMailValidator>(); });
builder.Services.AddControllersWithViews().AddFluentValidation(fv => { fv.DisableDataAnnotationsValidation = true; fv.RegisterValidatorsFromAssemblyContaining<AdminEducationValidator>(); });
builder.Services.AddControllersWithViews().AddFluentValidation(fv => { fv.DisableDataAnnotationsValidation = true; fv.RegisterValidatorsFromAssemblyContaining<AdminJobsValidator>(); });
builder.Services.AddControllersWithViews().AddFluentValidation(fv => { fv.DisableDataAnnotationsValidation = true; fv.RegisterValidatorsFromAssemblyContaining<AdminSkillsValidator>(); });
builder.Services.AddControllersWithViews().AddFluentValidation(fv => { fv.DisableDataAnnotationsValidation = true; fv.RegisterValidatorsFromAssemblyContaining<AdminProjectsValidator>(); });
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/AdminGiris/Panel";
        options.AccessDeniedPath = "/AdminGiris/Panel";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Limit = 30,
            Period = "1m"
        }
    };
});
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddAuthorization();
var app = builder.Build();
app.UseIpRateLimiting();
app.MapControllers();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/Home/Error");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();