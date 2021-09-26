using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source
{
    public static class GamePaths
    {
        // TODO: remake paths
        public static string Heightmap { get; set; } = GetLocalPath("heightmap.bmp");
        public static string HeightmapCS { get; set; } = GetLocalPath("heightmap.colorstore");
        public static string Tilemap { get; set; } = GetLocalPath("tilemap.bmp");
        public static string TilemapCS { get; set; } = GetLocalPath("tilemap.colorstore");

        public static int MapIndex { get; set; } = 6;

        public static string TileAtlasUVFile { get; set; } = GetLocalPath("TileAtlas.uv");
        public static string TileAtlasTexFile { get; set; } = GetLocalPath("TileAtlas.tex");

        public static string GameClientFiles { get; set; } = GetLocalPath("ClassicUO");
        // TODO: add path to folder for maps to edit (in/output)

        static GamePaths()
        {
            Dictionary<string, Action<string>> configProps = new Dictionary<string, Action<string>>()
            {
                { nameof(Heightmap), s => Heightmap = s },
                { nameof(HeightmapCS), s => HeightmapCS = s },
                { nameof(Tilemap), s => Tilemap = s },
                { nameof(TilemapCS), s => TilemapCS = s },
                { nameof(MapIndex), s =>
                {
                    if (int.TryParse(s, out int index))
                        MapIndex = index;
                }},

                { nameof(TileAtlasUVFile), s => TileAtlasUVFile = s },
                { nameof(TileAtlasTexFile), s => TileAtlasTexFile = s },
                { nameof(GameClientFiles), s => GameClientFiles = s },
            };

            if (System.IO.File.Exists("config.cfg"))
            {
                string[] lines = System.IO.File.ReadAllLines("config.cfg");

                foreach(string line in lines)
                {
                    string[] lineSplit = line.Split('=');

                    if (lineSplit.Length == 2 && configProps.TryGetValue(lineSplit[0], out Action<string> setAction))
                    {
                        setAction(lineSplit[1]);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the path to a file or folder in the <see cref="GetLocalPath()"/>
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        static string GetLocalPath(string file)
        {
            return System.IO.Path.Combine(GetLocalPath(), file);
        }

        /// <summary>
        /// Gets the local path (i.e. the path where the executable is stored)
        /// </summary>
        /// <returns></returns>
        static string GetLocalPath()
        {
            return Environment.CurrentDirectory;
        }
    }
}
