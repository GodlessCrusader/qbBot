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

                var replyEmbedBuilder = new EmbedBuilder();

                replyEmbedBuilder
                    .WithTitle("Music Box")
                    .WithColor(Color.DarkOrange)
                    .AddField($"Currently playing: {currentTrackIndex + 1}. {player.List[currentTrackIndex].Title}",
                    player.List[currentTrackIndex].Duration);
                    
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
                }
                else
                    iterationEnd = player.List.Count;
                
                for (int i = iterationStart; i < iterationEnd; i++)
                {
                    tracksSelectorBuilder.AddOption(
                        $"{counter++}. {player.List[i].Title}",
                        (i + 1).ToString());
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
