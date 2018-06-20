#region Licence...

//-----------------------------------------------------------------------------
// Date:	10/9/05	Time: 3:00p
// Module:	AsmHelper.cs
// Classes:	AsmHelper
//
// This module contains the definition of the AsmHelper class. Which implements
// dynamic assembly loading/unloading and invoking methods from loaded assembly.
//
// Written by Oleg Shilo (oshilo@gmail.com)
// Copyright (c) 2005-2012. All rights reserved.
//
// Redistribution and use of this code in source and binary forms, without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright notice,
//	this list of conditions and the following disclaimer.
// 2. Neither the name of an author nor the names of the contributors may be used
//	to endorse or promote products derived from this software without specific
//	prior written permission.
// 3. This code may be used in compiled form in any way you desire. This
//	  file may be redistributed unmodified by any means PROVIDING it is
//	not sold for profit without the authors written consent, and
//	providing that this notice and the authors name is included.
//
// Redistribution and use of this code in source and binary forms, with modification,
// are permitted provided that all above conditions are met and software is not used
// or sold for profit.
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
using System.Reflection;
using System.Collections;

#if !net1

using System.Collections.Generic;
using System.Linq;

#endif

using System.Reflection.Emit;
using CSScriptLibrary;
using System.Diagnostics;
using csscript;

#if !net1

/// <summary>
/// Method extensions for
/// </summary>
public static class CSScriptLibraryExtensionMethods
{
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
    static public string GetScriptName(this Assembly assembly)
    {
        return CSScript.GetScriptName(assembly);
    }

    /// <summary>
    /// Constructs and returns an instance of CSScriptLibrary.AsmHelper class from the underlying Assembly.
    /// </summary>
    /// <returns>CSScriptLibrary.AsmHelper</returns>
    /// <param name="obj">Instance of the type to be extended</param>
    /// <returns>CSScriptLibrary.AsmHelper</returns>
    public static CSScriptLibrary.AsmHelper GetHelper(this Assembly obj)
    {
        return new CSScriptLibrary.AsmHelper(obj);
    }

    /// <summary>
    /// Returns which emitted delegate based on MethodInfo of the underlying assembly.
    /// </summary>
    /// <param name="obj">Instance of the type to be extended</param>
    /// <param name="methodName">'Method' name including 'Type' name (eg. MyType.DoJob). It is allowd to use wild
    /// card character to indicate that the Type name of the method is irrelevant (e.g. "*.Method" or even *.*).</param>
    /// <param name="list">List of 'Method' arguments.
    /// Note that values of the items in the list do not have any importance. The type of the list item is
    /// to be used for method search. For example if class Calc has method Sum(int a, int b) the method invoker
    /// can be obtained as following:
    /// <para>
    /// GetStaticMethod("Calc.Sum", 0, 0)
    /// </para>
    /// You can pass any integer as the second and third parameter because it will be used only to obtain the
    /// information about the paramater type (in this case System.Int32).</param>
    /// <returns>Returns delegate of CSScriptLibrary.MethodDelegate type.</returns>
    public static CSScriptLibrary.MethodDelegate GetStaticMethod(this Assembly obj, string methodName, params object[] list)
    {
        return new CSScriptLibrary.AsmHelper(obj).GetStaticMethod(methodName, list);
    }

    /// <summary>
    /// Returns which emitted delegate based on MethodInfo of the underlying assembly.
    /// </summary>
    /// <param name="obj">Instance of the type to be extended</param>
    /// <param name="methodName">'Method' name including 'Type' name (eg. MyType.DoJob). It is allowd to use wild
    /// card character to indicate that the Type name of the method is irrelevant (e.g. "*.Method" or even *.*).</param>
    /// <param name="list">List of 'Method' arguments.</param>
    /// <returns>Returns delegate of CSScriptLibrary.MethodDelegate type.</returns>
    public static CSScriptLibrary.MethodDelegate GetStaticMethodWithArgs(this Assembly obj, string methodName, params Type[] list)
    {
        return new CSScriptLibrary.AsmHelper(obj).GetStaticMethodWithArgs(methodName, list);
    }

    /// <summary>
    /// <param name="obj">Instance of the type to be extended</param>
    /// Specialised version of GetMethodInvoker which returns MethodDelegate of the very first method found in the
    /// underlying assembly. This method is an overloaded implementation of the GetStaticMethod(string methodName, params object[] list).
    ///
    /// Use this method when script assembly contains only one single type with one method.
    /// </summary>
    /// <returns>Returns delegate of CSScriptLibrary.MethodDelegate type.</returns>
    public static CSScriptLibrary.MethodDelegate GetStaticMethod(this Assembly obj)
    {
        return new CSScriptLibrary.AsmHelper(obj).GetStaticMethod();
    }

    /// <summary>
    /// Attempts to create instance of a class from underlying assembly.
    /// </summary>
    /// <param name="obj">Instance of the type to be extended</param>
    /// <param name="typeName">The 'Type' full name of the type to create. (see Assembly.CreateInstance()).
    ///
    /// You can use wild card meaning the first type found. However only full wild card "*" is supported.</param>
    /// <returns>Instance of the 'Type'. Returns null if the instance cannot be created.</returns>
    public static Object TryCreateObject(this Assembly obj, string typeName)
    {
        return new CSScriptLibrary.AsmHelper(obj).TryCreateObject(typeName);
    }

    /// <summary>
    /// Creates instance of a class from underlying assembly.
    /// </summary>
    /// <param name="obj">Instance of the type to be extended</param>
    /// <param name="typeName">The 'Type' full name of the type to create. (see Assembly.CreateInstance()).
    ///
    /// You can use wild card meaning the first type found. However only full wild card "*" is supported.</param>
    /// <returns>Instance of the 'Type'. Throws an ApplicationException if the instance cannot be created.</returns>
    public static Object CreateObject(this Assembly obj, string typeName)
    {
        return new CSScriptLibrary.AsmHelper(obj).CreateObject(typeName);
    }

#if !net1

