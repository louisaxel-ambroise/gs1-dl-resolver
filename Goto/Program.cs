using Goto;
using Goto.Infrastructure.Authentication;
using Goto.Services.Data;
using Goto.Services.Data.Entities;
using Microsoft.Extensions.Options;

var builder = WebApplication
    .CreateBuilder(args)
    .RegisterApiServices()
    .RegisterDigitalLinkToolkit()
    .RegisterDigitalLinkResolver();

var app = builder.Build();

if (builder.Environment.IsDevelopment() || args.Contains("--bootstrap"))
{
    using var scope = app.Services.CreateScope();
    using var context = scope.ServiceProvider.GetRequiredService<Context>();
    context.Database.EnsureCreated();
    var bootstrapResult = context.SeedApiKeys(builder.Configuration.GetSection("ApiKeys").Get<ApiKeyDefinition>());

    foreach(var result in bootstrapResult)
    {
        Console.WriteLine(result);
    }
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapStaticAssets();
app.UseRequestLocalization();
app.UseExceptionHandler("/error");
app.MapControllers();

app.Run();