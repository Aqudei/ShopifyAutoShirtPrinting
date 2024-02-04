using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintAgent.ViewModels
{
    public class MainViewModel : PageBase
    {
        public override string Title
        {
            get
            {
                if (AppDomain.CurrentDomain.BaseDirectory.ToLower().Contains("staging"))
                {
                    return "Print Agent (Staging)";
                }

                return "Print Agent (Stable)";
            }
        }

        public MainViewModel()
        {
            Debug.WriteLine(AppDomain.CurrentDomain.FriendlyName);
        }
    }


}