    /// <summary>
    /// Attempts to align (pseudo typecast) object to interface.
    /// <para>The object does not necessarily need to implement the interface formally.</para>
    /// <para>See <see cref="T:CSScriptLibrary.ThirdpartyLibraries.Rubenhak.Utils.ObjectCaster"/>.</para>
    /// </summary>
    /// <typeparam name="T">Interface definition to align with.</typeparam>
    /// <param name="obj">The object to be aligned with the interface.</param>
    /// <returns>Interface object or <c>null</c> if alignment was unsuccessful.</returns>
    public static T TryAlignToInterface<T>(this object obj) where T : class
    {
        return CSScriptLibrary.ThirdpartyLibraries.Rubenhak.Utils.ObjectCaster<T>.As(obj);
    }

    /// <summary>
    /// Attempts to align (pseudo typecast) object to interface.
    /// <para>The object does not necessarily need to implement the interface formally.</para>
    /// <para>See <see cref="T:CSScriptLibrary.ThirdpartyLibraries.Rubenhak.Utils.ObjectCaster"/>.</para>
    /// </summary>
    /// <typeparam name="T">Interface definition to align with.</typeparam>
    /// <param name="obj">The object to be aligned with the interface.</param>
    /// <param name="useAppDomainAssemblies">If set to <c>true</c> uses all loaded assemblies of the current <see cref="T:System.AppDomain"/>
    /// when emitting (compiling) aligned proxy object.</param>
    /// <returns>Interface object or <c>null</c> if alignment was unsuccessful.</returns>
    public static T TryAlignToInterface<T>(this object obj, bool useAppDomainAssemblies) where T : class
    {
        string[] refAssemblies;
        if (useAppDomainAssemblies)
            refAssemblies = CSSUtils.GetAppDomainAssemblies();
        else
            refAssemblies = new string[0];

        return CSScriptLibrary.ThirdpartyLibraries.Rubenhak.Utils.ObjectCaster<T>.As(obj, refAssemblies);
    }

    /// <summary>
    /// Attempts to align (pseudo typecast) object to interface.
    /// <para>The object does not necessarily need to implement the interface formally.</para>
    /// <para>See <see cref="T:CSScriptLibrary.ThirdpartyLibraries.Rubenhak.Utils.ObjectCaster"/>.</para>
    /// </summary>
    /// <typeparam name="T">Interface definition to align with.</typeparam>
    /// <param name="obj">The object to be aligned with the interface.</param>
    /// <param name="refAssemblies">The string array containing file nemes to the additional dependency
    /// assemblies the interface depends in. </param>
    /// <returns>Interface object or <c>null</c> if alignment was unsuccessful.</returns>
    public static T TryAlignToInterface<T>(this object obj, params string[] refAssemblies) where T : class
    {
        return CSScriptLibrary.ThirdpartyLibraries.Rubenhak.Utils.ObjectCaster<T>.As(obj, refAssemblies);
    }

    /// <summary>
    /// Aligns (pseudo typecasts) object to interface.
    /// <para>The object does not necessarily need to implement the interface formally.</para>
    /// <para>See <see cref="T:CSScriptLibrary.ThirdpartyLibraries.Rubenhak.Utils.ObjectCaster"/>.</para>
    /// </summary>
    /// <typeparam name="T">Interface definition to align with.</typeparam>
    /// <param name="obj">The object to be aligned with the interface.</param>
    /// <param name="useAppDomainAssemblies">If set to <c>true</c> uses all loaded assemblies of the current <see cref="T:System.AppDomain"/>
    /// when emitting (compiling) aligned proxy object.</param>
    /// <returns>Interface object.</returns>
    public static T AlignToInterface<T>(this object obj, bool useAppDomainAssemblies) where T : class
    {
        var retval = obj.TryAlignToInterface<T>(useAppDomainAssemblies);

        if (retval == null)
            throw new ApplicationException("The object (" + obj + ") cannot be aligned to " + typeof(T) + " interface.");

        return retval;
    }

    /// <summary>
    /// Aligns (pseudo typecasts) object to interface.
    /// <para>The object does not necessarily need to implement the interface formally.</para>
    /// <para>See <see cref="T:CSScriptLibrary.ThirdpartyLibraries.Rubenhak.Utils.ObjectCaster"/>.</para>
    /// </summary>
    /// <typeparam name="T">Interface definition to align with.</typeparam>
    /// <param name="obj">The object to be aligned with the interface.</param>
    /// <param name="refAssemblies">The string array containing file nemes to the additional dependency
    /// assemblies the interface depends in. </param>
    /// <returns>Interface object.</returns>
    public static T AlignToInterface<T>(this object obj, params string[] refAssemblies) where T : class
    {
        var retval = obj.TryAlignToInterface<T>(refAssemblies);

        if (retval == null)
            throw new ApplicationException("The object (" + obj + ") cannot be aligned to " + typeof(T) + " interface.");

        return retval;
    }

    /// <summary>
    /// Aligns (pseudo typecasts) object to interface.
    /// <para>The object does not necessarily need to implement the interface formally.</para>
    /// <para>See <see cref="T:CSScriptLibrary.ThirdpartyLibraries.Rubenhak.Utils.ObjectCaster"/>.</para>
    /// </summary>
    /// <typeparam name="T">Interface definition to align with.</typeparam>
    /// <param name="obj">The object to be aligned with the interface.</param>
    /// <returns>Interface object.</returns>
    public static T AlignToInterface<T>(this object obj) where T : class
    {
        var retval = obj.TryAlignToInterface<T>();

        if (retval == null)
            throw new ApplicationException("The object (" + obj + ") cannot be aligned to " + typeof(T) + " interface.");

        return retval;
    }

#endif
}

#endif

namespace CSScriptLibrary
{
    /// <summary>
    /// Delegate which is used as a return type for AsmHelper.GetMethodInvoker().
    ///
    /// AsmHelper.GetMethodInvoker() allows obtaining dynamic method delegate emitted on the base of the MethodInfo (from the compiled script type).
    /// </summary>
    /// <param name="instance">Instance of the type which method is to be invoked.</param>
    /// <param name="paramters">Optional method parameters.</param>
    /// <returns>Returns MethodInfo return value</returns>
    public delegate object FastInvokeDelegate(object instance, params object[] paramters);

