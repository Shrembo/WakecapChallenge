using WakeCap.Application;
using WakeCap.Infrastructure;
using WakeCap.Persistence.EfCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddOpenApi();

WakeCapInfrastructureModule.Register(builder.Services, builder.Configuration);
WakeCapPersistenceModule.Register(builder.Services, builder.Configuration);
WakeCapApplicationModule.Register(builder.Services, builder.Configuration);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
