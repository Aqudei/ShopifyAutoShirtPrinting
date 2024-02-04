using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintAgent.Models
{
    public interface IRule
    {
        string GetHotFolder(PrintRequest lineItem);
    }
}
