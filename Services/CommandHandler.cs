using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord.Addons.Hosting;
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

        public CommandHandler(DiscordSocketClient client, ILogger<DiscordClientService> logger, IServiceProvider provider, CommandService commandService, IConfiguration config, IAudioService audioService) : base(client, logger)
        {
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
            if (!(arg is SocketUserMessage mes) || arg.Source == Discord.MessageSource.Bot) return;
            
            var argPos = 0;
            
            if (!mes.HasCharPrefix('!', ref argPos)) return;

            var context = new SocketCommandContext(Client, mes);

            await _commandService.ExecuteAsync(context, argPos, _provider);
            //Console.WriteLine($"Message content{arg.Content}");
            // var words = arg.Content.Split(' ');
            // commands.Find(x => x.Alias == words[0]).Handle(bot, arg);
        }



    }



    
}
