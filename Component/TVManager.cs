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
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NorthernLightsBroadcast
{
    [RegisterTypeInIl2Cpp]
    public class TVManager : MonoBehaviour
    {
        public GearItem thisGearItem;
        public ObjectGuid objectGuid;
        public string thisGuid;
        public bool firstStartDone = false;

        public GameObject screenObject;
        public MeshRenderer objectRenderer;
        public TVPlayer tvplayer;
        public TVUI ui;
        public TVButton redbutton;
        public GameObject dummyCamera;
        public VideoPlayer videoPlayer;
        public bool isSetup = false;
        public bool isCRT = false;
        public string errorText;
        public Light ambilight;

        public Setting audioSetting;
        public double saveTime = 0;

        public Shot playerAudio;
        public Shot staticAudio;

        public GraphicRaycaster graphicsRaycaster;
        public PointerEventData pointerEventData;

        public enum TVState { Off, Static, Paused, Preparing, Error, Playing, Resume, Ended };
        public TVState currentState = TVState.Off;

        

        public TVManager(IntPtr intPtr) : base(intPtr)
        {
        }
               
       public void Awake()
        {
            if (!isSetup)
            {
                if (this.gameObject.name.Contains("OBJ_TelevisionB_LOD0") || this.gameObject.name.Contains("GEAR_TV_CRT"))
                {
                    screenObject = GameObject.Instantiate(NorthernLightsBroadcastMain.NLB_TV_CRT, this.transform);
                }
                else if (this.gameObject.name.Contains("OBJ_Television_LOD0") || this.gameObject.name.Contains("GEAR_TV_LCD"))
                {
                    screenObject = GameObject.Instantiate(NorthernLightsBroadcastMain.NLB_TV_LCD, this.transform);
                }
                else if (this.gameObject.name.Contains("OBJ_Television") || this.gameObject.name.Contains("GEAR_TV_WALL"))
                {
                    screenObject = GameObject.Instantiate(NorthernLightsBroadcastMain.NLB_TV_WALL, this.transform);
                }

                screenObject.name = "NLB_TV";

                objectRenderer = this.gameObject.GetComponent<MeshRenderer>();

                redbutton = screenObject.transform.Find("PowerButton").gameObject.AddComponent<TVButton>();
                redbutton.manager = this;

                ambilight = screenObject.transform.Find("Ambilight").gameObject.GetComponent<Light>();
                ambilight.enabled = false;

                dummyCamera = screenObject.transform.Find("CameraDummy").gameObject;
                dummyCamera.transform.localPosition = dummyCamera.transform.localPosition + new Vector3(0, 0, 0.32f);

                CreateAudioSetting();
                staticAudio = AudioMaster.CreateShot(this.gameObject, AudioMaster.SourceType.Custom);
                staticAudio.AssignClip(NorthernLightsBroadcastMain.tvAudioManager.GetClip("static"));
                staticAudio._audioSource.loop = true;
                staticAudio.Stop();
                staticAudio.ApplySettings(audioSetting);

                playerAudio = AudioMaster.CreateShot(this.gameObject, AudioMaster.SourceType.Custom);
                playerAudio.ApplySettings(audioSetting);

                graphicsRaycaster = screenObject.transform.Find("OSD").gameObject.GetComponent<GraphicRaycaster>();

                videoPlayer = screenObject.GetComponent<VideoPlayer>();
                tvplayer = this.gameObject.AddComponent<TVPlayer>();
                ui = this.gameObject.AddComponent<TVUI>();

                thisGearItem = this.gameObject.GetComponent<GearItem>();
                objectGuid = this.gameObject.GetComponent<ObjectGuid>();

                if(objectGuid != null)
                {
                    thisGuid = objectGuid.GetPDID();
                }               

                string tempFolder = SaveLoad.GetFolder(thisGuid);

                if(tempFolder != null && Directory.Exists(tempFolder))
                {
                    ui.currentFolder = tempFolder;
                }

                string tempClip = SaveLoad.GetLastPlayed(thisGuid);

                if (tempClip != null && File.Exists(tempClip))
                {
                    ui.currentClip = tempClip;
                }

                
                if (this.gameObject.name.Contains("GEAR_TV_LCD") || this.gameObject.name.Contains("GEAR_TV_CRT") || this.gameObject.name.Contains("GEAR_TV_WALL"))
                {
                    if (objectGuid == null)
                    {
                        objectGuid = this.gameObject.AddComponent<ObjectGuid>();
                        thisGuid = objectGuid.GetPDID();

                        if (thisGuid == null)
                        {
                            objectGuid.MaybeRuntimeRegister();
                            thisGuid = objectGuid.GetPDID();
                        }
                    }
                    else
                    {
                        thisGuid = objectGuid.GetPDID();
                    }
                }

                isSetup = true;

                if (SaveLoad.GetState(thisGuid) == TVState.Playing)
                {
                    ui.Prepare();
                }
                else
                {
                    SwitchState(SaveLoad.GetState(thisGuid));
                }                
            }                          
        }

        public void Update()
        {
            if (isSetup)
            {
                if (currentState == TVState.Playing)
                {
                    saveTime = videoPlayer.time;

                    if (ui.OSDOpen && TVLock.lockedInTVView)
                    {
                        UpdateTimeText();
                        UpdateTimeSlider();
                    }
                }               
            }    
        }

        public void UpdateTimeText()
        {
            TimeSpan time = TimeSpan.FromSeconds(videoPlayer.time);
            string str = time.ToString(@"hh\:mm\:ss");
            ui.timeText.text = str;
        }

        public void UpdateTimeSlider()
        {            
            ui.progressBar.value = (float)videoPlayer.frame;
            ui.progressBar.minValue = 1;
            ui.progressBar.maxValue = (float)videoPlayer.frameCount;
        }

        [HideFromIl2Cpp]
        public void SavePlaytime()
        {
            if(ui.currentClip == null)
            {
                return;
            }

            FileStuff.AddFrameValueToFile(ui.currentClip, saveTime);
            FileStuff.SaveFrameFile();

            //TimeSpan time = TimeSpan.FromSeconds(videoPlayer.time);
        }

        [HideFromIl2Cpp]
        public double GetPlayTime()
        {
            double savedTime = FileStuff.GetFrameValueFromFile(ui.currentClip);

            if (savedTime <= 0 || savedTime + 1 >= videoPlayer.length)
            {
                savedTime = 0;
            }

            return savedTime;
        }

        [HideFromIl2Cpp]
        public void SwitchState(TVState newState)
        {
            currentState = newState;            

            switch (newState)
            {
                case TVState.Off:
                    ui.screenOff.SetActive(true);
                    ui.screenPlayback480.SetActive(false);
                    ui.screenPlayback720.SetActive(false);
                    ui.screenPlayback1080.SetActive(false);
                    ui.screenStatic.SetActive(false);
                    ui.screenError.SetActive(false);
                    ui.screenLoading.SetActive(false);
                    ui.osdAudio.SetActive(false);
                    ui.osdButtons.SetActive(false);
                    ui.osdFileMenu.SetActive(false);
                    ui.ActivateOSD(false);
                    videoPlayer.Stop();
                    staticAudio.Stop();
                    ambilight.enabled = false;
                    redbutton.Glow(false);
                    break;

                case TVState.Static:
                    ui.screenOff.SetActive(false);
                    ui.screenPlayback480.SetActive(false);
                    ui.screenPlayback720.SetActive(false);
                    ui.screenPlayback1080.SetActive(false);
                    ui.screenStatic.SetActive(true);
                    ui.screenError.SetActive(false);
                    ui.screenLoading.SetActive(false);
                    ui.playButton.gameObject.SetActive(true);
                    ui.pauseButton.gameObject.SetActive(false);
                    videoPlayer.Stop();
                    staticAudio.Play();
                    ui.playingNowText.text = "Stopped";
                    redbutton.Glow(true);
                    ambilight.enabled = false;
                    break;

                case TVState.Paused:
                    ui.screenOff.SetActive(false);                    
                    ui.screenStatic.SetActive(false);
                    ui.screenError.SetActive(false);
                    ui.screenLoading.SetActive(false);
                    ui.screenPlayback1080.SetActive(true);
                    ui.playingNowText.text = "Paused";
                    ui.playButton.gameObject.SetActive(true);
                    ui.pauseButton.gameObject.SetActive(false);
                    videoPlayer.Pause();
                    redbutton.Glow(true);
                    staticAudio.Stop();
                    ambilight.enabled = false;
                    break;

                case TVState.Playing:
                    ui.screenOff.SetActive(false);                    
                    ui.screenStatic.SetActive(false);
                    ui.screenError.SetActive(false);
                    ui.screenLoading.SetActive(false);
                    ui.screenPlayback1080.SetActive(true);
                    ui.SwitchRenderTexture();
                    ui.playButton.gameObject.SetActive(false);
                    ui.pauseButton.gameObject.SetActive(true);
                    ui.ActivateOSD(false);
                    staticAudio.Stop();
                    redbutton.Glow(true);
                    videoPlayer.time = FileStuff.GetFrameValueFromFile(ui.currentClip);
                    videoPlayer.Play();
                    ambilight.enabled = false;
                    SaveLoad.SetLastPlayed(thisGuid, ui.currentClip);
                    break;

                case TVState.Error:
                    ui.errorText.text = errorText;
                    ui.screenOff.SetActive(false);
                    ui.screenStatic.SetActive(false);
                    ui.screenError.SetActive(true);
                    ui.screenLoading.SetActive(false);
                    //ui.screenPlayback1080.SetActive(true);
                    ui.osdAudio.SetActive(false);
                    ui.osdButtons.SetActive(false);
                    ui.osdFileMenu.SetActive(false);
                    redbutton.Glow(true);
                    //ui.ActivateOSD(false);
                    ui.playingNowText.text = "Error";
                    staticAudio.Play();
                    videoPlayer.Stop();
                    ambilight.enabled = false;
                    break;

                    
                case TVState.Preparing:
                    ui.screenOff.SetActive(false);
                    ui.screenStatic.SetActive(true);
                    ui.screenError.SetActive(false);
                    ui.screenLoading.SetActive(true);
                    ui.screenPlayback1080.SetActive(true);
                    redbutton.Glow(true);
                    ui.playingNowText.text = "Loading";
                    ui.osdAudio.SetActive(false);
                    ui.osdButtons.SetActive(false);
                    ui.osdFileMenu.SetActive(false);
                    ui.playButton.gameObject.SetActive(true);
                    ui.pauseButton.gameObject.SetActive(false);
                    //ui.ActivateOSD(false);
                    staticAudio.Stop();
                    videoPlayer.Prepare();
                    ambilight.enabled = false;
                    break;

                case TVState.Resume:                   
                    videoPlayer.Play();
                    redbutton.Glow(true);
                    SwitchState(TVState.Playing);
                    break;

                case TVState.Ended:
                    videoPlayer.Stop();
                    redbutton.Glow(true);
                    saveTime = 0;
                    SavePlaytime();

                    if (Settings.options.playFolder)
                    {                        
                        ui.NextClip();
                    }
                    else
                    {
                        SwitchState(TVState.Static);
                    }
                    break;
            }
        }

        public void OnDestroy()
        {
            SavePlaytime();
            SaveLoad.SetState(thisGuid, currentState);
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