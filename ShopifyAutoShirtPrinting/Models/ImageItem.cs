using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Models
{
    internal class ImageItem : BindableBase
    {
        private string _filePath;
        private string _status = "Pending";

        public string FilePath { get => _filePath; set => SetProperty(ref _filePath, value); }
        public string Status { get => _status; set => SetProperty(ref _status, value); }
    }
}