    /// <summary>
    /// Delegate which is used as a return type for AsmHelper.GetMethodInvoker().
    ///
    /// AsmHelper.GetStaticMethod() and AsmHelper.GetMethod() allow obtaining dynamic method delegate emitted on the base of the MethodInfo (from the compiled script type).
    /// </summary>
    /// <param name="paramters">Optional method parameters.</param>
    /// <returns>Returns MethodInfo return value</returns>
    public delegate object MethodDelegate(params object[] paramters);

    /// <summary>
    /// Helper class to simplify working with dynamically loaded assemblies.
    /// </summary>
    public class AsmHelper : IDisposable
    {
        IAsmBrowser asmBrowser;
        AppDomain remoteAppDomain;

        bool deleteOnExit = false;

#if !net1

        /// <summary>
        /// Aligns (pseudo typecasts) object to the specified interface.
        /// <para>The object does not necessarily need to implement the interface formally.</para>
        /// <para>See <see cref="T:CSScriptLibrary.ThirdpartyLibraries.Rubenhak.Utils.ObjectCaster"/>.</para>
        /// <remarks>
        /// The important difference between this method being called from <see cref="AsmHelper"/> working
        /// with the assembly in current and remote <see cref="AppDomain"/> is that is that the actual
        /// interface alignment is performed in the corresponding <see cref="AppDomain"/>.
        /// </remarks>
        /// </summary>
        /// <typeparam name="T">Interface definition to align with.</typeparam>
        /// <param name="obj">The object to be aligned with the interface.</param>
        /// <returns>Interface object.</returns>
        public T AlignToInterface<T>(object obj) where T : class
        {
            return this.asmBrowser.AlignToInterface<T>(obj);
        }

        /// <summary>
        /// Aligns (pseudo typecasts) object to the specified interface.
        /// <para>The object does not necessarily need to implement the interface formally.</para>
        /// <para>See <see cref="T:CSScriptLibrary.ThirdpartyLibraries.Rubenhak.Utils.ObjectCaster"/>.</para>
        /// <remarks>
        /// The important difference between this method being called from <see cref="AsmHelper"/> working
        /// with the assembly in current and remote <see cref="AppDomain"/> is that is that the actual
        /// interface allignment is performed in the corresponding <see cref="AppDomain"/>.
        /// </remarks>
        /// </summary>
        /// <typeparam name="T">Interface definition to align with.</typeparam>
        /// <param name="obj">The object to be aligned with the interface.</param>
        /// <param name="refAssemblies">The string array containing file nemes to the additional dependency
        /// assemblies the interface depends in. </param>
        /// <returns>Interface object.</returns>
        public T AlignToInterface<T>(object obj, params string[] refAssemblies) where T : class
        {
            return this.asmBrowser.AlignToInterface<T>(obj, refAssemblies);
        }

        /// <summary>
        /// Aligns (pseudo typecasts) object to the specified interface.
        /// <para>The object does not necessarily need to implement the interface formally.</para>
        /// <para>See <see cref="T:CSScriptLibrary.ThirdpartyLibraries.Rubenhak.Utils.ObjectCaster"/>.</para>
        /// <remarks>
        /// The important difference between this method being called from <see cref="AsmHelper"/> working
        /// with the assembly in current and remote <see cref="AppDomain"/> is that is that the actual
        /// interface allignment is performed in the corresponding <see cref="AppDomain"/>.
        /// </remarks>
        /// </summary>
        /// <typeparam name="T">Interface definition to align with.</typeparam>
        /// <param name="obj">The object to be aligned with the interface.</param>
        /// <param name="useAppDomainAssemblies">If set to <c>true</c> uses all loaded assemblies of the current <see cref="T:System.AppDomain"/></param>
        /// <returns>Interface object.</returns>
        public T AlignToInterface<T>(object obj, bool useAppDomainAssemblies) where T : class
        {
            return this.asmBrowser.AlignToInterface<T>(obj, useAppDomainAssemblies);
        }

        /// <summary>
        /// Creates object in remote or current <see cref="AppDomain"/> and aligns (pseudo typecasts) it to the specified interface.
        /// <para>Semantecally it is an equivalent of calling
        /// <code>asmHelper.AlignToInterface(asmHelper.CreateObject(typeName))</code>
        /// </para>
        /// </summary>
        /// <typeparam name="T">Interface definition to align with.</typeparam>
        /// <param name="typeName">The 'Type' full name of the type to create. (see Assembly.CreateInstance()).
        /// You can use wild card meaning the first type found. However only full wild card "*" is supported.</param>
        /// <returns>Interface object.</returns>
        public T CreateAndAlignToInterface<T>(string typeName) where T : class
        {
            return this.asmBrowser.AlignToInterface<T>(this.CreateObject(typeName));
        }

#endif

        /// <summary>
        /// Instance of the AppDomain, which is used to execute the script.
        /// </summary>
        public AppDomain ScriptExecutionDomain
        {
            get { return remoteAppDomain != null ? remoteAppDomain : AppDomain.CurrentDomain; }
        }

        /// <summary>
        /// Flag that indicates if method caching is enabled. It is set to true by default.
        /// <para></para>
        /// When caching is enabled AsmHelper generates (emits) extremely fast delegates for
        /// the methods being invoked. If AsmHelper is in cache mode it performs more than twice faster.
        /// However generation of the delegate does take some time that is why you may consider
        /// switching caching off if the method is to be invoked only once.
        /// </summary>
        public bool CachingEnabled
        {
            get { return this.asmBrowser.CachingEnabled; }
            set { this.asmBrowser.CachingEnabled = value; }
        }

#if !net1

        /// <summary>
        /// This method returns extremely fast delegate for the method specified by "methodName" and
        /// method arguments "list". Ivoking such delegate is ~100 times faster than invoking with pure reflection
        /// (MethodInfo.Invoke()).
        /// </summary>
        /// <param name="methodName">'Method' name including 'Type' name (eg. MyType.DoJob). It is allowd to use wild
        /// card character to indicate that the Type name of the method is irrelevant (e.g. "*.Method" or "*.*").</param>
        /// <param name="list">List of 'Method' arguments.
        /// Note that values of the items in the list do not have any importance. The type of the list item is
        /// to be used for method search. For example if class Calc has method Sum(int a, int b) the method invoker
        /// can be obtained as following:
        /// <para></para>
        /// GetMethodInvoker("Calc.Sum", 0, 0)
        /// <para></para>
        /// You can pass any integer as the second and third parameter because it will be used only to obtain the
        /// information about the paramater type (in this case System.Int32).</param>
        /// <returns>Returns delegate of CSScriptLibrary.FastInvokeDelegate type.</returns>
        public FastInvokeDelegate GetMethodInvoker(string methodName, params object[] list)
        {
            return this.asmBrowser.GetMethodInvoker(methodName, list);
        }

