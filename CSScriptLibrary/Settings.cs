#region Licence...

//-----------------------------------------------------------------------------
// Date:	25/10/10	Time: 2:33p
// Module:	settings.cs
// Classes:	Settings
//			ExecuteOptions
//
// This module contains the definition of the CSExecutor class. Which implements
// compiling C# code and executing 'Main' method of compiled assembly
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

#if net1
using System.Collections;
#else

#endif

using System.Threading;
using System.Xml;

#if !InterfaceAssembly

using System.Drawing.Design;

#endif

using System.ComponentModel;

namespace csscript
{
    /// <summary>
    /// Settings is an class that holds CS-Script application settings.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Command to be executed to perform custom cleanup.
        /// If this value is empty automatic cleanup of all
        /// temporary files will occurs after the script execution.
        /// This implies that the script has to be executed in the
        /// separate AppDomain and some performance penalty will be incurred.
        ///
        /// Setting this value to the command for custom cleanup application
        /// (e.g. csc.exe cleanTemp.cs) will force the script engine to execute
        /// script in the 'current' AppDomain what will improve performance.
        /// </summary>
        [Category("CustomCleanup"), Description("Command to be executed to perform custom cleanup.")]
        public string CleanupShellCommand
        {
            get { return cleanupShellCommand; }
            set { cleanupShellCommand = value; }
        }

        /// <summary>
        /// Returns value of the CleanupShellCommand (with expanding environment variables).
        /// </summary>
        /// <returns>shell command string</returns>
        public string ExpandCleanupShellCommand() { return Environment.ExpandEnvironmentVariables(cleanupShellCommand); }

        private string cleanupShellCommand = "";

        /// <summary>
        /// This value indicates frequency of the custom cleanup
        /// operation. It has affect only if CleanupShellCommand is not empty.
        /// </summary>
        [Category("CustomCleanup"), Description("This value indicates frequency of the custom cleanup operation.")]
        public uint DoCleanupAfterNumberOfRuns
        {
            get { return doCleanupAfterNumberOfRuns; }
            set { doCleanupAfterNumberOfRuns = value; }
        }

        private uint doCleanupAfterNumberOfRuns = 30;

        /// <summary>
        /// Location of alternative code provider assembly. If set it forces script engine to use an alternative code compiler.
        /// </summary>
        [Category("Extensibility"), Description("Location of alternative code provider assembly. If set it forces script engine to use an alternative code compiler.")]
#if !InterfaceAssembly
        [Editor(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(UITypeEditor))]
#endif
        public string UseAlternativeCompiler
        {
            get { return useAlternativeCompiler; }
            set { useAlternativeCompiler = value; }
        }

        /// <summary>
        /// Returns value of the UseAlternativeCompiler (with expanding environment variables).
        /// </summary>
        /// <returns>Path string</returns>
        public string ExpandUseAlternativeCompiler() { return Environment.ExpandEnvironmentVariables(useAlternativeCompiler); }

        private string useAlternativeCompiler = "";

        /// <summary>
        /// Location of PostProcessor assembly. If set it forces script engine to pass compiled script through PostProcessor before the execution.
        /// </summary>
        [Category("Extensibility"), Description("Location of PostProcessor assembly. If set it forces script engine to pass compiled script through PostProcessor before the execution.")]
#if !InterfaceAssembly
        [Editor(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(UITypeEditor))]
#endif
        public string UsePostProcessor
        {
            get { return usePostProcessor; }
            set { usePostProcessor = value; }
        }

        /// <summary>
        /// Returns value of the UsePostProcessor (with expanding environment variables).
        /// </summary>
        /// <returns>Path string</returns>
        public string ExpandUsePostProcessor() { return Environment.ExpandEnvironmentVariables(usePostProcessor); }

        private string usePostProcessor = "";

