using Discord.Addons.Hosting;
using Discord.WebSocket;
using DiscordBot.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Discord.Addons.Hosting;
using qbBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Tracking;
using Lavalink4NET.Player;

public class Program
{
   // public static void Main(string[] args) => new Program().MainAsync();

    public static async Task Main()
    {

        var builder = new HostBuilder()
            .ConfigureAppConfiguration(x =>
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false)
                    .Build();

                x.AddConfiguration(configuration);
            })
            .ConfigureLogging(x =>
            {
                x.AddConsole();
                x.SetMinimumLevel(LogLevel.Debug);
            })
            .ConfigureDiscordHost((context, config) =>
            {
                config.SocketConfig = new DiscordSocketConfig()
                {
                    LogLevel = Discord.LogSeverity.Debug,
                    AlwaysDownloadUsers = true,
                    MessageCacheSize = 200
                };

                config.Token = context.Configuration["Token"];
            })
            .UseCommandService((context, config) =>
            {
                config.CaseSensitiveCommands = false;
                config.LogLevel = Discord.LogSeverity.Debug;
                config.DefaultRunMode = Discord.Commands.RunMode.Async;
            }
            )
            .ConfigureServices((context, services) =>
            {
                services.AddHostedService<CommandHandler>();
                services.AddSingleton<IAudioService, LavalinkNode>();
                services.AddSingleton(new LavalinkNodeOptions() 
                {
                    RestUri = context.Configuration["RestUri"],
                    WebSocketUri = context.Configuration["WebSocketUri"],
                    Password = context.Configuration["NodePass"],
                });
                services.AddSingleton<IDiscordClientWrapper, DiscordClientWrapper>();
                services.AddSingleton<InactivityTrackingOptions>();
                services.AddSingleton<InactivityTrackingService>();
                //services.AddSingleton<PlayerFactory<LavalinkPlayer>>();
                

            })
            .UseConsoleLifetime();

        var host = builder.Build();

        using (host)
        {
            var client = host.Services.GetRequiredService<DiscordSocketClient>();
            var audioService = host.Services.GetRequiredService<IAudioService>();
            client.Ready += () => audioService.InitializeAsync();
            await host.RunAsync();
        }
        Console.ReadLine();
    }
}