        /// <summary>
        /// Specialised version of GetMethodInvoker which returns MethodDelegate thus you do not need to specify
        /// object instance (null) when calling static methods.
        /// </summary>
        /// <param name="methodName">'Method' name including 'Type' name (eg. MyType.DoJob). It is allowd to use wild
        /// card character to indicate that the Type name of the method is irrelevant (e.g. "*.Method" or "*.*").</param>
        /// <param name="list">List of 'Method' arguments. </param>
        /// <returns>Returns delegate of CSScriptLibrary.MethodDelegate type.</returns>
        /// <remarks>
        /// <para>
        /// <para>
        /// Note that values of the items in the list do not have any importance. The type of the list item is
        /// to be used for method search. For example if class Calc has method Sum(int a, int b) the method invoker
        /// can be obtained as following:
        /// </para>
        /// </para>
        /// <example>
        /// <code>GetStaticMethod("Calc.Sum", 0, 0)</code>
        /// </example>
        /// <para>
        /// You can pass any integer as the second and third parameter because it will be used only to obtain the
        /// information about the paramater type (in this case System.Int32).
        /// </para>
        /// </remarks>
        public MethodDelegate GetStaticMethod(string methodName, params object[] list)
        {
            FastInvokeDelegate method = this.asmBrowser.GetMethodInvoker(methodName, list);
            return delegate(object[] paramters) { return method(null, paramters); };
        }

        /// <summary>
        /// Specialised version of GetMethodInvoker which returns MethodDelegate thus you do not need to specify
        /// object instance (null) when calling static methods.
        /// </summary>
        /// <param name="methodName">'Method' name including 'Type' name (eg. MyType.DoJob). It is allowd to use wild
        /// card character to indicate that the Type name of the method is irrelevant (e.g. "*.Method" or "*.*").</param>
        /// <param name="list">List of 'Method' arguments. </param>
        /// <returns>Returns delegate of CSScriptLibrary.MethodDelegate type.</returns>
        public MethodDelegate GetStaticMethodWithArgs(string methodName, params Type[] list)
        {
            FastInvokeDelegate method = this.asmBrowser.GetMethodInvoker(methodName, list);
            return delegate(object[] paramters) { return method(null, paramters); };
        }

        /// <summary>
        /// Specialised version of GetMethodInvoker which returns MethodDelegate of the very first method found in the
        /// underlying assembly. This method is an overloaded implementation of the GetStaticMethod(string methodName, params object[] list).
        /// <para>
        /// Use thids method when script assembly contains only one single type with one method.
        /// </para>
        /// </summary>
        /// <returns>Returns delegate of CSScriptLibrary.MethodDelegate type.</returns>
        public MethodDelegate GetStaticMethod()
        {
            FastInvokeDelegate method = this.asmBrowser.GetMethodInvoker("*.*");
            return delegate(object[] paramters) { return method(null, paramters); };
        }

        /// <summary>
        /// Specialised version of GetMethodInvoker which returns MethodDelegate thus you do not need to specify
        /// object instance when calling instance methods as delegate will maintain the instance object internaly.
        /// </summary>
        /// <param name="instance">Instance of the type, which implements method is to be wrapped by MethodDelegate.</param>
        /// <param name="methodName">'Method' name including 'Type' name (eg. MyType.DoJob). It is allowd to use wild
        /// card character to indicate that the Type name of the method is irrelevant (e.g. "*.Method" or "*.*").</param>
        /// <param name="list">List of 'Method' arguments.
        /// <para>
        /// Note that values of the items in the list do not have any importance. The type of the list item is
        /// to be used for method search. For example if class Calc has method Sum(int a, int b) the method invoker
        /// can be obtained as following:
        /// <code>
        /// GetMethod(instance, "Sum", 0, 0)
        /// </code>
        /// You can pass any integer as the second and third parameter because it will be used only to obtain the
        /// information about the paramater type (in this case System.Int32).
        /// </para>
        /// </param>
        /// <returns>Returns delegate of CSScriptLibrary.MethodDelegate type.</returns>
        public MethodDelegate GetMethod(object instance, string methodName, params object[] list)
        {
            FastInvokeDelegate method = this.asmBrowser.GetMethodInvoker(instance.GetType().FullName + "." + methodName, list);
            return delegate(object[] paramters) { return method(instance, paramters); };
        }

#endif

        /// <summary>
        /// Creates an instance of AsmHelper for working with assembly dynamically loaded to current AppDomain.
        /// Calling "Dispose" is optional for "current AppDomain"scenario as no new AppDomain will be ever created.
        /// </summary>
        /// <param name="asm">Assembly object.</param>
        public AsmHelper(Assembly asm)
        {
            this.asmBrowser = (IAsmBrowser)(new AsmBrowser(asm));
            InitProbingDirs();
        }

        /// <summary>
        /// Creates an instance of AsmHelper for working with assembly dynamically loaded to non-current AppDomain.
        /// This method initialises instance and creates new ('remote') AppDomain with 'domainName' name. New AppDomain is automatically unloaded as result of "disposable" behaviour of AsmHelper.
        /// </summary>
        /// <param name="asmFile">File name of the assembly to be loaded.</param>
        /// <param name="domainName">Name of the domain to be created.</param>
        /// <param name="deleteOnExit">'true' if assembly file should be deleted when new AppDomain is unloaded; otherwise, 'false'.</param>
        public AsmHelper(string asmFile, string domainName, bool deleteOnExit)
        {
            this.deleteOnExit = deleteOnExit;
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = Path.GetDirectoryName(asmFile);
            setup.PrivateBinPath = AppDomain.CurrentDomain.BaseDirectory;
            setup.ApplicationName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            setup.ShadowCopyFiles = "true";
            setup.ShadowCopyDirectories = Path.GetDirectoryName(asmFile);
            remoteAppDomain = AppDomain.CreateDomain(domainName != null ? domainName : "", null, setup);

            AsmRemoteBrowser asmBrowser = (AsmRemoteBrowser)remoteAppDomain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().Location, typeof(AsmRemoteBrowser).ToString());
            asmBrowser.AsmFile = asmFile;
            this.asmBrowser = (IAsmBrowser)asmBrowser;