        /// <summary>
        /// DefaultApartmentState is an ApartmemntState, which will be used
        /// at run-time if none specified in the code with COM threading model attributes.
        /// </summary>
        [Category("RuntimeSettings"), Description("DefaultApartmentState is an ApartmemntState, which will be used at run-time if none specified in the code with COM threading model attributes.")]
        public ApartmentState DefaultApartmentState
        {
            get { return defaultApartmentState; }
            set { defaultApartmentState = value; }
        }

        private ApartmentState defaultApartmentState = ApartmentState.STA;

        /// <summary>
        /// Default command-line arguments. For example if "/dbg" is specified all scripts will be compiled in debug mode
        /// regardless if the user specified "/dbg" when a particular script is launched.
        /// </summary>
        [Category("RuntimeSettings"), Description("Default command-line arguments (e.g./dbg) for all scripts.")]
        public string DefaultArguments
        {
            get { return defaultArguments; }
            set { defaultArguments = value; }
        }

        private string defaultArguments = CSSUtils.cmdFlagPrefix + "c " + CSSUtils.cmdFlagPrefix + "sconfig";

        ///// <summary>
        ///// Enables using a surrogate process to host the script engine at runtime. This may be a useful option for fine control over the hosting process
        ///// (e.g. ensuring "CPU type" of the process, CLR version to be loaded).
        ///// </summary>
        //[Category("RuntimeSettings")]
        //internal bool UseSurrogateHostingProcess  //do not expose it to the user just yet
        //{
        //    get { return useSurrogatepHostingProcess; }
        //    set { useSurrogatepHostingProcess = value; }
        //}

        private bool useSurrogatepHostingProcess = false;

        bool openEndDirectiveSyntax = true;

        /// <summary>
        /// Enables omitting closing character (";") for CS-Script directives (e.g. "//css_ref System.Xml.dll" instead of "//css_ref System.Xml.dll;").
        /// </summary>
        [Browsable(false)]
        public bool OpenEndDirectiveSyntax
        {
            get { return openEndDirectiveSyntax; }
            set { openEndDirectiveSyntax = value; }
        }

        /// <summary>
        /// Specifies the .NET Framework version that the script is compiled against. This option can have the following values:
        ///   v2.0
        ///   v3.0
        ///   v3.5
        ///   v4.0
        /// </summary>
        [Browsable(false)]
        public string TargetFramework
        {
            get { return targetFramework; }
            set { targetFramework = value; }
        }

#if net35
        private string targetFramework = "v3.5";
#else
        private string targetFramework = "v4.0";
#endif

        /// <summary>
        /// Specifies the .NET Framework version that the script is compiled against. This option can have the following values:
        ///   v2.0
        ///   v3.0
        ///   v3.5
        ///   v4.0
        /// </summary>
        [Category("RuntimeSettings")]
        [Description("Specifies the .NET Framework version that the script is compiled against.\nThis option is for compilation only.\nFor changing the script execution CLR use //css_host directive from the script.\nYou are discouraged from modifying this value thus if the change is required you need to edit css_config.xml file directly.")]
        public string CompilerFramework
        {
            get { return targetFramework; }
        }

        /// <summary>
        /// List of assembly names to be automatically referenced by the scripst. The items must be separated by coma or semicolon. Specifying .dll extension (e.g. System.Core.dll) is optional.
        /// Assembly can contain expandable environment variables.
        /// </summary>
        [Category("Extensibility"), Description("List of assembly names to be automatically referenced by the scripts (e.g. System.dll, System.Core.dll). Assembly extension is optional.")]
        public string DefaultRefAssemblies
        {
            get { return defaultRefAssemblies; }
            set { defaultRefAssemblies = value; }
        }

        private string defaultRefAssemblies = "System.Core; System.Linq;";

        /// <summary>
        /// Returns value of the DefaultRefAssemblies (with expanding environment variables).
        /// </summary>
        /// <returns>List of assembly names</returns>
        public string ExpandDefaultRefAssemblies() { return Environment.ExpandEnvironmentVariables(DefaultRefAssemblies); }

