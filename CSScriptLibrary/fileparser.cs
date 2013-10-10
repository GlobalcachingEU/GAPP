#region Licence...

//-----------------------------------------------------------------------------
// Date:	31/01/05	Time: 17:15p
// Module:	fileparser.cs
// Classes:	ParsingParams
//			ScriptInfo
//			ScriptParser
//			FileParser
//			FileParserComparer
//
// This module contains the definition of the classes which implement
// parsing script code. The result of such processing is a collections of the names
// of the namespacs and assemblies used by the script code.
//
// Written by Oleg Shilo (oshilo@gmail.com)
// Copyright (c) 2004-2012. All rights reserved.
//
// Redistribution and use of this code WITHOUT MODIFICATIONS are permitted provided that
// the following conditions are met:
// 1. Redistributions must retain the above copyright notice, this list of conditions
//  and the following disclaimer.
// 2. Neither the name of an author nor the names of the contributors may be used
//	to endorse or promote products derived from this software without specific
//	prior written permission.
//
// Redistribution and use of this code WITH MODIFICATIONS are permitted provided that all
// above conditions are met and software is not used or sold for profit.
//
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
// TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//	Caution: Bugs are expected!
//----------------------------------------------

#endregion Licence...

using System;
using System.IO;
using System.Text;

#if net1
using System.Collections;
#else

using System.Collections.Generic;

#endif

using csscript;

namespace CSScriptLibrary
{
    /// <summary>
    /// ParsingParams is an class that holds parsing parameters (parameters that controls how file is to be parsed).
    /// At this moment they are namespace renaming rules only.
    /// </summary>
    class ParsingParams
    {
        #region Public interface...

        public ParsingParams()
        {
#if net1
            renameNamespaceMap = new ArrayList();
#else
            renameNamespaceMap = new List<string[]>();
#endif
        }

        public string[][] RenameNamespaceMap
        {
#if net1
            get { return (string[][])renameNamespaceMap.ToArray(typeof(string[])); }
#else
            get { return renameNamespaceMap.ToArray(); }
#endif
        }

        public void AddRenameNamespaceMap(string[][] names)
        {
            renameNamespaceMap.AddRange(names);
        }

        /// <summary>
        /// Compare() is to be used to help with implementation of IComparer for sorting operations.
        /// </summary>
        public static int Compare(ParsingParams xPrams, ParsingParams yPrams)
        {
            if (xPrams == null && yPrams == null)
                return 0;

            int retval = xPrams == null ? -1 : (yPrams == null ? 1 : 0);

            if (retval == 0)
            {
                string[][] xNames = xPrams.RenameNamespaceMap;
                string[][] yNames = yPrams.RenameNamespaceMap;
                retval = System.Collections.Comparer.Default.Compare(xNames.Length, yNames.Length);
                if (retval == 0)
                {
                    for (int i = 0; i < xNames.Length && retval == 0; i++)
                    {
                        retval = System.Collections.Comparer.Default.Compare(xNames[i].Length, yNames[i].Length);
                        if (retval == 0)
                        {
                            for (int j = 0; j < xNames[i].Length; j++)
                            {
                                retval = System.Collections.Comparer.Default.Compare(xNames[i][j], yNames[i][j]);
                            }
                        }
                    }
                }
            }
            return retval;
        }

        public bool preserveMain = false;

        #endregion Public interface...

#if net1
        private ArrayList renameNamespaceMap;
#else
        List<string[]> renameNamespaceMap;
#endif
    }

    /// <summary>
    /// Class which is a placeholder for general information of the script file
    /// </summary>
    class ScriptInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">ImportInfo object containing the information how the script file should be parsed.</param>
        public ScriptInfo(CSharpParser.ImportInfo info)
        {
            this.fileName = info.file;
            parseParams.AddRenameNamespaceMap(info.renaming);
            parseParams.preserveMain = info.preserveMain;
        }

