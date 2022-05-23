using SMAServer.Config;
using SMAServer.Managers;
using SMAServer.Workers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Configuration.AddJsonFile("secrets.json");

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<PersistenceManager>();
builder.Services.AddHostedService<PersistenceWorker>();
builder.Services.AddHostedService<DiscordBotWorker>();

builder.Services.AddOptions();
builder.Services.Configure<SecretsConfig>(builder.Configuration.GetSection("Secrets"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
