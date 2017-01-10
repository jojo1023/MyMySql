using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BadSql
{
    public class Table
    {
        public string Name { get; set; }
        public List<SqlColumn> SqlColumns { get; }
        public BinaryTree<SqlRow> Tree { get; protected set; }

        int currentId = 0;
        public int CurrentID
        {
            get
            {
                return currentId;
            }
        }
        public Table(string name, params SqlColumn[] sqlColumns)
        {
            Name = name;
            SqlColumns = sqlColumns.ToList();
            SqlColumnIndicesByName = SqlColumns.Select((col, ind) => new { Column = col, Index = ind }).ToDictionary(col => col.Column.Name, col => col.Index);
            Tree = new BinaryTree<SqlRow>(2);
        }
        public SqlRow AddRow(params IComparable[] values)
        {
            SqlRow newRow;
            Tree.AddNode(newRow = new SqlRow(currentId, this, values));
            currentId++;
            return newRow;
        }
        public void AddRow(SqlRow newRow)
        {
            Tree.AddNode(newRow);
        }

        public List<SqlRow> Select(string columnName, Opperations opperation, IComparable value)
        {
            List<SqlRow> sortedRows = Tree.GetNodesSorted();
            List<SqlRow> returnRows = new List<SqlRow>();
            int temp;
            if (SqlColumnIndicesByName.TryGetValue(columnName, out temp) && SqlColumns[temp].VarType == value.GetType())
            {
                foreach (SqlRow row in sortedRows)
                {
                    IComparable currentRowValue = row[columnName].Value;
                    switch (opperation)
                    {
                        case (Opperations.EqualTo):
                            if (currentRowValue.Equals(value))
                            {
                                returnRows.Add(row);
                            }
                            break;
                        case (Opperations.GreaterThan):
                            if (currentRowValue.CompareTo(value) > 0)
                            {
                                returnRows.Add(row);
                            }
                            break;
                        case (Opperations.LessThan):
                            if (currentRowValue.CompareTo(value) < 0)
                            {
                                returnRows.Add(row);
                            }
                            break;
                        case (Opperations.GreaterThanOrEqualTo):
                            if (currentRowValue.CompareTo(value) >= 0)
                            {
                                returnRows.Add(row);
                            }
                            break;
                        case (Opperations.LessThanOrEqualTo):
                            if (currentRowValue.CompareTo(value) <= 0)
                            {
                                returnRows.Add(row);
                            }
                            break;
                        case (Opperations.NotEqualTo):
                            if (!currentRowValue.Equals(value))
                            {
                                returnRows.Add(row);
                            }
                            break;
                    }

                }
            }

            return returnRows;
        }

        public List<SqlRow> Select()
        {
            return Tree.GetNodesSorted();
        }

        public SqlColumn this[string colName]
        {
            get
            {
                int colInd;
                if (!SqlColumnIndicesByName.TryGetValue(colName, out colInd))
                {
                    return null;
                }

                return SqlColumns[colInd];
            }
        }
        // Select((row) => row.GetHashCode() <= 2)

        protected internal IDictionary<string, int> SqlColumnIndicesByName { get; private set; }

        public void Sort()
        {
            Tree.Sort();
        }

        public int Delete(string columnName, Opperations opperation, IComparable value)
        {
            int returnInt = 0;
            List<SqlRow> itemsToDelete = Select(columnName, opperation, value);
            foreach (SqlRow row in itemsToDelete)
            {
                Tree.UserDelete(row);
                returnInt++;
            }
            return returnInt;
        }
        public int Delete()
        {
            int returnInt = 0;
            List<SqlRow> itemsToDelete = Select();
            foreach (SqlRow row in itemsToDelete)
            {
                Tree.UserDelete(row);
                returnInt++;
            }
            return returnInt;
        }

        public int Update(List<SetPair> setPairs, string columnName, Opperations opperation, IComparable whereValue, out string errors)
        {
            int returnInt = 0;
            errors = "";
            List<SqlRow> itemsToUpdate = Select(columnName, opperation, whereValue);
            foreach (SetPair setPair in setPairs)
            {
                returnInt++;
                if (setPair.Collum.VarType == setPair.Value.GetType())
                {
                    foreach (SqlRow row in itemsToUpdate)
                    {
                        row[setPair.Collum.Name].Value = setPair.Value;
                    }
                }
                else
                {
                    errors += "Values Don't Match Collumn Types, ";
                    break;
                }
            }
            return returnInt;
        }
        public int Update(List<SetPair> setPairs, out string errors)
        {
            int returnInt = 0;
            errors = "";
            List<SqlRow> itemsToUpdate = Select();
            foreach (SetPair setPair in setPairs)
            {
                returnInt++;
                if (setPair.Collum.VarType == setPair.Value.GetType())
                {
                    foreach (SqlRow row in itemsToUpdate)
                    {
                        row[setPair.Collum.Name].Value = setPair.Value;
                    }
                }
                else
                {
                    errors += "Values Don't Match Collumn Types, ";
                    break;
                }
            }
            return returnInt;
        }

        public Table(XElement tableElement)
        {
            currentId = int.Parse(tableElement.Attribute("currentID").Value);
            List<SqlColumn> collumns = new List<SqlColumn>();
            foreach (XElement collumElement in tableElement.Element("Collumns").Elements())
            {
                collumns.Add(new SqlColumn(collumElement.FirstAttribute.Name.ToString(), Type.GetType(collumElement.FirstAttribute.Value)));
            }

            Name = tableElement.Attribute("name").Value;
            SqlColumns = collumns;
            SqlColumnIndicesByName = SqlColumns.Select((col, ind) => new { Column = col, Index = ind }).ToDictionary(col => col.Column.Name, col => col.Index);
            Tree = new BinaryTree<SqlRow>(2);

            if (tableElement.Element("BinaryTree").HasElements)
            {
                Tree.BaseNode = FillBinaryTreeFromXML(null, tableElement.Element("BinaryTree").Element("Node"));
            }
        }
        public Node<SqlRow> FillBinaryTreeFromXML(Node<SqlRow> lastNode, XElement currentElement)
        {
            List<IComparable> values = new List<IComparable>();
            foreach (XElement cellElement in currentElement.Element("Cells").Elements())
            {
                values.Add(cellElement.Attribute("Value").Value);
            }
            SqlRow row = new SqlRow(int.Parse(currentElement.Attribute("id").Value), this, values.ToArray());

            Node<SqlRow> currentNode = new Node<SqlRow>(row, lastNode);

            if (currentElement.Element("Left").HasElements)
            {
                currentNode.Left = FillBinaryTreeFromXML(currentNode, currentElement.Element("Left").Element("Node"));
            }
            if (currentElement.Element("Right").HasElements)
            {
                currentNode.Right = FillBinaryTreeFromXML(currentNode, currentElement.Element("Right").Element("Node"));
            }

            return currentNode;
        }
    }
    public class SetPair
    {
        public SqlColumn Collum { get; set; }
        public IComparable Value { get; set; }
        public SetPair(SqlColumn collumn, IComparable value)
        {
            Collum = collumn;
            Value = value;
        }
    }

}

