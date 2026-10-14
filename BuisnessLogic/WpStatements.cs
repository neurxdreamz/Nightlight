using System;
using System.Collections.Generic;
using System.Text;

namespace BuisnessLogic
{
    public class WpStatements
    {
        public abstract class Statement
        {
            public abstract string RawText { get; }
        }
    }
}