        public ParsingParams parseParams = new ParsingParams();
        public string fileName;
    }

    /// <summary>
    /// Class that implements parsing the single C# script file
    /// </summary>
    class FileParser
    {
        static bool _throwOnError = true;

        public System.Threading.ApartmentState ThreadingModel
        {
            get
            {
                if (this.parser == null)
                    return System.Threading.ApartmentState.Unknown;
                else
                    return this.parser.ThreadingModel;
            }
        }

        public FileParser()
        {
        }

        public FileParser(string fileName, ParsingParams prams, bool process, bool imported, string[] searchDirs, bool throwOnError)
        {
            FileParser._throwOnError = throwOnError;
            this.imported = imported;
            this.prams = prams;
            this.searchDirs = searchDirs;
            this.fileName = ResolveFile(fileName, searchDirs);
            if (process)
                ProcessFile();
        }

        public string fileNameImported = "";
        public ParsingParams prams = null;
        
        public string FileToCompile
        {
            get { return imported ? fileNameImported : fileName; }
        }

        public string[] SearchDirs
        {
            get { return searchDirs; }
        }

        public bool Imported
        {
            get { return imported; }
        }

        public string[] ReferencedNamespaces
        {
            get { return parser.RefNamespaces; }
        }

        public string[] IgnoreNamespaces
        {
            get { return parser.IgnoreNamespaces; }
        }
        public string[] Precompilers
        {
            get { return parser.Precompilers; }
        }

        public string[] ReferencedAssemblies
        {
            get { return parser.RefAssemblies; }
        }

        public string[] ExtraSearchDirs
        {
            get { return parser.ExtraSearchDirs; }
        }

        public string[] ReferencedResources
        {
            get { return parser.ResFiles; }
        }

        public string[] CompilerOptions
        {
            get { return parser.CompilerOptions; }
        }

        public ScriptInfo[] ReferencedScripts
        {
#if net1
            get { return (ScriptInfo[])referencedScripts.ToArray(typeof(ScriptInfo)); }
#else
            get { return referencedScripts.ToArray(); }
#endif
        }

        public void ProcessFile()
        {
            referencedAssemblies.Clear();
            referencedScripts.Clear();
            referencedNamespaces.Clear();
            referencedResources.Clear();

            this.parser = new CSharpParser(fileName, true);

            foreach (CSharpParser.ImportInfo info in parser.Imports)
            {
                referencedScripts.Add(new ScriptInfo(info));
            }
            referencedAssemblies.AddRange(parser.RefAssemblies);
            referencedNamespaces.AddRange(parser.RefNamespaces);
            referencedResources.AddRange(parser.ResFiles);

            if (imported)
            {
                if (prams != null)
                {
                    parser.DoRenaming(prams.RenameNamespaceMap, prams.preserveMain);
                }
                if (parser.ModifiedCode == "")
                {
                    fileNameImported = fileName; //importing does not require any modification of the original code
                }
                else
                {
                    fileNameImported = Path.Combine(CSExecutor.ScriptCacheDir, string.Format("i_{0}_{1}{2}", Path.GetFileNameWithoutExtension(fileName), Path.GetDirectoryName(fileName).GetHashCode(), Path.GetExtension(fileName)));
                    if (!Directory.Exists(Path.GetDirectoryName(fileNameImported)))
                        Directory.CreateDirectory(Path.GetDirectoryName(fileNameImported));
                    if (File.Exists(fileNameImported))
                    {
                        File.SetAttributes(fileNameImported, FileAttributes.Normal);
                        File.Delete(fileNameImported);
                    }

                    using (StreamWriter scriptWriter = new StreamWriter(fileNameImported, false, Encoding.GetEncoding(0)))
                    {
                        //scriptWriter.Write(ComposeHeader(fileNameImported)); //using a big header at start is overkill (it also shifts line numbers so they do not match with the original script file)
                        //but maight be required in future
                        scriptWriter.WriteLine(parser.ModifiedCode);
                        scriptWriter.WriteLine("///////////////////////////////////////////");
                        scriptWriter.WriteLine("// Compiler-generated file - DO NOT EDIT!");
                        scriptWriter.WriteLine("///////////////////////////////////////////");
                    }
                    File.SetAttributes(fileNameImported, FileAttributes.ReadOnly);
                }
            }
        }

#if net1
        private ArrayList referencedScripts = new ArrayList();
        private ArrayList referencedNamespaces = new ArrayList();
        private ArrayList referencedAssemblies = new ArrayList();
        private ArrayList referencedResources = new ArrayList();
#else
        private List<ScriptInfo> referencedScripts = new List<ScriptInfo>();
        private List<string> referencedNamespaces = new List<string>();
        private List<string> referencedAssemblies = new List<string>();
        private List<string> referencedResources = new List<string>();
#endif
        private string[] searchDirs;
        private bool imported = false;

        /// <summary>
        /// Searches for script file by given script name. Calls ResolveFile(string fileName, string[] extraDirs, bool throwOnError)
        /// with throwOnError flag set to true.
        /// </summary>
        public static string ResolveFile(string fileName, string[] extraDirs)
        {
            return ResolveFile(fileName, extraDirs, _throwOnError);
        }

        /// <summary>
        /// Searches for script file by given script name. Search order:
        /// 1. Current directory
        /// 2. extraDirs (usually %CSSCRIPT_DIR%\Lib and ExtraLibDirectory)
        /// 3. PATH
        /// Also fixes file name if user did not provide extension for script file (assuming .cs extension)
        /// </summary>
        public static string ResolveFile(string file, string[] extraDirs, bool throwOnError)
        {
            string retval = ResolveFile(file, extraDirs, "");
            if (retval == "")
                retval = ResolveFile(file, extraDirs, ".cs"); 
            if (retval == "")
                retval = ResolveFile(file, extraDirs, ".csl"); //script link file

            if (retval == "")
            {
                if (throwOnError)
                    throw new FileNotFoundException(string.Format("Could not find file \"{0}\"", file));

                retval = file;
                if (!retval.EndsWith(".cs"))
                    retval += ".cs";
            }

            return retval;
        }

        private static string ResolveFile(string file, string[] extraDirs, string extension)
        {
            string fileName = file;
            //current directory
            if (Path.GetExtension(fileName) == "")
                fileName += extension;

            if (File.Exists(fileName))
            {
                return Path.GetFullPath(fileName);
            }

            //arbitrary directories
            if (extraDirs != null)
            {
                foreach (string extraDir in extraDirs)
                {
                    string dir = extraDir;
                    if (File.Exists(Path.Combine(dir, fileName)))
                    {
                        return Path.GetFullPath(Path.Combine(dir, fileName));
                    }
                }
            }

            //PATH
            string[] pathDirs = Environment.GetEnvironmentVariable("PATH").Replace("\"", "").Split(';');
            foreach (string pathDir in pathDirs)
            {
                string dir = pathDir;
                if (File.Exists(Path.Combine(dir, fileName)))
                {
                    return Path.GetFullPath(Path.Combine(dir, fileName));
                }
            }

            return "";
        }

        static public string headerTemplate =
                @"/*" + Environment.NewLine +
                @" Created by {0}" +
                @" Original location: {1}" + Environment.NewLine +
                @" C# source equivalent of {2}" + Environment.NewLine +
                @" compiler-generated file created {3} - DO NOT EDIT!" + Environment.NewLine +
                @"*/" + Environment.NewLine;

        public string ComposeHeader(string path)
        {
            return string.Format(headerTemplate, csscript.AppInfo.appLogoShort, path, fileName, DateTime.Now);
        }

        public string fileName = "";
        public CSharpParser parser;
    }

    /// <summary>
    /// Class that implements parsing the single C# Script file
    /// </summary>
    /// <summary>
    /// Implementation of the IComparer for sorting operations of collections of FileParser instances
    /// </summary>
    ///
