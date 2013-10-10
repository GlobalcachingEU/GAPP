#region Licence...

//-----------------------------------------------------------------------------
// Date:	17/10/04	Time: 2:33p
// Module:	AssemblyResolver.cs
// Classes:	AssemblyResolver
//
// This module contains the definition of the AssemblyResolver class. Which implements
// some methods for simplified Assembly navigation
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
using System.Reflection;

#if net1
using System.Collections;
#else

using System.Collections.Generic;

#endif

using System.IO;
using csscript;

////////////////////////////////////////////////////
//
//  shfusion.dll is no longer supported by .NET 4.0
//
////////////////////////////////////////////////////
namespace CSScriptLibrary
{
    class MyClass
    {
        public MyClass()
        {
            
        }
        
    }

    /// <summary>
    /// Class for resolving assembly name to assembly file
    /// </summary>
    public class AssemblyResolver
    {
        #region Class public data...

        /// <summary>
        /// File to be excluded from assembly search
        /// </summary>
        static public string ignoreFileName = "";

        #endregion Class public data...

        #region Class public methods...

        static bool cacheProbingResults;

        /// <summary>
        /// Gets or sets a value indicating whether the assembly probing results should be cached. Default value is <c>false</c>;
        /// <para>
        /// Caching means that during the probing if the assembly is not found in one of the probing directories this directory will not
        /// be checked again if the same assembly is to be resolved in the future.
        /// </para>
        /// <para>
        /// This setting is to be used with the caution. While it can bring some performance benefits when the list of probing directories
        /// is large it also may be wrong to assume that if the assembly in not found in a particular directory it still will not be there if the probing is repeated.
        /// </para>
        /// </summary>
        /// <value><c>true</c> if probing results should be cached; otherwise, <c>false</c>.</value>
        public static bool CacheProbingResults
        {
            get
            {
                return cacheProbingResults;
            }
            set
            {
                cacheProbingResults = value;
                if (!value)
                    lock (NotFoundAssemblies)
                    {
                        NotFoundAssemblies.Clear();
                    }
            }
        }

#if net1
        private static readonly System.Collections.Hashtable NotFoundAssemblies = new System.Collections.Hashtable();
#else
        private static readonly HashSet<int> NotFoundAssemblies = new HashSet<int>();
#endif

        private static int BuildHashSetValue(string assemblyName, string directory)
        {
            return ((assemblyName ?? "") + (directory ?? "")).GetHashCode();
        }

        private static Assembly TryLoadAssemblyFrom(string assemblyName, string asmFile)
        {
            try
            {
                AssemblyName asmName = AssemblyName.GetAssemblyName(asmFile);
                if (asmName != null && asmName.FullName == assemblyName)
                    return Assembly.LoadFrom(asmFile);
                else if (assemblyName.IndexOf(",") == -1 && asmName.FullName.StartsWith(assemblyName)) //short name requested
                    return Assembly.LoadFrom(asmFile);
            }
            catch { }
            return null;
        }

