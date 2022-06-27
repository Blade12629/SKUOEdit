using UnityEngine;

namespace Assets.Source.Ultima
{
    public static class GameFiles
    {
        public static void LoadClientFiles()
        {
            Debug.Log("--- Loading Client Files ---");

            Debug.Log("Loading ClassicUO");
            ClassicUO.Client.Load();

            Debug.Log("--- Loaded Client Files ---");
        }

        public static void Unload()
        {
            ClassicUO.IO.UOFileManager.Unload();
        }

        public static string GetUOPath(string file)
        {
            return System.IO.Path.Combine(GameConfig.GameClientFiles, file);
        }
    }
}
