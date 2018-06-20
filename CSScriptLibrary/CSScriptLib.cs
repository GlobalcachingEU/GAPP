#region Licence...

//-----------------------------------------------------------------------------
// Date:	10/11/04	Time: 3:00p
// Module:	CSScriptLib.cs
// Classes:	CSScript
//			AppInfo
//
// This module contains the definition of the CSScript class. Which implements
// compiling C# script engine (CSExecutor). Can be used for hosting C# script engine
// from any CLR application
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

//using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;
using System.Collections;

#if !net1

using System.Collections.Generic;
using System.Linq;

#endif

using csscript;
using System.Threading;
using System.CodeDom.Compiler;
using System.Security;
using System.Security.Permissions;

namespace CSScriptLibrary
{
#if !net1
    /// <summary>
    /// Simple helper class for extending functionality of <see cref="T:System.AppDomain"/>.
    /// <para>
    /// This class mainly consist of the extension methods for <see cref="T:System.AppDomain"/> and it is to be used for executing the arbitrary
    /// code routines in the separate (temporary) <see cref="T:System.AppDomain"/> with the optional unloading.
    /// </para>This class is particularly useful for executing the CS-Script script in the separate <see cref="T:System.AppDomain"/> as this is the
    /// only way to unload the script assembly after the execution (known .NET limitation).
    /// <para>
    /// <example>The following are the examples of the execution CS-Script scripts and unloading them after the execution:
    ///<code>
    /// AppDomain.CurrentDomain
    ///          .Clone()
    ///          .Execute(Job)
    ///          .Unload();
    /// ...
    ///
    /// void Job()
    /// {
    ///     var script = CSScript.LoadMethod("some C# script code")
    ///                          .GetStaticMethod();
    ///     script();
    /// };
    /// </code>
    /// <code>
    /// AppDomain remote = AppDomain.CurrentDomain.Clone();
    ///
    /// remote.Execute(() =>
    ///     {
    ///         var Sum = CSScript.BuildEval(@"func(float a, float b) {
    ///                                                 return a + b;
    ///                                        }");
    ///
    ///         var Average = CSScript.BuildEval(@"func(float a, float b) {
    ///                                                 return (a + b)/2;
    ///                                            }");
    ///
    ///         Console.WriteLine("Sum = {0}\nAverage={1}", Sum(1f, 2f), Average(1f, 2f));
    ///     });
    ///
    /// remote.Unload();
    /// </code>
    /// </example>
    /// </para>
    /// <remarks>
    /// The functionality of this class is very similar to the <see cref="T:CSScriptLibrary.AsmHelper"/>, which also allows executing and unloading the script(s).
    /// However  <see cref="T:CSScriptLibrary.AppDomainHelper"/> is designed as a generic class and as such it is more suitable for executing a "job" routines instead of individual scripts.
    /// <para>
    /// This creates some attractive opportunities for grouping scripting routines in a single <see cref="T:CSScriptLibrary.AsmHelper"/>, which allows simple calling conventions (e.g. <c>CSScript.Load()</c>
    /// instead of <c>CSScript.Compile()</c>) lighter type system (e.g. no need for MarshalByRefObject inheritance).
    /// </para>
    /// </remarks>
    /// </summary>
    public static class AppDomainHelper
    {
        private class RemoteExecutor : MarshalByRefObject
        {
            public void Execute(Action action)
            {
                action();
            }
        }

        /// <summary>
        /// Executes the <see cref="T:System.Action"/> delegate in the specified <see cref="T:System.AppDomain"/>.
        /// <example>The following are the examples of the execution CS-Script scripts and unloading them after the execution:
        ///<code>
        /// var remoteDomain = AppDomain.CurrentDomain.Clone();
        /// remoteDomain.Execute(Job)
        /// remoteDomain.Unload();
        /// ...
        ///
        /// void Job()
        /// {
        ///     var script = CSScript.LoadMethod("some C# script code")
        ///                          .GetStaticMethod();
        ///     script();
        /// };
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="domain">The <see cref="T:System.AppDomain"/> the delegate should be executed in.</param>
        /// <param name="action">The delegate.</param>
        /// <returns>Reference to the <see cref="T:System.AppDomain"/>. It is the same object, which is passed as the <paramref name="domain"/>.</returns>
        public static AppDomain Execute(this AppDomain domain, Action action)
        {
            var remote = (RemoteExecutor)domain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().Location, typeof(RemoteExecutor).ToString());
            remote.Execute(action);
            return domain;
        }

        /// <summary>
        /// <para>Executes the delegate in the temporary <see cref="T:System.AppDomain"/> with the following unloading of this domain.
        /// </para>
        /// <example>The following code the complete equivalent implementation of the <c>ExecuteAndUnload</c>:
        ///<code>
        /// AppDomain.CurrentDomain
        ///          .Clone()
        ///          .Execute(action)
        ///          .Unload();
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="action">The delegate to be executed.</param>
        public static void ExecuteAndUnload(Action action)
        {
            AppDomain.CurrentDomain
                     .Clone()
                     .Execute(action)
                     .Unload();
        }

        /// <summary>
        /// Unloads the specified <see cref="T:System.AppDomain"/>.
        /// </summary>
        /// <param name="domain">The <see cref="T:System.AppDomain"/> to be unloaded.</param>
        public static void Unload(this AppDomain domain)
        {
            AppDomain.Unload(domain);
        }

