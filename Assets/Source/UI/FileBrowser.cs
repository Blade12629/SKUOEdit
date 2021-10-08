using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SimpleFileBrowser;
using FB = SimpleFileBrowser.FileBrowser;
using System.Collections;

namespace Assets.Source.UI
{
    public static class FileBrowser
    {
        public static void OpenFile(string title, Action<string> onSelected, Action onCancelled)
        {
            FB.ShowLoadDialog(new FB.OnSuccess(s => InvokeSelected(s, onSelected)), new FB.OnCancel(onCancelled), FB.PickMode.Files, title: title);
        }

        public static void SaveFile(string title, Action<string> onSelected, Action onCancelled)
        {
            FB.ShowSaveDialog(new FB.OnSuccess(s => InvokeSelected(s, onSelected)), new FB.OnCancel(onCancelled), FB.PickMode.Files, title: title);
        }

        public static void SelectFolder(string title, Action<string> onSelected, Action onCancelled)
        {
            FB.ShowLoadDialog(new FB.OnSuccess(s => InvokeSelected(s, onSelected)), new FB.OnCancel(onCancelled), FB.PickMode.Folders, title: title);
        }

        static void InvokeSelected(string[] s, Action<string> onSelected)
        {
            if (s == null || s.Length == 0)
                return;

            onSelected?.Invoke(s[0]);
        }
    }
}
