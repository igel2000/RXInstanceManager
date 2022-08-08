using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace RXInstanceManager
{
    public static class Dialogs
    {
        public static CommonOpenFileDialog ShowSelectFolderDialog(string defaultDirectory = null)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.Title = "My Title";
            dialog.IsFolderPicker = true;
            dialog.InitialDirectory = @"C:\";

            dialog.AddToMostRecentlyUsedList = false;
            dialog.AllowNonFileSystemItems = false;
            dialog.DefaultDirectory = !string.IsNullOrWhiteSpace(defaultDirectory) ? defaultDirectory : @"C:\";
            dialog.EnsureFileExists = true;
            dialog.EnsurePathExists = true;
            dialog.EnsureReadOnly = false;
            dialog.EnsureValidNames = true;
            dialog.Multiselect = false;
            dialog.ShowPlacesList = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                return dialog;

            return null;
        }

        public static string ShowEnterValueDialog(string emptyValue, string defaultValue = null)
        {
            var dialog = new EnterValueDialog(defaultValue);

            var dialogResult = dialog.ShowDialog();
            if (!dialogResult.HasValue || !dialogResult.Value)
                return null;

            return dialog.Value;
        }

        public static void ShowInformationDialog(string information)
        {
            var dialog = new InformationDialog();
            dialog.Value = information;
            dialog.ShowDialog();
        }
        public static void ShowFileContentDialog(string path)
        {
            var dialog = new InformationDialog();
            dialog.Path = path;
            dialog.ShowDialog();
        }
    }
}
