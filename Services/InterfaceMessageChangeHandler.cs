using Discord;
using Discord.WebSocket;
using Lavalink4NET;
using qbBot.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qbBot.Services
{
    public class InterfaceMessageChangeHandler
    {
        const int MAX_OPTIONS_COUNT = 25;
       
        private IAudioService _audioService { get; set; }
        private IDiscordClientWrapper _wrapper { get; set; }
        public InterfaceMessageChangeHandler( IAudioService audioService, IDiscordClientWrapper wrapper)
        {
            _audioService = audioService;
            _wrapper = wrapper;
        }


        public async void ModifyInterfaceMessageAsync(object sender, int currentTrackIndex)
        {
            if (sender is ListedLavalinkPlayer)
            {
                var player = sender as ListedLavalinkPlayer;

                if(player.State == Lavalink4NET.Player.PlayerState.Destroyed)
                {
                    await player.Message.DeleteAsync();
                    return;
                }
                var playButtonStyle = ButtonStyle.Success;
                var loopButtonStyle = ButtonStyle.Success;
                var shuffleButtonStyle = ButtonStyle.Success;
                if (player.State == Lavalink4NET.Player.PlayerState.Paused)
                    playButtonStyle = ButtonStyle.Danger;
                if(player.IsLooping)
                    loopButtonStyle = ButtonStyle.Danger;
                if(player.IsShuffling)
                    shuffleButtonStyle = ButtonStyle.Danger;
                var componentBuilder = new ComponentBuilder();
                componentBuilder
                    .WithButton(
                        label: Emoji.Parse(":track_previous:").ToString(),
                        customId: "previous-track",
                        style: ButtonStyle.Success
                    )
                    .WithButton(
                        label: Emoji.Parse(":play_pause:").ToString(),
                        customId: "play-pause",
                        style: playButtonStyle
                    )
                    .WithButton(
                        label: Emoji.Parse(":repeat:").ToString(),
                        customId: "repeat",
                        style: loopButtonStyle
                    )
                    .WithButton(
                        label: Emoji.Parse(":twisted_rightwards_arrows:").ToString(),
                        customId: "shuffle",
                        style: shuffleButtonStyle
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
               

                EmbedBuilder replyEmbedBuilder = new EmbedBuilder();
               
                if (player.List.Any())
                {
                    
                    replyEmbedBuilder
                        .WithTitle("Music Box")
                        .WithColor(Color.DarkOrange)
                        .AddField($"Currently playing: {currentTrackIndex + 1}. {player.List[currentTrackIndex].Title.Shorten(50)}",
                        player.List[currentTrackIndex].Duration);
                }
                else
                {
                    replyEmbedBuilder
                        .WithTitle("0 tracks loaded")
                        .WithColor(Color.Red)
                        .AddField(" - ", " - ");
                }
                    
                    
                var tracksSelectorBuilder = new SelectMenuBuilder();
                    tracksSelectorBuilder
                        .WithPlaceholder("Go to:")
                        .WithCustomId("goto");

                var playlistsSelectorBuilder = new SelectMenuBuilder();
                playlistsSelectorBuilder
                    .WithPlaceholder("Playlist:")
                    .WithCustomId("playlist")
                    .AddOption("Add playlist ->", "add-playlist");
                
                foreach(var playlist in player.Playlist)
                {
                    playlistsSelectorBuilder.AddOption(playlist.Key, playlist.Key);
                }

                int counter = 1;
                int iterationEnd = MAX_OPTIONS_COUNT;
                int iterationStart = 0;
                if (player.List.Count >= MAX_OPTIONS_COUNT)
                {
                    if (currentTrackIndex >= MAX_OPTIONS_COUNT / 2)
                    {
                        counter = 1 + currentTrackIndex - MAX_OPTIONS_COUNT / 2;
                        iterationStart = counter - 1;
                        iterationEnd = iterationStart + MAX_OPTIONS_COUNT;
                    }

                    if (player.List.Count - currentTrackIndex <= MAX_OPTIONS_COUNT / 2)
                    {
                        iterationStart = currentTrackIndex - MAX_OPTIONS_COUNT / 2;
                        iterationEnd = player.List.Count;
                    }
                }
                else
                    iterationEnd = player.List.Count;


                for (int i = iterationStart; i < iterationEnd; i++)
                {
                    tracksSelectorBuilder.AddOption(
                        $"{counter++}. {player.List[i].Title.Shorten(50)}",
                        (i + 1).ToString().Shorten(50));
                }
                

                componentBuilder.WithSelectMenu(playlistsSelectorBuilder);
                componentBuilder.WithSelectMenu(tracksSelectorBuilder);


                await player.Message.ModifyAsync(x => { 
                    x.Components = componentBuilder.Build();
                    x.Embed = replyEmbedBuilder.Build();
                });


            }

            

           
        }

        
    }
}
