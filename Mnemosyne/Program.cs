﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mnemosyne
{
    class Program
    {
        static void Main(string[] args)
        {
            new MnemosyneLogger().Info(new LogMessage("TestMessage"));
        }
    }
}