            InitProbingDirs();
        }

        /// <summary>
        /// Executes static method of the underlying assembly.
        /// </summary>
        /// <param name="methodName">'Method' name including 'Type' name (e.g. MyType.DoJob). It is allowed to use wild card character
        /// to indicate that the Type name of the method is irrelevant (e.g. "*.Method").</param>
        /// <param name="list">List of 'Method' arguments.</param>
        /// <returns>Returns object of the same type as 'Method' return type.</returns>
        public object Invoke(string methodName, params object[] list)
        {
            if (this.disposed)
                throw new ObjectDisposedException(this.ToString());
            return asmBrowser.Invoke(methodName, list);
        }

        /// <summary>
        /// Executes an instance method of the underlying assembly.
        /// </summary>
        /// <param name="obj">Instance of the object whose method is to be invoked.</param>
        /// <param name="methodName">'Method' name (excluding 'Type' name). It is allowed to use wild card character
        /// to indicate that the Type name of the method is irrelevant (e.g. "*.Method" or even "*.*").</param>
        /// <param name="list">List of 'Method' arguments.</param>
        /// <returns>Returns object of the same type as 'Method' return type.</returns>
        public object InvokeInst(object obj, string methodName, params object[] list)
        {
            if (this.disposed)
                throw new ObjectDisposedException(this.ToString());
            return asmBrowser.Invoke(obj, methodName, list);
        }

        /// <summary>
        /// Attempts to create instance of a class from underlying assembly.
        /// </summary>
        /// <param name="typeName">The 'Type' full name of the type to create. (see Assembly.CreateInstance()).
        ///
        /// You can use wild card meaning the first type found. However only full wild card "*" is supported.</param>
        /// <returns>Instance of the 'Type'. Returns null if the instance cannot be created.</returns>
        public object TryCreateObject(string typeName)
        {
            if (this.disposed)
                throw new ObjectDisposedException(this.ToString());
            return asmBrowser.CreateInstance(typeName);
        }

        /// <summary>
        /// Creates instance of a class from underlying assembly.
        /// </summary>
        /// <param name="typeName">The 'Type' full name of the type to create. (see Assembly.CreateInstance()).
        /// You can use wild card meaning the first type found. However only full wild card "*" is supported.</param>
        /// <returns>Instance of the 'Type'. Throws an ApplicationException if the instance cannot be created.</returns>
        public object CreateObject(string typeName)
        {
            object retval = TryCreateObject(typeName);
            if (retval == null)
                throw new ApplicationException(typeName + " cannot be instantiated. Make sure the type name is correct.");
            else
                return retval;
        }

        /// <summary>
        /// Unloads 'remote' AppDomain if it was created.
        /// </summary>
        private void Unload()
        {
            try
            {
                if (remoteAppDomain != null)
                {
                    string asmFile = ((AsmRemoteBrowser)this.asmBrowser).AsmFile;
                    AppDomain.Unload(remoteAppDomain);
                    remoteAppDomain = null;
                    if (deleteOnExit)
                    {
                        File.Delete(asmFile);
                    }
                }
            }
            catch
            {
                //ignore exception as it is possible that we are trying to unload AppDomain
                //during the object finalization (which is illegal).
            }
        }

