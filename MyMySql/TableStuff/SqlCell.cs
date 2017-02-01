using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql
{
    public class SqlCell
    {
        public SqlColumn Collumn { get; }
        public SqlRow Row { get; }
        public IComparable Value { get; set; }

        /// <summary>
        /// Constructor that initializes and sets all the variables in the class
        /// </summary>
        /// <param name="value">The data stored in the cell</param>
        /// <param name="collumn">The column that this cell is in</param>
        /// <param name="row">The row that this cell is in</param>
        public SqlCell(IComparable value, SqlColumn collumn, SqlRow row)
        {
            Value = value;
            Collumn = collumn;
            Row = row;
        }
    }
}
