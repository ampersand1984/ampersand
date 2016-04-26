using System;
using System.IO;
using System.Windows;

namespace ampersand.Core.Common
{
    public class Win32Helper
    {
        public static string ShowFileDialog(string fileName, string title = "")
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.FileName = fileName; // Default file name
            saveFileDialog.DefaultExt = ".xml"; // Default file extension
            saveFileDialog.Filter = "xml (.xml)|*.xml"; // Filter files by extension
            saveFileDialog.Title = title; // Filter files by extension

            // Show save file dialog box
            var result = saveFileDialog.ShowDialog();

            // Process save file dialog box results

            var filenameResult = string.Empty;

            if (result == true)
            {
                // Save document
                filenameResult = saveFileDialog.FileName;
            }

            return filenameResult;
        }

        public static void SaveFile(string filePath, string content)
        {
            File.WriteAllText(filePath, content); 
        }

        public static void ShowError(Exception ex)
        {
            MessageBox.Show(ex.Message, "Error: " + ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
