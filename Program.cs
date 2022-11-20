using Discord.Addons.Hosting;
using Discord.WebSocket;
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
using System.Diagnostics;
using System.Reflection;
using Discord.Addons.Hosting.Util;

public class Program
{
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
                    MessageCacheSize = 200,
                    GatewayIntents = Discord.GatewayIntents.AllUnprivileged | Discord.GatewayIntents.MessageContent
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
                services.AddSingleton<InterfaceMessageChangeHandler>();
                services.AddSingleton<MusicBoxButtonClickHandler>();
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
                                
            })
            .UseConsoleLifetime();

        var host = builder.Build();

  
        using (host)
        {
            
            var musicBoxComponentHandler = host.Services.GetRequiredService<MusicBoxButtonClickHandler>();
            var client = host.Services.GetRequiredService<DiscordSocketClient>();
            var audioService = host.Services.GetRequiredService<IAudioService>();
            client.Ready += audioService.InitializeAsync;  
            client.ButtonExecuted += musicBoxComponentHandler.HandleMusicBoxComponentAsync;
            client.SelectMenuExecuted += musicBoxComponentHandler.HandleMusicBoxComponentAsync;
            client.ModalSubmitted += musicBoxComponentHandler.HandleAddPlaylistModal;
            await host.RunAsync();
            
        }
      
    }
}


