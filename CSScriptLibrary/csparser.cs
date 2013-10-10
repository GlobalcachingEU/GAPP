#region Licence...

//-----------------------------------------------------------------------------
// Date:	26/12/05	Time: 16:11p
// Module:	csparser.cs
// Classes:	CSharpParser
//
// This module contains the definition of the class CSharpParser, which implements
// parsing C# code. The result of such processing is a collections of the names
// of the namespacs and assemblies used by C# code and a 'code' (content of
// C# script file stripped out of comments, "using [namespace name];" and C# script engine directives.
//
// C# script engine directives:
//	[//css_import <file>[,rename_namespace(<oldName>, <newName>);]
//	[//css_reference <file>;]
//	[//css_prescript file([arg0][, arg1]..[,arg2])[,ignore];]
//	[//css_postscript file([arg0][, arg1]..[,arg2])[,ignore];]
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
using System.Collections;

#if !net1

using System.Collections.Generic;

#endif

using System.Threading;
using System.Text;

namespace csscript
{
    #region CSharpParser...

    /// <summary>
    /// Very light parser for C# code. The main purpose of it is to be very fast and reliable.
    /// It only extracts code information relative to the CS-Script.
    /// </summary>
    public class CSharpParser
    {
        /// <summary>
        /// Class to hold the script information on what pre- or post-execution script needs to be executed.
        /// pre- and post-script CS-Script command format:
        /// //css_prescript file([arg0][, arg1]..[,arg2])[,ignore];
        /// //file - script file (extension is optional)
        /// //arg0..N - script string arguments;
        /// If $this is specified as arg0..N it will be replaced with the parent script full name at execution time.
        /// </summary>
        public class CmdScriptInfo
        {
            /// <summary>
            ///	Creates an instance of CmdScriptInfo.
            /// </summary>
            /// <param name="statement">CS-Script pre- or post-script directive</param>
            /// <param name="preScript">If set to true the 'statement' is a pre-script otherwise it is a post-script.</param>
            /// <param name="parentScript">The file name of the main script.</param>
            public CmdScriptInfo(string statement, bool preScript, string parentScript)
            {
                this.preScript = preScript;

                int rBracket = -1;
                int lBracket = statement.IndexOf("(");
                if (lBracket != -1)
                {
#if net1
                    ArrayList argList = new ArrayList();
#else
                    List<string> argList = new List<string>();
#endif
                    argList.Add(CSSUtils.cmdFlagPrefix + "nl");
                    argList.Add(statement.Substring(0, lBracket).Trim());

                    rBracket = statement.LastIndexOf(")");
                    if (rBracket == -1)
                        throw new ApplicationException("Canot parse statement (" + statement + ").");

                    string clearArg;
                    foreach (string arg in statement.Substring(lBracket + 1, rBracket - lBracket - 1).Split(",".ToCharArray()))
                    {
                        clearArg = arg.Trim();
                        if (clearArg != string.Empty)
                            argList.Add(clearArg.StartsWith("$this") ? (clearArg == "$this.name" ? Path.GetFileNameWithoutExtension(parentScript) : parentScript) : clearArg);
                    }
#if net1
                    args = (string[])argList.ToArray(typeof(string));
#else
                    args = argList.ToArray();
#endif
                    if (statement.Substring(rBracket + 1).Trim() == "ignore")
                        abortOnError = false;
                }
                else
                    throw new ApplicationException("Canot parse statement (" + statement + ").");
            }

            /// <summary>
            /// Script file and it's arguments.
            /// </summary>
            public string[] args;
            /// <summary>
            /// If set to 'true' the CmdScriptInfo describes the pre-script, otherwise it is for the post-script.
            /// </summary>
            public bool preScript;
            /// <summary>
            /// If set to 'true' parent script will be aborted on pre/post-script error, otherwise the error will be ignored.
            /// </summary>
            public bool abortOnError = true;
        }

