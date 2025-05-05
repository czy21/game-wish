using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Nacos.V2.DependencyInjection;
using NLog;
using NLog.Web;
using Refit;
using StackExchange.Redis;
using Sunny.Framework.External.Client;
using Sunny.Framework.Web;
using WishServer;
using WishServer.Repository;

LogManager.Setup().SetupExtensions(o =>
{
    //o.RegisterLayoutRenderer<ColoredLevelLayoutRenderer>("level");
});

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWebConfigure();
builder.Services.AddWebConfigure(builder.Configuration);

builder.Services.Configure<AppSetting>(builder.Configuration.Bind);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(b =>
{
    b.RegisterModule<AppRegister>();
});

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseMySQL(builder.Configuration.Get<AppSetting>()?.Data.MySQL.Url ?? string.Empty));

builder.Services.ConfigureDbContext<AppDbContext>(opt => opt.EnableSensitiveDataLogging());

builder.Services.AddControllers().AddJsonOptions(options =>
{
    //options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddSingleton<IConnectionMultiplexer>(c => ConnectionMultiplexer.Connect(builder.Configuration.Get<AppSetting>()?.Data.Redis.Url ?? string.Empty));

builder.Services.AddSingleton(c => c.GetService<IConnectionMultiplexer>().GetDatabase());

builder.Services.AddRefitClient<IDYOAuthClient>().ConfigureHttpClient(c => c.BaseAddress = new Uri("https://developer.toutiao.com/api"));

builder.Services.AddRefitClient<IDYClient>().ConfigureHttpClient(c => c.BaseAddress = new Uri("https://webcast.bytedance.com/api"));

builder.Services.AddRefitClient<IKSClient>().ConfigureHttpClient(c => c.BaseAddress = new Uri("https://open.kuaishou.com"));

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapControllers();

app.UseWebSockets();

app.MapHealthChecks("/actuator/health");

app.Run();