        /// <summary>
        /// List of directories to be used to search (probing) for referenced assemblies and script files.
        /// This setting is similar to the system environment variable PATH.
        /// </summary>
        [Category("Extensibility"), Description("List of directories to be used to search (probing) for referenced assemblies and script files.\nThis setting is similar to the system environment variable PATH.")]
        public string SearchDirs
        {
            get { return searchDirs; }
            set { searchDirs = value; }
        }

        private string searchDirs = "%CSSCRIPT_DIR%" + Path.DirectorySeparatorChar + "Lib";

        /// <summary>
        /// Add search directory to the search (probing) path Settings.SearchDirs.
        /// For example if Settings.SearchDirs = "c:\scripts" then after call Settings.AddSearchDir("c:\temp") Settings.SearchDirs is "c:\scripts;c:\temp"
        /// </summary>
        /// <param name="dir">Directory path.</param>
        public void AddSearchDir(string dir)
        {
            foreach (string searchDir in searchDirs.Split(';'))
                if (searchDir != "" && Utils.IsSamePath(Path.GetFullPath(searchDir), Path.GetFullPath(dir)))
                    return; //already there

            searchDirs += ";" + dir;
        }

        /// <summary>
        /// The value, which indicates if auto-generated files (if any) should should be hidden in the temporary directory.
        /// </summary>
        [Category("RuntimeSettings"), Description("The value, which indicates if auto-generated files (if any) should should be hidden in the temporary directory.")]
        public HideOptions HideAutoGeneratedFiles
        {
            get { return hideOptions; }
            set { hideOptions = value; }
        }

        private string precompiler = "";

        /// <summary>
        /// Path to the precompiller script/assembly (see documentation for details). You can specify multiple recompiles separating them by semicolon.
        /// </summary>
        [Category("RuntimeSettings"), Description("Path to the precompiller script/assembly (see documentation for details). You can specify multiple recompiles separating them by semicolon.")]
        public string Precompiler
        {
            get { return precompiler; }
            set { precompiler = value; }
        }

        private HideOptions hideOptions = HideOptions.HideMostFiles;
        ///// <summary>
        ///// The value, which indicates which version of CLR compiler should be used to compile script.
        ///// For example CLR 2.0 can use the following compiler versions:
        ///// default - .NET 2.0
        ///// 3.5 - .NET 3.5
        ///// Use empty string for default compiler.
        ///// </summary>private string compilerVersion = "";
        //[Category("RuntimeSettings")]
        //public string CompilerVersion
        //{
        //    get { return compilerVersion; }
        //    set { compilerVersion = value; }
        //}
        //private string compilerVersion = "";

        /// <summary>
        /// Enum for possible hide auto-generated files scenarios
        /// Note: when HideAll is used it is responsibility of the pre/post script to implement actual hiding.
        /// </summary>
        public enum HideOptions
        {
            /// <summary>
            /// Do not hide auto-generated files.
            /// </summary>
            DoNotHide,
            /// <summary>
            /// Hide the most of the auto-generated (cache and "imported") files.
            /// </summary>
            HideMostFiles,
            /// <summary>
            /// Hide all auto-generated files including the files generated by pre/post scripts.
            /// </summary>
            HideAll
        }

        /// <summary>
        /// Boolean flag that indicates how much error details to be reported should error occur.
        /// false - Top level exception will be reported
        /// true - Whole exception stack will be reported
        /// </summary>
        [Category("RuntimeSettings"), Description("Indicates how much error details to be reported should error occur.")]
        public bool ReportDetailedErrorInfo
        {
            get { return reportDetailedErrorInfo; }
            set { reportDetailedErrorInfo = value; }
        }

        bool reportDetailedErrorInfo = true;

