using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClawlerInterfaces.Exceptions
{
    public class ClawlerConfigException : Exception
    {
        private string _message;
        public ClawlerConfigException()
        {

        }

        public ClawlerConfigException(string message)
        {
            this._message = message;
        }


        public override string Message => this._message;
    }
}
