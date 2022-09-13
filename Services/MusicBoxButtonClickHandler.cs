using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qbBot.Services
{
    public class MusicBoxButtonClickHandler
    {
        private IAudioService _audioService { get; set; }
        private IDiscordClientWrapper _wrapper { get; set; }
        
        private List<LavalinkTrack>? _tracks { get; set; }
        private List<OnClickMethod> _onClickMethods { get; set; }

        public MusicBoxButtonClickHandler(IAudioService audioService, IDiscordClientWrapper discordClientWrapper)
        {
            _wrapper = discordClientWrapper;
            _audioService = audioService;
            _onClickMethods = new List<OnClickMethod>() 
            {
                new OnClickMethod("play-pause", PlayPauseAsync),
                new OnClickMethod("next-track", NextTrackAsync),
                new OnClickMethod("previous-track", PreviousTrackAsync),
                new OnClickMethod("exit", ExitAsync),
                new OnClickMethod("goto", GoToAsync)
            };
            
        }
        
        public async Task HandleAsync(SocketMessageComponent component, QueuedLavalinkPlayer player)
        {
            
            if(!_onClickMethods.Exists(x => x.Name == component.Data.CustomId))
            {
                throw new ArgumentException("Such command doesn't exist");
            }

            _tracks = player.Queue.ToList();

            await _onClickMethods.Where(x => x.Name == component.Data.CustomId).Single().ExecuteAsync(player);
        }

        private async Task PlayPauseAsync(QueuedLavalinkPlayer player)
        {
            if (player.State == PlayerState.Paused)
               await player.ResumeAsync();
            else if (player.State == PlayerState.Playing)
                await player.PauseAsync();
        }

        private async Task RepeatAsync(QueuedLavalinkPlayer player)
        {

        }
        private async Task NextTrackAsync(QueuedLavalinkPlayer player)
        {
            await player.SkipAsync();
        }

        private async Task ExitAsync(QueuedLavalinkPlayer player)
        {
            await player.StopAsync(true);
            await player.DisposeAsync();
        }

        private async Task GoToAsync(QueuedLavalinkPlayer player)
        {
           
        }
        private async Task PreviousTrackAsync(QueuedLavalinkPlayer player)
        {
           // await player.Queue.
        }
    }
     
    internal delegate Task Execute(QueuedLavalinkPlayer player);
    internal class OnClickMethod
    {
        public OnClickMethod(string name, Execute execute)
        {
            Name = name;
            ExecuteAsync = execute;
        }

        public string Name { get; set; }
        public Execute ExecuteAsync { get; set; }
    }
}
