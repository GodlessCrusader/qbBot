using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Player;
using qbBot.Classes;
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
        
        public async Task HandleAsync(SocketMessageComponent component, ListedLavalinkPlayer player)
        {
            
            if(!_onClickMethods.Exists(x => x.Name == component.Data.CustomId))
            {
                throw new ArgumentException("Such command doesn't exist");
            }

            

            await _onClickMethods.Where(x => x.Name == component.Data.CustomId).Single().ExecuteAsync(player);
        }

        private async Task PlayPauseAsync(ListedLavalinkPlayer player)
        {
            if (player.State == PlayerState.Paused)
               await player.ResumeAsync();
            else if (player.State == PlayerState.Playing)
                await player.PauseAsync();
        }

        private async Task RepeatAsync(ListedLavalinkPlayer player)
        {

        }
        private async Task NextTrackAsync(ListedLavalinkPlayer player)
        {
           
            await player.SkipAsync();
        }

        private async Task ExitAsync(ListedLavalinkPlayer player)
        {
            await player.StopAsync(true);
            await player.DisposeAsync();
        }

        private async Task GoToAsync(ListedLavalinkPlayer player)
        {
           
        }

        private async Task PreviousTrackAsync(ListedLavalinkPlayer player)
        {
            await player.PreviousAsync();
        }
    }
     
    internal delegate Task Execute(ListedLavalinkPlayer player);
    internal class OnClickMethod
    {
        internal OnClickMethod(string name, Execute execute)
        {
            Name = name;
            ExecuteAsync = execute;
        }

        public string Name { get; set; }
        public Execute ExecuteAsync { get; set; }
    }
}
