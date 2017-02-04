using MyMySql.ICustomWords;
using MyMySql.IWords;
using MyMySql.IWords.ICustomWords;
using MyMySql.IWords.ILanguageWords;
using MyMySql.TableStuff;
using MyMySql.TrieStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MyMySql
{
    public class Database
    {
        Dictionary<string, Table> tables = new Dictionary<string, Table>();

        Dictionary<AllWordTypes, AllWordDictionaryInfo> allWordDictionary = new Dictionary<AllWordTypes, AllWordDictionaryInfo>();

        Dictionary<char, SyntaxWord> syntaxDictionary = new Dictionary<char, SyntaxWord>();

        Dictionary<string, LogicOperationWord> logicOperationDictionary = new Dictionary<string, LogicOperationWord>();
        Dictionary<string, MathOperationWord> mathOpperationDictionary = new Dictionary<string, MathOperationWord>();
        List<char> logicOpperationChars = new List<char>();
        List<char> mathOpperationChars = new List<char>();
        Dictionary<string, Type> typeDictionary = new Dictionary<string, Type>();

        char[] whiteSpaceChars = new char[] { ' ', '\t', '\n' };
        SyntaxWord commaSyntaxWord;
        SyntaxWord dotSyntaxWord;
        SyntaxWord parenthesesSyntaxWord;
        SyntaxWord quoteSyntaxWord;

        Color defaultColor = Color.Black;

        Dictionary<string, Keyword> keywordDictionary = new Dictionary<string, Keyword>();

        Dictionary<string, TableWord> tableDictionary = new Dictionary<string, TableWord>();
        Dictionary<string, List<IWord>> columnDictionary = new Dictionary<string, List<IWord>>();

        Dictionary<string, FunctionWord> functionDictionary = new Dictionary<string, FunctionWord>();

        List<Command> commands = new List<Command>();
        Trie commandTrie = new Trie(false);
        public Database(XDocument xdoc)
        {
            typeDictionary.Add("int", typeof(int));
            typeDictionary.Add("string", typeof(string));
            typeDictionary.Add("bool", typeof(bool));

            syntaxDictionary.Add('(', parenthesesSyntaxWord = new SyntaxWord('(', new List<char>() { ')' }, false, LexicalComplier, AllWordTypes.Parentheses, ParseSyntaxWithChildren, new List<List<WordRange>>()));
            syntaxDictionary.Add('"', quoteSyntaxWord = new SyntaxWord('"', new List<char>() { '"' }, false, QuoteFunc, AllWordTypes.Syntax, ParseCustomCustomSyntax, new List<List<WordRange>>()));

            commaSyntaxWord = new SyntaxWord(',', new List<char>(), true, AddBreakFunc, AllWordTypes.Comma, ParseSyntaxWithChildren, new List<List<WordRange>>());
            syntaxDictionary.Add(',', commaSyntaxWord);

            dotSyntaxWord = new SyntaxWord('.', new List<char>(), true, AddBreakFunc, AllWordTypes.Syntax, null, new List<List<WordRange>>());
            syntaxDictionary.Add('.', dotSyntaxWord);

            List<List<WordRange>> logicWordRange = new List<List<WordRange>>();
            logicWordRange.Add(new List<WordRange>() { new WordRange(1, 2, new ColumnWord("", null, null, ParseCustomSyntax, false)), new WordRange(1, 2, new ColumnWord("", null, null, ParseCustomSyntax, false)) });
            logicWordRange.Add(new List<WordRange>() { new WordRange(1, 2, new CustomCustomWord("", null, null, ParseCustomCustomSyntax)), new WordRange(1, 2, new CustomCustomWord("", null, null, ParseCustomCustomSyntax)) });
            logicWordRange.Add(new List<WordRange>() { new WordRange(1, 2, new ColumnWord("", null, null, ParseCustomSyntax, false)), new WordRange(1, 2, new CustomCustomWord("", null, null, ParseCustomCustomSyntax)) });
            logicWordRange.Add(new List<WordRange>() { new WordRange(1, 2, new CustomCustomWord("", null, null, ParseCustomCustomSyntax)), new WordRange(1, 2, new ColumnWord("", null, null, ParseCustomSyntax, false)) });

            logicOperationDictionary.Add("=", new LogicOperationWord("=", null, ParseLogicOpperationSyntax, logicWordRange));
            logicOperationDictionary.Add("<=", new LogicOperationWord("<=", null, ParseLogicOpperationSyntax, logicWordRange));
            logicOperationDictionary.Add(">=", new LogicOperationWord(">=", null, ParseLogicOpperationSyntax, logicWordRange));
            logicOperationDictionary.Add("!=", new LogicOperationWord("!=", null, ParseLogicOpperationSyntax, logicWordRange));
            logicOperationDictionary.Add("<", new LogicOperationWord("<", null, ParseLogicOpperationSyntax, logicWordRange));
            logicOperationDictionary.Add(">", new LogicOperationWord(">", null, ParseLogicOpperationSyntax, logicWordRange));
            foreach (string key in logicOperationDictionary.Keys)
            {
                for (int i = 0; i < key.Length; i++)
                {
                    if (!syntaxDictionary.ContainsKey(key[i]))
                    {
                        syntaxDictionary.Add(key[i], new SyntaxWord(key[i], new List<char>(), true, AddBreakFunc, AllWordTypes.Syntax, ParseToWordType, new List<List<WordRange>>()));
                        logicOpperationChars.Add(key[i]);
                    }
                }
            }
            mathOpperationDictionary.Add("+", new MathOperationWord("+", null, ParseToWordType));
            mathOpperationDictionary.Add("-", new MathOperationWord("-", null, ParseToWordType));
            mathOpperationDictionary.Add("*", new MathOperationWord("*", null, ParseToWordType));
            mathOpperationDictionary.Add("/", new MathOperationWord("/", null, ParseToWordType));
            foreach (string key in mathOpperationDictionary.Keys)
            {
                for (int i = 0; i < key.Length; i++)
                {
                    if (!syntaxDictionary.ContainsKey(key[i]))
                    {
                        syntaxDictionary.Add(key[i], new SyntaxWord(key[i], new List<char>(), true, AddBreakFunc, AllWordTypes.Syntax, ParseToWordType, new List<List<WordRange>>()));
                        mathOpperationChars.Add(key[i]);
                    }
                }
            }

            #region keywordStuff
            List<WordRange> nothingWordRange = new List<WordRange>() { new WordRange(0, 1, null) };

            #region selectCommand
            List<List<WordRange>> selectParamaters = new List<List<WordRange>>();
            SyntaxWord selectComma = new SyntaxWord(commaSyntaxWord.Input, commaSyntaxWord, commaSyntaxWord.RangesThatWorked);
            selectComma.ChildrenRanges.Add(new List<WordRange>() { new WordRange(1, 2, new ColumnWord("", null, null, ParseCustomSyntax, false)) });
            selectParamaters.Add(new List<WordRange>() { new WordRange(2, int.MaxValue, selectComma) });
            selectParamaters.Add(new List<WordRange>() { new WordRange(1, 2, new ColumnWord("", null, null, ParseCustomSyntax, false)) });
            selectParamaters.Add(new List<WordRange>() { new WordRange(1, 2, new FunctionWord("", new List<List<WordRange>>(), ParseFunction)) });
            keywordDictionary.Add("select", new Keyword("select", selectParamaters));

            List<List<WordRange>> fromParamaters = new List<List<WordRange>>();
            fromParamaters.Add(new List<WordRange>() { new WordRange(1, 2, new TableWord("", "", null, ParseCustomSyntax, false)) });
            keywordDictionary.Add("from", new Keyword("from", fromParamaters));

            List<List<WordRange>> whereParamaters = new List<List<WordRange>>();
            whereParamaters.Add(new List<WordRange>() { new WordRange(1, 2, new LogicOperationWord("", null, ParseLogicOpperationSyntax, logicWordRange)) });
            keywordDictionary.Add("where", new Keyword("where", whereParamaters));

            List<List<CommandKeywordInfo>> selectCommandInfo = new List<List<CommandKeywordInfo>>();
            selectCommandInfo.Add(new List<CommandKeywordInfo>() { new CommandKeywordInfo() { CommandKeyword = keywordDictionary["select"],  KeywordRangesThatDontWork = new List<List<WordRange>>()},
                                                                   new CommandKeywordInfo() { CommandKeyword = keywordDictionary["from"], KeywordRangesThatDontWork = new List<List<WordRange>>() } });

            Command selectCommand = new Command(selectCommandInfo, new InputInfo() { InputType = null, KeywordsNotAllowedAsInput = new List<string>() }, typeof(Table), SelectFunction);
            commands.Add(selectCommand);
            Command whereCommand = new Command(new List<List<CommandKeywordInfo>>()
                                              { new List<CommandKeywordInfo>()
                                              { new CommandKeywordInfo() { CommandKeyword = keywordDictionary["where"], KeywordRangesThatDontWork = new List<List<WordRange>>() } } },
                                              new InputInfo() { InputType = typeof(Table), KeywordsNotAllowedAsInput = new List<string>() { "where" } }, typeof(Table), WhereFunction);
            commands.Add(whereCommand);
            #endregion

            #region insertCommand
            List<List<WordRange>> insertParamaters = new List<List<WordRange>>();
            insertParamaters.Add(new List<WordRange>() { new WordRange(1, 2, new TableWord("", "", null, ParseCustomSyntax, false)) });
            keywordDictionary.Add("into", new Keyword("into", new List<List<WordRange>>(insertParamaters)));
            insertParamaters.Add(nothingWordRange);
            keywordDictionary.Add("insert", new Keyword("insert", insertParamaters));

            List<List<WordRange>> valuesParamaters = new List<List<WordRange>>();
            SyntaxWord valuesParentheses = new SyntaxWord(parenthesesSyntaxWord.Input, parenthesesSyntaxWord, parenthesesSyntaxWord.RangesThatWorked);
            valuesParentheses.ChildrenRanges.Add(new List<WordRange>() { new WordRange(1, 2, new CustomCustomWord("", null, null, ParseCustomCustomSyntax)) });
            SyntaxWord valuesParenthesesComma = new SyntaxWord(commaSyntaxWord.Input, commaSyntaxWord, commaSyntaxWord.RangesThatWorked);
            valuesParenthesesComma.ChildrenRanges.Add(new List<WordRange>() { new WordRange(1, 2, new CustomCustomWord("", null, null, ParseCustomCustomSyntax)) });
            valuesParentheses.ChildrenRanges.Add(new List<WordRange>() { new WordRange(1, int.MaxValue, valuesParenthesesComma) });
            valuesParamaters.Add(new List<WordRange>() { new WordRange(1, 2, valuesParentheses) });
            SyntaxWord valuesCommaParentheses = new SyntaxWord(commaSyntaxWord.Input, commaSyntaxWord, commaSyntaxWord.RangesThatWorked);
            valuesCommaParentheses.ChildrenRanges.Add(new List<WordRange>() { new WordRange(1, 2, valuesParentheses) });
            valuesParamaters.Add(new List<WordRange>() { new WordRange(2, int.MaxValue, valuesCommaParentheses) });
            keywordDictionary.Add("values", new Keyword("values", valuesParamaters));

            List<List<CommandKeywordInfo>> insertCommandInfo = new List<List<CommandKeywordInfo>>();
            insertCommandInfo.Add(new List<CommandKeywordInfo>() { new CommandKeywordInfo() { CommandKeyword = keywordDictionary["insert"],  KeywordRangesThatDontWork = new List<List<WordRange>>() { keywordDictionary["insert"].ChildrenRanges[1] } },
                                                                   new CommandKeywordInfo() { CommandKeyword = keywordDictionary["values"], KeywordRangesThatDontWork = new List<List<WordRange>>() } });

            insertCommandInfo.Add(new List<CommandKeywordInfo>() { new CommandKeywordInfo() { CommandKeyword = keywordDictionary["insert"],  KeywordRangesThatDontWork = new List<List<WordRange>>() { keywordDictionary["insert"].ChildrenRanges[0] } },
                                                                   new CommandKeywordInfo() { CommandKeyword = keywordDictionary["into"], KeywordRangesThatDontWork = new List<List<WordRange>>() },
                                                                   new CommandKeywordInfo() { CommandKeyword = keywordDictionary["values"], KeywordRangesThatDontWork = new List<List<WordRange>>() }});
            Command insertCommand = new Command(insertCommandInfo, new InputInfo() { InputType = null, KeywordsNotAllowedAsInput = new List<string>() }, null, InsertFunction);
            commands.Add(insertCommand);
            #endregion

            #region updateCommand
            keywordDictionary.Add("update", new Keyword("update", fromParamaters));

            List<List<WordRange>> setParameters = new List<List<WordRange>>();
            List<WordRange> columnLogicColumnWordRanges = new List<WordRange>() { new WordRange(1, 2, new LogicOperationWord("", null, ParseLogicOpperationSyntax, new List<List<WordRange>>() { logicWordRange[2] })) };
            setParameters.Add(columnLogicColumnWordRanges);
            SyntaxWord setComma = new SyntaxWord(commaSyntaxWord.Input, commaSyntaxWord, commaSyntaxWord.RangesThatWorked);
            setComma.ChildrenRanges.Add(columnLogicColumnWordRanges);
            setParameters.Add(new List<WordRange>() { new WordRange(2, int.MaxValue, setComma) });
            keywordDictionary.Add("set", new Keyword("set", setParameters));

            List<List<CommandKeywordInfo>> updateCommandInfo = new List<List<CommandKeywordInfo>>();
            updateCommandInfo.Add(new List<CommandKeywordInfo>() { new CommandKeywordInfo() { CommandKeyword = keywordDictionary["update"],  KeywordRangesThatDontWork = new List<List<WordRange>>()},
                                                                   new CommandKeywordInfo() { CommandKeyword = keywordDictionary["set"], KeywordRangesThatDontWork = new List<List<WordRange>>() } });

            updateCommandInfo.Add(new List<CommandKeywordInfo>() { new CommandKeywordInfo() { CommandKeyword = keywordDictionary["update"],  KeywordRangesThatDontWork = new List<List<WordRange>>()},
                                                                   new CommandKeywordInfo() { CommandKeyword = keywordDictionary["set"], KeywordRangesThatDontWork = new List<List<WordRange>>() }});
            Command updateCommand = new Command(updateCommandInfo, new InputInfo() { InputType = null, KeywordsNotAllowedAsInput = new List<string>() }, typeof(Table), UpdateFunction);
            commands.Add(updateCommand);
            #endregion

            #region createTableCommand
            keywordDictionary.Add("create", new Keyword("create", new List<List<WordRange>>() { nothingWordRange }));

            List<List<WordRange>> tableParameters = new List<List<WordRange>>();
            SyntaxWord tableParentheses = new SyntaxWord(parenthesesSyntaxWord.Input, parenthesesSyntaxWord, parenthesesSyntaxWord.RangesThatWorked);
            List<WordRange> columnTypeRanges = new List<WordRange>() { new WordRange(1, 2, new ColumnWord("", null, null, ParseCustomSyntax, true)), new WordRange(1, 2, new CustomCustomWord("", typeof(Type), null, ParseCustomCustomSyntax)) };
            tableParentheses.ChildrenRanges.Add(columnTypeRanges);
            SyntaxWord tableComma = new SyntaxWord(commaSyntaxWord.Input, commaSyntaxWord, commaSyntaxWord.RangesThatWorked);
            tableComma.ChildrenRanges.Add(columnTypeRanges);
            tableParentheses.ChildrenRanges.Add(new List<WordRange>() { new WordRange(2, int.MaxValue, tableComma) });
            tableParameters.Add(new List<WordRange>() { new WordRange(1, 2, new TableWord("", "", null, ParseCustomSyntax, true)), new WordRange(1, 2, tableParentheses) });
            keywordDictionary.Add("table", new Keyword("table", tableParameters));

            List<List<CommandKeywordInfo>> createTableCommandInfo = new List<List<CommandKeywordInfo>>();
            createTableCommandInfo.Add(new List<CommandKeywordInfo>() { new CommandKeywordInfo() { CommandKeyword = keywordDictionary["create"],  KeywordRangesThatDontWork = new List<List<WordRange>>()},
                                                                        new CommandKeywordInfo() { CommandKeyword = keywordDictionary["table"], KeywordRangesThatDontWork = new List<List<WordRange>>() } });
            Command createTableCommand = new Command(createTableCommandInfo, new InputInfo() { InputType = null, KeywordsNotAllowedAsInput = new List<string>() }, null, CreateTableFunction);
            commands.Add(createTableCommand);
            #endregion

            #region JoinRegion

            keywordDictionary.Add("inner", new Keyword("inner", new List<List<WordRange>>() { nothingWordRange }));

            List<List<WordRange>> joinRanges = new List<List<WordRange>>();
            joinRanges.Add(new List<WordRange>() { new WordRange(1, 2, new TableWord("", "", null, ParseCustomSyntax, false)) });
            keywordDictionary.Add("join", new Keyword("join", joinRanges));

            List<List<WordRange>> onRanges = new List<List<WordRange>>();
            List<List<WordRange>> onLogicRanges = new List<List<WordRange>>();
            onLogicRanges.Add(new List<WordRange>() { new WordRange(2, 3, new ColumnWord("", null, null, ParseCustomSyntax, false)) });
            onRanges.Add(new List<WordRange>() { new WordRange(1, 2, new LogicOperationWord("", null, ParseLogicOpperationSyntax, onLogicRanges)) });
            keywordDictionary.Add("on", new Keyword("on", onRanges));

            List<List<CommandKeywordInfo>> joinCommandInfo = new List<List<CommandKeywordInfo>>();
            joinCommandInfo.Add(new List<CommandKeywordInfo>() { new CommandKeywordInfo() { CommandKeyword = keywordDictionary["inner"], KeywordRangesThatDontWork = new List<List<WordRange>>()},
                                                                 new CommandKeywordInfo() {CommandKeyword = keywordDictionary["join"], KeywordRangesThatDontWork = new List<List<WordRange>>() },
                                                                 new CommandKeywordInfo() {CommandKeyword = keywordDictionary["on"], KeywordRangesThatDontWork = new List<List<WordRange>>() } });

            joinCommandInfo.Add(new List<CommandKeywordInfo>() { new CommandKeywordInfo() {CommandKeyword = keywordDictionary["join"], KeywordRangesThatDontWork = new List<List<WordRange>>() },
                                                                 new CommandKeywordInfo() {CommandKeyword = keywordDictionary["on"], KeywordRangesThatDontWork = new List<List<WordRange>>() } });
            commands.Add(new Command(joinCommandInfo, new InputInfo() { InputType = typeof(Table), KeywordsNotAllowedAsInput = new List<string>() { "where" } }, typeof(Table), JoinFunction));
            #endregion

            #endregion

            Dictionary<string, SyntaxWord> parenthesesDictionary = new Dictionary<string, SyntaxWord>();
            parenthesesDictionary.Add(parenthesesSyntaxWord.Input, parenthesesSyntaxWord);
            allWordDictionary.Add(AllWordTypes.Parentheses, new AllWordDictionaryInfo() { WordDictionary = new Hashtable(parenthesesDictionary), DoToLower = false, ContainsListOfIWords = false });

            Dictionary<string, SyntaxWord> commaDictionary = new Dictionary<string, SyntaxWord>();
            commaDictionary.Add(commaSyntaxWord.Input, commaSyntaxWord);
            allWordDictionary.Add(AllWordTypes.Comma, new AllWordDictionaryInfo() { WordDictionary = new Hashtable(commaDictionary), DoToLower = false, ContainsListOfIWords = false });

            allWordDictionary.Add(AllWordTypes.LogicOperation, new AllWordDictionaryInfo() { WordDictionary = new Hashtable(logicOperationDictionary), DoToLower = false, ContainsListOfIWords = false });
            allWordDictionary.Add(AllWordTypes.MathOperation, new AllWordDictionaryInfo() { WordDictionary = new Hashtable(mathOpperationDictionary), DoToLower = false, ContainsListOfIWords = false });


            functionDictionary.Add("*", new FunctionWord("*", new List<List<WordRange>>() { nothingWordRange }, ParseCustomSyntax));

            List<List<WordRange>> countParamaters = new List<List<WordRange>>();
            SyntaxWord countParentheses = new SyntaxWord(parenthesesSyntaxWord.Input, parenthesesSyntaxWord, parenthesesSyntaxWord.RangesThatWorked);
            countParentheses.ChildrenRanges.Add(new List<WordRange>() { new WordRange(1, 2, new ColumnWord("", null, null, ParseCustomSyntax, false)) });
            countParamaters.Add(new List<WordRange>() { new WordRange(1, 2, countParentheses) });
            functionDictionary.Add("count", new FunctionWord("count", countParamaters, ParseFunction));


            LoadXML(xdoc);
            tables["Students"].Tree.BaseNode = null;
            //tables.Add("Students", null);
            //tables["Students"] = new Table("Students", new SqlColumn("StudentID", typeof(int)), new SqlColumn("Name", typeof(string)));
            //tables["Students"].AddRow(0, "josiah");
            //tables["Students"].AddRow(1, "bob");
            //tables["Students"].AddRow(2, "joe");
            //tables.Add("Animals", null);
            //tables["Animals"] = new Table("Animals", new SqlColumn("AnimalID", typeof(int)), new SqlColumn("Name", typeof(string)));
            //tables["Animals"].AddRow(0, "mesha");
            //tables["Animals"].AddRow(1, "ringo");
            //tables["Animals"].AddRow(2, "coco");
            foreach (Table table in tables.Values)
            {
                tableDictionary.Add(table.Name, new TableWord(table.Name, "", table, ParseCustomSyntax, false));
                foreach (SqlColumn column in table.SqlColumns)
                {
                    if (!columnDictionary.ContainsKey(column.Name))
                    {
                        columnDictionary.Add(column.Name, new List<IWord>());
                    }
                    columnDictionary[column.Name].Add(new ColumnWord(column.Name, column, table, ParseCustomSyntax, false));
                    columnDictionary.Add(table.Name + dotSyntaxWord.StartingSyntax + column.Name, new List<IWord>() { new ColumnWord(table.Name + dotSyntaxWord.StartingSyntax + column.Name, column, table, ParseCustomSyntax, false) });
                }
            }
            allWordDictionary.Add(AllWordTypes.Table, new AllWordDictionaryInfo() { WordDictionary = new Hashtable(tableDictionary), DoToLower = false, ContainsListOfIWords = false });
            allWordDictionary.Add(AllWordTypes.Column, new AllWordDictionaryInfo() { WordDictionary = new Hashtable(columnDictionary), DoToLower = false, ContainsListOfIWords = true });

            foreach (Command command in commands)
            {
                commandTrie.AddCommand(command);
            }
        }

        public OutputInfo DoSQlStuff(string userInput)
        {
            OutputInfo outputInfo = new OutputInfo() { Output = "", Errors = new List<string>(), SyntaxHighlightedInput = new RenderString(new List<RenderCharacter>()) };

            CompilerInfo compilerInfo = RunCompilers(userInput);
            outputInfo.SyntaxHighlightedInput = compilerInfo.SyntaxHighlightedInput;
            outputInfo.Errors = compilerInfo.Errors;
            if (outputInfo.Errors.Count == 0)
            {
                for (int i = 0; i < compilerInfo.Lexemes.Count; i++)
                {
                    outputInfo.Output += compilerInfo.Lexemes[i].GetType().Name + "Input: " + compilerInfo.Lexemes[i].Input;
                    if (i + 1 < compilerInfo.Lexemes.Count)
                    {
                        outputInfo.Output += Environment.NewLine;
                    }
                }
            }
            return outputInfo;
        }

        public CompilerInfo RunCompilers(string userInput)
        {
            CompilerInfo returnInfo = new CompilerInfo() { Lexemes = new List<IWord>(), Errors = new List<string>() };

            CompilerInfo lexicalInfo = LexicalComplier(userInput);
            returnInfo.Errors.AddRange(lexicalInfo.Errors);
            returnInfo.Lexemes = lexicalInfo.Lexemes;
            returnInfo.SyntaxHighlightedInput = lexicalInfo.SyntaxHighlightedInput;
            if (returnInfo.Errors.Count == 0)
            {
                CompilerInfo dotInfo = DotCompiler(lexicalInfo.Lexemes);
                returnInfo.Errors.AddRange(dotInfo.Errors);
                returnInfo.Lexemes = dotInfo.Lexemes;
                returnInfo.SyntaxHighlightedInput = dotInfo.SyntaxHighlightedInput;
                if (returnInfo.Errors.Count == 0)
                {
                    CompilerInfo operationInfo = OperationCompiler(dotInfo.Lexemes);
                    returnInfo.Errors.AddRange(operationInfo.Errors);
                    returnInfo.Lexemes = operationInfo.Lexemes;
                    returnInfo.SyntaxHighlightedInput = operationInfo.SyntaxHighlightedInput;
                    if (returnInfo.Errors.Count == 0)
                    {
                        CompilerInfo commaInfo = CommaCompiler(operationInfo.Lexemes, null);
                        returnInfo.Errors.AddRange(commaInfo.Errors);
                        returnInfo.Lexemes = commaInfo.Lexemes;
                        returnInfo.SyntaxHighlightedInput = commaInfo.SyntaxHighlightedInput;
                        if (returnInfo.Errors.Count == 0)
                        {
                            KeywordsInfo keywordSyntaxInfo = KeywordSyntaxCompiler(commaInfo.Lexemes);
                            returnInfo.Errors.AddRange(keywordSyntaxInfo.Errors);
                            returnInfo.SyntaxHighlightedInput = keywordSyntaxInfo.SyntaxHighlightedInput;

                            if (returnInfo.Errors.Count == 0)
                            {
                                CommandsInfo commandGroupInfo = CommandGroupCompiler(keywordSyntaxInfo.Keywords);
                                returnInfo.Errors.AddRange(commandGroupInfo.Errors);
                                returnInfo.SyntaxHighlightedInput = commandGroupInfo.SyntaxHighlightedInput;

                                if (returnInfo.Errors.Count == 0)
                                {
                                    CommandsInfo commandKeywordInfo = CommandKeywordCompiler(commandGroupInfo.Commands);
                                    returnInfo.Errors.AddRange(commandKeywordInfo.Errors);
                                    returnInfo.SyntaxHighlightedInput = commandKeywordInfo.SyntaxHighlightedInput;
                                    if (returnInfo.Errors.Count == 0)
                                    {
                                        CommandsInfo commandCustomCustomInfo = CommandCustomCustomCompiler(commandKeywordInfo.Commands);
                                        returnInfo.Errors.AddRange(commandCustomCustomInfo.Errors);
                                        returnInfo.SyntaxHighlightedInput = commandCustomCustomInfo.SyntaxHighlightedInput;
                                        List<IWord> columns = new List<IWord>();
                                        foreach (ColumnWord column in commandCustomCustomInfo.Commands[0].ColumnsInCommand)
                                        {
                                            columns.Add(column);
                                        }
                                        columns.Add(new LexicalWord(""));
                                        Table testSelect = SelectFunction(null, columns, commandCustomCustomInfo.Commands[0]).ReturnTable;
                                        string test = DisplayTable(testSelect, testSelect.Select(), testSelect.SqlColumns);
                                    }
                                }
                                allWordDictionary = RemoveInitializingWordsFromDictionaries(allWordDictionary);
                            }
                        }
                    }
                }
            }

            return returnInfo;
        }

        XDocument LoadXML(XDocument xdoc)
        {
            foreach (XElement tableElement in xdoc.Root.Elements())
            {
                Table table = new Table(tableElement);
                tables.Add(table.Name, table);
            }
            xdoc.Root.RemoveAll();
            return xdoc;
        }

        public void SaveXML(XDocument xdoc, string xmlUrl)
        {
            List<KeyValuePair<string, Table>> tableList = tables.ToList();
            xdoc.Root.RemoveAll();
            for (int i = 0; i < tableList.Count; i++)
            {
                Table table = tableList[i].Value;
                XElement tableElement = new XElement("Table");
                tableElement.Add(new XAttribute("name", tableList[i].Key));
                tableElement.Add(new XAttribute("currentID", table.CurrentID));

                XElement collumnsElement = new XElement("Columns");
                foreach (SqlColumn collumn in table.SqlColumns)
                {
                    XElement currentCollumn = new XElement("Column");
                    currentCollumn.Add(new XAttribute(collumn.Name, collumn.VarType));
                    collumnsElement.Add(currentCollumn);
                }
                tableElement.Add(collumnsElement);

                XElement binaryTree = new XElement("BinaryTree");
                if (table.Tree.BaseNode != null)
                {
                    binaryTree = FillXMLBinaryTree(table, table.Tree.BaseNode, binaryTree);
                }

                tableElement.Add(binaryTree);
                xdoc.Root.Add(tableElement);
            }
            //xdoc.Save(xmlUrl);
        }

        XElement FillXMLBinaryTree(Table table, BSTNode<SqlRow> node, XElement currentElement)
        {
            XElement newNode = new XElement("Node", new XAttribute("id", node.Value.ID));
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

        #region LexicalStage
        /// <summary>
        /// Breaks up the input and handles the syntax in syntaxDictionary
        /// </summary>
        /// <param name="input">The user's input</param>
        /// <returns>LexicalCompilerInfo which has Errors, any incorrect syntax, and Lexemes, a list of IWords that contain LexicalWords amd SyntaxWords</returns>
        CompilerInfo LexicalComplier(string input)
        {
            CompilerInfo returnInfo = new CompilerInfo() { Lexemes = new List<IWord>(), Errors = new List<string>(), SyntaxHighlightedInput = new RenderString(input, defaultColor) };

            string currentWord = "";//The currentWord that will be put into a lexeme

            //loops through input, when it hits a whiteSpace curentWord is added and it also handles syntax by calling the syntax's function
            for (int i = 0; i < input.Length; i++)
            {

                //If the current character is a syntax character then add currentWord and run the syntax's function
                if (syntaxDictionary.ContainsKey(input[i]))
                {
                    if (currentWord.Trim(whiteSpaceChars) != "")
                    {
                        returnInfo.Lexemes.Add(new LexicalWord(currentWord.Trim(whiteSpaceChars)));
                    }
                    currentWord = "";
                    SyntaxWord currentSyntaxWord = syntaxDictionary[input[i]];

                    //Finds the closeing syntax by looping forward
                    int endOfSyntax = input.Length;
                    if (currentSyntaxWord.CloseingSyntaxList.Count > 0)
                    {
                        for (int j = i + 1; j < input.Length; j++)
                        {
                            if (currentSyntaxWord.CloseingSyntaxList.Contains(input[j]))
                            {
                                endOfSyntax = j;
                                break;
                            }
                            else if (input[j] == currentSyntaxWord.StartingSyntax)
                            {
                                returnInfo.Errors.Add("Incorect Syntax Near " + currentSyntaxWord.Input);
                                break;
                            }
                        }
                    }
                    else
                    {
                        endOfSyntax = i + 1;
                    }

                    //Get the lexemes inside the syntax by initilizeing a new syntax with the string inside it and running the syntax's GetWordsInsideSyntax
                    if (returnInfo.Errors.Count == 0)
                    {
                        if (endOfSyntax < input.Length || currentSyntaxWord.EndLineIsCloseingSyntax)
                        {
                            SyntaxWord newSyntaxWord = new SyntaxWord(input.Substring(i + 1, endOfSyntax - i - 1), currentSyntaxWord, null);
                            CompilerInfo newSyntaxWordInfo = newSyntaxWord.FindWordsInsideSyntax();
                            if (newSyntaxWordInfo.Errors.Count > 0)
                            {
                                returnInfo.Errors.AddRange(newSyntaxWordInfo.Errors);
                                break;
                            }
                            returnInfo.Lexemes.Add(newSyntaxWord);
                        }
                        else
                        {
                            returnInfo.Errors.Add("Incorect Syntax Near " + currentSyntaxWord.Input);
                            break;
                        }
                        //Sets i to the endOfSyntax because the syntax hadled it up to that point. 
                        if (currentSyntaxWord.CloseingSyntaxList.Count > 0)
                        {
                            i = endOfSyntax;
                        }
                        else
                        {
                            i = endOfSyntax - 1;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                //End of input breaks things up
                else if (i + 1 >= input.Length)
                {
                    currentWord += input[i];
                    if (currentWord.Trim(whiteSpaceChars) != "")
                    {
                        returnInfo.Lexemes.Add(new LexicalWord(currentWord.Trim(whiteSpaceChars)));
                    }
                }
                //If the current character is in whiteSpaceChars add currentWord
                else if (whiteSpaceChars.Contains(input[i]))
                {
                    if (currentWord.Trim(whiteSpaceChars) != "")
                    {
                        returnInfo.Lexemes.Add(new LexicalWord(currentWord.Trim(whiteSpaceChars)));
                    }
                    currentWord = "";
                }
                else
                {
                    currentWord += input[i];
                }
            }

            return returnInfo;
        }

        /// <summary>
        /// Gets the data inside quotes and makes it a lexeme
        /// </summary>
        /// <param name="input">The user's input</param>
        /// <returns>The list of lexemes in the quotes (there will be 1)</returns>
        CompilerInfo QuoteFunc(string input)
        {
            return new CompilerInfo() { Lexemes = new List<IWord>() { new LexicalWord(input) }, Errors = new List<string>() };
        }

        /// <summary>
        /// The function for syntax that just ignores the syntax
        /// </summary>
        /// <param name="input">The users input</param>
        /// <returns>An empty list of lexemes and no errors</returns>
        CompilerInfo AddBreakFunc(string input)
        {
            return new CompilerInfo() { Lexemes = new List<IWord>(), Errors = new List<string>() };
        }

        /// <summary>
        /// Handles the syntax of the dots by setting the lexeme after the dot to be the lexeme before the dot's child
        /// </summary>
        /// <param name="lexemes">The lexemes after LexicalComiler</param>
        /// <returns>The new lexemes and any syntax issues</returns>
        CompilerInfo DotCompiler(List<IWord> lexemes)
        {
            CompilerInfo returnInfo = new CompilerInfo() { Lexemes = lexemes, Errors = new List<string>() };

            //Loops through the lexemes to find and handle the dots
            for (int i = 0; i < returnInfo.Lexemes.Count; i++)
            {
                if (returnInfo.Lexemes[i].Input == dotSyntaxWord.Input)
                {
                    //if the lexemes before and after the dot follow the correct syntax then set the lexeme after the dot to be the child of the lexeme before the dot
                    if (i > 0 && i + 1 < returnInfo.Lexemes.Count &&
                        returnInfo.Lexemes[i - 1].WordType == WordTypes.Lexical && returnInfo.Lexemes[i + 1].WordType == WordTypes.Lexical &&
                        ((LexicalWord)returnInfo.Lexemes[i - 1]).DotChild == null && ((LexicalWord)returnInfo.Lexemes[i + 1]).DotChild == null)
                    {
                        //LexicalWord currentWord = new LexicalWord(returnInfo.Lexemes[i - 1].Input, new LexicalWord(returnInfo.Lexemes[i + 1].Input));
                        LexicalWord currentWord = new LexicalWord(returnInfo.Lexemes[i - 1].Input + dotSyntaxWord.StartingSyntax + returnInfo.Lexemes[i + 1].Input);
                        returnInfo.Lexemes[i] = currentWord;
                        returnInfo.Lexemes.RemoveAt(i + 1);
                        returnInfo.Lexemes.RemoveAt(i - 1);
                        i = i - 1;
                    }
                    else
                    {
                        returnInfo.Errors.Add("Incorect Use of " + dotSyntaxWord.Input);
                        break;
                    }
                }
                //handle dots inside the syntax of the currentLexeme
                else if (returnInfo.Lexemes[i].WordType == WordTypes.Language)
                {
                    SyntaxWord currentSyntaxWord = (SyntaxWord)returnInfo.Lexemes[i];
                    CompilerInfo syntaxDotInfo = DotCompiler(currentSyntaxWord.WordsInsideSyntax);
                    if (syntaxDotInfo.Errors.Count == 0)
                    {
                        currentSyntaxWord.WordsInsideSyntax = syntaxDotInfo.Lexemes;
                    }
                    else
                    {
                        returnInfo.Errors.AddRange(syntaxDotInfo.Errors);
                        break;
                    }
                }
            }

            return returnInfo;
        }

        /// <summary>
        /// Handles comma syntax by putting data seperated with commas into comma groups
        /// </summary>
        /// <param name="lexemes">The lexemes that the input has been broken up into from dot comiler</param>
        /// <param name="syntaxParent">The syntsax that the lexemes are in</param>
        /// <returns>The new lexemes and any syntax issues</returns>
        CompilerInfo CommaCompiler(List<IWord> lexemes, IWord syntaxParent)
        {
            CompilerInfo returnInfo = new CompilerInfo() { Lexemes = new List<IWord>(), Errors = new List<string>() };
            bool hasCommaSyntax = false;
            int indexOfLastComma = -100;
            int amountOfChildrenInLastComma = 0;
            int lastIndexNotInComma = 0;
            //Loops through all the lexemes and figures out which lexemes chould be in comma groups
            for (int i = 0; i < lexemes.Count; i++)
            {
                //if the lexeme is a syntax word handle the commas of the words inside the syntax
                if (lexemes[i].WordType == WordTypes.Language && lexemes[i].AllWordType != commaSyntaxWord.AllWordType)
                {
                    CompilerInfo syntaxInfo = CommaCompiler(lexemes[i].Children, lexemes[i]);
                    if (syntaxInfo.Errors.Count == 0)
                    {
                        lexemes[i].Children = syntaxInfo.Lexemes;
                    }
                    else
                    {
                        returnInfo.Errors = syntaxInfo.Errors;
                        break;
                    }
                }

                //If the current lexeme is a comma or the current lexeme is at the end of a comma series
                if (lexemes[i].Input == commaSyntaxWord.Input ||
                    (hasCommaSyntax && syntaxParent != null && i + 1 == lexemes.Count) ||
                    (i - indexOfLastComma == amountOfChildrenInLastComma && syntaxParent == null && (i + 1 >= lexemes.Count || lexemes[i + 1].Input != commaSyntaxWord.Input)))
                {
                    SyntaxWord newCommaSyntax = new SyntaxWord(commaSyntaxWord.StartingSyntax, commaSyntaxWord.CloseingSyntaxList, commaSyntaxWord.EndLineIsCloseingSyntax, commaSyntaxWord.FillWordsInsideSyntax, commaSyntaxWord.AllWordType, commaSyntaxWord.ParseSyntax, commaSyntaxWord.ChildrenRanges);

                    hasCommaSyntax = true;

                    newCommaSyntax.WordsInsideSyntax = new List<IWord>();

                    bool setCommaIndex = false;//if this lexeme is a comma
                    if (lexemes[i].Input == commaSyntaxWord.Input)
                    {
                        setCommaIndex = true;
                    }

                    //Put the lexemes in newCommaSyntax and remove them from the original lexemes list
                    if (i > 0 && setCommaIndex)
                    {
                        newCommaSyntax.WordsInsideSyntax.Add(lexemes[i - 1]);
                        lexemes.RemoveAt(i - 1);
                        i = i - 1;
                    }
                    else if (!setCommaIndex)
                    {
                        newCommaSyntax.WordsInsideSyntax.Add(lexemes[i]);
                    }
                    else
                    {
                        returnInfo.Errors.Add("Incorect Comma Syntax");
                        break;
                    }

                    //if the newCommaSyntax has a parent (Ex: Its in parentheses) then get the lexemes before it and put it in newCommaSyntax
                    int stopLoopingBackwardIndex = 0;
                    if (syntaxParent == null)
                    {
                        stopLoopingBackwardIndex = lastIndexNotInComma;
                    }
                    //loops backwards through the lexemes until it finds a language or comma lexeme 
                    for (int j = i - 1; j >= stopLoopingBackwardIndex; j--)
                    {
                        if (lexemes[j].WordType == WordTypes.Language && ((SyntaxWord)lexemes[j]).StartingSyntax == commaSyntaxWord.StartingSyntax)
                        {
                            break;
                        }
                        else
                        {
                            //Adds the current lexeme to the newComma Syntax and removes it from the original list
                            i = j;
                            newCommaSyntax.WordsInsideSyntax.Add(lexemes[j]);
                            lexemes.RemoveAt(j);
                        }
                    }
                    //Reverses the lexemes in newCommaSyntax because they were inserted backwards
                    newCommaSyntax.WordsInsideSyntax.Reverse();

                    lexemes[i] = newCommaSyntax;
                    amountOfChildrenInLastComma = lexemes[i].Children.Count;

                    //if this lexeme was a comma then set indexOfLastComma to i
                    if (setCommaIndex)
                    {
                        indexOfLastComma = i;
                    }
                }
                else
                {
                    lastIndexNotInComma = i + 1;
                }
            }
            returnInfo.Lexemes = lexemes;
            return returnInfo;
        }


        /// <summary>
        /// Adds a item to a list at a certain index
        /// </summary>
        /// <param name="list">The list to be added to</param>
        /// <param name="item">The item being added to the list</param>
        /// <param name="index">The index that the item will be added at</param>
        /// <returns>The list with the item added at the index</returns>
        List<IWord> AddAtIndex(List<IWord> list, IWord item, int index)
        {
            List<IWord> retrunList = new List<IWord>();

            //Clamps index
            if (index >= list.Count)
            {
                index = list.Count - 1;
            }
            else if (index < 0)
            {
                index = 0;
            }

            //Adds every item from list to retrunList and adds item
            for (int i = 0; i < list.Count; i++)
            {
                if (i == index)
                {
                    retrunList.Add(item);
                }
                retrunList.Add(list[i]);
            }
            return retrunList;
        }

        /// <summary>
        /// Handles both math and logic opperations in the lexemes
        /// </summary>
        /// <param name="lexemes">The lexemes that the input has been broken up into from comma comiler</param>
        /// <returns>The new lexemes and any syntax issues</returns>
        CompilerInfo OperationCompiler(List<IWord> lexemes)
        {
            CompilerInfo returnInfo = new CompilerInfo() { Lexemes = lexemes, Errors = new List<string>() };
            //loops throught the lexemes to find the opperations
            for (int i = returnInfo.Lexemes.Count - 1; i >= 0; i--)
            {
                //if the current lexeme is an opperation lexeme then find the rest of the opperation and change this lexeme to an opperation
                if (returnInfo.Lexemes[i].WordType == WordTypes.Language)
                {
                    SyntaxWord syntaxLexeme = (SyntaxWord)returnInfo.Lexemes[i];
                    if (logicOpperationChars.Contains(syntaxLexeme.StartingSyntax) || mathOpperationChars.Contains(syntaxLexeme.StartingSyntax))
                    {
                        string fullOperation = syntaxLexeme.Input;
                        //loops forward in the lexemes to get the fullOperation 
                        for (int j = i + 1; j < returnInfo.Lexemes.Count; j++)
                        {
                            if (returnInfo.Lexemes[j].WordType == WordTypes.Language && (logicOpperationChars.Contains(((SyntaxWord)returnInfo.Lexemes[j]).StartingSyntax) || mathOpperationChars.Contains(((SyntaxWord)returnInfo.Lexemes[j]).StartingSyntax)))
                            {
                                fullOperation += returnInfo.Lexemes[j].Input;
                                returnInfo.Lexemes.RemoveAt(j);
                                j = j - 1;
                            }
                            else
                            {
                                break;
                            }
                        }

                        //Sets the current lexeme to an opperation
                        if (logicOperationDictionary.Keys.Contains(fullOperation))
                        {
                            IWord leftWord = null;
                            IWord rightWord = null;
                            if (i > 0 && i + 1 < returnInfo.Lexemes.Count)
                            {
                                leftWord = returnInfo.Lexemes[i - 1];
                                rightWord = returnInfo.Lexemes[i + 1];
                                returnInfo.Lexemes.RemoveAt(i + 1);
                                returnInfo.Lexemes.RemoveAt(i - 1);
                                i = i - 1;
                            }
                            else
                            {
                                returnInfo.Errors.Add("Must be something to left or right of opperation");
                                break;
                            }
                            returnInfo.Lexemes[i] = new LogicOperationWord(leftWord, rightWord, new List<WordRange>(), logicOperationDictionary[fullOperation]);
                        }
                        else if (mathOpperationDictionary.Keys.Contains(fullOperation))
                        {
                            returnInfo.Lexemes[i] = new MathOperationWord(fullOperation, mathOpperationDictionary[fullOperation].MathFunction, mathOpperationDictionary[fullOperation].ParseSyntax);
                        }
                        else
                        {
                            returnInfo.Errors.Add("Incorect use of " + fullOperation);
                            break;
                        }
                    }
                    //if the current lexeme is syntax (comma or parentheses) handle the opperations in their children
                    else
                    {
                        CompilerInfo syntaxChildenInfo = OperationCompiler(syntaxLexeme.WordsInsideSyntax);
                        if (returnInfo.Errors.Count == 0)
                        {
                            syntaxLexeme.WordsInsideSyntax = syntaxChildenInfo.Lexemes;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            return returnInfo;
        }
        #endregion

        #region KeywordSyntaxStage
        public KeywordsInfo KeywordSyntaxCompiler(List<IWord> lexemes)
        {
            KeywordsInfo returnInfo = new KeywordsInfo() { Keywords = new List<Keyword>(), Errors = new List<string>() };
            bool hasFoundAKeyword = false;
            for (int i = 0; i < lexemes.Count; i++)
            {
                if (keywordDictionary.ContainsKey(lexemes[i].Input.ToLower()))
                {
                    Keyword currentKeyword = new Keyword(keywordDictionary[lexemes[i].Input.ToLower()], new List<IWord>(), null);
                    for (int j = i + 1; j < lexemes.Count; j++)
                    {
                        if (keywordDictionary.ContainsKey(lexemes[j].Input.ToLower()))
                        {
                            i = j - 1;
                            break;
                        }
                        else if (functionDictionary.ContainsKey(lexemes[j].Input.ToLower()))
                        {
                            FunctionWord currentFunction;
                            if (j + 1 < lexemes.Count && lexemes[j + 1].AllWordType == AllWordTypes.Parentheses)
                            {
                                currentFunction = new FunctionWord(new List<IWord>() { lexemes[j + 1] }, functionDictionary[lexemes[j].Input.ToLower()], null);
                                lexemes.RemoveAt(j + 1);
                            }
                            else
                            {
                                currentFunction = new FunctionWord(new List<IWord>(), functionDictionary[lexemes[j].Input.ToLower()], null);
                            }
                            currentKeyword.KeywordParameters.Add(currentFunction);
                            lexemes.RemoveAt(j);
                            j = j - 1;
                        }
                        else
                        {
                            currentKeyword.KeywordParameters.Add(lexemes[j]);
                            lexemes.RemoveAt(j);
                            j = j - 1;
                        }
                    }
                    List<WordRange> rangesThatWorked;
                    CompilerInfo keywordChildrenInfo = ParseChildren(currentKeyword.Children, currentKeyword.ChildrenRanges, out rangesThatWorked);
                    if (keywordChildrenInfo.Errors.Count == 0)
                    {
                        currentKeyword.Children = keywordChildrenInfo.Lexemes;
                        currentKeyword.RangesThatWorked = rangesThatWorked;
                        returnInfo.Keywords.Add(currentKeyword);
                    }
                    else
                    {
                        returnInfo.Errors.Add("Incorect use of the Keyoword " + currentKeyword.Input.ToUpper());
                        break;
                    }
                }
                else if (!hasFoundAKeyword)
                {
                    returnInfo.Errors.Add(lexemes[i].Input + " is not a Keyword");
                    break;
                }
            }
            return returnInfo;
        }

        public ParseSyntaxInfo ParseCustomSyntax(IWord wordToParse, IWord wordInstructions)
        {
            ParseSyntaxInfo returnInfo = new ParseSyntaxInfo() { Word = wordToParse, Errors = new List<string>() };
            if (wordToParse.ChildrenRanges.Count == 0 && wordToParse.AllWordType != AllWordTypes.Syntax)
            {
                if (allWordDictionary.ContainsKey(wordInstructions.AllWordType) && allWordDictionary[wordInstructions.AllWordType].WordDictionary != null)
                {
                    if (allWordDictionary[wordInstructions.AllWordType].DoToLower)
                    {
                        if (!wordInstructions.Initializing)
                        {
                            if (allWordDictionary[wordInstructions.AllWordType].WordDictionary.ContainsKey(returnInfo.Word.Input.ToLower()))
                            {
                                object possibleReturnWord = allWordDictionary[wordInstructions.AllWordType].WordDictionary[returnInfo.Word.Input.ToLower()];
                                if (allWordDictionary[wordInstructions.AllWordType].ContainsListOfIWords)
                                {
                                    if (possibleReturnWord is List<IWord>)
                                    {
                                        List<IWord> returnWordList = new List<IWord>((List<IWord>)possibleReturnWord);
                                        if (returnWordList.Count == 1)
                                        {
                                            returnInfo.Word = returnWordList[0];
                                        }
                                        else if (returnWordList.Count > 1)
                                        {
                                            returnInfo.Word = returnWordList[0];
                                            returnWordList.RemoveAt(0);
                                            returnInfo.Word.WordsThisCouldBe = returnWordList;
                                        }
                                        else
                                        {
                                            returnInfo.Errors.Add("Word doesn't correspond with a custom value");
                                        }
                                    }
                                    else
                                    {
                                        returnInfo.Errors.Add("Word isn't an IWord");
                                    }
                                }
                                else
                                {
                                    if (possibleReturnWord is IWord)
                                    {
                                        returnInfo.Word = (IWord)possibleReturnWord;
                                    }
                                    else
                                    {
                                        returnInfo.Errors.Add("Word isn't an IWord");
                                    }
                                }
                            }
                            else
                            {
                                returnInfo.Errors.Add("Word doesn't correspond with a custom value");
                            }
                        }
                        else
                        {
                            if (allWordDictionary[wordInstructions.AllWordType].ContainsListOfIWords)
                            {
                                wordInstructions.Input = returnInfo.Word.Input.ToLower();
                                returnInfo.Word = wordInstructions;
                                if (allWordDictionary[wordInstructions.AllWordType].WordDictionary.ContainsKey(returnInfo.Word.Input.ToLower()))
                                {
                                    ((List<IWord>)allWordDictionary[wordInstructions.AllWordType].WordDictionary[returnInfo.Word.Input.ToLower()]).Add(returnInfo.Word);
                                }
                                else
                                {
                                    allWordDictionary[wordInstructions.AllWordType].WordDictionary[returnInfo.Word.Input.ToLower()] = new List<IWord>() { returnInfo.Word };
                                }
                            }
                            else
                            {
                                if (!allWordDictionary[wordInstructions.AllWordType].WordDictionary.ContainsKey(returnInfo.Word.Input.ToLower()))
                                {
                                    wordInstructions.Input = returnInfo.Word.Input.ToLower();
                                    returnInfo.Word = wordInstructions;
                                    allWordDictionary[wordInstructions.AllWordType].WordDictionary[returnInfo.Word.Input.ToLower()] = returnInfo.Word;
                                }
                                else
                                {
                                    returnInfo.Errors.Add("Word already has a custom value");
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!wordInstructions.Initializing)
                        {
                            if (allWordDictionary[wordInstructions.AllWordType].WordDictionary.ContainsKey(returnInfo.Word.Input))
                            {
                                object possibleReturnWord = allWordDictionary[wordInstructions.AllWordType].WordDictionary[returnInfo.Word.Input];
                                if (allWordDictionary[wordInstructions.AllWordType].ContainsListOfIWords)
                                {
                                    if (possibleReturnWord is List<IWord>)
                                    {
                                        List<IWord> returnWordList = new List<IWord>((List<IWord>)possibleReturnWord);
                                        if (returnWordList.Count == 1)
                                        {
                                            returnInfo.Word = returnWordList[0];
                                        }
                                        else if (returnWordList.Count > 1)
                                        {
                                            returnInfo.Word = returnWordList[0];
                                            returnWordList.RemoveAt(0);
                                            returnInfo.Word.WordsThisCouldBe = returnWordList;
                                        }
                                        else
                                        {
                                            returnInfo.Errors.Add("Word doesn't correspond with a custom value");
                                        }
                                    }
                                    else
                                    {
                                        returnInfo.Errors.Add("Word isn't an IWord");
                                    }
                                }
                                else
                                {
                                    if (possibleReturnWord is IWord)
                                    {
                                        returnInfo.Word = (IWord)possibleReturnWord;
                                    }
                                    else
                                    {
                                        returnInfo.Errors.Add("Word isn't an IWord");
                                    }
                                }
                            }
                            else
                            {
                                returnInfo.Errors.Add("Word doesn't correspond with a custom value");
                            }
                        }
                        else
                        {
                            if (allWordDictionary[wordInstructions.AllWordType].ContainsListOfIWords)
                            {
                                wordInstructions.Input = returnInfo.Word.Input;
                                returnInfo.Word = wordInstructions;
                                if (allWordDictionary[wordInstructions.AllWordType].WordDictionary.ContainsKey(returnInfo.Word.Input))
                                {
                                    ((List<IWord>)allWordDictionary[wordInstructions.AllWordType].WordDictionary[returnInfo.Word.Input]).Add(returnInfo.Word);
                                }
                                else
                                {
                                    allWordDictionary[wordInstructions.AllWordType].WordDictionary[returnInfo.Word.Input] = new List<IWord>() { returnInfo.Word };
                                }
                            }
                            else
                            {
                                if (!allWordDictionary[wordInstructions.AllWordType].WordDictionary.ContainsKey(returnInfo.Word.Input))
                                {
                                    wordInstructions.Input = returnInfo.Word.Input;
                                    returnInfo.Word = wordInstructions;
                                    allWordDictionary[wordInstructions.AllWordType].WordDictionary[returnInfo.Word.Input] = returnInfo.Word;
                                }
                                else
                                {
                                    returnInfo.Errors.Add("Word already has a custom value");
                                }
                            }
                        }
                    }
                }
                else
                {
                    returnInfo.Errors.Add("No dictionary for type");
                }
            }
            else
            {
                returnInfo.Errors.Add("Word shouldn't have children");
            }
            return returnInfo;
        }

        public ParseSyntaxInfo ParseToWordType(IWord wordToParse, IWord wordInstructions)
        {
            ParseSyntaxInfo returnInfo = new ParseSyntaxInfo() { Word = wordToParse, Errors = new List<string>() };

            if (wordToParse.AllWordType != wordInstructions.AllWordType && wordToParse.ChildrenRanges.Count == 0)
            {
                returnInfo.Errors.Add("Word is not the correct type");
            }
            return returnInfo;
        }

        public ParseSyntaxInfo ParseSyntaxWithChildren(IWord wordToParse, IWord wordInstructions)
        {
            ParseSyntaxInfo returnInfo = new ParseSyntaxInfo() { Word = wordToParse, Errors = new List<string>() };

            if (returnInfo.Word.AllWordType == wordInstructions.AllWordType)
            {
                List<WordRange> rangesThatWorked;
                CompilerInfo childrenInfo = ParseChildren(wordToParse.Children, wordInstructions.ChildrenRanges, out rangesThatWorked);
                if (childrenInfo.Errors.Count == 0)
                {
                    returnInfo.Word.Children = childrenInfo.Lexemes;
                    returnInfo.Word.RangesThatWorked = rangesThatWorked;
                }
                else
                {
                    returnInfo.Errors.AddRange(childrenInfo.Errors);
                }
            }
            else
            {
                returnInfo.Errors.Add("Word is not the correct type");
            }

            return returnInfo;
        }

        public ParseSyntaxInfo ParseFunction(IWord wordToParse, IWord wordInstructions)
        {
            ParseSyntaxInfo returnInfo = new ParseSyntaxInfo() { Word = wordToParse, Errors = new List<string>() };

            if (returnInfo.Word.AllWordType == wordInstructions.AllWordType)
            {
                List<WordRange> rangesThatWorked;
                CompilerInfo childrenInfo = ParseChildren(wordToParse.Children, wordToParse.ChildrenRanges, out rangesThatWorked);
                if (childrenInfo.Errors.Count == 0)
                {
                    returnInfo.Word.Children = childrenInfo.Lexemes;
                    returnInfo.Word.RangesThatWorked = rangesThatWorked;
                }
                else
                {
                    returnInfo.Errors.AddRange(childrenInfo.Errors);
                }
            }
            else
            {
                returnInfo.Errors.Add("Word is not the correct type");
            }

            return returnInfo;
        }

        public CompilerInfo ParseChildren(List<IWord> wordsToParse, List<List<WordRange>> childrenInstructions, out List<WordRange> rangesThatWorked)
        {
            CompilerInfo returnInfo = new CompilerInfo() { Lexemes = wordsToParse, Errors = new List<string>() };
            bool followsAListOfRanges = false;
            List<IWord> possibleParsedWords = returnInfo.Lexemes;
            rangesThatWorked = null;
            foreach (List<WordRange> ranges in childrenInstructions)
            {
                bool followsThisListOfRanges = false;
                int wordsToParseIndex = 0;
                possibleParsedWords = new List<IWord>(returnInfo.Lexemes);
                foreach (WordRange range in ranges)
                {
                    bool followsThisRange = true;

                    int startingWordToParseIndex = wordsToParseIndex;
                    if (possibleParsedWords.Count - wordsToParseIndex >= range.Min)
                    {
                        for (int i = wordsToParseIndex; i < possibleParsedWords.Count; i++)
                        {
                            if (i + 1 - startingWordToParseIndex < range.Max)
                            {
                                wordsToParseIndex = i + 1;
                                ParseSyntaxInfo lexemeInfo = range.TypeOfWord.ParseSyntax.Invoke(possibleParsedWords[i], range.TypeOfWord);
                                if (lexemeInfo.Errors.Count == 0)
                                {
                                    possibleParsedWords[i] = lexemeInfo.Word;
                                }
                                else
                                {
                                    followsThisRange = false;
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (followsThisRange && wordsToParseIndex - startingWordToParseIndex >= range.Min)
                        {
                            followsThisListOfRanges = true;
                            continue;
                        }
                        else
                        {
                            followsThisListOfRanges = false;
                            break;
                        }
                    }
                    else
                    {
                        followsThisListOfRanges = false;
                        break;
                    }
                }
                if (followsThisListOfRanges && wordsToParseIndex == possibleParsedWords.Count)
                {
                    rangesThatWorked = ranges;
                    followsAListOfRanges = true;
                    break;
                }
            }

            if (followsAListOfRanges)
            {
                returnInfo.Lexemes = possibleParsedWords;
            }
            else
            {
                returnInfo.Errors.Add("Doesn't follow keyword");
            }
            return returnInfo;
        }

        public ParseSyntaxInfo ParseCustomCustomSyntax(IWord wordToParse, IWord wordInstructions)
        {
            CustomCustomWord returnWord = new CustomCustomWord(wordToParse.Input, null, null, ParseCustomCustomSyntax);
            ParseSyntaxInfo returnInfo = new ParseSyntaxInfo() { Word = wordToParse, Errors = new List<string>() };
            if (wordToParse.Input == quoteSyntaxWord.Input && wordToParse.Children.Count == 1)
            {
                if (wordToParse.Children[0].Children.Count > 0)
                {
                    returnInfo.Errors.Add("Custom Custom Words can't have Children");
                }
                else
                {
                    returnWord.Input = wordToParse.Children[0].Input;
                    returnWord.Data = wordToParse.Children[0].Input;
                    returnWord.VarType = typeof(string);
                }
            }
            else if (wordToParse.Children.Count > 0)
            {
                returnInfo.Errors.Add("Custom Custom Words can't have Children");
            }
            else
            {
                bool possibleBoolValue;
                int possibleIntValue;
                if (bool.TryParse(returnInfo.Word.Input, out possibleBoolValue))
                {
                    returnWord.Data = possibleBoolValue;
                    returnWord.VarType = typeof(bool);
                }
                else if (typeDictionary.ContainsKey(returnInfo.Word.Input))
                {
                    returnWord.Data = typeDictionary[returnInfo.Word.Input].Name;
                    returnWord.VarType = typeof(Type);
                }
                else if (int.TryParse(wordToParse.Input, out possibleIntValue))
                {
                    returnWord.Data = possibleIntValue;
                    returnWord.VarType = typeof(int);
                }
                else
                {
                    returnInfo.Errors.Add("Strings have to be in quotes");
                }

            }
            if (wordInstructions.AllWordType == AllWordTypes.CustomCustom && returnInfo.Errors.Count == 0)
            {
                CustomCustomWord customCustomInstructions = (CustomCustomWord)wordInstructions;
                if (customCustomInstructions.VarType != null)
                {
                    if (customCustomInstructions.VarType != returnWord.VarType)
                    {
                        returnInfo.Errors.Add("Input has to be a " + customCustomInstructions.VarType);
                    }
                }
            }
            returnInfo.Word = returnWord;
            return returnInfo;
        }

        public ParseSyntaxInfo ParseLogicOpperationSyntax(IWord wordToParse, IWord wordInstructions)
        {
            ParseSyntaxInfo returnInfo = new ParseSyntaxInfo() { Word = wordToParse, Errors = new List<string>() };

            ParseSyntaxInfo childrenInfo = ParseSyntaxWithChildren(wordToParse, wordInstructions);

            if (childrenInfo.Errors.Count == 0)
            {
                returnInfo.Word = childrenInfo.Word;
                Type varType = null;
                for (int i = 0; i < returnInfo.Word.Children.Count; i++)
                {
                    if (i == 0)
                    {
                        varType = returnInfo.Word.Children[i].VarType;
                    }
                    else if (varType != returnInfo.Word.Children[i].VarType)
                    {
                        returnInfo.Errors.Add(returnInfo.Word.Children[i].Input + " is not a " + returnInfo.Word.Children[i].VarType.Name);
                        break;
                    }
                }
            }
            else
            {
                returnInfo.Errors.AddRange(childrenInfo.Errors);
            }
            return returnInfo;
        }
        #endregion

        public CommandsInfo CommandGroupCompiler(List<Keyword> keywords)
        {
            CommandsInfo returnInfo = new CommandsInfo() { Commands = new List<Command>(), Errors = new List<string>() };
            List<Keyword> currentKeywords = new List<Keyword>(keywords);
            List<Keyword> nextKeywords = new List<Keyword>();
            List<Keyword> lastNextKeywords = new List<Keyword>();
            while (currentKeywords.Count > 0)
            {
                for (int i = currentKeywords.Count - 1; i >= 0; i--)
                {
                    if (currentKeywords[i].AllWordType == AllWordTypes.Keyword)
                    {
                        Command possibleCommand = commandTrie.GetCommand(currentKeywords, commandTrie.BaseNode, 0);
                        if (possibleCommand != null)
                        {
                            returnInfo.Commands.Add(possibleCommand);
                            break;
                        }
                        else
                        {
                            nextKeywords.Add(currentKeywords[i]);
                            currentKeywords.RemoveAt(i);
                        }
                    }
                    else
                    {
                        returnInfo.Errors.Add("Not Given Keywords");
                    }
                }
                if (lastNextKeywords.Count > 0 && EqualLists(nextKeywords, lastNextKeywords))
                {
                    returnInfo.Errors.Add("Not a Full Command");
                    break;
                }
                else
                {
                    currentKeywords = nextKeywords;
                    lastNextKeywords = new List<Keyword>(nextKeywords);
                    currentKeywords.Reverse();
                    nextKeywords = new List<Keyword>();
                }
            }
            if (returnInfo.Commands.Count == 0 && returnInfo.Errors.Count == 0)
            {
                returnInfo.Errors.Add("Not a Full Command");
            }
            return returnInfo;
        }

        public bool EqualLists(List<Keyword> list1, List<Keyword> list2)
        {
            bool returnBool = true;
            if (list1.Count == list2.Count)
            {
                for (int i = 0; i < list1.Count; i++)
                {
                    if (list1[i] != list2[i])
                    {
                        return false;
                    }
                }
            }
            else
            {
                returnBool = false;
            }
            return returnBool;
        }

        public CommandsInfo CommandKeywordCompiler(List<Command> commands)
        {
            CommandsInfo returnInfo = new CommandsInfo() { Commands = commands, Errors = new List<string>() };

            foreach (Command command in returnInfo.Commands)
            {
                command.GetCustomWordsInCommand();

                if (command.TablesInCommand.Count > 1)
                {
                    foreach (ColumnWord column in command.ColumnsInCommand)
                    {
                        if (column.Input.Contains(dotSyntaxWord.StartingSyntax) && column.WordsThisCouldBe.Count == 0)
                        {
                            bool columnInATable = false;
                            foreach (TableWord table in command.TablesInCommand)
                            {
                                if (column.OwningTable == table.TableDirectory)
                                {
                                    columnInATable = true;
                                    break;
                                }
                            }
                            if (!columnInATable)
                            {
                                returnInfo.Errors.Add(column.Input + " is not in the tables");
                                break;
                            }
                        }
                        else
                        {
                            returnInfo.Errors.Add("Must specify table name");
                            break;
                        }
                    }
                }
                else if (command.TablesInCommand.Count > 0)
                {
                    TableWord table = command.TablesInCommand[0];
                    for (int i = 0; i < command.ColumnsInCommand.Count; i++)
                    {
                        ColumnWord column = command.ColumnsInCommand[i];
                        if (column.WordsThisCouldBe.Count > 0)
                        {
                            ColumnWord actualColumn = null;
                            for (int j = 0; j < column.WordsThisCouldBe.Count + 1; j++)
                            {
                                ColumnWord currentColumn;
                                if (j >= column.WordsThisCouldBe.Count)
                                {
                                    currentColumn = new ColumnWord(column);
                                }
                                else
                                {
                                    if (column.WordsThisCouldBe[j].AllWordType == AllWordTypes.Column)
                                    {
                                        currentColumn = (ColumnWord)column.WordsThisCouldBe[j];
                                    }
                                    else
                                    {
                                        returnInfo.Errors.Add(column.WordsThisCouldBe[j].Input + " is not a column");
                                        break;
                                    }
                                }
                                if (currentColumn.OwningTable == table.TableDirectory)
                                {
                                    currentColumn.WordsThisCouldBe = new List<IWord>();
                                    actualColumn = currentColumn;
                                    break;
                                }
                            }
                            if (returnInfo.Errors.Count > 0)
                            {
                                break;
                            }
                            else if (actualColumn == null)
                            {
                                returnInfo.Errors.Add(column.Input + " is not in " + table.Input);
                                break;
                            }
                            else
                            {
                                column = actualColumn;
                            }
                        }
                        else
                        {
                            if (column.OwningTable != table.TableDirectory)
                            {
                                returnInfo.Errors.Add(column.Input + " is not in " + table.Input);
                                break;
                            }
                        }
                        command.ColumnsInCommand[i] = column;
                    }
                }
                else if (command.ColumnsInCommand.Count > 0)
                {
                    returnInfo.Errors.Add("Expecting a table");
                    break;
                }
            }

            return returnInfo;
        }

        public CommandsInfo CommandCustomCustomCompiler(List<Command> commands)
        {
            CommandsInfo returnInfo = new CommandsInfo() { Commands = commands, Errors = new List<string>() };

            foreach (Command command in commands)
            {
                if (command.TablesInCommand.Count == 1)
                {
                    TableWord table = command.TablesInCommand[0];
                    List<List<CustomCustomWord>> customCustomWordSets = new List<List<CustomCustomWord>>();

                    for (int i = 0; i < command.CustomCustomsInCommand.Count; i++)
                    {
                        if (i % table.TableDirectory.SqlColumns.Count == 0)
                        {
                            customCustomWordSets.Add(new List<CustomCustomWord>());
                        }
                        customCustomWordSets.Last().Add(command.CustomCustomsInCommand[i]);
                        if (i + 1 >= command.CustomCustomsInCommand.Count)
                        {
                            if (customCustomWordSets.Last().Count != table.TableDirectory.SqlColumns.Count)
                            {
                                returnInfo.Errors.Add(table.Input + " Expects " + table.TableDirectory.SqlColumns.Count + " Inputs");
                                break;
                            }
                        }
                    }
                    if (returnInfo.Errors.Count > 0)
                    {
                        break;
                    }
                    foreach (List<CustomCustomWord> customCustomWordSet in customCustomWordSets)
                    {
                        bool followsSet = true;
                        for (int i = 0; i < customCustomWordSet.Count; i++)
                        {
                            if (customCustomWordSet[i].VarType != table.TableDirectory.SqlColumns[i].VarType)
                            {
                                followsSet = false;
                                break;
                            }
                        }
                        if (!followsSet)
                        {
                            returnInfo.Errors.Add(table.Input + " Expects Different Inputs");
                            break;
                        }
                    }
                    if (returnInfo.Errors.Count > 0)
                    {
                        break;
                    }
                }
                else if (command.CustomCustomsInCommand.Count > 0)
                {
                    returnInfo.Errors.Add("Command Expects one Table");
                    break;
                }
            }
            return returnInfo;
        }

        Dictionary<AllWordTypes, AllWordDictionaryInfo> RemoveInitializingWordsFromDictionaries(Dictionary<AllWordTypes, AllWordDictionaryInfo> dictionary)
        {
            Dictionary<AllWordTypes, AllWordDictionaryInfo> returnDictionary = new Dictionary<AllWordTypes, AllWordDictionaryInfo>(dictionary);

            foreach (KeyValuePair<AllWordTypes, AllWordDictionaryInfo> dictionaryInfo in dictionary)
            {
                returnDictionary[dictionaryInfo.Key] = RemoveInitializingWordsFromDictionary(returnDictionary[dictionaryInfo.Key]);
            }

            return returnDictionary;
        }
        AllWordDictionaryInfo RemoveInitializingWordsFromDictionary(AllWordDictionaryInfo dictionaryInfo)
        {
            AllWordDictionaryInfo returnInfo = new AllWordDictionaryInfo() { WordDictionary = new Hashtable(dictionaryInfo.WordDictionary), ContainsListOfIWords = dictionaryInfo.ContainsListOfIWords, DoToLower = dictionaryInfo.DoToLower };
            foreach (string key in dictionaryInfo.WordDictionary.Keys)
            {
                if (returnInfo.ContainsListOfIWords)
                {
                    List<IWord> words = (List<IWord>)returnInfo.WordDictionary[key];
                    for (int i = 0; i < words.Count; i++)
                    {
                        if (words[i].Initializing)
                        {
                            words.RemoveAt(i);
                            i = i - 1;
                        }
                    }
                    if (words.Count == 0)
                    {
                        returnInfo.WordDictionary.Remove(key);
                    }
                }
                else
                {
                    IWord word = (IWord)returnInfo.WordDictionary[key];
                    if (word.Initializing)
                    {
                        returnInfo.WordDictionary.Remove(key);
                    }
                }
            }
            return returnInfo;
        }


        #region CommandFuctions
        public ICommandReturn SelectFunction(ICommandReturn commandReturn, List<IWord> words, Command command)
        {
            Table returnTable;
            List<List<SqlRow>> allRows = new List<List<SqlRow>>();
            int amountOfRows = 0;
            for (int i = 0; i < command.TablesInCommand.Count; i++)
            {
                allRows.Add(command.TablesInCommand[i].TableDirectory.Select());
                if (amountOfRows == 0)
                {
                    amountOfRows = allRows.Last().Count;
                }
                else
                {
                    amountOfRows *= allRows.Last().Count;
                }
            }

            List<SqlColumn> returnTableColumns = new List<SqlColumn>();
            //List<List<List<IComparable>>> rowsCells = new List<List<List<IComparable>>>();
            List<IComparable> emptyValuesList = new List<IComparable>();
            for (int i = 0; i < words.Count - 1; i++)
            {
                SqlColumn currentColumn = ((ColumnWord)words[i]).ColumnDirectory;
                returnTableColumns.Add(currentColumn);
                emptyValuesList.Add(new EmptyICompareable());
            }
            IComparable[] emptyValues = emptyValuesList.ToArray();
            returnTable = new Table("", returnTableColumns.ToArray());
            SqlRow[] tableRows = new SqlRow[amountOfRows];
            
            for (int i = 0; i < returnTableColumns.Count; i++)
            {
                int tableRowIndex = 0;
                List<SqlRow> currentSelect = returnTableColumns[i].OwningTable.Select();
                foreach (SqlRow row in currentSelect)
                {
                    int columnIndex = row.GetColumnIndex(returnTableColumns[i]);
                    if (columnIndex >= 0)
                    {
                        SqlRow currentRow;
                        if (tableRows[tableRowIndex] == null)
                        {
                            currentRow = returnTable.AddRow(emptyValues);
                        }
                        else
                        {
                            currentRow = tableRows[tableRowIndex];
                        }
                        currentRow.Cells[i].Value = row.Cells[columnIndex].Value;
                        tableRows[tableRowIndex] = currentRow;
                        tableRowIndex++;
                    }

                }
            }

            //for (int i = 0; i < allRows[0].Count; i++)
            //{
            //    List<SqlCell> fistTableCells = new List<SqlCell>();
            //    foreach (SqlColumn column in returnTableColumns)
            //    {
            //        int columnIndex = allRows[0][i].GetColumnIndex(column);
            //        if (columnIndex >= 0)
            //        {
            //            fistTableCells.Add(allRows[0][i].Cells[columnIndex]);
            //        }
            //    }
            //    if (allRows.Count > 1)
            //    {
            //        for (int j = 1; j < allRows.Count; j++)
            //        {
            //            foreach (SqlRow row in allRows[j])
            //            {
            //                List<SqlCell> thisRowCells = new List<SqlCell>(fistTableCells);
            //                foreach (SqlColumn column in returnTableColumns)
            //                {
            //                    int columnIndex = row.GetColumnIndex(column);
            //                    if (columnIndex >= 0)
            //                    {
            //                        thisRowCells.Add(row.Cells[columnIndex]);
            //                    }
            //                }

            //                returnTable.AddRow(thisRowCells);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        returnTable.AddRow(fistTableCells);
            //    }
            //}

            return returnTable;
        }
        public ICommandReturn WhereFunction(ICommandReturn commandReturn, List<IWord> words, Command command)
        {
            return null;
        }
        public ICommandReturn InsertFunction(ICommandReturn commandReturn, List<IWord> words, Command command)
        {
            return null;
        }
        public ICommandReturn CreateTableFunction(ICommandReturn commandReturn, List<IWord> words, Command command)
        {
            return null;
        }
        public ICommandReturn UpdateFunction(ICommandReturn commandReturn, List<IWord> words, Command command)
        {
            return null;
        }
        public ICommandReturn JoinFunction(ICommandReturn commandReturn, List<IWord> words, Command command)
        {
            return null;
        }
        #endregion
    }
    public struct CommandsInfo
    {
        public List<Command> Commands;
        public List<string> Errors;
        public RenderString SyntaxHighlightedInput;
    }
    public struct KeywordsInfo
    {
        public List<Keyword> Keywords;
        public List<string> Errors;
        public RenderString SyntaxHighlightedInput;
    }
    public struct CompilerInfo
    {
        public List<IWord> Lexemes;
        public List<string> Errors;
        public RenderString SyntaxHighlightedInput;
    }
    public struct OutputInfo
    {
        public string Output;
        public List<string> Errors;
        public RenderString SyntaxHighlightedInput;
    }
    public struct AllWordDictionaryInfo
    {
        public Hashtable WordDictionary;
        public bool DoToLower;
        public bool ContainsListOfIWords;
    }
}