        /// <summary>
        /// Implementation of IDisposable.Dispose(). Disposes allocated exetrnal resources if any. Call this method to unload non-current AppDomain (if it was created).
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Actual implementation of IDisposable.Dispose()
        /// </summary>
        /// <param name="disposing">'false' if the method has been called by the runtime from inside the finalizer ; otherwise, 'true'.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                Unload();
            }
            disposed = true;
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~AsmHelper()
        {
            Dispose(false);
        }

        bool disposed = false;

        /// <summary>
        ///Array of directories to be used for assembly probing.
        /// </summary>
        public string[] ProbingDirs
        {
            get { return this.asmBrowser.ProbingDirs; }
            set { this.asmBrowser.ProbingDirs = value; }
        }

        private void InitProbingDirs()
        {
            ArrayList dirs = new ArrayList();
            if (CSScript.AssemblyResolvingEnabled && CSScript.ShareHostRefAssemblies)
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

                        //In fact ignore all exceptions as we should continue if for whatever reason the assembly location cannot be obtained
                    }

            if (CSScript.AssemblyResolvingEnabled)
                foreach (string dir in CSScript.GlobalSettings.SearchDirs.Split(';'))
                    if (dir != "")
                        dirs.Add(Environment.ExpandEnvironmentVariables(dir));

            ProbingDirs = CSScript.RemovePathDuplicates((string[])dirs.ToArray(typeof(string)));
        }
    }

    /// <summary>
    /// Defines method for calling assembly methods and instantiating assembly types.
    /// </summary>
    interface IAsmBrowser
    {
        object Invoke(string methodName, params object[] list);

        object Invoke(object obj, string methodName, params object[] list);

        object CreateInstance(string typeName);

        string[] ProbingDirs { get; set; }

        bool CachingEnabled { get; set; }

#if !net1

        T AlignToInterface<T>(object obj) where T : class;

        T AlignToInterface<T>(object obj, bool useAppDomainAssemblies) where T : class;

        T AlignToInterface<T>(object obj, params string[] refAssemblies) where T : class;

#endif

        FastInvokeDelegate GetMethodInvoker(string methodName, object[] list);

        FastInvokeDelegate GetMethodInvoker(string methodName, Type[] list);

        FastInvokeDelegate GetMethodInvoker(string methodName);
    }

    internal class AsmRemoteBrowser : MarshalByRefObject, IAsmBrowser
    {
        private string workingDir;

        AsmBrowser asmBrowser;

        public AsmBrowser AsmBrowser
        {
            get
            {
                if (AsmFile == null)
                    throw new ApplicationException("Assembly name (asmFile) was not set");
                
                return asmBrowser;
            }
        }

        string asmFile;

        public string AsmFile
        {
            get
            {
                return asmFile;
            }
            set
            {
                if (asmFile == null)
                    AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolveEventHandler);

                asmFile = value;
                workingDir = Path.GetDirectoryName(AsmFile);
                asmBrowser = new AsmBrowser(Assembly.LoadFrom(AsmFile));
                asmBrowser.CachingEnabled = cachingEnabled;
            }
        }

        public string[] ProbingDirs
        {
            get { return probingDirs; }
            set { probingDirs = value; }
        }

        string[] probingDirs = new string[] { };

        public bool CachingEnabled
        {
            get { return cachingEnabled; }
            set { cachingEnabled = value; if (asmBrowser != null) asmBrowser.CachingEnabled = cachingEnabled; }
        }

        bool cachingEnabled = true;

        public FastInvokeDelegate GetMethodInvoker(string methodName)
        {
            return this.AsmBrowser.GetMethodInvoker(methodName, new Type[0]);
        }

        public FastInvokeDelegate GetMethodInvoker(string methodName, object[] list)
        {
            return this.AsmBrowser.GetMethodInvoker(methodName, list);
        }

        public FastInvokeDelegate GetMethodInvoker(string methodName, Type[] list)
        {
            return this.AsmBrowser.GetMethodInvoker(methodName, list);
        }

        private Assembly ResolveEventHandler(object sender, ResolveEventArgs args)
        {
            Assembly retval = AssemblyResolver.ResolveAssembly(args.Name, workingDir);
            if (retval == null)
                foreach (string dir in probingDirs)
                    if (null != (retval = AssemblyResolver.ResolveAssembly(args.Name, Environment.ExpandEnvironmentVariables(dir))))
                        break;

            return retval;
        }

        public object Invoke(string methodName, params object[] list)
        {
            return this.AsmBrowser.Invoke(methodName, list);
        }

        public object Invoke(object obj, string methodName, params object[] list)
        {
            return this.AsmBrowser.Invoke(obj, methodName, list);
        }

        //creates instance of a Type from underying assembly
        public object CreateInstance(string typeName)
        {
            if (asmBrowser == null)
            {
                if (AsmFile == null)
                    throw new ApplicationException("Assembly name (asmFile) was not set");

                workingDir = Path.GetDirectoryName(AsmFile);
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolveEventHandler);
                asmBrowser = new AsmBrowser(Assembly.LoadFrom(AsmFile));
            }
            return asmBrowser.CreateInstance(typeName);
        }

#if !net1

        public T AlignToInterface<T>(object obj) where T : class
        {
            var retval = CSScriptLibrary.ThirdpartyLibraries.Rubenhak.Utils.ObjectCaster<T>.As(obj);

            if (retval == null)
                throw new ApplicationException("The object cannot be aligned to this interface.");

            return retval;
        }

        public T AlignToInterface<T>(object obj, bool useAppDomainAssemblies) where T : class
        {
            string[] refAssemblies;
            if (useAppDomainAssemblies)
                refAssemblies = CSSUtils.GetAppDomainAssemblies();
            else
                refAssemblies = new string[0];

            var retval = CSScriptLibrary.ThirdpartyLibraries.Rubenhak.Utils.ObjectCaster<T>.As(obj, refAssemblies);

            if (retval == null)
                throw new ApplicationException("The object cannot be aligned to this interface.");

            return retval;
        }

        public T AlignToInterface<T>(object obj, params string[] refAssemblies) where T : class
        {
            var retval = CSScriptLibrary.ThirdpartyLibraries.Rubenhak.Utils.ObjectCaster<T>.As(obj, refAssemblies);

            if (retval == null)
                throw new ApplicationException("The object cannot be aligned to this interface.");

            return retval;
        }

