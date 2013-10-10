using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.EventArguments
{
    public class CommandLineEventArgs
    {
        public string[] Arguments { get; private set; }

        public CommandLineEventArgs(string[] args)
        {
            Arguments = args;
        }
    }

    public delegate void CommandLineEventHandler(object sender, CommandLineEventArgs e);
}
