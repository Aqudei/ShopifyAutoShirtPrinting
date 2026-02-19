using Prism.Services.Dialogs;
using System.Windows;

namespace ShopifyEasyShirtPrinting.Views
{
    /// <summary>
    /// Interaction logic for MetroDialogWindow.xaml
    /// </summary>
    public partial class MetroDialogWindow : IDialogWindow
    {
        public MetroDialogWindow()
        {
            InitializeComponent();

            // Set MaxWidth to 90% of the primary screen width
            MaxWidth = SystemParameters.PrimaryScreenWidth * 0.9;

            // Optional: Set MaxHeight to 90% of the primary screen height
            MaxHeight = SystemParameters.PrimaryScreenHeight * 0.9;

            // 2. Hook into the SizeChanged event to keep it centered
            this.SizeChanged += (s, e) =>
            {
                // Calculate the center position relative to the WorkArea (excludes Taskbar)
                double screenWidth = SystemParameters.WorkArea.Width;
                double screenHeight = SystemParameters.WorkArea.Height;

                this.Left = (screenWidth - this.ActualWidth) / 2 + SystemParameters.WorkArea.Left;
                this.Top = (screenHeight - this.ActualHeight) / 2 + SystemParameters.WorkArea.Top;
            };
        }

      
        public IDialogResult Result { get; set; }
    }
}