        /// <summary>
        /// Class to hold the script importing information, which actually controls how script is imported.
        /// </summary>
        public class ImportInfo
        {
            /// <summary>
            /// Creates an instance of ImportInfo.
            /// </summary>
            /// <param name="statement">CS-Script import directive (//css_import...) string.</param>
            /// <param name="parentScript">name of the parent (primary) script file.</param>
            public ImportInfo(string statement, string parentScript)
            {
#if net1
                ArrayList renameingMap = new ArrayList();
#else
                List<string[]> renameingMap = new List<string[]>();
#endif
                statement = statement.Replace("($this.name)", Path.GetFileNameWithoutExtension(parentScript));
                string[] parts = statement.Replace("\t", "").Trim().Replace(")", "").Split("(,".ToCharArray());

                this.file = parts[0];

                for (int i = 1; i < parts.Length; )
                {
                    parts[i] = parts[i].Trim();
                    if (parts[i] == "rename_namespace" && i + 2 < parts.Length)
                    {
                        string[] names = new string[] { parts[i + 1], parts[i + 2].Replace(")", "") };
                        renameingMap.Add(names);
                        i += 3;
                    }
                    else if (parts[i] == "preserve_main")
                    {
                        preserveMain = true;
                        i += 1;
                    }
                    else
                        throw new ApplicationException("Cannot parse \"//css_import...\"");
                }
                if (renameingMap.Count == 0)
                    this.renaming = new string[0][];
                else
#if net1
                    this.renaming = (string[][])renameingMap.ToArray(typeof(string[]));
#else
                    this.renaming = renameingMap.ToArray();
#endif
            }

            /// <summary>
            /// The file to be imporeted.
            /// </summary>
            public string file;
            /// <summary>
            /// Renaming instructions (old_name vs. new_name)
            /// </summary>
            public string[][] renaming;
            /// <summary>
            /// If set to 'true' "static...Main" in the imported script is not renamed.
            /// </summary>
            public bool preserveMain = false;
        }

#if net1
        ArrayList stringRegions = new ArrayList();
        ArrayList commentRegions = new ArrayList();
#else
        List<int[]> stringRegions = new List<int[]>();
        List<int[]> commentRegions = new List<int[]>();
#endif

        #region Public interface

        /// <summary>
        /// Creates an instance of CSharpParser.
        /// </summary>
        /// <param name="code">C# code string</param>
        public CSharpParser(string code)
        {
            Init(code, "");
        }

        /// <summary>
        /// Creates an instance of CSharpParser.
        /// </summary>
        /// <param name="script">C# script (code or file).</param>
        /// <param name="isFile">If set to 'true' the script is a file, otherwise it is a C# code.</param>
        public CSharpParser(string script, bool isFile)
        {
            if (!isFile)
                Init(script, "");
            else
                using (StreamReader sr = new StreamReader(script, Encoding.GetEncoding(0)))
                    Init(sr.ReadToEnd(), script);
        }

        /// <summary>
        /// Creates an instance of CSharpParser.
        /// </summary>
        /// <param name="script">C# script (code or file).</param>
        /// <param name="isFile">If set to 'true' the script is a file, otherwise it is a C# code.</param>
        /// <param name="directivesToSearch">Additional C# script directives to search. The search result is stored in CSharpParser.CustomDirectives.</param>
        public CSharpParser(string script, bool isFile, string[] directivesToSearch)
        {
            if (!isFile)
                Init(script, "", directivesToSearch);
            else
                using (StreamReader sr = new StreamReader(script))
                    Init(sr.ReadToEnd(), script, directivesToSearch);
        }

        /// <summary>
        /// The result of search for additional C# script directives to search (directive vs. value).
        /// </summary>
        public Hashtable CustomDirectives = new Hashtable();

        /// <summary>
        /// Parses the C# code. Only one of the 'code' and 'file' parameters can be non empty.
        /// </summary>
        /// <param name="code">C# script code (empty string if code is in a file form).</param>
        /// <param name="file">The script file name (empty if code is in the text form).</param>
        public void Init(string code, string file)
        {
            Init(code, file, null);
        }

