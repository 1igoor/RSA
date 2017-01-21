using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekt.Exceptions
{
    class ConnectionPropertyException : Exception
    {
        public ConnectionPropertyException(string msg): base(msg)
        {

        }
    }
}
