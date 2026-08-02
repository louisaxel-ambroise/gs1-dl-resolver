using Goto;
using Goto.Services.Data;

var builder = WebApplication
    .CreateBuilder(args)
    .RegisterApiServices()
    .RegisterDigitalLinkToolkit()
    .RegisterDigitalLinkResolver();

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
