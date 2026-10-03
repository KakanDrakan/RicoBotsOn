using Application.Sessions;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Session;

namespace RicoBotsOn
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });

            builder.Services.AddEndpointsApiExplorer();  
            builder.Services.AddSwaggerGen();

            builder.Services.AddSingleton<Application.Interfaces.ISessionStore, SessionStore>();
            builder.Services.AddScoped<SessionService>();

            var app = builder.Build();

            var sessionStore = app.Services.GetRequiredService<Application.Interfaces.ISessionStore>();
            sessionStore.Save(new LobbySession("test", MatchFactory.CreateDefault(new Random())));

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(); 
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseCors();

            app.MapControllers();

            app.Run();
        }
    }
}
