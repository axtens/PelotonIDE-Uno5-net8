﻿using EncodingChecker;

using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;

namespace PelotonIDE.Presentation
{
    public sealed partial class MainPage : Page
    {
        private async Task<bool> AreYouSureToClose()
        {
            ContentDialog dialog = new()
            {
                XamlRoot = this.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "Document changed but not saved. Close?",
                PrimaryButtonText = "No",
                SecondaryButtonText = "Yes"
            };
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Secondary) { return true; }
            if (result == ContentDialogResult.Primary) { return false; }
            return false;
        }

        private async Task<bool> AreYouSureYouWantToRunALongTimeSilently()
        {
            string il = Type_1_GetVirtualRegistry<string>("InterfaceLanguageName");
            Dictionary<string, string> global = LanguageSettings[il]["GLOBAL"];
            Dictionary<string, string> frmMain = LanguageSettings[il]["frmMain"];
            CultureInfo cultureInfo = new(global["Locale"]);

            string tag = new string[] { "mnu20Seconds", "mnu100Seconds", "mnu200Seconds", "mnu1000Seconds", "mnuInfinite" }[Type_3_GetInFocusTab<long>("Timeout")];

            string title = $"{frmMain["mnuGo"]} '{frmMain[tag]}' {frmMain["mnuTimeout"].ToLower()}, '{frmMain["mnuQuiet"].ToLower(cultureInfo)}' {frmMain["mnuRunningMode"].ToLower(cultureInfo)}?";
            string secondary = $"'{frmMain["mnuVerbose"]}' {frmMain["mnuTimeout"]}";

            ContentDialog dialog = new()
            {
                XamlRoot = this.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = title,
                PrimaryButtonText = global["1207"],
                SecondaryButtonText = secondary,
                CloseButtonText = global["1201"],
            };

            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                return true;
            }
            if (result == ContentDialogResult.Secondary)
            {
                Type_3_UpdateInFocusTabSettings<long>("Quietude", true, 1);
                UpdateStatusBarFromInFocusTab();
                return true;
            }
            return false;
        }

        private void ChooseEngine_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem me = (MenuFlyoutItem)sender;
            switch ((string)me.Tag)
            {
                case "P2":
                    MenuItemHighlightController(mnuNewEngine, false);
                    MenuItemHighlightController(mnuOldEngine, true);
                    Engine = 2;
                    break
;
                case "P3":
                    MenuItemHighlightController(mnuNewEngine, true);
                    MenuItemHighlightController(mnuOldEngine, false);
                    Engine = 3;
                    break;
            }
            interpreter.Text = (string)me.Tag;
            if (AnInFocusTabExists())
            {
                Type_2_UpdatePerTabSettings("Engine", true, Engine);
                Type_3_UpdateInFocusTabSettings("Engine", true, Engine);
            }
        }

        private async void Close()
        {
            if (tabControl.MenuItems.Count > 0)
            {
                CustomTabItem navigationViewItem = (CustomTabItem)tabControl.SelectedItem;
                CustomRichEditBox currentRichEditBox = _richEditBoxes[navigationViewItem.Tag];
                // var t1 = tab1;
                if (currentRichEditBox.IsDirty)
                {
                    if (!await AreYouSureToClose()) return;
                }
                _richEditBoxes.Remove(navigationViewItem.Tag);
                tabControl.MenuItems.Remove(tabControl.SelectedItem);
                if (tabControl.MenuItems.Count > 0)
                {
                    tabControl.SelectedItem = tabControl.MenuItems[tabControl.MenuItems.Count - 1];
                }
                else
                {
                    tabControl.Content = null;
                    tabControl.SelectedItem = null;
                }
                UpdateCommandLineInStatusBar();
            }
        }

        private void CopyText()
        {
            CustomTabItem navigationViewItem = (CustomTabItem)tabControl.SelectedItem;
            CustomRichEditBox currentRichEditBox = _richEditBoxes[navigationViewItem.Tag];
            string selectedText = currentRichEditBox.Document.Selection.Text;
            DataPackage dataPackage = new();
            dataPackage.SetText(selectedText);
            Clipboard.SetContent(dataPackage);
        }

        private void CreateNewRichEditBox()
        {
            CustomRichEditBox richEditBox = new()
            {
                IsDirty = false,
            };
            richEditBox.KeyDown += RichEditBox_KeyDown;
            richEditBox.AcceptsReturn = true;
            CustomTabItem navigationViewItem = new()
            {
                Content = LanguageSettings[LocalSettings.Values["InterfaceLanguageName"].ToString()!]["GLOBAL"]["Document"] + " " + TabControlCounter,  //(tabControl.MenuItems.Count + 1),
                //Content = "Tab " + (tabControl.MenuItems.Count + 1),
                Tag = "Tab" + TabControlCounter,//(tabControl.MenuItems.Count + 1),
                IsNewFile = true,
                TabSettingsDict = ClonePerTabSettings(PerTabInterpreterParameters),
                Height = 30
            };
            richEditBox.Tag = navigationViewItem.Tag;
            tabControl.Content = richEditBox;
            _richEditBoxes[richEditBox.Tag] = richEditBox;
            tabControl.MenuItems.Add(navigationViewItem);
            tabControl.SelectedItem = navigationViewItem; // in focus?
            richEditBox.Focus(FocusState.Keyboard);
            UpdateLanguageNameInStatusBar(navigationViewItem.TabSettingsDict);
            UpdateCommandLineInStatusBar();

            AssertSelectedOutputTab();
            TabControlCounter += 1;
        }

        private void Cut()
        {
            CustomTabItem navigationViewItem = (CustomTabItem)tabControl.SelectedItem;
            CustomRichEditBox currentRichEditBox = _richEditBoxes[navigationViewItem.Tag];
            string selectedText = currentRichEditBox.Document.Selection.Text;
            DataPackage dataPackage = new();
            dataPackage.SetText(selectedText);
            Clipboard.SetContent(dataPackage);
            currentRichEditBox.Document.Selection.Delete(Microsoft.UI.Text.TextRangeUnit.Character, 1);
        }

        private void EditCopy_Click(object sender, RoutedEventArgs e)
        {
            CopyText();
        }

        private void EditCut_Click(object sender, RoutedEventArgs e)
        {
            Cut();
        }

        private async void EditPaste_Click(object sender, RoutedEventArgs e)
        {
            Paste();
        }

        private void EditSelectAll_Click(object sender, RoutedEventArgs e)
        {
            SelectAll();
        }

        private void FileClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void FileNew_Click(object sender, RoutedEventArgs e)
        {
            CreateNewRichEditBox();
        }

        private async void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            Open();
        }

        private async void FileSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private async void FileSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveAs();
        }

        private async void HandleInterfaceLanguageChange(string langName)
        {
            Telemetry.SetEnabled(false);

            Dictionary<string, Dictionary<string, string>> selectedLanguage = LanguageSettings[langName];
            Telemetry.Transmit("Changing interface language to", langName, long.Parse(selectedLanguage["GLOBAL"]["ID"]));

            SetMenuText(selectedLanguage["frmMain"]);
            Type_1_UpdateVirtualRegistry("InterfaceLanguageName", langName);
            Type_1_UpdateVirtualRegistry("InterfaceLanguageID", long.Parse(selectedLanguage["GLOBAL"]["ID"]));

            InterfaceLanguageSelectionBuilder(mnuSelectLanguage, Internationalization_Click);
            InterpreterLanguageSelectionBuilder(mnuRun, "mnuLanguage", MnuLanguage_Click);
            CustomTabItem navigationViewItem = (CustomTabItem)tabControl.SelectedItem;
            if (navigationViewItem.TabSettingsDict != null)
            {
                UpdateLanguageNameInStatusBar(navigationViewItem.TabSettingsDict);
                UpdateStatusBarFromInFocusTab();
            }
        }

        private void HelpAbout_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = this.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "PelotonIDE v1.0",
                Content = "", // Based on original code by\r\nHakob Chalikyan <hchalikyan3@gmail.com>",
                CloseButtonText = "OK"
            };
            _ = dialog.ShowAsync();
        }

        private void InsertCodeTemplate_Click(object sender, RoutedEventArgs e)
        {
            bool VariableLength = Type_1_GetVirtualRegistry<bool>("VariableLength");
            CustomTabItem navigationViewItem = (CustomTabItem)tabControl.SelectedItem;
            CustomRichEditBox currentRichEditBox = _richEditBoxes[navigationViewItem.Tag];
            ITextSelection selection = currentRichEditBox.Document.Selection;
            if (selection != null)
            {
                MenuFlyoutItem menuFlyoutItem = (MenuFlyoutItem)sender;
                selection.StartPosition = selection.EndPosition;
                switch (menuFlyoutItem.Name)
                {
                    case "MakePeloton":
                        if (VariableLength)
                        {
                            selection.Text = "<# ></#>";
                        }
                        else
                        {
                            selection.Text = "<@ ></@>";
                        }
                        break;

                    case "MakePelotonVariableLength":
                        if (VariableLength)
                        {
                            selection.Text = "<@ ></@>";
                        }
                        else
                        {
                            selection.Text = "<# ></#>";
                        }
                        break;
                }
                selection.EndPosition = selection.StartPosition;
                currentRichEditBox.Document.Selection.Move(TextRangeUnit.Character, 3);
            }
        }

        private void Internationalization_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem me = (MenuFlyoutItem)sender;
            string name = me.Name;
            mnuSelectLanguage.Items.ForEach(item =>
            {
                MenuItemHighlightController((MenuFlyoutItem)item, item.Name == name);
            });
            HandleInterfaceLanguageChange(name);
        }
        private void InterpretMenu_Quietude_Click(object sender, RoutedEventArgs e)
        {
            string il = Type_1_GetVirtualRegistry<string>("InterfaceLanguageName");
            Dictionary<string, string> global = LanguageSettings[il]["GLOBAL"];
            Dictionary<string, string> frmMain = LanguageSettings[il]["frmMain"];
            CultureInfo cultureInfo = new(global["Locale"]);

            long quietude = 0;
            foreach (MenuFlyoutItemBase? item in from key in new string[] { "mnuQuiet", "mnuVerbose", "mnuVerbosePauseOnExit" }
                                                 let items = from item in mnuRunningMode.Items where item.Name == key select item
                                                 from item in items
                                                 select item)
            {
                MenuItemHighlightController((MenuFlyoutItem)item, false);
            }

            MenuFlyoutItem? me = sender as MenuFlyoutItem;
            string clicked = me.Name;
            mnuRunningMode.Tag = clicked;
            switch (clicked)
            {
                case "mnuQuiet":
                    MenuItemHighlightController(me, true);
                    quietude = 0;
                    break;

                case "mnuVerbose":
                    MenuItemHighlightController(me, true);
                    quietude = 1;
                    break;

                case "mnuVerbosePauseOnExit":
                    MenuItemHighlightController(me, true);
                    quietude = 2;
                    break;
            }

            CustomTabItem navigationViewItem = (CustomTabItem)tabControl.SelectedItem;
            CustomRichEditBox currentRichEditBox = _richEditBoxes[navigationViewItem.Tag];

            Type_1_UpdateVirtualRegistry("Quietude", quietude);
            Type_2_UpdatePerTabSettings("Quietude", true, quietude);
            _ = Type_3_UpdateInFocusTabSettingsIfPermittedAsync<long>("Quietude", true, quietude, $"{frmMain["mnuUpdate"]} {frmMain["mnuRunningMode"].ToLower(cultureInfo)} = '{frmMain[me.Name].ToLower(cultureInfo)}'");
            //Type_3_UpdateInFocusTabSettings("Quietude", true, quietude);
            UpdateCommandLineInStatusBar();

            AssertSelectedOutputTab();

        }

        private void InterpretMenu_Rendering_Click(object sender, RoutedEventArgs e)
        {
            Telemetry.SetEnabled(false);
            MenuFlyoutItem me = (MenuFlyoutItem)sender;

            SolidColorBrush black = new(Colors.Black);
            SolidColorBrush white = new(Colors.White);

            mnuRendering.Items.ForEach(item => MenuItemHighlightController((MenuFlyoutItem)item!, false));

            string render = (string)me.Tag;

            List<string> renderers = [.. Type_1_GetVirtualRegistry<string>("Rendering").Split(',')];
            if (renderers.Contains(render))
            {
                renderers.Remove(render);
            }
            else
            {
                renderers.Add(render);
            }

            renderers.ForEach(renderer =>
            {
                mnuRendering.Items.ForEach(item =>
                {
                    if ((string)item.Tag == renderer)
                    {
                        item.Background = black;
                        item.Foreground = white;
                    }
                });
            });

            Type_1_UpdateVirtualRegistry<string>("Rendering", renderers.JoinBy(","));
            Type_2_UpdatePerTabSettings<string>("Rendering", true, renderers.JoinBy(","));
            Type_3_UpdateInFocusTabSettings<string>("Rendering", true, renderers.JoinBy(","));

            Type_2_UpdatePerTabSettings<long>("SelectedRenderer", true, Type_1_GetVirtualRegistry<long>("SelectedRenderer"));

            AssertSelectedOutputTab();
        }

        private void InterpretMenu_Timeout_Click(object sender, RoutedEventArgs e)
        {
            string il = Type_1_GetVirtualRegistry<string>("InterfaceLanguageName");
            Dictionary<string, string> global = LanguageSettings[il]["GLOBAL"];
            Dictionary<string, string> frmMain = LanguageSettings[il]["frmMain"];
            CultureInfo cultureInfo = new(global["Locale"]);

            Telemetry.SetEnabled(false);

            foreach (MenuFlyoutItemBase? item in from key in new string[] { "mnu20Seconds", "mnu100Seconds", "mnu200Seconds", "mnu1000Seconds", "mnuInfinite" }
                                                 let items = from item in mnuTimeout.Items where item.Name == key select item
                                                 from item in items
                                                 select item)
            {
                MenuItemHighlightController((MenuFlyoutItem)item!, false);
            }

            MenuFlyoutItem me = (MenuFlyoutItem)sender;
            long timeout = 0;
            Telemetry.Transmit(me.Name, me.Tag);
            switch (me.Name)
            {
                case "mnu20Seconds":
                    MenuItemHighlightController(mnu20Seconds, true);
                    timeout = 0;
                    break;

                case "mnu100Seconds":
                    MenuItemHighlightController(mnu100Seconds, true);
                    timeout = 1;
                    break;

                case "mnu200Seconds":
                    MenuItemHighlightController(mnu200Seconds, true);
                    timeout = 2;
                    break;

                case "mnu1000Seconds":
                    MenuItemHighlightController(mnu1000Seconds, true);
                    timeout = 3;
                    break;

                case "mnuInfinite":
                    MenuItemHighlightController(mnuInfinite, true);
                    timeout = 4;
                    break;
            }
            Type_1_UpdateVirtualRegistry<long>("Timeout", timeout);
            Type_2_UpdatePerTabSettings<long>("Timeout", true, timeout);
            _ = Type_3_UpdateInFocusTabSettingsIfPermittedAsync<long>("Timeout", true, timeout, $"{frmMain["mnuUpdate"]} {frmMain["mnuTimeout"].ToLower(cultureInfo)} = '{frmMain[me.Name].ToLower(cultureInfo)}'");
        }

        private void MnuIDEConfiguration_Click(object sender, RoutedEventArgs e)
        {
            string? interp = LocalSettings.Values["Engine.3"].ToString();

            Frame.Navigate(typeof(IDEConfigPage), new NavigationData()
            {
                Source = "MainPage",
                KVPs = new()
                {
                    { "Interpreter", interp!},
                    { "Scripts",  Scripts!},
                    { "Language", LanguageSettings[Type_1_GetVirtualRegistry<string>("InterfaceLanguageName")] }
                }
            });
        }

        private async void MnuLanguage_Click(object sender, RoutedEventArgs e)
        {
            string il = Type_1_GetVirtualRegistry<string>("InterfaceLanguageName");
            Dictionary<string, string> global = LanguageSettings[il]["GLOBAL"];
            Dictionary<string, string> frmMain = LanguageSettings[il]["frmMain"];
            CultureInfo cultureInfo = new(global["Locale"]);

            MenuFlyoutItem me = (MenuFlyoutItem)sender;
            string lang = me.Name;

            // iterate the list, and turn off the highlight then assert highlight on chosen

            CustomTabItem navigationViewItem = (CustomTabItem)tabControl.SelectedItem;
            //CustomRichEditBox currentRichEditBox = _richEditBoxes[navigationViewItem.Tag];

            //var text = me.Text;

            string id = LanguageSettings[lang]["GLOBAL"]["ID"];

            Type_1_UpdateVirtualRegistry("InterpreterLanguageName", lang);
            Type_1_UpdateVirtualRegistry("InterpreterLanguageID", long.Parse(id));
            //Type_1_UpdateVirtualRegistry("VariableLength", VariableLength);

            Type_2_UpdatePerTabSettings("Language", true, long.Parse(id));

            string message = $"{frmMain["mnuUpdate"]} {frmMain["mnuLanguage"].ToLower(cultureInfo)} = '{LanguageSettings[lang]["GLOBAL"]["153"][..1].ToUpper(cultureInfo)}{LanguageSettings[lang]["GLOBAL"]["153"][1..].ToLower(cultureInfo)}'";
            await Type_3_UpdateInFocusTabSettingsIfPermittedAsync("Language", true, long.Parse(id), message);
            //Type_3_UpdateInFocusTabSettings("Language", true, long.Parse(id));

            ChangeHighlightOfMenuBarForLanguage(mnuRun, Type_1_GetVirtualRegistry<string>("InterpreterLanguageName"));
            UpdateLanguageNameInStatusBar(navigationViewItem.TabSettingsDict);
            UpdateCommandLineInStatusBar();
        }
        private async void Open()
        {
            FileOpenPicker open = new()
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            open.FileTypeFilter.Add(".pr");
            open.FileTypeFilter.Add(".p");

            // For Uno.WinUI-based apps
            nint hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App._window);
            WinRT.Interop.InitializeWithWindow.Initialize(open, hwnd);

            StorageFile pickedFile = await open.PickSingleFileAsync();
            if (pickedFile != null)
            {
                CreateNewRichEditBox();
                CustomTabItem navigationViewItem = (CustomTabItem)tabControl.MenuItems[tabControl.MenuItems.Count - 1];
                navigationViewItem.IsNewFile = false;
                navigationViewItem.SavedFilePath = pickedFile;
                navigationViewItem.Content = pickedFile.Name;
                navigationViewItem.Height = 30; // FIXME is this where to do it?
                navigationViewItem.TabSettingsDict = ClonePerTabSettings(PerTabInterpreterParameters);
                // navigationViewItem.MaxHeight = 60; // FIXME is this where to do it?
                // navigationViewItem.VerticalAlignment = VerticalAlignment.Bottom;
                CustomRichEditBox newestRichEditBox = _richEditBoxes[navigationViewItem.Tag];
                using (Windows.Storage.Streams.IRandomAccessStream randAccStream =
                    await pickedFile.OpenAsync(FileAccessMode.Read))
                {
                    // var encoding = EncChecker.EncCheck.DetectFileAsEncoding(pickedFile.Path);
                    bool hasBOM = false;
                    Encoding? encoding = TextEncoding.GetFileEncoding(pickedFile.Path, 1000, ref hasBOM);
                    switch (pickedFile.FileType.ToLower())
                    { // Load the file into the Document property of the RichEditBox.
                        case ".pr":
                            {
                                newestRichEditBox.Document.LoadFromStream(TextSetOptions.FormatRtf, randAccStream);
                                newestRichEditBox.IsRTF = true;
                                newestRichEditBox.IsDirty = false;
                                break;
                            }
                        case ".p":
                            {
                                string text = File.ReadAllText(pickedFile.Path, encoding!);
                                newestRichEditBox.Document.SetText(TextSetOptions.UnicodeBidi, text);
                                newestRichEditBox.IsRTF = false;
                                newestRichEditBox.IsDirty = false;
                                break;
                            }
                        default:
                            {
                                string text = File.ReadAllText(pickedFile.Path, encoding!);
                                newestRichEditBox.Document.SetText(TextSetOptions.UnicodeBidi, text);
                                newestRichEditBox.IsRTF = false;
                                newestRichEditBox.IsDirty = false;
                                break;
                            }
                    }
                    Type_1_UpdateVirtualRegistry("MostRecentPickedFilePath", Path.GetDirectoryName(pickedFile.Path));
                }
                if (newestRichEditBox.IsRTF)
                {
                    HandleCustomPropertyLoading(pickedFile, newestRichEditBox);
                }

                UpdateLanguageNameInStatusBar(navigationViewItem.TabSettingsDict);
                UpdateStatusBarFromInFocusTab();
                UpdateCommandLineInStatusBar();
                UpdateInterpreterInStatusBar();
                UpdateTopMostRendererInCurrentTab();

                AssertSelectedOutputTab();
                //bool flag = InFocusTabIsPrFile(); // FIXME What's this for??
            }
        }
        private async void Paste()
        {
            DataPackageView dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                string textToPaste = await dataPackageView.GetTextAsync();

                if (!string.IsNullOrEmpty(textToPaste))
                {
                    CustomTabItem navigationViewItem = (CustomTabItem)tabControl.SelectedItem;
                    CustomRichEditBox currentRichEditBox = _richEditBoxes[navigationViewItem.Tag];
                    currentRichEditBox.Document.Selection.Paste(0);
                }
            }
        }

        //private void InsertVariableLengthCodeTemplate_Click(object sender, RoutedEventArgs e)
        //{
        //    CustomTabItem navigationViewItem = (CustomTabItem)tabControl.SelectedItem;
        //    CustomRichEditBox currentRichEditBox = _richEditBoxes[navigationViewItem.Tag];
        //    ITextSelection selection = currentRichEditBox.Document.Selection;
        //    if (selection != null)
        //    {
        //        selection.StartPosition = selection.EndPosition;
        //        selection.Text = ;
        //        selection.EndPosition = selection.StartPosition;
        //        currentRichEditBox.Document.Selection.Move(TextRangeUnit.Character, 3);
        //    }
        //}
        private async void ResetToFactorySettings_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = this.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "Factory Reset",
                Content = "Confirm reset. Application will shut down after reset.",
                PrimaryButtonText = "OK",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Secondary,
            };
            ContentDialogResult result = await dialog.ShowAsync();

            if (result is ContentDialogResult.Secondary)
            {
                return;
            }

            Dictionary<string, object> dict = [];
            Dictionary<string, object>? fac = await GetFactorySettings();
            File.WriteAllText(Path.Combine(Path.GetTempPath(), "PelotonIDE_FactorySettings_log.json"), JsonConvert.SerializeObject(fac));

            foreach (KeyValuePair<string, object> key in ApplicationData.Current.LocalSettings.Values)
            {
                dict.Add(key.Key, key.Value);
            }
            File.WriteAllText(Path.Combine(Path.GetTempPath(), "PelotonIDE_LocalSettings_log.json"), JsonConvert.SerializeObject(dict));

            foreach (KeyValuePair<string, object> setting in ApplicationData.Current.LocalSettings.Values)
            {
                ApplicationData.Current.LocalSettings.DeleteContainer(setting.Key);
            }
            try
            {
                await ApplicationData.Current.ClearAsync();
            }
            catch (Exception er)
            {
                Telemetry.SetEnabled(false);
                Telemetry.Transmit(er.Message, er.StackTrace);
            }
            Environment.Exit(0);
        }

        private async void Save()
        {
            CustomTabItem navigationViewItem = (CustomTabItem)tabControl.SelectedItem;

            if (navigationViewItem != null)
            {
                if (navigationViewItem.IsNewFile)
                {
                    FileSavePicker savePicker = new()
                    {
                        SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                    };

                    // Dropdown of file types the user can save the file as
                    savePicker.FileTypeChoices.Add("Rich Text", new List<string>() { ".pr" });
                    savePicker.FileTypeChoices.Add("UTF-8", new List<string>() { ".p" });

                    string? tabTitle = navigationViewItem.Content.ToString();
                    savePicker.SuggestedFileName = tabTitle ?? "New Document";

                    // For Uno.WinUI-based apps
                    nint hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App._window);
                    WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

                    StorageFile file = await savePicker.PickSaveFileAsync();
                    if (file != null)
                    {
                        CustomRichEditBox currentRichEditBox = _richEditBoxes[navigationViewItem.Tag];
                        // Prevent updates to the remote version of the file until we
                        // finish making changes and call CompleteUpdatesAsync.
                        CachedFileManager.DeferUpdates(file);
                        // write to file
                        using (Windows.Storage.Streams.IRandomAccessStream randAccStream =
                        await file.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            randAccStream.Size = 0;
                            if (file.FileType == ".pr")
                            {
                                currentRichEditBox.Document.SaveToStream(TextGetOptions.FormatRtf | TextGetOptions.AdjustCrlf, randAccStream);
                                currentRichEditBox.IsRTF = true;
                                currentRichEditBox.IsDirty = false;
                            }
                            else if (file.FileType == ".p")
                            {
                                currentRichEditBox.Document.GetText(TextGetOptions.None, out string plainText);
                                using (DataWriter dataWriter = new(randAccStream))
                                {
                                    dataWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf16LE;
                                    dataWriter.WriteString(plainText);
                                    await dataWriter.StoreAsync();
                                    await randAccStream.FlushAsync();
                                }
                                currentRichEditBox.IsRTF = false;
                                currentRichEditBox.IsDirty = false;
                            }
                            else
                            {
                                currentRichEditBox.Document.GetText(TextGetOptions.None, out string plainText);
                                using (DataWriter dataWriter = new(randAccStream))
                                {
                                    dataWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf16LE;
                                    dataWriter.WriteString(plainText);
                                    await dataWriter.StoreAsync();
                                    await randAccStream.FlushAsync();
                                }
                                currentRichEditBox.IsRTF = false;
                                currentRichEditBox.IsDirty = false;
                            }
                        }

                        // Let Windows know that we're finished changing the file so the
                        // other app can update the remote version of the file.
                        FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                        if (status != FileUpdateStatus.Complete)
                        {
                            Windows.UI.Popups.MessageDialog errorBox =
                                new($"File {file.Name} couldn't be saved.");
                            await errorBox.ShowAsync();
                        }

                        CustomTabItem savedItem = (CustomTabItem)tabControl.SelectedItem;
                        savedItem.IsNewFile = false;
                        savedItem.Content = file.Name;
                        savedItem.SavedFilePath = file;
                        if (currentRichEditBox.IsRTF)
                        {
                            HandleCustomPropertySaving(file, navigationViewItem);
                        }
                    }
                }
                else
                {
                    if (navigationViewItem.SavedFilePath != null)
                    {
                        CustomRichEditBox currentRichEditBox = _richEditBoxes[navigationViewItem.Tag];
                        StorageFile file = navigationViewItem.SavedFilePath;
                        CachedFileManager.DeferUpdates(file);
                        // write to file
                        using (Windows.Storage.Streams.IRandomAccessStream randAccStream =
                            await file.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            randAccStream.Size = 0;

                            if (file.FileType == ".pr")
                            {
                                currentRichEditBox.Document.SaveToStream(Microsoft.UI.Text.TextGetOptions.FormatRtf, randAccStream);
                                currentRichEditBox.IsRTF = true;
                                currentRichEditBox.IsDirty = false;
                            }
                            else if (file.FileType == ".p")
                            {
                                currentRichEditBox.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out string plainText);
                                using (DataWriter dataWriter = new(randAccStream))
                                {
                                    dataWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf16LE;
                                    dataWriter.WriteString(plainText);
                                    await dataWriter.StoreAsync();
                                    await randAccStream.FlushAsync();
                                }
                                currentRichEditBox.IsRTF = false;
                                currentRichEditBox.IsDirty = false;
                            }
                            else
                            {
                                currentRichEditBox.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out string plainText);
                                using (DataWriter dataWriter = new(randAccStream))
                                {
                                    dataWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf16LE;
                                    dataWriter.WriteString(plainText);
                                    await dataWriter.StoreAsync();
                                    await randAccStream.FlushAsync();
                                }
                                currentRichEditBox.IsRTF = false;
                                currentRichEditBox.IsDirty = false;
                            }
                        }

                        // Let Windows know that we're finished changing the file so the
                        // other app can update the remote version of the file.
                        FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                        if (status != FileUpdateStatus.Complete)
                        {
                            Windows.UI.Popups.MessageDialog errorBox =
                                new("File " + file.Name + " couldn't be saved.");
                            await errorBox.ShowAsync();
                        }
                        CustomTabItem savedItem = (CustomTabItem)tabControl.SelectedItem;
                        savedItem.IsNewFile = false;
                        savedItem.Content = file.Name;

                        if (currentRichEditBox.IsRTF)
                        {
                            HandleCustomPropertySaving(file, navigationViewItem);
                        }
                        currentRichEditBox.IsDirty = false;
                    }
                }
            }
        }
        private async void SaveAs()
        {
            CustomTabItem navigationViewItem = (CustomTabItem)tabControl.SelectedItem;

            if (navigationViewItem != null)
            {
                FileSavePicker savePicker = new()
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };

                // Dropdown of file types the user can save the file as
                if ((navigationViewItem.Content as string).EndsWith(".p"))
                {
                    savePicker.FileTypeChoices.Add("Unicode Text", new List<string>() { ".p" }); // "UTF-8"
                    savePicker.FileTypeChoices.Add("Rich Text", new List<string>() { ".pr" });
                }
                else
                {
                    savePicker.FileTypeChoices.Add("Rich Text", new List<string>() { ".pr" });
                    savePicker.FileTypeChoices.Add("Unicode Text", new List<string>() { ".p" });
                }

                string? tabTitle = navigationViewItem.Content.ToString();
                savePicker.SuggestedFileName = tabTitle ?? "New Document";

                // For Uno.WinUI-based apps
                nint hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App._window);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    CustomRichEditBox currentRichEditBox = _richEditBoxes[navigationViewItem.Tag];
                    // Prevent updates to the remote version of the file until we
                    // finish making changes and call CompleteUpdatesAsync.
                    CachedFileManager.DeferUpdates(file);
                    // write to file
                    using (Windows.Storage.Streams.IRandomAccessStream randAccStream =
                        await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
                    {
                        randAccStream.Size = 0;

                        if (file.FileType == ".pr")
                        {
                            currentRichEditBox.Document.SaveToStream(Microsoft.UI.Text.TextGetOptions.FormatRtf, randAccStream);
                            currentRichEditBox.IsRTF = true;
                        }
                        else if (file.FileType == ".p")
                        {
                            currentRichEditBox.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out string plainText);
                            using (DataWriter dataWriter = new(randAccStream))
                            {
                                dataWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf16LE;
                                dataWriter.WriteString(plainText);
                                await dataWriter.StoreAsync();
                                await randAccStream.FlushAsync();
                            }
                            currentRichEditBox.IsRTF = false;
                        }
                        else
                        {
                            currentRichEditBox.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out string plainText);
                            using (DataWriter dataWriter = new(randAccStream))
                            {
                                dataWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf16LE;
                                dataWriter.WriteString(plainText);
                                await dataWriter.StoreAsync();
                                await randAccStream.FlushAsync();
                            }
                            currentRichEditBox.IsRTF = false;

                        }
                    }

                    // Let Windows know that we're finished changing the file so the
                    // other app can update the remote version of the file.
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status != FileUpdateStatus.Complete)
                    {
                        Windows.UI.Popups.MessageDialog errorBox =
                            new($"File {file.Name} couldn't be saved.");
                        await errorBox.ShowAsync();
                    }
                    CustomTabItem savedItem = (CustomTabItem)tabControl.SelectedItem;
                    savedItem.IsNewFile = false;
                    savedItem.Content = file.Name;
                    savedItem.SavedFilePath = file;

                    if (currentRichEditBox.IsRTF)
                    {
                        HandleCustomPropertySaving(file, navigationViewItem);
                    }
                }
            }
        }
        private void SelectAll()
        {
            CustomTabItem navigationViewItem = (CustomTabItem)tabControl.SelectedItem;
            CustomRichEditBox currentRichEditBox = _richEditBoxes[navigationViewItem.Tag];
            currentRichEditBox.Focus(FocusState.Pointer);
            currentRichEditBox.Document.GetText(TextGetOptions.None, out string? allText);
            int endPosition = allText.Length - 1;
            currentRichEditBox.Document.Selection.SetRange(0, endPosition);
        }
        private async void ShowMemory_Click(object sender, RoutedEventArgs e)
        {
            Telemetry.SetEnabled(false);

            CustomTabItem navigationViewItem = (CustomTabItem)tabControl.SelectedItem;
            Dictionary<string, Dictionary<string, object>>? currentTabSettings = navigationViewItem.TabSettingsDict;
            List<string> lines = ["Current Tab"];
            foreach (string key in currentTabSettings.Keys)
            {
                bool isInternal = (bool)currentTabSettings[key]["Internal"];
                if ((bool)currentTabSettings[key]["Defined"])
                {
                    if (isInternal)
                    {
                        lines.Add($"\t{key} -> {currentTabSettings[key]["Value"]}");
                    }
                    else
                    {
                        lines.Add($"\t{key} -> /{currentTabSettings[key]["Key"]}:{currentTabSettings[key]["Value"]}");
                    }
                }
            }
            lines.Add("");
            lines.Add("PerTab Settings");
            foreach (string key in PerTabInterpreterParameters.Keys)
            {
                bool isInternal = (bool)PerTabInterpreterParameters[key]["Internal"];
                if ((bool)PerTabInterpreterParameters[key]["Defined"])
                {
                    if (isInternal)
                    {
                        lines.Add($"\t{key} -> {PerTabInterpreterParameters[key]["Value"]}");
                    }
                    else
                    {
                        lines.Add($"\t{key} -> /{PerTabInterpreterParameters[key]["Key"]}:{PerTabInterpreterParameters[key]["Value"]}");
                    }
                }
            }

            lines.Add("");
            lines.Add("Virtual Registry");
            foreach (KeyValuePair<string, object> val in ApplicationData.Current.LocalSettings.Values.OrderBy(pair => pair.Key))
            {
                lines.Add($"\t{val.Key} -> {val.Value}");
            }
            Telemetry.Transmit(lines.JoinBy("\r\n"));
            ContentDialog dialog = new()
            {
                XamlRoot = this.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "Show Memory",
                Content = lines.JoinBy("\n"),
                PrimaryButtonText = "OK",
                DefaultButton = ContentDialogButton.Primary,
                CanBeScrollAnchor = true,
            };
            _ = await dialog.ShowAsync();
        }

        private void SwapCodeTemplatesLabels(bool isVariable)
        {
            (MakePelotonVariableLength.Text, MakePeloton.Text) = (MakePeloton.Text, MakePelotonVariableLength.Text);
        }

        private void VariableLength_Click(object sender, RoutedEventArgs e)
        {
            string il = Type_1_GetVirtualRegistry<string>("InterfaceLanguageName");
            Dictionary<string, string> global = LanguageSettings[il]["GLOBAL"];
            Dictionary<string, string> frmMain = LanguageSettings[il]["frmMain"];
            CultureInfo cultureInfo = new(global["Locale"]);

            bool varlen = Type_1_GetVirtualRegistry<bool>("VariableLength");
            bool VariableLength;
            if (!varlen)
            {
                MenuItemHighlightController(mnuVariableLength, true);
                VariableLength = true;
            }
            else
            {
                MenuItemHighlightController(mnuVariableLength, false);
                VariableLength = false;
            }
            Type_1_UpdateVirtualRegistry("VariableLength", VariableLength);
            Type_2_UpdatePerTabSettings("VariableLength", VariableLength, VariableLength);
            string message = VariableLength ? global["fixedLength"].ToLower(cultureInfo) : global["variableLength"].ToLower(cultureInfo);
            _ = Type_3_UpdateInFocusTabSettingsIfPermittedAsync<bool>("VariableLength", VariableLength, VariableLength, $"{frmMain["mnuUpdate"]} = {message}?"); // mnuUpdate

            SwapCodeTemplatesLabels(VariableLength);

            UpdateCommandLineInStatusBar();
            UpdateStatusBarFromInFocusTab();
        }
    }
}