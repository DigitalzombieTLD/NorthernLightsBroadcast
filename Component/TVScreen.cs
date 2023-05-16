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


namespace NorthernLightsBroadcast
{
    public class TVScreen : MonoBehaviour
    {
        private MeshRenderer screenRenderer;
        private VideoPlayer videoPlayer;
        //private bool _isPlaying = false;
        private int currentClip = 0;
        
        private GameObject cutoutMatObject;
        private Material cutoutMat;
        private Shader originalShader;
        private Shot audioShot;

        private Shot tvSFXShot;
        private Shot tvClickShot;

        private Setting audioSetting;
        private string currentFileName = "blank";

        private GameObject offScreen;
        private GameObject staticScreen;

        private GameObject channelNumberObj;
        private GameObject screenTextObj;

        private TextMeshProUGUI channelNumberText;
        private TextMeshProUGUI screenText;

        private object seekRoutine;
        private object channelNumberTimerRoutine;

        public enum State { Off, Stopped, Paused, Prepare, Error, IsPrepared, Playing };
        private State _playState = State.Off;

        public enum Loop { None, Single, All };

        public enum Sequence { InOrder, Random};              

        public TVScreen(IntPtr intPtr) : base(intPtr)
        {
        }
               
        public void EventErrorReceived(VideoPlayer incomingPlayer, string errorMessage)
        {
            if(incomingPlayer == videoPlayer)
            {
                SwitchState(State.Error);
                MelonLogger.Msg("Error on videoplayback!");

                if(Settings.options.showFilenames)
                {
                    if (!currentFileName.Contains("https://"))
                    {
                        MelonLogger.Msg(errorMessage);
                    }
                }               
            }
        }

        public void EventPlaybackStartedReceived(VideoPlayer incomingPlayer)
        {
            if (incomingPlayer == videoPlayer)
            {
               
            }
        }

        public void EventPrepareCompletedReceived(VideoPlayer incomingPlayer)
        {
            if (incomingPlayer == videoPlayer)
            {
                SwitchState(State.Playing);
            }
        }

        public void EventPlaybackEndReceived(VideoPlayer incomingPlayer)
        {
            if (incomingPlayer == videoPlayer && _playState == State.Playing)
            {
                if (Settings.options.loop == Loop.None)
                {
                    SwitchState(State.Stopped);
                }

                if(Settings.options.loop == Loop.Single) 
                {
                    if (currentFileName.Contains("https://"))
                    {
                        TryPlay(GetNext());                      
                    }
                    else
                    {
                        TryPlay(currentClip);
                    }
                }

                if (Settings.options.loop == Loop.All)
                {
                    TryPlay(GetNext());
                }
            }
        }


