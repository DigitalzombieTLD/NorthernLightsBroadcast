using MelonLoader;
using UnityEngine;
using Il2CppInterop;
using Il2CppInterop.Runtime.Injection; 
using System.Collections;
using System.Reflection;
using Il2Cpp;

namespace NorthernLightsBroadcast
{
	public static class StreamStuff
	{
        public static bool gotList = false;
        
        public static string indexFileContent;

        public static int globalChance = 50;

        public static List<string> fileURL = new List<string>();
        public static Dictionary<string, int> playbackChance = new Dictionary<string, int>();
        public static Dictionary<string, int> playbackMaxCount = new Dictionary<string, int>();

        public static string indexFileURL = "https://digitalzombie.de/NorthernLightsBroadcast/index";

        private static string _fileURL = "https://digitalzombie.de/NorthernLightsBroadcast/lines.txt";

        public static void GetText()
        {
            string fileContent = new System.Net.WebClient().DownloadString(_fileURL);

            string[] lines = fileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);


            foreach(string line in lines) 
            { 
                MelonLogger.Msg(line);
            }
        }

        public static void GetIndexList()
        {
            try
            {
                indexFileContent = new System.Net.WebClient().DownloadString(indexFileURL);
                
                string[] lines = indexFileContent.Split("#");

                foreach (string line in lines)
                {                   
                    if (!line.Contains("___") && line.Contains("|"))
                    {       
                        string[] splits = new string[3];
                        splits = line.Split("|");
                     
                        fileURL.Add(splits[0]);
                        playbackChance.Add(splits[0], int.Parse(splits[1]));
                        playbackMaxCount.Add(splits[0], int.Parse(splits[2]));
                    }
                }

                indexFileContent = "";          

                if (fileURL.Count > 0)
                {
                    
                    gotList = true;
                }                
            }
            catch
            {
                gotList = false;
            }

         
        }
      
    }
}