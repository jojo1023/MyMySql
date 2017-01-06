using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadSql
{
    public class SqlCell
    {
        public SqlColumn Collumn { get; }
        public SqlRow Row { get; }
        public IComparable Value { get; set; }
        public SqlCell(IComparable value, SqlColumn collumn, SqlRow row)
        {
            Value = value;
            Collumn = collumn;
            Row = row;
        }
    }
}
