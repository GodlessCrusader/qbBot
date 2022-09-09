using Discord.Commands;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Player;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qbBot.Modules
{
    public class General : ModuleBase<SocketCommandContext>
    {
        private IAudioService _audioService { get; set; } 
        private PlayerFactory<LavalinkPlayer> _audioPlayerFactory { get; set; }
        private IDiscordClientWrapper _wrapper { get; set; }
        private LavalinkPlayer _player;
        public General(IAudioService audioService,  IDiscordClientWrapper discordClientWrapper)
        {
            _wrapper = discordClientWrapper;
            _audioService = audioService;
            _player = new LavalinkPlayer();
            _audioPlayerFactory = new PlayerFactory<LavalinkPlayer>(() => { return new LavalinkPlayer() { }; });
            _player = _audioPlayerFactory.Invoke();
            
        }
        
        [Command("ping")]
        public async Task PingAsync()
        {
            await ReplyAsync("Pong");
          
        }

        [Command("rickRoll")]
        [Alias("rr")]
        public async Task MusicBoxAsync()
        {
            
            var channel = Context.Guild.VoiceChannels.First(x => x.ConnectedUsers.Contains(Context.User));
            var track = await _audioService.GetTrackAsync("https://www.youtube.com/watch?v=dQw4w9WgXcQ&ab_channel=RickAstley", Lavalink4NET.Rest.SearchMode.YouTube);
            var player = await _audioService.JoinAsync<LavalinkPlayer>(Context.Guild.Id, channel.Id);
            await player.PlayAsync(track);
            //   Context.Client.Ready += 
        }
    }
}
