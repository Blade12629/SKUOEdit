using Assets.SourceRemake;
using Assets.Source.Textures;
//using SKMapGenerator.Ultima;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                UOAtlas.Initialize(true);

                UOAtlas.AddNoDraw();

                for (int i = 0; i < 0x4000; i++)
                {
                    UOAtlas.AddTile(i);
                    UOAtlas.AddTexture(i);
                }
            }

            UOAtlas.ApplyChanges();

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
