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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace qbBot.Modules
{
    public class General : ModuleBase<SocketCommandContext>
    {
        private InterfaceMessageChangeHandler _interfaceMessageChangeHandler { get; set; }
        private IAudioService _audioService { get; set; }
        private IDiscordClientWrapper _wrapper { get; set; }
        public General(IAudioService audioService,
            IDiscordClientWrapper discordClientWrapper,
            InterfaceMessageChangeHandler interfaceMessageChangeHandler)
        {
            _wrapper = discordClientWrapper;
            _audioService = audioService;
            _interfaceMessageChangeHandler = interfaceMessageChangeHandler;
        }

        [Command("ping")]
        public async Task PingAsync()
        {
            await ReplyAsync("Pong");
        }

        [Command("help")]
        [Alias("!")]
        public async Task HelpAsync()
        {
            var embedBuilder = new EmbedBuilder();
            embedBuilder
                .WithTitle("qbBot Help.")
                .WithAuthor("Godless Crusader")
                .AddField("ping", "Help");
        }

        [Command("quit")]
        [Alias("q")]
        public async Task QuitAsync()
        {
            var player = _audioService.GetPlayer(Context.Guild.Id);
            if(await PlayerInteractionCheckAsync(player))
            {
                
                
                if (player is ListedLavalinkPlayer)
                {
                    var listed = (ListedLavalinkPlayer)player;
                    await listed.ExitAsync();
                }
                else
                {
                    await player.DestroyAsync();
                }    
        
                player.Dispose();
            }    
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

            var player = _audioService.GetPlayer(Context.Guild.Id);
            if(player == null)
                player = await _audioService.JoinAsync<LavalinkPlayer>(Context.Guild.Id, channel.Id, true);
            await player.PlayAsync(track);
            ReplyAsync("https://media.giphy.com/media/Vuw9m5wXviFIQ/giphy.gif");
        }

        [Command("goto")]
        public async Task GoToTrackAsync(int position)
        {
            var player = _audioService.GetPlayer<ListedLavalinkPlayer>(Context.Guild.Id);
            var result = await PlayerInteractionCheckAsync(player);
            if(result)
            {
                await player.GotoAsync(position);
            }
            
        }

        [Command("musicBox", false)]
        [Alias("mb")]
        public async Task MusicBoxAsync(string tracksUrl)
        {
            var check = await ChannelJoinInitCheckAsync();
            if (!check)
                return;
           
            var tracks = await _audioService.GetTracksAsync(tracksUrl);

            var playListName = "Playlist";
            
            if (Context.Message.Embeds.Count != 0)
                playListName = Context.Message.Embeds.First().Title;
                
            if(!tracks.Any())
            {
                await ReplyAsync("Couldn't find anything");
                return;
            }


            var channel = Context.Guild.VoiceChannels.First(x => x.ConnectedUsers.Contains(Context.User));
            
            var player = _audioService.GetPlayer<ListedLavalinkPlayer>(Context.Guild.Id);

            IUserMessage playerMessage;
            

            if (player == null)
            {
                player = await _audioService.JoinAsync<ListedLavalinkPlayer>(Context.Guild.Id, channel.Id, true);
                player.MessageModificationRequired += _interfaceMessageChangeHandler.ModifyInterfaceMessageAsync;

                playerMessage = await ReplyAsync($"MusicBox Player");
                player.Message = playerMessage;
                player.List.AddRange(tracks);
                await player.GotoAsync(1);
                await player.AddPlaylist(playListName, tracksUrl);
            }
            else
            {
                await player.AddPlaylist(playListName, tracksUrl);
            }
            

        }

        
        private async Task<bool> PlayerInteractionCheckAsync(LavalinkPlayer? player)
        {
            if (player == null || player.State == PlayerState.Destroyed || player.State == PlayerState.NotConnected)
            {
                await ReplyAsync("Player is uninitialized. Please consider initializing player with !mb command!");
                return false;
            }

            return true;
        }
        private async Task<bool> ChannelJoinInitCheckAsync()
        {
            if (Context.Guild == null)
            {
                await ReplyAsync("Please type command in guild chat");
                return false;
            }

            if (Context.Guild.VoiceChannels.Any(x => x.ConnectedUsers.Contains(Context.User)) == false)
            {
                await ReplyAsync("Please join target channel");
                return false;
            }

            return true;
        }


    }
}