        /// <summary>
        /// Parses the C# code.
        /// </summary>
        /// <param name="code">C# script (code or file).</param>
        /// <param name="file">If set to 'true' the script is a file, otherwise it is a C# code.</param>
        /// <param name="directivesToSearch">Additional C# script directives to search. The search result is stored in CSharpParser.CustomDirectives.</param>
        public void Init(string code, string file, string[] directivesToSearch)
        {
            this.code = code;

            //analyse comments and strings
            NoteCommentsAndStrings();

            //note the end of the header area (from the text start to the first class declaration)
            int pos = code.IndexOf("class");
            int endCodePos = code.Length - 1;
            while (pos != -1)
            {
                if (IsToken(pos, "class".Length) && !IsComment(pos))
                {
                    endCodePos = pos;
                    break;
                }
                pos = code.IndexOf("class", pos + 1);
            }

            //analyse script arguments
            foreach (string statement in GetRawStatements("//css_args", endCodePos))
            {
                foreach (string arg in statement.Split(','))
                {
                    string newArg = arg.Trim();
                    if (newArg.StartsWith("\""))
                        newArg = newArg.Substring(1);
                    if (newArg.EndsWith("\""))
                        newArg = newArg.Remove(newArg.Length - 1, 1);
                    args.Add(newArg);
                }
            }

            //analyse 'pre' and 'post' script commands
            foreach (string statement in GetRawStatements("//css_pre", endCodePos))
                cmdScripts.Add(new CmdScriptInfo(statement.Trim(), true, file));
            foreach (string statement in GetRawStatements("//css_prescript", endCodePos))
                cmdScripts.Add(new CmdScriptInfo(statement.Trim(), true, file));
            foreach (string statement in GetRawStatements("//css_post", endCodePos))
                cmdScripts.Add(new CmdScriptInfo(statement.Trim(), false, file));
            foreach (string statement in GetRawStatements("//css_postscript", endCodePos))
                cmdScripts.Add(new CmdScriptInfo(statement.Trim(), false, file));

            //analyse script imports/includes
            foreach (string statement in GetRawStatements("//css_import", endCodePos))
                imports.Add(new ImportInfo(Environment.ExpandEnvironmentVariables(statement).Trim(), file));
            foreach (string statement in GetRawStatements("//css_imp", endCodePos))
                imports.Add(new ImportInfo(Environment.ExpandEnvironmentVariables(statement).Trim(), file));
            foreach (string statement in GetRawStatements("//css_include", endCodePos))
                imports.Add(new ImportInfo(Environment.ExpandEnvironmentVariables(statement).Trim() + ",preserve_main", file));
            foreach (string statement in GetRawStatements("//css_inc", endCodePos))
                imports.Add(new ImportInfo(Environment.ExpandEnvironmentVariables(statement).Trim() + ",preserve_main", file));

            //analyse assembly references
            foreach (string statement in GetRawStatements("//css_reference", endCodePos))
                refAssemblies.Add(Environment.ExpandEnvironmentVariables(statement).Trim());
            foreach (string statement in GetRawStatements("//css_ref", endCodePos))
                refAssemblies.Add(Environment.ExpandEnvironmentVariables(statement).Trim());

            //analyse precompilers
            foreach (string statement in GetRawStatements("//css_precompiler", endCodePos))
                precompilers.Add(Environment.ExpandEnvironmentVariables(statement).Trim());

            foreach (string statement in GetRawStatements("//css_pc", endCodePos))
                precompilers.Add(Environment.ExpandEnvironmentVariables(statement).Trim());

            //analyse compiler options
            foreach (string statement in GetRawStatements("//css_co", endCodePos))
                compilerOptions.Add(Environment.ExpandEnvironmentVariables(statement).Trim());

            foreach (string statement in GetRawStatements("//css_host", endCodePos))
                hostOptions.Add(Environment.ExpandEnvironmentVariables(statement).Trim());

            //analyse assembly references
            foreach (string statement in GetRawStatements("//css_ignore_namespace", endCodePos))
                ignoreNamespaces.Add(statement.Trim());
            foreach (string statement in GetRawStatements("//css_ignore_ns", endCodePos))
                ignoreNamespaces.Add(statement.Trim());

            //analyse resource references
            foreach (string statement in GetRawStatements("//css_resource", endCodePos))
                resFiles.Add(statement.Trim());
            foreach (string statement in GetRawStatements("//css_res", endCodePos))
                resFiles.Add(statement.Trim());

            //analyse resource references
            foreach (string statement in GetRawStatements("//css_searchdir", endCodePos))
                searchDirs.Add(Environment.ExpandEnvironmentVariables(statement).Trim());
            foreach (string statement in GetRawStatements("//css_dir", endCodePos))
                searchDirs.Add(Environment.ExpandEnvironmentVariables(statement).Trim());

            //analyse namespace references
            foreach (string statement in GetRawStatements("using", endCodePos, true))
                if (!statement.StartsWith("(")) //just to cut off "using statements" as we are interested in "using directives" only
                    refNamespaces.Add(statement.Trim().Replace("\t", "").Replace("\r", "").Replace("\n", "").Replace(" ", ""));

            //analyse threading model
            pos = code.IndexOf("TAThread]");
            while (pos != -1 && pos > 0 && threadingModel == ApartmentState.Unknown)
            {
                if (!IsComment(pos - 1) && !IsString(pos - 1) && IsToken(pos - 2, "??TAThread]".Length))
                {
                    if (code[pos - 1] == 'S')
                        threadingModel = ApartmentState.STA;
                    else if (code[pos - 1] == 'M')
                        threadingModel = ApartmentState.MTA;
                }
                pos = code.IndexOf("TAThread]", pos + 1);
            }

            this.CustomDirectives.Clear();
            if (directivesToSearch != null)
            {
                foreach (string directive in directivesToSearch)
                {
#if net1
                    this.CustomDirectives[directive] = new ArrayList();
                    foreach (string statement in GetRawStatements(directive, endCodePos))
                        (this.CustomDirectives[directive] as ArrayList).Add(statement.Trim());
#else
                    this.CustomDirectives[directive] = new List<string>();
                    foreach (string statement in GetRawStatements(directive, endCodePos))
                        (this.CustomDirectives[directive] as List<string>).Add(statement.Trim());
#endif
                }
            }
        }

