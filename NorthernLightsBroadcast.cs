using MelonLoader;
using UnityEngine;
using Il2CppInterop;
using Il2CppInterop.Runtime.Injection; 
using System.Collections;
using System.Reflection;
using Il2Cpp;
using AudioMgr;

namespace NorthernLightsBroadcast
{
	public class NorthernLightsBroadcastMain : MelonMod
	{
        public static AssetBundle assetBundle;
        public static GameObject VID_Television_Prefab;
        public static GameObject VID_TelevisionB_Prefab;
        public static GameObject VID_TelevisionWall_Prefab;
        public static ClipManager tvAudioManager;

        public static string videoPath = Application.dataPath + @"/../Mods/NorthernLightsBroadcast/";
        public static RaycastHit hit;
        public static int layerMask = 0;
        public static List<string> clipNames = new List<string>();

        public override void OnInitializeMelon()
		{
            LoadEmbeddedAssetBundle();
          
            layerMask |= 1 << 17; // gear layer
            layerMask |= 1 << 19; // InteractiveProp layer

            if (!Directory.Exists(videoPath))
            {
                Directory.CreateDirectory(videoPath);                
               
            }

            FileStuff.OpenFrameFile();

            string[] allTheFilesInDir = Directory.GetFiles(videoPath, "*", SearchOption.TopDirectoryOnly);

            foreach (string singleFile in allTheFilesInDir)
            {
                clipNames.Add(Path.GetFileName(singleFile));
            }

            Settings.OnLoad();


            if (!Settings.options.disableStronks)
            {
                StreamStuff.GetIndexList();
                OtherStuff.OpenCountFile();           
            }           
        }       

        public override void OnUpdate()
		{
            if(clipNames.Count > 0 || StreamStuff.fileURL.Count > 0)
            {
                if (InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.interactButton))
                {
                    if (Physics.Raycast(GameManager.GetMainCamera().transform.position, GameManager.GetMainCamera().transform.TransformDirection(Vector3.forward), out hit, 3f, layerMask))
                    {
                        GameObject hitObject = hit.collider.gameObject;
                        string hitObjectName = hitObject.name;

                        if (hitObjectName == "VID_Television_Prefab" || hitObjectName == "VID_TelevisionB_Prefab" || hitObjectName == "VID_TelevisionWall_Prefab")
                        {
                            TVScreen foundScreen = hitObject.transform.GetComponent<TVScreen>();

                            foundScreen.Toggle();
                        }
                    }
                }
            }
        }

        public static void LoadEmbeddedAssetBundle()
        {
            MemoryStream memoryStream;
            System.IO.Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NorthernLightsBroadcast.Resources.NorthernLightsBroadcast");
            memoryStream = new MemoryStream((int)stream.Length);
            stream.CopyTo(memoryStream);

            assetBundle = AssetBundle.LoadFromMemory(memoryStream.ToArray());

            VID_Television_Prefab = assetBundle.LoadAsset<GameObject>("VID_Television_Prefab");
            VID_Television_Prefab.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(VID_Television_Prefab);

            VID_TelevisionB_Prefab = assetBundle.LoadAsset<GameObject>("VID_TelevisionB_Prefab");
            VID_TelevisionB_Prefab.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(VID_TelevisionB_Prefab);

            VID_TelevisionWall_Prefab = assetBundle.LoadAsset<GameObject>("VID_TelevisionWall_Prefab");
            VID_TelevisionWall_Prefab.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(VID_TelevisionWall_Prefab);

            tvAudioManager = AudioMaster.NewClipManager();

            tvAudioManager.LoadClipFromBundle("click", "audiobuttonclick", assetBundle);
            tvAudioManager.LoadClipFromBundle("static", "audiotvstatic", assetBundle);
        }

    }
}