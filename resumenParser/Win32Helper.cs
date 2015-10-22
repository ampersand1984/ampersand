using System;
using System.IO;
using System.Windows;
using System.Xml.Linq;

namespace ResumenParser
{
    public class Win32Helper
    {
        internal static string ShowFileDialog(string fileName)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.FileName = fileName; // Default file name
            saveFileDialog.DefaultExt = ".xml"; // Default file extension
            saveFileDialog.Filter = "xml (.xml)|*.xml"; // Filter files by extension

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

        internal static void SaveFile(string filePath, XDocument content)
        {
            File.WriteAllText(filePath, content.ToString()); 
        }

        internal static void ShowError(Exception ex)
        {
            MessageBox.Show(ex.Message, "Error: " + ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
