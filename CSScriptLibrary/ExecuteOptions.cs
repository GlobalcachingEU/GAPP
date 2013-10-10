#region Licence...

//-----------------------------------------------------------------------------
// Date:	17/10/04	Time: 2:33p
// Module:	csscript.cs
// Classes:	CSExecutor
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
using System.Reflection;

#if net1
using System.Collections;
#else

using System.Collections.Generic;

#endif

using System.Text;
using CSScriptLibrary;
using System.Runtime.InteropServices;
using System.Threading;
using System.CodeDom.Compiler;
//using System.Windows.Forms;
using System.Globalization;
using System.Diagnostics;
using Microsoft.CSharp;


namespace csscript
{
    /// <summary>
    /// Application specific runtime settings
    /// </summary>
    internal class ExecuteOptions : ICloneable
    {
        public static ExecuteOptions options;

        public ExecuteOptions()
        {
            options = this;
        }

        public object Clone()
        {
            ExecuteOptions clone = new ExecuteOptions();
            clone.processFile = this.processFile;
            clone.scriptFileName = this.scriptFileName;
            clone.noLogo = this.noLogo;
            clone.useCompiled = this.useCompiled;
            clone.useSmartCaching = this.useSmartCaching;
            clone.DLLExtension = this.DLLExtension;
            clone.forceCompile = this.forceCompile;
            clone.supressExecution = this.supressExecution;
            clone.DBG = this.DBG;
            clone.TargetFramework = this.TargetFramework;
            clone.verbose = this.verbose;
            clone.startDebugger = this.startDebugger;
            clone.local = this.local;
            clone.buildExecutable = this.buildExecutable;
#if net1
                clone.refAssemblies = (string[])new ArrayList(this.refAssemblies).ToArray(typeof(string));
                clone.searchDirs = (string[])new ArrayList(this.searchDirs).ToArray(typeof(string));

#else
            clone.refAssemblies = new List<string>(this.refAssemblies).ToArray();
            clone.searchDirs = new List<string>(this.searchDirs).ToArray();
#endif

            clone.buildWinExecutable = this.buildWinExecutable;
            clone.useSurrogateHostingProcess = this.useSurrogateHostingProcess;
            clone.altCompiler = this.altCompiler;
            clone.preCompilers = this.preCompilers;
            clone.postProcessor = this.postProcessor;
            clone.compilerOptions = this.compilerOptions;
            clone.reportDetailedErrorInfo = this.reportDetailedErrorInfo;
            clone.hideCompilerWarnings = this.hideCompilerWarnings;
            clone.apartmentState = this.apartmentState;
            clone.openEndDirectiveSyntax = this.openEndDirectiveSyntax;
            clone.forceOutputAssembly = this.forceOutputAssembly;
            clone.cleanupShellCommand = this.cleanupShellCommand;
            clone.versionOnly = this.versionOnly;
            clone.noConfig = this.noConfig;
            //clone.suppressExternalHosting = this.suppressExternalHosting;
            clone.altConfig = this.altConfig;
            clone.defaultRefAssemblies = this.defaultRefAssemblies;
            clone.hideTemp = this.hideTemp;
            clone.autoClass = this.autoClass;
            clone.compilationContext = this.compilationContext;
            clone.useScriptConfig = this.useScriptConfig;
            clone.customConfigFileName = this.customConfigFileName;
            clone.scriptFileNamePrimary = this.scriptFileNamePrimary;
            clone.doCleanupAfterNumberOfRuns = this.doCleanupAfterNumberOfRuns;
            clone.inMemoryAsm = this.inMemoryAsm;
            clone.shareHostRefAssemblies = this.shareHostRefAssemblies;
            return clone;
        }

