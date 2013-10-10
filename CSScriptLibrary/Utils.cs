#region Licence...

//-----------------------------------------------------------------------------
// Date:	25/10/10
// Module:	Utils.cs
// Classes:	...
//
// This module contains the definition of the utility classes used by CS-Script modules
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
using System.Reflection;

#if !net1

using System.Collections.Generic;
using System.Linq;

#endif

using System.Text;
using CSScriptLibrary;
using System.Runtime.InteropServices;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Globalization;
using System.Threading;
using System.Collections;

namespace csscript
{
    internal class CurrentDirGuard : IDisposable
    {
        string currentDir = Environment.CurrentDirectory;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
                Environment.CurrentDirectory = currentDir;

            disposed = true;
        }

        ~CurrentDirGuard()
        {
            Dispose(false);
        }

        bool disposed = false;
    }

    internal class Utils
    {
        //unfortunately LINQ is not available for .NET 1.1 compilations
        public static string[] Concat(string[] array1, string[] array2)
        {
            string[] retval = new string[array1.Length + array2.Length];
            Array.Copy(array1, 0, retval, 0, array1.Length);
            Array.Copy(array2, 0, retval, array1.Length, array2.Length);
            return retval;
        }

        public static string[] Concat(string[] array1, string item)
        {
            string[] retval = new string[array1.Length + 1];
            Array.Copy(array1, 0, retval, 0, array1.Length);
            retval[retval.Length - 1] = item;
            return retval;
        }

        public static string[] RemovePathDuplicates(string[] list)
        {
            System.Collections.ArrayList retval = new System.Collections.ArrayList();
            foreach (string item in list)
            {
                string path = Path.GetFullPath(item.Trim());

                bool found = false;
                foreach (string pathItem in retval)
                    if (Utils.IsSamePath(pathItem, path))
                    {
                        found = true;
                        break;
                    }
                if (!found)
                    retval.Add(path);
            }

            return (string[])retval.ToArray(typeof(string));
        }

        public static string[] RemoveDuplicates(string[] list)
        {
            System.Collections.ArrayList retval = new System.Collections.ArrayList();
            foreach (string item in list)
            {
                if (item.Trim() != "")
                {
                    if (!retval.Contains(item))
                        retval.Add(item);
                }
            }

            return (string[])retval.ToArray(typeof(string));
        }

        public static string[] RemoveEmptyStrings(string[] list)
        {
            System.Collections.ArrayList retval = new System.Collections.ArrayList();
            foreach (string item in list)
            {
                if (item.Trim() != "")
                    retval.Add(item);
            }

            return (string[])retval.ToArray(typeof(string));
        }

        //to avoid throwing the exception
        public static string GetAssemblyDirectoryName(Assembly asm)
        {
            if (asm.Location == null || asm.Location == "")
                return "";
            else
                return Path.GetDirectoryName(asm.Location);
        }

        //to avoid throwing the exception
        public static string GetAssemblyFileName(Assembly asm)
        {
            if (asm.Location == null || asm.Location == "")
                return "";
            else
                return Path.GetFileName(asm.Location);
        }

        public static string RemoveAssemblyExtension(string asmName)
        {
#if net1
            if (asmName.ToLower().EndsWith(".dll") || asmName.ToLower().EndsWith(".exe"))
#else
            if (asmName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase) || asmName.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase))
