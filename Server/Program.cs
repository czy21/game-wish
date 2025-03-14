using Autofac;
using Autofac.Extensions.DependencyInjection;
using Refit;
using System.Text.Json.Serialization;
using WishServer;
using WishServer.Client;

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

builder.Services.AddRefitClient<DYAccessTokenClient>().ConfigureHttpClient(c => c.BaseAddress = new Uri("https://developer.toutiao.com/api"));

builder.Services.AddRefitClient<DYWebCastClient>().ConfigureHttpClient(c => c.BaseAddress = new Uri("https://webcast.bytedance.com/api"));

var app = builder.Build();

app.MapControllers();

app.UseWebSockets();

app.Run();
