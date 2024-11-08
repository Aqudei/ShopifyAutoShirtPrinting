using Common.BGTasker;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.BGTasker
{
    public abstract class BGTaskBase : BindableBase, IBGTask
    {
        private volatile bool doRun = true;
        private string _status;

        public string Name => "";

        public string Status { get => _status; set => SetProperty(ref _status , value); }

        protected bool ContinueRunning { get => doRun; set => doRun = value; }

        public abstract void Execute();

        public virtual void Stop()
        {
            ContinueRunning = false;
        }
    }
}