        private class RenamingInfo
        {
            public RenamingInfo(int stratPos, int endPos, string newValue)
            {
                this.stratPos = stratPos;
                this.endPos = endPos;
                this.newValue = newValue;
            }

            public int stratPos;
            public int endPos;
            public string newValue;
        }

#if net1
        private class RenamingInfoComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                int retval = (x == null) ? -1 : (y == null ? 1 : 0);

                if (retval == 0)
                    return Comparer.Default.Compare(((RenamingInfo)x).stratPos, ((RenamingInfo)y).stratPos);
                else
                    return retval;
            }
        }
#else

        private class RenamingInfoComparer : System.Collections.Generic.IComparer<RenamingInfo>
        {
            public int Compare(RenamingInfo x, RenamingInfo y)
            {
                int retval = (x == null) ? -1 : (y == null ? 1 : 0);

                if (retval == 0)
                    return Comparer.Default.Compare(x.stratPos, y.stratPos);
                else
                    return retval;
            }
        }

#endif

        /// <summary>
        /// Renames namespaces according renaming instructions.
        /// </summary>
        /// <param name="renamingMap">Renaming instructions (old_name vs. new_name).</param>
        /// <param name="preserveMain">/// If set to 'true' "static...Main" in the imported script is not renamed.</param>
        public void DoRenaming(string[][] renamingMap, bool preserveMain)
        {
            int renamingPos = -1;
#if net1
            ArrayList renamingPositions = new ArrayList();
#else
            List<RenamingInfo> renamingPositions = new List<RenamingInfo>();
#endif
            int pos = FindStatement("Main", 0);
            while (!preserveMain && pos != -1 && renamingPos == -1)
            {
                int declarationStart = code.LastIndexOfAny("{};".ToCharArray(), pos, pos);
                do
                {
                    if (!IsComment(declarationStart) || !IsString(declarationStart))
                    {
                        //test if it is "static void" Main
                        string statement = StripNonStringStatementComments(code.Substring(declarationStart + 1, pos - declarationStart - 1));
                        string[] tokens = statement.Trim().Split("\n\r\t ".ToCharArray());

                        foreach (string token in tokens)
                        {
                            if (token.Trim() == "static")
                                renamingPos = pos;
                        }
                        break;
                    }
                    else
                        declarationStart = code.LastIndexOfAny("{};".ToCharArray(), declarationStart - 1, declarationStart - 1);
                }
                while (declarationStart != -1 && renamingPos == -1);

                pos = FindStatement("Main", pos + 1);
            }
            if (renamingPos != -1)
                renamingPositions.Add(new RenamingInfo(renamingPos, renamingPos + "Main".Length, "i_Main"));

            foreach (string[] names in renamingMap)
            {
                renamingPos = -1;
                pos = FindStatement(names[0], 0);
                while (pos != -1 && renamingPos == -1)
                {
                    int declarationStart = code.LastIndexOfAny("{};".ToCharArray(), pos, pos);
                    do
                    {
                        if (!IsComment(declarationStart) || !IsString(declarationStart))
                        {
                            //test if it is "namespace" <name>
                            string test = code.Substring(declarationStart + 1, pos - declarationStart - 1);
                            string statement = StripNonStringStatementComments(code.Substring(declarationStart + 1, pos - declarationStart - 1));
                            string[] tokens = statement.Trim().Split("\n\r\t ".ToCharArray());

                            foreach (string token in tokens)
                            {
                                if (token.Trim() == "namespace")
                                {
                                    renamingPos = pos;
                                    break;
                                }
                            }
                            break;
                        }
                        else
                            declarationStart = code.LastIndexOfAny("{};".ToCharArray(), declarationStart - 1, declarationStart - 1);
                    }
                    while (declarationStart != -1 && renamingPos == -1);

                    pos = FindStatement(names[0], pos + 1);
                }
                if (renamingPos != -1)
                    renamingPositions.Add(new RenamingInfo(renamingPos, renamingPos + names[0].Length, names[1]));
            }

            renamingPositions.Sort(new RenamingInfoComparer());

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < renamingPositions.Count; i++)
            {
                //RenamingInfo info = (RenamingInfo)renamingPositions[i];
                //int prevEnd = ((i - 1) >= 0) ? ((RenamingInfo)renamingPositions[i - 1]).endPos : 0;
#if net1
                RenamingInfo info = (RenamingInfo)renamingPositions[i];
                int prevEnd = ((i - 1) >= 0) ? ((RenamingInfo)renamingPositions[i - 1]).endPos : 0;
#else
                RenamingInfo info = renamingPositions[i];
                int prevEnd = ((i - 1) >= 0) ? renamingPositions[i - 1].endPos : 0;
#endif

                sb.Append(code.Substring(prevEnd, info.stratPos - prevEnd));
                sb.Append(info.newValue);
                if (i == renamingPositions.Count - 1) // the last renaming
                    sb.Append(code.Substring(info.endPos, code.Length - info.endPos));
            }
            this.modifiedCode = sb.ToString();
        }

        /// <summary>
        /// Embedded script arguments. The both script and engine arguments are allowed except "/noconfig" engine command line switch.
        /// </summary>
        public string[] Args
        {
            get
            {
#if net1
                return (string[])args.ToArray(typeof(string));
#else
                return args.ToArray();
#endif
            }
        }

        /// <summary>
        /// Embedded compiler options.
        /// </summary>
        public string[] CompilerOptions
        {
            get
            {
#if net1
                return (string[])compilerOptions.ToArray(typeof(string));
#else
                return compilerOptions.ToArray();
#endif
            }
        }

        /// <summary>
        /// Embedded compiler options.
        /// </summary>
        public string[] HostOptions
        {
            get
            {
#if net1
                return (string[])hostOptions.ToArray(typeof(string));
#else
                return hostOptions.ToArray();
#endif
            }
        }
        
        /// <summary>
        /// Precompilers.
        /// </summary>
        public string[] Precompilers
        {
            get
            {
#if net1
                return (string[])precompilers.ToArray(typeof(string));
#else
                return precompilers.ToArray();
#endif
            }
        }
        /// <summary>
        /// References to the external assemblies and namespaces.
        /// </summary>
        public string[] References
        {
            get
            {
#if net1
                ArrayList retval = new ArrayList();
                retval.AddRange(refAssemblies);
                retval.AddRange(refNamespaces);
                return (string[])retval.ToArray(typeof(string));
#else
                List<string> retval = new List<string>();
                retval.AddRange(refAssemblies);
                retval.AddRange(refNamespaces);
                return retval.ToArray();
#endif
            }
        }

        /// <summary>
        /// References to the external assemblies.
        /// </summary>
        public string[] RefAssemblies
        {
#if net1
            get { return (string[])refAssemblies.ToArray(typeof(string)); }
#else
            get { return refAssemblies.ToArray(); }
#endif
        }

        /// <summary>
        /// Names of namespaces to be ignored by namespace-to-assembly resolver.
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
        /// Additional search directories (for script and assembly probing).
        /// </summary>
        public string[] ExtraSearchDirs
        {
#if net1
            get { return (string[])searchDirs.ToArray(typeof(string)); }
#else
            get { return searchDirs.ToArray(); }
#endif
        }

        /// <summary>
        /// References to the resource files.
        /// </summary>
        public string[] ResFiles
        {
#if net1
            get { return (string[])resFiles.ToArray(typeof(string)); }
#else
            get { return resFiles.ToArray(); }
#endif
        }

        /// <summary>
        /// References to the namespaces.
        /// </summary>
        public string[] RefNamespaces
        {
#if net1
            get { return (string[])refNamespaces.ToArray(typeof(string)); }
#else
            get { return refNamespaces.ToArray(); }
#endif
        }

        /// <summary>
        /// C# scripts to be imported.
        /// </summary>
        public ImportInfo[] Imports
        {
#if net1
            get { return (ImportInfo[])imports.ToArray(typeof(ImportInfo)); }
#else
            get { return imports.ToArray(); }
#endif
        }

        /// <summary>
        /// Pre- and post-execution scripts.
        /// </summary>
        public CmdScriptInfo[] CmdScripts
        {
#if net1
            get { return (CmdScriptInfo[])cmdScripts.ToArray(typeof(CmdScriptInfo)); }
#else
            get { return cmdScripts.ToArray(); }
#endif
        }

        /// <summary>
        /// Appartment state of the script.
        /// </summary>
        public ApartmentState ThreadingModel
        {
            get { return threadingModel; }
        }

        /// <summary>
        /// Script C# raw code.
        /// </summary>
        public string Code
        {
            get { return code; }
        }

        /// <summary>
        /// Script C# code after namespace renaming.
        /// </summary>
        public string ModifiedCode
        {
            get { return modifiedCode; }
        }

        #endregion Public interface

