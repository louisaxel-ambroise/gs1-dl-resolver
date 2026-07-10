using DigitalLinkToolkit.Conversion;
using DigitalLinkToolkit.Conversion.Validation;
using DigitalLinkToolkit.Translation;
using Goto.Infrastructure.Authentication;
using Goto.Infrastructure.Results.Converters;
using Goto.Infrastructure.Routing.Binding;
using Goto.Infrastructure.Views;
using Goto.Services;
using Goto.Services.Data;
using Goto.Services.Data.Entities;
using Goto.Services.Translations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Sqids;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);
var tdtEngineBuilder = new TdtEngineBuilder();

tdtEngineBuilder = Directory.GetFiles("wwwroot/DefinitionFiles").Aggregate(tdtEngineBuilder, (builder, file) => builder.AddDefinitionFile(file));
tdtEngineBuilder = Directory.GetFiles("wwwroot/Tables").Aggregate(tdtEngineBuilder, (builder, file) => builder.AddTableFile(file));

CompanyPrefix.Initialize("wwwroot/gcpprefixformatlist.xml");
OptimizationCodes.Initialize("wwwroot/OptimizationCodes.json", new() { PropertyNameCaseInsensitive = true });
ApplicationIdentifiers.Initialize("wwwroot/ApplicationIdentifiers.json", new() { PropertyNameCaseInsensitive = true });

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new GS1ResolverModelBinderProvider());
    options.OutputFormatters.Add(new HtmlViewFormatter());
    options.RespectBrowserAcceptHeader = true;
}).AddJsonOptions(opt => opt.JsonSerializerOptions.Converters.Insert(0, new LinksetResultConverter()));
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Clear();
    options.ViewLocationFormats.Add("/Infrastructure/Views/{1}/{0}" + RazorViewEngine.ViewExtension);
});
builder.Services.Configure<RequestLocalizationOptions>(opt => opt.AddSupportedUICultures("en", "fr", "nl", "de", "ar"));
builder.Services.AddAuthentication("ApiKey").AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationSchemeHandler>("ApiKey", opts => opts.ApiKey = builder.Configuration.GetValue<string>("ApiKey"));
builder.Services.AddCors(opt => opt.AddDefaultPolicy(p => p.AllowAnyHeader().AllowAnyOrigin().WithMethods("GET", "HEAD", "OPTIONS")));
builder.Services.AddLocalization();
builder.Services.AddDbContext<Context>(opt =>
{
    opt.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
    opt.ConfigureWarnings(warnings =>
    {
        warnings.Throw(RelationalEventId.MultipleCollectionIncludeWarning);
        warnings.Throw(CoreEventId.FirstWithoutOrderByAndFilterWarning);
    });
});
builder.Services.AddScoped(ctx => ctx.GetRequiredService<IHttpContextAccessor>().HttpContext?.User ?? new());
builder.Services.AddScoped(ctx => ctx.GetRequiredService<IHttpContextAccessor>().HttpContext?.Features?.Get<IExceptionHandlerFeature>() ?? new ExceptionHandlerFeature());
builder.Services.AddScoped<Clock>();
builder.Services.AddSingleton(tdtEngineBuilder.BuildEngine());
builder.Services.AddSingleton(OptimizationCodes.Shared);
builder.Services.AddSingleton(ApplicationIdentifiers.Shared);
builder.Services.AddSingleton<CommonLocalizationService>();
builder.Services.AddSingleton<DigitalLinkConverter>();
builder.Services.AddSingleton<IdentifierConverter>();
builder.Services.AddSingleton(Channel.CreateBounded<Insight>(new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.DropOldest }));
builder.Services.AddSingleton(new SqidsEncoder<int>(new SqidsOptions { MinLength = 10, Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" }));
builder.Services.AddHostedService<InsightConsumerService>();

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    using var context = scope.ServiceProvider.GetRequiredService<Context>();
    context.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapStaticAssets();
app.UseRequestLocalization();
app.UseExceptionHandler("/error");
app.MapControllers();

app.Run();
