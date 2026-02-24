using Chat.MessagingService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("gateway", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5046"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<MessagingDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("MessagingDb"));
});

var app = builder.Build();


app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("gateway");

app.MapControllers();

app.Run();
