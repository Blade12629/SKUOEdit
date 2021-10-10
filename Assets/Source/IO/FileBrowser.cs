using System;
using FB = SimpleFileBrowser.FileBrowser;

namespace Assets.Source.IO
{
    public static class FileBrowser
    {
        public static void OpenFile(string title, Action<string> onSelected, Action onCancelled)
        {
            FB.ShowLoadDialog(new FB.OnSuccess(s => InvokeSelected(s, onSelected)),
                              new FB.OnCancel(() => onCancelled?.Invoke()),
                              FB.PickMode.Files, initialPath: Environment.CurrentDirectory, title: title);
        }

        public static void SaveFile(string title, Action<string> onSelected, Action onCancelled)
        {
            FB.ShowSaveDialog(new FB.OnSuccess(s => InvokeSelected(s, onSelected)),
                              new FB.OnCancel(() => onCancelled?.Invoke()),
                              FB.PickMode.Files, initialPath: Environment.CurrentDirectory, title: title);
        }

        public static void SelectFolder(string title, Action<string> onSelected, Action onCancelled)
        {
            FB.ShowLoadDialog(new FB.OnSuccess(s => InvokeSelected(s, onSelected)),
                              new FB.OnCancel(() => onCancelled?.Invoke()),
                              FB.PickMode.Folders, initialPath: Environment.CurrentDirectory, title: title);
        }

        static void InvokeSelected(string[] s, Action<string> onSelected)
        {
            if (s == null || s.Length == 0)
                return;

            onSelected?.Invoke(s[0]);
        }
    }
}
