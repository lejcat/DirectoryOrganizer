using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DirectoryOrganizer
{
    public class MainWindowDataModel
    {
        #region Read/Write Profile
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        #endregion

        private string ConfigPath { get; set; }
        private string LogPath { get; set; }

        public MainWindowDataModel()
        {
            ConfigPath = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigPath);
            ConfigPath = Path.ChangeExtension(ConfigPath, ".ini");
            LogPath = Path.ChangeExtension(ConfigPath, ".log");
        }

        public string LoadPaths()
        {
            string result = string.Empty;

            try
            {
                if (File.Exists(ConfigPath))
                {
                    StringBuilder strValue = new StringBuilder(2048);

                    GetPrivateProfileString("Path", "Count", "0", strValue, strValue.Capacity, ConfigPath);
                    if (int.TryParse(strValue.ToString(), out int count) && count > 0)
                    {
                        for (int i = 1; i <= count; i++)
                        {
                            try
                            {
                                GetPrivateProfileString("Path", "Line" + i, "", strValue, strValue.Capacity, ConfigPath);
                                if (!string.IsNullOrEmpty(strValue.ToString()))
                                {
                                    if (!string.IsNullOrEmpty(result))
                                    {
                                        result += "\n";
                                    }
                                    result += strValue.ToString();
                                }
                            }
                            catch (Exception ex)
                            {
                                SaveErrorLog(ex.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SaveErrorLog(ex.Message);
            }

            return result;
        }
        public (bool, string) LoadOptions()
        {
            StringBuilder strCheck = new StringBuilder();
            StringBuilder strCount = new StringBuilder();

            try
            {
                if (File.Exists(ConfigPath))
                {
                    GetPrivateProfileString("Option", "CheckLastContents", "false", strCheck, strCheck.Capacity, ConfigPath);

                    GetPrivateProfileString("Option", "CheckCount", "3", strCount, strCount.Capacity, ConfigPath);
                }
            }
            catch (Exception ex)
            {
                SaveErrorLog(ex.Message);
            }

            bool chk = false;
            string count = "3";

            bool.TryParse(strCheck.ToString(), out chk);
            if (!string.IsNullOrEmpty(strCount.ToString()))
            {
                count = strCount.ToString();
            }

            return (chk, count);
        }

        public void SavePaths(string pathLines)
        {
            if (string.IsNullOrEmpty(pathLines))
                return;

            try
            {
                string[] lines = pathLines.Split('\n');
                if (lines.Length > 0)
                {
                    WritePrivateProfileString("Path", "Count", lines.Length.ToString(), ConfigPath);

                    int cnt = 1;
                    foreach (string line in lines)
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                WritePrivateProfileString("Path", "Line" + cnt++, line, ConfigPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            SaveErrorLog("[" + line + "]" + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SaveErrorLog(ex.Message);
            }
        }
        public void SaveOptions(bool checkContent, int count)
        {
            try
            {
                WritePrivateProfileString("Option", "CheckLastContents", checkContent.ToString(), ConfigPath);
                WritePrivateProfileString("Option", "CheckCount", count.ToString(), ConfigPath);
            }
            catch (Exception ex)
            {
                SaveErrorLog(ex.Message);
            }
        }

        public void SaveErrorLog(string message)
        {
            if (!string.IsNullOrEmpty(LogPath))
            {
                try
                {
                    string text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : " + message.Replace("\n", " ");
                    using (StreamWriter logWriter = File.AppendText(LogPath))
                    {
                        logWriter.WriteLine(text);
                    }
                }
                catch
                { }
            }
        }

        public string FindDirectories()
        {
            string result = string.Empty;

            if (Application.Current != null)
            {
                if (Application.Current.MainWindow is MainWindow window)
                {
                    try
                    {
                        CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                        dialog.IsFolderPicker = true;
                        dialog.Multiselect = true;
                        dialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

                        if (dialog.ShowDialog(window) == CommonFileDialogResult.Ok)
                        {
                            result = string.Join("\n", dialog.FileNames);
                        }
                    }
                    catch (Exception ex)
                    {
                        SaveErrorLog(ex.Message);
                    }
                }
            }

            return result;
        }

        public void MoveDirectory(string source, string destination)
        {
            if (!string.IsNullOrEmpty(source))
            {
                if (Path.IsPathRooted(source.Trim().Trim('/').Trim('\\')) == false)
                {
                    source = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, source.Trim().Trim('/').Trim('\\'));
                }
                if (Path.IsPathRooted(destination.Trim().Trim('/').Trim('\\')) == false)
                {
                    destination = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, destination.Trim().Trim('/').Trim('\\'));
                }

                if (Directory.Exists(source))
                {
                    string[] files = Directory.GetFiles(source, "*", SearchOption.AllDirectories);
                    if (files != null)
                    {
                        foreach (string file in files)
                        {
                            try
                            {
                                if (File.Exists(file))
                                {
                                    string dstFile = file.Replace(source, destination);

                                    string dstDir = Path.GetDirectoryName(dstFile);
                                    if (Directory.Exists(dstDir) == false)
                                    {
                                        Directory.CreateDirectory(dstDir);
                                    }

                                    try
                                    {
                                        File.Copy(file, dstFile, true);

                                        if (File.Exists(file))
                                        {
                                            File.Delete(file);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        SaveErrorLog("Copy and Delete <" + Path.GetFileName(source) + "/" + Path.GetFileName(file) + "> : " + ex.Message);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                SaveErrorLog("Move Directory Contents <" + Path.GetFileName(source) + "> : " + ex.Message);
                            }
                        }
                    }

                    try
                    {
                        files = Directory.GetFiles(source, "*", SearchOption.AllDirectories);

                        if (files == null || files.Length <= 0)
                        {
                            Directory.Delete(source, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        SaveErrorLog("Delete Empty Source <" + Path.GetFileName(source) + "> : " + ex.Message);
                    }
                }
            }
        }
    }
}
