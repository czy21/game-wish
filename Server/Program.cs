using System.Text.Json.Serialization;
using WishServer.Manager;
using WishServer.Util;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{

    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;

});

builder.Services.AddScoped<IMessageHandler,RoomManager>();
builder.Services.AddScoped<IMessageHandler, RankManager>();

var app = builder.Build();

app.MapControllers();

app.UseWebSockets();

app.Run();
