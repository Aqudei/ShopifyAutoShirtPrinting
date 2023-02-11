using System;
using System.Collections;
using System.Collections.ObjectModel;
using Prism.Commands;
using Prism.Mvvm;
using ShopifyEasyShirtPrinting.Models;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        public ObservableCollection<SettingItem> AppSettings { get; set; } = new();

        public string Title => "Settings";
        public SettingsViewModel()
        {
            foreach (DictionaryEntry environmentVariable in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User))
            {
                if (environmentVariable.Key.ToString().StartsWith("SHOPIFY_"))
                {
                    AppSettings.Add(new SettingItem { Name = environmentVariable.Key.ToString(), Value = environmentVariable.Value.ToString() });
                }
            }

            //AppSettings.Add(new SettingItem { Name = "SHOPIFY_TOKEN", Value = Environment.GetEnvironmentVariable("SHOPIFY_TOKEN") });
            //AppSettings.Add(new SettingItem { Name = "SHOPIFY_API_KEY", Value = Environment.GetEnvironmentVariable("SHOPIFY_API_KEY") });
            //AppSettings.Add(new SettingItem { Name = "SHOPIFY_API_SECRET", Value = Environment.GetEnvironmentVariable("SHOPIFY_API_SECRET") });
            //AppSettings.Add(new SettingItem { Name = "SHOPIFY_SHOP_URL", Value = Environment.GetEnvironmentVariable("SHOPIFY_SHOP_URL") });
        }

        private DelegateCommand _saveCommand;

        public DelegateCommand SaveCommand => _saveCommand ??= new DelegateCommand(SaveSettings);

        private void SaveSettings()
        {
            foreach (var settingItem in AppSettings)
            {
                if (string.IsNullOrWhiteSpace(settingItem.Name))
                    continue;

                Environment.SetEnvironmentVariable(settingItem.Name, settingItem.Value, EnvironmentVariableTarget.User);
            }
        }

        private DelegateCommand<SettingItem> _deleteCommand;

        public DelegateCommand<SettingItem> DeleteCommand
        {
            get { return _deleteCommand ??= new DelegateCommand<SettingItem>(DeleteSetting); }
        }

        private void DeleteSetting(SettingItem settingItem)
        {
            if (Environment.GetEnvironmentVariable(settingItem.Name, EnvironmentVariableTarget.User) == null)
                return;

            Environment.SetEnvironmentVariable(settingItem.Name, null, EnvironmentVariableTarget.User);
            AppSettings.Remove(settingItem);
        }
    }
}
