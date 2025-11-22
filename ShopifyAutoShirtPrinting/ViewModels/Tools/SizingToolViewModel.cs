using ImageMagick;
using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using ShopifyEasyShirtPrinting.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ShopifyEasyShirtPrinting.ViewModels.Tools
{
    internal class SizingToolViewModel : PageBase
    {
        private DelegateCommand importFilesCommand;
        private readonly IDialogCoordinator _dialogCoordinator;

        public override string Title => "Sizing";

        public ObservableCollection<ImageItem> Items { get; set; } = new();
        public string DestinationFolder { get => _destinationFolder; set => SetProperty(ref _destinationFolder, value); }
        public DelegateCommand ImportFilesCommand { get => importFilesCommand ??= new DelegateCommand(HandleImportFile); }
        private DelegateCommand _startProcessingCommand;
        private string _destinationFolder;
        private DelegateCommand<string> _browseFolderCommand;
        private Color? _backgroundColor;
        public DelegateCommand ClearItemsCommand { get => clearItemsCommand ??= new DelegateCommand(HandleClearItems); }

        private void HandleClearItems()
        {
            Items.Clear();
        }

        public DelegateCommand StartProcessingCommand
        {
            get { return _startProcessingCommand ??= new DelegateCommand(HandleStartProcessing); }
        }
        private bool _hasItems;

        public bool HasItems
        {
            get { return _hasItems; }
            set { SetProperty(ref _hasItems, value); }
        }

        public Color? BackgroundColor
        {
            get => _backgroundColor;
            set => SetProperty(ref _backgroundColor, value);
        }
        private uint _canvasWidth;

        private uint _canvasHeight;
        private DelegateCommand clearItemsCommand;

        public uint CanvasWidth
        {
            get { return _canvasWidth; }
            set { SetProperty(ref _canvasWidth, value); }
        }

        public uint CanvasHeight
        {
            get { return _canvasHeight; }
            set { SetProperty(ref _canvasHeight, value); }
        }


        private void ProcessImage(string imagePath)
        {
            using MagickImage image = new MagickImage(imagePath);
            var color = new MagickColor(BackgroundColor.Value.R, BackgroundColor.Value.G, BackgroundColor.Value.B, BackgroundColor.Value.A);


            using var canvas = new MagickImage(color, CanvasWidth, CanvasHeight);



            // Crop whitespace
            image.Trim();
            image.Resize(CanvasWidth, CanvasHeight);
            canvas.Composite(image, Gravity.Center);

            // Save the cropped image to a file
            var baseName = Path.GetFileName(imagePath);
            var destinationPath = Path.Combine(DestinationFolder, baseName);
            canvas.Write(destinationPath);
        }

        private async void HandleStartProcessing()
        {
            await Task.Run(async () =>
            {
                var progress = await _dialogCoordinator.ShowProgressAsync(this, "Please wait", "Cropping images...");
                try
                {
                    foreach (var item in Items)
                    {
                        ProcessImage(item.FilePath);
                        await _dispatcher.InvokeAsync(() => item.Status = "Done");
                    }


                    Process.Start("explorer.exe", $"\"{DestinationFolder}\"");
                }
                catch
                {

                }
                finally
                {
                    await progress.CloseAsync();
                }
            });
        }

        public DelegateCommand<string> BrowseFolderCommand { get => _browseFolderCommand ??= new DelegateCommand<string>(HandleBrowseFolder); }

        private void HandleBrowseFolder(string what)
        {
            var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog
            {
                IsFolderPicker = true
            };
            var result = dialog.ShowDialog();
            if (result == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
            {
                if (what == "destination")
                {
                    DestinationFolder = dialog.FileName;
                }
            }
        }

        public SizingToolViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;
        }
        private async void HandleImportFile()
        {
            var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog
            {
                Multiselect = true,
            };

            dialog.Filters.Add(new Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogFilter("Image Files", "*.jpg;*.png;*.bmp"));

            var result = dialog.ShowDialog();
            if (result != Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
            {
                return;
            }

            var progress = await _dialogCoordinator.ShowProgressAsync(this, "Please wait", "Importing images");
            progress.SetIndeterminate();


            foreach (var filePath in dialog.FileNames)
            {
                if (!Items.Any(x => x.FilePath == filePath))
                    Items.Add(new ImageItem
                    {
                        FilePath = filePath,
                        Status = "Pending"
                    });
            }

            await progress.CloseAsync();
        }

        public SizingToolViewModel()
        {
            Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            HasItems = Items.Count() > 0;
        }
    }
}
