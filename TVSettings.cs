using UnityEngine;
using ModSettings;
using MelonLoader;
using AudioMgr;

namespace NorthernLightsBroadcast
{
    internal class TVSettings : JsonModSettings
    {
        [Section("Buttons")]

        [Name("Interact button")]
        [Description("Button to witch TV on/off")]
        public KeyCode interactButton = KeyCode.Mouse2;

        [Section("Playback")]

        [Name("Sequence")]
        [Description("Playback order")]
        public TVScreen.Sequence sequence = TVScreen.Sequence.Random;

        [Name("Loop")]
        [Description("Loop settings for playback")]
        public TVScreen.Loop loop = TVScreen.Loop.All;

       
        [Section("Debug")]

        [Name("Log filenames")]
        [Description("Videofile name will be logged on playback error")]
        public bool showFilenames = true;

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

        public static void OnLoad()
        {
            options = new TVSettings();
            options.AddToModSettings("NorthernLightsBroadcast");
        }
    }
}