        /// <summary>
        /// Clones the specified <see cref="T:System.AppDomain"/>. The mandatory "creation" properties of the <paramref name="domain"/> are used to create the new instance of <see cref="T:System.AppDomain"/>.
        /// <para>The "friendly name" of the cloned <see cref="T:System.AppDomain"/> is a string representation of the random <c>GUID</c>.</para>
        /// </summary>
        /// <param name="domain">The <see cref="T:System.AppDomain"/> to be cloned.</param>
        /// <returns>The newly created <see cref="T:System.AppDomain"/>.</returns>
        public static AppDomain Clone(this AppDomain domain)
        {
            return domain.Clone(Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Clones the specified <see cref="T:System.AppDomain"/>. The mandatory "creation" properties of the <paramref name="domain"/> are used to create the new instance of <see cref="T:System.AppDomain"/>.
        /// <para>The <paramref name="name"/> parameter is used as the "friendly name" for the cloned <see cref="T:System.AppDomain"/>.</para>
        /// </summary>
        /// <param name="domain">The <see cref="T:System.AppDomain"/> to be cloned.</param>
        /// <param name="name">The "friendly name" of the new <see cref="T:System.AppDomain"/> to be created.</param>
        /// <returns>The newly created <see cref="T:System.AppDomain"/>.</returns>
        public static AppDomain Clone(this AppDomain domain, string name)
        {
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            setup.PrivateBinPath = AppDomain.CurrentDomain.BaseDirectory;
            setup.ShadowCopyFiles = "true";
            setup.ShadowCopyDirectories = setup.ApplicationBase;

            return AppDomain.CreateDomain(name, null, setup);
        }
    }
#endif

    /// <summary>
    /// Simple security helper class. This class is nothing else but a syntactic sugar.
    /// <para>
    /// <example>The following is an example of execution under .NET sandbox:
    /// <code>
    /// Sandbox.With(SecurityPermissionFlag.Execution)
    ///        .Execute(() =>
    ///                 {
    ///                     //call sandboxed actions
    ///                 });
    /// </code>
    /// </example>
    /// </para>
    /// </summary>
    public static class Sandbox
    {
        /// <summary>
        /// Generic void/void delegate
        /// </summary>
        public delegate void Action();
#if net1
#endif
        /// <summary>
        /// Extension method. Executes <see cref="T:System.Action"/> with the specified array of permissions
        /// </summary>
        /// <param name="permissions">The permissions set to be used for the execution.</param>
        /// <param name="action">The action to be executed.</param>
#if net1
        public static void Execute(PermissionSet permissions, Action action)
#else

        public static void Execute(this PermissionSet permissions, Action action)
#endif
        {
            permissions.PermitOnly();

            try
            {
                action();
            }
            finally
            {
                CodeAccessPermission.RevertPermitOnly();
            }
        }

        /// <summary>
        /// Returns the specified permissions as <see cref="T:System.Security.PermissionSet"/> to be used with <see cref="T:CSScriptLibrary.Sandbox.Execute"/>.
        /// </summary>
        /// <param name="permissions">The permissions.</param>
        /// <returns><see cref="T:System.Security.PermissionSet"/> instance.</returns>
        public static PermissionSet With(params IPermission[] permissions)
        {
            PermissionSet permissionSet = new PermissionSet(PermissionState.None);

            foreach (IPermission permission in permissions)
                permissionSet.AddPermission(permission);

            return permissionSet;
        }

        /// <summary>
        /// Returns the specified permissions as <see cref="T:System.Security.PermissionSet"/> to be used with <see cref="T:csscript.Sandbox.Execute"/>.
        /// </summary>
        /// <param name="permissionsFlag">The permissions flag. Can be combination of multiple values.</param>
        /// <returns><see cref="T:System.Security.PermissionSet"/> instance.</returns>
        public static PermissionSet With(SecurityPermissionFlag permissionsFlag)
        {
            PermissionSet permissionSet = new PermissionSet(PermissionState.None);
            permissionSet.AddPermission(new SecurityPermission(permissionsFlag));

            return permissionSet;
        }
    }

    /// <summary>
    /// Delegate to handle output from script
    /// </summary>
    public delegate void PrintDelegate(string msg);

    /// <summary>
    /// Class which is implements CS-Script class library interface.
    /// </summary>
    public class CSScript
    {
        static string dummy = "";

        /// <summary>
        /// Default constructor
        /// </summary>
        public CSScript()
        {
            rethrow = false;
        }

        /// <summary>
        /// Determines whether the specified assembly is a script assembly (compiled script) and returns full path of the script file
        /// used to compile the assembly. The analysis is based on the fact that script assembly (in hosing scenarios) is always
        /// stamped with <see cref="T:System.Reflection.AssemblyDescriptionAttribute"/>, which contains name of the script file the
        /// assembly was compiled from.
        /// <para>The format of the description </para>
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>
        /// 	Script file path if the specified assembly is a script assembly otherwise <c>null</c>.
        /// </returns>
        static public string GetScriptName(Assembly assembly)
        {
            //Note assembly can contain only single AssemblyDescriptionAttribute
            foreach (AssemblyDescriptionAttribute attribute in assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), true))
                return attribute.Description;
            return null;
        }

        /// <summary>
        /// Force caught exceptions to be re-thrown.
        /// </summary>
        static public bool Rethrow
        {
            get { return rethrow; }
            set { rethrow = value; }
        }

        /// <summary>
        /// Enables automatic resolving of unsuccessful assembly probing on the base of the Settings.SearchDirs.
        /// Default value is true.
        ///
        /// CLR does assembly probing only in GAC and in the local (with respect to the application) directories. CS-Script
        /// however allows you to specify extra directory(es) for assembly probing by setting enabling CS-Script assembly resolving
        /// through setting the AssemblyResolvingEnabled to true and changing the Settings.SearchDirs appropriately.
        /// </summary>
        static public bool AssemblyResolvingEnabled
        {
            get { return assemblyResolvingEnabled; }
            set
            {
                if (value)
                {
                    AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(OnAssemblyResolve);
                    callingResolveEnabledAssembly = Assembly.GetCallingAssembly();
                }
                else
                    AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(OnAssemblyResolve);

                assemblyResolvingEnabled = value;
            }
        }

        static bool assemblyResolvingEnabled = true;

        /// <summary>
        /// Gets or sets the assembly sharing mode. If set to true all assemblies (including the host assembly itself)
        /// currently loaded to the host application AppDomain are automatically available/accessible from the script code.
        /// Default value is true.
        ///
        /// Sharing the same assembly set between the host application and the script require AssemblyResolvingEnabled to
        /// be enabled. Whenever SharesHostRefAssemblies is changed to true it automatically sets AssemblyResolvingEnabled to
        /// true as well.
        /// </summary>
        static public bool ShareHostRefAssemblies
        {
            get { return shareHostRefAssemblies; }
            set
            {
                if (shareHostRefAssemblies != value)
                {
                    shareHostRefAssemblies = value;
                    if (shareHostRefAssemblies)
                        AssemblyResolvingEnabled = true;
                }
            }
        }

        static private bool shareHostRefAssemblies = true;
        static Assembly callingResolveEnabledAssembly;

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly retval = null;
            if (args.Name == "GetExecutingAssembly()")
                retval = callingResolveEnabledAssembly;//Assembly.GetExecutingAssembly();
            else if (args.Name == "GetEntryAssembly()")
                retval = Assembly.GetEntryAssembly();
            else
            {
                ExecuteOptions options = InitExecuteOptions(new ExecuteOptions(), CSScript.GlobalSettings, null, ref dummy);

                foreach (string dir in options.searchDirs)
                {
                    if ((retval = AssemblyResolver.ResolveAssembly(args.Name, dir)) != null)
                        break;
                }
            }
            return retval;
        }

        /// <summary>
        /// Settings object containing runtime settings, which controls script compilation/execution.
        /// This is Settings class essentially is a deserialized content of the CS-Script configuration file (css_config.xml).
        /// </summary>
        public static Settings GlobalSettings = Settings.Load(Environment.ExpandEnvironmentVariables(@"%CSSCRIPT_DIR%\css_config.xml"));

#if !net1
        /// <summary>
        /// Collection of all compiling results. Every time the script is compiled the compiling result is added to this collection regardless of
        /// the success or failure of the actual compilation.
        /// </summary>
        public static Dictionary<FileInfo, CompilerResults> CompilingHistory = new Dictionary<FileInfo, CompilerResults>();

        private static bool keepCompilingHistory = false;

        /// <summary>
        /// Gets or sets a value indicating whether compiling history should be kept. The compilation results are stored in <see cref="CompilingHistory"></see>.
        /// </summary>
        /// <value>
        /// <c>true</c> if compiling history should be kept; otherwise, <c>false</c>.
        /// </value>
        public static bool KeepCompilingHistory
        {
            get { return keepCompilingHistory; }
            set { keepCompilingHistory = value; }
        }

