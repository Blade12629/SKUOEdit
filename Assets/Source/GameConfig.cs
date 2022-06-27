using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Assets.Source
{
    public static class GameConfig
    {
        public static string TileAtlasFile { get; set; } = GetLocalPath("tiles.atlas");
        public static string StaticAtlasFile { get; set; } = GetLocalPath("statics.atlas");

        public static string GameClientFiles { get; set; } = GetLocalPath("ClassicUO");

        public static bool EnableGrid { get; set; } = false;
        public static float GridSize { get; set; } = 0.03f;
        public static Color GridColor { get; set; } = new Color(44, 207, 42, 1);

        public static bool ConfigNotFound => _configNotFound;

        static readonly string _configPath = GetLocalPath("config.cfg");
        static bool _configNotFound;

        static readonly Dictionary<string, Action<string>> _configInProps = new Dictionary<string, Action<string>>()
        {
            { nameof(TileAtlasFile), s => TileAtlasFile = s },
            { nameof(StaticAtlasFile), s => StaticAtlasFile = s },

            { nameof(GameClientFiles), s => GameClientFiles = s },

            { nameof(EnableGrid), s => EnableGrid = ParseBool(s, EnableGrid) },
            { nameof(GridSize), s => GridSize = ParseFloat(s, GridSize) },
            { nameof(GridColor), s => GridColor = ParseColor(s, GridColor) }
        };

        static readonly Dictionary<string, Func<object, string>> _configOutProps = new Dictionary<string, Func<object, string>>()
        {
            { nameof(TileAtlasFile), o => (string)o },
            { nameof(StaticAtlasFile), o => (string)o },
            { nameof(GameClientFiles), o => (string)o },
            { nameof(EnableGrid), o => o.ToString() },
            { nameof(GridSize), o => o.ToString() },
            { nameof(GridColor), o => WriteColor((Color)o) },
        };

        static GameConfig()
        {
            Load();
        }

        public static void Load()
        {
            if (System.IO.File.Exists(_configPath))
            {
                string[] lines = System.IO.File.ReadAllLines(_configPath);

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    // check for comments
                    if (line.StartsWith("//"))
                        continue;

                    string[] lineSplit = line.Split('=');

                    if (lineSplit.Length == 2)
                    {
                        if (lineSplit[1].Equals("null"))
                            continue;

                        try
                        {
                            if (_configInProps.TryGetValue(lineSplit[0], out Action<string> setAction))
                                setAction(lineSplit[1]);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Failed to set config property: {lineSplit[1]}: {ex}");
                        }
                    }
                }
            }
            else
                _configNotFound = true;
        }

        public static void Save()
        {
            StringBuilder cfgBuilder = new StringBuilder();
            cfgBuilder.AppendLine("// Use value 'NULL' or remove the line specific line to set the default value");

            PropertyInfo[] props = typeof(GameConfig).GetProperties(BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < props.Length; i++)
            {
                PropertyInfo prop = props[i];

                if (!prop.CanRead || !prop.CanWrite || !_configOutProps.TryGetValue(prop.Name, out Func<object, string> getAction))
                    continue;

                object propValue = prop.GetValue(null);

                if (propValue == null)
                    cfgBuilder.AppendLine($"{prop.Name}=NULL");
                else
                    cfgBuilder.AppendLine($"{prop.Name}={getAction(propValue)}");
            }

            if (System.IO.File.Exists(_configPath))
                System.IO.File.Delete(_configPath);

            System.IO.File.WriteAllText(_configPath, cfgBuilder.ToString());
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

        static bool ParseBool(string v, bool @default)
        {
            if (bool.TryParse(v, out bool b))
                return b;

            return @default;
        }

        static float ParseFloat(string v, float @default)
        {
            if (float.TryParse(v, out float f))
                return f;

            return @default;
        }

        static int ParseInt(string v, int @default)
        {
            if (int.TryParse(v, out int i))
                return i;

            return @default;
        }

        static Color ParseColor(string v, Color @default)
        {
            if (int.TryParse(v, out int cv))
            {
                if (cv <= 0)
                    return new Color();

                //AA RR GG BB
                byte a = (byte)((cv >> 24)  & 0xFF);
                byte r = (byte)((cv >> 16)  & 0xFF);
                byte g = (byte)((cv >> 8)   & 0xFF);
                byte b = (byte)( cv         & 0xFF);

                return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
            }

            return @default;
        }

        static string WriteColor(Color c)
        {
            //AA RR GG BB
            int a = (int)(c.a * 255f);
            int b = (int)(c.b * 255f);
            int g = (int)(c.g * 255f);
            int r = (int)(c.r * 255f);

            int result = (int)((a << 24) |
                               (r << 16) |
                               (g << 8)  |
                                b);

            return result.ToString("X6");
        }
    }
}
