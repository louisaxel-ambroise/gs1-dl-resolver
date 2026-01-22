using Gs1DigitalLink.Api.Contracts;
using Gs1DigitalLink.Api.Formatters.Html;
using Gs1DigitalLink.Api.Services;
using Gs1DigitalLink.Core;
using Gs1DigitalLink.Core.Services.Resolution;
using Microsoft.AspNetCore.Mvc.Razor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDigitalLinkCore();
builder.Services.AddScoped<ILanguageContext, HttpLanguageContext>();
builder.Services.AddScoped<IEventDispatcher, HttpContextEventDispatcher>();
builder.Services.AddHostedService<InsightConsumer>();
builder.Services.AddAuthentication();
builder.Services.AddRouting();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<GS1ResolverOptions>(builder.Configuration.GetSection(GS1ResolverOptions.Key));
builder.Services.AddLocalization(options => options.ResourcesPath = "Formatters/Html/Views/Resources");
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Add("/Formatters/Html/Views/Shared/{1}/{0}.cshtml");
});
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.AddSupportedUICultures("en", "fr", "nl", "de");
    options.SetDefaultCulture("en");
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyOrigin().WithMethods("GET", "HEAD", "OPTIONS"));
    options.AddPolicy("DataManagement", policy => policy.AllowAnyHeader().AllowAnyOrigin().WithMethods("GET", "POST", "DELETE", "HEAD", "OPTIONS"));
});
builder.Services.AddControllersWithViews(options =>
{
    options.OutputFormatters.Add(new HtmlViewFormatter());
    options.RespectBrowserAcceptHeader = true;
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.Use(async (req, next) =>
{
    var methods = new[] { HttpMethod.Post.Method, HttpMethod.Put.Method, HttpMethod.Patch.Method, HttpMethod.Delete.Method };
    await next();

    if(methods.Contains(req.Request.Method))
    {
        var context = req.RequestServices.GetRequiredService<DigitalLinkContext>();
        context.SaveChanges();
    }
});
app.UseRequestLocalization();
app.UseExceptionHandler("/error");
app.MapControllers();
app.Run();
