#region license

// Copyright (c) 2021, andreakarasho
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by andreakarasho - https://github.com/andreakarasho
// 4. Neither the name of the copyright holder nor the
//    names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Assets.SourceRemake;
//using Assets.Source;
//using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game;
using ClassicUO.IO.Resources;
//using ClassicUO.Utility.Logging;

namespace ClassicUO.IO
{
    internal static class UOFileManager
    {
        public static string GetUOFilePath(string file)
        {
            if (UOFilesOverrideMap.Instance.TryGetValue(file.ToLowerInvariant(), out string uoFilePath))
            {
                return uoFilePath;
            }

            return Path.Combine(GameConfig.GameClientFiles, file);
        }

        public static void Load()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            UOFilesOverrideMap.Instance.Load(); // need to load this first so that it manages can perform the file overrides if needed

            List<Task> tasks = new List<Task>
            {
                AnimDataLoader.Instance.Load(),
                ArtLoader.Instance.Load(),
                HuesLoader.Instance.Load(),
                TileDataLoader.Instance.Load(),
                MultiLoader.Instance.Load(),
                TexmapsLoader.Instance.Load(),
                LightsLoader.Instance.Load(),
                MultiMapLoader.Instance.Load()
            };

            if (!Task.WhenAll(tasks).Wait(TimeSpan.FromSeconds(10)))
            {
                UnityEngine.Debug.LogError("Loading files timeout.");
            }

            Read_Art_def();

            UnityEngine.Debug.Log($"Files loaded in: {stopwatch.ElapsedMilliseconds} ms!");
            stopwatch.Stop();
        }

        public static void Unload()
        {
            AnimDataLoader.Instance.Dispose();
            ArtLoader.Instance.Dispose();
            HuesLoader.Instance.Dispose();
            TileDataLoader.Instance.Dispose();
            MultiLoader.Instance.Dispose();
            TexmapsLoader.Instance.Dispose();
            LightsLoader.Instance.Dispose();
            MultiMapLoader.Instance.Dispose();
        }

        private static void Read_Art_def()
        {
            string pathdef = GetUOFilePath("art.def");

            if (File.Exists(pathdef))
            {
                TileDataLoader tiledataLoader =  TileDataLoader.Instance;
                ArtLoader artLoader = ArtLoader.Instance;
                
                using (DefReader reader = new DefReader(pathdef, 1))
                {
                    while (reader.Next())
                    {
                        int index = reader.ReadInt();

                        if (index < 0 || index >= Constants.MAX_LAND_DATA_INDEX_COUNT + tiledataLoader.StaticData.Length)
                        {
                            continue;
                        }

                        int[] group = reader.ReadGroup();

                        if (group == null)
                        {
                            continue;
                        }

                        for (int i = 0; i < group.Length; i++)
                        {
                            int checkIndex = group[i];

                            if (checkIndex < 0 || checkIndex >= Constants.MAX_LAND_DATA_INDEX_COUNT + tiledataLoader.StaticData.Length)
                            {
                                continue;
                            }

                            if (index < artLoader.Entries.Length && checkIndex < artLoader.Entries.Length)
                            {
                                ref UOFileIndex currentEntry = ref artLoader.GetValidRefEntry(index);
                                ref UOFileIndex checkEntry = ref artLoader.GetValidRefEntry(checkIndex);

                                if (currentEntry.Equals(UOFileIndex.Invalid) && !checkEntry.Equals(UOFileIndex.Invalid))
                                {
                                    artLoader.Entries[index] = artLoader.Entries[checkIndex];
                                }
                            }

                            if (index < Constants.MAX_LAND_DATA_INDEX_COUNT &&
                                checkIndex < Constants.MAX_LAND_DATA_INDEX_COUNT && 
                                checkIndex < tiledataLoader.LandData.Length && 
                                index < tiledataLoader.LandData.Length &&
                                !tiledataLoader.LandData[checkIndex].Equals(default) &&
                                tiledataLoader.LandData[index].Equals(default))
                            {
                                tiledataLoader.LandData[index] = tiledataLoader.LandData[checkIndex];

                                break;
                            }

                            if (index >= Constants.MAX_LAND_DATA_INDEX_COUNT && checkIndex >= Constants.MAX_LAND_DATA_INDEX_COUNT &&
                                tiledataLoader.StaticData[index].Equals(default) && !tiledataLoader.StaticData[checkIndex].Equals(default))
                            {
                                tiledataLoader.StaticData[index] = tiledataLoader.StaticData[checkIndex];

                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}