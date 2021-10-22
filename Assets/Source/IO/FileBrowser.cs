using System;
using System.IO;
using FB = SimpleFileBrowser.FileBrowser;

namespace Assets.Source.IO
{
    public static class FileBrowser
    {
        static string _initialPath;

        static FileBrowser()
        {
#if UNITY_EDITOR
            _initialPath = System.IO.Path.Combine(Environment.CurrentDirectory, "Test");
#else
            _initialPath = Environment.CurrentDirectory;
#endif
        }

        public static void OpenFile(string title, Action<string> onSelected, Action onCancelled)
        {
            FB.ShowLoadDialog(new FB.OnSuccess(s => InvokeSelected(s, onSelected)),
                              new FB.OnCancel(() => onCancelled?.Invoke()),
                              FB.PickMode.Files, initialPath: _initialPath, title: title);
        }

        public static void SaveFile(string title, Action<string> onSelected, Action onCancelled)
        {
            FB.ShowSaveDialog(new FB.OnSuccess(s => InvokeSelected(s, onSelected)),
                              new FB.OnCancel(() => onCancelled?.Invoke()),
                              FB.PickMode.Files, initialPath: _initialPath, title: title);
        }

        public static void SelectFolder(string title, Action<string> onSelected, Action onCancelled)
        {
            FB.ShowLoadDialog(new FB.OnSuccess(s => InvokeSelected(s, onSelected)),
                              new FB.OnCancel(() => onCancelled?.Invoke()),
                              FB.PickMode.Folders, initialPath: _initialPath, title: title);
        }

        /// <summary>
        /// Does not require to be run on the unity mainthread
        /// </summary>
        public static void BackupFile(string file, int maxBackups)
        {
            if (!File.Exists(file))
                return;

            // count backups
            for (int i = 0; i < maxBackups; i++)
            {
                // we have a backup slot available
                if (!File.Exists(file + i))
                {
                    File.Move(file, file + i);
                    return;
                }
            }

            // we reached max backups, delete oldest backup
            string fileToDelete = file + (maxBackups - 1);

            if (File.Exists(fileToDelete))
                File.Delete(fileToDelete);

            // move every backup one iteration higher (1 -> 2)
            for (int i = maxBackups - 2; i > 0; i--)
                File.Move(file + i, file + (i + 1));

            // move file to iteration 0
            File.Move(file, file + 0);
        }

        static void InvokeSelected(string[] s, Action<string> onSelected)
        {
            if (s == null || s.Length == 0)
                return;

            onSelected?.Invoke(s[0]);
        }
    }
}
