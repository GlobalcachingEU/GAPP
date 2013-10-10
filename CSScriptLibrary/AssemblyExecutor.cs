#region Licence...

//-----------------------------------------------------------------------------
// Date:	17/10/04	Time: 2:33p
// Module:	AssemblyExecutor.cs
// Classes:	AssemblyExecutor
//			RemoteExecutor
//
// This module contains the definition of the AssemblyExecutor class. Which implements
// executing 'public static void Main(..)' method of a assembly in a different AddDomain
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
using CSScriptLibrary;

namespace csscript
{
    /// <summary>
    /// Executes "public static void Main(..)" of assembly in a separate domain.
    /// </summary>
    class AssemblyExecutor
    {
        AppDomain appDomain;
        RemoteExecutor remoteExecutor;
        string assemblyFileName;

        public AssemblyExecutor(string fileNname, string domainName)
        {
            assemblyFileName = fileNname;
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = Path.GetDirectoryName(assemblyFileName);
            setup.PrivateBinPath = AppDomain.CurrentDomain.BaseDirectory;
            setup.ApplicationName = Utils.GetAssemblyFileName(Assembly.GetExecutingAssembly());
            setup.ShadowCopyFiles = "true";
            setup.ShadowCopyDirectories = Path.GetDirectoryName(assemblyFileName);
            appDomain = AppDomain.CreateDomain(domainName, null, setup);

            remoteExecutor = (RemoteExecutor)appDomain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(RemoteExecutor).ToString());
            remoteExecutor.searchDirs = ExecuteOptions.options.searchDirs;
        }

        public void Execute(string[] args)
        {
            remoteExecutor.ExecuteAssembly(assemblyFileName, args);
        }

        public void Unload()
        {
            AppDomain.Unload(appDomain);
            appDomain = null;
        }
    }

    /// <summary>
    /// Invokes static method 'Main' from the assembly.
    /// </summary>
    class RemoteExecutor : MarshalByRefObject
    {
        public string[] searchDirs = new string[0];

        public RemoteExecutor(string[] searchDirs)
        {
            this.searchDirs = searchDirs;
        }

        public RemoteExecutor()
        {
        }

        /// <summary>
        /// AppDomain event handler. This handler will be called if CLR cannot resolve
        /// referenced local assemblies
        /// </summary>
        public Assembly ResolveEventHandler(object sender, ResolveEventArgs args)
        {
            Assembly retval = null;
            foreach (string dir in searchDirs)
                if (null != (retval = AssemblyResolver.ResolveAssembly(args.Name, dir)))
                    break;
            return retval;
        }

        public Assembly ResolveResEventHandler(object sender, ResolveEventArgs args)
        {
            return Assembly.LoadFrom(this.asmFile);
        }

        string asmFile = "";

        public void ExecuteAssembly(string filename, string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolveEventHandler);
            AppDomain.CurrentDomain.ResourceResolve += new ResolveEventHandler(ResolveResEventHandler); //xaml

            asmFile = filename;

            Assembly assembly;
            if (!ExecuteOptions.options.inMemoryAsm)
            {
                assembly = Assembly.LoadFrom(filename);
            }
            else
            {
                //Load(byte[]) does not lock the assembly file as LoadFrom(filename) does
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    byte[] data = new byte[fs.Length];
                    fs.Read(data, 0, data.Length);
                    string dbg = Path.ChangeExtension(filename, ".pdb");
                    if (File.Exists(dbg))
                        using (FileStream fsDbg = new FileStream(dbg, FileMode.Open))
                        {
                            byte[] dbgData = new byte[fsDbg.Length];
                            fsDbg.Read(dbgData, 0, dbgData.Length);
                            assembly = Assembly.Load(data, dbgData);
                        }
                    else
                        assembly = Assembly.Load(data);
                }
            }
            InvokeStaticMain(assembly, args);
        }

        public void InvokeStaticMain(Assembly compiledAssembly, string[] scriptArgs) 
        {
            MethodInfo method = null;
            foreach (Module m in compiledAssembly.GetModules())
            {
                foreach (Type t in m.GetTypes())
                {
                    BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Static;
                    foreach (MemberInfo mi in t.GetMembers(bf))
                    {
                        if (mi.Name == "Main")
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
                throw new ApplicationException("Cannot find entry point. Make sure script file contains method: 'public static Main(...)'");
            }
        }
    }
}