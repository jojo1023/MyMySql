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
            Tree = new BinaryTree<SqlRow>();
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

        public List<SqlRow> Select(bool hasWhereClause, string columnName, Opperations opperation, IComparable value)
        {
            List<SqlRow> sortedRows = Tree.GetNodesInOrder();
            if (hasWhereClause)
            {
                List<SqlRow> returnRows = new List<SqlRow>();
                int temp;
                if (SqlColumnIndicesByName.TryGetValue(columnName, out temp) && SqlColumns[temp].VarType == value.GetType())
                {
                    foreach (SqlRow row in sortedRows)
                    {
                        IComparable currentRowValue = row[columnName].Value;
                        switch (opperation)
                        {
                            case (Opperations.Equal):
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
                            case (Opperations.GreaterThanOrEqual):
                                if (currentRowValue.CompareTo(value) >= 0)
                                {
                                    returnRows.Add(row);
                                }
                                break;
                            case (Opperations.LessThanOrEqual):
                                if (currentRowValue.CompareTo(value) <= 0)
                                {
                                    returnRows.Add(row);
                                }
                                break;
                            case (Opperations.NotEqual):
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
            return sortedRows;
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

        protected internal IDictionary<string, int> SqlColumnIndicesByName { get; private set; }

        public void Sort()
        {
            Tree.Sort();
        }

        public int Delete(bool hasWhereClause, string columnName, Opperations opperation, IComparable value)
        {
            int returnInt = 0;
            List<SqlRow> itemsToDelete = Select(hasWhereClause, columnName, opperation, value);
            foreach (SqlRow row in itemsToDelete)
            {
                Tree.DeleteNode(row);
                returnInt++;
            }
            return returnInt;
        }

        public int Update(bool hasWhereClause, List<ColumnValuePair> setPairs, string columnName, Opperations opperation, IComparable whereValue, out string errors)
        {
            int returnInt = 0;
            errors = "";
            List<SqlRow> itemsToUpdate = Select(hasWhereClause, columnName, opperation, whereValue);
            foreach (ColumnValuePair setPair in setPairs)
            {
                returnInt++;
                if (setPair.Column.VarType == setPair.Value.GetType())
                {
                    foreach (SqlRow row in itemsToUpdate)
                    {
                        row[setPair.Column.Name].Value = setPair.Value;
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
            Tree = new BinaryTree<SqlRow>();

            if (tableElement.Element("BinaryTree").HasElements)
            {
                Tree.BaseNode = FillBinaryTreeFromXML(null, tableElement.Element("BinaryTree").Element("Node"));
            }
        }
        Node<SqlRow> FillBinaryTreeFromXML(Node<SqlRow> lastNode, XElement currentElement)
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

}