        /// <summary>
        /// Resolves assembly name to assembly file. Loads assembly file to the current AppDomain.
        /// </summary>
        /// <param name="assemblyName">The name of assembly</param>
        /// <param name="dir">The name of directory where local assemblies are expected to be</param>
        /// <returns>loaded assembly</returns>
        static public Assembly ResolveAssembly(string assemblyName, string dir)
        {
            //if (assemblyName.StartsWith("System.Runtime.Serialization.resources"))
            //  return null;
            int hashSetValue = -1;

            if (CacheProbingResults)
            {
                hashSetValue = BuildHashSetValue(assemblyName, dir);

                lock (NotFoundAssemblies)
                {
                    if (NotFoundAssemblies.Contains(hashSetValue))
                        return null;
                }
            }

            try
            {
                if (Directory.Exists(dir))
                {
                    Assembly retval = null;
                    string[] asmFileNameTokens = assemblyName.Split(",".ToCharArray(), 5);

                    string asmFile = Path.Combine(dir, asmFileNameTokens[0]);
                    if (ignoreFileName != Path.GetFileName(asmFile) && File.Exists(asmFile))
                        if (null != (retval = TryLoadAssemblyFrom(assemblyName, asmFile)))
                            return retval;

                    //try file with name AssemblyDisplayName + .dll
                    asmFile = Path.Combine(dir, asmFileNameTokens[0]) + ".dll";

                    if (ignoreFileName != Path.GetFileName(asmFile) && File.Exists(asmFile))
                        if (null != (retval = TryLoadAssemblyFrom(assemblyName, asmFile)))
                            return retval;

                    //try file with extension
                    foreach (string file in Directory.GetFiles(dir, asmFileNameTokens[0] + "*"))
                        if (null != (retval = TryLoadAssemblyFrom(assemblyName, file)))
                            return retval;
                }
            }
            catch { }

            if (CacheProbingResults)
            {
                lock (NotFoundAssemblies)
                {
#if net1
                    NotFoundAssemblies.Add(hashSetValue, null);
#else
                    NotFoundAssemblies.Add(hashSetValue);
#endif
                }
            }
            return null;
        }

        static readonly char[] illegalChars = ":*?<>|\"".ToCharArray();

        /// <summary>
        /// Determines whether the string is a legal path token.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// 	<c>true</c> if the string is a legal path token; otherwise, <c>false</c>.
        /// </returns>
        static public bool IsLegalPathToken(string name)
        {
            return name.IndexOfAny(illegalChars) != -1;
        }

        /// <summary>
        /// Resolves namespace/assembly(file) name into array of assembly locations (local and GAC ones).
        /// </summary>
        /// <param name="name">'namespace'/assembly(file) name</param>
        /// <param name="searchDirs">Assembly search directories</param>
        /// <returns>collection of assembly file names where namespace is implemented</returns>
        static public string[] FindAssembly(string name, string[] searchDirs)
        {
#if net1
            ArrayList retval = new ArrayList();
#else
            List<string> retval = new List<string>();
#endif
            if (!IsLegalPathToken(name))
            {
                foreach (string dir in searchDirs)
                {
                    foreach (string asmLocation in FindLocalAssembly(name, dir))	//local assemblies alternative locations
                        retval.Add(asmLocation);

                    if (retval.Count != 0)
                        break;
                }

                if (retval.Count == 0)
                {
                    string nameSpace = Utils.RemoveAssemblyExtension(name);
                    foreach (string asmGACLocation in FindGlobalAssembly(nameSpace))
                    {
                        retval.Add(asmGACLocation);
                    }
                }
            }
#if net1
            return (string[])retval.ToArray(typeof(string));
#else
            return retval.ToArray();
#endif
        }
        
        /// <summary>
        /// Resolves namespace into array of local assembly locations.
        /// (Currently it returns only one assembly location but in future
        /// it can be extended to collect all assemblies with the same namespace)
        /// </summary>
        /// <param name="name">namespace/assembly name</param>
        /// <param name="dir">directory</param>
        /// <returns>collection of assembly file names where namespace is implemented</returns>
        static public string[] FindLocalAssembly(string name, string dir)
        {
            //We are returning and array because name may represent assembly name or namespace
            //and as such can consist of more than one assembly file (multiple assembly file is not supported at this stage).
            try
            {
                string asmFile = Path.Combine(dir, name);

                //cannot just check Directory.Exists(dir) as "name" can contain sum subDir parts
                if (Directory.Exists(Path.GetDirectoryName(asmFile)))
                {
                    //test well-known assembly extensions first
                    foreach (string ext in new string[] { "", ".dll", ".exe", ".compiled" })
                    {
                        string file = asmFile + ext; //just in case if user did not specify the extension
                        if (ignoreFileName != Path.GetFileName(file) && File.Exists(file))
                            return new string[] { file };
                    }

                    if (asmFile != Path.GetFileName(asmFile) && File.Exists(asmFile))
                        return new string[] { asmFile };
                }
            }
            catch { } //name may not be a valid path name
            return new string[0];
        }

