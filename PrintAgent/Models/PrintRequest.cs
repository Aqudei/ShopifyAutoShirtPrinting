using Common.Models;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintAgent.Models
{
    public class PrintRequest : BindableBase
    {
        private string _printFile;
        private string _status;

        public DateTime Timestamp { get; set; }
        public MyLineItem LineItem { get; set; }
        public Variant Variant { get; set; }
        public string PrintFile { get => _printFile; set => SetProperty(ref _printFile, value); }

        public string Status { get => _status; set => SetProperty(ref _status, value); }
    }
}
