using Autofac;
using Autofac.Extensions.DependencyInjection;
using Refit;
using StackExchange.Redis;
using System.Text.Json.Serialization;
using WishServer;
using WishServer.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ConfigProperties>(builder.Configuration.Bind);

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

builder.Services.AddSingleton<IConnectionMultiplexer>(c => ConnectionMultiplexer.Connect(builder.Configuration.Get<ConfigProperties>()?.Data.Redis.Url));

builder.Services.AddSingleton(c => c.GetService<IConnectionMultiplexer>().GetDatabase());

builder.Services.AddRefitClient<DYAccessTokenClient>().ConfigureHttpClient(c => c.BaseAddress = new Uri("https://developer.toutiao.com/api"));

builder.Services.AddRefitClient<DYWebCastClient>().ConfigureHttpClient(c => c.BaseAddress = new Uri("https://webcast.bytedance.com/api"));

var app = builder.Build();

app.MapControllers();

app.UseWebSockets();

app.Run();
