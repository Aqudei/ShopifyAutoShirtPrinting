using AutoMapper;
using Common.Api;
using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintAgent.ViewModels
{
    public class SettingsViewModel : PageBase
    {
        private string _apiBaseUrl;
        private DelegateCommand saveCommand;
        private IMapper _mapper;
        private IDialogCoordinator _dialogCoordinator;

        public override string Title => "Settings";
        public string ApiBaseUrl { get => _apiBaseUrl; set => SetProperty(ref _apiBaseUrl, value); }
        public string HotFolderRoot { get => _hotFolderRoot; set => SetProperty(ref _hotFolderRoot, value); }
        public string RulesFile { get => _rulesFile; set => SetProperty(ref _rulesFile, value); }
        private string _printFiles;

        public string PrintFilesFolder
        {
            get { return _printFiles; }
            set { SetProperty(ref _printFiles, value); }
        }

        public SettingsViewModel(IMapper mapper, IDialogCoordinator dialogCoordinator)
        {
            _mapper = mapper;
            _dialogCoordinator = dialogCoordinator;
            mapper.Map(PrintAgent.Properties.Settings.Default, this);
        }

        public DelegateCommand SaveCommand { get => saveCommand ??= new DelegateCommand(HandleSave); }

        private DelegateCommand<string> _browseFileCommand;
        private string _hotFolderRoot;
        private DelegateCommand<string> _browseFolderCommand;
        private string _rulesFile;

        public DelegateCommand<string> BrowseFileCommand
        {
            get
            {

                return _browseFileCommand ??= new DelegateCommand<string>(HandleBrowseFile);
            }
        }


        public DelegateCommand<string> BrowseFolderCommand { get => _browseFolderCommand ??= new DelegateCommand<string>(HandleBrowseFolder); }

        private void HandleBrowseFolder(string obj)
        {
            var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog
            {
                IsFolderPicker = true,
            };
            var dlgResult = dialog.ShowDialog();

            if (dlgResult != Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
            {
                return;
            }

            if (obj.ToLower() == "hotfolder")
            {
                HotFolderRoot = dialog.FileName;
            }

            if (obj.ToLower() == "printfiles")
            {
                PrintFilesFolder = dialog.FileName;
            }
        }

        private void HandleBrowseFile(string obj)
        {
            var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog
            {

            };
            var dlgResult = dialog.ShowDialog();

            if (dlgResult != Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
            {
                return;
            }

            if (obj == "rules")
            {
                RulesFile = dialog.FileName;
            }
        }

        private async void HandleSave()
        {
            _mapper.Map(this, PrintAgent.Properties.Settings.Default);
            PrintAgent.Properties.Settings.Default.Save();
            await _dialogCoordinator.ShowMessageAsync(this, "Settings", "Settings saved!");
        }
    }


}
