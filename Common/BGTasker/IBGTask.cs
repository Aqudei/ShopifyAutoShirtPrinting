using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.BGTasker
{
    public interface IBGTask
    {
        void Execute();
        void Stop();
        string Name { get; }
        string Status { get; set; }
    }
}
