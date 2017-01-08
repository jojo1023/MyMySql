using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadSql
{
    [DebuggerDisplay("Input = {Input}")]
    public class SqlCustomInput : ISqlInput
    {
        public string Input { get; set; }
        public SqlKeyWord Parent { get; set; }
        public bool InQuotes { get; set; }
        public SqlCustomInput(string input, SqlKeyWord parent, bool inQuotes)
        {
            Input = input;
            Parent = parent;
            InQuotes = inQuotes;
        }
    }
}
