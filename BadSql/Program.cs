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
        static BinaryTree<Student> tree = new BinaryTree<Student>(2);
        static string xmlFile = "SqlData.xml";
        static XDocument xdoc = XDocument.Load(xmlFile);
        static Dictionary<string, Table> tables = new Dictionary<string, Table>();
        static Dictionary<string, Range> keywords = new Dictionary<string, Range>();

        public static void Main(string[] args)
        {
            keywords.Add("select", new Range(0, int.MaxValue, false, true));
            keywords.Add("from", new Range(1, 2, false, false));
            keywords.Add("where", new Range(3, 4, false, false));
            keywords.Add("insert", new Range(1, 2, false, false));
            keywords.Add("into", new Range(1, 2, false, false));
            keywords.Add("values", new Range(1, int.MaxValue, true, true));
            keywords.Add("create", new Range(0, 1, false, false));
            keywords.Add("table", new Range(1, int.MaxValue, true, false));
            keywords.Add("delete", new Range(0, 1, false, false));
            keywords.Add("sort", new Range(1, 2, false, false));
            keywords.Add("drop", new Range(0, 1, false, false));
            keywords.Add("update", new Range(1, 2, false, false));
            keywords.Add("set", new Range(1, int.MaxValue, false, true));

            LoadXML();
            Random random = new Random();
            List<SqlKeyWord> input;

            SqlColumn whereCollum;
            Opperations whereOpperation;
            IComparable whereValue;
            bool hasWhere;
            Table table;
            string originalInput;
            tables.Add("Test", new Table("Test", new SqlColumn("ID", typeof(int)), new SqlColumn("Stuff", typeof(string))));
            do
            {
                //input = Console.ReadLine().Split(' ');
                string errors;
                originalInput = Console.ReadLine();
                //input = Split(originalInput, out errors);
                input = Split2(originalInput, out errors);
                if (input.Count > 0 && errors == "")
                {
                    switch (input[0].Input.ToLower())
                    {
                        #region Select
                        case ("select"):
                            if (input.Count >= 2)
                            {
                                List<SqlColumn> collums = new List<SqlColumn>();
                                whereCollum = null;
                                whereOpperation = Opperations.EqualTo;
                                whereValue = null;
                                bool whereWorked = false;
                                table = null;
                                SqlKeyWord select = input[0];
                                SqlKeyWord from = input[1];
                                if (from.Input.ToLower() != "from")
                                {
                                    errors += "Incorect Use Of Select, ";
                                    break;
                                }
                                if (select.Children.Count == 1 && isSqlKeyWord(KeyWordTypes.CommaGroup, select.Children[0]) && ((SqlKeyWord)select.Children[0]).Children.Count == 1 && ((SqlKeyWord)select.Children[0]).Children[0].Input == "*" && tables.ContainsKey(from.Children[0].Input))
                                {
                                    table = tables[from.Children[0].Input];
                                    collums = table.SqlColumns;
                                }
                                else if (tables.ContainsKey(from.Children[0].Input))

                                {
                                    table = tables[from.Children[0].Input];
                                    bool parseingError = false;
                                    for (int i = 0; i < select.Children.Count; i++)
                                    {
                                        if (isSqlKeyWord(KeyWordTypes.CommaGroup, select.Children[i]))
                                        {
                                            SqlKeyWord commaGroup = (SqlKeyWord)select.Children[i];
                                            if (commaGroup.Children.Count == 1)
                                            {
                                                SqlColumn possibleCollumn = table[commaGroup.Children[0].Input];
                                                if (possibleCollumn != null)
                                                {
                                                    collums.Add(possibleCollumn);
                                                }
                                                else
                                                {
                                                    errors += table.Name + " Table Doesn't Have the Collumn: " + commaGroup.Children[0].Input + ", ";
                                                    parseingError = true;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                errors += "Incorect Collumn Syntax, ";
                                                parseingError = true;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            errors += "Collumn Parseing Error, ";
                                            parseingError = true;
                                            break;
                                        }
                                    }
                                    if (parseingError)
                                    {
                                        break;
                                    }

                                }
                                else
                                {
                                    errors += "Table Doesn't Exist, ";
                                    break;
                                }
                                if (input.Count >= 3)
                                {
                                    SqlKeyWord whereKeyWord = input[2];
                                    whereWorked = GetWhereInfo(whereKeyWord, table, out whereCollum, out whereOpperation, out whereValue);
                                    if (!whereWorked)
                                    {
                                        errors += "Incorect Where Syntax, ";
                                        break;
                                    }
                                }

                                List<SqlRow> rows;
                                if (whereWorked)
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
                        //fix with new parseing
                        #region Insert
                        case ("insert"):
                            if (input.Count >= 2)
                            {
                                SqlKeyWord Values = null;
                                
                                table = null;
                                if (input.Count >= 3 && input[1].Input.ToLower() == "into" && input[2].Input.ToLower() == "values")
                                {
                                    if (tables.ContainsKey(input[1].Children[0].Input.ToLower()))
                                    {
                                        table = tables[input[1].Children[0].Input.ToLower()];
                                        Values = input[2];
                                    }
                                    else
                                    {
                                        errors += "Table Doesn't Exist, ";
                                        break;
                                    }
                                }
                                else if (input[1].Input.ToLower() == "values")
                                {
                                    Values = input[1];
                                }
                                else
                                {
                                    errors += "Incorect Syntax Near Insert, ";
                                    break;
                                }

                                bool correctSyntax = false;
                                int tableIndex = 0;
                                if (input[1].Input.ToLower() == "into" && tables.ContainsKey(input[2].Input) && input[3].Input.ToLower() == "values")
                                {
                                    tableIndex = 2;
                                    correctSyntax = true;
                                }
                                else if (tables.ContainsKey(input[1].Input) && input[2].Input.ToLower() == "values")
                                {
                                    tableIndex = 1;
                                    correctSyntax = true;
                                }
                                if (correctSyntax)
                                {
                                    table = tables[input[tableIndex].Input];
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
                                                compareValue = ((IConvertible)input[i].Input).ToType(currentCollumn.VarType, System.Globalization.CultureInfo.InvariantCulture);
                                            }
                                            catch (Exception e)
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
                        //fix with new parseing
                        #region CreateTable
                        case ("create"):
                            if (input.Count >= 5 && input[1].Input.ToLower() == "table")
                            {
                                string tableName = input[2].Input;
                                bool typesParsed = true;
                                List<SqlColumn> newTableCollumns = new List<SqlColumn>();
                                for (int i = 4; i < input.Count; i += 2)
                                {
                                    Type collumnType = Type.GetType(input[i].Input);
                                    if (collumnType != null)
                                    {
                                        newTableCollumns.Add(new SqlColumn(input[i - 1].Input, collumnType));
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
                        //fix with new parseing
                        #region Delete
                        case ("delete"):
                            whereCollum = null;
                            whereOpperation = Opperations.EqualTo;
                            whereValue = null;
                            hasWhere = false;
                            if (input.Count >= 3 && input[1].Input.ToLower() == "from" && tables.ContainsKey(input[2].Input))
                            {
                                table = tables[input[2].Input];
                                if (table.Name != "")
                                {
                                    if (input.Count >= 6 && input[3].Input.ToLower() == "where")
                                    {
                                        SqlColumn tempCollumn = table[input[4].Input];

                                        if (tempCollumn != null)
                                        {
                                            whereCollum = tempCollumn;
                                            bool hasOpperation = true;
                                            switch (input[5].Input)
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
                                                object compareValue = ((IConvertible)input[6].Input).ToType(whereCollum.VarType, System.Globalization.CultureInfo.InvariantCulture);

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
                            if (tables.ContainsKey(input[0].Children[0].Input))
                            {
                                table = tables[input[0].Children[0].Input];
                                table.Sort();
                                Console.WriteLine("Table Sorted");
                            }
                            else
                            {
                                errors += "Table Doesn't Exist, ";
                            }
                            break;
                        #endregion

                        #region Drop Table
                        case ("drop"):
                            if (input.Count >= 2)
                            {
                                if (input[1].Input.ToLower() == "table" && input[1].Children.Count == 1)
                                {
                                    if (tables.ContainsKey(input[1].Children[0].Input))
                                    {
                                        tables.Remove(input[1].Children[0].Input);
                                        Console.WriteLine("1 Table Droped");
                                    }
                                    else
                                    {
                                        errors += "Table Doesn't Exist, ";
                                    }
                                }
                                else
                                {
                                    errors += "Incorrect Syntax Near Table, ";
                                }
                            }
                            break;
                        #endregion
                        //fix with new parseing
                        #region Update
                        case ("update"):
                            whereCollum = null;
                            whereOpperation = Opperations.EqualTo;
                            whereValue = null;
                            hasWhere = false;
                            if (input.Count >= 5 && tables.ContainsKey(input[1].Input))
                            {
                                table = tables[input[1].Input];
                                if (input[2].Input.ToLower() == "set")
                                {
                                    SqlColumn collumnToSet = table[input[3].Input];
                                    if (collumnToSet != null)
                                    {
                                        object valueObject = ((IConvertible)input[4].Input).ToType(collumnToSet.VarType, System.Globalization.CultureInfo.InvariantCulture);

                                        if (valueObject is IComparable)
                                        {
                                            IComparable value = (IComparable)valueObject;

                                            //where logic
                                            if (input.Count >= 9 && input[5].Input.ToLower() == "where")
                                            {
                                                SqlColumn tempCollumn = table[input[6].Input];

                                                if (tempCollumn != null)
                                                {
                                                    whereCollum = tempCollumn;
                                                    bool hasOpperation = true;
                                                    switch (input[7].Input)
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
                                                        object compareValue = ((IConvertible)input[8].Input).ToType(whereCollum.VarType, System.Globalization.CultureInfo.InvariantCulture);

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
                        #region Default
                        default:
                            errors += input[0].Input + " is Not a Starting Command, ";
                            break;
                            #endregion
                    }
                }
                if (errors.Length > 2)
                {
                    errors = errors.Remove(errors.Length - 2, 2);
                    Console.WriteLine(errors);
                }
            } while (originalInput.ToLower() != "exit");
            SaveXML();
        }
        public static bool GetWhereInfo(SqlKeyWord WhereKeyWord, Table table, out SqlColumn whereCollum, out Opperations whereOpperation, out IComparable whereValue)
        {
            whereCollum = null;
            whereOpperation = Opperations.EqualTo;
            whereValue = null;
            bool whereWorks = false;
            if (WhereKeyWord.Input.ToLower() == "where" && WhereKeyWord.Children.Count >= 3)
            {
                SqlColumn tempCollumn = table[WhereKeyWord.Children[0].Input];

                if (tempCollumn != null)
                {
                    whereCollum = tempCollumn;
                    bool hasOpperation = true;
                    switch (WhereKeyWord.Children[1].Input)
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
                        object compareValue = ((IConvertible)WhereKeyWord.Children[2].Input).ToType(whereCollum.VarType, System.Globalization.CultureInfo.InvariantCulture);

                        if (compareValue is IComparable)
                        {
                            whereValue = (IComparable)compareValue;
                            whereWorks = true;
                        }
                    }
                }
            }

            return whereWorks;
        }

        public static bool isSqlKeyWord(KeyWordTypes keyWordType, ISqlInput possibleKeyWord)
        {
            return possibleKeyWord.GetType() == typeof(SqlKeyWord) && keyWordType == ((SqlKeyWord)possibleKeyWord).KeyWordType;
        }

        //public static List<SqlKeyWord> Split(string input, out string errors)
        //{
        //    List<SqlKeyWord> returnList = new List<SqlKeyWord>();
        //    errors = "";
        //    int startOfWord = 0;
        //    int amountOfParentheses = 0;
        //    for (int i = 0; i < input.Length; i++)
        //    {
        //        if (input[i] == ' ')
        //        {
        //            string newSqlInput = input.Substring(startOfWord, i - startOfWord);
        //            if (keywords.Keys.Contains(newSqlInput.ToLower()))
        //            {
        //                SqlKeyWord newKeyWord = new SqlKeyWord(newSqlInput, null, keywords[newSqlInput.ToLower()]);
        //                SqlKeyWord nextKeyWord;
        //                newKeyWord.Children = GetKeyWordChildren(newKeyWord, input, i, false, amountOfParentheses, out amountOfParentheses, out i, out errors, out nextKeyWord);
        //                returnList.Add(newKeyWord);
        //                returnList = GetBottomKeyWords(nextKeyWord, input, i, amountOfParentheses, returnList, errors, out i, out amountOfParentheses, out errors);
        //                startOfWord = i;
        //            }
        //        }
        //        else if (input[i] == '(')
        //        {
        //            string newSqlInput = input.Substring(startOfWord, i - startOfWord);
        //            if (keywords.Keys.Contains(newSqlInput.ToLower()))
        //            {
        //                SqlKeyWord newKeyWord = new SqlKeyWord(newSqlInput, null, keywords[newSqlInput.ToLower()]);
        //                SqlKeyWord nextKeyWord;
        //                newKeyWord.Children = GetKeyWordChildren(newKeyWord, input, i, false, amountOfParentheses, out amountOfParentheses, out i, out errors, out nextKeyWord);
        //                returnList.Add(newKeyWord);
        //                returnList = GetBottomKeyWords(nextKeyWord, input, i, amountOfParentheses, returnList, errors, out i, out amountOfParentheses, out errors);
        //                startOfWord = i;
        //            }
        //        }
        //        //else if (input[i] == '"')
        //        //{
        //        //}
        //    }

        //    if (amountOfParentheses != 0)
        //    {
        //        errors += "Incorect Parentheses Syntax, ";
        //    }
        //    return returnList;
        //}

        //public static List<SqlKeyWord> GetBottomKeyWords(SqlKeyWord nextKeyWord, string input, int currentIndex, int amountOfParenthesesIn, List<SqlKeyWord> currentList, string errorsIn, out int newIndex, out int amountOfParentheses, out string errors)
        //{
        //    newIndex = currentIndex;
        //    amountOfParentheses = amountOfParenthesesIn;
        //    errors = errorsIn;
        //    if (nextKeyWord != null)
        //    {
        //        SqlKeyWord newNextKeyWord;
        //        nextKeyWord.Children = GetKeyWordChildren(nextKeyWord, input, currentIndex, false, amountOfParenthesesIn, out amountOfParentheses, out newIndex, out errors, out newNextKeyWord);
        //        currentList.Add(nextKeyWord);

        //        if (newNextKeyWord != null)
        //        {
        //            currentList = GetBottomKeyWords(newNextKeyWord, input, newIndex, amountOfParentheses, currentList, errors, out newIndex, out amountOfParentheses, out errors);
        //        }
        //    }
        //    return currentList;
        //}

        //public static List<ISqlInput> GetKeyWordChildren(SqlKeyWord keyWord, string input, int currentIndex, bool inParenthesesGroup, int amountOfParenthesesIn, out int amountOfParentheses, out int newIndex, out string errors, out SqlKeyWord nextKeyWord)
        //{
        //    List<ISqlInput> returnList = new List<ISqlInput>();
        //    bool inQuotes = false;
        //    bool justInQuotes = false;
        //    amountOfParentheses = amountOfParenthesesIn;
        //    errors = "";
        //    nextKeyWord = null;
        //    string currentWord = "";
        //    newIndex = currentIndex;
        //    bool justHadComma = false;
        //    string currentSpaceWord = "";
        //    string lastSpaceWord = "";
        //    bool addToCurrentSpaceWord = false;
        //    for (int i = currentIndex; i < input.Length; i++)
        //    {
        //        if (!inQuotes)
        //        {
        //            if (input[i] != ' ')
        //            {

        //                if (input[i] == '"')
        //                {
        //                    if (currentWord == "")
        //                    {
        //                        inQuotes = true;
        //                        justInQuotes = true;
        //                    }
        //                    else
        //                    {
        //                        errors += "Incorect Quote Syntax, ";
        //                        newIndex = i + 1;
        //                        break;
        //                    }
        //                }
        //                else if (input[i] == ',' || input[i] == ')' || input[i] == '(' || input[i] == '=' || input[i] == '!' || input[i] == '<' || input[i] == '>')
        //                {
        //                    #region addCurrentWord
        //                    //bool isOpenParen = false;

        //                    //if (input[i] == '(')
        //                    //{
        //                    //    amountOfParentheses++;
        //                    //    isOpenParen = true;
        //                    //}
        //                    //if (input[i] == ')')
        //                    //{
        //                    //    amountOfParentheses--;
        //                    //}

        //                    //if (currentWord != "")
        //                    //{
        //                    //    justHadComma = true;
        //                    //    if (returnList.Count + 1 >= keyWord.ChildrenAmountRange.Max)
        //                    //    {
        //                    //        errors += "Incorect Use of " + keyWord.Input + ", ";
        //                    //        newIndex = i + 1;
        //                    //        break;
        //                    //    }
        //                    //    else
        //                    //    {
        //                    //        if (!justInQuotes && keywords.ContainsKey(currentWord.ToLower()))
        //                    //        {
        //                    //            newKeyWord = new SqlKeyWord(currentWord, keyWord, keywords[currentWord.ToLower()]);

        //                    //            if (!isOpenParen)
        //                    //            {
        //                    //                newKeyWord.Children = GetKeyWordChildren(newKeyWord, input, i + 1, false, amountOfParentheses, out amountOfParentheses, out i, out errors);

        //                    //                returnList.Add(newKeyWord);
        //                    //                if (i >= input.Length)
        //                    //                {
        //                    //                    newIndex = i;
        //                    //                    break;
        //                    //                }
        //                    //            }
        //                    //        }
        //                    //        else
        //                    //        {
        //                    //            returnList.Add(new SqlCustomInput(currentWord, keyWord, justInQuotes));
        //                    //            justInQuotes = false;
        //                    //        }
        //                    //    }

        //                    //    if (input[i] == ')')
        //                    //    {
        //                    //        if (inParenthesesGroup)
        //                    //        {
        //                    //            newIndex = i + 1;
        //                    //            break;
        //                    //        }
        //                    //    }
        //                    //    currentWord = "";
        //                    //}


        //                    //if (isOpenParen)
        //                    //{
        //                    //    if (newKeyWord == null)
        //                    //    {
        //                    //        SqlKeyWord openParenKeyWord = new SqlKeyWord("ParenthesesGroup", keyWord, new Range(0, int.MaxValue));
        //                    //        openParenKeyWord.Children = GetKeyWordChildren(openParenKeyWord, input, i + 1, true, amountOfParentheses, out amountOfParentheses, out i, out errors);
        //                    //        returnList.Add(openParenKeyWord);
        //                    //        if (i >= input.Length)
        //                    //        {
        //                    //            newIndex = i;
        //                    //            break;
        //                    //        }
        //                    //    }
        //                    //    else
        //                    //    {
        //                    //        SqlKeyWord openParenKeyWord = new SqlKeyWord("ParenthesesGroup", newKeyWord, new Range(0, int.MaxValue));
        //                    //        openParenKeyWord.Children = GetKeyWordChildren(openParenKeyWord, input, i + 1, true, amountOfParentheses, out amountOfParentheses, out i, out errors);
        //                    //        newKeyWord.Children.Add(openParenKeyWord);
        //                    //        if (i >= input.Length)
        //                    //        {
        //                    //            returnList.Add(newKeyWord);
        //                    //            newIndex = i;
        //                    //            break;
        //                    //        }
        //                    //        newKeyWord.Children.AddRange(GetKeyWordChildren(newKeyWord, input, i + 1, false, amountOfParentheses, out amountOfParentheses, out i, out errors));

        //                    //        returnList.Add(newKeyWord);
        //                    //        if (i >= input.Length)
        //                    //        {
        //                    //            newIndex = i;
        //                    //            break;
        //                    //        }
        //                    //    }
        //                    //}
        //                    #endregion

        //                    if (input[i] == '=' || input[i] == '!' || input[i] == '<' || input[i] == '>')
        //                    {
        //                        justHadComma = false;
        //                    }

        //                    bool currentInputParentheses = false;
        //                    if (input[i] == '(')
        //                    {
        //                        amountOfParentheses++;
        //                        currentInputParentheses = true;
        //                    }
        //                    if (input[i] == ')')
        //                    {
        //                        amountOfParentheses--;
        //                        currentInputParentheses = true;
        //                    }
        //                    if (currentWord.Trim() != "")
        //                    {
        //                        if (keywords.ContainsKey(currentWord.ToLower()) && !justInQuotes)
        //                        {
        //                            if ((currentInputParentheses && keyWord.ChildrenAmountRange.CanHaveParentheses) || !currentInputParentheses)
        //                            {
        //                                nextKeyWord = new SqlKeyWord(currentWord, keyWord.Parent, keywords[currentWord.ToLower()]);
        //                                newIndex = i + 1;
        //                                break;
        //                            }
        //                            else
        //                            {
        //                                errors += "Incorect Use of " + keyWord.Input + ", ";
        //                                newIndex = i + 1;
        //                                break;
        //                            }
        //                        }
        //                        else if (addToCurrentSpaceWord && currentSpaceWord.Trim() != "" && keywords.ContainsKey(currentSpaceWord.ToLower()))
        //                        {
        //                            SqlKeyWord possibleNextKeyWord = new SqlKeyWord(currentSpaceWord, keyWord.Parent, keywords[currentSpaceWord.ToLower()]);
        //                            currentSpaceWord = "";
        //                            addToCurrentSpaceWord = false;
        //                            if ((currentInputParentheses && possibleNextKeyWord.ChildrenAmountRange.CanHaveParentheses) || !currentInputParentheses)
        //                            {
        //                                if (lastSpaceWord.Trim() != "")
        //                                {
        //                                    returnList.Add(new SqlCustomInput(lastSpaceWord, keyWord, justInQuotes));
        //                                    lastSpaceWord = "";
        //                                }
        //                                nextKeyWord = possibleNextKeyWord;
        //                                newIndex = i + 1;
        //                                break;
        //                            }
        //                            else
        //                            {
        //                                errors += "Incorect Use of " + possibleNextKeyWord.Input + ", ";
        //                                newIndex = i + 1;
        //                                break;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (returnList.Count + 1 + keyWord.Children.Count >= keyWord.ChildrenAmountRange.Max)
        //                            {
        //                                errors += "Incorect Use of " + keyWord.Input + ", ";
        //                                newIndex = i + 1;
        //                                break;
        //                            }
        //                            else
        //                            {
        //                                returnList.Add(new SqlCustomInput(currentWord, keyWord, justInQuotes));
        //                                currentWord = "";
        //                                currentSpaceWord = "";
        //                                addToCurrentSpaceWord = false;
        //                            }

        //                        }

        //                        justInQuotes = false;
        //                    }
        //                    justHadComma = true;
        //                }
        //            }
        //            else if (input[i] == ' ')
        //            {
        //                if (addToCurrentSpaceWord && currentSpaceWord.Trim() != "" && keywords.ContainsKey(currentSpaceWord.ToLower()))
        //                {
        //                    SqlKeyWord possibleNextKeyWord = new SqlKeyWord(currentSpaceWord, keyWord.Parent, keywords[currentSpaceWord.ToLower()]);
        //                    if (lastSpaceWord.Trim() != "")
        //                    {
        //                        returnList.Add(new SqlCustomInput(lastSpaceWord, keyWord, justInQuotes));
        //                    }
        //                    nextKeyWord = possibleNextKeyWord;
        //                    newIndex = i + 1;
        //                    break;
        //                }
        //                lastSpaceWord = currentWord;
        //                currentSpaceWord = "";
        //                addToCurrentSpaceWord = true;
        //            }
        //            if (input[i] != ' ' && input[i] != ',' && input[i] != ')' && input[i] != '(')
        //            {
        //                if (addToCurrentSpaceWord)
        //                {
        //                    currentSpaceWord += input[i];
        //                }
        //                currentWord += input[i];
        //            }
        //        }

        //        else
        //        {
        //            if (input[i] == '"')
        //            {
        //                inQuotes = false;
        //            }
        //            else
        //            {
        //                currentWord += input[i];
        //            }
        //        }

        //        if (i + 1 >= input.Length)
        //        {
        //            newIndex = i + 1;
        //            if (returnList.Count + 1 >= keyWord.ChildrenAmountRange.Max)
        //            {
        //                errors += "Incorect Use of " + keyWord.Input + ", ";
        //                break;
        //            }
        //            else
        //            {
        //                if (currentWord != "")
        //                {
        //                    returnList.Add(new SqlCustomInput(currentWord, keyWord, justInQuotes));
        //                    justInQuotes = false;
        //                }
        //            }
        //        }
        //    }
        //    if (inQuotes)
        //    {
        //        errors += "Incorect Quote Syntax, ";
        //    }
        //    if (returnList.Count < keyWord.ChildrenAmountRange.Min)
        //    {
        //        errors += "Incorect Use of " + keyWord.Input + ", ";
        //    }
        //    return returnList;
        //}


        public static List<SqlKeyWord> Split2(string input, out string errors)
        {
            List<SqlKeyWord> returnList = new List<SqlKeyWord>();
            errors = "";
            string currentWord = "";
            int amountOfParentheses = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == ' ' || input[i] == '(')
                {
                    if (input[i] == '(')
                    {
                        amountOfParentheses++;
                    }
                    if (keywords.ContainsKey(currentWord.ToLower()))
                    {
                        SqlKeyWord keyWord = new SqlKeyWord(currentWord, null, keywords[currentWord.ToLower()], KeyWordTypes.Command);
                        SqlKeyWord nextKeyWord;
                        keyWord.Children = GetKeyWordChildren2(keyWord, input, i, amountOfParentheses, new List<ISqlInput>(), out amountOfParentheses, out i, out errors, out nextKeyWord);
                        returnList.Add(keyWord);
                        returnList = GetBottomKeyWords2(nextKeyWord, input, i, amountOfParentheses, returnList, errors, out i, out amountOfParentheses, out errors);
                        break;
                    }
                    else
                    {
                        errors += "Incorrect Use of" + currentWord + ", ";
                        break;
                    }
                }
                else if (input[i] == '"')
                {
                    errors += "Incorrect Quote Syntax, ";
                    break;
                }
                else if (input[i] == ')')
                {
                    errors += "Incorrect Parentheses Syntax, ";
                    break;
                }
                else if (input[i] == ',')
                {
                    errors += "Incorrect Comma Syntax, ";
                    break;
                }
                else
                {
                    currentWord += input[i];
                }
            }

            if (amountOfParentheses != 0)
            {
                errors += "Incorrect Parentheses Syntax, ";
            }

            return returnList;
        }

        public static List<SqlKeyWord> GetBottomKeyWords2(SqlKeyWord nextKeyWord, string input, int currentIndex, int amountOfParenthesesIn, List<SqlKeyWord> currentList, string errorsIn, out int newIndex, out int amountOfParentheses, out string errors)
        {
            newIndex = currentIndex;
            amountOfParentheses = amountOfParenthesesIn;
            errors = errorsIn;
            if (nextKeyWord != null)
            {
                SqlKeyWord newNextKeyWord;
                nextKeyWord.Children = GetKeyWordChildren2(nextKeyWord, input, currentIndex, amountOfParenthesesIn, new List<ISqlInput>(), out amountOfParentheses, out newIndex, out errors, out newNextKeyWord);
                currentList.Add(nextKeyWord);

                if (newNextKeyWord != null)
                {
                    currentList = GetBottomKeyWords2(newNextKeyWord, input, newIndex, amountOfParentheses, currentList, errors, out newIndex, out amountOfParentheses, out errors);
                }
            }
            return currentList;
        }

        public static List<ISqlInput> GetKeyWordChildren2(SqlKeyWord keyWord, string input, int currentIndex, int amountOfParenthesesIn, List<ISqlInput> currentList, out int amountOfParentheses, out int newIndex, out string errors, out SqlKeyWord nextKeyWord)
        {
            List<ISqlInput> returnList = currentList;

            amountOfParentheses = amountOfParenthesesIn;
            newIndex = currentIndex;
            errors = "";
            nextKeyWord = null;
            bool inQuotes = false;
            string currentWord = "";
            bool justInQuotes = false;
            bool justHadComma = false;
            SqlKeyWord commaGroup = new SqlKeyWord("CommaGroup", keyWord, new Range(1, int.MaxValue, false, false), KeyWordTypes.CommaGroup);
            for (int i = currentIndex; i < input.Length; i++)
            {
                newIndex = i;
                if (!inQuotes)
                {
                    if (input[i] == '"')
                    {
                        if (currentWord.Trim() == "")
                        {
                            inQuotes = true;
                            currentWord = "";
                        }
                        else
                        {
                            errors += "Incorect Quote Syntax, ";
                            break;
                        }
                    }
                    else if (input[i] == '(')
                    {
                        amountOfParentheses++;
                        if (keyWord.ChildrenAmountRange.CanHaveParentheses && !justInQuotes)
                        {
                            if (currentWord.Trim() != "")
                            {
                                if (keywords.ContainsKey(currentWord.ToLower()))
                                {
                                    if (keyWord.ChildrenAmountRange.CanHaveCommas && commaGroup.Children.Count > 0)
                                    {
                                        returnList.Add(commaGroup);
                                    }
                                    nextKeyWord = new SqlKeyWord(currentWord, keyWord.Parent, keywords[currentWord.ToLower()], KeyWordTypes.Command);
                                    break;
                                }
                                else
                                {
                                    if (keyWord.ChildrenAmountRange.CanHaveCommas)
                                    {
                                        commaGroup.Children.Add(new SqlCustomInput(currentWord, commaGroup, justInQuotes));
                                        returnList.Add(commaGroup);
                                    }
                                    else
                                    {
                                        returnList.Add(new SqlCustomInput(currentWord, keyWord, justInQuotes));
                                    }
                                }
                            }

                            SqlKeyWord newParenthesesGroup = new SqlKeyWord("ParenthesesGroup", keyWord, new Range(1, int.MaxValue, false, true), KeyWordTypes.ParenthesesGroup);

                            newParenthesesGroup.Children = GetKeyWordChildren2(newParenthesesGroup, input, i + 1, amountOfParentheses, newParenthesesGroup.Children, out amountOfParentheses, out newIndex, out errors, out nextKeyWord);
                            returnList.Add(newParenthesesGroup);
                            break;
                        }
                        else
                        {
                            errors += "Incorect Parentheses Syntax, ";
                            break;
                        }
                    }
                    else if (input[i] == ')')
                    {
                        amountOfParentheses--;
                        if ((!keywords.ContainsKey(currentWord.ToLower()) || justInQuotes) && currentWord.Trim() != "")
                        {
                            if (keyWord.ChildrenAmountRange.CanHaveCommas)
                            {
                                commaGroup.Children.Add(new SqlCustomInput(currentWord, keyWord, justInQuotes));
                                returnList.Add(commaGroup);
                                break;
                            }
                            else
                            {
                                returnList.Add(new SqlCustomInput(currentWord, keyWord, justInQuotes));
                            }
                            break;
                        }
                        else
                        {
                            errors += "Incorect Use of " + currentWord + ", ";
                            break;
                        }
                    }
                    else if (input[i] == ',')
                    {
                        if ((!keywords.ContainsKey(currentWord.ToLower()) || justInQuotes) && currentWord.Trim() != "")
                        {
                            if (keyWord.ChildrenAmountRange.CanHaveCommas)
                            {
                                if (keyWord.KeyWordType != KeyWordTypes.CommaGroup)
                                {

                                    commaGroup.Children.Add(new SqlCustomInput(currentWord, commaGroup, justInQuotes));

                                    returnList.Add(commaGroup);
                                }
                                else
                                {
                                    returnList.Add(new SqlCustomInput(currentWord, keyWord, justInQuotes));
                                }
                                currentWord = "";
                            }
                            else
                            {
                                errors += "Incorect Comma Syntax, ";
                                break;
                            }
                        }
                        else
                        {
                            errors += "Incorect Use of " + currentWord + ", ";
                            break;
                        }
                        currentWord = "";
                        justHadComma = true;
                        commaGroup = new SqlKeyWord("CommaGroup", keyWord, new Range(1, int.MaxValue, false, false), KeyWordTypes.CommaGroup);
                    }
                    else if (input[i] == ' ')
                    {
                        if (currentWord.Trim() != "" && !justHadComma)
                        {
                            if (keywords.ContainsKey(currentWord.ToLower()) && !justInQuotes)
                            {
                                if (keyWord.ChildrenAmountRange.CanHaveCommas && commaGroup.Children.Count > 0)
                                {
                                    returnList.Add(commaGroup);
                                }
                                nextKeyWord = new SqlKeyWord(currentWord, keyWord.Parent, keywords[currentWord.ToLower()], KeyWordTypes.Command);
                                break;
                            }
                            else
                            {
                                if (keyWord.ChildrenAmountRange.CanHaveCommas)
                                {
                                    SqlCustomInput newCustomInput = new SqlCustomInput(currentWord, commaGroup, justInQuotes);
                                    commaGroup.Children.Add(newCustomInput);
                                }
                                else
                                {
                                    returnList.Add(new SqlCustomInput(currentWord, keyWord, justInQuotes));
                                }
                            }
                        }
                        justHadComma = false;
                        currentWord = "";
                    }
                    else
                    {
                        currentWord += input[i];
                    }
                    justInQuotes = false;
                }
                else
                {
                    //in Quotes
                    if (input[i] == '"')
                    {
                        inQuotes = false;
                        justInQuotes = true;
                    }
                    else
                    {
                        currentWord += input[i];
                    }
                }

                if (i + 1 >= input.Length)
                {
                    if (keyWord.ChildrenAmountRange.CanHaveCommas)
                    {
                        if (currentWord.Trim() != "")
                        {
                            if (!keywords.ContainsKey(currentWord.ToLower()) || justInQuotes)
                            {
                                commaGroup.Children.Add(new SqlCustomInput(currentWord, commaGroup, justInQuotes));
                                returnList.Add(commaGroup);
                            }
                            else
                            {
                                errors += "Incorect Use of " + currentWord + ", ";
                                break;
                            }
                        }
                        else if (commaGroup.Children.Count > 0)
                        {
                            returnList.Add(commaGroup);
                        }
                    }
                    else
                    {
                        if (currentWord.Trim() != "")
                        {
                            if (!keywords.ContainsKey(currentWord.ToLower()) || justInQuotes)
                            {
                                returnList.Add(new SqlCustomInput(currentWord, keyWord, justInQuotes));
                            }
                            else
                            {
                                errors += "Incorect Use of " + currentWord + ", ";
                                break;
                            }
                        }
                    }
                }
            }
            if (returnList.Count < keyWord.ChildrenAmountRange.Min)
            {
                errors += "Incorect Use of " + keyWord.Input + ", ";
            }
            if (returnList.Count >= keyWord.ChildrenAmountRange.Max)
            {
                errors += "Incorect Use of " + keyWord.Input + ", ";
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