#endif

        /// <summary>
        /// Invokes global (static) CSExecutor (C# script engine)
        /// </summary>
        /// <param name="print">Print delegate to be used (if not null) to handle script engine output (eg. compilation errors).</param>
        /// <param name="args">Script arguments.</param>
        static public void Execute(CSScriptLibrary.PrintDelegate print, string[] args)
        {
            lock (CSExecutor.options)
            {
                ExecuteOptions oldOptions = CSExecutor.options;
                try
                {
                    csscript.AppInfo.appName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
                    csscript.CSExecutor exec = new csscript.CSExecutor();
                    exec.Rethrow = Rethrow;

                    InitExecuteOptions(CSExecutor.options, CSScript.GlobalSettings, null, ref dummy);

                    exec.Execute(args, new csscript.PrintDelegate(print != null ? print : new CSScriptLibrary.PrintDelegate(DefaultPrint)), null);
                }
                finally
                {
                    CSExecutor.options = oldOptions;
                }
            }
        }

        /// <summary>
        /// Invokes CSExecutor (C# script engine)
        /// </summary>
        /// <param name="print">Print delegate to be used (if not null) to handle script engine output (eg. compilation errors).</param>
        /// <param name="args">Script arguments.</param>
        /// <param name="rethrow">Flag, which indicated if script exceptions should be rethrowed by the script engine without any handling.</param>
        public void Execute(CSScriptLibrary.PrintDelegate print, string[] args, bool rethrow)
        {
            lock (CSExecutor.options)
            {
                ExecuteOptions oldOptions = CSExecutor.options;
                try
                {
                    AppInfo.appName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
                    CSExecutor exec = new CSExecutor();
                    exec.Rethrow = rethrow;

                    InitExecuteOptions(CSExecutor.options, CSScript.GlobalSettings, null, ref dummy);

                    exec.Execute(args, new csscript.PrintDelegate(print != null ? print : new CSScriptLibrary.PrintDelegate(DefaultPrint)), null);
                }
                finally
                {
                    CSExecutor.options = oldOptions;
                }
            }
        }

        /// <summary>
        /// Compiles script code into assembly with CSExecutor
        /// </summary>
        /// <param name="scriptText">The script code to be compiled.</param>
        /// <param name="refAssemblies">The string array containing file nemes to the additional assemblies referenced by the script. </param>
        /// <returns>Compiled assembly file name.</returns>
        static public string CompileCode(string scriptText, params string[] refAssemblies)
        {
            return CompileCode(scriptText, null, false, refAssemblies);
        }

        /// <summary>
        /// Compiles script code into assembly with CSExecutor
        /// </summary>
        /// <param name="scriptText">The script code to be compiled.</param>
        /// <param name="assemblyFile">The name of compiled assembly. If set to null a temporary file name will be used.</param>
        /// <param name="debugBuild">'true' if debug information should be included in assembly; otherwise, 'false'.</param>
        /// <param name="refAssemblies">The string array containing file nemes to the additional assemblies referenced by the script. </param>
        /// <returns>Compiled assembly file name.</returns>
        static public string CompileCode(string scriptText, string assemblyFile, bool debugBuild, params string[] refAssemblies)
        {
            string tempFile = CSExecutor.GetScriptTempFile();
            try
            {
                using (StreamWriter sw = new StreamWriter(tempFile))
                {
                    sw.Write(scriptText);
                }
                return Compile(tempFile, assemblyFile, debugBuild, refAssemblies);
            }
            finally
            {
                if (!debugBuild)
                    File.Delete(tempFile);
                else
                {
                    if (tempFiles == null)
                    {
                        tempFiles = new ArrayList();
                        //Note: ApplicationExit will not be called if this library is hosted by a console application.
                        //Thus CS-Script periodical cleanup will take care of the temp files

                        //Application.ApplicationExit += new EventHandler(OnApplicationExit); //will not be available on .NET CE
                        AppDomain.CurrentDomain.DomainUnload += new EventHandler(CurrentDomain_DomainUnload);
                    }
                    tempFiles.Add(tempFile);
                }
            }
        }

        /// <summary>
        /// Returns the name of the temporary file in the CSSCRIPT subfolder of Path.GetTempPath().
        /// </summary>
        /// <returns>Temporary file name.</returns>
        static public string GetScriptTempFile()
        {
            return CSExecutor.GetScriptTempFile();
        }

        /// <summary>
        /// Returns the name of the CSScript temporary folder.
        /// </summary>
        /// <returns>Temporary folder name.</returns>
        static public string GetScriptTempDir()
        {
            return CSExecutor.GetScriptTempDir();
        }

        /// <summary>
        /// Compiles script file into assembly with CSExecutor
        /// </summary>
        /// <param name="scriptFile">The name of script file to be compiled.</param>
        /// <param name="assemblyFile">The name of compiled assembly. If set to null a temporary file name will be used.</param>
        /// <param name="debugBuild">'true' if debug information should be included in assembly; otherwise, 'false'.</param>
        /// <param name="refAssemblies">The string array containing file names to the additional assemblies referenced by the script. </param>
        /// <returns>Compiled assembly file name.</returns>
        static public string Compile(string scriptFile, string assemblyFile, bool debugBuild, params string[] refAssemblies)
        {
            return CompileWithConfig(scriptFile, assemblyFile, debugBuild, CSScript.GlobalSettings, null, refAssemblies);
        }

        /// <summary>
        /// Compiles script file into assembly (temporary file) with CSExecutor.
        /// This method is an equivalent of the CSScript.Compile(scriptFile, null, false);
        /// </summary>
        /// <param name="scriptFile">The name of script file to be compiled.</param>
        /// <param name="refAssemblies">The string array containing file names to the additional assemblies referenced by the script. </param>
        /// <returns>Compiled assembly file name.</returns>
        static public string Compile(string scriptFile, params string[] refAssemblies)
        {
            return Compile(scriptFile, null, false, refAssemblies);
        }

        /// <summary>
        /// Compiles script file into assembly with CSExecutor. Uses specified config file to load script engine settings.
        /// </summary>
        /// <param name="scriptFile">The name of script file to be compiled.</param>
        /// <param name="assemblyFile">The name of compiled assembly. If set to null a temporary file name will be used.</param>
        /// <param name="debugBuild">'true' if debug information should be included in assembly; otherwise, 'false'.</param>
        /// <param name="cssConfigFile">The name of CS-Script configuration file. If null the default config file will be used (appDir/css_config.xml).</param>
        /// <returns>Compiled assembly file name.</returns>
        static public string CompileWithConfig(string scriptFile, string assemblyFile, bool debugBuild, string cssConfigFile)
        {
            return CompileWithConfig(scriptFile, assemblyFile, debugBuild, cssConfigFile, null, null);
        }

        /// <summary>
        /// Compiles script file into assembly with CSExecutor. Uses specified config file to load script engine settings and compiler specific options.
        /// </summary>
        /// <param name="scriptFile">The name of script file to be compiled.</param>
        /// <param name="assemblyFile">The name of compiled assembly. If set to null a temporary file name will be used.</param>
        /// <param name="debugBuild">'true' if debug information should be included in assembly; otherwise, 'false'.</param>
        /// <param name="cssConfigFile">The name of CS-Script configuration file. If null the default config file will be used (appDir/css_config.xml).</param>
        /// <param name="compilerOptions">The string value to be passed directly to the language compiler. </param>
        /// <param name="refAssemblies">The string array containing file names to the additional assemblies referenced by the script. </param>
        /// <returns>Compiled assembly file name.</returns>
        static public string CompileWithConfig(string scriptFile, string assemblyFile, bool debugBuild, string cssConfigFile, string compilerOptions, params string[] refAssemblies)
        {
            Settings settings = Settings.Load(cssConfigFile != null ? cssConfigFile : Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "css_config.xml"));
            if (settings == null)
                throw new ApplicationException("The configuration file \"" + cssConfigFile + "\" cannot be loaded");

            return CompileWithConfig(scriptFile, assemblyFile, debugBuild, settings, compilerOptions, refAssemblies);
        }

        private static string GetCompilerLockName(string scriptFile, Settings scriptSettings)
        {
            if (scriptSettings.OptimisticConcurrencyModel)
            {
                return Process.GetCurrentProcess().Id.ToString(); //less aggressive lock
            }
            else
            {
                return string.Format("{0}.{1}", Process.GetCurrentProcess().Id, scriptFile.GetHashCode());
            }
        }

        /// <summary>
        /// Compiles script file into assembly with CSExecutor. Uses script engine settings object and compiler specific options.
        /// </summary>
        /// <param name="scriptFile">The name of script file to be compiled.</param>
        /// <param name="assemblyFile">The name of compiled assembly. If set to null a temporary file name will be used.</param>
        /// <param name="debugBuild">'true' if debug information should be included in assembly; otherwise, 'false'.</param>
        /// <param name="scriptSettings">The script engine Settings object.</param>
        /// <param name="compilerOptions">The string value to be passed directly to the language compiler.  </param>
        /// <param name="refAssemblies">The string array containing file names to the additional assemblies referenced by the script. </param>
        /// <returns>Compiled assembly file name.</returns>
        static public string CompileWithConfig(string scriptFile, string assemblyFile, bool debugBuild, Settings scriptSettings, string compilerOptions, params string[] refAssemblies)
        {
            using (Mutex fileLock = new Mutex(false, GetCompilerLockName(assemblyFile, scriptSettings)))
            {
                lock (CSExecutor.options)
                {
                    ExecuteOptions oldOptions = CSExecutor.options;
                    try
                    {
                        int start = Environment.TickCount;
                        fileLock.WaitOne(5000, false); //let other thread/process (if any) to finish loading/compiling the same file; 5 seconds should be enough, if you need more use more sophisticated synchronization
                        //Trace.WriteLine(">>>  Waited  " + (Environment.TickCount - start));

                        CSExecutor exec = new csscript.CSExecutor();
                        exec.Rethrow = true;

                        InitExecuteOptions(CSExecutor.options, scriptSettings, compilerOptions, ref scriptFile);
                        CSExecutor.options.DBG = debugBuild;
                        ExecuteOptions.options.useSmartCaching = CacheEnabled;

                        if (refAssemblies != null && refAssemblies.Length != 0)
                        {
                            string dir;
                            foreach (string file in refAssemblies)
                            {
                                dir = Path.GetDirectoryName(file);
                                CSExecutor.options.AddSearchDir(dir); //settings used by Compiler
                                CSScript.GlobalSettings.AddSearchDir(dir); //settings used by AsmHelper
                            }
                            CSExecutor.options.refAssemblies = refAssemblies;
                        }

                        if (CacheEnabled)
                        {
                            if (assemblyFile != null)
                            {
                                if (!ScriptAsmOutOfDate(scriptFile, assemblyFile))
                                    return assemblyFile;
                            }
                            else
                            {
                                Assembly asm = GetCachedScriptAssembly(scriptFile);
                                if (asm != null)
                                    return asm.Location;
                            }
                        }
#if !net1
                        string retval = exec.Compile(scriptFile, assemblyFile, debugBuild);
                        if (KeepCompilingHistory)
                            CompilingHistory.Add(new FileInfo(scriptFile), exec.LastCompileResult);
                        return retval;
#else
                        return exec.Compile(scriptFile, assemblyFile, debugBuild);
#endif
                    }
                    finally
                    {
                        CSExecutor.options = oldOptions;
                        try { fileLock.ReleaseMutex(); }
                        catch { }
                    }
                }
            }
        }

        private static ExecuteOptions InitExecuteOptions(ExecuteOptions options, Settings scriptSettings, string compilerOptions, ref string scriptFile)
        {
            Settings settings = (scriptSettings == null ? CSScript.GlobalSettings : scriptSettings);

            options.altCompiler = settings.ExpandUseAlternativeCompiler();
            options.compilerOptions = compilerOptions != null ? compilerOptions : "";
            options.apartmentState = settings.DefaultApartmentState;
            options.reportDetailedErrorInfo = settings.ReportDetailedErrorInfo;
            options.cleanupShellCommand = settings.CleanupShellCommand;
            options.inMemoryAsm = settings.InMemoryAsssembly;
            options.TargetFramework = settings.TargetFramework;
            options.doCleanupAfterNumberOfRuns = settings.DoCleanupAfterNumberOfRuns;
            options.useCompiled = CSScript.CacheEnabled;
            options.useSurrogateHostingProcess = false; //regardless of the input useSurrogateHostingProcess is not appropriate for teh hosting scenarios, so set it to 'false'

            ArrayList dirs = new ArrayList();

            options.shareHostRefAssemblies = ShareHostRefAssemblies;
            if (options.shareHostRefAssemblies)
            {
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                    try
                    {
                        if (asm is System.Reflection.Emit.AssemblyBuilder)
                            continue;

                        if (asm.FullName.StartsWith("Anonymously Hosted DynamicMethods") || !File.Exists(asm.Location))
                            continue;

                        dirs.Add(Path.GetDirectoryName(asm.Location));
                    }
                    catch
                    {
                        //Under ASP.NET some assemblies do not have location (e.g. dynamically built/emitted assemblies)
                        //in such case NotSupportedException will be raised

                        //In fact ignore all exceptions as we should continue if for whatever reason assembly the location cannot be obtained
                    }
            }

            string libDir = Environment.ExpandEnvironmentVariables("%CSSCRIPT_DIR%" + Path.DirectorySeparatorChar + "lib");
            if (!libDir.StartsWith("%"))
                dirs.Add(libDir);

            if (settings != null)
                dirs.AddRange(Environment.ExpandEnvironmentVariables(settings.SearchDirs).Split(",;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));

            if (scriptFile != "")
            {
                scriptFile = FileParser.ResolveFile(scriptFile, (string[])dirs.ToArray(typeof(string))); //to handle the case when the script file is specified by file name only
                dirs.Add(Path.GetDirectoryName(scriptFile));
            }

            options.searchDirs = RemovePathDuplicates((string[])dirs.ToArray(typeof(string)));

            options.scriptFileName = scriptFile;

            return options;
        }

        /// <summary>
        /// Surrounds the method implementation code into a class and compiles it code into assembly with CSExecutor and loads it in current AppDomain.
        /// The most convenient way of using dynamic methods is to declare them as static methods. In this case they can be invoked with wild card character as a class name (e.g. asmHelper.Invoke("*.SayHello")). Otherwise you will need to instantiate class "DyamicClass.Script" in order to call dynamic method.
        ///
        /// You can have multiple methods implementations in the single methodCode. Also you can specify namespaces at the begining of the code:
        ///
        /// CSScript.LoadMethod(
        ///     @"using System.Windows.Forms;
        ///
        ///     public static void SayHello(string gritting)
        ///     {
        ///         MessageBoxSayHello(gritting);
        ///         ConsoleSayHello(gritting);
        ///     }
        ///     public static void MessageBoxSayHello(string gritting)
        ///     {
        ///         MessageBox.Show(gritting);
        ///     }
        ///     public static void ConsoleSayHello(string gritting)
        ///     {
        ///         Console.WriteLine(gritting);
        ///     }");
        /// </summary>
        /// <param name="methodCode">The C# code, containing method implementation.</param>
        /// <param name="refAssemblies">The string array containing file names to the additional assemblies referenced by the script. </param>
        /// <returns>Compiled assembly.</returns>
        static public Assembly LoadMethod(string methodCode, params string[] refAssemblies)
        {
            return LoadMethod(methodCode, null, false, refAssemblies);
        }

        static string evalNamespaces;

        /// <summary>
        /// Sets the <c>;</c>-delimited string containing namespaces to be used by the C# expressions being compiled with
        /// <see cref="T:CSScriptLibrary.CSScript.EvalBuild"/> and <see cref="T:CSScriptLibrary.CSScript.Eval"/>.
        /// <para>The default value is <c>"System;System.IO;System.Diagnostics;System.Collections.Generic;System.Threading"</c></para>
        /// </summary>
        /// <para>The following is a typical example of <c>BuildEval</c> usage:</para>
        /// <code>
        /// CSScript.EvalNamespaces = "System;System.Diagnostics";
        ///
        /// var Trace = CSScript.BuildEval(@"trace (object message)
        ///                                  {
        ///                                      Trace.WriteLine(""EVAL:"" + message);
        ///                                  }");
        ///
        /// var Average = CSScript.BuildEval("avrg (int a, int b)  { return (a+b)/2.0; }");
        ///
        /// Trace(Average(7, 8));
        /// </code>
        /// <value>
        /// The <c>Eval</c> namespaces.
        /// </value>
        public static string EvalNamespaces
        {
            private get
            {
                if (evalNamespaces == null)
                    evalNamespaces = SplitNamespaces("System;System.IO;System.Diagnostics;System.Collections.Generic;System.Threading");
                return evalNamespaces;
            }
            set
            {
                evalNamespaces = SplitNamespaces(value);
            }
        }

        private static string SplitNamespaces(string text)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string @namespace in text.Split(new char[] { ';' }))
            {
                sb.Append("using ");
                sb.Append(@namespace);
                sb.Append(";\n");
            }
            return sb.ToString();
        }

