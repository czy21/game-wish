using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Refit;
using StackExchange.Redis;
using System.Text.Json.Serialization;
using WishServer;
using WishServer.Client;
using WishServer.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ConfigProperties>(builder.Configuration.Bind);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(b =>
{
    b.RegisterModule<ServiceRegister>();
});

builder.Services.AddDbContext<DbMasterContext>(opt => opt.UseMySQL(builder.Configuration.Get<ConfigProperties>()?.Data.MySQL.Url ?? string.Empty));

builder.Logging.AddSimpleConsole(options =>
{
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    //options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddSingleton<IConnectionMultiplexer>(c => ConnectionMultiplexer.Connect(builder.Configuration.Get<ConfigProperties>()?.Data.Redis.Url ?? string.Empty));

builder.Services.AddSingleton(c => c.GetService<IConnectionMultiplexer>().GetDatabase());

builder.Services.AddRefitClient<DYOAuthClient>().ConfigureHttpClient(c => c.BaseAddress = new Uri("https://developer.toutiao.com/api"));

builder.Services.AddRefitClient<DYWebCastClient>().ConfigureHttpClient(c => c.BaseAddress = new Uri("https://webcast.bytedance.com/api"));

builder.Services.AddRefitClient<KSOAuthClient>().ConfigureHttpClient(c => c.BaseAddress = new Uri("https://open.kuaishou.com"));

var app = builder.Build();

app.MapControllers();

app.UseWebSockets();

app.Run();
