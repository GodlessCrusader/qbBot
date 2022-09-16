using Discord;
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
        private IAudioService _audioService { get; set; }
        private IDiscordClientWrapper _wrapper { get; set; }
        public InterfaceMessageChangeHandler(IAudioService audioService, IDiscordClientWrapper wrapper)
        {
            _audioService = audioService;
            _wrapper = wrapper;
        }

        public async void ModifyInterfaceMessageAsync(object sender, int currentTrackIndex)
        {
            if (sender is ListedLavalinkPlayer)
            {
                var player = (ListedLavalinkPlayer)sender;

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
                    
                var selectorBuilder = new SelectMenuBuilder();
                    selectorBuilder
                        .WithPlaceholder("Go to:")
                        .WithCustomId("goto");
                    var counter = currentTrackIndex + 1;
                if(player.List.Count - currentTrackIndex - 1 >= 25)
                    for (int i = 0; i < 25; i++)
                    {
                        selectorBuilder.AddOption($"{counter++}. {player.List[currentTrackIndex + i].Title}",
                            (currentTrackIndex + i).ToString());

                    }
                else
                    for(int i = currentTrackIndex; i < player.List.Count; i++)
                    {
                        selectorBuilder.AddOption($"{counter++}. {player.List[i].Title}",
                            i.ToString());
                    }
                componentBuilder.WithSelectMenu(selectorBuilder);


                await player.Message.ModifyAsync(x => { 
                    x.Components = componentBuilder.Build();
                    x.Embed = replyEmbedBuilder.Build();
                });


            }

            

           
        }
    }
}
