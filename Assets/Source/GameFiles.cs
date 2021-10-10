using Assets.Source.Textures;
using Assets.SourceRemake;
//using SKMapGenerator.Ultima;
using UnityEngine;

namespace Assets.Source
{
    public static class GameFiles
    {
        public static void LoadClientFiles()
        {
            Debug.Log("--- Loading Client Files ---");

            Debug.Log("Loading ClassicUO");
            ClassicUO.Client.Load();

            Debug.Log("Loading TileAtlas");
            if (System.IO.File.Exists(GameConfig.TileAtlasUVFile) &&
                System.IO.File.Exists(GameConfig.TileAtlasTexFile))
            {
                GameTextures.Import(GameConfig.TileAtlasUVFile, GameConfig.TileAtlasTexFile);
            }
            else
            {
                GameTextures.Initialize(true);
                GameTextures.AddNoDraw();

                for (int i = 0; i < 0x4000; i++)
                {
                    GameTextures.AddTile(i);
                    GameTextures.AddTexture(i);
                }
            }

            GameTextures.ApplyChanges();

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
