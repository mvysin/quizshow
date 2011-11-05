using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Vysin.QuizShow
{
    class MRUFileList
    {
        List<string> files = new List<string>(maxFileCount);
        const string regKeyName = "FileMRU";
        const int maxFileCount = 30;

        public MRUFileList()
        {
            RegistryKey key = Application.UserAppDataRegistry.CreateSubKey(regKeyName);
            string[] values = key.GetValueNames();
            Array.Sort(values);

            for (int i = 0; i < Math.Min(values.Length, maxFileCount); i++)
            {
                string file = key.GetValue(values[i]) as string;
                if (file == null)
                    continue;
                files.Add(file);
            }
        }

        public void AddFile(string file)
        {
            bool fileAdded = false;

            // check if it already exists
            string canonicalFile = file.ToUpperInvariant();
            for (int i = 0; i < files.Count; i++)
            {
                // check canonical file paths
                if (canonicalFile == files[i].ToUpperInvariant())
                {
                    files.RemoveAt(i);
                    if (!fileAdded)
                    {
                        files.Insert(0, file);
                        fileAdded = true;
                    }
                }
            }

            // if this is a brand-new file, add it to the start
            if (!fileAdded)
            {
                if (files.Count == maxFileCount)
                    files.RemoveAt(files.Count-1);
                files.Insert(0, file);
                fileAdded = true;
            }

            WriteBack();
        }

        public string GetFile(int n)
        {
            return files[n];
        }

        public int RecentFileCount
        {
            get { return files.Count; }
        }

        public void RemoveFile(string file)
        {
            bool removedFile = false;
            string canonicalFile = file.ToUpperInvariant();

            for (int i = 0; i < files.Count; i++)
            {
                // check canonical file paths
                if (canonicalFile == files[i].ToUpperInvariant())
                {
                    files.RemoveAt(i);
                    removedFile = true;
                }
            }

            if (removedFile)
                WriteBack();
        }

        private void WriteBack()
        {
            Application.UserAppDataRegistry.DeleteSubKey(regKeyName);
            RegistryKey key = Application.UserAppDataRegistry.CreateSubKey(regKeyName);
            for (int i = 0; i < files.Count; i++)
            {
                string value = i.ToString("D3");
                key.SetValue(value, files[i]);
            }
        }
    }
}