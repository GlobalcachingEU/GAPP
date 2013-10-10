#region Licence...

//-----------------------------------------------------------------------------
// Date:	30/10/10	Time: 8:21
// Module:	ScriptLauncherBuilder.cs
// Classes:	ScriptLauncherBuilder
//			
// This module contains the definition of the ScriptLauncherBuilder class. Which implements
// compiling light-weigh host application for the script execution.
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
    internal class ScriptLauncherBuilder
    {
        public static string GetLauncherName(string assembly)
        {
            return assembly + ".host.exe";
        }

        // UnitTest
        // Make Surrogate scenario to compile conditionally 
        // + check and delete the exe before building
        // + set Appartment state
        // + update all ExecutionClients incliding csslib
        // + when starting remove css and //x args 
        //+ try to solve limitations with console Input redurectionlimi
        //+ ensure launcher is not build when building dll/exe without execution
        public string BuildSurrogateLauncher(string scriptAssembly, string tragetFramework, CompilerParameters compilerParams, ApartmentState appartmentState)
        {
            //string 
#if !net4
            throw new ApplicationException("Cannot build surrogate host application because this script engine is build against early version of CLR.");
#else
            var provider = CodeDomProvider.CreateProvider("C#", new Dictionary<string, string> { { "CompilerVersion", tragetFramework } });

            compilerParams.OutputAssembly = GetLauncherName(scriptAssembly);
            compilerParams.GenerateExecutable = true;
            compilerParams.GenerateInMemory = false;
            compilerParams.IncludeDebugInformation = false;


            try
            {
                if (File.Exists(compilerParams.OutputAssembly))
                    File.Delete(compilerParams.OutputAssembly);
            }
            catch (Exception e)
            {
                throw new ApplicationException("Cannot build surrogate host application", e);
            }

            if (compilerParams.CompilerOptions != null)
                compilerParams.CompilerOptions = compilerParams.CompilerOptions.Replace("/d:TRACE", "")
                                                                               .Replace("/d:DEBUG", "");

            if (!AppInfo.appConsole)
                compilerParams.CompilerOptions += " /target:winexe";

            string refAssemblies = "";
            string appartment    = "[STAThread]";
            if (appartmentState == ApartmentState.MTA)
                appartment = "[" + appartmentState + "Thread]";
            else if (appartmentState == ApartmentState.Unknown)
                appartment = "";

            foreach (string asm in compilerParams.ReferencedAssemblies)
                if (File.Exists(asm)) //ignore GAC (not full path) assemblies 
                    refAssemblies += Assembly.ReflectionOnlyLoadFrom(asm).FullName + ":" + asm + ";";

            string code = launcherCode
                                .Replace("${REF_ASSEMBLIES}", refAssemblies)
                                .Replace("${APPARTMENT}", appartment)
                                .Replace("${ASM_MANE}", Path.GetFileName(scriptAssembly));

            CompilerResults retval;

            bool debugLauncher = false;
            if (debugLauncher)
            {
                compilerParams.IncludeDebugInformation = true;
                compilerParams.CompilerOptions += " /d:DEBUG";
                //string launcherFile = @"C:\Users\OSH\Desktop\New folder (2)\script.launcher.cs";
                string launcherFile = Path.GetTempFileName();
                File.WriteAllText(launcherFile, code);
                retval = provider.CompileAssemblyFromFile(compilerParams, launcherFile);
            }
            else
                retval = provider.CompileAssemblyFromSource(compilerParams, code);


            if (retval.Errors.Count != 0)
                throw CompilerException.Create(retval.Errors, true);

            CSSUtils.SetTimestamp(compilerParams.OutputAssembly, scriptAssembly);
            return compilerParams.OutputAssembly;
#endif
        }
        const string launcherCode =
@"using System;
using System.Collections;
using System.IO;
using System.Reflection;

class Script
{
    ${APPARTMENT}
    static public int Main(string[] args)
    {
        try
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            MainImpl(args);
        }
        catch(Exception e)
        {
            Console.WriteLine(e.ToString());
            return 1;
        }
        return Environment.ExitCode;
    }
   
    static public void MainImpl(string[] args)
    {
        System.Diagnostics.Debug.Assert(false);

        string scriptAssembly = """";
        bool debug = false;

        ArrayList newArgs = new ArrayList();
        foreach (string arg in args)
            if (arg.StartsWith(""/css_host_dbg:""))
                debug = (arg == ""/css_host_dbg:true"");
            else if (arg.StartsWith(""/css_host_asm:""))
                scriptAssembly = arg.Substring(""/css_host_asm:"".Length);
            else
                newArgs.Add(arg);

        if (debug)
        {
            System.Diagnostics.Debugger.Launch();
            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
        }
        
        if (scriptAssembly == """")
        {
            scriptAssembly = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ""${ASM_MANE}"");
        }
        InvokeStaticMain(Assembly.LoadFrom(scriptAssembly), (string[])newArgs.ToArray(typeof(string)));
    }

    static void InvokeStaticMain(Assembly compiledAssembly, string[] scriptArgs)
    {
        MethodInfo method = null;
        foreach (Module m in compiledAssembly.GetModules())
        {
            foreach (Type t in m.GetTypes())
            {
                BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Static;
                foreach (MemberInfo mi in t.GetMembers(bf))
                {
                    if (mi.Name == ""Main"")
                    {
                        method = t.GetMethod(mi.Name, bf);
                    }
                    if (method != null)
                        break;
                }
                if (method != null)
                    break;
            }
            if (method != null)
                break;
        }
        if (method != null)
        {
            object retval = null;
            if (method.GetParameters().Length != 0)
                retval = method.Invoke(new object(), new object[] { (Object)scriptArgs });
            else
                retval = method.Invoke(new object(), null);

            if (retval != null)
            {
                try
                {
                    Environment.ExitCode = int.Parse(retval.ToString());
                }
                catch { }
            }
        }
        else
        {
            throw new ApplicationException(""Cannot find entry point. Make sure script file contains method: 'public static Main(...)'"");
        }
    }

    static string refAssemblies = @""${REF_ASSEMBLIES}"";
    static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        if (refAssemblies != """")
        {
            foreach (string asm in refAssemblies.Split(';'))
                if (asm.StartsWith(args.Name))
                    return Assembly.LoadFrom(asm.Substring(args.Name.Length + 1));
        }
        return null;
    }
}";
    }
}