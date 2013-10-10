using System.IO;
using System.Text;
using System.Collections.Generic;
using System;
using System.Collections;

namespace csscript
{
    /// <summary>
    /// This class is a container for information related to the script precompilation.
    /// <para>It is used to pass an input information from the script engine to the <c>precompiler</c> instance as well as to pass back
    /// to the script engine some output information (e.g. added referenced assemblies)</para> .
    /// </summary>
    internal class PrecompilationContext
    {
        ///// <summary>
        ///// Full path of the script being passed for precompilation.
        ///// </summary>
        //public string ScriptFileName;
        ///// <summary>
        ///// Flag, which indicates if the script passed for precompilation is an entry script (primary script).
        ///// <para>This field can be used to determine the precompilation algorithm based on the entry script. For example
        ///// generating the <c>static Main()</c> wrapper for classless scripts should be done only for an entry script but not for other included/imported script. </para>
        ///// </summary>
        //public bool IsPrimaryScript = true;
        /// <summary>
        /// Collection of the referenced assemblies to be added to the process script referenced assemblies.
        /// <para>You may want to add new items to the referenced assemblies because of the precompilation logic (e.g. some code using assemblies not referenced by the primary script 
        /// is injected in the script).</para>
        /// </summary>
        public List<string> NewReferences = new List<string>();
        /// <summary>
        /// Collection of the new dependency items (files). 
        /// <para>Dependency files are checked before the script execution in order to understand if the script should be recompiled or it can be loaded from 
        /// the cache. If any of the dependency files is changed since the last script execution the script engine will recompile the script. In the simple execution 
        /// scenarios the script file is a dependency file.</para>
        /// </summary>
        public List<string> NewDependencies = new List<string>();
        /// <summary>
        /// Collection of the assembly and script probing directories to be added to the process search directories.
        /// <para>You may want to add new items to the process search directories because of the precompilation logic.</para>
        /// </summary>
        public List<string> NewSearchDirs = new List<string>();
        /// <summary>
        /// Collection of the process assembly and script probing directories.
        /// </summary>
        public string[] SearchDirs = new string[0];
    }

    ///// <summary>
    ///// The interface that all CS-Script precompilers need to implement.
    ///// <para>
    ///// The following is an example of the precompiler for sanitizing the script containing hashbang string on Linux.
    ///// </para>
    ///// <code>
    /////  public class HashBangPrecompiler : IPrecompiler
    /////  {
    /////     public bool Compile(ref string code, PrecompilationContext context)
    /////     {
    /////         if (code.StartsWith("#!"))
    /////         {
    /////              code = "//" + code; //comment the Linux hashbang line to keep C# compiler happy
    /////              return true;
    /////         }
    /////              
    /////         return false;
    /////     }
    ///// }
    ///// </code>
    ///// </summary>
    //public interface IPrecompiler
    //{
    //    /// <summary>
    //    /// Compiles/modifies the specified code.
    //    /// <para>
    //    /// </para>
    //    /// </summary>
    //    /// <param name="code">The code to be compiled.</param>
    //    /// <param name="context">The context.</param>
    //    /// <returns><c>True</c> if the <paramref name="code"/> code has been modified and <c>False</c> </returns>
    //    bool Compile(ref string code, PrecompilationContext context);
    //}

    internal class DefaultPrecompiler
    {
        public static bool Compile(ref string code, string scriptFile, bool IsPrimaryScript, Hashtable context)
        {
            if (code.StartsWith("#!"))
            {
                code = "//" + code; //comment the Linux hashbang line to keep C# compiler happy
                return true;
            }

            return false;
        }
    }

    internal class AutoclassPrecompiler// : IPrecompiler
    {
        //static string FileToClassName(string text)
        //{
        //    return Path.GetFileNameWithoutExtension(text).Replace("_", ""); //double '_' are not allowed for class names
        //}

        public static bool Compile(ref string content, string scriptFile, bool IsPrimaryScript, Hashtable context)
        {
            if (!IsPrimaryScript)
                return false;

            StringBuilder code = new StringBuilder(4096);
            code.Append("//Auto-generated file"+Environment.NewLine); //cannot use AppendLine as it is not available in StringBuilder v1.1
            //code.Append("using System;\r\n");

            bool headerProcessed = false;
            string line;
            using (StringReader sr = new StringReader(content))
                while ((line = sr.ReadLine()) != null)
                {
                    if (!headerProcessed && !line.TrimStart().StartsWith("using ")) //not using...; statement of the file header
                        if (!line.StartsWith("//") && line.Trim() != "") //not comments or empty line
                        {
                            headerProcessed = true;
                            //code.Append("namespace Scripting\r\n");
                            //code.Append("{\r\n");
                            
                            code.Append("   public class ScriptClass" + Environment.NewLine);
                            code.Append("   {" + Environment.NewLine);
                            code.Append("   static public ");
                        }
                        
                    code.Append(line);
                    code.Append(Environment.NewLine);
                }

            code.Append("   }" + Environment.NewLine);

            content = code.ToString();
            return true;
        }
    }
}