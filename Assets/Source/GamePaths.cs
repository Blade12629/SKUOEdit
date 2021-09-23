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
        public static string Heightmap { get; set; } = System.IO.Path.Combine(Environment.CurrentDirectory, "heightmap.bmp");
        public static string HeightmapCS { get; set; } = System.IO.Path.Combine(Environment.CurrentDirectory, "heightmap.colorstore");
        public static string Tilemap { get; set; } = System.IO.Path.Combine(Environment.CurrentDirectory, "tilemap.bmp");
        public static string TilemapCS { get; set; } = System.IO.Path.Combine(Environment.CurrentDirectory, "tilemap.colorstore");
        public static string SAA { get; set; } = System.IO.Path.Combine(Environment.CurrentDirectory, "statics.saa");
        public static string OutputDirectory { get; set; } = System.IO.Path.Combine(Environment.CurrentDirectory, "\\output");
        public static string InputDirectory { get; set; } = Environment.CurrentDirectory;
        public static int MapIndex { get; set; } = 6;

        public static string TileAtlasUVFile { get; set; }
        public static string TileAtlasTexFile { get; set; }

        public static string ArtFile { get; set; } = System.IO.Path.Combine(Environment.CurrentDirectory, "art.mul");
        public static string ArtIdxFile { get; set; } = System.IO.Path.Combine(Environment.CurrentDirectory, "artidx.mul");
        
        public static string TexFile { get; set; } = System.IO.Path.Combine(Environment.CurrentDirectory, "texmaps.mul");
        public static string TexIdxFile { get; set; } = System.IO.Path.Combine(Environment.CurrentDirectory, "texmapsIdx.mul");

        public static string TileDataFile { get; set; } = System.IO.Path.Combine(Environment.CurrentDirectory, "tiledata.mul");

        public static string MUL_UOP_Directory { get; set; } = Environment.CurrentDirectory;

        static GamePaths()
        {
            Dictionary<string, Action<string>> configProps = new Dictionary<string, Action<string>>()
            {
                { nameof(Heightmap), s => Heightmap = s },
                { nameof(HeightmapCS), s => HeightmapCS = s },
                { nameof(Tilemap), s => Tilemap = s },
                { nameof(TilemapCS), s => TilemapCS = s },
                { nameof(SAA), s => SAA = s },
                { nameof(OutputDirectory), s => OutputDirectory = s },
                { nameof(InputDirectory), s => InputDirectory = s },
                { nameof(MapIndex), s =>
                {
                    if (int.TryParse(s, out int index))
                        MapIndex = index;
                }},

                { nameof(ArtFile), s => ArtFile = s },
                { nameof(ArtIdxFile), s => ArtIdxFile = s },
                { nameof(TexFile), s => TexFile = s },
                { nameof(TexIdxFile), s => TexIdxFile = s },
                { nameof(TileDataFile), s => TileDataFile = s },
                { nameof(MUL_UOP_Directory), s => MUL_UOP_Directory = s },
                { nameof(TileAtlasUVFile), s => TileAtlasUVFile = s },
                { nameof(TileAtlasTexFile), s => TileAtlasTexFile = s },
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
    }
}