#endif
    }

    internal class AsmBrowser : IAsmBrowser
    {
        private string workingDir;
#if net1
        private Hashtable methodCache = new Hashtable(); //cached delegates of the type methods
        private Hashtable infoCache = new Hashtable(); //cached MethodInfo(f) of the type method(s)
#else
        private Dictionary<MethodInfo, FastInvoker> methodCache = new Dictionary<MethodInfo, FastInvoker>(); //cached delegates of the type methods
        private Dictionary<MethodSignature, MethodInfo> infoCache = new Dictionary<MethodSignature, MethodInfo>(); //cached MethodInfo(f) of the type method(s)
#endif

        private Assembly asm;

        public AsmBrowser(Assembly asm)
        {
            if (asm == null)
                throw new ArgumentNullException("asm");
            this.asm = asm;

            if (asm.Location != "")
                workingDir = Path.GetDirectoryName(asm.Location);

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolveEventHandler);
        }

        public string[] ProbingDirs
        {
            get { return probingDirs; }
            set { probingDirs = value; }
        }

        string[] probingDirs = new string[] { };

        public bool CachingEnabled
        {
            get { return cachingEnabled; }
            set { cachingEnabled = value; }
        }

        bool cachingEnabled = true;

        private Assembly ResolveEventHandler(object sender, ResolveEventArgs args)
        {
            Assembly retval = AssemblyResolver.ResolveAssembly(args.Name, workingDir);
            if (retval == null)
                foreach (string dir in probingDirs)
                    if (null != (retval = AssemblyResolver.ResolveAssembly(args.Name, Environment.ExpandEnvironmentVariables(dir))))
                        break;

            return retval;
        }

        //executes static method of underying assembly
        public object Invoke(string methodName, params object[] list)
        {
            return Invoke((object)null, methodName, list);
        }

        struct MethodSignature
        {
            public MethodSignature(string name, params object[] args)
            {
                this.name = name;
                this.parameters = new Type[args.Length];
                for (int i = 0; i < this.parameters.Length; i++)
                    this.parameters[i] = args[i].GetType();
            }

            public string name;
            public Type[] parameters;

            public static bool operator ==(MethodSignature x, MethodSignature y)
            {
                return x.Equals(y);
            }

            public static bool operator !=(MethodSignature x, MethodSignature y)
            {
                return !x.Equals(y);
            }

            public override bool Equals(object obj)
            {
                MethodSignature sig = (MethodSignature)obj;
                if (this.name != sig.name)
                    return false;
                if (this.parameters.Length != sig.parameters.Length)
                    return false;
                for (int i = 0; i < this.parameters.Length; i++)
                    if (this.parameters[i] != sig.parameters[i])
                        return false;

                return true;
            }

            public override int GetHashCode()
            {
                StringBuilder sb = new StringBuilder(name);

                foreach (Type param in parameters)
                    sb.Append(param.ToString());

                return sb.ToString().GetHashCode();
            }
        }

        //executes instance method of underying assembly
        public object Invoke(object obj, string methodName, params object[] list)
        {
            string[] names = methodName.Split(".".ToCharArray());
            if (names.Length < 2 && obj != null)
                methodName = obj.GetType().FullName + "." + methodName;

            MethodSignature methodID = new MethodSignature(methodName, list);
            MethodInfo method;
            if (!infoCache.ContainsKey(methodID))
            {
                method = FindMethod(methodName, list);
                infoCache[methodID] = method;
            }
            else
            {
#if net1
                method = (MethodInfo)infoCache[methodID];
#else
                method = infoCache[methodID];
#endif
            }
            try
            {
#if !net1
                if (cachingEnabled)
                {
                    if (!methodCache.ContainsKey(method))
                        methodCache[method] = new FastInvoker(method);

                    return methodCache[method].Invoke(obj, list);
                }
                else
#endif
                    return method.Invoke(obj, list);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException != null ? ex.InnerException : ex; //unpack the exception
            }
        }

        private MethodInfo FindMethod(string methodName, object[] list)
        {
            Type[] args = new Type[list.Length];
            for (int i = 0; i < list.Length; i++)
                args[i] = list[i].GetType();

            return FindMethod(methodName, args);
        }

        private MethodInfo FindMethod(string methodName, Type[] args)
        {
            string[] names = methodName.Split(".".ToCharArray());
            if (names.Length < 2)
                throw new ApplicationException("Invalid method name format (must be: \"<type>.<method>\")");

            string methodShortName = names[names.Length - 1];
            string typeName = names[names.Length - 2];
            MethodInfo method;

            foreach (Module m in asm.GetModules())
            {
                Type[] types;
                if (names[0] == "*")
                    types = m.GetTypes();
                else
                    types = m.FindTypes(Module.FilterTypeName, names[names.Length - 2]);

                foreach (Type t in types)
                    if (methodShortName == "*")
                    {
                        MethodInfo[] methods = t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        if (methods.Length != 0)
                            return methods[0]; //return the very first method
                    }
                    else if ((method = t.GetMethod(methodShortName, args)) != null)
                    {
                        if (typeName.EndsWith("*"))
                            return method;
                        else if (methodName == method.DeclaringType.FullName + "." + methodShortName)
                            return method;
                    }
            }

            string msg = "Method " + methodName + "(";
            if (args.Length > 0)
            {
                foreach (Type arg in args)
                    msg += arg.ToString() + ", ";
                msg = msg.Remove(msg.Length - 2, 2);
            }
            msg += ") cannot be found.";

            throw new ApplicationException(msg);
        }

        public FastInvokeDelegate GetMethodInvoker(string methodName)
        {
            MethodInfo method = FindMethod(methodName, new Type[0]); //if method cannot be found FindMethod will throw an exception
            return GetMethodInvoker(method);
        }

        public FastInvokeDelegate GetMethodInvoker(string methodName, Type[] list)
        {
            MethodInfo method = FindMethod(methodName, list); //if method cannot be found FindMethod will throw an exception
            return GetMethodInvoker(method);
        }

        public FastInvokeDelegate GetMethodInvoker(string methodName, object[] list)
        {
            MethodInfo method = FindMethod(methodName, list); //if method cannot be found FindMethod will throw an exception
            return GetMethodInvoker(method);
        }

        private FastInvokeDelegate GetMethodInvoker(MethodInfo method)
        {
            try
            {
                if (!methodCache.ContainsKey(method))
                    methodCache[method] = new FastInvoker(method);
#if net1
                return (methodCache[method] as FastInvoker).GetMethodInvoker();
#else
                return methodCache[method].GetMethodInvoker();
#endif
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException != null ? ex.InnerException : ex; //unpack the exception
            }
        }

        /// <summary>
        /// Creates instance of a Type from underying assembly.
        /// </summary>
        /// <param name="typeName">Name of the type to be instantiated. Allows wild card character (e.g. *.MyClass can be used to instantiate MyNamespace.MyClass).</param>
        /// <returns>Created instance of the type.</returns>
        public object CreateInstance(string typeName)
        {
            if (typeName.IndexOf("*") != -1)
            {
                //note typeName for FindTypes does not include namespace
                if (typeName == "*")
                    return asm.CreateInstance(asm.GetModules()[0].GetTypes()[0].FullName); //instantiate the first type found
                else
                {
                    Type[] types = asm.GetModules()[0].FindTypes(Module.FilterTypeName, typeName);
                    if (types.Length == 0)
                        throw new ApplicationException("Type " + typeName + " cannot be found.");
                    return asm.CreateInstance(types[0].FullName);
                }
            }
            else
                return asm.CreateInstance(typeName);
        }

#if !net1

        public T AlignToInterface<T>(object obj) where T : class
        {
            var retval = CSScriptLibrary.ThirdpartyLibraries.Rubenhak.Utils.ObjectCaster<T>.As(obj);

            if (retval == null)
                throw new ApplicationException("The object cannot be aligned to this interface.");

            return retval;
        }
                
        public T AlignToInterface<T>(object obj, bool useAppDomainAssemblies) where T : class
        {
            string[] refAssemblies;
            if (useAppDomainAssemblies)
                refAssemblies = CSSUtils.GetAppDomainAssemblies();
            else
                refAssemblies = new string[0];

            var retval = CSScriptLibrary.ThirdpartyLibraries.Rubenhak.Utils.ObjectCaster<T>.As(obj, refAssemblies);

            if (retval == null)
                throw new ApplicationException("The object cannot be aligned to this interface.");

            return retval;
        }

        public T AlignToInterface<T>(object obj, params string[] refAssemblies) where T : class
        {
            var retval = CSScriptLibrary.ThirdpartyLibraries.Rubenhak.Utils.ObjectCaster<T>.As(obj, refAssemblies);

            if (retval == null)
                throw new ApplicationException("The object cannot be aligned to this interface.");

            return retval;
        }

