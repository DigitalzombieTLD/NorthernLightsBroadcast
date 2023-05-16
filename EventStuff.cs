using MelonLoader;
using UnityEngine;
using Il2CppInterop;
using Il2CppInterop.Runtime.Injection; 
using System.Collections;
using System.Reflection;
using Il2Cpp;
using UnityEngine.Video;
using static AudioMgr.VolumeMaster;

namespace NorthernLightsBroadcast
{
    public static class EventStuff
    {
        public delegate void OnErrorReceived(VideoPlayer incomingPlayer, string message);
        public static event OnErrorReceived onErrorReceived;

        public delegate void OnPlaybackStarted(VideoPlayer incomingPlayer);
        public static event OnPlaybackStarted onPlaybackStarted;

        public delegate void OnPrepareCompleted(VideoPlayer incomingPlayer);
        public static event OnPrepareCompleted onPrepareCompleted;

        public delegate void OnEndReached(VideoPlayer incomingPlayer);
        public static event OnEndReached onEndReached;

        public static void ErrorReceived(VideoPlayer incomingPlayer, string message)
        {
            if (onErrorReceived != null)
            {
                onErrorReceived(incomingPlayer, message);
            }
        }

        public static void PlaybackStarted(VideoPlayer incomingPlayer)
        {
            if (onPlaybackStarted != null)
            {
                onPlaybackStarted(incomingPlayer);
            }
        }

        public static void PrepareCompleted(VideoPlayer incomingPlayer)
        {
            if (onPrepareCompleted != null)
            {
                onPrepareCompleted(incomingPlayer);
            }
        }

        public static void EndReached(VideoPlayer incomingPlayer)
        {
            if (onEndReached != null)
            {
                onEndReached(incomingPlayer);
            }
        }
    }      
}