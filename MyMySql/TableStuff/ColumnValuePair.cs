using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql
{
    //Pair of collumn and values used for the set command
    public class ColumnValuePair
    {
        public SqlColumn Column { get; set; }
        public IComparable Value { get; set; }

        /// <summary>
        /// Constructor that initializes and sets the variables
        /// </summary>
        /// <param name="column">The column to set</param>
        /// <param name="value">The value to set the column to</param>
        public ColumnValuePair(SqlColumn column, IComparable value)
        {
            Column = column;
            Value = value;
        }
    }
}
