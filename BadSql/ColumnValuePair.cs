using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadSql
{
    //Pair of collumn and values used for the set Command
    public class ColumnValuePair
    {
        public SqlColumn Column { get; set; }
        public IComparable Value { get; set; }

        public ColumnValuePair(SqlColumn column, IComparable value)
        {
            Column = column;
            Value = value;
        }
    }
}