        /// <summary>
        /// Gets or sets a value indicating whether Optimistic Concurrency model should be used when executing scripts from the host application.
        /// If set to <c>true</c> the script loading (not the execution) is globally thread-safe. If set to <c>false</c> the script loading is
        /// thread-safe only among loading operations for the same script file.
        /// <para>The default value is <c>true</c>.</para>
        /// </summary>
        /// <value>
        /// 	<c>true</c> if Optimistic Concurrency model otherwise, <c>false</c>.
        /// </value>
        [Browsable(false)]
        public bool OptimisticConcurrencyModel
        {
            get { return optimisticConcurrencyModel; }
            set { optimisticConcurrencyModel = value; }
        }

        private bool optimisticConcurrencyModel = true;

        /// <summary>
        /// Boolean flag that indicates if compiler warnings should be included in script compilation output.
        /// false - warnings will be displayed
        /// true - warnings will not be displayed
        /// </summary>
        [Category("RuntimeSettings"), Description("Indicates if compiler warnings should be included in script compilation output.")]
        public bool HideCompilerWarnings
        {
            get { return hideCompilerWarnings; }
            set { hideCompilerWarnings = value; }
        }

        private bool hideCompilerWarnings = false;

        /// <summary>
        /// Boolean flag that indicates the script assembly is to be loaded by CLR as an in-memory byte stream instead of the file.
        /// This setting can be useful when you need to prevent script assembly (compiled script) from locking by CLR during the execution.
        /// false - script assembly will be loaded as a file. It is an equivalent of Assembly.LoadFrom(string assemblyFile).
        /// true - script assembly will be loaded as a file. It is an equivalent of Assembly.Load(byte[] rawAssembly)
        /// </summary>
        [Category("RuntimeSettings"), Description("Indicates the script assembly is to be loaded by CLR as an in-memory byte stream instead of the file.")]
        public bool InMemoryAsssembly
        {
            get { return inMemoryAsm; }
            set { inMemoryAsm = value; }
        }

        private bool inMemoryAsm = false;

