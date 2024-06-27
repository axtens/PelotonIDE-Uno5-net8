﻿using DocumentFormat.OpenXml.Office2019.Presentation;

using Microsoft.UI.Xaml.Input;
using Microsoft.VisualBasic;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;

using TabSettingJson = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, object>>;

namespace PelotonIDE.Presentation
{
    public partial class CustomTabItem : NavigationViewItem
    {
        public bool IsNewFile { get; set; }

        public StorageFile? SavedFilePath { get; set; }

        public TabSettingJson? TabSettingsDict { get; set; }

        public CustomTabItem()
        {
            SavedFilePath = null;
        }

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            Telemetry.SetEnabled(false);
            Telemetry.Transmit("CustomTabItem", e.DeviceId, e.Handled, e.Key, e.KeyStatus, e.OriginalKey, e.OriginalSource);
            //if (e.Key == (VirtualKey.Tab|VirtualKey.Control))
            //{
            //    Debug.WriteLine(e.KeyStatus);
            //}
            base.OnKeyDown(e);
        }
    }
}