#if net1
    class FileParserComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            if (x == null && y == null)
                return 0;

            int retval = x == null ? -1 : (y == null ? 1 : 0);

            if (retval == 0)
            {
                FileParser xParser = (FileParser)x;
                FileParser yParser = (FileParser)y;
                retval = string.Compare(xParser.fileName, yParser.fileName, true);
                if (retval == 0)
                {
                    retval = ParsingParams.Compare(xParser.prams, yParser.prams);
                }
            }

            return retval;
        }
    }
#else

    class FileParserComparer : IComparer<FileParser>
    {
        public int Compare(FileParser x, FileParser y)
        {
            if (x == null && y == null)
                return 0;

            int retval = x == null ? -1 : (y == null ? 1 : 0);

            if (retval == 0)
            {
                retval = string.Compare(x.fileName, y.fileName, true);
                if (retval == 0)
                {
                    retval = ParsingParams.Compare(x.prams, y.prams);
                }
            }

            return retval;
        }
    }

#endif

    /// <summary>
    /// Class that manages parsing the main and all imported (if any) C# Script files
    /// </summary>
    public class ScriptParser
    {
        bool throwOnError = true;
        /// <summary>
        /// ApartmentState of a script during the execution (default: ApartmentState.Unknown)
        /// </summary>
        public System.Threading.ApartmentState apartmentState = System.Threading.ApartmentState.Unknown;

        /// <summary>
        /// Collection of the files to be compiled (including dependant scripts)
        /// </summary>
        public string[] FilesToCompile
        {
            get
            {
#if net1
                ArrayList retval = new ArrayList();
                foreach (FileParser file in fileParsers)
                    retval.Add(file.FileToCompile);
                return (string[])retval.ToArray(typeof(string));
#else
                List<string> retval = new List<string>();
                foreach (FileParser file in fileParsers)
                    retval.Add(file.FileToCompile);
                return retval.ToArray();
#endif
            }
        }

        /// <summary>
        /// Collection of the imported files (dependant scripts)
        /// </summary>
        public string[] ImportedFiles
        {
            get
            {
#if net1
                ArrayList retval = new ArrayList();
                foreach (FileParser file in fileParsers)
                {
                    if (file.Imported)
                        retval.Add(file.fileName);
                }
                return (string[])retval.ToArray(typeof(string));
#else
                List<string> retval = new List<string>();
                foreach (FileParser file in fileParsers)
                {
                    if (file.Imported)
                        retval.Add(file.fileName);
                }
                return retval.ToArray();
#endif
            }
        }

        /// <summary>
        /// Collection of resource files referenced from code
        /// </summary>
        public string[] ReferencedResources
        {
#if net1
            get { return (string[])referencedResources.ToArray(typeof(string)); }
#else
            get { return referencedResources.ToArray(); }
#endif
        }

        /// <summary>
        /// Collection of compiler options
        /// </summary>
        public string[] CompilerOptions
        {
#if net1
            get { return (string[])compilerOptions.ToArray(typeof(string)); }
#else
            get { return compilerOptions.ToArray(); }
#endif
        }
        /// <summary>
        /// Precompilers specified in the primary script file.
        /// </summary>
        public string[] Precompilers
        {
#if net1
            get { return (string[])precompilers.ToArray(typeof(string)); }
#else
            get { return precompilers.ToArray(); }
#endif
        }

        /// <summary>
        /// Collection of namespaces referenced from code (including those referenced in dependand scripts)
        /// </summary>
        public string[] ReferencedNamespaces
        {
#if net1
            get { return (string[])referencedNamespaces.ToArray(typeof(string)); }
#else
            get { return referencedNamespaces.ToArray(); }
#endif
        }

        /// <summary>
        /// Collection of namespaces, which if found in code, should not be resolved into referenced assembly.
        /// </summary>
        public string[] IgnoreNamespaces
        {
#if net1
            get { return (string[])ignoreNamespaces.ToArray(typeof(string)); }
#else
            get { return ignoreNamespaces.ToArray(); }
#endif
        }

        /// <summary>
        /// Collection of referenced asesemblies. All assemblies are referenced either from command-line, code or resolved from referenced namespaces.
        /// </summary>
        public string[] ReferencedAssemblies
        {
#if net1
            get { return (string[])referencedAssemblies.ToArray(typeof(string)); }
#else
            get { return referencedAssemblies.ToArray(); }
#endif
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fileName">Script file name</param>
        public ScriptParser(string fileName)
        {
            Init(fileName, null);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fileName">Script file name</param>
        /// <param name="searchDirs">Extra ScriptLibrary directory </param>
        public ScriptParser(string fileName, string[] searchDirs)
        {
            //if ((CSExecutor.ExecuteOptions.options != null && CSExecutor.options.useSmartCaching) && CSExecutor.ScriptCacheDir == "") //in case if ScriptParser is used outside of the script engine
            if (CSExecutor.ScriptCacheDir == "") //in case if ScriptParser is used outside of the script engine
                CSExecutor.SetScriptCacheDir(fileName);
            Init(fileName, searchDirs);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fileName">Script file name</param>
        /// <param name="searchDirs">Extra ScriptLibrary directory(ies) </param>
        /// <param name="throwOnError">flag to indicate if the file parsing/processing error should raise an exception</param>
        public ScriptParser(string fileName, string[] searchDirs, bool throwOnError)
        {
            this.throwOnError = throwOnError;
            //if ((CSExecutor.ExecuteOptions.options != null && CSExecutor.ExecuteOptions.options.useSmartCaching) && CSExecutor.ScriptCacheDir == "") //in case if ScriptParser is used outside of the script engine
            if (CSExecutor.ScriptCacheDir == "") //in case if ScriptParser is used outside of the script engine
                CSExecutor.SetScriptCacheDir(fileName);
            Init(fileName, searchDirs);
        }

        /// <summary>
        /// Initialization of ScriptParser instance
        /// </summary>
        /// <param name="fileName">Script file name</param>
        /// <param name="searchDirs">Extra ScriptLibrary directory(ies) </param>
        private void Init(string fileName, string[] searchDirs)
        {
#if net1
            referencedNamespaces = new ArrayList();
            referencedAssemblies = new ArrayList();
            referencedResources = new ArrayList();
            ignoreNamespaces = new ArrayList();
            compilerOptions = new ArrayList();
            precompilers = new ArrayList();
#else
            referencedNamespaces = new List<string>();
            referencedAssemblies = new List<string>();
            referencedResources = new List<string>();
            ignoreNamespaces = new List<string>();
            precompilers = new List<string>();
            compilerOptions = new List<string>();
#endif
            //process main file
            FileParser mainFile = new FileParser(fileName, null, true, false, searchDirs, throwOnError);
            this.apartmentState = mainFile.ThreadingModel;

            foreach (string file in mainFile.Precompilers)
                PushPrecompiler(file);

            foreach (string namespaceName in mainFile.IgnoreNamespaces)
                PushIgnoreNamespace(namespaceName);

            foreach (string namespaceName in mainFile.ReferencedNamespaces)
                PushNamespace(namespaceName);

            foreach (string asmName in mainFile.ReferencedAssemblies)
                PushAssembly(asmName);

            foreach (string resFile in mainFile.ReferencedResources)
                PushResource(resFile);

            foreach (string opt in mainFile.CompilerOptions)
                PushCompilerOptions(opt);

#if net1
            ArrayList dirs = new ArrayList();
            dirs.Add(Path.GetDirectoryName(mainFile.fileName));//note: mainFile.fileName is warrantied to be a full name but fileName is not
            if (searchDirs != null)
                dirs.AddRange(searchDirs);
            foreach (string dir in mainFile.ExtraSearchDirs)
                if (Path.IsPathRooted(dir))
                    dirs.Add(Path.GetFullPath(dir));
                else
                    dirs.Add(Path.Combine(Path.GetDirectoryName(mainFile.fileName), dir));
            this.SearchDirs = (string[])dirs.ToArray(typeof(string));
#else
            List<string> dirs = new List<string>();
            dirs.Add(Path.GetDirectoryName(mainFile.fileName));//note: mainFile.fileName is warrantied to be a full name but fileName is not
            if (searchDirs != null)
                dirs.AddRange(searchDirs);

            foreach (string dir in mainFile.ExtraSearchDirs)
            {
                if (Path.IsPathRooted(dir))
                    dirs.Add(Path.GetFullPath(dir));
                else
                    dirs.Add(Path.Combine(Path.GetDirectoryName(mainFile.fileName), dir));
            }

            this.SearchDirs = dirs.ToArray();
#endif

            //process imported files if any
            foreach (ScriptInfo fileInfo in mainFile.ReferencedScripts)
                ProcessFile(fileInfo);

            //Main script file shall always be the first. Add it now as previously array was sorted a few times
            this.fileParsers.Insert(0, mainFile);
        }

        private void ProcessFile(ScriptInfo fileInfo)
        {
            FileParserComparer fileComparer = new FileParserComparer();

            FileParser importedFile = new FileParser(fileInfo.fileName, fileInfo.parseParams, false, true, this.SearchDirs, throwOnError); //do not parse it yet (the third param is false)
            if (fileParsers.BinarySearch(importedFile, fileComparer) < 0)
            {
                if (File.Exists(importedFile.fileName))
                {
                    importedFile.ProcessFile(); //parse now namespaces, ref. assemblies and scripts; also it will do namespace renaming

                    this.fileParsers.Add(importedFile);
                    this.fileParsers.Sort(fileComparer);

                    foreach (string namespaceName in importedFile.ReferencedNamespaces)
                        PushNamespace(namespaceName);

                    foreach (string asmName in importedFile.ReferencedAssemblies)
                        PushAssembly(asmName);

                    foreach (string file in importedFile.Precompilers)
                        PushPrecompiler(file);

                    foreach (ScriptInfo scriptFile in importedFile.ReferencedScripts)
                        ProcessFile(scriptFile);

                    foreach (string resFile in importedFile.ReferencedResources)
                        PushResource(resFile);
#if net1
                    ArrayList dirs = new ArrayList(this.SearchDirs);
                    foreach (string dir in importedFile.ExtraSearchDirs)
                        if (Path.IsPathRooted(dir))
                            dirs.Add(Path.GetFullPath(dir));
                        else
                            dirs.Add(Path.Combine(Path.GetDirectoryName(importedFile.fileName), dir));
                    this.SearchDirs = (string[])dirs.ToArray(typeof(string));
#else
                    List<string> dirs = new List<string>(this.SearchDirs);
                    foreach (string dir in importedFile.ExtraSearchDirs)
                        if (Path.IsPathRooted(dir))
                            dirs.Add(Path.GetFullPath(dir));
                        else
                            dirs.Add(Path.Combine(Path.GetDirectoryName(importedFile.fileName), dir));
                    this.SearchDirs = dirs.ToArray();
#endif
                }
                else
                {
                    importedFile.fileNameImported = importedFile.fileName;
                    this.fileParsers.Add(importedFile);
                    this.fileParsers.Sort(fileComparer);
                }
            }
        }

#if net1
        private ArrayList fileParsers = new ArrayList();
#else
        private List<FileParser> fileParsers = new List<FileParser>();
#endif

        /// <summary>
        /// Saves all imported scripts int temporary location.
        /// </summary>
        /// <returns>Collection of the saved imported scrips file names</returns>
        public string[] SaveImportedScripts()
        {
            string workingDir = Path.GetDirectoryName(((FileParser)fileParsers[0]).fileName);
#if net1
            ArrayList retval = new ArrayList();
#else
            List<string> retval = new List<string>();
#endif
            foreach (FileParser file in fileParsers)
            {
                if (file.Imported)
                {
                    if (file.fileNameImported != file.fileName) //script file was copied
                        retval.Add(file.fileNameImported);
                    else
                        retval.Add(file.fileName);
                }
            }
#if net1
            return (string[])retval.ToArray(typeof(string));
#else
            return retval.ToArray();
#endif
        }

        /// <summary>
        /// Deletes imported scripts as a cleanup operation
        /// </summary>
        public void DeleteImportedFiles()
        {
            foreach (FileParser file in fileParsers)
            {
                if (file.Imported && file.fileNameImported != file.fileName) //the file was copied
                {
                    try
                    {
                        File.SetAttributes(file.FileToCompile, FileAttributes.Normal);
                        File.Delete(file.FileToCompile);
                    }
                    catch { }
                }
            }
        }

#if net1
        private ArrayList referencedNamespaces;
        private ArrayList ignoreNamespaces;
        private ArrayList compilerOptions;
        private ArrayList referencedResources;
        private ArrayList precompilers;
        private ArrayList referencedAssemblies;
#else
        private List<string> referencedNamespaces;
        private List<string> ignoreNamespaces;
        private List<string> referencedResources;
        private List<string> compilerOptions;
        private List<string> precompilers;
        private List<string> referencedAssemblies;
#endif
        /// <summary>
        /// CS-Script SearchDirectories specified in the parsed script or its dependent scripts.
        /// </summary>
        public string[] SearchDirs;
#if net1
        private void PushItem(ArrayList collection, string item)
#else
        private void PushItem(List<string> collection, string item)
#endif   
        {
            if (collection.Count > 1)
                collection.Sort();

            AddIfNotThere(collection, item);
        }

        private void PushNamespace(string nameSpace)
        {
            PushItem(referencedNamespaces, nameSpace);
        }
       
        private void PushPrecompiler(string file)
        {
            PushItem(precompilers, file);
        }

        private void PushIgnoreNamespace(string nameSpace)
        {
            PushItem(ignoreNamespaces, nameSpace);
        }

        private void PushAssembly(string asmName)
        {
            PushItem(referencedAssemblies, asmName);
        }

        private void PushResource(string resName)
        {
            PushItem(referencedResources, resName);
        }

        private void PushCompilerOptions(string option)
        {
            AddIfNotThere(compilerOptions, option);
        }

#if net1
        class StringComparer : System.Collections.IComparer
        {
            public int Compare(object x, object y)
            {
                return string.Compare(x as string, y as string, true);
            }
        }
        void AddIfNotThere(System.Collections.ArrayList arr, string item)
        {
            if (arr.BinarySearch(item, new StringComparer()) < 0)
                arr.Add(item);
        }
#else

        class StringComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return string.Compare(x, y, true);
            }
        }

        void AddIfNotThere(List<string> list, string item)
        {
            if (list.BinarySearch(item, new StringComparer()) < 0)
                list.Add(item);
        }

#endif
    }
}