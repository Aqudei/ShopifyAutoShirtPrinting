using ShopifyEasyShirtPrinting.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ShopifyEasyShirtPrinting.Views
{
    /// <summary>
    /// Interaction logic for OrganizerView.xaml
    /// </summary>
    public partial class OrganizerView : UserControl
    {
        public OrganizerView()
        {
            InitializeComponent();
        }

        private void DataGrid_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void DataGrid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var vm = DataContext as OrganizerViewModel;
                if (vm != null)
                {
                    vm.HandleFilesDrop(files);
                }
                // Process the dropped files
                // For example, you can add the file paths to a list and bind it to the DataGrid
            }
            e.Handled = true;
        }
    }
}