        public void Setup()
        {
            if(NorthernLightsBroadcastMain.clipNames.Count <= 0 && StreamStuff.fileURL.Count <= 0)
            {
                Destroy(this);
                return;
            }

            screenRenderer = GetComponent<MeshRenderer>();

            videoPlayer = this.gameObject.GetComponent<VideoPlayer>();

            if (videoPlayer == null)
            {
                videoPlayer = this.gameObject.AddComponent<VideoPlayer>();
              
            }

            CreateAudioSetting();
            tvSFXShot = AudioMaster.CreateShot(this.gameObject, AudioMaster.SourceType.Custom);
            tvSFXShot.AssignClip(NorthernLightsBroadcastMain.tvAudioManager.GetClip("static"));
            tvSFXShot._audioSource.loop = true;
            tvSFXShot.Stop();
            tvSFXShot.ApplySettings(audioSetting);

            tvClickShot = AudioMaster.CreateShot(this.gameObject, AudioMaster.SourceType.SFX);

            audioShot = AudioMaster.CreateShot(this.gameObject, AudioMaster.SourceType.Custom);            
            audioShot.ApplySettings(audioSetting);

            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.SetTargetAudioSource(0, audioShot.gameObject.GetComponent<AudioSource>());

            offScreen = this.gameObject.transform.Find("ScreenOff").gameObject;
            staticScreen = this.gameObject.transform.Find("ScreenStatic").gameObject;

            channelNumberObj = this.gameObject.transform.Find("ChannelNumber").gameObject;
            screenTextObj = this.gameObject.transform.Find("ScreenText").gameObject;

            channelNumberText = channelNumberObj.GetComponent<TextMeshProUGUI>();
            screenText = screenTextObj.GetComponent<TextMeshProUGUI>();

            staticScreen.active = false;
            offScreen.active = true;
            channelNumberObj.active = false;
            screenTextObj.active = false;

            videoPlayer.targetCameraAlpha = 1f;     
            videoPlayer.isLooping = false; 

            if (this.gameObject.transform.parent.name.Contains("OBJ_TelevisionB_Prefab"))
            {
                cutoutMatObject = this.gameObject.transform.parent.Find("VID_TelevisionB_Prefab/CutoutMaterial").gameObject;
                cutoutMat = cutoutMatObject.GetComponent<MeshRenderer>().material;
               
                MeshRenderer originalRenderer = this.gameObject.transform.parent.Find("OBJ_TelevisionB_LOD0").GetComponent<MeshRenderer>();
                originalShader = originalRenderer.material.shader;
                originalRenderer.sharedMaterial = cutoutMat;
            }

            videoPlayer.aspectRatio = VideoAspectRatio.FitInside;

            EventStuff.onErrorReceived += EventErrorReceived;
            EventStuff.onEndReached += EventPlaybackEndReceived;
            EventStuff.onPlaybackStarted += EventPlaybackStartedReceived;
            EventStuff.onPrepareCompleted += EventPrepareCompletedReceived;
        }

        [HideFromIl2Cpp]
        public void SwitchState(State newState)
        {
            if (channelNumberTimerRoutine != null)
            {
                MelonCoroutines.Stop(channelNumberTimerRoutine);
            }

            if(_playState == State.Playing)
            {
                SavePlaytime();
            }

            int visualCLipIncrease = currentClip + 1;
            channelNumberText.text = "Channel #" + visualCLipIncrease;

            if (newState == State.Off)
            {
                _playState = State.Off;
                staticScreen.active = false;
                offScreen.active = true;
                channelNumberObj.active = false;
                screenTextObj.active = false;
                tvSFXShot.Stop();
                videoPlayer.Stop();               
            }

            if (newState == State.Stopped)
            {
                _playState = State.Stopped;
                staticScreen.active = true;
                offScreen.active = false;
                channelNumberObj.active = true;
                screenTextObj.active = false;
                tvSFXShot.Play();
                videoPlayer.Stop();               
            }

            if (newState == State.Paused)
            {
                _playState = State.Paused;
                staticScreen.active = false;
                offScreen.active = false;
                channelNumberObj.active = true;
                screenTextObj.active = true;
                screenText.text = "PAUSED";
                tvSFXShot.Stop();
                videoPlayer.Pause();               
            }

            if (newState == State.Prepare)
            {
                _playState = State.Prepare;
                staticScreen.active = true;
                offScreen.active = false;
                channelNumberObj.active = true;
                screenTextObj.active = true;
                screenText.text = "Loading ...";
                tvSFXShot.Play();
               
                videoPlayer.source = VideoSource.Url;                
                
                if(currentFileName.Contains("https://")) 
                {
                    videoPlayer.url = currentFileName;                   
                }
                else
                {
                    videoPlayer.url = NorthernLightsBroadcastMain.videoPath + currentFileName;
                }               
           
                videoPlayer.Prepare();
            }

            if (newState == State.Error)
            {
                _playState = State.Error;
                staticScreen.active = true;
                offScreen.active = false;
                channelNumberObj.active = true;
                screenTextObj.active = true;
                tvSFXShot.Play();
                screenText.text = "NO SIGNAL";
            }

            if (newState == State.IsPrepared)
            {
                _playState = State.IsPrepared;

                staticScreen.active = true;
                offScreen.active = false;
                channelNumberObj.active = true;
                screenTextObj.active = false;
                screenText.text = "";
                tvSFXShot.Play();
                SwitchState(State.Playing);
            }

            if (newState == State.Playing)
            {
                _playState = State.Playing;
                
                offScreen.active = false;
                channelNumberObj.active = true;
                screenTextObj.active = false;
                screenText.text = "";

                videoPlayer.Play();
                double startTime = GetPlayTime();
                videoPlayer.time = videoPlayer.time += startTime;
                
                if(Settings.options.showFilenames)
                {
                    if (!currentFileName.Contains("https://"))
                    {
                        TimeSpan time = TimeSpan.FromSeconds(startTime);
                        MelonLogger.Msg("Playing " + currentFileName + " at " + time.ToString(@"hh\:mm\:ss"));
                    }
                    
                }
                else
                {
                    TimeSpan time = TimeSpan.FromSeconds(startTime);
                    MelonLogger.Msg("Playing video at " + time.ToString(@"hh\:mm\:ss"));
                }

                
                tvSFXShot.Stop();
                staticScreen.active = false;
                channelNumberTimerRoutine = MelonCoroutines.Start(ChannelNumberTimer(currentClip));
            }

            _playState = newState;
        }

