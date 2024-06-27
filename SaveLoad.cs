using MelonLoader;
using UnityEngine;
using System.Collections;
using Il2Cpp;
using NLBIni;


using ModData;
using Il2CppNewtonsoft.Json.Linq;
using Il2CppMono;

namespace NorthernLightsBroadcast
{
	public static class SaveLoad
	{
        public static IniDataParser iniDataParser;
        public static IniData thisIniData;
        public static ModDataManager dataManager;
        public static string moddataString;
        public static bool reloadPending = true;

        public static void LoadTheTVs()
		{
            if (reloadPending)
            {
                MelonLogger.Msg("Loading TV data");
                iniDataParser = new IniDataParser();
                thisIniData = new IniData();
                dataManager = new ModDataManager("NorthernLightsBroadcast", false);

                iniDataParser.Configuration.AllowCreateSectionsOnFly = true;
                iniDataParser.Configuration.SkipInvalidLines = true;
                iniDataParser.Configuration.OverrideDuplicateKeys = true;
                iniDataParser.Configuration.AllowDuplicateKeys = false;

                moddataString = dataManager.Load();
                thisIniData = iniDataParser.Parse(moddataString);

                reloadPending = false;
            }
        }      

        public static void MaybeAddTV(string TVID)
        {
            if (!thisIniData.Sections.ContainsSection(TVID))
            {
                thisIniData.Sections.AddSection(TVID);
            }
        }

        public static string GetFolder(string TVID)
        {
            if(thisIniData == null)
            {
                return UnityEngine.Application.dataPath;
            }

            if (TVID == null || !thisIniData[TVID].ContainsKey("currentFolder"))
            {
                return UnityEngine.Application.dataPath;
            }

            if (!Directory.Exists(thisIniData[TVID]["currentFolder"]))
            {
                return UnityEngine.Application.dataPath;
            }

            return thisIniData[TVID]["currentFolder"];
        }

        public static void SetFolder(string TVID, string folder)
        {
            if (thisIniData != null && TVID != "")
            {
                thisIniData[TVID].AddKey("currentFolder");
                thisIniData[TVID]["currentFolder"] = folder;
            }
        }

        public static float GetVolume(string TVID)
        {
            if (TVID == null || !thisIniData[TVID].ContainsKey("volume"))
            {
                return 0.5f;
            }
            
            return float.Parse(thisIniData[TVID]["volume"]);
        }

        public static void SetVolume(string TVID, float volume)
        {
            if (thisIniData != null && TVID != "")
            {
                thisIniData[TVID].AddKey("volume");
                thisIniData[TVID]["volume"] = volume.ToString();
            }
        }


        public static string GetLastPlayed(string TVID)
		{
            if (TVID == null || !thisIniData[TVID].ContainsKey("lastPlayed"))
            {
                return "";
            }

            return thisIniData[TVID]["lastPlayed"];
        }

        public static void SetLastPlayed(string TVID, string lastPlayed)
        {
            if (thisIniData != null && TVID != "")
            {
                thisIniData[TVID].AddKey("lastPlayed");
                thisIniData[TVID]["lastPlayed"] = lastPlayed;
            }
        }

        public static TVManager.TVState GetState(string TVID)
		{
            if (TVID == null || !thisIniData[TVID].ContainsKey("state"))
            {
                return TVManager.TVState.Off;
            }
            
            return (TVManager.TVState)System.Enum.Parse(typeof(TVManager.TVState), thisIniData[TVID]["state"]);            
            }      

        public static void SetState(string TVID, TVManager.TVState state)
        {  
            if(thisIniData != null && TVID != "")            
            {
                thisIniData[TVID].AddKey("state");
                thisIniData[TVID]["state"] = state.ToString();
            }            
        }

        public static void SaveTheTVs()
		{
            dataManager.Save(thisIniData.ToString());
        }
	}
}