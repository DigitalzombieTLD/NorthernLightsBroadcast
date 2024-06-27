using UnityEngine;
using ModSettings;
using MelonLoader;
using AudioMgr;
using static NorthernLightsBroadcast.Settings;

namespace NorthernLightsBroadcast
{
    internal class TVSettings : JsonModSettings
    {
        [Section("Buttons")]

        [Name("Interact button")]
        [Description("Button to witch TV on/off")]
        public KeyCode interactButton = KeyCode.Mouse2;

        [Section("Playback")]
     
        [Name("Play folder")]
        [Description("Continues playing all files after the last played file")]
        public bool playFolder = true;

        [Name("Loop folder")]
        [Description("Loops through all files inside the current folder")]
        public bool loopFolder = false;

        [Section("Debug")]
        
        [Name("Debug")]
        [Description("Debug mode. Default: Off")]
        public bool disableStronks = false;

        protected override void OnConfirm()
        {
            base.OnConfirm();
        }
    }

    internal static class Settings
    {
        public static TVSettings options;
        public enum LoopSetting { Off, LoopFile, LoopFolder };

        public static void OnLoad()
        {
            options = new TVSettings();
            options.AddToModSettings("NorthernLightsBroadcast");
        }
    }
}