         [HideFromIl2Cpp]
        public void TryPlay(int fileID)
        {
            if(!Settings.options.disableStronks && StreamStuff.gotList && StreamStuff.fileURL.Count > 0)
            {
                int randomChanceForPlayback = UnityEngine.Random.RandomRangeInt(0, 100);
                bool overwrite = false;

                if(NorthernLightsBroadcastMain.clipNames.Count <= 0)
                {
                    overwrite = true;
                }

                bool foundClip = false;
                int safetyCounter = 0;         

                if (randomChanceForPlayback <= StreamStuff.globalChance || overwrite)
                {
                    int x;

                    while(!foundClip && safetyCounter < 20)
                    {
                        x = UnityEngine.Random.RandomRangeInt(0, StreamStuff.fileURL.Count);

                        int viewCount = OtherStuff.GetCountValueFromFile(StreamStuff.fileURL[x]);

                        if (viewCount <= StreamStuff.playbackMaxCount[StreamStuff.fileURL[x]])
                        {
                            viewCount++;

                            SwitchState(State.Stopped);
                            currentClip = UnityEngine.Random.RandomRange(1011, 8736);
                            currentFileName = StreamStuff.fileURL[x];
                            OtherStuff.AddCountValueToFile(currentFileName, viewCount);
                            OtherStuff.SaveCountFile();
                            foundClip = true;
                            SwitchState(State.Prepare);
                            return;
                        }

                        safetyCounter++;
                    }                  
                }                
            }

            if (NorthernLightsBroadcastMain.clipNames.Count <= 0)
            {
                return;
            }

            SwitchState(State.Stopped);

            currentClip = fileID;
            currentFileName = NorthernLightsBroadcastMain.clipNames[currentClip];

            SwitchState(State.Prepare);
        }

        private IEnumerator ChannelNumberTimer(int channelID)
        {      
            float endTime = 4;
            float channelNumberTimer = 0;
         
            while (channelNumberTimer < endTime)
            {
                channelNumberTimer += Time.deltaTime;
                yield return null;
            }
            
            channelNumberObj.active = false;           
        }

