using Discord;
using Discord.Commands;
using Lavalink4NET;
using Lavalink4NET.Player;
using qbBot.Classes;
using qbBot.Services;

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
                .WithColor(Color.Blue)
                .WithTitle("qbBot Help.")
                .WithAuthor("Godless Crusader")
                .AddField("!ping", "Replies with pong")
                .AddField("!quit / !q","Makes bot to exit voice channel")
                .AddField("!musicbox / !mb", "Use it to play music from youtube. !mb [youtube link]")
                .AddField("!rickroll / !rr", "〵( ͝ ° ͜ʖ͡° )〴")
                .AddField("!roll / !r", "Use to roll dice. !r [dice expression like 3d6]")
                .AddField("!goto", "Use to change current playing musicbox track. !goto [track number]");
            await ReplyAsync(embed: embedBuilder.Build());
        }

        [Command("skip")]
        [Alias("s")]
        public async Task SkipAsync()
        {
            var player = _audioService.GetPlayer(Context.Guild.Id);
            
            if (await PlayerInteractionCheckAsync(player))
            {
                if (player is ListedLavalinkPlayer)
                {
                    var listed = (ListedLavalinkPlayer)player;
                    await listed.SkipAsync();
                }
                else
                {
                    await ReplyAsync("This player doesn't support queueing. Consider reinitializing with !q -> !mb");
                }

                player.Dispose();
            }
            
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
              
            if(!tracks.Any())
            {
                await ReplyAsync("Couldn't find anything");
                return;
            }

            if (Context.Message.Embeds.Count != 0)
                playListName = Context.Message.Embeds.First().Title;
            else
                playListName = tracks.First().Title;

            if (playListName.Length >= 100)
               playListName = playListName.Take(99).ToString();
            
            var channel = Context.Guild.VoiceChannels.First(x => x.ConnectedUsers.Contains(Context.User));
            
            var player = _audioService.GetPlayer<ListedLavalinkPlayer>(Context.Guild.Id);

            IUserMessage playerMessage;
            

            if (player == null)
            {
                player = await _audioService.JoinAsync<ListedLavalinkPlayer>(Context.Guild.Id, channel.Id, true);
                player.MessageModificationRequired += _interfaceMessageChangeHandler.ModifyInterfaceMessageAsync;

                playerMessage = await ReplyAsync($"MusicBox Player");
                player.Message = playerMessage;
                await player.AddPlaylistAsync(playListName, tracksUrl, _audioService);
            }
            else
            {
                if (player.Playlist.Count < 24)
                    await player.AddPlaylistAsync(playListName, tracksUrl, _audioService);
                else
                    await ReplyAsync("Playlist List is full.");
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