#if net1
        ArrayList searchDirs = new ArrayList();
        ArrayList resFiles = new ArrayList();
        ArrayList refAssemblies = new ArrayList();
        ArrayList compilerOptions = new ArrayList();
        ArrayList hostOptions = new ArrayList();
        ArrayList cmdScripts = new ArrayList();
        ArrayList refNamespaces = new ArrayList();
        ArrayList ignoreNamespaces = new ArrayList();
        ArrayList imports = new ArrayList();
        ArrayList precompilers = new ArrayList();
        ArrayList args = new ArrayList();
#else
        List<string> searchDirs = new List<string>();
        List<string> resFiles = new List<string>();
        List<string> refAssemblies = new List<string>();
        List<string> compilerOptions = new List<string>();
        List<string> hostOptions = new List<string>();
        List<CmdScriptInfo> cmdScripts = new List<CmdScriptInfo>();
        List<string> refNamespaces = new List<string>();
        List<string> ignoreNamespaces = new List<string>();
        List<ImportInfo> imports = new List<ImportInfo>();
        List<string> precompilers = new List<string>();
        List<string> args = new List<string>();
#endif

        ApartmentState threadingModel = ApartmentState.Unknown;
        string code = "";
        string modifiedCode = "";

        /// <summary>
        /// Enables omitting closing character (";") for CS-Script directives (e.g. "//css_ref System.Xml.dll" instead of "//css_ref System.Xml.dll;").
        /// </summary>
        public static bool OpenEndDirectiveSyntax = true;

        string[] GetRawStatements(string pattern, int endIndex)
        {
            return GetRawStatements(pattern, endIndex, false);
        }

        string[] GetRawStatements(string pattern, int endIndex, bool ignoreComments)
        {
#if net1
            ArrayList retval = new ArrayList();
#else
            List<string> retval = new List<string>();
#endif

            int pos = code.IndexOf(pattern);
            int endPos = -1;
            while (pos != -1 && pos <= endIndex)
            {
                if (IsToken(pos, pattern.Length))
                {
                    if (!ignoreComments || (ignoreComments && !IsComment(pos)))
                    {
                        pos += pattern.Length;

                        if (OpenEndDirectiveSyntax)
                        {
                            int endOfLine = code.IndexOf("\r", pos);
                            if (endOfLine != -1)
                                endPos = Math.Min(code.IndexOf(";", pos), endOfLine);
                            else
                                endPos = code.IndexOf(";", pos);
                        }
                        else
                            endPos = code.IndexOf(";", pos);
                            
                        if (endPos != -1)
                            retval.Add(code.Substring(pos, endPos - pos).Trim());
                    }
                }
                pos = code.IndexOf(pattern, pos + 1);
            }
#if net1
            return (string[])retval.ToArray(typeof(string));
#else
            return retval.ToArray();
#endif
        }

        int[] AllRawIndexOf(string pattern, int startIndex, int endIndex) //all raw matches
        {
#if net1
            ArrayList retval = new ArrayList();
#else
            List<int> retval = new List<int>();
#endif

            int pos = code.IndexOf(pattern, startIndex, endIndex - startIndex);
            while (pos != -1)
            {
                retval.Add(pos);
                pos = code.IndexOf(pattern, pos + 1, endIndex - (pos + 1));
            }
#if net1
            return (int[])retval.ToArray(typeof(int));
#else
            return retval.ToArray();
#endif
        }

        int IndexOf(string pattern, int startIndex, int endIndex) //non-comment match
        {
            int pos = code.IndexOf(pattern, startIndex, endIndex - startIndex);
            while (pos != -1)
            {
                if (!IsComment(pos) && IsToken(pos, pattern.Length))
                    return pos;

                pos = code.IndexOf(pattern, pos + 1, endIndex - (pos + 1));
            }
            return -1;
        }

        bool IsComment(int charPos)
        {
            foreach (int[] region in commentRegions)
            {
                if (charPos < region[0])
                    return false;
                else if (region[0] <= charPos && charPos <= region[1])
                    return true;
            }
            return false;
        }

        bool IsString(int charPos)
        {
            foreach (int[] region in stringRegions)
            {
                if (charPos < region[0])
                    return false;
                else if (region[0] <= charPos && charPos <= region[1])
                    return true;
            }
            return false;
        }

        bool IsToken(int startPos, int length)
        {
            if (code.Length < startPos + length)
                return false;

            int probeStart = (startPos != 0) ? startPos - 1 : 0;
            int endPos = (code.Length == startPos + length) ? startPos + length : startPos + length + 1;

            string original = code.Substring(startPos, length);
            string probeStr = code.Substring(probeStart, endPos - probeStart);

            probeStr = probeStr.Replace(";", "").Replace("(", "").Replace(")", "").Replace("{", "");
            probeStr = probeStr.Trim();

            return probeStr.Length == original.Length;
        }

        void NoteCommentsAndStrings()
        {
#if net1
            ArrayList quotationChars = new ArrayList();
#else
            List<int> quotationChars = new List<int>();
#endif
            int startPos = -1;
            int startSLC = -1; //single line comment
            int startMLC = -1; //multiple line comment
            int searchOffset = 0;
            string endToken = "";
            string startToken = "";
            int endPos = -1;
            int lastEndPos = -1;
            do
            {
                startSLC = code.IndexOf("//", searchOffset);
                startMLC = code.IndexOf("/*", searchOffset);

                if (startSLC == Math.Min(startSLC != -1 ? startSLC : Int16.MaxValue,
                                         startMLC != -1 ? startMLC : Int16.MaxValue))
                {
                    startPos = startSLC;
                    startToken = "//";
                    endToken = "\n";
                }
                else
                {
                    startPos = startMLC;
                    startToken = "/*";
                    endToken = "*/";
                }

                if (startPos != -1)
                    endPos = code.IndexOf(endToken, startPos + startToken.Length);

                if (startPos != -1 && endPos != -1)
                {
                    int startCode = commentRegions.Count == 0 ? 0 : ((int[])commentRegions[commentRegions.Count - 1])[1] + 1;

                    int[] quotationIndexes = AllRawIndexOf("\"", startCode, startPos);
                    if ((quotationIndexes.Length % 2) != 0)
                    {
                        searchOffset = startPos + startToken.Length;
                        continue;
                    }

                    //string comment = code.Substring(startPos, endPos - startPos);
                    commentRegions.Add(new int[2] { startPos, endPos });
                    quotationChars.AddRange(quotationIndexes);

                    searchOffset = endPos + endToken.Length;
                }
            }
            while (startPos != -1 && endPos != -1);

            if (lastEndPos != 0 && searchOffset < code.Length)
            {
                quotationChars.AddRange(AllRawIndexOf("\"", searchOffset, code.Length));
            }

            for (int i = 0; i < quotationChars.Count; i++)
            {
#if net1
                if (i + 1 < stringRegions.Count)
                    stringRegions.Add(new int[] { (int)quotationChars[i], (int)quotationChars[i + 1] });
                else
                    stringRegions.Add(new int[] { (int)quotationChars[i], -1 });
#else
                if (i + 1 < stringRegions.Count)
                    stringRegions.Add(new int[] { quotationChars[i], quotationChars[i + 1] });
                else
                    stringRegions.Add(new int[] { quotationChars[i], -1 });
#endif
                i++;
            }
        }

        int FindStatement(string pattern, int start)
        {
            if (code.Length == 0)
                return -1;

            int pos = IndexOf(pattern, start, code.Length - 1);
            while (pos != -1)
            {
                if (!IsString(pos))
                    return pos;
                else
                    pos = IndexOf(pattern, pos + 1, code.Length - 1);
            }
            return -1;
        }

        string StripNonStringStatementComments(string text)
        {
            StringBuilder sb = new StringBuilder();
            int startPos = -1;
            int startSLC = -1; //single line comment
            int startMLC = -1; //multiple line comment
            int searchOffset = 0;
            string endToken = "";
            string startToken = "";
            int endPos = -1;
            int lastEndPos = -1;
            do
            {
                startSLC = text.IndexOf("//", searchOffset);
                startMLC = text.IndexOf("/*", searchOffset);

                if (startSLC == Math.Min(startSLC != -1 ? startSLC : Int16.MaxValue,
                                         startMLC != -1 ? startMLC : Int16.MaxValue))
                {
                    startPos = startSLC;
                    startToken = "//";
                    endToken = "\n";
                }
                else
                {
                    startPos = startMLC;
                    startToken = "/*";
                    endToken = "*/";
                }

                if (startPos != -1)
                    endPos = text.IndexOf(endToken, startPos + startToken.Length);

                if (startPos != -1 && endPos != -1)
                {
                    string codeFragment = text.Substring(searchOffset, startPos - searchOffset);
                    sb.Append(codeFragment);

                    searchOffset = endPos + endToken.Length;
                }
            }
            while (startPos != -1 && endPos != -1);

            if (lastEndPos != 0 && searchOffset < code.Length)
            {
                string codeFragment = text.Substring(searchOffset, text.Length - searchOffset);
                sb.Append(codeFragment);
            }
            return sb.ToString();
        }
    }

    #endregion CSharpParser...
}