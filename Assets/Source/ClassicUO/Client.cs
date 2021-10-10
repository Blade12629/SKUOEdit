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
using System.IO;
using Assets.SourceRemake;
//using Assets.Source;
using ClassicUO.Data;
using ClassicUO.IO;
//using ClassicUO.Utility.Logging;
using ClassicUO.Utility.Platforms;

namespace ClassicUO
{
    internal static class Client
    {
        public static ClientVersion Version { get; private set; }
        public static ClientFlags Protocol { get; set; }
        public static string ClientPath { get; private set; }
        public static bool IsUOPInstallation { get; private set; }
        public static bool UseUOPGumps { get; set; }

        public static void ShowErrorMessage(string msg)
        {
            UnityEngine.Debug.LogError(msg);
        }


        public static void Load()
        {
            UnityEngine.Debug.Log(">>>>>>>>>>>>> Loading >>>>>>>>>>>>>");

            string clientPath = GameConfig.GameClientFiles;

            UnityEngine.Debug.Log($"Ultima Online installation folder: {clientPath}");

            UnityEngine.Debug.Log("Loading files...");

            //if (!string.IsNullOrWhiteSpace(Settings.GlobalSettings.ClientVersion))
            //{
            //    // sanitize client version
            //    Settings.GlobalSettings.ClientVersion = Settings.GlobalSettings.ClientVersion.Replace(",", ".").Replace(" ", "").ToLower();
            //}

            string clientVersionText = null/*Settings.GlobalSettings.ClientVersion*/;

            // check if directory is good
            if (!Directory.Exists(clientPath))
            {
                UnityEngine.Debug.LogError("Invalid client directory: " + clientPath);
                ShowErrorMessage(string.Format(/*ResErrorMessages.ClientPathIsNotAValidUODirectory*/"{0} is not a valid uo directory", clientPath));

                throw new InvalidClientDirectory($"'{clientPath}' is not a valid directory");
            }

            // try to load the client version
            if (!ClientVersionHelper.IsClientVersionValid(clientVersionText, out ClientVersion clientVersion))
            {
                UnityEngine.Debug.LogWarning($"Client version [{clientVersionText}] is invalid, let's try to read the client.exe");

                // mmm something bad happened, try to load from client.exe
                if (!ClientVersionHelper.TryParseFromFile(Path.Combine(clientPath, "client.exe"), out clientVersionText) || !ClientVersionHelper.IsClientVersionValid(clientVersionText, out clientVersion))
                {
                    UnityEngine.Debug.LogError("Invalid client version: " + clientVersionText);
                    ShowErrorMessage(string.Format(/*ResGumps.ImpossibleToDefineTheClientVersion0*/"Impossible to define client version {0}", clientVersionText));

                    throw new InvalidClientVersion($"Invalid client version: '{clientVersionText}'");
                }

                UnityEngine.Debug.Log($"Found a valid client.exe [{clientVersionText} - {clientVersion}]");

                // update the wrong/missing client version in settings.json
                //Settings.GlobalSettings.ClientVersion = clientVersionText;
            }

            Version = clientVersion;
            ClientPath = clientPath;

            IsUOPInstallation = Version >= ClientVersion.CV_7000 && File.Exists(UOFileManager.GetUOFilePath("MainMisc.uop"));

            Protocol = ClientFlags.CF_T2A;

            if (Version >= ClientVersion.CV_200)
            {
                Protocol |= ClientFlags.CF_RE;
            }

            if (Version >= ClientVersion.CV_300)
            {
                Protocol |= ClientFlags.CF_TD;
            }

            if (Version >= ClientVersion.CV_308)
            {
                Protocol |= ClientFlags.CF_LBR;
            }

            if (Version >= ClientVersion.CV_308Z)
            {
                Protocol |= ClientFlags.CF_AOS;
            }

            if (Version >= ClientVersion.CV_405A)
            {
                Protocol |= ClientFlags.CF_SE;
            }

            if (Version >= ClientVersion.CV_60144)
            {
                Protocol |= ClientFlags.CF_SA;
            }

            UnityEngine.Debug.Log($"Client path: '{clientPath}'");
            UnityEngine.Debug.Log($"Client version: {clientVersion}");
            UnityEngine.Debug.Log($"Protocol: {Protocol}");
            UnityEngine.Debug.Log("UOP? " + (IsUOPInstallation ? "yes" : "no"));

            // ok now load uo files
            UOFileManager.Load();

            UnityEngine.Debug.Log("Done!");

            //UoAssist.Start();

            UnityEngine.Debug.Log(">>>>>>>>>>>>> DONE >>>>>>>>>>>>>");
        }
    }

    public enum ClientFlags
    {
        CF_T2A,
        CF_RE,
        CF_TD,
        CF_LBR,
        CF_AOS,
        CF_SE,
        CF_SA
    }
    internal class InvalidClientVersion : Exception
    {
        public InvalidClientVersion(string msg) : base(msg)
        {
        }
    }

    internal class InvalidClientDirectory : Exception
    {
        public InvalidClientDirectory(string msg) : base(msg)
        {
        }
    }
}