using Discord;
using Lavalink4NET;
using Lavalink4NET.Events;
using Lavalink4NET.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace qbBot.Classes
{
    public class ListedLavalinkPlayer : Lavalink4NET.Player.LavalinkPlayer
    {
        private readonly bool _disconnectOnStop;

        private int[] _playOrder;
        public ListedLavalinkPlayer()
        {
            IsShuffling = false;
            Playlist = new();
            List = new();
            _disconnectOnStop = DisconnectOnStop;
            DisconnectOnStop = false;
            MessageModificationRequired = (o, i) => { return; };
        }

        public IUserMessage? Message { get; set; }

        public event EventHandler<int> MessageModificationRequired;

        public bool IsShuffling { get; set; }
        public bool IsLooping { get; set; }

        private int _currentTrackIndex = 0;
        public List<LavalinkTrack> List { get; }
        public Dictionary<string, string> Playlist { get; }
        public override Task OnTrackEndAsync(TrackEndEventArgs eventArgs)
        {
            
            if (eventArgs.MayStartNext)
            {
                return SkipAsync();
            }
            else if (_disconnectOnStop)
            {
                return DisconnectAsync();
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        
        public new virtual Task<int> PlayAsync(LavalinkTrack track, TimeSpan? startTime = null,
            TimeSpan? endTime = null, bool noReplace = false)
            => PlayAsync(track, true, startTime, endTime, noReplace);

        
        public virtual async Task<int> PlayAsync(LavalinkTrack track, bool enqueue = false,
            TimeSpan? startTime = null, TimeSpan? endTime = null, bool noReplace = false)
        {
            EnsureNotDestroyed();
            EnsureConnected();

            // check if the track should be enqueued (if a track is already playing)
            if (enqueue && (List.Count > 0 || State == PlayerState.Playing || State == PlayerState.Paused))
            {
                // add the track to the queue
                List.Add(track);

                // return track queue position
                return List.Count;
            }

            // play the track immediately
            await base.PlayAsync(track, startTime, endTime, noReplace);

            // 0 = now playing
            return 0;
        }

        public virtual Task GotoAsync(int index)
        {
            if (index - 1 < 0 || index - 1 > List.Count - 1 || List.Count == 0)
            {
                return Task.CompletedTask;
            }

            EnsureNotDestroyed();
            EnsureConnected();
           
            _currentTrackIndex = index - 1;
            
            MessageModificationRequired.Invoke(this, _currentTrackIndex);
            
            return PlayAsync(List[_currentTrackIndex], false);
        }

        public virtual Task PreviousAsync(int count = 1)
        {
            if (count <= 0)
            {
                return Task.CompletedTask;
            }

            EnsureNotDestroyed();
            EnsureConnected();

            if (IsLooping && CurrentTrack != null)
            {
                return PlayAsync(CurrentTrack, false);
            }

            else if (!(List.Count == 0))
            {
                

                for(int i = 0; i < count; i++) 
                {
                    // requeue track
                    _currentTrackIndex --;
                    if(_currentTrackIndex < 0)
                        _currentTrackIndex = List.Count - 1;
                }

                MessageModificationRequired.Invoke(this, _playOrder[_currentTrackIndex] - 1);
                // a track to play was found, dequeue and play
                return PlayAsync(List[_playOrder[_currentTrackIndex] - 1], false);
            }
            // no tracks queued, stop player and disconnect if specified
           
            return Task.CompletedTask;
        }
        
        public async Task ShuffleAsync()
        {
            
            if (!IsShuffling)
            {
                IsShuffling = true;
                var rng = new Random(11);
                _playOrder = Enumerable.Range(1, List.Count).OrderBy(x => rng.Next()).ToArray();
                for(int i = 0; i < _playOrder.Length; i++)
                {
                    if (_playOrder[i] == _currentTrackIndex + 1)
                    {
                        _currentTrackIndex = i;
                        break;
                    }
                }
            }
            else
            {
                IsShuffling = false;
                _currentTrackIndex = _playOrder[_currentTrackIndex];
                _playOrder = Enumerable.Range(1, List.Count).ToArray();
                
            }
                
               
        }

        public virtual Task SkipAsync(int count = 1)
        {
            // no tracks to skip
            if (count <= 0)
            {
                return Task.CompletedTask;
            }

            EnsureNotDestroyed();
            EnsureConnected();

            
            if (IsLooping && CurrentTrack != null)
            {
                return PlayAsync(CurrentTrack, false);
            }
            
            else if (! (List.Count == 0))
            {

                for (int i = 0; i < count; i++)
                {
                    _currentTrackIndex ++;
                    if(_currentTrackIndex > List.Count - 1)
                        _currentTrackIndex = 0;
                }

                MessageModificationRequired.Invoke(this, _playOrder[_currentTrackIndex] - 1);
                return PlayAsync(List[ _playOrder[_currentTrackIndex] - 1], false);
            }

            return Task.CompletedTask;
        }

        public override Task StopAsync(bool disconnect = false)
        {
            List.Clear();
            Playlist.Clear();
            return base.StopAsync(disconnect);
        }

        public async Task ExitAsync()
        {
            await StopAsync(true);
            await DestroyAsync();
            MessageModificationRequired.Invoke(this, _currentTrackIndex);
        }

        public Task AddPlaylistAsync(string title, string url, IAudioService service)
        {
            Playlist.Add(title, url);
            if(List.Count == 0)
            {
                return ChangePlaylistAsync(title, service);
            }
            MessageModificationRequired.Invoke(this, _currentTrackIndex);
            return Task.CompletedTask;
        }
        public async Task ChangePlaylistAsync(string key, IAudioService service)
        {
            if(!Playlist.ContainsKey(key))
                return;
            
            var tracks = await service.GetTracksAsync(Playlist[key]);
            if (tracks == null)
                return;

            List.Clear();
            List.AddRange(tracks);
            _playOrder = Enumerable.Range(1, List.Count).ToArray();
            _currentTrackIndex = 0;
            MessageModificationRequired.Invoke(this, _currentTrackIndex);
            await GotoAsync(_currentTrackIndex + 1);
            
        }
    }
}