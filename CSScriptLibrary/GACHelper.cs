#region Licence...

//-----------------------------------------------------------------------------
// Date:	17/10/04	Time: 2:33p
// Module:	GACHelper.cs
// Classes:	COM
//			InstallReference
//			InstallReferenceGuid
//			AssemblyCache
//			AssemblyEnum
//
// This module contains the definition of the GAC helper classes.
//
// Written by Oleg Shilo (oshilo@gmail.com). Based on work by Junfeng Zhang
// (Simple wrapper for GAC).
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
using System.Runtime.InteropServices;
using System.Text;

/*
 * PSS ID Number: Q317540
 *
 * Use the GAC API in the following scenarios:
 *	When you install an assembly to the GAC.
 *	When you remove an assembly from the GAC.
 *	When you export an assembly from the GAC.
 *	When you enumerate assemblies that are available in the GAC.
 *
*/
//#pragma warning disable 414

namespace csscript
{
    /// <summary>
    /// COM HR checker: just to make code more compact;
    /// </summary>
    class COM
    {
        static public void CheckHR(int hr)
        {
            if (hr < 0)
                Marshal.ThrowExceptionForHR(hr);
        }
    }

    /// <summary>
    /// IAssemblyCache; COM import
    /// </summary>
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae")]
    internal interface IAssemblyCache
    {
        //PreserveSig() Indicates that the HRESULT or retval signature transformation that takes place during COM interop calls should be suppressed
        [PreserveSig()]
        int UninstallAssembly(int flags,
                                [MarshalAs(UnmanagedType.LPWStr)]
                                string assemblyName,
                                InstallReference refData,
                                out AssemblyCacheUninstallDisposition disposition);

        [PreserveSig()]
        int QueryAssemblyInfo(int flags,
                                [MarshalAs(UnmanagedType.LPWStr)]
                                string assemblyName,
                                ref AssemblyInfo assemblyInfo);

        [PreserveSig()]
        int Reserved(int flags,
                                IntPtr pvReserved,
                                out Object ppAsmItem,
                                [MarshalAs(UnmanagedType.LPWStr)]
                                string assemblyName);

        [PreserveSig()]
        int Reserved(out Object ppAsmScavenger);