        public object Derive()
        {
            ExecuteOptions clone = new ExecuteOptions();
            clone.processFile = this.processFile;
            //clone.scriptFileName = this.scriptFileName;
            //clone.noLogo = this.noLogo;
            //clone.useCompiled = this.useCompiled;
            clone.useSmartCaching = this.useSmartCaching;
            //clone.DLLExtension = this.DLLExtension;
            //clone.forceCompile = this.forceCompile;
            clone.supressExecution = this.supressExecution;
            clone.InjectScriptAssemblyAttribute = this.InjectScriptAssemblyAttribute;
            clone.DBG = this.DBG;
            clone.TargetFramework = this.TargetFramework;
            clone.verbose = this.verbose;
            clone.local = this.local;
            clone.buildExecutable = this.buildExecutable;
#if net1
                clone.refAssemblies = (string[])new ArrayList(this.refAssemblies).ToArray(typeof(string));
                clone.searchDirs = (string[])new ArrayList(this.searchDirs).ToArray(typeof(string));
#else
            clone.refAssemblies = new List<string>(this.refAssemblies).ToArray();
            clone.searchDirs = new List<string>(this.searchDirs).ToArray();
#endif
            clone.buildWinExecutable = this.buildWinExecutable;
            clone.altCompiler = this.altCompiler;
            clone.preCompilers = this.preCompilers;
            clone.defaultRefAssemblies = this.defaultRefAssemblies;
            clone.postProcessor = this.postProcessor;
            clone.compilerOptions = this.compilerOptions;
            clone.reportDetailedErrorInfo = this.reportDetailedErrorInfo;
            clone.hideCompilerWarnings = this.hideCompilerWarnings;
            clone.openEndDirectiveSyntax = this.openEndDirectiveSyntax;
            clone.apartmentState = this.apartmentState;
            clone.forceOutputAssembly = this.forceOutputAssembly;
            clone.versionOnly = this.versionOnly;
            clone.cleanupShellCommand = this.cleanupShellCommand;
            clone.noConfig = this.noConfig;
            //clone.suppressExternalHosting = this.suppressExternalHosting;
            clone.compilationContext = this.compilationContext;
            clone.autoClass = this.autoClass;
            clone.altConfig = this.altConfig;
            clone.hideTemp = this.hideTemp;
            clone.scriptFileNamePrimary = this.scriptFileNamePrimary;
            clone.doCleanupAfterNumberOfRuns = this.doCleanupAfterNumberOfRuns;
            clone.shareHostRefAssemblies = this.shareHostRefAssemblies;
            clone.inMemoryAsm = this.inMemoryAsm;

            return clone;
        }

        public bool inMemoryAsm = false;
        public bool processFile = true;
        public int compilationContext = 0;
        public string scriptFileName = "";
        public string scriptFileNamePrimary = null;
        public bool noLogo = false;
        public bool useCompiled = false;
        public bool useScriptConfig = false;
        public string customConfigFileName = "";
        public bool useSmartCaching = true; //hardcoded true but can be set from config file in the future
        public bool DLLExtension = false;
        public bool forceCompile = false;
        public bool supressExecution = false;
        public bool DBG = false;
#if net35
        public string TargetFramework = "v3.5";
#else
        public string TargetFramework = "v4.0";
#endif
        internal bool InjectScriptAssemblyAttribute = true;
        public bool verbose = false;
        public bool startDebugger = false;
        public bool local = false;
        public bool buildExecutable = false;
        public string[] refAssemblies = new string[0];
        public string[] searchDirs = new string[0];
        public bool shareHostRefAssemblies = false;
        public bool buildWinExecutable = false;
        public bool openEndDirectiveSyntax = true;
        public bool useSurrogateHostingProcess = false;
        public string altCompiler = "";
        public string preCompilers = "";
        public string defaultRefAssemblies = "";
        public string postProcessor = "";
        public bool reportDetailedErrorInfo = false;
        public bool hideCompilerWarnings = false;
        public ApartmentState apartmentState = ApartmentState.STA;
        public string forceOutputAssembly = "";
        public string cleanupShellCommand = "";
        public bool noConfig = false;
        //public bool suppressExternalHosting = true;
        public bool autoClass = false;
        public bool versionOnly = false;
        public string compilerOptions = "";
        public string altConfig = "";
        public Settings.HideOptions hideTemp = Settings.HideOptions.HideMostFiles;
        public uint doCleanupAfterNumberOfRuns = 20;

        public void AddSearchDir(string dir)
        {
#if net1
                foreach (string item in this.searchDirs)
                    if (item == dir)
                        return;
#else
            if (Array.Find(this.searchDirs, (x) => x == dir) != null)
                return;
#endif
            string[] newSearchDirs = new string[this.searchDirs.Length + 1];
            this.searchDirs.CopyTo(newSearchDirs, 0);
            newSearchDirs[newSearchDirs.Length - 1] = dir;
            this.searchDirs = newSearchDirs;
        }

        public string[] ExtractShellCommand(string command)
        {
            int pos = command.IndexOf("\"");
            string endToken = "\"";
            if (pos == -1 || pos != 0) //no quotation marks
                endToken = " ";

            pos = command.IndexOf(endToken, pos + 1);
            if (pos == -1)
                return new string[] { command };
            else
                return new string[] { command.Substring(0, pos).Replace("\"", ""), command.Substring(pos + 1).Trim() };
        }
    }

}