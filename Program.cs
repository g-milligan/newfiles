using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
namespace g2NewFilesGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            //GET THE ROOT DIRECTORY FOR THE TEMPLATES
            //========================================

            string newfile_templates = "newfile_templates";
            string startEscToken = "|_-+StrtToKen..=!_|"; string endEscToken = "|_--eNdToKen..=!_|";
            //bool foundDir = false;
            //get the current directory of this .exe file
            string currentDirPath = AppDomain.CurrentDomain.BaseDirectory;
            //get the new current directory (up one level)
            string upOneDirPath = currentDirPath;
            string projFolder = upOneDirPath;
            //trim off the trailing "\" if it is there
            if (upOneDirPath.LastIndexOf("\\") == upOneDirPath.Length - 1)
            {
                //trim off "\"
                upOneDirPath = upOneDirPath.Substring(0, upOneDirPath.Length - 1);
                projFolder = upOneDirPath;
            }
            //if upOneDirPath still contains "\"
            if (upOneDirPath.LastIndexOf("\\") != -1)
            {
                //trim off the last folder in the currentDirPath
                upOneDirPath = upOneDirPath.Substring(0, upOneDirPath.LastIndexOf("\\"));
                //get JUST the project folder without the path
                projFolder = projFolder.Substring(projFolder.LastIndexOf("\\") + 1);
            }

            //IF THE ROOT TEMPLATE FOLDER EXISTS
            //==================================

            string templateRootPath = upOneDirPath + "\\" + newfile_templates;
            if (Directory.Exists(templateRootPath))
            {
                //GET A LIST OF DIRECTORIES THAT CONTAIN AT LEAST ONE FILE
                //========================================================

                //detect all of the folder paths that contain files
                List<string> templateOptions = new List<string>();
                List<string> templateOutputOptions = new List<string>();
                loadDirsWithFiles(templateRootPath, ref templateOptions);
                //if there is at least one template
                if (templateOptions.Count > 0)
                {
                    Console.WriteLine("\n  Template files will be copied from " + newfile_templates + " --> " + projFolder + "\n");
                    Console.WriteLine("  Choose template by [number]: ");

                    int chosenTemplateIndex = -1; bool validOption = false;
                    //if there is more than one template option
                    if (templateOptions.Count > 1)
                    {
                        //ALLOW USER TO CHOOSE WHICH TEMPLATE DIRECTORY TO USE
                        //====================================================

                        //for each folder path that contains file(s)
                        for (int t = 0; t < templateOptions.Count; t++)
                        {
                            //show the template option and build the display string for this option
                            showTemplateOption(t, newfile_templates, templateOptions, ref templateOutputOptions);
                        }
                        //allow user to choose template and accept key input
                        Console.Write("\n  Choose template: ");
                        string line = Console.ReadLine(); line = line.Trim();
                        if (int.TryParse(line, out chosenTemplateIndex))
                        {
                            //this IS an int...
                            validOption = true;
                        }
                        else //this is NOT an int
                        {
                            chosenTemplateIndex = -1;
                            Console.WriteLine("  Error, \"" + line + "\" is not an integer.");
                        }
                    }
                    else //only one template option
                    {
                        //just use the first index
                        chosenTemplateIndex = 0;
                        validOption = true;
                        //show the template option and build the display string for this option
                        showTemplateOption(0, newfile_templates, templateOptions, ref templateOutputOptions);
                    }
                    //if input is valid for template option
                    if (validOption)
                    {
                        //if the integer is NOT too high
                        if (chosenTemplateIndex < templateOptions.Count)
                        {
                            //if the integer is NOT too low
                            if (chosenTemplateIndex > -1)
                            {
                                //DISPLAY THE CHOSEN TEMPLATE AND READ THE TOKENS FROM EACH FILE TEMPLATE
                                //=======================================================================

                                Console.Clear();
                                Console.WriteLine("\n  TEMPLATE: \n");
                                Console.WriteLine(templateOutputOptions[chosenTemplateIndex]);
                                Console.WriteLine("\n  ==========================================\n");
                                //Dictionary<[filePath], [originalFileContent]>
                                Dictionary<string, string> pathOriginalContentLookup = new Dictionary<string, string>();
                                //Dictionary<[filePath], List<[tokenText]>>
                                Dictionary<string, List<string>> pathTokensLookup = new Dictionary<string, List<string>>();
                                //Dictionary<[filePath], Dictionary<[tokenAlias], [tokenStr]>>
                                Dictionary<string, Dictionary<string, string>> pathAliasTokenStr = new Dictionary<string, Dictionary<string, string>>();
                                bool atLeastOneToken = false;

                                //LOOP EACH FILE THE FIRST TIME
                                //1) get a dictionary; items are a file's content and file path is an item's key
                                //2) get a dictionary; items are a list of token substrings in the content and file path is an item's key
                                //=============================

                                //get the files in this directory
                                DirectoryInfo dir = new DirectoryInfo(templateOptions[chosenTemplateIndex]);
                                FileInfo[] files = dir.GetFiles();
                                foreach (FileInfo file in files)
                                {
                                    //STORE ORIGINAL FILE CONTENTS AND GET AN ARRAY OF TOKENS FOR THIS FILE
                                    //=====================================================================

                                    //get the file contents
                                    string contents = System.IO.File.ReadAllText(file.FullName);
                                    //escape certain string contents
                                    contents = contents.Replace("\\<<", startEscToken);
                                    contents = contents.Replace("\\>>", endEscToken);
                                    //store the file path and original content
                                    pathOriginalContentLookup.Add(file.FullName, contents);
                                    //store the tokens for this file's content
                                    List<string> tokens = getTokensFromContent(contents);
                                    pathTokensLookup.Add(file.FullName, tokens);
                                    //if there is at least one token in this file
                                    if (tokens.Count > 0)
                                    {
                                        atLeastOneToken = true;

                                        //STORE ALL OF THE TOKEN ALIASES (IF ANY) FOR THIS FILE
                                        //============================================

                                        //for each token in this file
                                        for (int t = 0; t < tokens.Count; t++)
                                        {
                                            //if this token has an alias
                                            string tAlias = getTokenPart("alias", tokens[t]);
                                            if (tAlias != "")
                                            {
                                                //if the name is not blank
                                                string tName = getTokenPart("name", tokens[t]);
                                                if (tName != "")
                                                {
                                                    //if this file path isn't already a key in the pathAliasTokenStr dictionary
                                                    if (!pathAliasTokenStr.ContainsKey(file.FullName))
                                                    {
                                                        //create this path-key in the dictionary, Dictionary<[tokenAlias], [tokenStr]>
                                                        Dictionary<string, string> aliasTokenStr = new Dictionary<string, string>();
                                                        pathAliasTokenStr.Add(file.FullName, aliasTokenStr);
                                                    }
                                                    //if this alias isn't already listed for this file
                                                    if (!pathAliasTokenStr[file.FullName].ContainsKey(tAlias))
                                                    {
                                                        //add the alias/name to the file's listing
                                                        pathAliasTokenStr[file.FullName].Add(tAlias, tokens[t]);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                //Dictionary<[filePath], [changedFileName]>
                                Dictionary<string, string> changedFileNames = new Dictionary<string,string>();
                                //Dictionary<[filePath], [changedFileDir]>
                                Dictionary<string, string> changedFileDirs = new Dictionary<string, string>(); 
                                //if there are any tokens in any of the template files
                                if (atLeastOneToken)
                                {
                                    //LOOP EACH FILE A SECOND TIME
                                    //1) assemble each unique token key name into a list where each key name appears only once 
                                    //============================

                                    string configVarsMsg = "";
                                    configVarsMsg += "  CONFIGURE TEMPLATE VARIABLES: \n\n";
                                    //List<[tokenName]>
                                    List<string> uniqueTokenNames = new List<string>();
                                    //Dictionary<[tokenName], [nameAlias]>
                                    //for each file 
                                    foreach (KeyValuePair<string, List<string>> pathTokensPair in pathTokensLookup)
                                    {
                                        //if this file has any tokens that need to be configured
                                        if (pathTokensPair.Value.Count > 0)
                                        {
                                            //get just the filename without the path
                                            string fileName = pathTokensPair.Key;
                                            if (fileName.IndexOf("\\") != -1)
                                            {
                                                fileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                                            }
                                            configVarsMsg += "  "+fileName + "\n";

                                            //LOOP THROUGH THE TOKENS INSIDE THE FILE
                                            //1) assemble each unique token key name into a list where each key name appears only once 
                                            //=======================================

                                            //for each token that needs to be configured
                                            for (int t = 0; t < pathTokensPair.Value.Count; t++ )
                                            {
                                                //show the tokens that need to be configured
                                                string tokenStr = pathTokensPair.Value[t];
                                                configVarsMsg += " \t" + tokenStr + "\n";
                                                //get just the unique token name (last item in the : separated list)
                                                string uniqueTokenName = getTokenPart("name", tokenStr);
                                                //if this is not a blank token
                                                if (uniqueTokenName != ".")
                                                {
                                                    //if this is NOT a literal token, eg: the file name is "hard-coded.txt"
                                                    if (uniqueTokenName.IndexOf("\"") != 0 && uniqueTokenName.IndexOf("'") != 0)
                                                    {
                                                        //if this token name is not already included in the list
                                                        if (!uniqueTokenNames.Contains(uniqueTokenName))
                                                        {
                                                            //add the unique token name, if not already in the list
                                                            uniqueTokenNames.Add(uniqueTokenName);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    //if there are any unique tokens (Not represented by a dot ., nor a "static value")
                                    if (uniqueTokenNames.Count > 0)
                                    {
                                        //ALLOW USER TO INPUT THEIR OWN TOKEN VALUES
                                        //1) accept an input value for each unique token
                                        //==========================================

                                        //show the configure variables message
                                        Console.WriteLine(configVarsMsg);
                                        //if there is only one token
                                        if (uniqueTokenNames.Count == 1)
                                        {
                                            Console.Write("\n  Just " + uniqueTokenNames.Count + " unique token --> ");
                                        }
                                        else
                                        {
                                            Console.Write("\n  " + uniqueTokenNames.Count + " unique tokens --> ");
                                        }
                                        //list the unique variable names
                                        for (int u = 0; u < uniqueTokenNames.Count; u++)
                                        {
                                            Console.Write("\"" + uniqueTokenNames[u] + "\"");
                                            if (u + 1 != uniqueTokenNames.Count)
                                            {
                                                Console.Write(", ");
                                            }
                                            else
                                            {
                                                Console.WriteLine("\n");
                                            }
                                        }
                                        Console.WriteLine("  -------------------------------------------------------\n");
                                        //for each token, require input
                                        Dictionary<string, string> tokenInputLookup = new Dictionary<string, string>();
                                        for (int i = 0; i < uniqueTokenNames.Count; i++)
                                        {
                                            //get the value for this token from the user
                                            Console.Write("  Enter --> " + uniqueTokenNames[i] + ": ");
                                            string line = Console.ReadLine(); line = line.Trim();
                                            tokenInputLookup.Add(uniqueTokenNames[i], line);
                                        }

                                        Console.WriteLine("\n  OK, got it. Hit any key to build..."); Console.ReadKey();

                                        //LOOP THROUGH TEMPLATE FILES A THIRD TIME
                                        //1) format each unique token's input value
                                        //2) replace the token string with the formatted input value within the file content
                                        //========================================

                                        //for each template file
                                        foreach (KeyValuePair<string, List<string>> pathPair in pathTokensLookup)
                                        {
                                            string filePath = pathPair.Key;
                                            string fileContent = pathOriginalContentLookup[filePath];
                                            //Dictionary<[tokenKey], [tokenInputValue]>
                                            List<string> tokens = pathPair.Value;

                                            //GET FORMATTED VALUES FOR ALIASES AND REMOVE ALIAS DECLARATIONS FROM FILE CONTENTS
                                            //===========================================

                                            //if there are any aliased tokens in this file
                                            Dictionary<string, string> aliasValueLookup = new Dictionary<string, string>();
                                            if (pathAliasTokenStr.ContainsKey(filePath))
                                            {
                                                //for each alias declaration inside the file
                                                Dictionary<string, string> aliasTokens = pathAliasTokenStr[filePath];
                                                foreach (KeyValuePair<string, string> aliasStrPair in aliasTokens)
                                                {
                                                    //if the alias doesn't already have an associated formatted value
                                                    string aliasKey = aliasStrPair.Key;
                                                    if (!aliasValueLookup.ContainsKey(aliasKey))
                                                    {
                                                        string tokenStr = aliasStrPair.Value;

                                                        //remove the tokenStr (alias variable declaration) from fileContent
                                                        fileContent = fileContent.Replace(tokenStr, "");

                                                        //split the token key parts up
                                                        string[] tokenParts = tokenStr.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                                        //get the formatted token value
                                                        string tokenValue = getFormattedTokenValue(tokenParts, tokenInputLookup);
                                                        //associate this value with this alias (only for this file)
                                                        aliasValueLookup.Add(aliasKey, tokenValue);
                                                    }

                                                }
                                            }

                                            //LOOP THROUGH ALL OF THE NON-ALIASED TOKENS IN THIS FILE
                                            //3) NON-ALIASED TOKENS: format each unique token's input value
                                            //4) NON-ALIASED TOKENS: replace the token string with the formatted input value within the file content
                                            //===========================================

                                            //for each token in the file
                                            for (int t = 0; t < tokens.Count; t++)
                                            {
                                                //FORMAT THE TOKEN VALUE DEPENDING ON IT'S PARAMETERS
                                                //=============================================

                                                //get the token key, eg: <<type:casing:name>>
                                                string tokenKey = tokens[t];
                                                //split the token key parts up
                                                string[] tokenParts = tokenKey.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                                //get the formatted token value
                                                string tokenValue = getFormattedTokenValue(tokenParts, tokenInputLookup);

                                                //INSERT THE FORMATTED TOKEN VALUE INTO THE CONTENT (OR SET IT AS A SPECIAL VALUE, LIKE FILENAME, ETC...)
                                                //=======================================================================================================

                                                //get the token name
                                                string tokenName = getTokenPart("name", tokenParts);
                                                //get the token type
                                                string type = getTokenPart("type", tokenParts);
                                                switch (type)
                                                {
                                                    case "var":
                                                        //replace the tokens with the actual values
                                                        fileContent = fileContent.Replace(tokenKey, tokenValue);
                                                        break;
                                                    case "filename": 
                                                        //remove these tokens from the file content
                                                        fileContent = fileContent.Replace(tokenKey, "");
                                                        //if this file doesn't already have a designated changed name
                                                        if (!changedFileNames.ContainsKey(filePath))
                                                        {
                                                            //if there is a specified file name (other than the existing template file's name)
                                                            if (tokenValue != "" && tokenValue != ".")
                                                            {
                                                                //set the new name of the file
                                                                changedFileNames.Add(filePath, tokenValue);
                                                            }
                                                            else
                                                            {
                                                                //no specified file name...

                                                                //if the file name was literally hard-coded (surrounded by "quotes")
                                                                if (tokenName.IndexOf("\"") == 0 || tokenName.IndexOf("'") == 0)
                                                                {
                                                                    //set the static filename value surrounded by "quotes"
                                                                    tokenValue = tokenName;
                                                                    //strip off starting quote
                                                                    tokenValue = tokenValue.Substring(1);
                                                                    //strip off ending quote
                                                                    tokenValue = tokenValue.Substring(0, tokenValue.Length - 1);
                                                                    //trim
                                                                    tokenValue = tokenValue.Trim();
                                                                    //if the literal file name value is not blank
                                                                    if (tokenValue.Length > 0)
                                                                    {
                                                                        //set the static name of the file
                                                                        changedFileNames.Add(filePath, tokenValue);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        //since this token specifies a filename, it may also specify the root directory for the file (if not, changedFileDir = "")...
                                                        //if this file doesn't already have a designated changed sub directory
                                                        if (!changedFileDirs.ContainsKey(filePath))
                                                        {
                                                            //if there is a specified directory in the tokenParts
                                                            string changedDir = getTokenPart("dir", tokenParts);
                                                            if (changedDir != "" && changedDir != ".")
                                                            {
                                                                //replace any aliases with real values that may be inside the directory
                                                                changedDir = getReplacedAliases(changedDir, aliasValueLookup);
                                                                //set the new sub directory of the file
                                                                changedFileDirs.Add(filePath, changedDir);
                                                            }
                                                        }
                                                        break;
                                                    default:
                                                        break;
                                                }
                                                //replace any aliases with real values that may be inside the fileContent
                                                fileContent = getReplacedAliases(fileContent, aliasValueLookup);
                                                //set the modified content into the array
                                                pathOriginalContentLookup[filePath] = fileContent;
                                            }
                                        }
                                    }
                                }

                                //LOOP THROUGH TEMPLATE FILES A FOURTH TIME
                                //1) create a new file based on the corresponding template file (the new file will have real input values instead of tokens)
                                //=========================================

                                Console.WriteLine("  -------------------------------------------------------");
                                //for each file to create
                                int fileCount = 0; int skippedFileCount = 0;
                                foreach (KeyValuePair<string, string> pathContentPair in pathOriginalContentLookup)
                                {
                                    string filePath = pathContentPair.Key;
                                    string fileName = "";
                                    //if changing the file name
                                    if (changedFileNames.ContainsKey(filePath))
                                    {
                                        //get just the file extension
                                        string fileExt = "";
                                        if (filePath.IndexOf(".") != -1)
                                        {
                                            fileExt = filePath.Substring(filePath.LastIndexOf("."));
                                        }
                                        fileName = changedFileNames[filePath] + fileExt;
                                    }
                                    else //use same filename as the original template file
                                    {
                                        fileName = filePath;
                                        if (fileName.IndexOf("\\") != -1)
                                        {
                                            //get just the filename with no path
                                            fileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                                        }
                                    }
                                    //if changing the file directory (under the current project directory)
                                    string changedFileDir = "";
                                    if (changedFileDirs.ContainsKey(filePath))
                                    {
                                        changedFileDir = changedFileDirs[filePath];
                                        //make sure each directory exists... create them if they don't
                                        string[] dirs = changedFileDir.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                                        string currentDir = upOneDirPath + "\\" + projFolder + "\\";
                                        for (int d = 0; d < dirs.Length; d++)
                                        {
                                            //if this directory doesn't exist
                                            currentDir += dirs[d].Trim() + "\\";
                                            if (!Directory.Exists(currentDir))
                                            {
                                                //create the directory
                                                Directory.CreateDirectory(currentDir);
                                            }
                                        }
                                        //append the final \\ at the end of the directory path
                                        changedFileDir += "\\";
                                    }
                                    //get the new file content
                                    string fileContent = pathContentPair.Value;
                                    string newFilePath = upOneDirPath + "\\" + projFolder + "\\" + changedFileDir + fileName;
                                    //if the new file doesn't already exist
                                    if (!File.Exists(newFilePath))
                                    {
                                        //if the file content is NOT blank
                                        fileContent = fileContent.Trim();
                                        if (fileContent.Length > 0)
                                        {
                                            //restore certain string contents
                                            fileContent = fileContent.Replace(startEscToken, "<<");
                                            fileContent = fileContent.Replace(endEscToken, ">>");
                                            //create the file with its content (maybe changed or maybe not changed and just copied over)
                                            System.IO.File.WriteAllText(newFilePath, fileContent);
                                            Console.WriteLine("  FILE CREATED: \t" + projFolder + "\\" + changedFileDir + fileName);
                                            fileCount++;
                                        }
                                        else
                                        {
                                            Console.WriteLine("  FILE SKIP (BLANK): \t" + projFolder + "\\" + changedFileDir + fileName);
                                            skippedFileCount++;
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("  FILE SKIP (ALREADY EXISTS): \t" + projFolder + "\\" + changedFileDir + fileName);
                                        skippedFileCount++;
                                    }
                                }
                                Console.WriteLine("  -------------------------------------------------------\n");
                                Console.WriteLine("\n  Done.\n  Created files: (" + fileCount + ") || Skipped files: (" + skippedFileCount + ")");
                            }
                            else
                            {
                                Console.WriteLine("  Error, \"" + chosenTemplateIndex + "\" is too low.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("  Error, there are only " + templateOptions.Count + " options. \"" + chosenTemplateIndex + "\" is too high.");
                        }
                    }
                }
                else //no templates
                {
                    Console.WriteLine("\n  There are no templates in \"../" + newfile_templates + "\".");
                    Console.WriteLine("  " + templateRootPath);
                }
            }
            else
            {
                //tell the user that newfile_templates folder doesn't exist
                Console.WriteLine("  Uh oh; the root template folder, \"../" + newfile_templates + "\" was not found:");
                Console.WriteLine("  " + templateRootPath);
                Console.Write("\n  Would you like to create this folder? (y/n): ");
                //if this user would like to create this missing folder
                string line = Console.ReadLine(); line = line.Trim();
                line = line.ToLower();
                if (line.IndexOf("y") == 0)
                {
                    //create this folder
                    Directory.CreateDirectory(templateRootPath);
                    string demoFolder = "ExampleTemplate";
                    Directory.CreateDirectory(templateRootPath + "\\" + demoFolder);
                    //create a demo file
                    string demoFilePath1 = templateRootPath + "\\" + demoFolder + "\\demofile.txt";
                    string demoFileContent1 = "";
                    demoFileContent1 += "  <<filename:uppercase:example output filename>>";
                    demoFileContent1 += "  Hi, <<var:capitalize:your name>> \r\n\r\n";
                    demoFileContent1 += "  ... so... you say your favorite color is <<var:uppercase:your favorite color>>? \r\n";
                    demoFileContent1 += "  Are you trying to be a <<var:lowercase:a random qualitative adjective>> person? \r\n";
                    demoFileContent1 += "  Despite this... I guess I will forgive you. \r\n\r\n";
                    demoFileContent1 += "  Anyway, you may have guessed by now that this template generator has the ability to insert 'madlib' tokens into your files.\r\n";
                    demoFileContent1 += "  You can insert these tokens into your template file(s) and then define their values when it comes time to generate a template.\r\n\r\n";
                    demoFileContent1 += "  Here are some token examples (you may recognize them):.\r\n";
                    demoFileContent1 += "  =============================\r\n\r\n";
                    demoFileContent1 += "  \\<<var:capitalize:your name\\>> \r\n";
                    demoFileContent1 += "  \\<<var:uppercase:your favorite color\\>> \r\n";
                    demoFileContent1 += "  \\<<var:lowercase:a random adjective\\>> \r\n\r\n";
                    demoFileContent1 += "  Note, 'var' is NOT the only type of token you can put into a template file...  \r\n";
                    demoFileContent1 += "  You can also define a template file's name or folder path within your project's directory. With...  \r\n\r\n";
                    demoFileContent1 += "  \\<<filename:lowercase:file name\\>> (you will be asked to enter the file name before the file is created from the template) \r\n";
                    demoFileContent1 += "  \\<<filename:lowercase:sub/path:file name\\>> (your file will be placed in a 'sub/path' directory under your project root directory) \r\n";
                    demoFileContent1 += "  \\<<filename:lowercase:sub/path:.\\>> (your file will be placed in a 'sub/path' directory, but it's template file name will remain the same) \r\n\r\n";
                    demoFileContent1 += "  I hope this helps. Peace out. \r\n";
                    System.IO.File.WriteAllText(demoFilePath1, demoFileContent1);
                    //closing message
                    Console.WriteLine("\n  folder created. Now... put files / sub-folders inside \"" + newfile_templates + "\" to serve as templates. \n" );
                    Console.WriteLine("  A template can contain multiple files that all get generated together. ");
                    Console.WriteLine("  A folder that contains one or more files is a single template (an example of a template was created for you). ");
                    Console.WriteLine("\n  Come back later when you have setup your template(s); just copy some files into  \"" + newfile_templates + "\"");
                }
            }
            Console.WriteLine("\n\n  Press any key to close...");
            Console.ReadKey();
        }

        //replace the aliases inside fileContent with their associated value (if the alias is inside fileContent)
        private static string getReplacedAliases(string fileContent, Dictionary<string, string> aliasValueLookup)
        {
            //if there are any aliases
            if (aliasValueLookup.Count > 0)
            {
                //if there is any file content
                if (fileContent.Trim().Length > 0)
                {
                    //for each alias
                    foreach (KeyValuePair<string, string> aliasValPair in aliasValueLookup)
                    {
                        //if the file content contains this alias
                        if (fileContent.Contains(aliasValPair.Key))
                        {
                            //replace this alias with the value inside file content
                            fileContent = fileContent.Replace(aliasValPair.Key, aliasValPair.Value);
                        }
                    }
                }
            }
            return fileContent;
        }

        //formats the value based on token parameters and input value, eg: decides the casing to apply to the user input
        private static string getFormattedTokenValue(string[] tokenParts, Dictionary<string, string> tokenInputLookup)
        {
            //get the token name
            string tokenName = getTokenPart("name", tokenParts);
            //get the token type
            string type = getTokenPart("type", tokenParts);
            //get the token casing
            string casing = getTokenPart("casing", tokenParts);
            //if not a blank tokenName, represented by a dot, . AND not a static value surrounded by "quotes"
            string tokenValue = "";
            if (tokenName != "." && tokenName.IndexOf("\"") != 0 && tokenName.IndexOf("'") != 0)
            {
                //get the token value... the value is formatted based on the different token parts, eg: casing
                tokenValue = tokenInputLookup[tokenName];
                //get the first letter of the casing 
                string firstCharCasing = casing.Trim().ToLower();
                firstCharCasing = firstCharCasing.Substring(0, 1);
                //default casing
                casing = "normal";
                //standardized what casing is assigned based on the first letter 
                //(for code-readability... no other reason)
                switch (firstCharCasing)
                {
                    case "u":
                        casing = "uppercase";
                        break;
                    case "l":
                        casing = "lowercase";
                        break;
                    case "c":
                        casing = "capitalize";
                        break;
                    default:
                        break;
                }
                //format depending on casing
                switch (casing)
                {
                    case "uppercase":
                        tokenValue = tokenValue.ToUpper();
                        break;
                    case "lowercase":
                        tokenValue = tokenValue.ToLower();
                        break;
                    case "capitalize":
                        string firstChar = tokenValue.Substring(0, 1);
                        string theRest = tokenValue.Substring(1);
                        firstChar = firstChar.ToUpper();
                        tokenValue = firstChar + theRest;
                        break;
                    case "normal":
                        //yep... do nothing. Leave as is
                        break;
                    default:
                        break;
                }
            }

            return tokenValue;
        }

        private static string getTokenPart(string partKey, string tokenStr)
        {
            //get the token parts, eg: <<type:casing:name>>
            string[] tokenParts = tokenStr.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            return getTokenPart(partKey, tokenParts);
        }
        private static string getTokenPart(string partKey, string[] tokenParts)
        {
            string returnStr = "";
            //return a different part depending on the given part key
            switch(partKey)
            {
                case "type":
                    //type is always first part
                    string type = tokenParts[0];
                    //if token name contains >>
                    if (type.Contains("<<"))
                    {
                        //remove starting <<
                        type = type.Substring(">>".Length);
                    }
                    //always trim and lowercase token type
                    type = type.Trim(); type = type.ToLower();
                    returnStr = type;
                    break;
                case "casing":
                    //token casing is always second part
                    string casing = tokenParts[1];
                    //always trim and lowercase token casing
                    casing = casing.Trim(); casing = casing.ToLower();
                    returnStr = casing;
                    break;
                case "dir":
                    //if there are more than 3 token parts
                    if (tokenParts.Length > 3)
                    {
                        //recursively get type
                        string tokenType = getTokenPart("type", tokenParts);
                        //if this type is a "filename"
                        if (tokenType == "filename")
                        {
                            //token directory is always second-to-last part, eg: <<filename:lowercase:folder/path:filename>>
                            string dir = tokenParts[tokenParts.Length - 2];
                            //always trim token dir
                            dir = dir.Trim();
                            returnStr = dir;
                            //normalize the directory separators
                            returnStr = returnStr.Replace("\\", "/");
                            returnStr = returnStr.Replace("///", "/");
                            returnStr = returnStr.Replace("//", "/");
                            returnStr = returnStr.Replace("/", "\\");
                            //if this dir path contains a separtor
                            if (returnStr.Contains("\\"))
                            {
                                //cannot end with \\
                                if (returnStr.LastIndexOf("\\") == returnStr.Length - 1)
                                {
                                    //trim off ending \\
                                    returnStr = returnStr.Substring(0, returnStr.Length - 1);
                                }
                                //cannot start with \\
                                if (returnStr.IndexOf("\\") == 0)
                                {
                                    //trim off starting \\
                                    returnStr = returnStr.Substring(1);
                                }
                            }
                        }
                    }
                    break;
                case "name":
                    //token name is always last part
                    string uniqueTokenName = tokenParts[tokenParts.Length - 1];
                    //if the token name contains a name alias, eg: {name}=>{shorter-alias}
                    if (uniqueTokenName.Contains("=>"))
                    {
                        //get just the name part and remove the alias part
                        uniqueTokenName = uniqueTokenName.Substring(0, uniqueTokenName.IndexOf("=>"));
                    }
                    else
                    {
                        //no name alias...

                        //if token name contains >>
                        if (uniqueTokenName.Contains(">>"))
                        {
                            //remove trailing >>
                            uniqueTokenName = uniqueTokenName.Substring(0, uniqueTokenName.LastIndexOf(">>"));
                        }
                    }
                    //always trim and lowercase token name
                    uniqueTokenName = uniqueTokenName.Trim(); uniqueTokenName = uniqueTokenName.ToLower();
                    returnStr = uniqueTokenName;
                    break;
                case "alias":
                    returnStr = "";
                    //recursively get type
                    string tType = getTokenPart("type", tokenParts);
                    //if this type is a "var" (only var types can have an alias)
                    if (tType == "var")
                    {
                        //token name => alias is always last part
                        string nameAndAlias = tokenParts[tokenParts.Length - 1];
                        //if the token name contains a name alias, eg: {name}=>{shorter-alias}
                        if (nameAndAlias.Contains("=>"))
                        {
                            //get just the alias part and remove the name part
                            string aliasStr = nameAndAlias.Substring(nameAndAlias.IndexOf("=>") + "=>".Length);
                            //if alias contains >>
                            if (aliasStr.Contains(">>"))
                            {
                                //remove trailing >>
                                aliasStr = aliasStr.Substring(0, aliasStr.LastIndexOf(">>"));
                            }
                            //trim the alias (alias is case sensitive so it does NOT get ToLower)
                            aliasStr = aliasStr.Trim();
                            returnStr = aliasStr;
                        }
                    }
                    break;

            }
            return returnStr;
        }

        private static void showTemplateOption(int index, string newfile_templates, List<string> templateOptions, ref List<string> templateOutputOptions)
        {
            //get the path for this option
            string templateOption = templateOptions[index];
            //remove first part of the path
            templateOption = templateOption.Substring(templateOption.IndexOf("\\" + newfile_templates) + 1);
            //write template option
            Console.WriteLine("-----------------------------------------------");
            string outputStr = "";
            outputStr += "  " + index + "\t " + templateOption;
            //print the files inside this folder
            DirectoryInfo dir = new DirectoryInfo(templateOptions[index]);
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                //print file
                outputStr += "\n\t\t " + file.Name;
            }
            templateOutputOptions.Add(outputStr);
            //print
            Console.WriteLine(outputStr);
        }

        private static void loadDirsWithFiles(string rootPath, ref List<string> templateOptions)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(rootPath);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + rootPath);
            }

            // Get the files in the directory
            FileInfo[] files = dir.GetFiles();
            //if there are any files in this directory
            if (files.Length > 0)
            {
                //if this directory is not already in the list
                if (!templateOptions.Contains(dir.FullName))
                {
                    //get the directory path
                    templateOptions.Add(dir.FullName);
                }
            }

            //for each sub directory
            foreach (DirectoryInfo subdir in dirs)
            {
                //find out what sub directories also have files
                loadDirsWithFiles(subdir.FullName, ref templateOptions);
            }
        }

        private static List<string> getTokensFromContent(string contents)
        {
            //Dictionary<[tokenKey], [blankToStoreTokenValue]>
            List<string> tokens = new List<string>();

            //what are the different possible token type starting strings?
            List<string> tokenStartTags = new List<string>();
            tokenStartTags.Add("var");
            tokenStartTags.Add("filename");

            //if the file content contains <
            string[] splitByCarrot = contents.Split(new char[]{'<'}, StringSplitOptions.RemoveEmptyEntries);
            if (splitByCarrot.Length > 0)
            {
                //for each string starting with <
                for (int c = 0; c < splitByCarrot.Length; c++)
                {
                    //if the string starts with <<
                    string str = splitByCarrot[c];
                    //if the string contains >>
                    if (str.Contains(">>"))
                    {
                        //get just the string between the << and >>
                        str = str.Substring(0, str.IndexOf(">>") + ">>".Length);
                        //if the string contains :
                        if (str.Contains(":"))
                        {
                            //get the token parts
                            string[] tokenParts = str.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            //if the first token part is a type listed in tokenStartTags
                            if(tokenStartTags.Contains(tokenParts[0]))
                            {
                                //if there are at least three parts to the token
                                if (tokenParts.Length > 2)
                                {
                                    //if contents contains this string starting with <<
                                    str = "<<" + str;
                                    if (contents.Contains(str))
                                    {
                                        //if this key is not already in the list
                                        if (!tokens.Contains(str))
                                        {
                                            //add the possible token to the allTokens list
                                            tokens.Add(str);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return tokens;
        }
    }
}
