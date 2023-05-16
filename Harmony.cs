using MelonLoader;
using UnityEngine;
using Il2CppInterop;
using Il2CppInterop.Runtime.Injection; 
using System.Collections;
using System.Reflection;
using Il2Cpp;
using UnityEngine.Video;

namespace NorthernLightsBroadcast
{
    [HarmonyLib.HarmonyPatch(typeof(StickToGround), "Awake")]
    public class StickToGroundPatch
    {
        public static void Postfix(StickToGround __instance)
        {
            if (__instance.gameObject.name.Contains("OBJ_Television") && (NorthernLightsBroadcastMain.clipNames.Count > 0 || StreamStuff.fileURL.Count > 0))
            {
                GameObject tvObject = __instance.gameObject;
                bool foundPrefab = false;


                if (tvObject != null)
                {
                    if (tvObject.name.Contains("_LOD0"))
                    {
                        if (tvObject.transform.parent.name.Contains("_Prefab"))
                        {
                            tvObject = tvObject.transform.parent.gameObject;
                            foundPrefab = true;
                        }
                        else if (tvObject.transform.parent.transform.parent.name.Contains("_Prefab"))
                        {
                            tvObject = tvObject.transform.parent.transform.parent.transform.gameObject;
                            foundPrefab = true;
                        }
                    }

                    if(!foundPrefab)
                    {
                        return;
                    }

                    TVScreen screen = tvObject.GetComponentInChildren<TVScreen>();

                    if (screen == null)
                    {
                        GameObject newScreen = null;                      

                        if (tvObject.name.Contains("OBJ_TelevisionB_Prefab"))
                        {                        
                            newScreen = UnityEngine.Object.Instantiate(NorthernLightsBroadcastMain.VID_TelevisionB_Prefab, tvObject.transform);
                            newScreen.name = "VID_TelevisionB_Prefab";
                            TVScreen newTV = newScreen.AddComponent<TVScreen>();
                            newTV.Setup();
                            
                        }
                        else if (tvObject.name.Contains("OBJ_TelevisionWall_Prefab"))
                        {
                            newScreen = UnityEngine.Object.Instantiate(NorthernLightsBroadcastMain.VID_TelevisionWall_Prefab, tvObject.transform);
                            newScreen.name = "VID_TelevisionWall_Prefab";
                            TVScreen newTV = newScreen.AddComponent<TVScreen>();
                            newTV.Setup();
                        }
                        else if (tvObject.name.Contains("OBJ_Television_Prefab"))
                        {
                            newScreen = UnityEngine.Object.Instantiate(NorthernLightsBroadcastMain.VID_Television_Prefab, tvObject.transform);
                            newScreen.name = "VID_Television_Prefab";
                            TVScreen newTV = newScreen.AddComponent<TVScreen>();
                            newTV.Setup();
                        }
                    }
                }
            }          
        }
    }

    //InvokeErrorReceivedCallback_Internal(VideoPlayer source, string errorStr)
    [HarmonyLib.HarmonyPatch(typeof(VideoPlayer), "InvokeErrorReceivedCallback_Internal")]
    public class ErrorReceivedPatch
    {
        public static void Postfix(VideoPlayer __instance, ref VideoPlayer source, ref string errorStr)
        {
            EventStuff.ErrorReceived(source, errorStr);
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(VideoPlayer), "InvokeStartedCallback_Internal")]
    public class PlaybackStartedPatch
    {
        public static void Postfix(VideoPlayer __instance, ref VideoPlayer source)
        {
            EventStuff.PlaybackStarted(source);
        }
    }
    

    [HarmonyLib.HarmonyPatch(typeof(VideoPlayer), "InvokePrepareCompletedCallback_Internal")]
    public class PrepareCompletedPatch
    {
        public static void Postfix(VideoPlayer __instance, ref VideoPlayer source)
        {
            EventStuff.PrepareCompleted(source);
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(VideoPlayer), "InvokeLoopPointReachedCallback_Internal")]
    public class EndReachedPatch
    {
        public static void Postfix(VideoPlayer __instance, ref VideoPlayer source)
        {
            EventStuff.EndReached(source);
        }
    }
}