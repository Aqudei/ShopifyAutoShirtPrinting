﻿using ShopifyEasyShirtPrinting.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShopifyEasyShirtPrinting.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for LoginDialog.xaml
    /// </summary>
    public partial class LoginDialog : UserControl
    {
        public LoginDialog()
        {
            InitializeComponent();
        }

        private void MyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var vm = (DataContext as LoginDialogViewModel);
            if (vm != null)
            {
                vm.Password = MyPasswordBox.SecurePassword;
                vm.ErrorMessage = "";
            }
        }
    }
}
