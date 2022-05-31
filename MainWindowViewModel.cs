using DirectoryOrganizer.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryOrganizer
{
    public class MainWindowViewModel : MainWindowViewModelBase
    {
        public MainWindowViewModel()
        {
            PageLoadedCommand = new RelayCommand(PageLoadedCommandExe);
            FindPathCommand = new RelayCommand(FindPathCommandExe);
            SaveConfigCommand = new RelayCommand(SaveConfigCommandExe);
            RunCommand = new RelayCommand(RunCommandExe);
            RunCheckCommand = new RelayCommand(RunCheckCommandExe);
        }

        private void PageLoadedCommandExe(object obj)
        {
            if (DataModel == null)
                return;

            PathString = DataModel.LoadPaths();

            (bool, string) option = DataModel.LoadOptions();
            CheckLastPathContents = option.Item1;
            LastPathLessContentsCount = option.Item2;
        }
        private void FindPathCommandExe(object obj)
        {
            if (DataModel == null)
                return;

            string newPath = DataModel.FindDirectories();

            if (!string.IsNullOrEmpty(newPath))
            {
                if (!string.IsNullOrEmpty(PathString))
                {
                    if (PathString.EndsWith(Environment.NewLine) == false)
                    {
                        PathString += "\n";
                    }
                }

                PathString += newPath;
            }
        }
        private void SaveConfigCommandExe(object obj)
        {
            if (DataModel == null)
                return;

            if (int.TryParse(LastPathLessContentsCount, out int count))
            {
                DataModel.SaveOptions(CheckLastPathContents, count);
                DataModel.SavePaths(PathString);
            }
            else
            {
                LogLines.Add("Check Count Value Incorrect");
            }
        }
        private async void RunCommandExe(object obj)
        {
            if (DataModel == null)
                return;

            IsPageEnabled = false;

            await RunOrganization();

            IsPageEnabled = true;
        }
        private async void RunCheckCommandExe(object obj)
        {
            if (DataModel == null)
                return;

            IsPageEnabled = false;

            await RunCheckContents();

            IsPageEnabled = true;
        }

        private async Task RunOrganization()
        {
            if (DataModel == null)
                return;

            if (string.IsNullOrWhiteSpace(PathString.Replace("\n", "")))
                return;

            LogLines.Clear();

            await Task.Delay(0);

            string[] paths = PathString.Split('\n');
            if (paths.Length > 1)
            {
                for (int i = 0; i < paths.Length - 1; i++)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(paths[i]))
                        {
                            //Root Directory
                            string src = paths[i];
                            if (Path.IsPathRooted(src.Trim().Trim('\\').Trim('/')) == false)
                            {
                                src = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, src.Trim().Trim('\\').Trim('/'));
                            }
                            if (Directory.Exists(src))
                            {
                                //Artist List
                                string[] items = Directory.GetDirectories(src, "*", SearchOption.TopDirectoryOnly);

                                //Set Next Root Directory
                                for (int j = i + 1; j < paths.Length; j++)
                                {
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(paths[j]))
                                        {
                                            //Destination Root
                                            string dst = paths[j];
                                            if (Path.IsPathRooted(dst.Trim().Trim('\\').Trim('/')) == false)
                                            {
                                                dst = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dst.Trim().Trim('\\').Trim('/'));
                                            }
                                            //Create Destination Root
                                            if (Directory.Exists(dst) == false)
                                            {
                                                Directory.CreateDirectory(dst);
                                            }

                                            //Progress for Artist
                                            foreach (string item in items)
                                            {
                                                try
                                                {
                                                    if (Directory.Exists(item))
                                                    {
                                                        //LogLines.Add("Check : " + Path.GetFileName(item));

                                                        //Artist Items
                                                        string[] subitems = Directory.GetDirectories(item, "*", SearchOption.TopDirectoryOnly);

                                                        //Destination Artist
                                                        string dstItem = Path.Combine(dst, Path.GetFileName(item));
                                                        if (Directory.Exists(dstItem))
                                                        {
                                                            //LogLines.Add("Find : " + Path.GetFileName(dstItem));

                                                            foreach (string subitem in subitems)
                                                            {
                                                                if (Directory.Exists(subitem))
                                                                {
                                                                    string dstSubs = Path.Combine(dstItem, Path.GetFileName(subitem));

                                                                    //Destination Item Already Exists
                                                                    if (Directory.Exists(dstSubs))
                                                                    {
                                                                        LogLines.Add("Find Old : " + Path.GetFileName(dstSubs));

                                                                        //check Contents Count
                                                                        string[] srcContents = Directory.GetFiles(subitem, "*", SearchOption.AllDirectories);
                                                                        LogLines.Add("* Source Count : " + srcContents.Length);

                                                                        string[] dstContents = Directory.GetFiles(dstSubs, "*", SearchOption.AllDirectories);
                                                                        LogLines.Add("* Destin Count : " + dstContents.Length);

                                                                        if (srcContents.Length > dstContents.Length)
                                                                        {
                                                                            //Delete Old/Less Contents Destination Item
                                                                            try
                                                                            {
                                                                                LogLines.Add("* Delete Old : " + Path.GetFileName(dstSubs));
                                                                                Directory.Delete(dstSubs, true);
                                                                            }
                                                                            catch (Exception ex)
                                                                            {
                                                                                LogLines.Add("-----Error----");
                                                                                DataModel.SaveErrorLog("[" + dstSubs + "]");
                                                                                DataModel.SaveErrorLog(ex.Message);
                                                                                LogLines.Add(dstSubs);
                                                                                LogLines.Add(ex.Message);
                                                                                LogLines.Add("--------------");
                                                                                continue;
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            //Delete Old/Less Contents Source Item
                                                                            try
                                                                            {
                                                                                LogLines.Add("* Delete New : " + Path.GetFileName(subitem));
                                                                                Directory.Delete(subitem, true);
                                                                            }
                                                                            catch (Exception ex)
                                                                            {
                                                                                LogLines.Add("-----Error----");
                                                                                DataModel.SaveErrorLog("[" + subitem + "]");
                                                                                DataModel.SaveErrorLog(ex.Message);
                                                                                LogLines.Add(subitem);
                                                                                LogLines.Add(ex.Message);
                                                                                LogLines.Add("--------------");
                                                                            }
                                                                            continue;
                                                                        }
                                                                    }

                                                                    //Move Source to Destination
                                                                    try
                                                                    {
                                                                        LogLines.Add("Move : " + subitem);
                                                                        LogLines.Add("-> " + dstSubs);
                                                                        DataModel.MoveDirectory(subitem, dstSubs);
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        LogLines.Add("-----Error----");
                                                                        DataModel.SaveErrorLog("[" + subitem + " -> " + dstSubs + "]");
                                                                        DataModel.SaveErrorLog(ex.Message);
                                                                        LogLines.Add(subitem);
                                                                        LogLines.Add(" -> " + dstSubs);
                                                                        LogLines.Add(ex.Message);
                                                                        LogLines.Add("--------------");
                                                                    }
                                                                }
                                                            }

                                                            //Check if item Empty -> Delete
                                                            subitems = Directory.GetFiles(item, "*", SearchOption.AllDirectories);
                                                            if (subitems.Length <= 0)
                                                            {
                                                                try
                                                                {
                                                                    LogLines.Add("* Delete Empty : " + item);
                                                                    Directory.Delete(item, true);
                                                                }
                                                                catch (Exception ex)
                                                                {
                                                                    LogLines.Add("-----Error----");
                                                                    DataModel.SaveErrorLog("[" + item + "]");
                                                                    DataModel.SaveErrorLog(ex.Message);
                                                                    LogLines.Add(item);
                                                                    LogLines.Add(ex.Message);
                                                                    LogLines.Add("--------------");
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    LogLines.Add("-----Error----");
                                                    DataModel.SaveErrorLog("[" + item + "]");
                                                    DataModel.SaveErrorLog(ex.Message);
                                                    LogLines.Add(item);
                                                    LogLines.Add(ex.Message);
                                                    LogLines.Add("--------------");
                                                }
                                            }

                                            if (j == paths.Length - 1)
                                            {
                                                if (CheckLastPathContents == true)
                                                {
                                                    LogLines.Add("* Check Contents Count");
                                                    if (int.TryParse(LastPathLessContentsCount, out int count))
                                                    {
                                                        LogLines.Add("* Move Contents To Parent / Under Count " + count);

                                                        foreach (string item in items)
                                                        {
                                                            string dstItem = Path.Combine(dst, Path.GetFileName(item));
                                                            if (Directory.Exists(dstItem))
                                                            {
                                                                string[] subDestin = Directory.GetDirectories(dstItem, "*", SearchOption.TopDirectoryOnly);
                                                                if (subDestin.Length < count)
                                                                {
                                                                    LogLines.Add("* Move " + Path.GetFileName(dstItem) + " contents");
                                                                    LogLines.Add("-> Parent : " + Path.GetFileName(dst));

                                                                    foreach (string subdst in subDestin)
                                                                    {
                                                                        string moveto = Path.Combine(dst, Path.GetFileName(subdst));

                                                                        if (Directory.Exists(moveto))
                                                                        {
                                                                            //check Contents Count
                                                                            string[] srcContents = Directory.GetFiles(subdst, "*", SearchOption.AllDirectories);
                                                                            LogLines.Add("* Source Count : " + srcContents.Length);

                                                                            string[] dstContents = Directory.GetFiles(moveto, "*", SearchOption.AllDirectories);
                                                                            LogLines.Add("* Destin Count : " + dstContents.Length);

                                                                            if (srcContents.Length > dstContents.Length)
                                                                            {
                                                                                //Delete Old/Less Contents Destination Item
                                                                                try
                                                                                {
                                                                                    LogLines.Add("* Delete Old : " + Path.GetFileName(moveto));
                                                                                    Directory.Delete(moveto, true);
                                                                                }
                                                                                catch (Exception ex)
                                                                                {
                                                                                    LogLines.Add("-----Error----");
                                                                                    DataModel.SaveErrorLog("[" + moveto + "]");
                                                                                    DataModel.SaveErrorLog(ex.Message);
                                                                                    LogLines.Add(moveto);
                                                                                    LogLines.Add(ex.Message);
                                                                                    LogLines.Add("--------------");
                                                                                    continue;
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                //Delete Old/Less Contents Source Item
                                                                                try
                                                                                {
                                                                                    LogLines.Add("* Delete New : " + Path.GetFileName(subdst));
                                                                                    Directory.Delete(subdst, true);
                                                                                }
                                                                                catch (Exception ex)
                                                                                {
                                                                                    LogLines.Add("-----Error----");
                                                                                    DataModel.SaveErrorLog("[" + subdst + "]");
                                                                                    DataModel.SaveErrorLog(ex.Message);
                                                                                    LogLines.Add(subdst);
                                                                                    LogLines.Add(ex.Message);
                                                                                    LogLines.Add("--------------");
                                                                                }
                                                                                continue;
                                                                            }
                                                                        }

                                                                        try
                                                                        {
                                                                            DataModel.MoveDirectory(subdst, moveto);
                                                                        }
                                                                        catch (Exception ex)
                                                                        {
                                                                            LogLines.Add("-----Error----");
                                                                            LogLines.Add(subdst + " -> " + Path.GetFileName(dst));
                                                                            LogLines.Add(ex.Message);
                                                                            LogLines.Add("--------------");
                                                                        }
                                                                    }

                                                                    //Check if item Empty -> Delete
                                                                    subDestin = Directory.GetFiles(dstItem, "*", SearchOption.AllDirectories);

                                                                    if (subDestin.Length <= 0)
                                                                    {
                                                                        try
                                                                        {
                                                                            LogLines.Add("* Delete Empty : " + dstItem);
                                                                            Directory.Delete(dstItem, true);
                                                                        }
                                                                        catch (Exception ex)
                                                                        {
                                                                            LogLines.Add("-----Error----");
                                                                            DataModel.SaveErrorLog("[" + dstItem + "]");
                                                                            DataModel.SaveErrorLog(ex.Message);
                                                                            LogLines.Add(dstItem);
                                                                            LogLines.Add(ex.Message);
                                                                            LogLines.Add("--------------");
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        LogLines.Add("* Count Value Error / Skip");
                                                    }
                                                    LogLines.Add("* Check Contents End");
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LogLines.Add("-----Error----");
                                        DataModel.SaveErrorLog("[" + paths[j] + "]");
                                        DataModel.SaveErrorLog(ex.Message);
                                        LogLines.Add(paths[j]);
                                        LogLines.Add(ex.Message);
                                        LogLines.Add("--------------");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogLines.Add("-----Error----");
                        DataModel.SaveErrorLog("[" + paths[i] + "]");
                        DataModel.SaveErrorLog(ex.Message);
                        LogLines.Add(paths[i]);
                        LogLines.Add(ex.Message);
                        LogLines.Add("--------------");
                    }
                }
            }
        }

        private async Task RunCheckContents()
        {
            if (DataModel == null)
                return;

            if (string.IsNullOrWhiteSpace(PathString.Replace("\n", "")))
                return;

            LogLines.Clear();

            await Task.Delay(0);

            string[] paths = PathString.Split('\n');
            if (paths.Length > 1)
            {
                string target = string.Empty;
                foreach (string path in paths)
                {
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        string src = path.Trim().Trim('\\').Trim('/');
                        if (Path.IsPathRooted(src.Trim().Trim('\\').Trim('/')) == false)
                        {
                            src = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, src.Trim().Trim('\\').Trim('/'));
                        }
                        if (Directory.Exists(src))
                        {
                            target = src;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(target))
                {
                    if (CheckLastPathContents == true)
                    {
                        LogLines.Add("* Check Contents Count");
                        if (int.TryParse(LastPathLessContentsCount, out int count))
                        {
                            LogLines.Add("* Move Contents To Parent / Under Count " + count);

                            string[] items = Directory.GetDirectories(target, "*", SearchOption.TopDirectoryOnly);

                            foreach (string item in items)
                            {
                                string dstItem = Path.Combine(target, Path.GetFileName(item));
                                if (Directory.Exists(dstItem))
                                {
                                    string[] subDestin = Directory.GetDirectories(dstItem, "*", SearchOption.TopDirectoryOnly);
                                    if (subDestin.Length < count)
                                    {
                                        LogLines.Add("* Move " + Path.GetFileName(dstItem) + " contents");
                                        LogLines.Add("-> Parent : " + Path.GetFileName(target));

                                        foreach (string subdst in subDestin)
                                        {
                                            string moveto = Path.Combine(target, Path.GetFileName(subdst));

                                            if (Directory.Exists(moveto))
                                            {
                                                //check Contents Count
                                                string[] srcContents = Directory.GetFiles(subdst, "*", SearchOption.AllDirectories);
                                                LogLines.Add("* Source Count : " + srcContents.Length);

                                                string[] dstContents = Directory.GetFiles(moveto, "*", SearchOption.AllDirectories);
                                                LogLines.Add("* Destin Count : " + dstContents.Length);

                                                if (srcContents.Length > dstContents.Length)
                                                {
                                                    //Delete Old/Less Contents Destination Item
                                                    try
                                                    {
                                                        LogLines.Add("* Delete Old : " + Path.GetFileName(moveto));
                                                        Directory.Delete(moveto, true);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        LogLines.Add("-----Error----");
                                                        DataModel.SaveErrorLog("[" + moveto + "]");
                                                        DataModel.SaveErrorLog(ex.Message);
                                                        LogLines.Add(moveto);
                                                        LogLines.Add(ex.Message);
                                                        LogLines.Add("--------------");
                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    //Delete Old/Less Contents Source Item
                                                    try
                                                    {
                                                        LogLines.Add("* Delete New : " + Path.GetFileName(subdst));
                                                        Directory.Delete(subdst, true);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        LogLines.Add("-----Error----");
                                                        DataModel.SaveErrorLog("[" + subdst + "]");
                                                        DataModel.SaveErrorLog(ex.Message);
                                                        LogLines.Add(subdst);
                                                        LogLines.Add(ex.Message);
                                                        LogLines.Add("--------------");
                                                    }
                                                    continue;
                                                }
                                            }

                                            try
                                            {
                                                DataModel.MoveDirectory(subdst, moveto);
                                            }
                                            catch (Exception ex)
                                            {
                                                LogLines.Add("-----Error----");
                                                LogLines.Add(subdst + " -> " + Path.GetFileName(target));
                                                LogLines.Add(ex.Message);
                                                LogLines.Add("--------------");
                                            }
                                        }

                                        //Check if item Empty -> Delete
                                        subDestin = Directory.GetFiles(dstItem, "*", SearchOption.AllDirectories);

                                        if (subDestin.Length <= 0)
                                        {
                                            try
                                            {
                                                LogLines.Add("* Delete Empty : " + dstItem);
                                                Directory.Delete(dstItem, true);
                                            }
                                            catch (Exception ex)
                                            {
                                                LogLines.Add("-----Error----");
                                                DataModel.SaveErrorLog("[" + dstItem + "]");
                                                DataModel.SaveErrorLog(ex.Message);
                                                LogLines.Add(dstItem);
                                                LogLines.Add(ex.Message);
                                                LogLines.Add("--------------");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            LogLines.Add("* Count Value Error / Skip");
                        }
                        LogLines.Add("* Check Contents End");
                    }
                }
            }
        }
    }
}