#endif
                return asmName.Substring(0, asmName.Length - 4);
            else
                return asmName;
        }

        public static int PathCompare(string path1, string path2)
        {
            if (Utils.IsLinux())
                return string.Compare(path1, path2);
            else
                return string.Compare(path1, path2, true);
        }

        public static bool IsSamePath(string path1, string path2)
        {
            return PathCompare(path1, path2) == 0;
        }

        public static bool IsLinux()
        {
            return (Environment.OSVersion.Platform == PlatformID.Unix);
        }

        public static bool ContainsPath(string path, string subPath)
        {
            return PathCompare(path.Substring(0, subPath.Length), subPath) == 0;
        }

        /// <summary>
        /// Adds compiler options to the CompilerParameters in a manner that it does separate every option by the space character
        /// </summary>
        static public void AddCompilerOptions(CompilerParameters compilerParams, string option)
        {
            compilerParams.CompilerOptions += option + " ";
        }

        ///// <summary>
        ///// More reliable version of the Path.GetTempFileName().
        ///// It is required because it was some reports about non unique names returned by Path.GetTempFileName()
        ///// when running in multi-threaded environment.
        ///// (it is not used yet as I did not give up on PInvoke GetTempFileName())
        ///// </summary>
        ///// <returns>Temporary file name.</returns>
        //string PathGetTempFileName()
        //{
        //    return Path.GetTempPath() + Guid.NewGuid().ToString() + ".tmp";
        //}
    }

    internal class CSSUtils
    {
        internal static void VerbosePrint(string message, ExecuteOptions options)
        {
            if (options.verbose)
                Console.WriteLine(message);
        }

        internal static string GetScriptedCodeAttributeInjectionCode(string scriptFileName)
        {
            string code = string.Format("[assembly: System.Reflection.AssemblyDescriptionAttribute(@\"{0}\")]", scriptFileName);
            string currentCode = "";
            string file = Path.Combine(CSExecutor.GetCacheDirectory(scriptFileName), Path.GetFileNameWithoutExtension(scriptFileName) + ".attr.g.cs");

            if (File.Exists(file))
                using (StreamReader sr = new StreamReader(file))
                    currentCode = sr.ReadToEnd();

            if (currentCode != code)
            {
                string dir = Path.GetDirectoryName(file);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using (StreamWriter sw = new StreamWriter(file))
                {
                    sw.Write(code);
                }
            }
            return file;
        }

        public static bool HaveSameTimestamp(string file1, string file2)
        {
            FileInfo info1 = new FileInfo(file1);
            FileInfo info2 = new FileInfo(file2);

            return (info2.LastWriteTime == info1.LastWriteTime &&
                    info2.LastWriteTimeUtc == info1.LastWriteTimeUtc);
        }

        public static void SetTimestamp(string fileDest, string fileSrc)
        {
            FileInfo info1 = new FileInfo(fileSrc);
            FileInfo info2 = new FileInfo(fileDest);

            info2.LastWriteTime = info1.LastWriteTime;
            info2.LastWriteTimeUtc = info1.LastWriteTimeUtc;
        }

        public delegate void ShowDocumentHandler();

        static internal string cmdFlagPrefix
        {
            get
            {
                if (Utils.IsLinux())
                    return "-";
                else
                    return "/";
            }
        }

        /// <summary>
        /// Parses application (script engine) arguments.
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <param name="executor">Script executor instance</param>
        /// <returns>Index of the first script argument.</returns>
        static internal int ParseAppArgs(string[] args, IScriptExecutor executor)
        {
            ExecuteOptions options = executor.GetOptions();
            //Debug.Assert(false);
            for (int i = 0; i < args.Length; i++)
            {
                if (File.Exists(args[i]))
                    return i; //on Linux '/' may indicate dir but not command

                if (args[i].StartsWith(cmdFlagPrefix))
                {
                    if (args[i] == cmdFlagPrefix + "nl") // -nl
                    {
                        options.noLogo = true;
                    }
                    else if (args[i] == cmdFlagPrefix + "c" && (!options.supressExecution)) // -c
                    {
                        options.useCompiled = true;
                    }
                    else if (args[i] == cmdFlagPrefix + "sconfig")// -sconfig
                    {
                        options.useScriptConfig = true;
                    }
                    else if (args[i].StartsWith(cmdFlagPrefix + "sconfig:")) // -sconfig:file
                    {
                        options.useScriptConfig = true;
                        options.customConfigFileName = args[i].Substring((cmdFlagPrefix + "sconfig:").Length);
                    }
                    else if (args[i] == cmdFlagPrefix + "verbose")
                    {
                        options.verbose = true;
                    }
                    else if (args[i].StartsWith(cmdFlagPrefix + "dir:")) // -dir:path1,path2
                    {
                        foreach (string dir in args[i].Substring((cmdFlagPrefix + "dir:").Length).Split(','))
                            options.AddSearchDir(dir.Trim());
                    }
                    else if (args[i].StartsWith(cmdFlagPrefix + "precompiler"))
                    {
                        if (args[i].StartsWith(cmdFlagPrefix + "precompiler:")) // -precompiler:file1,file2
                        {
                            options.preCompilers = args[i].Substring((cmdFlagPrefix + "precompiler:").Length);
                        }
                        else
                        {
                            executor.ShowPrecompilerSample();
                            options.processFile = false;
                        }
                    }
                    else if (args[i].StartsWith(cmdFlagPrefix + "pc:")) // -pc:
                    {
                        options.preCompilers = args[i].Substring((cmdFlagPrefix + "pc:").Length);
                    }
                    else if (args[i].StartsWith(cmdFlagPrefix + "noconfig"))// -noconfig:file
                    {
                        options.noConfig = true;
                        if (args[i].StartsWith(cmdFlagPrefix + "noconfig:"))
                            options.altConfig = args[i].Substring((cmdFlagPrefix + "noconfig:").Length);
                    }
                    else if (args[i] == cmdFlagPrefix + "autoclass" || args[i] == cmdFlagPrefix + "ac") // -autoclass -ac
                    {
                        options.autoClass = true;
                    }
                    else if (args[i].StartsWith(cmdFlagPrefix + "ca")) // -ca
                    {
                        options.useCompiled = true;
                        options.forceCompile = true;
                        options.supressExecution = true;
                    }
                    else if (args[i].StartsWith(cmdFlagPrefix + "co:")) // -co
                    {
                        options.compilerOptions = args[i].Substring((cmdFlagPrefix + "co:").Length);
                    }
                    else if (args[i].StartsWith(cmdFlagPrefix + "cd")) // -cd
                    {
                        options.supressExecution = true;
                        options.DLLExtension = true;
                    }
                    else if (args[i] == cmdFlagPrefix + "dbg" || args[i] == cmdFlagPrefix + "d") // -dbg -d
                    {
                        options.DBG = true;
                    }
                    else if (args[i] == cmdFlagPrefix + "l")
                    {
                        options.local = true;
                    }
                    else if (args[i] == cmdFlagPrefix + "v" || args[i] == cmdFlagPrefix + "V") // -v
                    {
                        executor.ShowVersion();
                        options.processFile = false;
                        options.versionOnly = true;
                    }
                    else if (args[i].StartsWith(cmdFlagPrefix + "r:")) // -r:file1,file2
                    {
                        string[] assemblies = args[i].Remove(0, 3).Split(",;".ToCharArray()); //important change
                        options.refAssemblies = assemblies;
                    }
                    else if (args[i].StartsWith(cmdFlagPrefix + "e") && !options.buildExecutable) // -e
                    {
                        options.buildExecutable = true;
                        options.supressExecution = true;
                        options.buildWinExecutable = args[i].StartsWith(cmdFlagPrefix + "ew"); // -ew
                    }
                    else if (args[0] == cmdFlagPrefix + "?" || args[0] == cmdFlagPrefix + "help") // -? -help
                    {
                        executor.ShowHelp();
                        options.processFile = false;
                        break;
                    }
                    else if (args[0] == cmdFlagPrefix + "s") // -s
                    {
                        executor.ShowSample();
                        options.processFile = false;
                        break;
                    }
                }
                else
                {
                    return i;
                }
            }

            return args.Length;
        }

        private delegate bool CompileMethod(ref string content, string scriptFile, bool IsPrimaryScript, Hashtable context);

        public static PrecompilationContext Precompile(string scriptFile, string[] filesToCompile, ExecuteOptions options)
        {
            PrecompilationContext context = new PrecompilationContext();
            context.SearchDirs = options.searchDirs;

            Hashtable contextData = new Hashtable();
            contextData["NewDependencies"] = context.NewDependencies;
            contextData["NewSearchDirs"] = context.NewSearchDirs;
            contextData["NewReferences"] = context.NewReferences;
            contextData["SearchDirs"] = context.SearchDirs;

#if net1
            System.Collections.Hashtable precompilers = CSSUtils.LoadPrecompilers(options);
#else
            Dictionary<string, List<object>> precompilers = CSSUtils.LoadPrecompilers(options);
#endif
            if (precompilers.Count != 0)
            {
                for (int i = 0; i < filesToCompile.Length; i++)
                {
                    string content = File.ReadAllText(filesToCompile[i]);

                    bool modified = false;
                    foreach (string precompilerFile in precompilers.Keys)
                    {
#if net1
                        foreach (object precompiler in precompilers[precompilerFile] as ArrayList)
#else
                        foreach (object precompiler in precompilers[precompilerFile])
#endif
                        {
                            if (options.verbose && i == 0)
                            {
                                CSSUtils.VerbosePrint("  Precompilers: ", options);
                                int index = 0;
                                foreach (string file in filesToCompile)
                                    CSSUtils.VerbosePrint("   " + index++ + " - " + precompiler.GetType() + "\n       " + precompilerFile, options);
                                CSSUtils.VerbosePrint("", options);
                            }

                            MethodInfo method = precompiler.GetType().GetMethod("Compile");
                            CompileMethod compile = (CompileMethod)Delegate.CreateDelegate(typeof(CompileMethod), method);
                            bool result = compile(ref content,
                                                  filesToCompile[i],
                                                  filesToCompile[i] == scriptFile,
                                                  contextData);

                            if (result)
                            {
                                context.NewDependencies.Add(precompilerFile);
                                modified = true;
                            }
                        }
                    }

                    if (modified)
                    {
                        filesToCompile[i] = CSSUtils.SaveAsAutogeneratedScript(content, filesToCompile[i]);
                    }
                }
            }

            options.searchDirs = Utils.Concat(options.searchDirs, context.NewSearchDirs.ToArray());

            foreach (string asm in context.NewReferences)
                options.defaultRefAssemblies += "," + asm; //the easiest way to inject extra references is to merge them with the extra assemblies already specified by user

            return context;
        }

        internal const string noDefaultPrecompilerSwitch = "nodefault";

#if net1
        public static System.Collections.Hashtable LoadPrecompilers(ExecuteOptions options)
        {
            System.Collections.Hashtable retval = new System.Collections.Hashtable();
            if (!options.preCompilers.StartsWith(noDefaultPrecompilerSwitch)) //no defaults
            {
                ArrayList compilers = new ArrayList();
                compilers.Add(new DefaultPrecompiler());
                retval.Add(Assembly.GetExecutingAssembly().Location, compilers);
            }

            if (options.autoClass)
            {
                if (retval.ContainsKey(Assembly.GetExecutingAssembly().Location))
                    (retval[Assembly.GetExecutingAssembly().Location] as ArrayList).Add(new AutoclassPrecompiler());
                else
                {
                    ArrayList compilers = new ArrayList();
                    compilers.Add(new AutoclassPrecompiler());
                    retval.Add(Assembly.GetExecutingAssembly().Location, compilers);
                }
            }
#else

        public static Dictionary<string, List<object>> LoadPrecompilers(ExecuteOptions options)
        {
            Dictionary<string, List<object>> retval = new Dictionary<string, List<object>>();

            if (!options.preCompilers.StartsWith(noDefaultPrecompilerSwitch)) //no defaults
                retval.Add(Assembly.GetExecutingAssembly().Location, new List<object>() { new DefaultPrecompiler() });

            if (options.autoClass)
            {
                if (retval.ContainsKey(Assembly.GetExecutingAssembly().Location))
                    retval[Assembly.GetExecutingAssembly().Location].Add(new AutoclassPrecompiler());
                else
                    retval.Add(Assembly.GetExecutingAssembly().Location, new List<object>() { new AutoclassPrecompiler() });
            }
#endif

            foreach (string precompiler in Utils.RemoveDuplicates((options.preCompilers).Split(new char[] { ',' })))
            {
                string precompilerFile = precompiler.Trim();

                if (precompilerFile != "" && precompilerFile != noDefaultPrecompilerSwitch)
                {
                    string sourceFile = FindImlementationFile(precompilerFile, options.searchDirs);
                    if (sourceFile == null)
                        throw new ApplicationException("Cannot find Precompiler file " + precompilerFile);

                    Assembly asm;
                    if (sourceFile.EndsWith(".dll", true, CultureInfo.InvariantCulture))
                        asm = Assembly.LoadFrom(sourceFile);
                    else
                        asm = CompilePrecompilerScript(sourceFile, options.searchDirs);

                    //string typeName = typeof(IPrecompiler).Name;

                    object precompilerObj = null;
                    foreach (Module m in asm.GetModules())
                    {
                        if (precompilerObj != null)
                            break;

                        foreach (Type t in m.GetTypes())
                        {
                            if (t.Name.EndsWith("Precompiler"))
                            {
                                precompilerObj = asm.CreateInstance(t.Name);
                                if (precompilerObj == null)
                                    throw new Exception("Precompiler " + sourceFile + " cannot be loaded. CreateInstance returned null.");
                                break;
                            }
                        }
                    }

#if net1
                    if (precompilerObj != null)
                    {
                        ArrayList compilers = new ArrayList();
                        compilers.Add(precompilerObj);
                        retval.Add(sourceFile, compilers);
                    }
#else
                    if (precompilerObj != null)
                        retval.Add(sourceFile, new List<object>() { precompilerObj });
#endif
                }
            }

            return retval;
        }

        public static string FindFile(string file, string[] searchDirs)
        {
            if (File.Exists(file))
            {
                return Path.GetFullPath(file);
            }
            else if (!Path.IsPathRooted(file))
            {
                foreach (string dir in searchDirs)
                    if (File.Exists(Path.Combine(dir, file)))
                        return Path.Combine(dir, file);
            }
            return null;
        }

        public static string FindImlementationFile(string file, string[] searchDirs)
        {
            string retval = FindFile(file, searchDirs);

            if (retval == null && !Path.HasExtension(file))
            {
                retval = FindFile(file + ".cs", searchDirs);
                if (retval == null)
                    retval = FindFile(file + ".dll", searchDirs);
            }
            return retval;
        }

        public static string[] CollectPrecompillers(CSharpParser parser, ExecuteOptions options)
        {
#if net1
            ArrayList allPrecompillers = new ArrayList();
#else
            List<string> allPrecompillers = new List<string>();
#endif
            allPrecompillers.AddRange(options.preCompilers.Split(','));

            foreach (string item in parser.Precompilers)
                allPrecompillers.AddRange(item.Split(','));

#if net1
            return Utils.RemoveDuplicates((string[])allPrecompillers.ToArray(typeof(string)));
#else
            return Utils.RemoveDuplicates(allPrecompillers.ToArray());
#endif
        }

        public static int GenerateCompilationContext(CSharpParser parser, ExecuteOptions options)
        {
            string[] allPrecompillers = CollectPrecompillers(parser, options);

            StringBuilder sb = new StringBuilder();

            foreach (string file in allPrecompillers)
            {
                if (file != "")
                {
                    sb.Append(FindImlementationFile(file, options.searchDirs));
                    sb.Append(",");
                }
            }

            return sb.ToString().GetHashCode();
        }
#if !net1
        public static string[] GetAppDomainAssemblies()
        {
            return (from a in AppDomain.CurrentDomain.GetAssemblies()
                    where !(a is System.Reflection.Emit.AssemblyBuilder)  //in .NET4.0 IsDynamic can be used
                       && !a.GlobalAssemblyCache && a.GetType().FullName != "System.Reflection.Emit.InternalAssemblyBuilder" //http://bloggingabout.net/blogs/vagif/archive/2010/07/02/net-4-0-and-notsupportedexception-complaining-about-dynamic-assemblies.aspx
                    select a.Location).ToArray();
        }
#endif
        public static Assembly CompilePrecompilerScript(string sourceFile, string[] searchDirs)
        {
            try
            {
                string precompilerAsm = Path.Combine(CSExecutor.GetCacheDirectory(sourceFile), Path.GetFileName(sourceFile) + ".compiled");

                using (Mutex fileLock = new Mutex(false, "CSSPrecompiling." + precompilerAsm.GetHashCode())) //have to use hash code as path delimiters are illegal in the mutex name
                {
                    //let other thread/process (if any) to finish loading/compiling the same file; 3 seconds should be enough
                    //if not we will just fail to compile as precompilerAsm will still be locked
                    fileLock.WaitOne(3000, false);

                    if (File.Exists(precompilerAsm))
                    {
                        if (File.GetLastWriteTimeUtc(sourceFile) <= File.GetLastWriteTimeUtc(precompilerAsm))
                            return Assembly.LoadFrom(precompilerAsm);

                        File.Delete(precompilerAsm);
                    }

                    ScriptParser parser = new ScriptParser(sourceFile, searchDirs);
                    CompilerParameters compilerParams = new CompilerParameters();

                    compilerParams.IncludeDebugInformation = true;
                    compilerParams.GenerateExecutable = false;
                    compilerParams.GenerateInMemory = false;
                    compilerParams.OutputAssembly = precompilerAsm;
#if net1
                    ArrayList refAssemblies = new ArrayList();
#else
                    List<string> refAssemblies = new List<string>();
#endif

                    //add local and global assemblies (if found) that have the same assembly name as a namespace
                    foreach (string nmSpace in parser.ReferencedNamespaces)
                        foreach (string asm in AssemblyResolver.FindAssembly(nmSpace, searchDirs))
                            refAssemblies.Add(asm);

                    //add assemblies referenced from code
                    foreach (string asmName in parser.ReferencedAssemblies)
                        if (asmName.StartsWith("\"") && asmName.EndsWith("\"")) //absolute path
                        {
                            //not-searchable assemblies
                            string asm = asmName.Replace("\"", "");
                            refAssemblies.Add(asm);
                        }
                        else
                        {
                            string nameSpace = Utils.RemoveAssemblyExtension(asmName);

                            string[] files = AssemblyResolver.FindAssembly(nameSpace, searchDirs);
                            if (files.Length > 0)
                                foreach (string asm in files)
                                    refAssemblies.Add(asm);
                            else
                                refAssemblies.Add(nameSpace + ".dll");
                        }

                    ////////////////////////////////////////
#if net1
                foreach (string asm in Utils.RemovePathDuplicates((string[])refAssemblies.ToArray(typeof(string))))
#else
                    foreach (string asm in Utils.RemovePathDuplicates(refAssemblies.ToArray()))
#endif
                    {
                        compilerParams.ReferencedAssemblies.Add(asm);
                    }

#pragma warning disable 618
                    CompilerResults result = new CSharpCodeProvider().CreateCompiler().CompileAssemblyFromFile(compilerParams, sourceFile);
#pragma warning restore 618

                    if (result.Errors.Count != 0)
                        throw CompilerException.Create(result.Errors, true);

                    if (!File.Exists(precompilerAsm))
                        throw new Exception("Unknown building error");

                    File.SetLastWriteTimeUtc(precompilerAsm, File.GetLastWriteTimeUtc(sourceFile));

                    Assembly retval = Assembly.LoadFrom(precompilerAsm);

                    return retval;
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException("Cannot load precompiler " + sourceFile + ": " + e.Message);
            }
        }

        public static string SaveAsAutogeneratedScript(string content, string originalFileName)
        {
            string autogenFile = Path.Combine(CSExecutor.GetCacheDirectory(originalFileName), Path.GetFileNameWithoutExtension(originalFileName) + ".g" + Path.GetExtension(originalFileName));
            using (StreamWriter sw = new StreamWriter(autogenFile, false, Encoding.Default))
                sw.Write(content);
            return autogenFile;
        }

        public static string GenerateAutoclass(string file)
        {
            StringBuilder code = new StringBuilder(4096);
            code.Append("//Auto-generated file\r\n"); //cannot use AppendLine as it is not available in StringBuilder v1.1
            //code.Append("using System;\r\n");

            bool headerProcessed = false;
            string line;
            using (StreamReader sr = new StreamReader(file, Encoding.Default))
                while ((line = sr.ReadLine()) != null)
                {
                    if (!headerProcessed && !line.TrimStart().StartsWith("using ")) //not using...; statement of the file header
                        if (!line.StartsWith("//") && line.Trim() != "") //not comments or empty line
                        {
                            headerProcessed = true;
                            //code.Append("namespace Scripting\r\n");
                            //code.Append("{\r\n");
                            code.Append("   public class ScriptClass\r\n");
                            code.Append("   {\r\n");
                            code.Append("   static public ");
                        }

                    code.Append(line);
                    code.Append("\r\n");
                }

            code.Append("   }\r\n");

            string autogenFile = SaveAsAutogeneratedScript(code.ToString(), file);

            return autogenFile;
        }
    }

    #region MetaDataItems...

    /// <summary>
    /// The MetaDataItems class contains information about script dependencies (referenced local
    /// assemblies and imported scripts) and compiler options. This information is required when
    /// scripts are executed in a 'cached' mode (/c switch). On the base of this information the script
    /// engine will compile new version of .compiled assembly if any of it's dependencies is changed. This
    /// is required even for referenced local assemblies as it is possible that they are a strongly
    /// named assemblies (recompiling is required for any compiled client of the strongly named assembly
    /// in case this assembly is changed).
    ///
    /// The perfect place to store the dependencies info (custom meta data) is the assembly
    /// resources. However if we do so such assemblies would have to be loaded in order to read their
    /// resources. It is not acceptable as after loading assembly cannot be unloaded. Also assembly loading
    /// can significantly compromise performance.
    ///
    /// That is why custom meta data is just physically appended to the file. This is a valid
    /// approach because such assembly is not to be distributed anywhere but to stay always
    /// on the PC and play the role of the temporary data for the script engine.
    ///
    /// Note: A .dll assembly is always compiled and linked in a normal way without any custom meta data attached.
    /// </summary>
    internal class MetaDataItems
    {
        public class MetaDataItem
        {
            public MetaDataItem(string file, DateTime date, bool assembly)
            {
                this.file = file;
                this.date = date;
                this.assembly = assembly;
            }

            public string file;
            public DateTime date;
            public bool assembly;
        }

#if net1
        public ArrayList items = new ArrayList();
#else
        public List<MetaDataItem> items = new List<MetaDataItem>();
#endif

        static public bool IsOutOfDate(string script, string assembly)
        {
            MetaDataItems depInfo = new MetaDataItems();

            if (depInfo.ReadFileStamp(assembly))
            {
                //Trace.WriteLine("Reading mete data...");
                //foreach (MetaDataItems.MetaDataItem item in depInfo.items)
                //    Trace.WriteLine(item.file + " : " + item.date);

                string dependencyFile = "";
                foreach (MetaDataItem item in depInfo.items)
                {
                    if (item.assembly)
                    {
                        if (Path.IsPathRooted(item.file)) //is absolute path
                        {
                            dependencyFile = item.file;
                        }
                        else
                        {
                            foreach (string dir in CSExecutor.options.searchDirs)
                            {
                                dependencyFile = Path.Combine(dir, item.file); //assembly should be in the same directory with the script
                                if (File.Exists(dependencyFile))
                                    break;
                            }
                        }
                    }
                    else
                        dependencyFile = FileParser.ResolveFile(item.file, CSExecutor.options.searchDirs, false);

                    if (!File.Exists(dependencyFile) || File.GetLastWriteTimeUtc(dependencyFile) != item.date)
                    {
                        return true;
                    }
                }
                return false;
            }
            else
                return true;
        }

        public string[] AddItems(System.Collections.Specialized.StringCollection files, bool isAssembly, string[] searchDirs)
        {
            string[] referencedAssemblies = new string[files.Count];
            files.CopyTo(referencedAssemblies, 0);
            return AddItems(referencedAssemblies, isAssembly, searchDirs);
        }

        public string[] AddItems(string[] files, bool isAssembly, string[] searchDirs)
        {
#if net1
            ArrayList newProbingDirs = new ArrayList();
#else
            List<string> newProbingDirs = new List<string>();
#endif
            if (isAssembly)
            {
                foreach (string asmFile in files)
                {
                    //under some conditions assemblies do not have a location (e.g. dynamically built/emitted assemblies under ASP.NET)
                    if (!File.Exists(asmFile))
                        continue;

                    try
                    {
                        if (!IsGACAssembly(asmFile))
                        {
                            bool found = false;
                            foreach (string dir in searchDirs)
                                if (!IsGACAssembly(asmFile) && string.Compare(dir, Path.GetDirectoryName(asmFile), true) == 0)
                                {
                                    found = true;
                                    AddItem(Path.GetFileName(asmFile), File.GetLastWriteTimeUtc(asmFile), true);
                                    break;
                                }

                            if (!found) //the assembly was not in the search dirs
                            {
                                newProbingDirs.Add(Path.GetDirectoryName(asmFile));
                                AddItem(asmFile, File.GetLastWriteTimeUtc(asmFile), true); //assembly from the absolute path
                            }
                        }
                    }
                    catch (NotSupportedException)
                    {
                        //under ASP.NET some assemblies do not have location (e.g. dynamically built/emitted assemblies)
                    }
                    catch (ArgumentException)
                    {
                        //The asm.location parameter contains invalid characters, is empty, or contains only white spaces, or contains a wildcard character
                    }
                    catch (PathTooLongException)
                    {
                        //The asm.location parameter is longer than the system-defined maximum length
                    }
                    catch
                    {
                        //In fact ignore all exceptions at we should continue if for what ever reason assembly location cannot be obtained
                    }
                }
            }
            else
            {
                foreach (string file in files)
                {
                    string fullPath = Path.GetFullPath(file);

                    bool local = false;
                    foreach (string dir in searchDirs)
                        if ((local = (string.Compare(dir, Path.GetDirectoryName(fullPath), true) == 0)))
                            break;

                    if (local)
                        AddItem(Path.GetFileName(file), File.GetLastWriteTimeUtc(file), false);
                    else
                        AddItem(file, File.GetLastWriteTimeUtc(file), false);
                }
            }
#if net1
            return (string[])newProbingDirs.ToArray(typeof(string));
#else
            return newProbingDirs.ToArray();
#endif
        }

        public void AddItem(string file, DateTime date, bool assembly)
        {
            this.items.Add(new MetaDataItem(file, date, assembly));
        }

        public bool StampFile(string file)
        {
            //Trace.WriteLine("Writing mete data...");
            //foreach (MetaDataItem item in items)
            //    Trace.WriteLine(item.file + " : " + item.date);

            try
            {
                using (FileStream fs = new FileStream(file, FileMode.Open))
                {
                    fs.Seek(0, SeekOrigin.End);
                    using (BinaryWriter w = new BinaryWriter(fs))
                    {
                        char[] data = this.ToString().ToCharArray();
                        w.Write(data);
                        w.Write((Int32)data.Length);
                        w.Write((Int32)(CSExecutor.options.DBG ? 1 : 0));
                        w.Write((Int32)(CSExecutor.options.compilationContext));
                        w.Write((Int32)Environment.Version.ToString().GetHashCode());
                        w.Write((Int32)stampID);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
            return true;
        }

        public bool ReadFileStamp(string file)
        {
            try
            {
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader r = new BinaryReader(fs))
                    {
                        fs.Seek(-intSize, SeekOrigin.End);
                        int stamp = r.ReadInt32();
                        if (stamp == stampID)
                        {
                            fs.Seek(-(intSize * 2), SeekOrigin.End);
                            if (r.ReadInt32() != Environment.Version.ToString().GetHashCode())
                            {
                                //Console.WriteLine("Environment.Version");
                                return false;
                            }

                            fs.Seek(-(intSize * 3), SeekOrigin.End);

                            //int yyy = r.ReadInt32();
                            //if (yyy != CSExecutor.options.compilationContext)
                            if (r.ReadInt32() != CSExecutor.options.compilationContext)
                            {
                                //Console.WriteLine("CSExecutor.options.compilationContext");
                                return false;
                            }

                            fs.Seek(-(intSize * 4), SeekOrigin.End);
                            if (r.ReadInt32() != (CSExecutor.options.DBG ? 1 : 0))
                            {
                                //Console.WriteLine("CSExecutor.options.DBG");
                                return false;
                            }

                            fs.Seek(-(intSize * 5), SeekOrigin.End);
                            int dataSize = r.ReadInt32();

                            if (dataSize != 0)
                            {
                                fs.Seek(-(intSize * 5 + dataSize), SeekOrigin.End);
                                return this.Parse(new string(r.ReadChars(dataSize)));
                            }
                            else
                                return true;
                        }
                        return false;
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        private new string ToString()
        {
            StringBuilder bs = new StringBuilder();
            foreach (MetaDataItem fileInfo in items)
            {
                bs.Append(fileInfo.file);
                bs.Append(";");
                bs.Append(fileInfo.date.ToFileTimeUtc().ToString());
                bs.Append(";");
                bs.Append(fileInfo.assembly ? "Y" : "N");
                bs.Append("|");
            }
            return bs.ToString();
        }

        private bool Parse(string data)
        {
            foreach (string itemData in data.Split("|".ToCharArray()))
            {
                if (itemData.Length > 0)
                {
                    string[] parts = itemData.Split(";".ToCharArray());
                    if (parts.Length == 3)
                        this.items.Add(new MetaDataItem(parts[0], DateTime.FromFileTimeUtc(long.Parse(parts[1])), parts[2] == "Y"));
                    else
                        return false;
                }
            }
            return true;
        }

        private int stampID = Assembly.GetExecutingAssembly().FullName.Split(",".ToCharArray())[1].GetHashCode();
        private int intSize = Marshal.SizeOf((Int32)0);

        //#pragma warning disable 414
        private int executionFlag = Marshal.SizeOf((Int32)0);
        //#pragma warning restore 414

        private bool IsGACAssembly(string file)
        {
            string s = file.ToLower();
#if net1
            return s.IndexOf("microsoft.net\\framework") != -1 || s.IndexOf("microsoft.net/framework") != -1 || s.IndexOf("gac_msil") != -1 || s.IndexOf("gac_64") != -1 || s.IndexOf("gac_32") != -1;
#else
            return s.Contains("microsoft.net\\framework") || s.Contains("microsoft.net/framework") || s.Contains("gac_msil") || s.Contains("gac_64") || s.Contains("gac_32");
#endif
        }
    }

    #endregion MetaDataItems...

    internal class HelpProvider
    {
        public static string BuildCommandInterfaceHelp()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(AppInfo.appLogo);
            builder.Append("\nUsage: " + AppInfo.appName + " <switch 1> <switch 2> <file> [params] [//x]\n");
            builder.Append("\n");
            builder.Append("<switch 1>\n");
            builder.Append(" {0}?    - Display help info.\n");
            builder.Append(" {0}e    - Compile script into console application executable.\n");
            builder.Append(" {0}ew   - Compile script into Windows application executable.\n");
            builder.Append(" {0}c    - Use compiled file (cache file .compiled) if found (to improve performance).\n");
            builder.Append(" {0}ca   - Compile script file into assembly (cache file .compiled) without execution.\n");
            builder.Append(" {0}cd   - Compile script file into assembly (.dll) without execution.\n\n");
            builder.Append(" {0}co:<options>\n");
            builder.Append("       - Pass compiler options directly to the language compiler\n");
            builder.Append("       (e.g.  {0}co:/d:TRACE pass /d:TRACE option to C# compiler\n");
            builder.Append("        or  {0}co:/platform:x86 to produce Win32 executable)\n\n");
            builder.Append(" {0}s    - Print content of sample script file (e.g. " + AppInfo.appName + " /s > sample.cs).\n");
            builder.Append(" {0}ac | {0}autoclass\n");
            builder.Append("       - Automatically generates wrapper class if the script does not define any class of its own:\n");
            builder.Append("\n");
            builder.Append("         using System;\n");
            builder.Append("                      \n");
            builder.Append("         void Main()\n");
            builder.Append("         {\n");
            builder.Append("             Console.WriteLine(\"Hello World!\");\n");
            builder.Append("         }\n");
            builder.Append("\n");
            builder.Append("\n");
            builder.Append("<switch 2>\n");
            if (AppInfo.appParamsHelp != "")
                builder.Append(" {0}" + AppInfo.appParamsHelp);	//application specific usage info
            builder.Append(" {0}dbg | " + CSSUtils.cmdFlagPrefix + "d\n");
            builder.Append("         - Force compiler to include debug information.\n");
            builder.Append(" {0}l    - 'local'(makes the script directory a 'current directory')\n");
            builder.Append(" {0}v    - Prints CS-Script version information\n");
            builder.Append(" {0}verbose \n");
            builder.Append("       - prints runtime information during the script execution (applicable for console clients only)\n");
            builder.Append(" {0}noconfig[:<file>]\n       - Do not use default CS-Script config file or use alternative one.\n");
            builder.Append("         (e.g. " + AppInfo.appName + " {0}noconfig sample.cs\n");
            builder.Append("         " + AppInfo.appName + " {0}noconfig:c:\\cs-script\\css_VB.dat sample.vb)\n");
            builder.Append(" {0}sconfig[:<file>]\n       - Use script config file or custom config file as a .NET application configuration file.\n");
            builder.Append("  This option might be useful for running scripts, which usually cannot be executed without configuration file (e.g. WCF, Remoting).\n\n");
            builder.Append("          (e.g. if {0}sconfig is used the expected config file name is <script_name>.cs.config or <script_name>.exe.config\n");
            builder.Append("           if {0}sconfig:myApp.config is used the expected config file name is myApp.config)\n");
            builder.Append(" {0}r:<assembly 1>:<assembly N>\n");
            builder.Append("       - Use explicitly referenced assembly. It is required only for\n");
            builder.Append("         rare cases when namespace cannot be resolved into assembly.\n");
            builder.Append("         (e.g. " + AppInfo.appName + " /r:myLib.dll myScript.cs).\n");
            builder.Append(" {0}dir:<directory 1>,<directory N>\n");
            builder.Append("       - Add path(s) to the assembly probing directory list.\n");
            builder.Append("         (e.g. " + AppInfo.appName + " /dir:C:\\MyLibraries myScript.cs).\n");
            builder.Append(" {0}co:<options>\n");
            builder.Append("       -  Passes compiler options directy to the language compiler.\n");
            builder.Append("         (e.g. /co:/d:TRACE pass /d:TRACE option to C# compiler).\n");
            builder.Append(" {0}precompiler[:<file 1>,<file N>]\n");
            builder.Append("       - specifies custom precompiler file(s). This can be either script or assembly file.\n");
            builder.Append("         If no file(s) specified prints the code template for the custom precompiler.\n");
            builder.Append("         There is a special reserved word '" + CSSUtils.noDefaultPrecompilerSwitch + "' to be used as a file name.\n");
            builder.Append("         It instructs script engine to prevent loading any built-in precompilers \n");
            builder.Append("         like the one for removing shebang\n");
            builder.Append("         before the execution.\n");
            builder.Append("         (see Precompilers chapter in the documentation)\n");
            builder.Append("\n");
            builder.Append("file   - Specifies name of a script file to be run.\n");
            builder.Append("params - Specifies optional parameters for a script file to be run.\n");
            builder.Append(" //x   - Launch debugger just before starting the script.\n");
            builder.Append("\n");
            if (AppInfo.appConsole) // a temporary hack to prevent showing a huge message box when not in console mode
            {
                builder.Append("\n");
                builder.Append("**************************************\n");
                builder.Append("Script specific syntax\n");
                builder.Append("**************************************\n");
                builder.Append("\n");
                builder.Append("Engine directives:\n");
                builder.Append("------------------------------------\n");
                builder.Append("//css_import <file>[, preserve_main][, rename_namespace(<oldName>, <newName>)];\n");
                builder.Append("\n");
                builder.Append("Alias - //css_imp\n");
                builder.Append("There are also another two aliases //css_include and //css_inc. They are equivalents of //css_import <file>, preserve_main\n");
                builder.Append("If $this (or $this.name) is specified as part of <file> it will be replaced at execution time with the main script full name (or file name only).\n");
                builder.Append("\n");
                builder.Append("file            - name of a script file to be imported at compile-time.\n");
                builder.Append("<preserve_main> - do not rename 'static Main'\n");
                builder.Append("oldName         - name of a namespace to be renamed during importing\n");
                builder.Append("newName         - new name of a namespace to be renamed during importing\n");
                builder.Append("\n");
                builder.Append("This directive is used to inject one script into another at compile time. Thus code from one script can be exercised in another one.\n");
                builder.Append("'Rename' clause can appear in the directive multiple times.\n");
                builder.Append("------------------------------------\n");
                builder.Append("//css_args arg0[,arg1]..[,argN];\n");
                builder.Append("\n");
                builder.Append("Embedded script arguments. The both script and engine arguments are allowed except \"/noconfig\" engine command switch.\n");
                builder.Append(" Example: //css_args /dbg;\n This directive will always force script engine to execute the script in debug mode.\n");
                builder.Append("------------------------------------\n");
                builder.Append("//css_reference <file>;\n");
                builder.Append("\n");
                builder.Append("Alias - //css_ref\n");
                builder.Append("\n");
                builder.Append("file	- name of the assembly file to be loaded at run-time.\n");
                builder.Append("\n");
                builder.Append("This directive is used to reference assemblies required at run time.\n");
                builder.Append("The assembly must be in GAC, the same folder with the script file or in the 'Script Library' folders (see 'CS-Script settings').\n");
                builder.Append("------------------------------------\n");
                builder.Append("//css_precompiler <file 1>,<file 2>;\n");
                builder.Append("\n");
                builder.Append("Alias - //css_pc\n");
                builder.Append("\n");
                builder.Append("file	- name of the script or assembly file implementing precompiler.\n");
                builder.Append("\n");
                builder.Append("This directive is used to specify the CS-Script precompilers to be loaded and exercised against script at run time.\n");
                builder.Append("------------------------------------\n");
                builder.Append("//css_searchdir <directory>;\n");
                builder.Append("\n");
                builder.Append("Alias - //css_dir\n");
                builder.Append("\n");
                builder.Append("directory - name of the directory to be used for script and assembly probing at run-time.\n");
                builder.Append("\n");
                builder.Append("This directive is used to extend set of search directories (script and assembly probing).\n");
                builder.Append("------------------------------------\n");
                builder.Append("//css_resource <file>;\n");
                builder.Append("\n");
                builder.Append("Alias - //css_res\n");
                builder.Append("\n");
                builder.Append("file	- name of the resource file (.resources) to be used with the script.\n");
                builder.Append("\n");
                builder.Append("This directive is used to reference resource file for script.\n");
                builder.Append(" Example: //css_res Scripting.Form1.resources;\n");
                builder.Append("------------------------------------\n");
                builder.Append("//css_co <options>;\n");
                builder.Append("\n");
                builder.Append("options - options string.\n");
                builder.Append("\n");
                builder.Append("This directive is used to pass compiler options string directly to the language specific CLR compiler.\n");
                builder.Append(" Example: //css_co /d:TRACE pass /d:TRACE option to C# compiler\n");
                builder.Append("          //css_co /platform:x86 to produce Win32 executable\n\n");
                builder.Append("------------------------------------\n");
                builder.Append("//css_ignore_namespace <namespace>;\n");
                builder.Append("\n");
                builder.Append("Alias - //css_ignore_ns\n");
                builder.Append("\n");
                builder.Append("namespace	- name of the namespace.\n");
                builder.Append("\n");
                builder.Append("This directive is used to prevent CS-Script from resolving the referenced namespace into assembly.\n");
                builder.Append("------------------------------------\n");
                builder.Append("//css_prescript file([arg0][,arg1]..[,argN])[ignore];\n");
                builder.Append("//css_postscript file([arg0][,arg1]..[,argN])[ignore];\n");
                builder.Append("\n");
                builder.Append("Aliases - //css_pre and //css_post\n");
                builder.Append("\n");
                builder.Append("file    - script file (extension is optional)\n");
                builder.Append("arg0..N - script string arguments\n");
                builder.Append("ignore  - continue execution of the main script in case of error\n");
                builder.Append("\n");
                builder.Append("These directives are used to execute secondary pre- and post-action scripts.\n");
                builder.Append("If $this (or $this.name) is specified as arg0..N it will be replaced at execution time with the main script full name (or file name only).\n");
                builder.Append("------------------------------------\n");
                builder.Append("//css_host [/version:<CLR_Version>] [/platform:<CPU>]\n");
                builder.Append("\n");
                builder.Append("CLR_Version - version of CLR the script should be execute on (e.g. //css_host /version:v3.5)\n");
                builder.Append("CPU - indicates which platforms the script should be run on: x86, Itanium, x64, or anycpu.\n");
                builder.Append("Sample: //css_host /version:v2.0 /platform:x86;");
                builder.Append("\n");
                builder.Append("These directive is used to execute script from a surrogate host process. The script engine application (cscs.exe or csws.exe) launches the script execution as a separate process of the s are used to execute secondary pre- and post-action scripts.\n");
                builder.Append("execution as a separate process of the specified CLR version and CPU architecture.\n");
                builder.Append("------------------------------------\n");
                builder.Append("\n");
                builder.Append("Any directive has to be written as a single line in order to have no impact on compiling by CLI compliant compiler.\n");
                builder.Append("It also must be placed before any namespace or class declaration.\n");
                builder.Append("\n");
                builder.Append("------------------------------------\n");
                builder.Append("Example:\n");
                builder.Append("\n");
                builder.Append(" using System;\n");
                builder.Append(" //css_prescript com(WScript.Shell, swshell.dll);\n");
                builder.Append(" //css_import tick, rename_namespace(CSScript, TickScript);\n");
                builder.Append(" //css_reference teechart.lite.dll;\n");
                builder.Append(" \n");
                builder.Append(" namespace CSScript\n");
                builder.Append(" {\n");
                builder.Append("   class TickImporter\n");
                builder.Append("   {\n");
                builder.Append("      static public void Main(string[] args)\n");
                builder.Append("      {\n");
                builder.Append("         TickScript.Ticker.i_Main(args);\n");
                builder.Append("      }\n");
                builder.Append("   }\n");
                builder.Append(" }\n");
                builder.Append("\n");
            }

            //return string.Format(builder.ToString(), CSSUtils.cmdFlagPrefix); //for some reason Format(..) fails
            return builder.ToString().Replace("{0}", CSSUtils.cmdFlagPrefix);
        }

        public static string BuildSampleCode()
        {
            StringBuilder builder = new StringBuilder();
            if (Utils.IsLinux())
            {
                builder.Append("#!<cscs.exe path> " + CSSUtils.cmdFlagPrefix + "nl " + Environment.NewLine);
                builder.Append("//css_reference System.Windows.Forms;" + Environment.NewLine);
            }

            builder.Append("using System;" + Environment.NewLine);
            builder.Append("using System.Windows.Forms;" + Environment.NewLine);
            builder.Append(Environment.NewLine);
            builder.Append("class Script" + Environment.NewLine);
            builder.Append("{" + Environment.NewLine);
            if (!Utils.IsLinux())
                builder.Append("    [STAThread]" + Environment.NewLine);
            builder.Append("    static public void Main(string[] args)" + Environment.NewLine);
            builder.Append("    {" + Environment.NewLine);
            builder.Append("        for (int i = 0; i < args.Length; i++)" + Environment.NewLine);
            builder.Append("            Console.WriteLine(args[i]);" + Environment.NewLine);
            builder.Append(Environment.NewLine);
            builder.Append("        MessageBox.Show(\"Just a test!\");" + Environment.NewLine);
            builder.Append(Environment.NewLine);
            builder.Append("    }" + Environment.NewLine);
            builder.Append("}" + Environment.NewLine);

            return builder.ToString();
        }

        public static string BuildPrecompilerSampleCode()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("using System;" + Environment.NewLine);
            builder.Append("using System.Collections;" + Environment.NewLine);
            builder.Append("using System.Collections.Generic;" + Environment.NewLine);
            builder.Append(Environment.NewLine);
            builder.Append("public class Sample_Precompiler //precompiler class name must end with 'Precompiler'" + Environment.NewLine);
            builder.Append("{" + Environment.NewLine);
            builder.Append("    public static bool Compile(ref string scriptCode, string scriptFile, bool isPrimaryScript, Hashtable context)" + Environment.NewLine);
            builder.Append("    {" + Environment.NewLine);
            builder.Append("        //if new assemblies are to be referenced add them (see 'Precompilers' in the documentation)" + Environment.NewLine);
            builder.Append("        //var newReferences = (List<string>)context[\"NewReferences\"];" + Environment.NewLine);
            builder.Append("        //newReferences.Add(\"System.Xml.dll\");" + Environment.NewLine);
            builder.Append(Environment.NewLine);
            builder.Append("        //if scriptCode needs to be altered assign scriptCode the new value and return true. Otherwise return false" + Environment.NewLine);
            builder.Append(Environment.NewLine);
            builder.Append("        //scriptCode = \"code after precompilation\";" + Environment.NewLine);
            builder.Append("        //return true;" + Environment.NewLine);
            builder.Append(Environment.NewLine);
            builder.Append("        return false;" + Environment.NewLine);
            builder.Append("    }" + Environment.NewLine);
            builder.Append("}" + Environment.NewLine);

            return builder.ToString();
        }

        public static string BuildVersionInfo()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(AppInfo.appLogo.TrimEnd() + " www.csscript.net\n");
            builder.Append("\n");
            builder.Append("   CLR:            " + Environment.Version + "\n");
            builder.Append("   System:         " + Environment.OSVersion + "\n");
#if net4
            builder.Append("   Architecture:   " + (Environment.Is64BitProcess ? "x64" : "x86") + "\n");
#endif
            builder.Append("   Home dir:       " + (Environment.GetEnvironmentVariable("CSSCRIPT_DIR") ?? "<not integrated>") + "\n");
            return builder.ToString();
        }
    }
}