#endif
    }

    /// <summary>
    /// Class which is capable of emitting the dynamic method delegate based on the MethodInfo. Such delegate is
    /// extremely fast and it can demonstrate up to 100 times better performance comparing to the pure
    /// Reflection method invokation (MethodInfo.Invoke()).
    ///
    ///
    /// Based on http://www.codeproject.com/KB/cs/FastInvokerWrapper.aspx
    /// </summary>
    public class FastInvoker
    {
        //it is almost 100 times faster than reflection (MethodInfo.Invoke())

        FastInvokeDelegate method;
        /// <summary>
        /// MethodInfo instance which was used to generate dynamic method delegate.
        /// </summary>
        public MethodInfo info;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">MethodInfo instance which is to be used to generate dynamic method delegate.</param>
        public FastInvoker(MethodInfo info)
        {
            this.info = info;
            method = GenerateMethodInvoker(info);
        }

        /// <summary>
        /// Invokes dynamic method delegate generated from the MethodInfo object.
        /// </summary>
        /// <param name="instance">Instance of the type which method is to be invoked.</param>
        /// <param name="paramters">Optional method parameters.</param>
        /// <returns>Invokes dynamic method delegate return value</returns>
        public object Invoke(object instance, params object[] paramters)
        {
            return method(instance, paramters);
        }

        /// <summary>
        /// Returns dynamic method delegate generated from the MethodInfo object.
        /// </summary>
        /// <returns>FastInvokeDelegate instance.</returns>
        public FastInvokeDelegate GetMethodInvoker()
        {
            return method;
        }

        private FastInvokeDelegate GenerateMethodInvoker(MethodInfo methodInfo)
        {
#if net1
            throw new NotImplementedException("This version of AsmHelper does not implement GenerateMethodInvoker()\n"+
                "Please use CSScriptLibrarly.dll which is compiled against at least CLR v2.0");
#else
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, typeof(object), new Type[] { typeof(object), typeof(object[]) }, methodInfo.DeclaringType.Module);
            ILGenerator il = dynamicMethod.GetILGenerator();
            ParameterInfo[] ps = methodInfo.GetParameters();
            Type[] paramTypes = new Type[ps.Length];
            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                    paramTypes[i] = ps[i].ParameterType.GetElementType();
                else
                    paramTypes[i] = ps[i].ParameterType;
            }
            LocalBuilder[] locals = new LocalBuilder[paramTypes.Length];

            for (int i = 0; i < paramTypes.Length; i++)
            {
                locals[i] = il.DeclareLocal(paramTypes[i], true);
            }
            for (int i = 0; i < paramTypes.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_1);
                EmitFastInt(il, i);
                il.Emit(OpCodes.Ldelem_Ref);
                EmitCastToReference(il, paramTypes[i]);
                il.Emit(OpCodes.Stloc, locals[i]);
            }
            if (!methodInfo.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
            }
            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                    il.Emit(OpCodes.Ldloca_S, locals[i]);
                else
                    il.Emit(OpCodes.Ldloc, locals[i]);
            }
            if (methodInfo.IsStatic)
                il.EmitCall(OpCodes.Call, methodInfo, null);
            else
                il.EmitCall(OpCodes.Callvirt, methodInfo, null);
            if (methodInfo.ReturnType == typeof(void))
                il.Emit(OpCodes.Ldnull);
            else
                EmitBoxIfNeeded(il, methodInfo.ReturnType);

            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                {
                    il.Emit(OpCodes.Ldarg_1);
                    EmitFastInt(il, i);
                    il.Emit(OpCodes.Ldloc, locals[i]);
                    if (locals[i].LocalType.IsValueType)
                        il.Emit(OpCodes.Box, locals[i].LocalType);
                    il.Emit(OpCodes.Stelem_Ref);
                }
            }

            il.Emit(OpCodes.Ret);
            FastInvokeDelegate invoder = (FastInvokeDelegate)dynamicMethod.CreateDelegate(typeof(FastInvokeDelegate));
            return invoder;
#endif
        }

        private static void EmitCastToReference(ILGenerator il, System.Type type)
        {
#if net1
            throw new NotImplementedException("This version of AsmHelper does not implement EmitCastToReference().\n"+
                "Please use CSScriptLibrarly.dll which is compiled against at least CLR v2.0");
#else
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, type);
            }
            else
            {
                il.Emit(OpCodes.Castclass, type);
            }
#endif
        }

        private static void EmitBoxIfNeeded(ILGenerator il, System.Type type)
        {
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Box, type);
            }
        }

        private static void EmitFastInt(ILGenerator il, int value)
        {
            switch (value)
            {
                case -1:
                    il.Emit(OpCodes.Ldc_I4_M1);
                    return;
                case 0:
                    il.Emit(OpCodes.Ldc_I4_0);
                    return;
                case 1:
                    il.Emit(OpCodes.Ldc_I4_1);
                    return;
                case 2:
                    il.Emit(OpCodes.Ldc_I4_2);
                    return;
                case 3:
                    il.Emit(OpCodes.Ldc_I4_3);
                    return;
                case 4:
                    il.Emit(OpCodes.Ldc_I4_4);
                    return;
                case 5:
                    il.Emit(OpCodes.Ldc_I4_5);
                    return;
                case 6:
                    il.Emit(OpCodes.Ldc_I4_6);
                    return;
                case 7:
                    il.Emit(OpCodes.Ldc_I4_7);
                    return;
                case 8:
                    il.Emit(OpCodes.Ldc_I4_8);
                    return;
            }

            if (value > -129 && value < 128)
            {
                il.Emit(OpCodes.Ldc_I4_S, (SByte)value);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4, value);
            }
        }
    }
}