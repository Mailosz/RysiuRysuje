using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RysiuRysuj
{
    public class UserCommand
    {
        string command;
        public UserCommand(string com)
        {
            command = com;
        }

        public override string ToString() => command;
    }
}
