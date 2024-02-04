using Prism.Services.Dialogs;

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
        }

        public IDialogResult Result { get; set; }
    }
}
