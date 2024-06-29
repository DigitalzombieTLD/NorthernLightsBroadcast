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
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Slider = UnityEngine.UI.Slider;
using Image = UnityEngine.UI.Image;

namespace NorthernLightsBroadcast
{
    [RegisterTypeInIl2Cpp]
    public class TVUI : MonoBehaviour
    {
        public Canvas canvas;
        public CanvasGroup canvasGroup;
        public TVManager manager;
        public bool isSetup = false;

        public GameObject screenPlayback;

        public GameObject screenOff;
        public GameObject screenStatic;
        public GameObject screenError;
        public GameObject screenLoading;

        public GameObject osdAudio;
        public GameObject osdButtons;
        public GameObject osdFileMenu;

        public GameObject OSD;
        public bool OSDOpen = false;
        public bool isFading = false;

        public bool fileBrowserOpen = false;

        // Audio
        public Slider audioSlider;
        public Button muteButton;

        // Buttons
        public Button playButton;
        public Button pauseButton;
        public Button stopButton;
        public Button nextButton;
        public Button prevButton;
        public Button fileBrowserButton;
        public Button uiActivator;
        public Slider progressBar;

        // Pagestuff
        public Button pageNext;
        public Button pagePrev;

        public TextMeshProUGUI playingNowText;
        public TextMeshProUGUI timeText;
        public TextMeshProUGUI currentDir;
        public TextMeshProUGUI errorText;
        public TextMeshProUGUI pageText;

        // File Browser
        public Button fileBrowserUpButton;

        public Button[] listButtons = new Button[8];
        public TextMeshProUGUI[] listText = new TextMeshProUGUI[8];
        public TextMeshProUGUI[] listLength = new TextMeshProUGUI[8];
        public Image[] listSprites = new Image[8];
        public GameObject[] driveButtonObjects = new GameObject[8];
        public Button[] driveButtons = new Button[8];

        public string currentFolder = UnityEngine.Application.dataPath + @"/../Mods";
        public string currentClip;
        public int currentClipIndex = 0;

        //public string[] filesInCurrentFolder;
        public Dictionary<string, bool> folderContents = new Dictionary<string, bool>(); // bool isFolder

        public int currentPage = 0;
        public int currentPageCount = 1;

        new Color32 folderColor = new Color32(90, 187, 248, 255);
        new Color32 audioColor = new Color32(255, 226, 115, 255);
        new Color32 videoColor = new Color32(112, 217, 81, 255);

        public TVUI(IntPtr intPtr) : base(intPtr)
        {
        }

        public void Awake()
        {
            if(isSetup)
            {
                return;
            }

            manager = this.gameObject.GetComponent<TVManager>();

            screenPlayback = this.transform.Find("NLB_TV/Screens/ScreenPlaybackMesh").gameObject;

            screenOff = this.transform.Find("NLB_TV/Screens/ScreenOff").gameObject;
            screenStatic = this.transform.Find("NLB_TV/Screens/ScreenStatic").gameObject;
            screenLoading = this.transform.Find("NLB_TV/Screens/ScreenLoading").gameObject;

            screenError = this.gameObject.transform.Find("NLB_TV/Screens/ScreenError").gameObject;
            errorText = this.gameObject.transform.Find("NLB_TV/Screens/ScreenError/ErrorWindow/Message").GetComponent<TextMeshProUGUI>();
            
            OSD = this.gameObject.transform.Find("NLB_TV/OSD").gameObject;
            canvas = OSD.GetComponent<Canvas>();
            canvasGroup = OSD.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            
            osdAudio = OSD.transform.Find("Audio").gameObject;
            osdButtons = OSD.transform.Find("Buttons").gameObject;
            osdFileMenu = OSD.transform.Find("FileMenu").gameObject;
            osdFileMenu.SetActive(false);

            uiActivator = OSD.transform.Find("UIActivator").GetComponent<Button>();
            uiActivator.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(new Action(delegate { this.OSDActivationButton(); })));

            pageNext = osdFileMenu.transform.Find("Pagestuff/ButtonRight").GetComponent<Button>();
            pageNext.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(new Action(delegate { this.NextPage(); })));
        
