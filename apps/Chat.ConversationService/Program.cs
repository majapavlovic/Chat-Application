using Chat.ConversationService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("gateway", policy =>
    {
        policy
            .WithOrigins("http://localhost:5046")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<ConversationDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("ConversationDb"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseCors("gateway");

app.MapControllers();

app.Run();
