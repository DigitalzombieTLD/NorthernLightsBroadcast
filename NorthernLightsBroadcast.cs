using MelonLoader;
using UnityEngine;
using Il2CppInterop;
using Il2CppInterop.Runtime.Injection; 
using System.Collections;
using System.Reflection;
using Il2Cpp;
using AudioMgr;
using System.Runtime.InteropServices;
using UnityEngine.EventSystems;
using MelonLoader.Utils;
using static Il2Cpp.CarcassSite;


namespace NorthernLightsBroadcast
{
	public class NorthernLightsBroadcastMain : MelonMod
	{
        public static AssetBundle assetBundle;

        public static GameObject NLB_TV_CRT;
        public static GameObject NLB_TV_LCD;
        public static GameObject NLB_TV_WALL;
        public static Material TelevisionB_Material_Cutout;

        public static ClipManager tvAudioManager;

        public static string videoPath = UnityEngine.Application.dataPath + @"/../Mods/audio/";
        public static RaycastHit hit;
        public static int layerMask = 0;

        public static Sprite folderIconSprite;
        public static Sprite audioIconSprite;
        public static Sprite videoIconSprite;

        public static Texture2D folderIcon;
        public static Texture2D audioIcon;
        public static Texture2D videoIcon;

        public static GameObject eventSystemObject;
        public static EventSystem eventSystem;
        public static StandaloneInputModule standaloneInputModule;        
        public static Camera eventCam;

        public override void OnInitializeMelon()
		{
            LoadEmbeddedAssetBundle();

            layerMask |= 1 << 0; // Default layer
            layerMask |= 1 << 17; // gear layer
            layerMask |= 1 << 19; // InteractiveProp layer
           
            FileStuff.OpenFrameFile();
            Settings.OnLoad();            
        }


        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            MelonLogger.Msg("SCENE LAODED: " + sceneName);
            TweenFactory.SceneManagerSceneLoaded();
            if ((!sceneName.Contains("Empty") && !sceneName.Contains("Boot") && !sceneName.Contains("MainMenu")))
            {
                if (!eventSystem)
                {
                    eventCam = GameManager.GetMainCamera();

                    eventSystemObject = new GameObject("EventSystem");
                    eventSystem = eventCam.gameObject.AddComponent<EventSystem>();
                    standaloneInputModule = eventCam.gameObject.AddComponent<StandaloneInputModule>();
                }
            }

            if (sceneName.Contains("MainMenu"))
            {
                SaveLoad.reloadPending = true;
            }

            if ((!sceneName.Contains("Empty") && !sceneName.Contains("Boot") && !sceneName.Contains("MainMenu")))
            {
                SaveLoad.LoadTheTVs();
            }
        }

        public override void OnUpdate()
		{
            if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactButton))
            {
                if (Physics.Raycast(GameManager.GetMainCamera().transform.position, GameManager.GetMainCamera().transform.TransformDirection(Vector3.forward), out hit, 2f, layerMask))
                {
                    GameObject hitObject = hit.collider.gameObject;
                    string hitObjectName = hitObject.name;

                    if (hitObjectName == "PowerButton")
                    {
                        TVButton foundButton = hitObject.transform.GetComponent<TVButton>();

                        if (foundButton != null)
                        {
                            foundButton.TogglePower();
                        }
                    }
                }            
            }
            
            if (TVLock.lockedInTVView)
            {
                TVLock.currentManager.objectRenderer.enabled = true;

                if (InputManager.GetKeyDown(InputManager.m_CurrentContext, KeyCode.Escape) || InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactButton))
                {
                    TVLock.ExitTVView();
                }
            }
            else if (!TVLock.lockedInTVView && InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactButton))
            {
                if (Physics.Raycast(GameManager.GetMainCamera().transform.position, GameManager.GetMainCamera().transform.TransformDirection(Vector3.forward), out hit, 2f, layerMask))
                {
                    GameObject hitObject = hit.collider.gameObject;
                    string hitObjectName = hitObject.name;

                    if (hitObjectName.Contains("OBJ_TelevisionB_LOD0") || hitObjectName.Contains("OBJ_Television_LOD0") || hitObjectName.Contains("GEAR_TV_LCD") || hitObjectName.Contains("GEAR_TV_CRT") || hitObjectName.Contains("GEAR_TV_WALL"))
                    {
                        TVManager foundManager = hitObject.transform.GetComponent<TVManager>();

                        if (foundManager != null)
                        {
                            TVLock.EnterTVView(foundManager);
                        }
                    }
                }
            }
           
        }

        public static void LoadEmbeddedAssetBundle()
        {
            MemoryStream memoryStream;
            System.IO.Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NorthernLightsBroadcast.Resources.northernlightsbroadcastbundle");
            memoryStream = new MemoryStream((int)stream.Length);
            stream.CopyTo(memoryStream);

            assetBundle = AssetBundle.LoadFromMemory(memoryStream.ToArray());           

            NLB_TV_CRT = assetBundle.LoadAsset<GameObject>("NLB_TV_CRT");
            NLB_TV_CRT.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(NLB_TV_CRT);

            NLB_TV_LCD = assetBundle.LoadAsset<GameObject>("NLB_TV_LCD");
            NLB_TV_LCD.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(NLB_TV_LCD);

            NLB_TV_WALL = assetBundle.LoadAsset<GameObject>("NLB_TV_WALL");
            NLB_TV_WALL.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(NLB_TV_WALL);

            TelevisionB_Material_Cutout = assetBundle.LoadAsset<Material>("MaterialTelevisionB");
            TelevisionB_Material_Cutout.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(TelevisionB_Material_Cutout); 
            
            folderIcon = assetBundle.LoadAsset<Texture2D>("icon_folder");            
            folderIconSprite = Sprite.Create(folderIcon, new Rect(0, 0, folderIcon.width, folderIcon.height), new Vector2(0.5f, 0.5f));            
            folderIconSprite.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(folderIconSprite);

            audioIcon = assetBundle.LoadAsset<Texture2D>("icon_audio");
            audioIconSprite = Sprite.Create(audioIcon, new Rect(0, 0, audioIcon.width, audioIcon.height), new Vector2(0.5f, 0.5f));
            audioIconSprite.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(audioIconSprite);

            videoIcon = assetBundle.LoadAsset<Texture2D>("icon_video");
            videoIconSprite = Sprite.Create(videoIcon, new Rect(0, 0, videoIcon.width, videoIcon.height), new Vector2(0.5f, 0.5f));
            videoIconSprite.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(videoIconSprite);

            tvAudioManager = AudioMaster.NewClipManager();

            tvAudioManager.LoadClipFromBundle("click", "audiobuttonclick", assetBundle);
            tvAudioManager.LoadClipFromBundle("static", "audiotvstatic", assetBundle);
        }

    }
}