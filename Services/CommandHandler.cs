using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Commands;
using Discord.WebSocket;
using Lavalink4NET;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace qbBot.Services
{
    public class CommandHandler : DiscordClientService
    {
        private readonly IServiceProvider _provider;
        private readonly CommandService _commandService;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly IAudioService _audioService;
        private readonly IDiscordClientWrapper _discordClientWrapper;

        public CommandHandler(IDiscordClientWrapper discordClientWrapper, DiscordSocketClient client, ILogger<DiscordClientService> logger, IServiceProvider provider, CommandService commandService, IConfiguration config, IAudioService audioService) : base(client, logger)
        {
            _discordClientWrapper = discordClientWrapper;
            _provider = provider;
            _commandService = commandService;
            _config = config;
            _audioService = audioService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Client.MessageReceived += HandleMessageAsync;
           
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task HandleMessageAsync(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage mes) || arg.Source == Discord.MessageSource.Bot)
            {
                return;
            }
            var argPos = 0;

            if (!mes.HasStringPrefix("!", ref argPos))
            {
                return;
            }
            var context = new SocketCommandContext(Client, mes);
            var cts = new CancellationTokenSource();
            await Client.WaitForReadyAsync(cts.Token);
            await _audioService.InitializeAsync();
            await _discordClientWrapper.InitializeAsync();
            await _commandService.ExecuteAsync(context, argPos, _provider);
            
        }



    }



    
}
