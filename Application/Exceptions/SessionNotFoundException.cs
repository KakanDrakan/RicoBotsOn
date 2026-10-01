using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Exceptions
{
    public class SessionNotFoundException : Exception
    {
        public SessionNotFoundException(string code) : base($"No session found for code '{code}'.") { }
    }
}
