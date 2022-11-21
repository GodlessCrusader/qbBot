using Discord;
using Discord.Interactions;
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
                new OnClickMethod("goto", GoToAsync),
                new OnClickMethod("repeat", RepeatAsync),
                new OnClickMethod("playlist", SelectPlaylistAsync),
                new OnClickMethod("shuffle", ShuffleAsync)
            };
        }
        
        public async Task HandleMusicBoxComponentAsync(SocketMessageComponent component)
        {
            
            if (!_onClickMethods.Exists(x => x.Name == component.Data.CustomId))
            {
                return;
            }

            var player = _audioService.GetPlayer<ListedLavalinkPlayer>((ulong)component.GuildId);

            if (player == null)
                return;

            await _onClickMethods.Where(x => x.Name == component.Data.CustomId).Single().ExecuteAsync(player, component);
            
            if (player.State == PlayerState.Destroyed)
            {
                player.Dispose();
            }
            
            await component.DeferAsync(); 
        }

        public async Task HandleAddPlaylistModal(SocketModal socketModal)
        {
            var title = socketModal.Data.Components.First(x => x.CustomId == "playlist-name").Value;
            var url = socketModal.Data.Components.First(x => x.CustomId == "playlist-url").Value;
            if (socketModal.GuildId == null)
                return;
            var player = _audioService.GetPlayer<ListedLavalinkPlayer>((ulong)socketModal.GuildId);
            player.AddPlaylistAsync(title, url, _audioService);
            
            await socketModal.DeferAsync();
        }

        private async Task SelectPlaylistAsync(ListedLavalinkPlayer player, SocketMessageComponent component)
        {
            if (component.Data.Values.First() == "add-playlist")
            {
                await AddPlaylistAsync(player, component);
                return;
            }

            await player.ChangePlaylistAsync(component.Data.Values.First(), _audioService);
        }

        private async Task AddPlaylistAsync(ListedLavalinkPlayer player, SocketMessageComponent component)
        {
            var formBuilder = new ModalBuilder();
            formBuilder
                .AddTextInput("Playlist name:", "playlist-name")
                .AddTextInput("Playlist url:", "playlist-url")
                .WithTitle("Add playlist")
                .WithCustomId("add-playlist-modal");
            await component.RespondWithModalAsync(formBuilder.Build());
            
        }

        private async Task PlayPauseAsync(ListedLavalinkPlayer player, SocketMessageComponent component)
        {
            if (player.State == PlayerState.Paused)
               await player.ResumeAsync();
            else if (player.State == PlayerState.Playing)
                await player.PauseAsync();
        }

        private async Task RepeatAsync(ListedLavalinkPlayer player, SocketMessageComponent component)
        {
            if(!player.IsLooping)
                player.IsLooping = true;
            else
                player.IsLooping = false;
        }

        private async Task NextTrackAsync(ListedLavalinkPlayer player, SocketMessageComponent component)
        {
            await player.SkipAsync();
        }

        private async Task ExitAsync(ListedLavalinkPlayer player, SocketMessageComponent component)
        {
            await player.ExitAsync();
        }

        private async Task GoToAsync(ListedLavalinkPlayer player, SocketMessageComponent component)
        {
            int index;
            if (!int.TryParse(component.Data.Values.First(),  out index) || index > player.List.Count)
            {
                return;
            }
            else
            {
                await player.GotoAsync(index);
            }
        }

        private async Task PreviousTrackAsync(ListedLavalinkPlayer player, SocketMessageComponent component)
        {
            await player.PreviousAsync();
        }

        private async Task ShuffleAsync(ListedLavalinkPlayer player, SocketMessageComponent component)
        {
            await player.ShuffleAsync();
        }
    }
     
    internal delegate Task Execute(ListedLavalinkPlayer player, SocketMessageComponent component);
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
