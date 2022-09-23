﻿using Discord;
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
                await _audioService.JoinAsync<ListedLavalinkPlayer>(Context.Guild.Id, channel.Id);
            await player.PlayAsync(track);
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
            
            if (Context.Message.Embeds != null)
                playListName = Context.Message.Embeds.First().Title;
                
            if(tracks == null)
            {
                await ReplyAsync("Couldn't find anything");
                return;
            }
            var channel = Context.Guild.VoiceChannels.First(x => x.ConnectedUsers.Contains(Context.User));
            
            var player = _audioService.GetPlayer<ListedLavalinkPlayer>(Context.Guild.Id);
            
            if (player == null)
            {
                player = await _audioService.JoinAsync<ListedLavalinkPlayer>(Context.Guild.Id, channel.Id, true);
                player.MessageModificationRequired += _interfaceMessageChangeHandler.ModifyInterfaceMessageAsync;
            }

            player.Playlist.Add(playListName, tracksUrl);
            player.List.AddRange(tracks);
            
            var playerMessage = await ReplyAsync($"MusicBox Player");
            if(player.Message == null)
                player.Message = playerMessage;
            await player.SkipAsync();
        }

        
        private async Task<bool> PlayerInteractionCheckAsync(LavalinkPlayer? player)
        {
            if (player == null || player.State == PlayerState.Destroyed || player.State == PlayerState.NotConnected)
            {
                await ReplyAsync("Please consider initializing player with !mb command!");
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