#if !net1

        /// <summary>
        /// Evaluates string as a method code and returns the <see cref="T:CSScriptLibrary.MethodDelegate"/>.
        /// <para><c>BuildEval</c> is a specific case of <see cref="T:CSScriptLibrary.CSScript.LoadMethod"/>, which
        /// offers a simpler and more convenient syntactical model. It has number of limitations comparing to
        /// the <see cref="T:CSScriptLibrary.CSScript.LoadMethod"/>.
        /// <list type="bullet">
        ///         <item ><description><c>methodCode</c> should contain nothing else but only a single method definition</description></item>
        ///         <item ><description>The method signature should not contain any return type.</description></item>
        ///         <item ><description>All namespaces used by the method code should be either specified explicitly in code or listed in the value of <see cref="T:CSSCriptLibrary.CSScript.EvalNamespaces"/>.</description></item>
        ///         <item ><description>The method code can only interact with the types of the currently loaded in the <c>AppDomain.CurrentDomain</c> assemblies.</description></item>
        /// </list>
        /// This is the when the flexibility is partially sacrificed for the sake of convenience.
        /// <para>The following is a typical example of <c>BuildEval</c> usage:</para>
        /// <code>
        /// CSScript.EvalNamespaces = "System;System.Diagnostics";
        ///
        /// var Trace = CSScript.BuildEval(@"trace (object message)
        ///                                  {
        ///                                      Trace.WriteLine(""EVAL:"" + message);
        ///                                  }");
        ///
        /// var Average = CSScript.BuildEval("avrg (int a, int b)  { return (a+b)/2.0; }");
        ///
        /// Trace(Average(7, 8));
        /// </code>
        /// <remarks>Note that CS-Script <c>BuildEval</c> should not be treated as <c>eval</c> in dynamic languages even despite some resemblance. After all C# is a static language.
        /// <para>CS-Script <c>BuildEval</c> yields the method delegate, which can access all public types of the AppDomain but it cannot interact with the types instances unless
        /// they are directly passed to the delegate or can be accessed through the Type static members.</para>
        /// </remarks>
        ///
        ///
        /// </para>
        /// </summary>
        /// <param name="methodCode">The method code.</param>
        /// <returns>Delegate with the "evaluated" routine. It can be invoked as any .NET delegate.</returns>
        static public MethodDelegate BuildEval(string methodCode)
        {
            string[] refAssemblies;
            string code = GenerateEvalSourceCode(methodCode, out refAssemblies, false);

            Assembly asm = LoadMethod(code, null, true, refAssemblies);

            return asm.GetStaticMethod();
        }

        /// <summary>
        /// Evaluates string as a method code and executes it with the specified method parameters.
        /// <para>
        /// <c>Eval</c> is very similar to <see cref="T:CSScriptLibrary.CSScript.BuildEval"/> and it shares the some of its limitations.
        /// <list type="bullet">
        ///         <item ><description><c>methodCode</c> should contain nothing else but only a single method definition</description></item>
        ///         <item ><description>The method signature should not contain any return type.</description></item>
        ///         <item ><description>All namespaces used by the method code should be either specified explicitly in code or listed in the value of <see cref="T:CSSCriptLibrary.CSScript.EvalNamespaces"/>.</description></item>
        /// </list>
        /// However <c>Eval</c> offers an important advantage comparing to the <c>BuildEval</c> - after the execution it unloads all dynamically emitted routines
        /// preventing any potential memory leaks. Though because of this the "evaluated" routines are not reusable thus you need to do the full eval every time
        /// you wan to invoke the routine. And of course this can affect performance dramatically and that is why usage of <c>Eval</c> should be considered very carefully.
        ///
        /// <para>Note that the calling convention is that all parameters of the method to be "evaluated" must be
        /// followed by the string of code defining this method. </para>
        /// <para>The following is a typical example of <c>Eval</c> usage:</para>
        /// <code>
        ///
        /// var result = CSScript.Eval(1, 3,
        ///                          @"sum (int a, int b) {
        ///                                return a+b;
        ///                            }");
        ///
        /// </code>
        ///
        /// <remarks>Note that CS-Script <c>Eval</c> should not be treated as <c>eval</c> in dynamic languages even despite some resemblance.
        /// After all C# is a static language.
        /// <para>CS-Script <c>Eval</c> can access all public types of the AppDomain but it cannot interact with the types instances unless
        /// they are directly passed to the delegate or can be accessed through the Type static members.</para>
        /// </remarks>
        ///
        ///
        /// </para>
        /// </summary>
        /// <param name="args">Collection of the method parameters followed by the method code.</param>
        /// <returns>The return value of the method being "evaluated"</returns>
        static public object Eval(params object[] args)
        {
            if (args.Length == 0)
                throw new Exception("You did not specify the code to 'Eval'");

            object lastArg = args.Last();

            if (lastArg == null || !(lastArg is string))
                throw new Exception("You did not specify the code to 'Eval'");

            string methodCode = ((string)lastArg).Trim();

            string methodName = methodCode.Split(new char[] { '(', ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

            if (methodName == null)
                throw new Exception("The code to 'Eval' is not valid. The expected code patterns is as 'func(type arg1,..., type argN){ return <result>; }'");

            object[] newArgs = new object[args.Length - 1];

            Array.Copy(args, newArgs, newArgs.Length);

            string[] refAssemblies;
            string code = GenerateEvalSourceCode(methodCode, out refAssemblies, true);

            using (var helper = new AsmHelper(CSScript.CompileCode(code, null, true, refAssemblies), null, true))
            {
                return helper.Invoke("*." + methodName, newArgs);
            }
        }

        static private string GenerateEvalSourceCode(string methodCode, out string[] refAssemblies, bool injectClassDef)
        {
            string code = evalNamespaces;

            if (injectClassDef)
            {
                //code += "[assembly: System.Runtime.CompilerServices.InternalsVisibleTo( \"Testpad\" )]\n";
                code += "public static class EvalClass {\n";
            }

            code += "public static object ";

            if (methodCode.EndsWith("}"))
                code += methodCode.Substring(0, methodCode.Length - 1) + "\n    return null;\n}"; //ensure "return null; is injected just before the last bracket"
            else
                code += methodCode;

            if (injectClassDef)
                code += "}";

            refAssemblies = AppDomain.CurrentDomain
                                     .GetAssemblies()
                                     .Select(a =>
                                     {
                                         try
                                         {
                                             if (!(a is System.Reflection.Emit.AssemblyBuilder))
                                                 return a.Location;
                                         }
                                         catch
                                         {
                                         }
                                         return "";
                                     })
                                     .Where(a => a != "")
                                     .ToArray();
            return code;
        }

#endif
        private static object LoadAutoCodeSynch = new object();

        /// <summary>
        /// Surrounds the method implementation code into a class and compiles it code into
        /// assembly with CSExecutor and loads it in current AppDomain. The most convenient way of
        /// using dynamic methods is to declare them as static methods. In this case they can be
        /// invoked with wild card character as a class name (e.g. asmHelper.Invoke("*.SayHello")).
        /// Otherwise you will need to instantiate class "DyamicClass.Script" in order to call dynamic method.
        ///
        ///
        /// You can have multiple methods implementations in the single methodCode. Also you can specify namespaces at the beginning of the code:
        ///
        /// CSScript.LoadMethod(
        ///     @"using System.Windows.Forms;
        ///
        ///     public static void SayHello(string gritting)
        ///     {
        ///         MessageBoxSayHello(gritting);
        ///         ConsoleSayHello(gritting);
        ///     }
        ///     public static void MessageBoxSayHello(string gritting)
        ///     {
        ///         MessageBox.Show(gritting);
        ///     }
        ///     public static void ConsoleSayHello(string gritting)
        ///     {
        ///         Console.WriteLine(gritting);
        ///     }");
        /// </summary>
        /// <param name="methodCode">The C# code, containing method implementation.</param>
        /// <param name="assemblyFile">The name of compiled assembly. If set to null a temporary file name will be used.</param>
        /// <param name="debugBuild">'true' if debug information should be included in assembly; otherwise, 'false'.</param>
        /// <param name="refAssemblies">The string array containing file names to the additional assemblies referenced by the script. </param>
        /// <returns>Compiled assembly.</returns>
        static public Assembly LoadMethod(string methodCode, string assemblyFile, bool debugBuild, params string[] refAssemblies)
        {
            lock (LoadAutoCodeSynch)
            {
                StringBuilder code = new StringBuilder(4096);
                code.Append("//Auto-generated file\r\n"); //cannot use AppendLine as it is not available in StringBuilder v1.1
                code.Append("using System;\r\n");

                bool headerProcessed = false;
                string line;
                using (StringReader sr = new StringReader(methodCode))
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (!headerProcessed && !line.TrimStart().StartsWith("using ")) //not using...; statement of the file header
                            if (!line.StartsWith("//") && line.Trim() != "") //not comments or empty line
                            {
                                headerProcessed = true;
                                //code.Append("[assembly: System.Runtime.CompilerServices.InternalsVisibleTo( \"Testpad\" )]\r\n"); //zos
                                //code.Append("[assembly: System.Runtime.CompilerServices.InternalsVisibleTo( \"Rubenhak.Utils.Gen\" )]\r\n"); //zos
                                //code.Append("[assembly: System.Runtime.CompilerServices.InternalsVisibleTo( \"" + Assembly.GetExecutingAssembly().GetName().Name + "\" )]\r\n"); //zos
                                code.Append("namespace Scripting\r\n");
                                code.Append("{\r\n");
                                code.Append("   public class DynamicClass\r\n");
                                code.Append("   {\r\n");
                            }

                        code.Append(line);
                        code.Append("\r\n");
                    }

                code.Append("   }\r\n");
                code.Append("}\r\n");

                return LoadCode(code.ToString(), assemblyFile, debugBuild, refAssemblies);
            }
        }

        /// <summary>
        /// Compiles script code into assembly with CSExecutor and loads it in current AppDomain.
        /// </summary>
        /// <param name="scriptText">The script code to be compiled.</param>
        /// <param name="refAssemblies">The string array containing file names to the additional assemblies referenced by the script. </param>
        /// <returns>Compiled assembly.</returns>
        static public Assembly LoadCode(string scriptText, params string[] refAssemblies)
        {
            return LoadCode(scriptText, null, false, refAssemblies);
        }

        /// <summary>
        /// Compiles script code into assembly with CSExecutor and loads it in current AppDomain.
        /// </summary>
        /// <param name="scriptText">The script code to be compiled.</param>
        /// <param name="assemblyFile">The name of compiled assembly. If set to null a temporary file name will be used.</param>
        /// <param name="debugBuild">'true' if debug information should be included in assembly; otherwise, 'false'.</param>
        /// <param name="refAssemblies">The string array containing file names to the additional assemblies referenced by the script. </param>
        /// <returns>Compiled assembly.</returns>
        static public Assembly LoadCode(string scriptText, string assemblyFile, bool debugBuild, params string[] refAssemblies)
        {
            return LoadCode(scriptText, "", assemblyFile, debugBuild, refAssemblies);
        }

#if !net1
        static Dictionary<UInt32, string> dynamicScriptsAssemblies = new Dictionary<UInt32, string>();
#else
        static Hashtable dynamicScriptsAssemblies = new Hashtable();
#endif

        /// <summary>
        /// Compiles script code into assembly with CSExecutor and loads it in current AppDomain.
        /// </summary>
        /// <param name="scriptText">The script code to be compiled.</param>
        /// <param name="tempFileExtension">The file extension of the temporary file to hold script code during compilation. This parameter may be
        /// needed if custom CS-Script compilers rely on file extension to identify the script syntax.</param>
        /// <param name="assemblyFile">The name of compiled assembly. If set to null a temporary file name will be used.</param>
        /// <param name="debugBuild">'true' if debug information should be included in assembly; otherwise, 'false'.</param>
        /// <param name="refAssemblies">The string array containing file nemes to the additional assemblies referenced by the script. </param>
        /// <returns>Compiled assembly.</returns>
        static public Assembly LoadCode(string scriptText, string tempFileExtension, string assemblyFile, bool debugBuild, params string[] refAssemblies)
        {
            UInt32 scriptTextCRC = 0;
            if (CacheEnabled)
            {
                scriptTextCRC = Crc32.Compute(Encoding.Unicode.GetBytes(scriptText));
                if (dynamicScriptsAssemblies.ContainsKey(scriptTextCRC))
                    try
                    {
#if !net1
                        return Assembly.LoadFrom(dynamicScriptsAssemblies[scriptTextCRC]);
#else
                        return Assembly.LoadFrom(dynamicScriptsAssemblies[scriptTextCRC].ToString());
#endif
                    }
                    catch
                    {
                        Trace.WriteLine("Cannot use cache...");
                    }
            }

            string tempFile = CSExecutor.GetScriptTempFile();
            if (tempFileExtension != null && tempFileExtension != "")
                tempFile = Path.ChangeExtension(tempFile, tempFileExtension);

            try
            {
                using (StreamWriter sw = new StreamWriter(tempFile))
                {
                    sw.Write(scriptText);
                }

                Assembly asm = Load(tempFile, assemblyFile, debugBuild, refAssemblies);

                if (CacheEnabled)
                    if (dynamicScriptsAssemblies.ContainsKey(scriptTextCRC))
                        dynamicScriptsAssemblies[scriptTextCRC] = asm.Location;
                    else
                        dynamicScriptsAssemblies.Add(scriptTextCRC, asm.Location);

                return asm;
            }
            finally
            {
                if (!debugBuild)
                    File.Delete(tempFile);
                else
                {
                    if (tempFiles == null)
                    {
                        tempFiles = new ArrayList();
                        //Note: ApplicationExit will not be called if this library is hosted by a console application.
                        //Thus CS-Script periodical cleanup will take care of the temp files
                        //Application.ApplicationExit += new EventHandler(OnApplicationExit); //will not be available on .NET CE
                        AppDomain.CurrentDomain.DomainUnload += new EventHandler(CurrentDomain_DomainUnload);
                    }
                    tempFiles.Add(tempFile);
                }
            }
        }

        private static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            OnApplicationExit(sender, e);
        }

        static ArrayList tempFiles;

        private static void OnApplicationExit(object sender, EventArgs e)
        {
            if (tempFiles != null)
                foreach (string file in tempFiles)
                    try
                    {
                        File.Delete(file);
                    }
                    catch { }
        }

        /// <summary>
        /// Compiles script file into assembly with CSExecutor and loads it in current AppDomain
        /// </summary>
        /// <param name="scriptFile">The name of script file to be compiled.</param>
        /// <param name="assemblyFile">The name of compiled assembly. If set to null a temporary file name will be used.</param>
        /// <param name="debugBuild">'true' if debug information should be included in assembly; otherwise, 'false'.</param>
        /// /// <param name="refAssemblies">The string array containing file names to the additional assemblies referenced by the script. </param>
        /// <returns>Compiled/Loaded assembly.</returns>
        static public Assembly Load(string scriptFile, string assemblyFile, bool debugBuild, params string[] refAssemblies)
        {
            using (Mutex fileLock = new Mutex(false, GetCompilerLockName(assemblyFile, CSScript.GlobalSettings)))
            {
                ExecuteOptions oldOptions = CSExecutor.options;

                try
                {
                    int start = Environment.TickCount;
                    fileLock.WaitOne(5000, false); //let other thread/process (if any) to finish loading/compiling the same file; 2 seconds should be enough, if you need more use more sophisticated synchronization
                    Trace.WriteLine(">>>  Waited  " + (Environment.TickCount - start));

                    CSExecutor exec = new CSExecutor();
                    exec.Rethrow = true;

                    InitExecuteOptions(CSExecutor.options, CSScript.GlobalSettings, "", ref scriptFile);
                    CSExecutor.options.DBG = debugBuild;
                    ExecuteOptions.options.useSmartCaching = CacheEnabled;

                    if (refAssemblies != null && refAssemblies.Length != 0)
                    {
                        string dir;
                        foreach (string file in refAssemblies)
                        {
                            dir = Path.GetDirectoryName(file);
                            CSExecutor.options.AddSearchDir(dir); //settings used by Compiler
                            CSScript.GlobalSettings.AddSearchDir(dir); //settings used by AsmHelper
                        }
                        CSExecutor.options.refAssemblies = refAssemblies;
                    }

                    Assembly retval = null;

                    if (CacheEnabled)
                        retval = GetCachedScriptAssembly(scriptFile);

                    if (retval == null)
                    {
                        string outputFile = exec.Compile(scriptFile, assemblyFile, debugBuild);

#if !net1
                        if (KeepCompilingHistory)
                            CompilingHistory.Add(new FileInfo(scriptFile), exec.LastCompileResult);
#endif

                        if (!ExecuteOptions.options.inMemoryAsm)
                        {
                            retval = Assembly.LoadFrom(outputFile);
                        }
                        else
                        {
                            //Load(byte[]) does not lock the assembly file as LoadFrom(filename) does
                            using (FileStream fs = new FileStream(outputFile, FileMode.Open))
                            {
                                byte[] data = new byte[fs.Length];
                                fs.Read(data, 0, data.Length);
                                string dbg = Path.ChangeExtension(outputFile, ".pdb");
                                if (File.Exists(dbg))
                                {
                                    using (FileStream fsDbg = new FileStream(dbg, FileMode.Open))
                                    {
                                        byte[] dbgData = new byte[fsDbg.Length];
                                        fsDbg.Read(dbgData, 0, dbgData.Length);
                                        retval = Assembly.Load(data, dbgData);
                                    }
                                }
                                else
                                    retval = Assembly.Load(data);
                            }
                        }

                        if (retval != null)
                            scriptCache.Add(new LoadedScript(scriptFile, retval));
                    }
                    return retval;
                }
                finally
                {
                    CSExecutor.options = oldOptions;
                }
            }
        }

        /// <summary>
        /// Compiles script file into assembly (temporary file) with CSExecutor and loads it in current AppDomain.
        /// This method is an equivalent of the CSScript.Load(scriptFile, null, false);
        /// </summary>
        /// <param name="scriptFile">The name of script file to be compiled.</param>
        /// <returns>Compiled/Loaded assembly.</returns>
        static public Assembly Load(string scriptFile)
        {
            return Load(scriptFile, null, false, null);
        }

        /// <summary>
        /// Compiles script file into assembly (temporary file) with CSExecutor and loads it in current AppDomain.
        /// This method is an equivalent of the CSScript.Load(scriptFile, null, false);
        /// </summary>
        /// <param name="scriptFile">The name of script file to be compiled.</param>
        /// <param name="refAssemblies">The string array containing file nemes to the additional assemblies referenced by the script. </param>
        /// <returns>Compiled/Loaded assembly.</returns>
        static public Assembly Load(string scriptFile, params string[] refAssemblies)
        {
            return Load(scriptFile, null, false, refAssemblies);
        }

        /// <summary>
        /// Default implementation of displaying application messages.
        /// </summary>
        private static void DefaultPrint(string msg)
        {
            //do nothing
        }

        static bool rethrow;

        /// <summary>
        /// LoadedScript is a class, which holds information about the script file location and it's compiled and loaded assmbly (current AppDomain).
        /// </summary>
        public class LoadedScript
        {
            /// <summary>
            /// Creates instance of LoadedScript
            /// </summary>
            /// <param name="script">Script file location.</param>
            /// <param name="asm">Compiled script assembly loaded into current AppDomain.</param>
            public LoadedScript(string script, Assembly asm)
            {
                this.script = Path.GetFullPath(script);
                this.asm = asm;
            }

            /// <summary>
            /// Script file location.
            /// </summary>
            public string script;
            /// <summary>
            /// Compiled script assembly loaded into current AppDomain.
            /// </summary>
            public Assembly asm;
        }

        /// <summary>
        /// Controls if ScriptCache should be used when script file loading is requested (CSScript.Load(...)). If set to true and the script file was previously compiled and already loaded
        /// the script engine will use that compiled script from the cache instead of compiling it again.
        /// Note the script cache is always maintained by the script engine. The CacheEnabled property only indicates if the cached script should be used or not when CSScript.Load(...) method is called.
        /// </summary>
        public static bool CacheEnabled = true;

        /// <summary>
        /// Cache of all loaded script files for the current process.
        /// </summary>
        public static LoadedScript[] ScriptCache
        {
            get
            {
#if net1
                return (LoadedScript[])scriptCache.ToArray(typeof(LoadedScript));
#else
                return scriptCache.ToArray();
#endif
            }
        }

