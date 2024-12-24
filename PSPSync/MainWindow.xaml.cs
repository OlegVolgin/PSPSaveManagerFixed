﻿using MediaDevices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PSPSync
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<IStorageDevice> storageDevices = new List<IStorageDevice>();
        List<SaveMeta> sd1Saves, sd2Saves;
        bool sd1offline, sd2offline;
        private bool mgrEnabled = true;
        public bool appInit = false;

        public MainWindow()
        {
            InitializeComponent();
            GlobalConfig.LoadConfig();
            UpdateStorageDeviceList();
            UpdateStorageDeviceItems();
            EventHandlers();
            KeyboardInput();
            
            this.SizeChanged += delegate
            {
                UpdateSizes();
            };
            appInit = true;
        }

        public void UpdateSizes() {
            double onePanelWidth = (this.Width / 2d) - (SD1toSD2.Width - 14);
            SD1s.Width = onePanelWidth - SD1s.Margin.Left;
            StorageDevice1.Width = onePanelWidth - SD1s.Margin.Left;
            SD2s.Width = onePanelWidth - SD2s.Margin.Right;
            StorageDevice2.Width = onePanelWidth - SD2s.Margin.Right;

            Thickness marginMod = SD1toSD2.Margin;
            marginMod.Left = onePanelWidth;
            SD1toSD2.Margin = marginMod;

            marginMod.Top = SD2toSD1.Margin.Top;
            SD2toSD1.Margin = marginMod;

            marginMod.Top = Sync.Margin.Top;
            Sync.Margin = marginMod;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            UpdateSizes();
            base.OnStateChanged(e);
        }

        public void KeyboardInput() {
            foreach (Control a in new Control[] { SD1s, SD2s, StorageDevice1, StorageDevice2, SD1toSD2, SD2toSD1, SD1Delete, SD2Delete})
            {
                a.PreviewKeyDown += (object sender, KeyEventArgs e) =>
                {
                    e.Handled = !mgrEnabled;
                };
            }
        }

        public void EventHandlers() {
            WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");

            ManagementEventWatcher insertWatcher = new ManagementEventWatcher(insertQuery);
            insertWatcher.EventArrived += new EventArrivedEventHandler(DeviceInsertedEvent);
            insertWatcher.Start();

            WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            ManagementEventWatcher removeWatcher = new ManagementEventWatcher(removeQuery);
            removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceRemovedEvent);
            removeWatcher.Start();
        }

        public void DeviceInsertedEvent(object sender, EventArrivedEventArgs e) {
            Dispatcher.Invoke(delegate
            {
                UpdateStorageDeviceList();
                UpdateStorageDeviceItems();
            });
        }
        public void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            Dispatcher.Invoke(delegate
            {
                UpdateStorageDeviceList();
                UpdateStorageDeviceItems();
            });
        }

        public void UpdateStorageDeviceList() {
            

            string sd1name = "", sd2name = "";


            if (StorageDevice1.SelectedIndex != -1) {
                sd1name = storageDevices[StorageDevice1.SelectedIndex].GetDeviceName();
            }
            if (StorageDevice2.SelectedIndex != -1) {
                sd2name = storageDevices[StorageDevice2.SelectedIndex].GetDeviceName();
            }

            ScanDrives();

            StorageDevice1.SelectedIndex = -1;
            StorageDevice2.SelectedIndex = -1;
            StorageDevice1.Items.Clear();
            StorageDevice2.Items.Clear();
            foreach (IStorageDevice a in storageDevices) {
                StorageDevice1.Items.Add(a.GetDeviceName());
                StorageDevice2.Items.Add(a.GetDeviceName());
            }

            if (sd1name != "")
            {
                int index = 0;
                for (int x = 0; x != storageDevices.Count; x++)
                {
                    if (storageDevices[x].GetDeviceName() == sd1name)
                    {
                        index = x;
                        break;
                    }
                }
                StorageDevice1.SelectedIndex = index;
            }
            else {
                StorageDevice1.SelectedIndex = 0;
            }
            if (sd2name != "")
            {
                int index = 0;
                for (int x = 0; x != storageDevices.Count; x++)
                {
                    if (storageDevices[x].GetDeviceName() == sd2name)
                    {
                        index = x;
                        break;
                    }
                }
                StorageDevice2.SelectedIndex = index;
            }
            else {
                StorageDevice2.SelectedIndex = 0;
            }

            
            
        }

        public void UpdateStorageDeviceItems(int row = 0) {
            
            if (StorageDevice1.SelectedIndex != -1)
            {
                if (storageDevices[StorageDevice1.SelectedIndex].GetDeviceSpeed() == GeneralDeviceSpeed.Fast || row != 2)
                {
                    SD1s.Items.Clear();
                    sd1Saves = storageDevices[StorageDevice1.SelectedIndex].ScanSaves();
                    if (sd1Saves == null)
                    {
                        SD1s.Items.Add("Offline... (Directory not found)");
                        SD1s.IsEnabled = false;
                        sd1offline = true;
                    }
                    else
                    {
                        SD1s.IsEnabled = true;
                        sd1offline = false;
                        foreach (SaveMeta a in sd1Saves)
                        {
                            SD1s.Items.Add(new SaveListItem(a));
                        }
                    }
                }
            }
            else {
                SD1s.Items.Clear();
            }
            if (StorageDevice2.SelectedIndex != -1)
            {
                if (storageDevices[StorageDevice2.SelectedIndex].GetDeviceSpeed() == GeneralDeviceSpeed.Fast || row != 1)
                {
                    SD2s.Items.Clear();
                    sd2Saves = storageDevices[StorageDevice2.SelectedIndex].ScanSaves();
                    if (sd2Saves == null)
                    {
                        SD2s.Items.Add("Offline... (Directory not found)");
                        SD2s.IsEnabled = false;
                        sd2offline = true;
                    }
                    else
                    {
                        SD2s.IsEnabled = true;
                        sd2offline = false;
                        foreach (SaveMeta a in sd2Saves)
                        {
                            SD2s.Items.Add(new SaveListItem(a));
                        }
                    }
                }
            }
            else {
                SD2s.Items.Clear();
            }
        }

        public void ScanDrives() {
            storageDevices.Clear();
            foreach (SavePath a in GlobalConfig.paths) {
                if (a.path.StartsWith("ftp://"))
                {
                    int indexofslash = a.path.IndexOf("/", 8);
                    storageDevices.Add(new FTPSaveDir(a.path.Substring(indexofslash), a.path.Substring(0, indexofslash), a.name));
                }
                else
                {
                    storageDevices.Add(new PSPSaveDir(a.path, a.name));
                }
            }
            string cmaPath = Environment.GetEnvironmentVariable("USERPROFILE") + "/Documents/PS Vita/PSAVEDATA/";
            if (Directory.Exists(cmaPath)) {
                foreach (string a in Directory.GetDirectories(cmaPath)) {
                    storageDevices.Add(new PSPSaveDir(a, $"PS Vita Content Manager [{GetGameID(a)}]"));
                }
            }

            foreach (string a in Directory.GetLogicalDrives()) {

                if (Directory.Exists(a + "/pspemu/PSP/"))
                {
                    storageDevices.Add(new PSPSaveDir(a + "/pspemu/PSP/SAVEDATA/", $"[{a}] PS Vita (SD or USB)"));
                }
                if (Directory.Exists(a + "/PSP/")) {
                    storageDevices.Add(new PSPSaveDir(a + "/PSP/SAVEDATA/", $"[{a}] PSP Memory Stick"));
                }
                if (Directory.Exists(a + "/switch/ppsspp/config/ppsspp/PSP/")) {
                    storageDevices.Add(new PSPSaveDir(a + "/switch/ppsspp/config/ppsspp/PSP/SAVEDATA/", $"[{a}] Switch"));
                }
            }

            foreach (MediaDevice a in MTPDevice.ScanMTPs()) {
                MTPDevice b = new MTPDevice(a);
                if (b.saveDirs.Count == 0)
                {
                    b.Close();
                }
                else
                {
                    foreach (MTPSaveDir c in b.saveDirs)
                    {
                        storageDevices.Add(c);
                    }
                }
            }
        }

        public bool IsOnTheSameDevice() {
            return StorageDevice1.SelectedIndex == StorageDevice2.SelectedIndex;
        }

        public SaveMeta GetMetaFromID(List<SaveMeta> list, string ID) {
            //Console.WriteLine("Looking for " + ID);
            foreach (SaveMeta a in list) {
                //Console.WriteLine("Compare " + a.directory);
                if (a.directory.EndsWith(ID)) {
                    return a;
                }
            }
            return null;
        }

        private void SD1toSD2_Click(object sender, RoutedEventArgs e)
        {
            if (CannotCopy(SD1s)) {
                return;
            }
            TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
            IStorageDevice sd = storageDevices[StorageDevice1.SelectedIndex];
            SaveMeta currentmeta = sd1Saves[SD1s.SelectedIndex];
            NamedStream[] files = sd.ReadSave(currentmeta.directory);
            CopySave(currentmeta, GetMetaFromID(sd2Saves, GetGameID(currentmeta.directory)), files, storageDevices[StorageDevice2.SelectedIndex], 2);
        }

        public bool CannotCopy(ListBox savelist) {
            if (sd1offline || sd2offline) {
                MessageBox.Show("Device offline");
                return true;
            }
            if (IsOnTheSameDevice()) {
                MessageBox.Show("Cannot copy to the same device");
                return true;
            }
            if (StorageDevice2.SelectedIndex == -1 || StorageDevice1.SelectedIndex == -1) {
                MessageBox.Show("Select a device from the drop down menu");
                return true;
            }
            if (savelist.SelectedIndex == -1) {
                MessageBox.Show("Select a save to copy");
                return true;
            }
            return false;
        }

        private void SD2toSD1_Click(object sender, RoutedEventArgs e)
        {
            if (CannotCopy(SD2s))
            {
                return;
            }
            TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
            IStorageDevice sd = storageDevices[StorageDevice2.SelectedIndex];
            SaveMeta currentmeta = sd2Saves[SD2s.SelectedIndex];
            NamedStream[] files = sd.ReadSave(currentmeta.directory);
            CopySave(currentmeta, GetMetaFromID(sd1Saves, GetGameID(currentmeta.directory)), files, storageDevices[StorageDevice1.SelectedIndex], 1);
        }

        public void SetMgrEnabled(bool enabled) {
            AllDisabled.Visibility = (enabled ? Visibility.Hidden : Visibility.Visible);
            mgrEnabled = enabled;
            //SD1s.IsEnabled = enabled;
            //SD2s.IsEnabled = enabled;
        }

        public void CopySave(SaveMeta srcMeta, SaveMeta otherMeta, NamedStream[] files, IStorageDevice dest, int updateDevice) {
            SetMgrEnabled(false);
            string id = GetGameID(srcMeta.directory);
            if (dest.HasSave(id))
            {
                if (otherMeta == null)
                {
                    MessageBoxResult a = MessageBox.Show("Save data in destination directory is corrupted (no PARAM.SFO file). Overwrite?", "", MessageBoxButton.YesNo);
                    if (a == MessageBoxResult.Yes)
                    {
                        dest.DeleteSave(GetGameID(srcMeta.directory));
                        dest.WriteSave(GetGameID(srcMeta.directory), files);
                        MessageBox.Show("Copied");
                        
                    }
                    SetMgrEnabled(true);
                }
                else
                {
                    CompareWindow a = new CompareWindow(otherMeta, srcMeta, dest.GetDeviceName(),
                        delegate
                        {
                            try
                            {
                                dest.DeleteSave(GetGameID(srcMeta.directory));
                                dest.WriteSave(GetGameID(srcMeta.directory), files);
                                MessageBox.Show("Copied");
                            }
                            catch (IOException)
                            {
                                MessageBox.Show("Copy operation failed");
                            }

                            foreach (NamedStream b in files)
                            {
                                b.stream.Dispose();
                            }
                        },
                        delegate
                        {
                            TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                            SetMgrEnabled(true);
                            UpdateStorageDeviceItems(updateDevice);
                        });
                    a.ShowDialog();
                }
                /*MessageBoxResult a = MessageBox.Show($"Destination already has a {GetGameID(srcMeta.directory)} save file. Overwrite?", "Warning", MessageBoxButton.YesNo);
                if (a == MessageBoxResult.No)
                {
                    SD1s.IsEnabled = true;
                    SD2s.IsEnabled = true;
                    return;
                }
                else
                {
                    dest.DeleteSave(GetGameID(srcMeta.directory));
                }*/
            }
            else {
                try
                {
                    dest.WriteSave(GetGameID(srcMeta.directory), files);
                    MessageBox.Show("Copied");
                }
                catch (IOException)
                {
                    MessageBox.Show("Copy operation failed");
                }

                foreach (NamedStream b in files)
                {
                    b.stream.Dispose();
                }
                TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                SetMgrEnabled(true);
                UpdateStorageDeviceItems(updateDevice);
            }
        }

        public static string GetGameID(string directory) {
            while (directory.EndsWith("/") || directory.EndsWith(@"\\")) {
                directory = directory.Substring(0, directory.Length - 1);
            }
            for (int fnw = directory.Length - 1; fnw >= 0; fnw--)
            {
                if (directory[fnw] == '/' || directory[fnw] == '\\')
                {
                    return directory.Substring(fnw + 1);
                }
            }
            throw new FormatException();
        }

        private void Sync_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This is not implemented yet");
        }

        private void Config_Click(object sender, RoutedEventArgs e)
        {
            Config a = new Config();
            a.Closed += delegate
            {
                UpdateStorageDeviceList();
                UpdateStorageDeviceItems();
            };
            a.Show();
        }

        private void SD1Delete_Click(object sender, RoutedEventArgs e)
        {
            if (SD1s.SelectedIndex == -1) {
                return;
            }
            SaveMeta srcMeta = sd1Saves[SD1s.SelectedIndex];
            IStorageDevice sd = storageDevices[StorageDevice1.SelectedIndex];
            MessageBoxResult a = MessageBox.Show($"Delete {GetGameID(srcMeta.directory)} save file from {sd.GetDeviceName()}?", "Warning", MessageBoxButton.YesNo);
            if (a == MessageBoxResult.Yes) {
                sd.DeleteSave(GetGameID(srcMeta.directory));
                MessageBox.Show("Save file deleted");
                UpdateStorageDeviceItems();
            }
        }

        private void SD2Delete_Click(object sender, RoutedEventArgs e)
        {
            if (SD2s.SelectedIndex == -1)
            {
                return;
            }
            SaveMeta srcMeta = sd2Saves[SD2s.SelectedIndex];
            IStorageDevice sd = storageDevices[StorageDevice2.SelectedIndex];
            MessageBoxResult a = MessageBox.Show($"Delete {GetGameID(srcMeta.directory)} save file from {sd.GetDeviceName()}?", "Warning", MessageBoxButton.YesNo);
            if (a == MessageBoxResult.Yes)
            {
                sd.DeleteSave(GetGameID(srcMeta.directory));
                MessageBox.Show("Save file deleted");
                UpdateStorageDeviceItems();
            }
        }

        private void RescanDrives_Click(object sender, RoutedEventArgs e)
        {
            UpdateStorageDeviceList();
            UpdateStorageDeviceItems();
        }

        private void StorageDevice1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StorageDevice1.SelectedIndex != -1 && appInit)
                UpdateStorageDeviceItems();
        }

        private void StorageDevice2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StorageDevice2.SelectedIndex != -1 && appInit)
                UpdateStorageDeviceItems();
        }
    }
}