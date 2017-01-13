using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadSql
{
    [DebuggerDisplay("{Min} - {Max}")]

    //The range of children that key words have includeing if they can have comma groups or parentheses groups
    
    public class Range
    {
        public int Min { get; set; }//inclusive minimum amount of children
        public int Max { get; set; }//exclusive maximium amount of children
        public bool CanHaveParentheses { get; set; }
        public bool CanHaveCommas { get; set; }
        /// <summary>
        /// Constructor that initializes all the feilds
        /// </summary>
        /// <param name="min">inclusive minimum amount of children</param>
        /// <param name="max">exclusive maximium amount of children</param>
        /// <param name="canHaveParentheses">If it can have parentheses groups as children</param>
        /// <param name="canHaveCommas">If it can have comas groups as children</param>
        public Range(int min, int max, bool canHaveParentheses, bool canHaveCommas)
        {
            Min = min;
            Max = max;
            CanHaveParentheses = canHaveParentheses;
            CanHaveCommas = canHaveCommas;
        }
    }
}
