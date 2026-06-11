using Goto.Data;
using Goto.Data.Entities;
using Goto.Infrastructure;
using Goto.Infrastructure.Binding;
using Goto.Infrastructure.Converters;
using Goto.Services;
using Goto.Services.Conversion;
using Goto.Services.Conversion.Utils;
using Goto.Services.Conversion.Utils.Validation;
using Goto.Translations;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);

CompanyPrefix.Initialize("wwwroot/gcpprefixformatlist.xml");
ApplicationIdentifiers.Initialize("wwwroot/ApplicationIdentifiers.json", new() { PropertyNameCaseInsensitive = true });

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
builder.Services.AddScoped<ApiTimeProvider>();
builder.Services.AddDbContext<Context>();
builder.Services.AddHostedService<InsightConsumerService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapStaticAssets();
app.UseRequestLocalization();
app.UseExceptionHandler("/error");
app.MapControllers();

app.Run();