#if net1
        static ArrayList scriptCache = new ArrayList();
#else
        static List<LoadedScript> scriptCache = new List<LoadedScript>();
#endif

        /// <summary>
        /// Returns cached script assembly matching the scrpt file name.
        /// </summary>
        /// <param name="file">Pull path of the script file.</param>
        /// <returns>Assembly loaded int the current AppDomain.
        /// Returns null if the loaded script cannot be found.
        /// </returns>
        public static Assembly GetCachedScriptAssembly(string file)
        {
            string path = Path.GetFullPath(file);

            foreach (LoadedScript item in ScriptCache)
                if (item.script == path && !ScriptAsmOutOfDate(path, item.asm.Location))
                    return item.asm;

            string cacheFile = Path.Combine(csscript.CSSEnvironment.GetCacheDirectory(path), Path.GetFileName(path) + ".compiled");

            if (File.Exists(cacheFile) && !ScriptAsmOutOfDate(path, cacheFile))
                return Assembly.LoadFrom(cacheFile);

            return null;
        }

        internal static bool ScriptAsmOutOfDate(string scriptFileName, string assemblyFileName)
        {
            if (File.GetLastWriteTimeUtc(scriptFileName) != File.GetLastWriteTimeUtc(assemblyFileName))
                return true;

            return MetaDataItems.IsOutOfDate(scriptFileName, assemblyFileName);
        }

        internal static string[] RemovePathDuplicates(string[] list)
        {
            lock (typeof(CSScript))
            {
                return Utils.RemovePathDuplicates(list);
            }
        }
    }
}

