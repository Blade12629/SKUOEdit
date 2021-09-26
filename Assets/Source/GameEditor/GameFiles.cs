using Assets.Source.Textures;
//using SKMapGenerator.Ultima;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Source.IO;

namespace Assets.Source.GameEditor
{
    public class GameFiles
    {
        public TileMatrix TileMatrix { get; private set; }
        public StaticTileMatrix StaticTileMatrix { get; private set; }

        public GameFiles()
        {
        }

        //public void LoadClientFiles()
        //{
        //    Debug.Log("--- Loading Client Files ---");

        //    Debug.Log("Loading client Art");
        //    Art.Load();

        //    Debug.Log("Loading client Textures");
        //    Textures.Load();

        //    Debug.Log("Loading TileAtlas");
        //    if (System.IO.File.Exists(GamePaths.TileAtlasUVFile) &&
        //        System.IO.File.Exists(GamePaths.TileAtlasTexFile))
        //    {
        //        UOAtlas.Initialize(Art, Textures);

        //        GameTextures.Import(GamePaths.TileAtlasUVFile, GamePaths.TileAtlasTexFile);
        //    }
        //    else
        //    {
        //        UOAtlas.Initialize(Art, Textures);
        //        UOAtlas.Initialize(true);

        //        UOAtlas.AddNoDraw();

        //        for (int i = 0; i < Art.LandColors.Count; i++)
        //        {
        //            UOAtlas.AddTile(i);
        //        }

        //        for (int i = 0; i < 0x4000; i++)
        //        {
        //            UOAtlas.AddTexture(i);
        //        }
        //    }

        //    UOAtlas.ApplyChanges();

        //    Debug.Log("Loading UOFiddler");
        //    Ultima.UOFiddler.Files.LoadMulPath();
        //    Ultima.UOFiddler.Files.ReLoadDirectory();

        //    Debug.Log("--- Loaded Client Files ---");
        //}

        public void LoadClientFiles()
        {
            Debug.Log("--- Loading Client Files ---");

            Debug.Log("Loading ClassicUO");
            ClassicUO.Client.Load();

            Debug.Log("Loading TileAtlas");
            if (System.IO.File.Exists(GamePaths.TileAtlasUVFile) &&
                System.IO.File.Exists(GamePaths.TileAtlasTexFile))
            {
                GameTextures.Import(GamePaths.TileAtlasUVFile, GamePaths.TileAtlasTexFile);
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

        public void LoadMap(int index, int width, int depth)
        {
            Debug.Log($"--- Loading map {index} with size {width}/{depth} ---");

            TileMatrix = new TileMatrix(GetUOPath($"map{index}.mul"), width, depth);
            TileMatrix.Load();

            StaticTileMatrix = new StaticTileMatrix(GetUOPath($"statics{index}.mul"), GetUOPath($"staidx{index}.mul"), width, depth);
            StaticTileMatrix.Load();

            Debug.Log($"--- Loaded map --- ");
        }

        string GetUOPath(string file)
        {
            return System.IO.Path.Combine(GamePaths.GameClientFiles, file);
        }
    }
}
