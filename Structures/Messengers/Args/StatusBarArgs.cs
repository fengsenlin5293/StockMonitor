using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structures.Messengers.Args
{
    public class StatusBarArgs
    {
        public StatusBarArgs(bool isBusy, string message)
        {
            this.IsBusy = isBusy;
            this.Message = message;
        }
        public bool IsBusy { get; private set; }
        public string Message { get; set; }
    }
}
