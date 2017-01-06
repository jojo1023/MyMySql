using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadSql
{
    public class SqlRow : IComparable
    {
        public Table OwningTable { get; }
        public int Id { get; private set; }
        public SqlRow(int id, Table table, params IComparable[] values)
        {
            Id = id;
            OwningTable = table;
            Cells = new List<SqlCell>();
            if (values.Length == OwningTable.SqlColumns.Count)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    SqlColumn currentCollumn = OwningTable.SqlColumns[i];
                    object compareValue = ((IConvertible)values[i]).ToType(currentCollumn.VarType, System.Globalization.CultureInfo.InvariantCulture);

                    if (compareValue is IComparable)
                    {
                        Cells.Add(new SqlCell((IComparable)compareValue, OwningTable.SqlColumns[i], this));
                    }
                    else
                    {
                        throw new InvalidCastException();
                    }
                }
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

        public List<SqlCell> Cells { get; }

        public SqlCell this[string colName]
        {
            get
            {
                int colInd;
                if (!OwningTable.SqlColumnIndicesByName.TryGetValue(colName, out colInd))
                {
                    return null;
                }

                return Cells[colInd];
            }
            set
            {
                int colInd;
                if (!OwningTable.SqlColumnIndicesByName.TryGetValue(colName, out colInd))
                {
                    throw new IndexOutOfRangeException();
                }

                Cells[colInd] = value;
            }
        }

        public int CompareTo(object obj)
        {
            if (obj.GetType() == typeof(SqlRow))
            {
                return Id.CompareTo(((SqlRow)obj).Id);
            }
            return 0;
        }
    }
}
