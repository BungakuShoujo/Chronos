﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CFlyFFAddonsExtractor
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region FIELDS

        List<String> FilesText = new List<string>();
        List<String> Files = new List<string>();
        Int32 FilesToExtract { get; set; }

        #endregion

        #region CONSTRUCTORS

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region EVENTS

        private void CONFIG_SELECT_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog _openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "WindSoul File|*.wdf;*.wd1;*.wd2"
            };


            if (_openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!_openFileDialog.FileName.EndsWith(".wdf") && !_openFileDialog.FileName.EndsWith(".wd2") && !_openFileDialog.FileName.EndsWith(".wd1"))
                {
                    this.CONFIG_PATH.Text = "Select a .wdf file";
                    this.DESTINATION_SELECT.IsEnabled = false;
                    return;
                }

                this.CONFIG_PATH.Text = _openFileDialog.FileName;
                this.DESTINATION_SELECT.IsEnabled = true;
            }
        }
        public void LISTER_FILE_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog _openFileDialog = new System.Windows.Forms.OpenFileDialog();
            if (_openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.CONFIG_PATH_Copy.Text = _openFileDialog.FileName;
                this.DESTINATION_SELECT.IsEnabled = true;
            }
        }
        private void CONFIG_SELECT_Copy_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog _folderBrowser = new System.Windows.Forms.FolderBrowserDialog();

            if (_folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.DESTINATION_PATH.Text = _folderBrowser.SelectedPath;
                this.LUA.IsEnabled = true;
                this.START.IsEnabled = true;
            }
        }

        private void START_Click(object sender, RoutedEventArgs e)
        {
            WindsoulDataFile.WdfPackage _package = null;
            Regex regex = new Regex("Model = \"\\S+\"");
            try
            {
                _package = new WindsoulDataFile.WdfPackage(this.CONFIG_PATH.Text);
                _package.Open();

                //this.FilesText = File.ReadAllLines(this.CONFIG_PATH_Copy.Text).Where(x => x.Contains("Model")).ToList();
                //foreach (var text in FilesText)
                //{
                //    var matches = regex.Matches(text);
                //    foreach (Match match in matches)
                //    {
                //        Files.Add(match.Value.Replace("\"", "").Replace("Model = ", ""));
                //        ++this.FilesToExtract;
                //    }
                //}
                /* Extracting Configure file */
                _package.Extract("addons\\Configure");
                FFLua.Decoder.Decompile("Configure", this.DESTINATION_PATH.Text + "\\Configure.lua");
                File.Delete("Configure");

                this.FilesText = File.ReadAllLines(this.DESTINATION_PATH.Text + "\\Configure.lua").ToList();
                this.Files = (from line in FilesText
                    where line.Contains("DoFile(") select new string(line.Substring(line.IndexOf("DoFile(\"", StringComparison.Ordinal) + 8)
                        .TakeWhile(c => c != '\"').ToArray()))
                    .Aggregate("", (current, filename) => current + filename + "\r\n")
                    .Split(new Char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                this.Files = Files.Concat(from line in FilesText
                                          where line.Contains("LoadFile(")
                                          select line.Split('\"')[1]).ToList();
                this.Files = Files.Concat(new string[] { "project\\MotionList", "project\\WndRes", "project\\WndCfg", "project\\MtrlExCfg" }).ToList();
                this.FilesToExtract = 0;
                foreach (String _file in Files)
                {
                    if (_file.EndsWith(".lua") == false)
                    {
                        ++this.FilesToExtract;
                    }
                }

                this.CONFIG_SELECT.IsEnabled = false;
                this.DESTINATION_SELECT.IsEnabled = false;
                this.LUA.IsEnabled = false;
                this.START.IsEnabled = false;

                Thread _thread = new Thread(new ParameterizedThreadStart(ExtractFiles));
                _thread.Start(_package);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Wdf package error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Extract files
        /// </summary>
        /// <param name="package"></param>
        private void ExtractFiles(Object package)
        {
            Int32 _extracted = 0;

            foreach (String _file in Files)
            {
                if (!_file.EndsWith(".lua"))
                {
                    String _newFile = _file.Replace("\\\\", "\\");

                    /* Update the current file */
                    this.CURRENT_FILE.Dispatcher.Invoke(new Action(() =>
                        {
                            this.CURRENT_FILE.Content = _newFile;
                        }));

                    /* Gets the destination path */
                    String _destinationPath = null;
                    this.DESTINATION_PATH.Dispatcher.Invoke(new Action(() =>
                    {
                        _destinationPath = this.DESTINATION_PATH.Text;
                    }));

                    /* Gets the lua action */
                    Boolean _lua = false;
                    this.LUA.Dispatcher.Invoke(new Action(() =>
                        {
                            _lua = this.LUA.IsChecked == true ? true : false;
                        }));

                    /* Extracts and converts if need */
                    FileInfo _info = new FileInfo(_destinationPath + "\\" + _newFile);
                    _info.Directory.Create();

                    if ((package as WindsoulDataFile.WdfPackage).Extract(_newFile, _info.Directory.FullName) == true && !_info.Name.EndsWith(".lua") && _lua == true)
                    {
                        FFLua.Decoder.Decompile(_info.FullName, _info.FullName + ".lua");
                    }
                    //_info.Delete();
                    ++_extracted;

                    /* Update progress bar value */
                    this.TOTAL_PROGRESS.Dispatcher.Invoke(new Action(() =>
                        {
                            this.TOTAL_PROGRESS.Value = _extracted * 100 / this.FilesToExtract;
                        }));
                }
            }
            (package as WindsoulDataFile.WdfPackage).Close();
            MessageBox.Show("Wdf package extracted", "Wdf package error", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        #endregion
    }
}
