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
        protected internal IDictionary<string, int> SqlColumnIndicesByName { get; private set; }

        int currentId = 0;
        public int CurrentID
        {
            get
            {
                return currentId;
            }
        }

        /// <summary>
        /// Constructor that intitializes the table
        /// </summary>
        /// <param name="name">The name of the table</param>
        /// <param name="sqlColumns">The columns of the table</param>
        public Table(string name, params SqlColumn[] sqlColumns)
        {
            Name = name;
            SqlColumns = sqlColumns.ToList();
            SqlColumnIndicesByName = SqlColumns.Select((col, ind) => new { Column = col, Index = ind }).ToDictionary(col => col.Column.Name, col => col.Index);
            Tree = new BinaryTree<SqlRow>();
        }

        /// <summary>
        /// Constructor that initializes the table from an XML element
        /// </summary>
        /// <param name="tableElement">The element to fill the table from</param>
        public Table(XElement tableElement)
        {
            currentId = int.Parse(tableElement.Attribute("currentID").Value);
            List<SqlColumn> collumns = new List<SqlColumn>();

            //loops though the elements in columns to get the columns of the tree
            foreach (XElement columElement in tableElement.Element("Columns").Elements())
            {
                collumns.Add(new SqlColumn(columElement.FirstAttribute.Name.ToString(), Type.GetType(columElement.FirstAttribute.Value)));
            }
            //Sets and initializes the feilds in the tree
            Name = tableElement.Attribute("name").Value;
            SqlColumns = collumns;
            SqlColumnIndicesByName = SqlColumns.Select((col, ind) => new { Column = col, Index = ind }).ToDictionary(col => col.Column.Name, col => col.Index);
            Tree = new BinaryTree<SqlRow>();

            //Fills the binary tree
            if (tableElement.Element("BinaryTree").HasElements)
            {
                Tree.BaseNode = FillBinaryTreeFromXML(null, tableElement.Element("BinaryTree").Element("Node"));
            }
        }

        /// <summary>
        /// Initializes and adds a row into the table by adding it to the binary tree
        /// </summary>
        /// <param name="values">The values of the cells in the row</param>
        /// <returns>Returns the new row</returns>
        public SqlRow AddRow(params IComparable[] values)
        {
            SqlRow newRow;

            //currentId makes sure each row in unique
            Tree.AddNode(newRow = new SqlRow(currentId, this, values));
            currentId++;
            return newRow;
        }

        /// <summary>
        /// Adds a row into the table by adding it to the binary tree 
        /// </summary>
        /// <param name="newRow">The row being added</param>
        public void AddRow(SqlRow newRow)
        {
            Tree.AddNode(newRow);
        }

        /// <summary>
        /// Selects the rows in the table where the rows match the where clause
        /// </summary>
        /// <param name="hasWhereClause">If the select command included a where clause or if it schould select all the rows</param>
        /// <param name="column">The column in the where clause</param>
        /// <param name="opperation">The logical opperation in the where clause</param>
        /// <param name="value">The value in the where clause</param>
        /// <returns>A sorted list of the rows selected</returns>
        public List<SqlRow> Select(bool hasWhereClause, SqlColumn column, Opperations opperation, IComparable value)
        {
            List<SqlRow> sortedRows = Tree.GetNodesInOrder();
            //if hasWhereClause check each row else return every row
            if (hasWhereClause)
            {
                List<SqlRow> returnRows = new List<SqlRow>();
                //if value is the same type as the values in the column
                if (column.VarType == value.GetType())
                {
                    //for every row in the table see if they pass the whereClause and if they do add them to return list
                    foreach (SqlRow row in sortedRows)
                    {
                        IComparable currentCellValue = row[column.Name].Value;
                        //see if the current row passes the where clause by checking if the currentCellValue opperation value retruns true
                        switch (opperation)
                        {
                            case (Opperations.Equal):
                                if (currentCellValue.Equals(value))
                                {
                                    returnRows.Add(row);
                                }
                                break;
                            case (Opperations.GreaterThan):
                                if (currentCellValue.CompareTo(value) > 0)
                                {
                                    returnRows.Add(row);
                                }
                                break;
                            case (Opperations.LessThan):
                                if (currentCellValue.CompareTo(value) < 0)
                                {
                                    returnRows.Add(row);
                                }
                                break;
                            case (Opperations.GreaterThanOrEqual):
                                if (currentCellValue.CompareTo(value) >= 0)
                                {
                                    returnRows.Add(row);
                                }
                                break;
                            case (Opperations.LessThanOrEqual):
                                if (currentCellValue.CompareTo(value) <= 0)
                                {
                                    returnRows.Add(row);
                                }
                                break;
                            case (Opperations.NotEqual):
                                if (!currentCellValue.Equals(value))
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

        /// <summary>
        /// Gets a column from its name
        /// </summary>
        /// <param name="colName">The name of the column to be found</param>
        /// <returns>Returns a column</returns>
        public SqlColumn this[string colName]
        {
            get
            {
                //If SqlColumnIndicesByName has the name colName then return the index of that column
                int colIndex;
                if (!SqlColumnIndicesByName.TryGetValue(colName, out colIndex))
                {
                    return null;
                }

                return SqlColumns[colIndex];
            }
        }
        
        /// <summary>
        /// Sorts the tree into a more balanced orientation
        /// </summary>
        public void Sort()
        {
            Tree.Sort();
        }

        /// <summary>
        /// Deletes all the rows in the tree where they pass the where clause
        /// </summary>
        /// <param name="hasWhereClause">If the delete command included a where clause or if it schould delete all the rows</param>
        /// <param name="column">The column in the where clause</param>
        /// <param name="opperation">The logical opperation in the where clause</param>
        /// <param name="value">The value in the where clause</param>
        /// <returns>The amount of rows that where deleted</returns>
        public int Delete(bool hasWhereClause, SqlColumn column, Opperations opperation, IComparable value)
        {
            //Selects all the items to be deleted and deletes them from the tree
            int returnInt = 0;
            List<SqlRow> itemsToDelete = Select(hasWhereClause, column, opperation, value);
            foreach (SqlRow row in itemsToDelete)
            {
                Tree.DeleteNode(row);
                returnInt++;
            }
            return returnInt;
        }

        /// <summary>
        /// Updates all the rows inthe table where they pass the where clause
        /// </summary>
        /// <param name="hasWhereClause">If the update command included a where clause or if it schould update all the rows</param>
        /// <param name="columnValuePairs">A list of the columns to set to values</param>
        /// <param name="whereColumn">The column in the where clause</param>
        /// <param name="whereOpperation">The logical opperation in the where clause</param>
        /// <param name="whereValue">The value in the where clause</param>
        /// <param name="errors">Any errors that occure while updateing the rows</param>
        /// <returns>The amount of rows updated</returns>
        public int Update(bool hasWhereClause, List<ColumnValuePair> columnValuePairs, SqlColumn whereColumn, Opperations whereOpperation, IComparable whereValue, out string errors)
        {
            int returnInt = 0;
            errors = "";

            //Selects all the rows that pass the where clause
            List<SqlRow> itemsToUpdate = Select(hasWhereClause, whereColumn, whereOpperation, whereValue);

            //foreach columnValuePair loop through the rows and set the cell of the columnValuePair.Column to columnValuePair.Value
            foreach (ColumnValuePair columnValuePair in columnValuePairs)
            {
                returnInt++;
                if (columnValuePair.Column.VarType == columnValuePair.Value.GetType())
                {
                    foreach (SqlRow row in itemsToUpdate)
                    {
                        row[columnValuePair.Column.Name].Value = columnValuePair.Value;
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

        /// <summary>
        /// Recursively fills the binary tree from the XML data
        /// </summary>
        /// <param name="lastNode">The last node added to the binary tree</param>
        /// <param name="currentElement">The current XML element to get its data inputed into the tree</param>
        /// <returns>A node to be added to the tree</returns>
        Node<SqlRow> FillBinaryTreeFromXML(Node<SqlRow> lastNode, XElement currentElement)
        {
            //Gets all the values of the cells in the row and initializes the row node
            List<IComparable> values = new List<IComparable>();
            foreach (XElement cellElement in currentElement.Element("Cells").Elements())
            {
                values.Add(cellElement.Attribute("Value").Value);
            }
            SqlRow row = new SqlRow(int.Parse(currentElement.Attribute("id").Value), this, values.ToArray());
            Node<SqlRow> currentNode = new Node<SqlRow>(row, lastNode);

            //Recursively gets this currentNode's children from the XML
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

