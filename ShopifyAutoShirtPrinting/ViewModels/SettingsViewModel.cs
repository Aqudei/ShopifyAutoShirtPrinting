using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using Prism.Commands;
using Prism.Mvvm;
using ShopifyEasyShirtPrinting.Models;
using ShopifyEasyShirtPrinting.Properties;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        public ObservableCollection<SettingItem> AppSettings { get; set; } = new();

        public string Title => "Settings";
        public SettingsViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;
            foreach (DictionaryEntry environmentVariable in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User))
            {
                if (environmentVariable.Key.ToString().StartsWith("SHOPIFY_"))
                {
                    AppSettings.Add(new SettingItem
                    {
                        Name = environmentVariable.Key.ToString(),
                        Value = environmentVariable.Value.ToString(),
                        SettingType = SettingItem.SettingItemType.Environment
                    });
                }
            }

            AppSettings.Add(new SettingItem
            {
                SettingType = SettingItem.SettingItemType.App,
                Name = nameof(Properties.Settings.Default.HotFoldersConfig),
                Value = Properties.Settings.Default.HotFoldersConfig
            });


            //AppSettings.Add(new SettingItem { Name = "SHOPIFY_TOKEN", Value = Environment.GetEnvironmentVariable("SHOPIFY_TOKEN") });
            //AppSettings.Add(new SettingItem { Name = "SHOPIFY_API_KEY", Value = Environment.GetEnvironmentVariable("SHOPIFY_API_KEY") });
            //AppSettings.Add(new SettingItem { Name = "SHOPIFY_API_SECRET", Value = Environment.GetEnvironmentVariable("SHOPIFY_API_SECRET") });
            //AppSettings.Add(new SettingItem { Name = "SHOPIFY_SHOP_URL", Value = Environment.GetEnvironmentVariable("SHOPIFY_SHOP_URL") });
        }


        private DelegateCommand _saveCommand;

        public DelegateCommand SaveCommand => _saveCommand ??= new DelegateCommand(() => Task.Run(SaveSettings));

        private async void SaveSettings()
        {
            foreach (var settingItem in AppSettings.Where(item => item.SettingType == SettingItem.SettingItemType.Environment))
            {
                if (string.IsNullOrWhiteSpace(settingItem.Name))
                    continue;

                Environment.SetEnvironmentVariable(settingItem.Name, settingItem.Value, EnvironmentVariableTarget.User);
            }

            foreach (var settingItem in AppSettings.Where(item => item.SettingType == SettingItem.SettingItemType.App))
            {
                Properties.Settings.Default[settingItem.Name] = settingItem.Value;
            }

            Properties.Settings.Default.Save();


            await _dialogCoordinator.ShowMessageAsync(this, "Success", "Settings successfully saved!");
        }

        private DelegateCommand<SettingItem> _deleteCommand;

        public DelegateCommand<SettingItem> DeleteCommand
        {
            get { return _deleteCommand ??= new DelegateCommand<SettingItem>(DeleteSetting); }
        }

        private void DeleteSetting(SettingItem settingItem)
        {
            if (settingItem.SettingType == SettingItem.SettingItemType.Environment)
            {
                if (Environment.GetEnvironmentVariable(settingItem.Name, EnvironmentVariableTarget.User) == null)
                    return;

                Environment.SetEnvironmentVariable(settingItem.Name, null, EnvironmentVariableTarget.User);
                AppSettings.Remove(settingItem);
            }
            else
            {
                Properties.Settings.Default[settingItem.Name] = "";
                Properties.Settings.Default.Save();
                settingItem.Value = "";
            }
        }
    }
}
