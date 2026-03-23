using FluentMigrator.Runner;
using Gs1DigitalLink.Web.Formatters.Html;
using Gs1DigitalLink.Core;
using Gs1DigitalLink.Core.Services.Registration;
using Gs1DigitalLink.Core.Services.Resolution;
using Gs1DigitalLink.Web;
using Gs1DigitalLink.Web.Contracts;
using Gs1DigitalLink.Web.Services;
using Gs1DigitalLink.Web.Services.Migrations;
using Microsoft.AspNetCore.Mvc.Razor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDigitalLinkCore();
builder.Services.AddScoped<IUserContext, HttpHeaderUserContext>();
builder.Services.AddScoped<ILanguageContext, HttpLanguageContext>();
builder.Services.AddScoped<IEventDispatcher, HttpContextEventDispatcher>();
builder.Services.AddAuthentication();
builder.Services.AddRouting();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHostedService<InsightConsumer>();
builder.Services.Configure<GS1ResolverOptions>(builder.Configuration.GetSection(GS1ResolverOptions.Key));
builder.Services.AddLocalization(options => options.ResourcesPath = "Formatters/Html/Views/Resources");
builder.Services.Configure<RazorViewEngineOptions>(options => options.ViewLocationFormats.Add("/Formatters/Html/Views/Shared/{1}/{0}.cshtml"));
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
builder.Services
    .AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddSQLite()
        .WithGlobalConnectionString("Data Source=registry.db")
        .ScanIn(typeof(M20260114183700_CreatePrefixTable).Assembly).For.All());

var app = builder.Build();

// Migrate database - Remove if not needed
MigrateDatabase(app.Services);

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.UseUnitOfWork();
app.UseRequestLocalization();
app.UseExceptionHandler("/error");
app.MapControllers();
app.Run();

static void MigrateDatabase(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}