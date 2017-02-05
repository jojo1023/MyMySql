using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MyMySql
{
    public class Table : ICommandReturn
    {
        public string Name { get; set; }
        public List<SqlColumn> SqlColumns { get; protected set; }
        public BinaryTree<SqlRow> Tree { get; protected set; }
        //protected internal IDictionary<string, int> SqlColumnIndicesByName { get; private set; }

        int currentId = 0;
        public int CurrentID
        {
            get
            {
                return currentId;
            }
        }

        public Table ReturnTable
        {
            get
            {
                return this;
            }

            set
            {
                Name = value.Name;
                SqlColumns = value.SqlColumns;
                Tree = value.Tree;
                //SqlColumnIndicesByName = value.SqlColumnIndicesByName;
                currentId = value.currentId;
                ReturnString = value.ReturnString;
            }
        }

        public RenderString ReturnString { get; set; }


        /// <summary>
        /// Constructor that intitializes the table
        /// </summary>
        /// <param name="name">The name of the table</param>
        /// <param name="sqlColumns">The columns of the table</param>
        public Table(string name, params SqlColumn[] sqlColumns)
        {
            Name = name;
            SqlColumns = sqlColumns.ToList();
            //SqlColumnIndicesByName = SqlColumns.Select((col, ind) => new { Column = col, Index = ind }).ToDictionary(col => col.Column.Name, col => col.Index);
            Tree = new BinaryTree<SqlRow>();
            ReturnString = null;
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
                collumns.Add(new SqlColumn(columElement.FirstAttribute.Name.ToString(), Type.GetType(columElement.FirstAttribute.Value), this));
            }
            //Sets and initializes the feilds in the tree
            Name = tableElement.Attribute("name").Value;
            SqlColumns = collumns;
            //SqlColumnIndicesByName = SqlColumns.Select((col, ind) => new { Column = col, Index = ind }).ToDictionary(col => col.Column.Name, col => col.Index);
            Tree = new BinaryTree<SqlRow>();

            //Fills the binary tree
            if (tableElement.Element("BinaryTree").HasElements)
            {
                Tree.BaseNode = FillBinaryTreeFromXML(null, tableElement.Element("BinaryTree").Element("Node"));
            }
            ReturnString = new RenderString(new List<RenderCharacter>());
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
        public SqlRow AddRow(List<SqlCell> cells)
        {
            SqlRow newRow;

            //currentId makes sure each row in unique
            Tree.AddNode(newRow = new SqlRow(currentId, this, cells));
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
        public List<SqlRow> Select()
        {
            return Tree.GetNodesInOrder();
        }


        /// <summary>
        /// Sorts the tree into a more balanced orientation
        /// </summary>
        public void Sort()
        {
            Tree.Sort();
        }

        /// <summary>
        /// Deletes a row
        /// </summary>
        /// <param name="rowToDelete">Row to delete</param>
        public void Delete(SqlRow rowToDelete)
        {
            Tree.DeleteNode(rowToDelete);
        }

        ///// <summary>
        ///// Updates all the rows inthe table where they pass the where clause
        ///// </summary>
        ///// <param name="hasWhereClause">If the update command included a where clause or if it schould update all the rows</param>
        ///// <param name="columnValuePairs">A list of the columns to set to values</param>
        ///// <param name="whereColumn">The column in the where clause</param>
        ///// <param name="whereOpperation">The logical opperation in the where clause</param>
        ///// <param name="whereValue">The value in the where clause</param>
        ///// <param name="errors">Any errors that occure while updateing the rows</param>
        ///// <returns>The amount of rows updated</returns>
        //public int Update(bool hasWhereClause, List<ColumnValuePair> columnValuePairs, SqlColumn whereColumn, Opperations whereOpperation, IComparable whereValue, out string errors)
        //{
        //    int returnInt = 0;
        //    errors = "";

        //    //Selects all the rows that pass the where clause
        //    List<SqlRow> itemsToUpdate = Select(hasWhereClause, whereColumn, whereOpperation, whereValue);

        //    //foreach columnValuePair loop through the rows and set the cell of the columnValuePair.Column to columnValuePair.Value
        //    foreach (ColumnValuePair columnValuePair in columnValuePairs)
        //    {
        //        returnInt++;
        //        if (columnValuePair.Column.VarType == columnValuePair.Value.GetType())
        //        {
        //            foreach (SqlRow row in itemsToUpdate)
        //            {
        //                row[columnValuePair.Column.Name].Value = columnValuePair.Value;
        //            }
        //        }
        //        else
        //        {
        //            errors += "Values Don't Match Collumn Types, ";
        //            break;
        //        }
        //    }
        //    return returnInt;
        //}

        /// <summary>
        /// Recursively fills the binary tree from the XML data
        /// </summary>
        /// <param name="lastNode">The last node added to the binary tree</param>
        /// <param name="currentElement">The current XML element to get its data inputed into the tree</param>
        /// <returns>A node to be added to the tree</returns>
        BSTNode<SqlRow> FillBinaryTreeFromXML(BSTNode<SqlRow> lastNode, XElement currentElement)
        {
            //Gets all the values of the cells in the row and initializes the row node
            List<IComparable> values = new List<IComparable>();
            foreach (XElement cellElement in currentElement.Element("Cells").Elements())
            {
                values.Add(cellElement.Attribute("Value").Value);
            }
            SqlRow row = new SqlRow(int.Parse(currentElement.Attribute("id").Value), this, values.ToArray());
            BSTNode<SqlRow> currentNode = new BSTNode<SqlRow>(row, lastNode);

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

        public override string ToString()
        {
            return DisplayTable(this, Select(), SqlColumns);
        }
        /// <summary>
        /// Displays a table and its data
        /// </summary>
        /// <param name="table">The table to diplay</param>
        /// <param name="rows">The rows to display</param>
        /// <param name="columns">The collumns to display</param>
        /// <returns>Returns a string that represents the table with ASCII</returns>
        string DisplayTable(Table table, List<SqlRow> rows, List<SqlColumn> columns)
        {
            string returnString = "";
            List<int> columnWidths = new List<int>(); //How wide a column each needs to be in characters to fit all the data in it
            int paddingSize = 1;
            int headerLevel = 0;//The current level of the header being displayed 0 = bottom of Table, 1 = bottom of Header, 2 = header, 3 = top of Header 

            //Characters for the sides and corners of the table
            char cornerChar = '+';
            char verticalChar = '|';
            char horizontalChar = '-';

            for (int i = 0; i < table.SqlColumns.Count; i++)
            {
                columnWidths.Add(table.SqlColumns[i].Name.Length);
            }
            //displays table without header
            if (rows.Count > 0)
            {
                returnString += DisplayColumns(table, rows, columns, 0, 0, paddingSize, verticalChar, columnWidths, out columnWidths);
                returnString += Environment.NewLine;
            }
            //loops through collumns to add header
            for (int x = columns.Count - 1; x >= 0; x--)
            {
                //if header level is not the bottom of the table
                if (headerLevel != 0)
                {
                    //if on last collumn new Line
                    if (x + 1 >= columns.Count)
                    {
                        returnString = Environment.NewLine + returnString;
                    }
                    //if header level is not header display a solid line
                    if (headerLevel != 2)
                    {
                        returnString = RepeatChar(horizontalChar, columnWidths[x] + (paddingSize * 2)) + cornerChar + returnString;
                    }
                    else
                    {
                        returnString = RepeatChar(' ', paddingSize) + columns[x].Name + RepeatChar(' ', columnWidths[x] + paddingSize - columns[x].Name.Length) + verticalChar + returnString;
                    }
                    //if on first collumn display cornerChar
                    if (x == 0)
                    {
                        //if header level is the header display verticalChar else display cornerChar
                        if (headerLevel == 2)
                        {
                            returnString = verticalChar + returnString;
                        }
                        else
                        {
                            returnString = cornerChar + returnString;
                        }

                    }
                }
                else
                {
                    int oppositeX = columns.Count - 1 - x;

                    //if on first collumn display cornerChar
                    if (oppositeX == 0)
                    {
                        returnString += cornerChar;
                    }
                    returnString += RepeatChar(horizontalChar, columnWidths[oppositeX] + (paddingSize * 2)) + cornerChar;
                    //if on last collumn new Line
                    if (oppositeX + 1 >= columns.Count)
                    {
                        returnString += Environment.NewLine;
                    }
                }
                //if first collumn and current header level is not the top display the next header level
                if (x == 0 && headerLevel < 3)
                {
                    headerLevel++;
                    x = columns.Count;
                }

            }

            return returnString;
        }

        //Displays the value for each column
        string DisplayColumns(Table table, List<SqlRow> rows, List<SqlColumn> columns, int rowIndex, int columnIndex, int paddingSize, char verticalChar, List<int> currentColumnWidths, out List<int> columnWidths)
        {
            string returnString = "";
            int amountOfExtraPaddingForRow;
            columnWidths = currentColumnWidths;
            string nextCollumn = "";
            string nextRow = "";
            //if on first collumn display wall
            if (columnIndex == 0)
            {
                returnString += verticalChar;
            }

            //if this rows data is longer than the collumn width set the collumn width to the length of this rows data
            if (columnWidths[columnIndex] < rows[rowIndex].Cells[columnIndex].Value.ToString().Length)
            {
                columnWidths[columnIndex] = rows[rowIndex].Cells[columnIndex].Value.ToString().Length;
            }

            returnString += RepeatChar(' ', paddingSize) + rows[rowIndex].Cells[columnIndex].Value.ToString();//Dislays current row and collumn value

            //if not on last collumn get next collumn
            if (columnIndex + 1 < columns.Count)
            {
                nextCollumn = DisplayColumns(table, rows, columns, rowIndex, columnIndex + 1, paddingSize, verticalChar, columnWidths, out columnWidths);
            }
            //if not on last row get next row
            else if (rowIndex + 1 < rows.Count)
            {
                nextRow = DisplayColumns(table, rows, columns, rowIndex + 1, 0, paddingSize, verticalChar, columnWidths, out columnWidths);
            }
            //add padding for collumn
            amountOfExtraPaddingForRow = columnWidths[columnIndex] - rows[rowIndex].Cells[columnIndex].Value.ToString().Length;
            returnString += RepeatChar(' ', paddingSize + amountOfExtraPaddingForRow) + verticalChar;
            //display next collumn and row
            returnString += nextCollumn;
            //if there is a next row new line and display it
            if (nextRow != "")
            {
                returnString += Environment.NewLine + nextRow;
            }


            return returnString;
        }

        //Returns a string of a character repeated for a certain amount of time
        string RepeatChar(char letter, int amountOfTimes)
        {
            string returnString = "";
            for (int i = 0; i < amountOfTimes; i++)
            {
                returnString += letter;
            }
            return returnString;
        }
    }

}

