using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FactorySettingsStructure = System.Collections.Generic.Dictionary<string, object>;

using Windows.Storage;

namespace PelotonIDE.Presentation
{
    public sealed partial class MainPage : Page
    {
        private static async Task<FactorySettingsStructure?> GetFactorySettings()
        {
            StorageFile globalSettings = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///PelotonIDE\\Presentation\\FactorySettings.json"));
            string globalSettingsString = File.ReadAllText(globalSettings.Path);
            return JsonConvert.DeserializeObject<FactorySettingsStructure>(globalSettingsString);
        }

        private OutputPanelPosition GetFactorySettingsWithLocalSettingsOverrideOrDefault(string name, OutputPanelPosition otherwise, FactorySettingsStructure? factory, ApplicationDataContainer? container)
        {
            OutputPanelPosition result = OutputPanelPosition.Bottom;
            bool noFactory = false;
            bool noContainer = false;
            if (factory.TryGetValue(name, out object? value1))
            {
                result = (OutputPanelPosition)Enum.Parse(typeof(OutputPanelPosition), (string)value1);
            }
            else
            {
                noFactory = true;
            }
            if (container.Values.TryGetValue(name, out object? value2))
            {
                result = (OutputPanelPosition)Enum.Parse(typeof(OutputPanelPosition), (string)value2);
            }
            else
            {
                noContainer = true;
            }
            if (noFactory && noContainer)
            {
                result = otherwise;
            }
            container.Values[name] = result.ToString();
            return result;
        }

        //private T? GetFactorySettingsWithLocalSettingsOverrideOrDefault<T>(string name, FactorySettingsStructure? factory, ApplicationDataContainer? container, T otherwise)
        //{
        //    T? result = default;
        //    bool noFactory = false;
        //    bool noContainer = false;
        //    if (factory.TryGetValue(name, out object? value1))
        //    {
        //        result = (T)value1;
        //    }
        //    else
        //    {
        //        noFactory = true;
        //    }
        //    if (container.Values.TryGetValue(name, out object? value2))
        //    {
        //        result = (T)value2;
        //    }
        //    else
        //    {
        //        noContainer = true;
        //    }
        //    if (noFactory && noContainer)
        //    {
        //        result = otherwise;
        //    }
        //    container.Values[name] = result;
        //    return result;
        //}

        //private int GetFactorySettingsWithLocalSettingsOverrideOrDefault(string name, int otherwise, FactorySettingsStructure? factory, ApplicationDataContainer? container)
        //{
        //    int result = default;
        //    bool noFactory = false;
        //    bool noContainer = false;
        //    if (factory.TryGetValue(name, out object? value1))
        //    {
        //        result = (int)value1;
        //    }
        //    else
        //    {
        //        noFactory = true;
        //    }
        //    if (container.Values.TryGetValue(name, out object? value2))
        //    {
        //        result = (int)value2;
        //    }
        //    else
        //    {
        //        noContainer = true;
        //    }
        //    if (noFactory && noContainer)
        //    {
        //        result = otherwise;
        //    }
        //    container.Values[name] = result;
        //    return result;
        //}
        //private long GetFactorySettingsWithLocalSettingsOverrideOrDefault(string name, long otherwise, FactorySettingsStructure? factory, ApplicationDataContainer? container)
        //{
        //    long result = default;
        //    bool noFactory = false;
        //    bool noContainer = false;
        //    if (factory.TryGetValue(name, out object? value1))
        //    {
        //        result = (long)value1;
        //    }
        //    else
        //    {
        //        noFactory = true;
        //    }
        //    if (container.Values.TryGetValue(name, out object? value2))
        //    {
        //        result = (long)value2;
        //    }
        //    else
        //    {
        //        noContainer = true;
        //    }
        //    if (noFactory && noContainer)
        //    {
        //        result = otherwise;
        //    }
        //    container.Values[name] = result;
        //    return result;
        //}
        //private bool GetFactorySettingsWithLocalSettingsOverrideOrDefault(string name, bool otherwise, FactorySettingsStructure? factory, ApplicationDataContainer? container)
        //{
        //    bool result = default;
        //    bool noFactory = false;
        //    bool noContainer = false;
        //    if (factory.TryGetValue(name, out object? value1))
        //    {
        //        result = (bool)value1;
        //    }
        //    else
        //    {
        //        noFactory = true;
        //    }
        //    if (container.Values.TryGetValue(name, out object? value2))
        //    {
        //        result = (bool)value2;
        //    }
        //    else
        //    {
        //        noContainer = true;
        //    }
        //    if (noFactory && noContainer)
        //    {
        //        result = otherwise;
        //    }
        //    container.Values[name] = result;
        //    return result;
        //}


        //private string? GetFactorySettingsWithLocalSettingsOverrideOrDefault(string name, string otherwise, FactorySettingsStructure? factory, ApplicationDataContainer? container)
        //{
        //    string? result = default;
        //    bool noFactory = false;
        //    bool noContainer = false;
        //    if (factory.TryGetValue(name, out object? value1))
        //    {
        //        result = (string)value1;
        //    }
        //    else
        //    {
        //        noFactory = true;
        //    }
        //    if (container.Values.TryGetValue(name, out object? value2))
        //    {
        //        result = (string)value2;
        //    }
        //    else
        //    {
        //        noContainer = true;
        //    }
        //    if (noFactory && noContainer)
        //    {
        //        result = otherwise;
        //    }
        //    container.Values[name] = result;
        //    return result;
        //}
    }
}