        [PreserveSig()]
        int InstallAssembly(int flags,
                                [MarshalAs(UnmanagedType.LPWStr)]
                                string assemblyFilePath,
                                InstallReference refData);
    }
    /// <summary>
    /// IAssemblyName; COM import
    /// </summary>
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("CD193BC0-B4BC-11d2-9833-00C04FC31D2E")]
    internal interface IAssemblyName
    {
        [PreserveSig()]
        int SetProperty(int PropertyId,
            IntPtr pvProperty,
            int cbProperty);

        [PreserveSig()]
        int GetProperty(int PropertyId,
                            IntPtr pvProperty,
                            ref int pcbProperty);

        [PreserveSig()]
        int Finalize();

        [PreserveSig()]
        int GetDisplayName(StringBuilder pDisplayName,
                            ref int pccDisplayName,
                            int displayFlags);

        [PreserveSig()]
        int Reserved(ref Guid guid,
                        Object o1,
                        Object o2,
                        string string1,
                        Int64 llFlags,
                        IntPtr pvReserved,
                        int cbReserved,
                        out IntPtr ppv);

        [PreserveSig()]
        int GetName(ref int pccBuffer,
            StringBuilder pwzName);

        [PreserveSig()]
        int GetVersion(out int versionHi,
            out int versionLow);

        [PreserveSig()]
        int IsEqual(IAssemblyName pAsmName,
            int cmpFlags);

        [PreserveSig()]
        int Clone(out IAssemblyName pAsmName);
    }
    /// <summary>
    /// IAssemblyEnum; COM import
    /// </summary>
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("21b8916c-f28e-11d2-a473-00c04f8ef448")]
    internal interface IAssemblyEnum
    {
        [PreserveSig()]
        int GetNextAssembly(IntPtr pvReserved,
            out IAssemblyName ppName,
            int flags);

        [PreserveSig()]
        int Reset();

        [PreserveSig()]
        int Clone(out IAssemblyEnum ppEnum);
    }
    /// <summary>
    /// AssemblyCommitFlags; Used by COM imported calls
    /// </summary>
    enum AssemblyCommitFlags
    {
        Default,
        Force
    }
    /// <summary>
    /// AssemblyCacheFlags; Used by COM imported calls
    /// </summary>
    [Flags]
    internal enum AssemblyCacheFlags
    {
        GAC = 2
    }

    /// <summary>
    /// AssemblyCacheUninstallDisposition; Used by COM imported calls
    /// </summary>
    enum AssemblyCacheUninstallDisposition
    {
        Unknown,
        Uninstalled,
        StillInUse,
        AlreadyUninstalled,
        DeletePending,
        HasInstallReference,
        ReferenceNotFound,
    }

    /// <summary>
    /// CreateAssemblyNameObjectFlags; Used by COM imported calls
    /// </summary>
    internal enum CreateAssemblyNameObjectFlags
    {
        CANOF_DEFAULT,
        CANOF_PARSE_DISPLAY_NAME,
        CANOF_SET_DEFAULT_VALUES
    }

    /// <summary>
    /// AssemblyNameDisplayFlags; Used by COM imported calls
    /// </summary>
    [Flags]
    internal enum AssemblyNameDisplayFlags
    {
        VERSION = 0x01,
        CULTURE = 0x02,
        PUBLIC_KEY_TOKEN = 0x04,
        PROCESSORARCHITECTURE = 0x20,
        RETARGETABLE = 0x80,
        ALL = VERSION
            | CULTURE
            | PROCESSORARCHITECTURE
            | PUBLIC_KEY_TOKEN
            | RETARGETABLE
    }

    /// <summary>
    /// InstallReference + struct initialization; Used by COM imported calls
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    class InstallReference
    {
        int cbSize;
        int flags;
        Guid guidScheme;
        [MarshalAs(UnmanagedType.LPWStr)]
        string identifier;
        [MarshalAs(UnmanagedType.LPWStr)]
        string nonCannonicalData;

        public InstallReference(Guid guid, string id, string data)
        {
            cbSize = (int)(2 * IntPtr.Size + 16 + (id.Length + data.Length) * 2);
            flags = 0;
            guidScheme = guid;
            identifier = id;
            nonCannonicalData = data;
        }

        public Guid GuidScheme
        {
            get { return guidScheme; }
        }
    }

    /// <summary>
    /// AssemblyInfo; Used by COM imported calls
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct AssemblyInfo
    {
        public int cbAssemblyInfo;
        public int assemblyFlags;
        public long assemblySizeInKB;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string currentAssemblyPath;
        public int cchBuf;
    }

    /// <summary>
    /// InstallReferenceGuid; Used by COM imported calls
    /// </summary>
    [ComVisible(false)]
    class InstallReferenceGuid
    {
        public static bool IsValidGuidScheme(Guid guid)
        {
            return (guid.Equals(UninstallSubkeyGuid) ||
                    guid.Equals(FilePathGuid) ||
                    guid.Equals(OpaqueStringGuid) ||
                    guid.Equals(Guid.Empty));
        }

        public readonly static Guid UninstallSubkeyGuid = new Guid("8cedc215-ac4b-488b-93c0-a50a49cb2fb8");
        public readonly static Guid FilePathGuid = new Guid("b02f9d65-fb77-4f7a-afa5-b391309f11c9");
        public readonly static Guid OpaqueStringGuid = new Guid("2ec93463-b0c3-45e1-8364-327e96aea856");
    }

    /// <summary>
    ///  Helper calss for IAssemblyCache
    /// </summary>
    [ComVisible(false)]
    class AssemblyCache
    {
        // If you use this, fusion will do the streaming & commit
        public static void InstallAssembly(string assemblyPath, InstallReference reference, AssemblyCommitFlags flags)
        {
            if (reference != null)
            {
                if (!InstallReferenceGuid.IsValidGuidScheme(reference.GuidScheme))
                    throw new ArgumentException("Invalid argument( reference guid).");
            }

            IAssemblyCache asmCache = null;

            COM.CheckHR(CreateAssemblyCache(out asmCache, 0));
            COM.CheckHR(asmCache.InstallAssembly((int)flags, assemblyPath, reference));
        }

        public static void UninstallAssembly(string assemblyName, InstallReference reference, out AssemblyCacheUninstallDisposition disp)
        {
            AssemblyCacheUninstallDisposition dispResult = AssemblyCacheUninstallDisposition.Uninstalled;
            if (reference != null)
            {
                if (!InstallReferenceGuid.IsValidGuidScheme(reference.GuidScheme))
                    throw new ArgumentException("Invalid argument (reference guid).");
            }

            IAssemblyCache asmCache = null;

            COM.CheckHR(CreateAssemblyCache(out asmCache, 0));
            COM.CheckHR(asmCache.UninstallAssembly(0, assemblyName, reference, out dispResult));

            disp = dispResult;
        }

        public static string QueryAssemblyInfo(string assemblyName)
        {
            if (assemblyName == null)
            {
                throw new ArgumentException("Invalid argument (assemblyName)");
            }

            AssemblyInfo aInfo = new AssemblyInfo();
            aInfo.cchBuf = 1024;
            aInfo.currentAssemblyPath = "Path".PadLeft(aInfo.cchBuf);

            IAssemblyCache ac = null;
            COM.CheckHR(CreateAssemblyCache(out ac, 0));
            COM.CheckHR(ac.QueryAssemblyInfo(0, assemblyName, ref aInfo));

            return aInfo.currentAssemblyPath;
        }

        [DllImport("fusion.dll")]
        internal static extern int CreateAssemblyCache(out IAssemblyCache ppAsmCache, int reserved);
    }

    /// <summary>
    /// Helper calss for IAssemblyEnum
    /// </summary>
    [ComVisible(false)]
    class AssemblyEnum
    {
        public AssemblyEnum(string sAsmName)
        {
            IAssemblyName asmName = null;
            if (sAsmName != null)	//if no name specified all assemblies will be returned
            {
                COM.CheckHR(CreateAssemblyNameObject(out asmName, sAsmName, CreateAssemblyNameObjectFlags.CANOF_PARSE_DISPLAY_NAME, IntPtr.Zero));
            }
            COM.CheckHR(CreateAssemblyEnum(out m_assemblyEnum, IntPtr.Zero, asmName, AssemblyCacheFlags.GAC, IntPtr.Zero));
        }

        public string GetNextAssembly()
        {
            string retval = null;
            if (!m_done)
            {
                IAssemblyName asmName = null;
                COM.CheckHR(m_assemblyEnum.GetNextAssembly((IntPtr)0, out asmName, 0));

                if (asmName != null)
                    retval = GetFullName(asmName);

                m_done = (retval == null);
            }
            return retval;
        }

        private string GetFullName(IAssemblyName asmName)
        {
            StringBuilder fullName = new StringBuilder(1024);
            int iLen = fullName.Capacity;
            COM.CheckHR(asmName.GetDisplayName(fullName, ref iLen, (int)AssemblyNameDisplayFlags.ALL));

            return fullName.ToString();
        }

        [DllImport("fusion.dll")]
        internal static extern int CreateAssemblyEnum(out IAssemblyEnum ppEnum,
            IntPtr pUnkReserved,
            IAssemblyName pName,
            AssemblyCacheFlags flags,
            IntPtr pvReserved);

        [DllImport("fusion.dll")]
        internal static extern int CreateAssemblyNameObject(out IAssemblyName ppAssemblyNameObj,
            [MarshalAs(UnmanagedType.LPWStr)]
            string szAssemblyName,
            CreateAssemblyNameObjectFlags flags,
            IntPtr pvReserved);

        private bool m_done;
        private IAssemblyEnum m_assemblyEnum = null;
    }
}