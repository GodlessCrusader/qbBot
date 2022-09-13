using Lavalink4NET.Events;
using Lavalink4NET.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qbBot.Classes
{
    public class ListedLavalinkPlayer : Lavalink4NET.Player.LavalinkPlayer
    {
        private readonly bool _disconnectOnStop;

        
        public ListedLavalinkPlayer()
        {
            List = new List<LavalinkTrack>();

           
            _disconnectOnStop = DisconnectOnStop;
            DisconnectOnStop = false;
        }


        public int CurrentTrackIndex { get; private set; }
        public bool IsLooping { get; set; }

        public List<LavalinkTrack> List { get; }


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

        
        /*public virtual async Task PlayTopAsync(LavalinkTrack track)
        {
            EnsureNotDestroyed();

            if (track is null)
            {
                throw new ArgumentNullException(nameof(track));
            }

            // play track if none is playing
            if (State == PlayerState.NotPlaying)
            {
                await PlayAsync(track, enqueue: false);
            }
            // the player is currently playing a track, enqueue the track at top
            else
            {
                Queue.Insert(0, track);
            }
        }*/

        
        /*public virtual async Task<bool> PushTrackAsync(LavalinkTrack track, bool push = false)
        {
            // star track immediately
            if (State == PlayerState.NotPlaying)
            {
                if (push)
                {
                    return false;
                }

                await PlayAsync(track, enqueue: false);
                return false;
            }

            // create clone and set starting position
            var oldTrack = CurrentTrack!.WithPosition(Position.Position);

            // enqueue old track with starting position
            Queue.Add(oldTrack);

            // push track
            await PlayAsync(track, enqueue: false);
            return true;
        }*/

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
                LavalinkTrack? track = null;

                for(int i = 0; i < count; i++) 
                {
                    // no more tracks in queue
                    if (List.Count < 1)
                    {
                        // no tracks found
                        return DisconnectAsync();
                    }

                    // requeue track
                    track = List[ List.Count - 1 ];
                    List.RemoveAt( List.Count - 1 );
                    List.Insert(0, track);
                    track = List[ List.Count - 1 ];
                }

                // a track to play was found, dequeue and play
                return PlayAsync(track!, false);
            }
            // no tracks queued, stop player and disconnect if specified
            else
            {
                StopAsync(disconnect: _disconnectOnStop);
            }

            return Task.CompletedTask;
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
                LavalinkTrack? track = null;

                for(int i = 0; i < count; i++)
                {
                    // no more tracks in queue
                    if (List.Count < 1)
                    {
                        // no tracks found
                        return DisconnectAsync();
                    }

                    // requeue track
                    track = List[0];
                    List.RemoveAt(0);
                    List.Add(track);
                }

                // a track to play was found, dequeue and play
                return PlayAsync(track!, false);
            }
            // no tracks queued, stop player and disconnect if specified
            else
            {
                StopAsync(disconnect: _disconnectOnStop);
            }

            return Task.CompletedTask;
        }

        public override Task StopAsync(bool disconnect = false)
        {
            List.Clear();
            return base.StopAsync(disconnect);
        }
    }
}
