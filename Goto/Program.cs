using Goto.Data;
using Goto.Data.Entities;
using Goto.Infrastructure;
using Goto.Infrastructure.Authentication;
using Goto.Infrastructure.Results.Converters;
using Goto.Infrastructure.Routing.Binding;
using Goto.Services;
using Goto.Translations;
using DigitalLinkToolkit.Translation;
using Microsoft.EntityFrameworkCore;
using Sqids;
using System.Threading.Channels;
using DigitalLinkToolkit.Conversion;
using DigitalLinkToolkit.Conversion.Validation;

var builder = WebApplication.CreateBuilder(args);

var tdtEngineBuilder = new TdtEngineBuilder();

tdtEngineBuilder = Directory.GetFiles("wwwroot/DefinitionFiles").Aggregate(tdtEngineBuilder, (builder, file) => builder.AddDefinitionFile(file));
tdtEngineBuilder = Directory.GetFiles("wwwroot/Tables").Aggregate(tdtEngineBuilder, (builder, file) => builder.AddTableFile(file));


CompanyPrefix.Initialize("wwwroot/gcpprefixformatlist.xml");
ApplicationIdentifiers.Initialize("wwwroot/ApplicationIdentifiers.json", new() { PropertyNameCaseInsensitive = true });

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new GS1ResolverModelBinderProvider());
    options.OutputFormatters.Add(new HtmlViewFormatter());
    options.RespectBrowserAcceptHeader = true;
}).AddJsonOptions(opt => opt.JsonSerializerOptions.Converters.Insert(0, new LinksetResultConverter()));
builder.Services.Configure<RequestLocalizationOptions>(opt => opt.AddSupportedUICultures("en", "fr", "nl", "de", "ar"));
builder.Services.AddCors(opt => opt.AddDefaultPolicy(p => p.AllowAnyHeader().AllowAnyOrigin().WithMethods("GET", "HEAD", "OPTIONS")));
builder.Services.AddLocalization();
builder.Services.AddSingleton(ApplicationIdentifiers.Shared);
builder.Services.AddSingleton(Channel.CreateBounded<Insight>(new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.DropOldest }));
builder.Services.AddSingleton<CommonLocalizationService>();
builder.Services.AddSingleton<DigitalLinkConverter>();
builder.Services.AddSingleton<IdentifierConverter>();
builder.Services.AddScoped<Clock>();
builder.Services.AddDbContext<Context>(opt => opt.UseSqlite($"Data Source=registry.db"));
builder.Services.AddHostedService<InsightConsumerService>();
builder.Services.AddAuthentication("ApiKey").AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationSchemeHandler>("ApiKey", opts => opts.ApiKey = builder.Configuration.GetValue<string>("ApiKey"));
builder.Services.AddScoped(ctx => ctx.GetRequiredService<IHttpContextAccessor>().HttpContext?.User ?? new());
builder.Services.AddSingleton(new SqidsEncoder<int>(new SqidsOptions { MinLength = 10, Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" }));
builder.Services.AddSingleton(tdtEngineBuilder.BuildEngine());

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapStaticAssets();
app.UseRequestLocalization();
app.UseExceptionHandler("/error");
app.MapControllers();

app.Run();