            pagePrev = osdFileMenu.transform.Find("Pagestuff/ButtonLeft").GetComponent<Button>();
            pagePrev.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(new Action(delegate { this.PrevPage(); })));
            
            pageText = osdFileMenu.transform.Find("Pagestuff/Pagenumber").GetComponent<TextMeshProUGUI>();
            pageText.text = "1 / 1";

            currentDir = osdFileMenu.transform.Find("CurrentDir").GetComponent<TextMeshProUGUI>();
            currentDir.text = currentFolder;

            fileBrowserUpButton = osdFileMenu.transform.Find("DirUp").GetComponent<Button>();
            fileBrowserUpButton.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(new Action(delegate { this.UpDir(); })));


            audioSlider = osdAudio.transform.Find("Slider").GetComponent<Slider>();
            audioSlider.value = SaveLoad.GetVolume(manager.thisGuid);
            manager.playerAudio._audioSource.volume = audioSlider.value;
            manager.staticAudio._audioSource.volume = audioSlider.value;

            audioSlider.onValueChanged.AddListener(DelegateSupport.ConvertDelegate<UnityAction<float>>(new Action<float>(delegate (float value) { VolumeSlider(); })));
            
            muteButton = osdAudio.transform.Find("Mute").GetComponent<Button>();           
            muteButton.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(new Action(delegate { this.Mute(); })));


            playingNowText = osdButtons.transform.Find("PlayingNow").GetComponent<TextMeshProUGUI>();
            timeText = osdButtons.transform.Find("Time").GetComponent<TextMeshProUGUI>();
            timeText.text = "00:00:00";
            playingNowText.text = "Stopped";

            playButton = osdButtons.transform.Find("PlayButtons/Play").GetComponent<Button>();
            playButton.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(new Action(delegate { this.Prepare(); })));
           
            pauseButton = osdButtons.transform.Find("PlayButtons/Pause").GetComponent<Button>(); 
            pauseButton.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(new Action(delegate { this.Pause(); })));
            stopButton = osdButtons.transform.Find("PlayButtons/Stop").GetComponent<Button>();
            stopButton.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(new Action(delegate { this.Stop(); })));
            nextButton = osdButtons.transform.Find("PlayButtons/Next").GetComponent<Button>();        
            nextButton.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(new Action(delegate { this.NextClip(); })));
            prevButton = osdButtons.transform.Find("PlayButtons/Prev").GetComponent<Button>();
            prevButton.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(new Action(delegate { this.PrevClip(); })));
            
            fileBrowserButton = osdButtons.transform.Find("PlayButtons/Browser").GetComponent<Button>();            
            fileBrowserButton.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(new Action(delegate { this.FileMenu(); })));

            
            
            progressBar = osdButtons.transform.Find("ProgressBar/Slider").GetComponent<Slider>();
            progressBar.value = 0f;
            progressBar.onValueChanged.AddListener(DelegateSupport.ConvertDelegate<UnityAction<float>>(new Action<float>(delegate (float value) { ProgressBar(); })));
            //rogressBar.OnPointerUp.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(new Action(delegate { ProgressBar(); })));

            listButtons[0] = osdFileMenu.transform.Find("ContentFilelist/Line1").GetComponent<Button>();              
            listButtons[0].onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(new Action(delegate { this.ItemButtom(0); })));
            listButtons[1] = osdFileMenu.transform.Find("ContentFilelist/Line2").GetComponent<Button>();
            listButtons[1].onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(new Action(delegate { this.ItemButtom(1); })));
            listButtons[2] = osdFileMenu.transform.Find("ContentFilelist/Line3").GetComponent<Button>();
            listButtons[2].onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(new Action(delegate { this.ItemButtom(2); })));
            listButtons[3] = osdFileMenu.transform.Find("ContentFilelist/Line4").GetComponent<Button>();
            listButtons[3].onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(new Action(delegate { this.ItemButtom(3); })));
            listButtons[4] = osdFileMenu.transform.Find("ContentFilelist/Line5").GetComponent<Button>();
            listButtons[4].onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(new Action(delegate { this.ItemButtom(4); })));
            listButtons[5] = osdFileMenu.transform.Find("ContentFilelist/Line6").GetComponent<Button>();
            listButtons[5].onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(new Action(delegate { this.ItemButtom(5); })));
            listButtons[6] = osdFileMenu.transform.Find("ContentFilelist/Line7").GetComponent<Button>();
            listButtons[6].onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(new Action(delegate { this.ItemButtom(6); })));
            listButtons[7] = osdFileMenu.transform.Find("ContentFilelist/Line8").GetComponent<Button>();
            listButtons[7].onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(new Action(delegate { this.ItemButtom(7); })));
         
            listSprites[0] = osdFileMenu.transform.Find("ContentFilelist/Line1/Icon").GetComponent<Image>();
            listSprites[1] = osdFileMenu.transform.Find("ContentFilelist/Line2/Icon").GetComponent<Image>();
            listSprites[2] = osdFileMenu.transform.Find("ContentFilelist/Line3/Icon").GetComponent<Image>();
            listSprites[3] = osdFileMenu.transform.Find("ContentFilelist/Line4/Icon").GetComponent<Image>();
            listSprites[4] = osdFileMenu.transform.Find("ContentFilelist/Line5/Icon").GetComponent<Image>();
            listSprites[5] = osdFileMenu.transform.Find("ContentFilelist/Line6/Icon").GetComponent<Image>();
            listSprites[6] = osdFileMenu.transform.Find("ContentFilelist/Line7/Icon").GetComponent<Image>();
            listSprites[7] = osdFileMenu.transform.Find("ContentFilelist/Line8/Icon").GetComponent<Image>();
        
            listText[0] = osdFileMenu.transform.Find("ContentFilelist/Line1/Text").GetComponent<TextMeshProUGUI>();
            listText[1] = osdFileMenu.transform.Find("ContentFilelist/Line2/Text").GetComponent<TextMeshProUGUI>();
            listText[2] = osdFileMenu.transform.Find("ContentFilelist/Line3/Text").GetComponent<TextMeshProUGUI>();
            listText[3] = osdFileMenu.transform.Find("ContentFilelist/Line4/Text").GetComponent<TextMeshProUGUI>();
            listText[4] = osdFileMenu.transform.Find("ContentFilelist/Line5/Text").GetComponent<TextMeshProUGUI>();
            listText[5] = osdFileMenu.transform.Find("ContentFilelist/Line6/Text").GetComponent<TextMeshProUGUI>();
            listText[6] = osdFileMenu.transform.Find("ContentFilelist/Line7/Text").GetComponent<TextMeshProUGUI>();
            listText[7] = osdFileMenu.transform.Find("ContentFilelist/Line8/Text").GetComponent<TextMeshProUGUI>();

            listLength[0] = osdFileMenu.transform.Find("ContentTime/Line1/Text").GetComponent<TextMeshProUGUI>();
            listLength[0].text = " ";
            listLength[1] = osdFileMenu.transform.Find("ContentTime/Line2/Text").GetComponent<TextMeshProUGUI>();
            listLength[1].text = " ";
            listLength[2] = osdFileMenu.transform.Find("ContentTime/Line3/Text").GetComponent<TextMeshProUGUI>();
            listLength[2].text = " ";
            listLength[3] = osdFileMenu.transform.Find("ContentTime/Line4/Text").GetComponent<TextMeshProUGUI>();
            listLength[3].text = " ";
            listLength[4] = osdFileMenu.transform.Find("ContentTime/Line5/Text").GetComponent<TextMeshProUGUI>();
            listLength[4].text = "   ";
            listLength[5] = osdFileMenu.transform.Find("ContentTime/Line6/Text").GetComponent<TextMeshProUGUI>();
            listLength[5].text = "  ";
            listLength[6] = osdFileMenu.transform.Find("ContentTime/Line7/Text").GetComponent<TextMeshProUGUI>();
            listLength[6].text = "  ";
            listLength[7] = osdFileMenu.transform.Find("ContentTime/Line8/Text").GetComponent<TextMeshProUGUI>();
            listLength[7].text = "  ";
          

            DriveInfo[] allDrives = DriveInfo.GetDrives();

            for(int i = 0; i < 8; i++)
            {
                driveButtonObjects[i] = osdFileMenu.transform.Find("ContentDrivelist/Line" + (i + 1)).gameObject;
                if(i < allDrives.Length)
                {
                    driveButtons[i] = driveButtonObjects[i].GetComponent<Button>();
                    driveButtonObjects[i].SetActive(true);
                    string driveletter = allDrives[i].Name.ToString();
                    driveButtons[i].onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(new Action(delegate { this.DriveButton(driveletter); })));                  
                    driveButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = driveletter;
                }
                else
                {
                    driveButtonObjects[i].SetActive(false);
                }
            }

            isSetup = true;
        }

        
        [HideFromIl2Cpp]
        public void PopulateFiles()
        {
            string[] files = FileStuff.GetFilesInPath(currentFolder);
            string[] dirs = FileStuff.GetFoldersInPath(currentFolder);

            SaveLoad.SetFolder(manager.thisGuid, currentFolder);

            folderContents = new Dictionary<string, bool>();

            foreach (string dir in dirs)
            {
                folderContents.Add(dir, true);
            }

            foreach (string file in files)
            {
                folderContents.Add(file, false);
            }
            
            currentPageCount = (int)Math.Ceiling((double)folderContents.Count / 8);
            
            // Count through "lines"
            for(int i = 0; i < 8; i++)
            {
                if(i + (currentPage * 8) < folderContents.Count)
                {
                    listButtons[i].gameObject.active = true;
                    listText[i].text = Path.GetFileName(folderContents.ElementAt(i + (currentPage * 8)).Key);

                    if(folderContents.ElementAt(i + (currentPage * 8)).Value)
                    {
                        listText[i].color = folderColor;
                        listSprites[i].sprite = NorthernLightsBroadcastMain.folderIconSprite;
                        //listSprites[i].UpdateMaterial();
                    }
                    else
                    {
                        listText[i].color = videoColor;
                        listSprites[i].sprite = NorthernLightsBroadcastMain.videoIconSprite;
                        //listSprites[i].UpdateMaterial();
                    }
                }
                else
                {                    
                    listText[i].text = "";
                    listSprites[i].sprite = null;
                    listButtons[i].gameObject.active = false;
                }
            }
        }

        [HideFromIl2Cpp]
        public IEnumerator FadeIn(float speed)
        {
            //yield return new WaitForSeconds(waitTime);
            isFading = true;
            float startAlpha = 0f;
            float endAlpha = 1f;

            System.Action<ITween<float>> updateFadeIn = (t) =>
            {
                canvasGroup.alpha = t.CurrentValue;
            };

            System.Action<ITween<float>> fadeInCompleted = (t) =>
            {
                canvasGroup.alpha = endAlpha;
                isFading = false;
            };

            canvasGroup.gameObject.Tween(canvasGroup.gameObject, startAlpha, endAlpha, speed, TweenScaleFunctions.SineEaseInOut, updateFadeIn, fadeInCompleted);

            yield return null;
        }

        [HideFromIl2Cpp]
        public IEnumerator FadeOut(float speed)
        {
            //yield return new WaitForSeconds(waitTime);
            isFading = true;
            float startAlpha = 1f;
            float endAlpha = 0f;

            System.Action<ITween<float>> updateFadeOut = (t) =>
            {
                canvasGroup.alpha = t.CurrentValue;
            };

            System.Action<ITween<float>> fadeOutCompleted = (t) =>
            {
                canvasGroup.alpha = endAlpha;
                isFading = false;

                osdAudio.SetActive(false);
                osdFileMenu.SetActive(false);                               
                fileBrowserOpen = false;
            };

            canvasGroup.gameObject.Tween(canvasGroup.gameObject, startAlpha, endAlpha, speed, TweenScaleFunctions.SineEaseInOut, updateFadeOut, fadeOutCompleted);

            yield return null;
        }


        [HideFromIl2Cpp]
        public void ActivateOSD(bool value)
        {
            if (value)
            {
                OSDOpen = true;
                osdAudio.SetActive(true);
                osdButtons.SetActive(true);
                osdFileMenu.SetActive(false);
                fileBrowserOpen = false;

                MelonCoroutines.Start(FadeIn(0.5f));
            }
            else
            {
                OSDOpen = false;
                MelonCoroutines.Start(FadeOut(0.5f));             
            }       
        }


        [HideFromIl2Cpp]
        public void OSDActivationButton()
        {
            if (manager.currentState != TVManager.TVState.Off && !isFading)
            {
                if (manager.currentState == TVManager.TVState.Error)
                {
                    manager.SwitchState(TVManager.TVState.Static);
                }

            ActivateOSD(!OSDOpen);
            }       
        }


        [HideFromIl2Cpp]
        public void UpdatePage()
        {
            PopulateFiles();
            currentDir.text = currentFolder;            
            int tempPage = currentPage + 1;
            pageText.text = tempPage + " / " + currentPageCount;
        }


        [HideFromIl2Cpp]
        public void ItemButtom(int button)
        {
            if(folderContents.ElementAt(button + (currentPage * 8)).Value)
            {
                currentFolder = folderContents.ElementAt(button + (currentPage * 8)).Key;
                currentPage = 0;
                UpdatePage();
            }
            else
            {
                currentClip = folderContents.ElementAt(button + (currentPage * 8)).Key;
                currentClipIndex = button + (currentPage * 8);                
                Prepare();
            }
        }


        [HideFromIl2Cpp]
        public void DriveButton(string driveletter)
        {
            currentFolder = driveletter;
            currentPage = 0;
            manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
            UpdatePage();
        }

        [HideFromIl2Cpp]
        public void Resume()
        {
            manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
            playingNowText.text = Path.GetFileName(currentClip);
            manager.videoPlayer.url = currentClip;
            manager.SwitchState(TVManager.TVState.Resume);
            screenLoading.SetActive(true);
        }

        [HideFromIl2Cpp]
        public void Prepare()
        {
            if(currentClip == null)
            {
                return;
            }

            if(manager.currentState == TVManager.TVState.Paused)
            {               
                manager.SwitchState(TVManager.TVState.Resume);               
                return;
            }

            manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
            playingNowText.text = Path.GetFileName(currentClip);
            manager.videoPlayer.url = currentClip;
            manager.SwitchState(TVManager.TVState.Preparing);
            screenLoading.SetActive(true);
        }

        [HideFromIl2Cpp]
        public void Stop()
        {
            manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click")); 
            manager.SavePlaytime();
            manager.SwitchState(TVManager.TVState.Static);
        }

        [HideFromIl2Cpp]
        public void Pause()
        {
            manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
            manager.SavePlaytime();
            manager.SwitchState(TVManager.TVState.Paused);
        }

        [HideFromIl2Cpp]
        public void NextClip()
        {
            if(currentClipIndex < folderContents.Count - 1 && !folderContents.ElementAt(currentClipIndex + 1).Value)
            {
                currentClipIndex++;
                currentClip = folderContents.ElementAt(currentClipIndex).Key;
                manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
                Prepare();
            }
            else
            {
                if(Settings.options.loopFolder)
                {
                    currentClipIndex = 0;
                    currentClip = folderContents.ElementAt(currentClipIndex).Key;
                    manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
                    Prepare();
                }
                else
                {
                    manager.SwitchState(TVManager.TVState.Static);
                }
            }
        }

        [HideFromIl2Cpp]
        public void PrevClip()
        {
            if(manager.currentState == TVManager.TVState.Playing)
            {
                if(manager.videoPlayer.time > 5)
                {
                    manager.videoPlayer.time = 0;
                    manager.saveTime = 0;
                    manager.SavePlaytime();
                }
                else
                {
                    if (currentClipIndex > 0 && !folderContents.ElementAt(currentClipIndex - 1).Value)
                    {
                        currentClipIndex--;
                        currentClip = folderContents.ElementAt(currentClipIndex).Key;
                        manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
                        Prepare();
                    }
                }
            }

            if (currentClipIndex > 0 && !folderContents.ElementAt(currentClipIndex - 1).Value)
            {
                currentClipIndex--;
                currentClip = folderContents.ElementAt(currentClipIndex).Key;
                manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
                Prepare();
            }
        }

        [HideFromIl2Cpp]
        public void FileMenu()
        {
            manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));

            if (fileBrowserOpen)
            {
                osdFileMenu.SetActive(false);
                fileBrowserOpen = false;
            }
            else
            {
                osdFileMenu.SetActive(true);
                UpdatePage();
                fileBrowserOpen = true;
            }
        }

        [HideFromIl2Cpp]
        public void UpDir()
        {
            var tempParent = System.IO.Directory.GetParent(currentFolder);
            if (tempParent == null)
            {
                return;
            }

            currentFolder = System.IO.Directory.GetParent(currentFolder).FullName;
            currentPage = 0;
            manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
            UpdatePage();
        }

        [HideFromIl2Cpp]
        public void Mute()
        {
            manager.playerAudio._audioSource.mute = !manager.playerAudio._audioSource.mute;
            manager.staticAudio._audioSource.mute = !manager.staticAudio._audioSource.mute;
        }

        [HideFromIl2Cpp]
        public void NextPage()
        {
            if(currentPage < currentPageCount - 1)
            {
                currentPage++;
                manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
                UpdatePage();
            }
        }

        [HideFromIl2Cpp]
        public void PrevPage()
        {
            if(currentPage > 0)
            {
                currentPage--;
                manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
                UpdatePage();
            }
        }

        [HideFromIl2Cpp]
        public void VolumeSlider()
        {
            manager.redbutton.tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
            manager.playerAudio.SetVolume(audioSlider.value);
            manager.staticAudio.SetVolume(audioSlider.value);
            //manager.redbutton.tvClickShot.SetVolume(audioSlider.value);
            SaveLoad.SetVolume(manager.thisGuid, audioSlider.value);
        }

        [HideFromIl2Cpp]
        public void ProgressBar()
        {
            //MelonLogger.Msg("ProgressBar pressed!!!!!!");
        }

        public void Update()
        {
          
        }
    }
}