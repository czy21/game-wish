using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using Refit;
using StackExchange.Redis;
using WishServer;
using WishServer.Client;
using WishServer.Repository;

LogManager.Setup().SetupExtensions(o =>
{
    //o.RegisterLayoutRenderer<ColoredLevelLayoutRenderer>("level");
});

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Host.UseNLog();

builder.Services.Configure<ConfigProperties>(builder.Configuration.Bind);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(b =>
{
    b.RegisterModule<ComponentRegister>();
});

builder.Services.AddDbContext<DbMasterContext>(opt => opt.UseMySQL(builder.Configuration.Get<ConfigProperties>()?.Data.MySQL.Url ?? string.Empty));

builder.Services.ConfigureDbContext<DbMasterContext>(opt => opt.EnableSensitiveDataLogging());

builder.Services.AddControllers().AddJsonOptions(options =>
{
    //options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddSingleton<IConnectionMultiplexer>(c => ConnectionMultiplexer.Connect(builder.Configuration.Get<ConfigProperties>()?.Data.Redis.Url ?? string.Empty));

builder.Services.AddSingleton(c => c.GetService<IConnectionMultiplexer>().GetDatabase());

builder.Services.AddRefitClient<DYOAuthClient>().ConfigureHttpClient(c => c.BaseAddress = new Uri("https://developer.toutiao.com/api"));

builder.Services.AddRefitClient<DYWebCastClient>().ConfigureHttpClient(c => c.BaseAddress = new Uri("https://webcast.bytedance.com/api"));

builder.Services.AddRefitClient<KSClient>().ConfigureHttpClient(c => c.BaseAddress = new Uri("https://open.kuaishou.com"));

var app = builder.Build();

app.MapControllers();

app.UseWebSockets();

app.Run();
