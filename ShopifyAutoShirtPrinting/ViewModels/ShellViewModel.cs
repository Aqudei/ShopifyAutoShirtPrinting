using Prism.Mvvm;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    public class ShellViewModel : BindableBase
    {

        public string Title => "TLKC Easy Workflow";
       
        public ShellViewModel(DryIoc.Container container)
        {
        }
    }
}