namespace csscript
{
    /// <summary>
    /// This class implements access to the CS-Script global configuration settings.
    /// </summary>
    public class CSSEnvironment
    {
        /// <summary>
        /// Generates the name of the cache directory for the specified script file.
        /// </summary>
        /// <param name="file">Script file name.</param>
        /// <returns>Cache directory name.</returns>
        public static string GetCacheDirectory(string file)
        {
            return CSExecutor.GetCacheDirectory(file);
        }

        /// <summary>
        /// Saves code to the script file in the dedicated CS-Script <c>temporary files</c> location. You do not have to delete the script file after the execution.
        /// It will be deleted as part of the periodical automatic CS-Script maintenance.
        /// </summary>
        /// <param name="content">The script file content.</param>
        /// <returns>Name of the created temporary script file.</returns>
        public static string SaveAsTempScript(string content)
        {
            string tempFile = CSExecutor.GetScriptTempFile();
            using (StreamWriter sw = new StreamWriter(tempFile))
            {
                sw.Write(content);
            }
            return tempFile;
        }

        /// <summary>
        /// Generates the script file path in the dedicated CS-Script <c>temporary files</c> location. You do not have to delete such file after the execution.
        /// It will be deleted as part of the periodical automatic CS-Script maintenance.
        /// </summary>
        /// <returns>Name of the temporary script file.</returns>
        public static string GetTempScriptName()
        {
            return CSExecutor.GetScriptTempFile();
        }

