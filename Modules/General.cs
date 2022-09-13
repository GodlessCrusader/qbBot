using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Player;
using Microsoft.Extensions.DependencyInjection;
using qbBot.Classes;
using qbBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace qbBot.Modules
{
    public class General : ModuleBase<SocketCommandContext>
    {
        private MusicBoxButtonClickHandler _musicBoxButtonClickHandler { get; set; }
        private IAudioService _audioService { get; set; } 
        private IDiscordClientWrapper _wrapper { get; set; }
        private ListedLavalinkPlayer _player;
        public General(IAudioService audioService, IDiscordClientWrapper discordClientWrapper, MusicBoxButtonClickHandler clickHandler)
        {
            _musicBoxButtonClickHandler = clickHandler;
            _wrapper = discordClientWrapper;
            _audioService = audioService;
            
        }
        
        [Command("ping")]
        public async Task PingAsync()
        {
            await ReplyAsync("Pong");
          
        }

        [Command("roll")]
        [Alias("r")]
        public async Task RollDiceAsync(string diceExpr)
        {
            Dice.Roller.DefaultConfig.MaxDice = 100;
            Dice.Roller.DefaultConfig.MaxSides = 1000;
            try
            {
                var result = Dice.Roller.Roll(diceExpr);
                await ReplyAsync(result.ToString());
                
            }
            catch(Exception e)
            {
                await ReplyAsync(e.Message);
            }
        }

        [Command("rickRoll")]
        [Alias("rr")]
        public async Task RickRollAsync()
        {
            var check = await ChannelJoinInitCheckAsync();
            if (!check)
                return;
            
            var channel = Context.Guild.VoiceChannels.First(x => x.ConnectedUsers.Contains(Context.User));
            var track = await _audioService.GetTrackAsync("https://www.youtube.com/watch?v=dQw4w9WgXcQ&ab_channel=RickAstley", Lavalink4NET.Rest.SearchMode.YouTube);
            _player = await _audioService.JoinAsync<ListedLavalinkPlayer>(Context.Guild.Id, channel.Id);
            await _player.PlayAsync(track, false);
        }
        
        [Command("musicBox", false)]
        [Alias("mb")]
        public async Task MusicBoxAsync(string tracksUrl)
        {
            var check = await ChannelJoinInitCheckAsync();
            if (!check)
                return;
           
            var tracks = await _audioService.GetTracksAsync(tracksUrl, Lavalink4NET.Rest.SearchMode.YouTube);
            var selectorBuilder = new SelectMenuBuilder();
            selectorBuilder
                .WithPlaceholder("Go to:")
                .AddOption("Add playlist ->","one")
                .WithCustomId("goto");

            foreach (var track in tracks)
            {
                selectorBuilder.AddOption(track.Title, track.TrackIdentifier);
            }
 
            var componentBuilder = new ComponentBuilder();
            componentBuilder
                .WithSelectMenu(selectorBuilder)
                .WithButton(
                    label: Emoji.Parse(":track_previous:").ToString(),
                    customId: "previous-track",
                    style : ButtonStyle.Success
                )
                .WithButton(
                    label: Emoji.Parse(":play_pause:").ToString(),
                    customId: "play-pause",
                    style: ButtonStyle.Success
                )
                .WithButton(
                    label: Emoji.Parse(":repeat:").ToString(),
                    customId: "repeat",
                    style: ButtonStyle.Success
                )
                .WithButton(
                    label: Emoji.Parse(":track_next:").ToString(),
                    customId: "next-track",
                    style: ButtonStyle.Success
                ).WithButton(
                    label: Emoji.Parse(":x:").ToString(),
                    customId: "exit",
                    style: ButtonStyle.Success
                );

            var channel = Context.Guild.VoiceChannels.First(x => x.ConnectedUsers.Contains(Context.User));
            _player = await _audioService.JoinAsync<ListedLavalinkPlayer>(Context.Guild.Id, channel.Id);
            Context.Client.ButtonExecuted += HandleMusicBoxComponentAsync;
            Context.Client.SelectMenuExecuted += HandleMusicBoxComponentAsync;
            _player.List.AddRange(tracks);
            await _player.SkipAsync();
            await ReplyAsync("s", components: componentBuilder.Build());

        }

        private async Task HandleMusicBoxComponentAsync(SocketMessageComponent component)
        {
            await _musicBoxButtonClickHandler.HandleAsync(component, _player);
            await component.DeferAsync();
        }

        private async Task<bool> ChannelJoinInitCheckAsync()
        {
            if (Context.Guild == null)
            {
                await ReplyAsync("Please type command in guild chat");
                return false;
            }

            if (Context.Guild.VoiceChannels.Where(x => x.ConnectedUsers.Contains(Context.User)) == null)
            {
                await ReplyAsync("Please join target channel");
                return false;
            }

            return true;
        }
    }
}
