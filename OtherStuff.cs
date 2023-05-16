using MelonLoader;
using UnityEngine;
using Il2CppInterop;
using Il2CppInterop.Runtime.Injection; 
using System.Collections;
using System.Reflection;
using Il2Cpp;

namespace NorthernLightsBroadcast
{
	public static class OtherStuff
	{
        public static string countFileURL = Application.dataPath + @"/count";
        public static Dictionary<string, int> counts = new Dictionary<string, int>();
        public static FileStream countFile;
              
        public static void OpenCountFile()
        {
            if(!File.Exists(countFileURL))
            {
                countFile = File.Create(countFileURL);
                countFile.Close();
            }

            string[] lines = File.ReadAllLines(countFileURL);

            foreach(string line in lines)
            {
                if(line.Contains("|"))
                {
                    string[] splits = new string[2];
                    splits = line.Split("|");

                    if (!counts.ContainsKey(splits[0]))
                    {
                        counts.Add(splits[0], int.Parse(splits[1]));
                    }                   
                }               
            }
        }

        public static void AddCountValueToFile(string clipName, int countValue)
        {
            int value = countValue;

            if (value < 0)
            {
                value = 0;
            }

            if (counts.ContainsKey(clipName))
            {
                counts[clipName] = countValue;
            }
            else
            {
                counts.Add(clipName, countValue);
            }
        }

        public static int GetCountValueFromFile(string clipName)
        {
            if (counts.ContainsKey(clipName))
            {
                return counts[clipName];
            }
            else
            {
                AddCountValueToFile(clipName, 0);
                return 0;
            }
        }

        public static void SaveCountFile()
        {
            using (StreamWriter writer = new StreamWriter(countFileURL))
            {               
                foreach (KeyValuePair<string, int> pair in counts)
                {
                    int value = pair.Value;

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