        /// <summary>
        /// Saves CS-Script application settings to a file (.dat).
        /// </summary>
        /// <param name="fileName">File name of the .dat file</param>
        public void Save(string fileName)
        {
            //It is very tempting to use XmlSerializer but it adds 200 ms to the
            //application startup time. Whereas current startup delay for cscs.exe is just a 100 ms.
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<CSSConfig/>");
                doc.DocumentElement.AppendChild(doc.CreateElement("defaultArguments")).AppendChild(doc.CreateTextNode(defaultArguments));
                doc.DocumentElement.AppendChild(doc.CreateElement("defaultApartmentState")).AppendChild(doc.CreateTextNode(defaultApartmentState.ToString()));
                doc.DocumentElement.AppendChild(doc.CreateElement("reportDetailedErrorInfo")).AppendChild(doc.CreateTextNode(reportDetailedErrorInfo.ToString()));
                doc.DocumentElement.AppendChild(doc.CreateElement("useAlternativeCompiler")).AppendChild(doc.CreateTextNode(useAlternativeCompiler));
                doc.DocumentElement.AppendChild(doc.CreateElement("usePostProcessor")).AppendChild(doc.CreateTextNode(usePostProcessor));
                doc.DocumentElement.AppendChild(doc.CreateElement("searchDirs")).AppendChild(doc.CreateTextNode(searchDirs));
                doc.DocumentElement.AppendChild(doc.CreateElement("cleanupShellCommand")).AppendChild(doc.CreateTextNode(cleanupShellCommand));
                doc.DocumentElement.AppendChild(doc.CreateElement("doCleanupAfterNumberOfRuns")).AppendChild(doc.CreateTextNode(doCleanupAfterNumberOfRuns.ToString()));
                doc.DocumentElement.AppendChild(doc.CreateElement("hideOptions")).AppendChild(doc.CreateTextNode(hideOptions.ToString()));
                doc.DocumentElement.AppendChild(doc.CreateElement("hideCompilerWarnings")).AppendChild(doc.CreateTextNode(hideCompilerWarnings.ToString()));
                doc.DocumentElement.AppendChild(doc.CreateElement("inMemoryAsm")).AppendChild(doc.CreateTextNode(inMemoryAsm.ToString()));
                doc.DocumentElement.AppendChild(doc.CreateElement("TragetFramework")).AppendChild(doc.CreateTextNode(TargetFramework));
                doc.DocumentElement.AppendChild(doc.CreateElement("defaultRefAssemblies")).AppendChild(doc.CreateTextNode(defaultRefAssemblies));
                doc.DocumentElement.AppendChild(doc.CreateElement("useSurrogatepHostingProcess")).AppendChild(doc.CreateTextNode(useSurrogatepHostingProcess.ToString()));
                doc.DocumentElement.AppendChild(doc.CreateElement("openEndDirectiveSyntax")).AppendChild(doc.CreateTextNode(openEndDirectiveSyntax.ToString()));
                doc.DocumentElement.AppendChild(doc.CreateElement("Precompiler")).AppendChild(doc.CreateTextNode(Precompiler));

                doc.Save(fileName);
            }
            catch { }
        }

        /// <summary>
        /// Loads CS-Script application settings from a file. Default settings object is returned if it cannot be loaded from the file.
        /// </summary>
        /// <param name="fileName">File name of the XML file</param>
        /// <returns>Setting object desterilized from the XML file</returns>
        public static Settings Load(string fileName)
        {
            return Load(fileName, true);
        }

        /// <summary>
        /// Loads CS-Script application settings from a file.
        /// </summary>
        /// <param name="fileName">File name of the XML file</param>
        /// <param name="createAlways">Create and return default settings object if it cannot be loaded from the file.</param>
        /// <returns>Setting object desterilized from the XML file</returns>
        public static Settings Load(string fileName, bool createAlways)
        {
            Settings settings = new Settings();
            if (File.Exists(fileName))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(fileName);
                    XmlNode data = doc.FirstChild;
                    settings.defaultArguments = data.SelectSingleNode("defaultArguments").InnerText;
                    settings.defaultApartmentState = (ApartmentState)Enum.Parse(typeof(ApartmentState), data.SelectSingleNode("defaultApartmentState").InnerText, false);
                    settings.reportDetailedErrorInfo = data.SelectSingleNode("reportDetailedErrorInfo").InnerText.ToLower() == "true";
                    settings.UseAlternativeCompiler = data.SelectSingleNode("useAlternativeCompiler").InnerText;
                    settings.UsePostProcessor = data.SelectSingleNode("usePostProcessor").InnerText;
                    settings.SearchDirs = data.SelectSingleNode("searchDirs").InnerText;
                    settings.cleanupShellCommand = data.SelectSingleNode("cleanupShellCommand").InnerText;
                    settings.doCleanupAfterNumberOfRuns = uint.Parse(data.SelectSingleNode("doCleanupAfterNumberOfRuns").InnerText);
                    settings.hideOptions = (HideOptions)Enum.Parse(typeof(HideOptions), data.SelectSingleNode("hideOptions").InnerText, true);
                    settings.hideCompilerWarnings = data.SelectSingleNode("hideCompilerWarnings").InnerText.ToLower() == "true";
                    settings.inMemoryAsm = data.SelectSingleNode("inMemoryAsm").InnerText.ToLower() == "true";
                    settings.TargetFramework = data.SelectSingleNode("TragetFramework").InnerText;
                    settings.defaultRefAssemblies = data.SelectSingleNode("defaultRefAssemblies").InnerText;
                    settings.useSurrogatepHostingProcess = data.SelectSingleNode("useSurrogatepHostingProcess").InnerText.ToLower() == "true";
                    settings.OpenEndDirectiveSyntax = data.SelectSingleNode("openEndDirectiveSyntax").InnerText.ToLower() == "true";
                    settings.Precompiler = data.SelectSingleNode("Precompiler").InnerText;
                }
                catch
                {
                    if (!createAlways)
                        settings = null;
                    else
                        settings.Save(fileName);
                }

                CSharpParser.OpenEndDirectiveSyntax = settings.OpenEndDirectiveSyntax;
            }
            return settings;
        }
    }
}