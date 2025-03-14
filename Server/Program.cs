using Autofac;
using Autofac.Extensions.DependencyInjection;
using System.Text.Json.Serialization;
using WishServer;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(b =>
{
    b.RegisterModule<MessageRegister>();
});

builder.Logging.AddSimpleConsole(options =>
{
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
});

builder.Services.AddControllers().AddJsonOptions(options =>
{

    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

});

var app = builder.Build();

app.MapControllers();

app.UseWebSockets();

app.Run();
