using NLog;
using Prism.Mvvm;
using System.Windows;
using System.Windows.Threading;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    public abstract class PageBase : BindableBase
    {
        protected Dispatcher _dispatcher;
        private string _title;

        public string Title { get => _title; set => SetProperty(ref _title , value); }

        public PageBase()
        {
            _dispatcher = Application.Current.Dispatcher;
        }


        public virtual void OnActivated()
        {

        }
    }
}
