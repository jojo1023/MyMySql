using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BadSql
{
    class Program
    {
        static int currentId = 0;
        static BinaryTree<Student> tree = new BinaryTree<Student>(2);
        static string xmlFile = "SqlData.xml";
        static XDocument xdoc = XDocument.Load(xmlFile);
        static Dictionary<string, Table> tables = new Dictionary<string, Table>();
        public static void Main(string[] args)
        {
            LoadXML();
            //Table testTable = new Table("Students", new SqlColumn("ID", typeof(int)),
            //    new SqlColumn("StudentName", typeof(string)),
            //    new SqlColumn("IsCool", typeof(bool)));

            //tables.Add("Students", testTable);
            //DisplayRows(tables["Students"], tables["Students"].Select());
            Random random = new Random();
            List<string> input;
            //testTable.AddRow(0, "Glen", false);
            //testTable.AddRow(1, "Glen", false);
            //testTable.AddRow(2, "Glen", true);
            //testTable.AddRow(3, "Josiah", true);
            //testTable.AddRow(4, "Josiah", true);
            //testTable.AddRow(5, "Josiah", true);
            //testTable.AddRow(6, "Josiah", true);
            //testTable.AddRow(7, "Aaron", true);
            //testTable.Sort();

            SqlColumn whereCollum;
            Opperations whereOpperation;
            IComparable whereValue;
            bool hasWhere;
            do
            {
                //input = Console.ReadLine().Split(' ');
                input = Split(Console.ReadLine());
                if (input.Count > 0)
                {
                    switch (input[0].ToLower())
                    {
                        #region select
                        case ("select"):
                            if (input.Count >= 4)
                            {
                                List<SqlColumn> collums = new List<SqlColumn>();
                                whereCollum = null;
                                whereOpperation = Opperations.EqualTo;
                                whereValue = null;
                                hasWhere = false;
                                int fromIndex = 2;
                                Table table = new Table("");
                                if (input[1] == "*" && tables.ContainsKey(input[3]))
                                {
                                    table = tables[input[3]];
                                    collums = table.SqlColumns;
                                }
                                else
                                {
                                    fromIndex = input.Count - 1;

                                    List<string> stringCollumns = new List<string>();
                                    for (int i = 1; i < input.Count; i++)
                                    {
                                        if (input[i].ToLower() == "from")
                                        {
                                            if (tables.ContainsKey(input[i + 1]))
                                            {
                                                fromIndex = i;

                                                table = tables[input[i + 1]];
                                                for (int j = 0; j < stringCollumns.Count; j++)
                                                {
                                                    SqlColumn currentColumn = table[stringCollumns[j]];
                                                    if (currentColumn != null)
                                                    {
                                                        collums.Add(currentColumn);
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                        else
                                        {
                                            stringCollumns.Add(input[i]);
                                        }
                                    }
                                }
                                if (fromIndex != input.Count - 1 && table.Name != "")
                                {
                                    if (input.Count >= fromIndex + 5 && input[fromIndex + 2].ToLower() == "where")
                                    {
                                        SqlColumn tempCollumn = table[input[fromIndex + 3]];

                                        if (tempCollumn != null)
                                        {
                                            whereCollum = tempCollumn;
                                            bool hasOpperation = true;
                                            switch (input[fromIndex + 4])
                                            {
                                                case ("="):
                                                    whereOpperation = Opperations.EqualTo;
                                                    break;
                                                case (">"):
                                                    whereOpperation = Opperations.GreaterThan;
                                                    break;
                                                case ("<"):
                                                    whereOpperation = Opperations.LessThan;
                                                    break;
                                                case (">="):
                                                    whereOpperation = Opperations.GreaterThanOrEqualTo;
                                                    break;
                                                case ("<="):
                                                    whereOpperation = Opperations.LessThanOrEqualTo;
                                                    break;
                                                case ("!="):
                                                    whereOpperation = Opperations.NotEqualTo;
                                                    break;
                                                default:
                                                    hasOpperation = false;
                                                    break;
                                            }
                                            if (hasOpperation)
                                            {
                                                object compareValue = ((IConvertible)input[fromIndex + 5]).ToType(whereCollum.VarType, System.Globalization.CultureInfo.InvariantCulture);

                                                if (compareValue is IComparable)
                                                {
                                                    whereValue = (IComparable)compareValue;
                                                    hasWhere = true;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    break;
                                }

                                List<SqlRow> rows;
                                if (hasWhere)
                                {
                                    rows = table.Select(whereCollum.Name, whereOpperation, whereValue);
                                }
                                else
                                {
                                    rows = table.Select();
                                }
                                DisplayRows(table, rows, collums);
                            }
                            break;
                        #endregion
                        #region insert
                        case ("insert"):
                            if (input.Count > 3)
                            {
                                bool correctSyntax = false;
                                int tableIndex = 0;
                                if (input[1].ToLower() == "into" && tables.ContainsKey(input[2]) && input[3].ToLower() == "values")
                                {
                                    tableIndex = 2;
                                    correctSyntax = true;
                                }
                                else if(tables.ContainsKey(input[1]) && input[2].ToLower() == "values")
                                {
                                    tableIndex = 1;
                                    correctSyntax = true;
                                }
                                if (correctSyntax)
                                {
                                    Table table = tables[input[tableIndex]];
                                    if (input.Count >= table.SqlColumns.Count + tableIndex + 2)
                                    {
                                        bool legalValues = false;
                                        List<IComparable> values = new List<IComparable>();
                                        for (int i = tableIndex + 2; i < input.Count; i++)
                                        {
                                            SqlColumn currentCollumn = table.SqlColumns[i - tableIndex - 2];
                                            object compareValue = null;
                                            try
                                            {
                                                compareValue = ((IConvertible)input[i]).ToType(currentCollumn.VarType, System.Globalization.CultureInfo.InvariantCulture);
                                            }
                                            catch
                                            {
                                                break;
                                            }
                                            if (compareValue is IComparable)
                                            {
                                                values.Add((IComparable)compareValue);
                                                legalValues = true;
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                        if (legalValues)
                                        {
                                            table.AddRow(values.ToArray());
                                            Console.WriteLine("1 Row Affected");
                                        }
                                    }
                                }
                            }
                            break;
                        #endregion
                        #region CreateTable
                        case ("create"):
                            if (input.Count >= 5 && input[1].ToLower() == "table")
                            {
                                string tableName = input[2];
                                bool typesParsed = true;
                                List<SqlColumn> newTableCollumns = new List<SqlColumn>();
                                for (int i = 4; i < input.Count; i += 2)
                                {
                                    Type collumnType = Type.GetType(input[i]);
                                    if (collumnType != null)
                                    {
                                        newTableCollumns.Add(new SqlColumn(input[i - 1], collumnType));
                                    }
                                    else
                                    {
                                        typesParsed = false;
                                        break;
                                    }
                                }
                                if (typesParsed && newTableCollumns.Count > 0)
                                {
                                    tables.Add(tableName, new Table(tableName, newTableCollumns.ToArray()));
                                    Console.WriteLine("Table Created");
                                }
                            }
                            break;
                        #endregion
                        #region Delete
                        case ("delete"):
                            whereCollum = null;
                            whereOpperation = Opperations.EqualTo;
                            whereValue = null;
                            hasWhere = false;
                            if (input.Count >= 3 && input[1].ToLower() == "from" && tables.ContainsKey(input[2]))
                            {
                                Table table = tables[input[2]];
                                if (table.Name != "")
                                {
                                    if (input.Count >= 6 && input[3].ToLower() == "where")
                                    {
                                        SqlColumn tempCollumn = table[input[4]];

                                        if (tempCollumn != null)
                                        {
                                            whereCollum = tempCollumn;
                                            bool hasOpperation = true;
                                            switch (input[5])
                                            {
                                                case ("="):
                                                    whereOpperation = Opperations.EqualTo;
                                                    break;
                                                case (">"):
                                                    whereOpperation = Opperations.GreaterThan;
                                                    break;
                                                case ("<"):
                                                    whereOpperation = Opperations.LessThan;
                                                    break;
                                                case (">="):
                                                    whereOpperation = Opperations.GreaterThanOrEqualTo;
                                                    break;
                                                case ("<="):
                                                    whereOpperation = Opperations.LessThanOrEqualTo;
                                                    break;
                                                case ("!="):
                                                    whereOpperation = Opperations.NotEqualTo;
                                                    break;
                                                default:
                                                    hasOpperation = false;
                                                    break;
                                            }
                                            if (hasOpperation)
                                            {
                                                object compareValue = ((IConvertible)input[6]).ToType(whereCollum.VarType, System.Globalization.CultureInfo.InvariantCulture);

                                                if (compareValue is IComparable)
                                                {
                                                    whereValue = (IComparable)compareValue;
                                                    hasWhere = true;
                                                }
                                            }
                                        }
                                    }
                                }
                                int amountOfRows;
                                if (!hasWhere)
                                {
                                    amountOfRows = table.Delete();
                                }
                                else
                                {
                                    amountOfRows = table.Delete(whereCollum.Name, whereOpperation, whereValue);
                                }
                                Console.WriteLine(amountOfRows.ToString() + " Rows " + "Affected");
                            }
                            break;
                        #endregion
                        #region Sort
                        case ("sort"):
                            if (input.Count >= 2 && tables.ContainsKey(input[1]))
                            {
                                Table table = tables[input[1]];
                                table.Sort();
                                Console.WriteLine("Table Sorted");
                            }
                            break;
                        #endregion
                        #region Drop Table
                        case ("drop"):
                            if (input.Count >= 3 && input[1].ToLower() == "table" && tables.ContainsKey(input[2]))
                            {
                                tables.Remove(input[2]);
                                Console.WriteLine("1 Table Droped");
                            }
                            break;
                        #endregion
                        #region Update
                        case ("update"):
                            whereCollum = null;
                            whereOpperation = Opperations.EqualTo;
                            whereValue = null;
                            hasWhere = false;
                            if (input.Count >= 5 && tables.ContainsKey(input[1]))
                            {
                                Table table = tables[input[1]];
                                if (input[2].ToLower() == "set")
                                {
                                    SqlColumn collumnToSet = table[input[3]];
                                    if (collumnToSet != null)
                                    {
                                        object valueObject = ((IConvertible)input[4]).ToType(collumnToSet.VarType, System.Globalization.CultureInfo.InvariantCulture);

                                        if (valueObject is IComparable)
                                        {
                                            IComparable value = (IComparable)valueObject;

                                            //where logic
                                            if (input.Count >= 9 && input[5].ToLower() == "where")
                                            {
                                                SqlColumn tempCollumn = table[input[6]];

                                                if (tempCollumn != null)
                                                {
                                                    whereCollum = tempCollumn;
                                                    bool hasOpperation = true;
                                                    switch (input[7])
                                                    {
                                                        case ("="):
                                                            whereOpperation = Opperations.EqualTo;
                                                            break;
                                                        case (">"):
                                                            whereOpperation = Opperations.GreaterThan;
                                                            break;
                                                        case ("<"):
                                                            whereOpperation = Opperations.LessThan;
                                                            break;
                                                        case (">="):
                                                            whereOpperation = Opperations.GreaterThanOrEqualTo;
                                                            break;
                                                        case ("<="):
                                                            whereOpperation = Opperations.LessThanOrEqualTo;
                                                            break;
                                                        case ("!="):
                                                            whereOpperation = Opperations.NotEqualTo;
                                                            break;
                                                        default:
                                                            hasOpperation = false;
                                                            break;
                                                    }
                                                    if (hasOpperation)
                                                    {
                                                        object compareValue = ((IConvertible)input[8]).ToType(whereCollum.VarType, System.Globalization.CultureInfo.InvariantCulture);

                                                        if (compareValue is IComparable)
                                                        {
                                                            whereValue = (IComparable)compareValue;
                                                            hasWhere = true;
                                                        }
                                                    }
                                                }
                                            }

                                            int amountOfUpdatedRows;
                                            if (!hasWhere)
                                            {
                                                amountOfUpdatedRows = table.Update(collumnToSet, value);
                                            }
                                            else
                                            {
                                                amountOfUpdatedRows = table.Update(collumnToSet, value, whereCollum.Name, whereOpperation, whereValue);
                                            }
                                            Console.WriteLine(amountOfUpdatedRows + " Rows Updated");
                                        }
                                    }
                                }
                            }
                            break;
                            #endregion
                            #region oldStuff
                            //case ("AddStudent"):
                            //    int age;
                            //    if (input.Length > 4 && int.TryParse(input[3], out age))
                            //    {
                            //        AddStudent(input[1], input[2], age, input[4]);
                            //    }
                            //    break;
                            //case ("DeleteStudent"):
                            //    int id;
                            //    if (input.Length > 1 && int.TryParse(input[1], out id))
                            //    {
                            //        DeleteStudent(id);
                            //    }
                            //    break;
                            //case ("ViewStudents"):
                            //    ViewStudents();
                            //    break;
                            //case ("BreakTree"):
                            //    for (double i = 0; i < 50000; i++)
                            //    {
                            //        tree.AddNode(new Student(currentId, "Josiah", "Ross", 15, "stuff"));
                            //        currentId++;
                            //        if (i % 1000 == 0)
                            //        {
                            //            Console.WriteLine(currentId);
                            //        }
                            //    }
                            //    break;
                            //case ("Sort"):

                            //    //sw.Restart();
                            //    //for (int i = 0; i < 2000; i++)
                            //    //{
                            //    //    tree.AddNode(new Student(currentId, "bob", "smith", 5, "bob@bob.com"));
                            //    //    currentId++;
                            //    //}
                            //    List<Student> sortedList = sort(tree.GetNodes());
                            //    List<Student> balanceList = balanceSort(sortedList, new List<Student>());


                            //    tree.BaseNode = null;
                            //    for (int i = 0; i < balanceList.Count; i++)
                            //    {
                            //        tree.AddNode(balanceList[i]);
                            //    }

                            //    //Console.WriteLine(sw.ElapsedMilliseconds);
                            //    break;

                            //case ("TestSelect"):

                            //    for (int i = 0; i < 20; i++)
                            //    {
                            //        testTable.AddRow("Josiah");
                            //    }

                            //    testTable.Sort();

                            //    List<SqlRow> rows = testTable.Select();

                            //    for (int i = 0; i < rows.Count; i++)
                            //    {
                            //        Console.Write("ID: " + rows[i].Id.ToString());

                            //        for (int j = 0; j < rows[i].Cells.Count; j++)
                            //        {
                            //            Console.Write(", " + rows[i].OwningTable.SqlColumns[j].Name + ": " + rows[i].Cells[j].Value.ToString());
                            //        }
                            //        Console.WriteLine();
                            //    }
                            //    break;
                            #endregion
                    }
                }
            } while (input[0] != "Exit");
            SaveXML();
        }
        #region oldStuff
        //    public static void AddStudent(string firstName, string lastName, int age, string email)
        //    {
        //        tree.AddNode(new Student(currentId, firstName, lastName, age, email));
        //        Console.WriteLine("Id: " + currentId);
        //        currentId++;
        //    }
        //    public static void DeleteStudent(int id)
        //    {
        //        tree.UserDelete(new Student(id));
        //    }
        //    public static void ViewStudents()
        //    {
        //        List<Student> students = tree.GetNodes();
        //        foreach (Student student in students)
        //        {
        //            Console.WriteLine("Id: " + student.Id + " Name: " + student.FirstName + " " + student.LastName + " Age: " + student.Age + " Email: " + student.Email);
        //        }
        //    }
        //    public static List<Student> sort(List<Student> input)
        //    {
        //        List<Student> returnList = new List<Student>();
        //        while (input.Count > 0)
        //        {
        //            returnList = insert(input[input.Count - 1], returnList);
        //            input.Remove(input[input.Count - 1]);
        //        }
        //        return returnList;
        //    }

        //    public static List<Student> balanceSort(List<Student> sortedList, List<Student> currentList)
        //    {
        //        if (sortedList.Count > 0)
        //        {
        //            List<List<Student>> nextLists = new List<List<Student>>();
        //            if (sortedList.Count % 2 == 0)
        //            {
        //                currentList.Add(sortedList[sortedList.Count / 2 - 1]);
        //                sortedList.RemoveAt(sortedList.Count / 2 - 1);

        //                nextLists.Add(sortedList.Take((sortedList.Count + 1) / 2 - 1).ToList());
        //                nextLists.Add(sortedList.Skip((sortedList.Count + 1) / 2 - 1).ToList());
        //            }
        //            else
        //            {
        //                currentList.Add(sortedList[(sortedList.Count + 1) / 2 - 1]);
        //                sortedList.RemoveAt((sortedList.Count + 1) / 2 - 1);

        //                nextLists.Add(sortedList.Take(sortedList.Count / 2 - 1).ToList());
        //                nextLists.Add(sortedList.Skip(sortedList.Count / 2 - 1).ToList());
        //            }
        //            foreach (List<Student> list in nextLists)
        //            {
        //                currentList = (balanceSort(list, currentList));
        //            }
        //        }
        //        return currentList;
        //    }



        //    public static List<Student> insert(Student number, List<Student> input)
        //    {
        //        List<Student> returnList = input;
        //        bool added = false;
        //        for (int i = 0; i < returnList.Count; i++)
        //        {
        //            if (number.Id <= returnList[i].Id)
        //            {
        //                added = true;
        //                Student previousNum = number;
        //                for (int j = i; j < returnList.Count; j++)
        //                {
        //                    Student temp = returnList[j];
        //                    returnList[j] = previousNum;
        //                    previousNum = temp;
        //                }
        //                returnList.Add(previousNum);
        //                break;
        //            }
        //        }
        //        if (!added)
        //        {
        //            returnList.Add(number);
        //        }
        //        return returnList;
        //    }
        #endregion
        public static List<string> Split(string input)
        {
            bool inParentheses = false;
            bool inQuotes = false;
            List<string> returnList = new List<string>();
            int lastCut = 0;
            string currentValue = "";
            string newInput;
            for (int i = 0; i < input.Length; i++)
            {
                if (!inQuotes)
                {
                    if (!inParentheses)
                    {
                        if (input[i] == ' ')
                        {
                            newInput = input.Substring(lastCut, i - lastCut);
                            if (newInput.Trim() != "")
                            {
                                returnList.Add(newInput);
                            }
                            lastCut = i + 1;
                        }
                        if (input[i] == '(')
                        {
                            newInput = input.Substring(lastCut, i - lastCut);
                            if (newInput.Trim() != "")
                            {
                                returnList.Add(newInput);
                            }
                            lastCut = i + 1;
                            currentValue = "";
                            inParentheses = true;
                        }
                        if (input[i] == '"')
                        {
                            inQuotes = true;
                            currentValue = "";
                        }
                    }
                    else
                    {

                        if (input[i] != ' ')
                        {

                            if (input[i] == '"')
                            {
                                inQuotes = true;
                                currentValue = "";
                            }
                            else
                            {
                                if (input[i] == ')')
                                {
                                    lastCut = i + 1;
                                    if (currentValue.Trim() != "")
                                    {
                                        returnList.Add(currentValue);
                                    }
                                    currentValue = "";
                                    inParentheses = false;
                                }
                                else
                                {
                                    if (input[i] == ',')
                                    {
                                        if (currentValue.Trim() != "")
                                        {
                                            returnList.Add(currentValue);
                                        }
                                        currentValue = "";
                                    }
                                    else
                                    {
                                        currentValue += input[i];
                                    }
                                }
                            }

                        }

                    }
                }
                else
                {
                    if (input[i] == '"')
                    {
                        lastCut = i + 1;
                        inQuotes = false;

                        if (currentValue.Trim() != "")
                        {
                            returnList.Add(currentValue);
                        }
                        currentValue = "";
                    }
                    else
                    {
                        currentValue += input[i];
                    }
                }
            }
            newInput = input.Substring(lastCut, input.Length - lastCut);
            if (newInput != "")
            {
                returnList.Add(newInput);
            }
            return returnList;
        }

        public static void LoadXML()
        {
            foreach (XElement tableElement in xdoc.Root.Elements())
            {
                Table table = new Table(tableElement);
                tables.Add(table.Name, table);
            }
            xdoc.Root.RemoveAll();
        }

        public static void SaveXML()
        {
            List<KeyValuePair<string, Table>> tableList = tables.ToList();
            for (int i = 0; i < tableList.Count; i++)
            {
                Table table = tableList[i].Value;
                XElement tableElement = new XElement("Table");
                tableElement.Add(new XAttribute("name", tableList[i].Key));
                tableElement.Add(new XAttribute("currentID", table.CurrentID));

                XElement collumnsElement = new XElement("Collumns");
                foreach (SqlColumn collumn in table.SqlColumns)
                {
                    XElement currentCollumn = new XElement("Collumn");
                    currentCollumn.Add(new XAttribute(collumn.Name, collumn.VarType));
                    collumnsElement.Add(currentCollumn);
                }
                tableElement.Add(collumnsElement);

                XElement binaryTree = new XElement("BinaryTree", new XAttribute("BalanceValue", table.Tree.BalanceValue));
                if (table.Tree.BaseNode != null)
                {
                    binaryTree = FillXMLBinaryTree(table, table.Tree.BaseNode, binaryTree);
                }

                tableElement.Add(binaryTree);
                xdoc.Root.Add(tableElement);
            }
            xdoc.Save(xmlFile);
        }
        public static XElement FillXMLBinaryTree(Table table, Node<SqlRow> node, XElement currentElement)
        {
            XElement newNode = new XElement("Node", new XAttribute("id", node.Value.Id));
            XElement cells = new XElement("Cells");
            for (int i = 0; i < node.Value.Cells.Count; i++)
            {
                XElement cell = new XElement("Cell", new XAttribute("Value", node.Value.Cells[i].Value));
                cells.Add(cell);
            }
            XElement left = new XElement("Left");
            XElement right = new XElement("Right");
            if (node.Left != null)
            {
                left = FillXMLBinaryTree(table, node.Left, left);
            }
            if (node.Right != null)
            {
                right = FillXMLBinaryTree(table, node.Right, right);
            }
            newNode.Add(cells);
            newNode.Add(left);
            newNode.Add(right);
            currentElement.Add(newNode);
            return currentElement;
        }

        public static void DisplayRows(Table table, List<SqlRow> rows, List<SqlColumn> collumns)
        {
            int startY = Console.CursorTop;
            int currentY = startY;
            int currentX = 0;
            char cornerChar = '+';
            char verticalChar = '|';
            char horizontalChar = '-';
            int currentYBefore;
            for (int x = 0; x < table.SqlColumns.Count; x++)
            {
                if (collumns.Contains(table.SqlColumns[x]))
                {
                    int CollumnWidth = 0;
                    Console.SetCursorPosition(currentX, currentY);
                    Console.Write(cornerChar);
                    currentY++;
                    Console.SetCursorPosition(currentX, currentY);
                    Console.Write(verticalChar + " " + table.SqlColumns[x].Name + " ");
                    CollumnWidth = table.SqlColumns[x].Name.Length;
                    currentY++;
                    Console.SetCursorPosition(currentX, currentY);
                    Console.Write(cornerChar);
                    currentY++;
                    currentYBefore = currentY;
                    for (int y = currentYBefore; y < currentYBefore + rows.Count; y++)
                    {
                        Console.SetCursorPosition(currentX, currentY);
                        Console.Write(verticalChar + " " + rows[y - currentYBefore].Cells[x].Value.ToString() + " ");
                        if (CollumnWidth < rows[y - currentYBefore].Cells[x].Value.ToString().Length)
                        {
                            CollumnWidth = rows[y - currentYBefore].Cells[x].Value.ToString().Length;
                        }
                        currentY++;
                    }
                    Console.SetCursorPosition(currentX, currentY);
                    Console.Write(cornerChar + RepeatChar(horizontalChar, CollumnWidth + 2));

                    Console.SetCursorPosition(currentX, currentYBefore - 1);
                    Console.Write(cornerChar + RepeatChar(horizontalChar, CollumnWidth + 2));

                    Console.SetCursorPosition(currentX, startY);
                    Console.Write(cornerChar + RepeatChar(horizontalChar, CollumnWidth + 2));

                    currentX += CollumnWidth + 3;
                    currentY = startY;
                }
            }
            Console.SetCursorPosition(currentX, currentY);
            Console.Write(cornerChar);
            currentY++;
            Console.SetCursorPosition(currentX, currentY);
            Console.Write(verticalChar);

            currentY++;
            Console.SetCursorPosition(currentX, currentY);
            Console.Write(cornerChar);
            currentY++;
            currentYBefore = currentY;
            for (int y = currentYBefore; y < currentYBefore + rows.Count; y++)
            {
                Console.SetCursorPosition(currentX, currentY);
                Console.Write(verticalChar);
                currentY++;
            }
            Console.SetCursorPosition(currentX, currentY);
            Console.Write(cornerChar);

            Console.SetCursorPosition(0, currentY + 1);
        }

        public static string RepeatChar(char letter, int amountOfTimes)
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
