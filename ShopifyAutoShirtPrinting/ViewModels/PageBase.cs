using NLog;
using Prism.Mvvm;
using System.Windows;
using System.Windows.Threading;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    public abstract class PageBase : BindableBase
    {
        protected Dispatcher _dispatcher;
        public abstract string Title { get; }

        public PageBase()
        {
            _dispatcher = Application.Current.Dispatcher;
        }


        public virtual void OnActivated()
        {

        }
    }
}