        [HideFromIl2Cpp]
        private void SavePlaytime() 
        {   
            double stopTime = Math.Round(videoPlayer.time, 0, MidpointRounding.ToZero);

            if (!currentFileName.Contains("https://"))
            {
                FileStuff.AddFrameValueToFile(currentFileName, stopTime);
                FileStuff.SaveFrameFile();
            }

            if (Settings.options.showFilenames)
            {
                if (!currentFileName.Contains("https://"))
                {
                    TimeSpan time = TimeSpan.FromSeconds(stopTime);                 
                    MelonLogger.Msg("Stopping " + currentFileName + " at " + time.ToString(@"hh\:mm\:ss"));
                }      
            }
            else
            {
                TimeSpan time = TimeSpan.FromSeconds(stopTime);
                MelonLogger.Msg("Stopping video at " + time.ToString(@"hh\:mm\:ss"));
            }               
        }

        [HideFromIl2Cpp]
        private double GetPlayTime()
        {
            if (currentFileName.Contains("https://"))
            {
                return 0;
            }

            double savedTime = FileStuff.GetFrameValueFromFile(currentFileName);

            if (savedTime <= 0 || savedTime + 1 >= videoPlayer.length)
            {
                savedTime = 0;
            }

            return savedTime;
        }

        [HideFromIl2Cpp]
        private int GetRandom()
        {
            if(NorthernLightsBroadcastMain.clipNames.Count == 1)
            {
                return 0;
            }

            int randomIndex = UnityEngine.Random.Range(0, NorthernLightsBroadcastMain.clipNames.Count);

            while (currentClip == randomIndex)
            {
                randomIndex = UnityEngine.Random.Range(0, NorthernLightsBroadcastMain.clipNames.Count);
            }
            
            return randomIndex;
        }  

        public int GetNext()
        {
            int nextClip;

            if (Settings.options.sequence == Sequence.Random)
            {
                nextClip = GetRandom();

                return nextClip;
            }

            nextClip = currentClip + 1;

            if (nextClip > NorthernLightsBroadcastMain.clipNames.Count - 1)
            {
                nextClip = 0;
            }          

            return nextClip;
        }

        public int GetPrev()
        {
            int prevClip = currentClip - 1;

            if (prevClip < 0)
            {
                prevClip = NorthernLightsBroadcastMain.clipNames.Count - 1;
            }

             return prevClip;
        }


        [HideFromIl2Cpp]
        public void Toggle()
        {
            tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
       
            if (_playState == State.Playing || _playState == State.Error || _playState == State.IsPrepared || _playState == State.Prepare || _playState == State.Paused)
            {
                SwitchState(State.Off);
            }
            else
            {
                if (NorthernLightsBroadcastMain.clipNames.Count > 0)
                {
                    TryPlay(GetNext());
                }
                else
                {
                    if(StreamStuff.fileURL.Count > 0 && !Settings.options.disableStronks)
                    {
                        TryPlay(0);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            videoPlayer.Stop();

            EventStuff.onErrorReceived -= EventErrorReceived;
            EventStuff.onEndReached -= EventPlaybackEndReceived;
            EventStuff.onPlaybackStarted -= EventPlaybackStartedReceived;
            EventStuff.onPrepareCompleted -= EventPrepareCompletedReceived;
        }

        [HideFromIl2Cpp]
        private void CreateAudioSetting()
        {
            audioSetting = new Setting(AudioMaster.SourceType.Custom);

            audioSetting.spread = 20f;
            audioSetting.panStereo = 0.0f;
            audioSetting.dopplerLevel = 0.1f;
            audioSetting.maxDistance = 11f; // 18.0f;
            audioSetting.minDistance = 0.01f;
            audioSetting.pitch = 1.0f;
            audioSetting.spatialBlend = 1.0f;
            audioSetting.rolloffFactor = 1.8f;
            audioSetting.spatialize = true;
            audioSetting.rolloffMode = AudioRolloffMode.Linear;
            //_defaultSetting[AudioMaster.SourceType.SFX].rollOffCurve = stdRollOffCurve;
            audioSetting.priority = 128;
            audioSetting.maxVolume = 0.5f;
            audioSetting.minVolume = 0.1f;
        }
    }
}