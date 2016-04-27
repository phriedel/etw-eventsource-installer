using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using EventSourceInstallerApp.Model;
using EventSourceInstallerLib;
using Microsoft.Win32;

namespace EventSourceInstallerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Member

        EventSourceInstaller _installer;
        Dictionary<string, ManifestBundle> _manifestDictionary;
        IEnumerable _dataSource;

        string _initialPath = Directory.Exists(@"c:\src") ? @"c:\src" : "";

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            _manifestDictionary = new Dictionary<string, ManifestBundle>();
            _installer = new EventSourceInstaller();
            _installer._newStatuMessageEvent += new StatusMessageEventHandler(UpdateBundlesStatus);
        }

        private void UpdateBundlesStatus(object sender, EventArgs e)
        {
            var statusEventMessage = sender as StatusEventMessage;

            if (_manifestDictionary.ContainsKey(statusEventMessage.ManifestKey))
            {
                _manifestDictionary[statusEventMessage.ManifestKey].StatusMessage = statusEventMessage.Message;
            }
        }

        #endregion

        #region Events

        #region Drag & Drop

        private void ManifestListView_Drop(object sender, DragEventArgs e)
        {
            try
            {
                DropFiles(e);
            }
            catch (Exception ex)
            {
                RemoveFiles();
                MessageBox.Show(ex.Message, "Unhandled exception while adding your files.");
            }
            finally
            {
                UpdateDataSource();
                ChangeButtonStatus();
            }
        }

        private void ManifestListView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }

        private void ManifestListView_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }

        #endregion

        #region Buttons

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var item in ManifestListView.ItemsSource)
                {
                    var bundle = item as ManifestBundle;
                    _installer.Install(bundle.Manifest, bundle.Dll, bundle.SourcePath, InstallationPathTextBox.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unhandled exception during manifest installation.");
            }
            finally
            {
                UpdateDataSource();
            }
        }

        private void UninstallButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var item in ManifestListView.ItemsSource)
                {
                    var bundle = item as ManifestBundle;
                    _installer.Uninstall(Path.Combine(bundle.SourcePath, bundle.Manifest));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unhandled exception during manifest uninstall.");
            }
            finally
            {
                UpdateDataSource();
            }
        }

        private void RemoveFilesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RemoveFiles();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unhandled exception while removing all files.");
            }
            finally
            {
                UpdateDataSource();
                ChangeButtonStatus();
            }
        }

        private void AddFilesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddFiles();
            }
            catch (Exception ex)
            {
                RemoveFiles();
                MessageBox.Show(ex.Message, "Unhandled exception while adding your files.");
            }
            finally
            {
                UpdateDataSource();
                ChangeButtonStatus();
            }
        }

        private void ChangeInstallationPathButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ChangeInstallationPath();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unhandled exception while changing the installation path.");
            }
        }

        private void LoadInstalledManifestsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RemoveFiles();
                AddInstalledManifests(InstallationPathTextBox.Text);
            }
            catch (Exception ex)
            {
                RemoveFiles();
                MessageBox.Show(ex.Message, "Unhandled exception while loading your installed manifest files.");
            }
            finally
            {
                UpdateDataSource();
                ChangeButtonStatus();
            }
        }

        #endregion

        #region Keys

        private void ManifestListView_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Delete)
                {
                    RemoveSelectedFiles();
                }
            }
            catch (Exception ex)
            {
                RemoveFiles();
                MessageBox.Show(ex.Message, "Unhandled exception while removing selected file.");
            }
            finally
            {
                UpdateDataSource();
                ChangeButtonStatus();
            }
        }

        #endregion

        #endregion

        #region Utilities

        private void DropFiles(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //You can have multiple files here
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                CreateAndAddBundles(files);
            }
        }

        private void AddFiles()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.InitialDirectory = _initialPath;
            openFileDialog.Filter = "Manifest file|*.man| Manifest dll|*.dll";
            openFileDialog.Title = "Add your *.man files. Regarding *.dlls will be added automatically.";

            if (openFileDialog.ShowDialog() == true)
            {
                //You can have multiple files here
                var files = openFileDialog.FileNames;

                CreateAndAddBundles(openFileDialog.FileNames);

                if (files.Length > 0)
                {
                    _initialPath = Path.GetDirectoryName(files[0]);
                }
            }
        }

        private void ChangeInstallationPath()
        {
            var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.ShowNewFolderButton = true;
            folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
            folderBrowserDialog.Description = "Choose your EventSource installation folder.";
            folderBrowserDialog.ShowDialog();

            if (folderBrowserDialog.SelectedPath != String.Empty)
            {
                InstallationPathTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void AddInstalledManifests(string installationDirectory)
        {
            string[] files = Directory.GetFiles(installationDirectory, "*.man");
            CreateAndAddBundles(files);
        }

        private void RemoveFiles()
        {
            _manifestDictionary = new Dictionary<string, ManifestBundle>();
        }

        private void RemoveSelectedFiles()
        {
            foreach (var item in ManifestListView.SelectedItems)
            {
                var key = (item as ManifestBundle).Manifest;
                key = Path.GetFileNameWithoutExtension(key);

                if (_manifestDictionary.ContainsKey(key))
                {
                    _manifestDictionary.Remove(key);
                }
            }
        }

        private void CreateAndAddBundles(string[] files)
        {
            foreach (var item in files)
            {
                #region Create Manifest Bundle

                ManifestBundle bundle = new ManifestBundle();
                bundle.SourcePath = Path.GetDirectoryName(item);
                var fileExtension = Path.GetExtension(item).ToLower();
                var fileName = Path.GetFileName(item);

                if (fileExtension == ".man")
                {
                    bundle.Manifest = fileName;
                    string dllFile = Path.GetDirectoryName(item) + "\\" + Path.GetFileNameWithoutExtension(item) + ".dll";
                    bundle.Dll = File.Exists(dllFile) ? Path.GetFileName(dllFile) : string.Empty;
                    AddFileToBundleCollection(_manifestDictionary, item, bundle);
                }
                else if (fileExtension == ".dll")
                {
                    bundle.Manifest = String.Empty;
                    bundle.Dll = fileName;
                    AddFileToBundleCollection(_manifestDictionary, item, bundle);
                }

                #endregion
            }
        }

        private static void AddFileToBundleCollection(Dictionary<string, ManifestBundle> dic, string item, ManifestBundle bundle)
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(item);

            if (dic.ContainsKey(fileNameWithoutExtension))
            {

                if (dic[fileNameWithoutExtension].SourcePath == bundle.SourcePath)
                {
                    if (dic[fileNameWithoutExtension].Manifest == String.Empty
                        && bundle.Manifest != String.Empty)
                    {
                        dic[fileNameWithoutExtension].Manifest = bundle.Manifest;
                    }

                    if (dic[fileNameWithoutExtension].Dll == String.Empty
                        && bundle.Dll != String.Empty)
                    {
                        dic[fileNameWithoutExtension].Dll = bundle.Dll;
                    }
                }
            }
            else
            {
                dic.Add(fileNameWithoutExtension, bundle);
            }
        }

        private void UpdateDataSource()
        {
            _dataSource = from item in _manifestDictionary
                          select new ManifestBundle
                          {
                              Manifest = item.Value.Manifest,
                              Dll = item.Value.Dll,
                              SourcePath = item.Value.SourcePath,
                              StatusMessage = item.Value.StatusMessage
                          };

            ManifestListView.ItemsSource = _dataSource;
        }

        private void ChangeButtonStatus()
        {
            RemoveFilesButton.IsEnabled = !ManifestListView.Items.IsEmpty;
            InstallButton.IsEnabled = !ManifestListView.Items.IsEmpty;
            UninstallButton.IsEnabled = !ManifestListView.Items.IsEmpty;
        }

        #endregion
    }
}