        /// <summary>
        /// Resolves namespace into array of global assembly (GAC) locations.
        /// </summary>
        /// <param name="namespaceStr">'namespace' name</param>
        /// <returns>collection of assembly file names where namespace is implemented</returns>
        static public string[] FindGlobalAssembly(String namespaceStr)
        {
#if net1
            ArrayList retval = new ArrayList();
#else
            List<string> retval = new List<string>();
#endif
            try
            {
                AssemblyEnum asmEnum = new csscript.AssemblyEnum(namespaceStr);

                string highestVersion = "";
                string asmName = "";
                do
                {
                    asmName = asmEnum.GetNextAssembly();
                    if (string.Compare(asmName, highestVersion) > 0)
                        highestVersion = asmName;
                }
                while (asmName != null);

                if (highestVersion != "")
                {
                    string asmLocation = AssemblyCache.QueryAssemblyInfo(highestVersion);
                    retval.Add(asmLocation);
                }
            }
            catch
            {
                //If exception is thrown it is very likely it is because where fusion.dll does not exist/unavailable/broken.
                //We might be running under the MONO run-time.
            }

#if net1
            if (retval.Count == 0 && namespaceStr.ToLower().EndsWith(".dll"))
                retval.Add(namespaceStr); //in case of if the namespaceStr is a dll name

            return (string[])retval.ToArray(typeof(string));
#else
            if (retval.Count == 0 && namespaceStr.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
                retval.Add(namespaceStr); //in case of if the namespaceStr is a dll name

            return retval.ToArray();
#endif
        }

        #endregion Class public methods...

        /// <summary>
        /// Search for namespace into local assembly file.
        /// </summary>
        static public bool IsNamespaceDefinedInAssembly(string asmFileName, string namespaceStr)
        {
            if (File.Exists(asmFileName))
            {
                try
                {
                    //non reflection base assembly inspection can be found here: http://ccimetadata.codeplex.com/
                    //also there are some indications that Reflector uses ILReader without reflection: http://blogs.msdn.com/haibo_luo/default.aspx?p=3
                    //Potential solutions: AsmReader in this file or Assembly.Load(byte[]);
#if net1
                    Assembly assembly = Assembly.LoadFrom(asmFileName);
#else
                    Assembly assembly = Assembly.ReflectionOnlyLoadFrom(asmFileName);
#endif
                    if (assembly != null)
                    {
                        foreach (Module m in assembly.GetModules())
                        {
                            foreach (Type t in m.GetTypes())
                            {
                                if (namespaceStr == t.Namespace)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                catch { }
            }
            return false;
        }

        private bool ProbeAssembly(string file)
        {
            try
            {
#if net1
                Assembly.LoadFrom(Path.GetFullPath(file));
#else
                Assembly.ReflectionOnlyLoadFrom(Path.GetFullPath(file));
#endif
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //public class AsmReader : MarshalByRefObject
        //{
        //    //Allows working with assembly loaded in a separate AppDomain
        //    //After processing the assembly and its domain automatically unloaded

        //    static public object Read(string file, Func<Assembly, object> routine)
        //    {
        //        var appDomain = AppDomain.CreateDomain("", null, new AppDomainSetup());

        //        object obj = appDomain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().Location, typeof(AsmReader).ToString());

        //        var retval = (obj as AsmReader).ReadInternal(file, routine);

        //        AppDomain.Unload(appDomain);

        //        return retval;
        //    }

        //    object ReadInternal(string file, Func<Assembly, object> routine)
        //    {
        //        return routine(Assembly.LoadFrom(file));
        //    }
        //}
    }
}