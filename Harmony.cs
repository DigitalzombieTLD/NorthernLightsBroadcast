using MelonLoader;
using UnityEngine;
using Il2CppInterop;
using Il2CppInterop.Runtime.Injection; 
using System.Collections;
using System.Reflection;
using Il2Cpp;
using UnityEngine.Video;
using static NorthernLightsBroadcast.TVManager;

namespace NorthernLightsBroadcast
{
    [HarmonyLib.HarmonyPatch(typeof(StickToGround), "Awake")]
    public class StickToGroundPatch
    {
        public static void Postfix(StickToGround __instance)
        {
            if (__instance.gameObject.name.Contains("OBJ_TelevisionB_LOD0") || __instance.gameObject.name.Contains("OBJ_Television_LOD0"))
            {
                GameObject tvObject = __instance.gameObject;
                bool foundPrefab = false;

                SaveLoad.LoadTheTVs();

                if (tvObject != null)
                {
                    TVManager manager = tvObject.GetComponent<TVManager>();

                    if (manager == null)
                    {
                        tvObject.AddComponent<TVManager>();

                        if (__instance.gameObject.name.Contains("OBJ_TelevisionB_LOD0"))
                        {
                            MeshRenderer originalRenderer = tvObject.GetComponent<MeshRenderer>();
                            originalRenderer.sharedMaterial = NorthernLightsBroadcastMain.TelevisionB_Material_Cutout;
                        }
                    }
                }
            }          
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(GearItem), "Awake")]
    public class tvComponentPatcher
    {
        public static void Postfix(ref GearItem __instance)
        {
            if (__instance.name.Contains("GEAR_TV_LCD") || __instance.name.Contains("GEAR_TV_CRT") || __instance.name.Contains("GEAR_TV_WALL"))
            {
                TVManager tvComponent = __instance.gameObject.GetComponent<TVManager>();
                
                if (tvComponent == null)
                {
                    tvComponent = __instance.gameObject.AddComponent<TVManager>();
                }               
            }
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(GearItem), "Deserialize")]
    public class tvComponentDeserializePatcher
    {
        public static void Postfix(ref GearItem __instance)
        {
            if (__instance.name.Contains("GEAR_TV_LCD") || __instance.name.Contains("GEAR_TV_CRT") || __instance.name.Contains("GEAR_TV_WALL"))
            {
                TVManager tvComponent = __instance.gameObject.GetComponent<TVManager>();

                if (tvComponent != null)
                {
                    ObjectGuid objectGuid = __instance.gameObject.GetComponent<ObjectGuid>();

                    if (objectGuid == null)
                    {
                        tvComponent.objectGuid = tvComponent.gameObject.AddComponent<ObjectGuid>();
                        tvComponent.thisGuid = tvComponent.objectGuid.GetPDID();
                        if (tvComponent.thisGuid == null)
                        {
                            tvComponent.objectGuid.MaybeRuntimeRegister();
                            tvComponent.thisGuid = tvComponent.objectGuid.GetPDID();
                        }                       
                    }
                    else
                    {
                        tvComponent.thisGuid = objectGuid.GetPDID();
                    }

                    if (SaveLoad.GetState(tvComponent.thisGuid) == TVState.Playing)
                    {
                        string tempFolder = SaveLoad.GetFolder(tvComponent.thisGuid);

                        if (tempFolder != null && Directory.Exists(tempFolder))
                        {
                            tvComponent.ui.currentFolder = tempFolder;
                        }

                        string tempClip = SaveLoad.GetLastPlayed(tvComponent.thisGuid);

                        if (tempClip != null && File.Exists(tempClip))
                        {
                            tvComponent.ui.currentClip = tempClip;
                        }

                        tvComponent.ui.Prepare();
                    }
                    else
                    {
                        tvComponent.SwitchState(SaveLoad.GetState(tvComponent.thisGuid));
                    }
                }
            }
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(InterfaceManager), "ShouldEnableMousePointer")]
    public class CursorPatch
    {
        public static void Postfix(InterfaceManager __instance, ref bool __result)
        {
            if(TVLock.lockedInTVView)
            {
                __result = true;
            }
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(PlayerManager), "ShouldSuppressCrosshairs")]
    public class CrosshairPAtch
    {
        public static void Postfix(PlayerManager __instance, ref bool __result)
        {
            if (TVLock.lockedInTVView)
            {
                __result = true;
            }
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(SaveGameSystem), nameof(SaveGameSystem.SaveSceneData))]
    public class SaveCandles
    {
        public static void Postfix(ref SlotData slot)
        {
            SaveLoad.SaveTheTVs();
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(PlayerManager), "AddItemToPlayerInventory")]
    public class tvTurnOffOnStow
    {
        public static void Prefix(ref PlayerManager __instance, ref GearItem gi, ref bool trackItemLooted, ref bool enableNotificationFlag)
        {
            if (gi.name.Contains("GEAR_TV_LCD") || gi.name.Contains("GEAR_TV_CRT") || gi.name.Contains("GEAR_TV_WALL"))
            {
                TVManager tvComponent = gi.gameObject.GetComponent<TVManager>();

                if (tvComponent.currentState != TVState.Off)
                {
                    tvComponent.SavePlaytime();
                    tvComponent.SwitchState(TVState.Off);
                    SaveLoad.SetState(tvComponent.thisGuid, tvComponent.currentState);
                }
            }
        }
    }
}