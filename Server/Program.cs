using System.Text.Encodings.Web;
using System.Text.Json;
using Autofac;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Refit;
using StackExchange.Redis;
using Sunny.Framework.External.Client;
using Sunny.Framework.Web;
using Sunny.Framework.Web.Middleware;
using WishServer;
using WishServer.AutoMapper;
using WishServer.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWebConfigure();

builder.Services.AddWebConfigure(builder.Configuration);

builder.Services.Configure<AppSetting>(builder.Configuration.Bind);

builder.Host.ConfigureContainer<ContainerBuilder>(b => b.RegisterModule<AppRegister>());

DefaultTypeMap.MatchNamesWithUnderscores = true;

builder.Services.AddAutoMapper(typeof(MapperProfile));

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseMySQL(builder.Configuration.Get<AppSetting>().Data.MySQL.Url));

builder.Services.ConfigureDbContext<AppDbContext>(opt => opt.EnableSensitiveDataLogging());

builder.Services.AddSingleton<IConnectionMultiplexer>(c => ConnectionMultiplexer.Connect(builder.Configuration.Get<AppSetting>().Data.Redis.Url));

builder.Services.AddSingleton(c => c.GetService<IConnectionMultiplexer>().GetDatabase());

builder.Services.AddScoped<RefitLogHandler>();

builder.Services.AddRefitClient<IDYOAuthClient>(p => p.GetRequiredService<RefitSettings>())
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://developer.toutiao.com/api"))
    .AddHttpMessageHandler<RefitLogHandler>();

builder.Services.AddRefitClient<IDYClient>(p => p.GetRequiredService<RefitSettings>())
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://webcast.bytedance.com/api"))
    .AddHttpMessageHandler<RefitLogHandler>();

builder.Services.AddRefitClient<IKSClient>(p => p.GetRequiredService<RefitSettings>())
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://open.kuaishou.com"))
    .AddHttpMessageHandler<RefitLogHandler>();

var app = builder.Build();

app.UseMiddleware<HttpLogMiddleware>();

app.MapControllers();

app.UseWebSockets();

app.UseWebHealthCheck();

app.Run();