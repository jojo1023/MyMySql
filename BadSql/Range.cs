using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadSql
{
    [DebuggerDisplay("{Min} - {Max}")]
    public class Range
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public bool CanHaveParentheses { get; set; }
        public bool CanHaveCommas { get; set; }
        public Range(int min, int max, bool canHaveParentheses, bool canHaveCommas)
        {
            Min = min;
            Max = max;
            CanHaveParentheses = canHaveParentheses;
            CanHaveCommas = canHaveCommas;
        }
    }
}