        /// <summary>
        /// Sets the location for the CS-Script temporary files directory.
        /// </summary>
        /// <param name="path">The path for the temporary directory.</param>
        static public void SetScriptTempDir(string path)
        {
            CSExecutor.SetScriptTempDir(path);
        }

        /// <summary>
        /// The full name of the script file being executed.
        /// </summary>
        public static string ScriptFile
        {
            get
            {
                scriptFile = FindExecuteOptionsField(Assembly.GetExecutingAssembly(), "scriptFileName");
                if (scriptFile == null)
                    scriptFile = FindExecuteOptionsField(Assembly.GetEntryAssembly(), "scriptFileName");
                return scriptFile;
            }
        }

        static private string scriptFile = null;

        /// <summary>
        /// The full name of the primary script file being executed. Usually it is the same file as ScriptFile.
        /// However these fields are different if analysed from the pre/post-script.
        /// </summary>
        public static string PrimaryScriptFile
        {
            get
            {
                if (scriptFileNamePrimary == null)
                {
                    scriptFileNamePrimary = FindExecuteOptionsField(Assembly.GetExecutingAssembly(), "scriptFileNamePrimary");
                    if (scriptFileNamePrimary == null || scriptFileNamePrimary == "")
                        scriptFileNamePrimary = FindExecuteOptionsField(Assembly.GetEntryAssembly(), "scriptFileNamePrimary");
                }
                return scriptFileNamePrimary;
            }
        }

