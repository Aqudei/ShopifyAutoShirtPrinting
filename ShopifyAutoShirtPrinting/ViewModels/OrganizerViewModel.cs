using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
using ShopifyEasyShirtPrinting.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace ShopifyEasyShirtPrinting.ViewModels;

public class OrganizerViewModel : BindableBase
{
    private string _destinationPath;

    public float Dpi
    {
        get => _dpi;
        set => SetProperty(ref _dpi, value);
    }

    public string DestinationPath
    {
        get => _destinationPath;
        set => SetProperty(ref _destinationPath, value);
    }

    public ObservableCollection<OrgFile> Files { get; set; } = new();

    private double _canvasWidth;

    public double CanvasWidth
    {
        get => _canvasWidth;
        set => SetProperty(ref _canvasWidth, value);
    }

    private double _canvasHeight;

    public double CanvasHeight
    {
        get => _canvasHeight;
        set => SetProperty(ref _canvasHeight, value);
    }

    private DelegateCommand _startProcessCommand;
    private float _dpi = 600;

    public DelegateCommand StartProcessingCommand
    {
        get { return _startProcessCommand ??= new DelegateCommand(StartProcessing); }
    }

    private void StartProcessing()
    {
        if (Files.Any())
            foreach (var orgFile in Files)
            {
                var fn = Path.GetFileNameWithoutExtension(orgFile.Filename);
                var sku = Sku.Parse(fn);
                if (sku == null) continue;

                var destination = Path.Combine(DestinationPath, sku.ProductType, sku.ProductName,
                    sku.ProductColour, sku.ProductFit, sku.ProductSize);
                orgFile.Destination = destination;
            }
    }

    public string Title => "Organizer";

    public OrganizerViewModel()
    {
        PropertyChanged += OrganizerViewModel_PropertyChanged;
        DestinationPath = Properties.Settings.Default.DestinationPath;
    }

    private void OrganizerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (nameof(DestinationPath) == e.PropertyName && !string.IsNullOrWhiteSpace(DestinationPath))
        {
            Properties.Settings.Default.DestinationPath = DestinationPath;
            Properties.Settings.Default.Save();
        }
    }

    private DelegateCommand<string> _selectFolderCommand;

    public DelegateCommand<string> SelectFolderCommand
    {
        get { return _selectFolderCommand ??= new DelegateCommand<string>(SelectFolder); }
    }

    private void SelectFolder(string forWhat)
    {
        var dlg = new CommonOpenFileDialog
        {
            IsFolderPicker = true
        };

        var dlgResult = dlg.ShowDialog();
        if (dlgResult != CommonFileDialogResult.Ok) return;

        DestinationPath = dlg.FileName;
    }

    private DelegateCommand _addFilesCommand;

    public DelegateCommand AddFilesCommand
    {
        get { return _addFilesCommand ??= new DelegateCommand(AddFiles); }
    }

    private void AddFiles()
    {
        var dlg = new CommonOpenFileDialog
        {
            Multiselect = true
        };
        var dlgResult = dlg.ShowDialog();

        if (dlgResult != CommonFileDialogResult.Ok) return;

        var files = dlg.FileNames;
        foreach (var file in files)
            Files.Add(new OrgFile
            {
                Filename = file
            });
    }


    private void LoadFiles()
    {
        var files = Directory.EnumerateFiles(Properties.Settings.Default.DestinationPath, "*.png",
            SearchOption.AllDirectories);
        foreach (var file in files)
            Application.Current.Dispatcher.Invoke(() =>
            {
                Files.Add(new OrgFile
                {
                    Filename = file
                });
            });
    }

    public void HandleFilesDrop(string[] files)
    {
        foreach (var file in files)
        {
            Files.Add(new OrgFile
            {
                Filename = file
            });
        }
    }
}