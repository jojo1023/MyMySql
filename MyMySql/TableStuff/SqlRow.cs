using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql
{
    public class SqlRow : IComparable
    {
        public Table OwningTable { get; }
        public List<SqlCell> Cells { get; }
        public int ID { get; private set; }

        /// <summary>
        /// Constructor that initializes and sets all the variables
        /// </summary>
        /// <param name="id">The hidden id of this row that is used in sorting rows</param>
        /// <param name="table">The table that this row is in</param>
        /// <param name="values">The values of the cells for each column in the table</param>
        public SqlRow(int id, Table table, params IComparable[] values)
        {
            ID = id;
            OwningTable = table;
            Cells = new List<SqlCell>();

            //if the amount of cells are equal to the amount of columns then create a cell for every value and cast the value to the coresponding column type
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
        
        /// <summary>
        /// Gets a cell from the name of the column its in
        /// </summary>
        /// <param name="colName">the name of the column that the cell is in</param>
        /// <returns>The cell that is in the column</returns>
        public SqlCell this[string colName]
        {
            get
            {
                //if the owning table has the column then get the index and return the cell in that index
                int colInd;
                if (!OwningTable.SqlColumnIndicesByName.TryGetValue(colName, out colInd))
                {
                    return null;
                }

                return Cells[colInd];
            }
            set
            {
                //if the owning table has the column then get the index and set the cell in that index
                int colInd;
                if (!OwningTable.SqlColumnIndicesByName.TryGetValue(colName, out colInd))
                {
                    throw new IndexOutOfRangeException();
                }

                Cells[colInd] = value;
            }
        }

        /// <summary>
        /// Compares an object with ID
        /// </summary>
        /// <param name="obj">The object to compare to</param>
        /// <returns>Returns -1 if the object is less than ID, 1 if the object is greater than the ID, and 0 if the object equal to the ID</returns>
        public int CompareTo(object obj)
        {
            //if the object is a Sqlrow compare their IDs
            if (obj.GetType() == typeof(SqlRow))
            {
                return ID.CompareTo(((SqlRow)obj).ID);
            }
            return 0;
        }
    }
}