        static private string scriptFileNamePrimary = null;

        static private string FindExecuteOptionsField(Assembly asm, string field)
        {
            Type t = asm.GetModules()[0].GetType("csscript.ExecuteOptions");
            if (t != null)
            {
                foreach (FieldInfo fi in t.GetFields(BindingFlags.Static | BindingFlags.Public))
                {
                    if (fi.Name == "options")
                    {
                        //need to use reflection as we might be running either cscs.exe or the script host application
                        //thus there is no warranty which assembly contains correct "options" object
                        object otionsObject = fi.GetValue(null);
                        if (otionsObject != null)
                        {
                            object scriptFileObject = otionsObject.GetType().GetField(field).GetValue(otionsObject);
                            if (scriptFileObject != null)
                                return scriptFileObject.ToString();
                        }
                        break;
                    }
                }
            }
            return null;
        }

        private CSSEnvironment()
        {
        }
    }

    delegate void PrintDelegate(string msg);

    /// <summary>
    /// Repository for application specific data
    /// </summary>
    internal class AppInfo
    {
        public static string appName = "CSScriptLibrary";
        public static bool appConsole = false;

        public static string appLogo
        {
            get { return "C# Script execution engine. Version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + ".\nCopyright (C) 2004 Oleg Shilo.\n"; }
        }

        public static string appLogoShort
        {
            get { return "C# Script execution engine. Version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + ".\n"; }
        }

        //#pragma warning disable 414
        public static string appParams = "[/nl]:";
        //#pragma warning restore 414
        public static string appParamsHelp = "nl	-	No logo mode: No banner will be shown at execution time.\n";
    }
}