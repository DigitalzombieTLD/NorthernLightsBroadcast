using MelonLoader;
using UnityEngine;
using Il2CppInterop;
using Il2CppInterop.Runtime.Injection; 
using System.Collections;
using System.Reflection;
using Il2Cpp;
using UnityEngine.Video;
using Il2CppInterop.Runtime.Attributes;
using AudioMgr;
using Il2CppNewtonsoft.Json.Linq;
using Il2CppTMPro;
using Il2CppInterop.Runtime;
using UnityEngine.Events;
using Harmony;



namespace NorthernLightsBroadcast
{
    [RegisterTypeInIl2Cpp]
    public class TVPlayer : MonoBehaviour
    {
        public TVManager manager;       
        public bool isSetup = false;

        public TVPlayer(IntPtr intPtr) : base(intPtr)
        {
        }

        public void Awake()
        {
            if (isSetup)
            {
                return;
            }

            manager = GetComponent<TVManager>();
            
            manager.videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            manager.videoPlayer.SetTargetAudioSource(0, manager.playerAudio._audioSource);

            manager.videoPlayer.targetCameraAlpha = 1f;
            manager.videoPlayer.isLooping = false;
            manager.videoPlayer.aspectRatio = VideoAspectRatio.FitInside;


            // thanks herp
            manager.videoPlayer.loopPointReached = (
                    (manager.videoPlayer.loopPointReached == null)
                    ? new System.Action<VideoPlayer>(PlaybackEnd)
                    : Il2CppSystem.Delegate.Combine(manager.videoPlayer.loopPointReached, (Il2CppSystem.Action<VideoPlayer>)PlaybackEnd).Cast<VideoPlayer.EventHandler>());

            manager.videoPlayer.started = (
                    (manager.videoPlayer.started == null)
                    ? new System.Action<VideoPlayer>(PlaybackStarted)
                    : Il2CppSystem.Delegate.Combine(manager.videoPlayer.started, (Il2CppSystem.Action<VideoPlayer>)PlaybackStarted).Cast<VideoPlayer.EventHandler>());

            manager.videoPlayer.prepareCompleted = (
                    (manager.videoPlayer.prepareCompleted == null)
                    ? new System.Action<VideoPlayer>(PrepareCompleted)
                    : Il2CppSystem.Delegate.Combine(manager.videoPlayer.prepareCompleted, (Il2CppSystem.Action<VideoPlayer>)PrepareCompleted).Cast<VideoPlayer.EventHandler>());


            manager.videoPlayer.errorReceived = (
                    (manager.videoPlayer.errorReceived == null)
                    ? new System.Action<VideoPlayer, string>(Error)
                    : Il2CppSystem.Delegate.Combine(manager.videoPlayer.errorReceived, (Il2CppSystem.Action<VideoPlayer, string>)Error).Cast<VideoPlayer.ErrorEventHandler>());

            isSetup = true;
        }      

        public void Error(VideoPlayer source, string message)
        {
            MelonLogger.Msg("Error on videoplayback -> " + message);
            manager.errorText = message;
            manager.SwitchState(TVManager.TVState.Error);
        }

        public void PlaybackStarted(VideoPlayer source)
        {
            manager.SwitchState(TVManager.TVState.Playing);
        }

        public void PrepareCompleted(VideoPlayer source)
        {            
            manager.SwitchState(TVManager.TVState.Playing);
        }

        public void PlaybackEnd(VideoPlayer source)
        {         
            manager.SwitchState(TVManager.TVState.Ended);
        }
    }
}