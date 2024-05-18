using ShopifyEasyShirtPrinting.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ShopifyEasyShirtPrinting.Views
{
    /// <summary>
    /// Interaction logic for ArchivesView.xaml
    /// </summary>
    public partial class ShipmentsView : UserControl
    {
        public ShipmentsView()
        {
            InitializeComponent();

            Loaded += ArchivesView_Loaded;
        }

        private void ArchivesView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ShipmentsViewModel vm)
                vm.Loaded();
        }

        private void ArchivesDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                e.Cancel = true;
            }
        }
    }
}
