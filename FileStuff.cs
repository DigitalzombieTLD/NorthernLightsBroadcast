using MelonLoader;
using UnityEngine;
using Il2CppInterop;
using Il2CppInterop.Runtime.Injection; 
using System.Collections;
using System.Reflection;
using Il2Cpp;

namespace NorthernLightsBroadcast
{
	public static class FileStuff
	{
        public static string settingsFile = Application.dataPath + @"/../Mods/NorthernLightsBroadcast_ResumeAtFrame.ini";
        public static Dictionary<string, double> clipFrames = new Dictionary<string, double>();
        public static FileStream frameFile;

      
        public static void OpenFrameFile()
        {
            if(!File.Exists(settingsFile))
            {
                frameFile = File.Create(settingsFile);
                frameFile.Close();
            }

            string[] lines = File.ReadAllLines(settingsFile);

            foreach(string line in lines)
            {
                string[] splits = new string[2];
                splits = line.Split("|");

                clipFrames.Add(splits[0], (long)float.Parse(splits[1]));
            }
        }


        public static void AddFrameValueToFile(string clipName, double frameValue)
        {
            double value = frameValue;

            if (value < 0)
            {
                value = 0;
            }

            if (clipFrames.ContainsKey(clipName))
            {
                clipFrames[clipName] = value;
            }
            else
            {
                clipFrames.Add(clipName, value);
            }
        }

        public static double GetFrameValueFromFile(string clipName)
        {
            if (clipFrames.ContainsKey(clipName))
            {
                return clipFrames[clipName];
            }
            else
            {
                AddFrameValueToFile(clipName, 0);
                return 0;
            }
        }

        public static void SaveFrameFile()
        {
            using (StreamWriter writer = new StreamWriter(settingsFile))
            {               
                foreach (KeyValuePair<string, double> pair in clipFrames)
                {
                    double value = pair.Value;

                    if(value < 0)
                    {
                        value = 0;
                    }

                    writer.WriteLine(pair.Key + "|" + value.ToString());
                }

                writer.Close();
            }            
        }
    }
}