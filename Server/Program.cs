using Autofac;
using Microsoft.EntityFrameworkCore;
using Refit;
using StackExchange.Redis;
using Sunny.Framework.External.Client;
using Sunny.Framework.Web;
using WishServer;
using WishServer.AutoMapper;
using WishServer.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWebConfigure();
builder.Services.AddWebConfigure(builder.Configuration);

builder.Services.Configure<AppSetting>(builder.Configuration.Bind);

builder.Host.ConfigureContainer<ContainerBuilder>(b =>
{
    b.RegisterModule<AppRegister>();
});

AppDbContext.InitMap();

builder.Services.AddAutoMapper(typeof(MapperProfile));

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseMySQL(builder.Configuration.Get<AppSetting>()?.Data.MySQL.Url ?? string.Empty));

builder.Services.ConfigureDbContext<AppDbContext>(opt => opt.EnableSensitiveDataLogging());

builder.Services.AddSingleton<IConnectionMultiplexer>(c => ConnectionMultiplexer.Connect(builder.Configuration.Get<AppSetting>()?.Data.Redis.Url ?? string.Empty));

builder.Services.AddSingleton(c => c.GetService<IConnectionMultiplexer>().GetDatabase());

builder.Services.AddRefitClient<IDYOAuthClient>().ConfigureHttpClient(c => c.BaseAddress = new Uri("https://developer.toutiao.com/api"));

builder.Services.AddRefitClient<IDYClient>().ConfigureHttpClient(c => c.BaseAddress = new Uri("https://webcast.bytedance.com/api"));

builder.Services.AddRefitClient<IKSClient>().ConfigureHttpClient(c => c.BaseAddress = new Uri("https://open.kuaishou.com"));

var app = builder.Build();

app.MapControllers();

app.UseWebSockets();

app.UseWebHealthCheck();

app.Run();
