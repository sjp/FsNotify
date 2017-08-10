using System;
using System.Runtime.InteropServices;

namespace SJP.FsNotify
{
    /// <summary>
    /// Provides platform-dependent information.
    /// </summary>
    public static class Platform
    {
        public static bool IsWindows => _isWindows.Value;

#if NETFX
        private readonly static Lazy<bool> _isWindows = new Lazy<bool>(() => Environment.OSVersion.Platform == PlatformID.Win32NT);
#else
        private readonly static Lazy<bool> _isWindows = new Lazy<bool>(() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
#endif

        public static Version OsVersion => _osVersion.Value;

        private readonly static Lazy<Version> _osVersion = new Lazy<Version>(GetOsVersion);

        private static Version GetOsVersion()
        {
#if NETFX
            return Environment.OSVersion.Version;
#else
            var versionInfo = new OSVERSIONINFOEX { dwOSVersionInfoSize = Marshal.SizeOf<OSVERSIONINFOEX>() };
            GetVersionEx(ref versionInfo);

            return new Version(versionInfo.dwMajorVersion, versionInfo.dwMinorVersion, versionInfo.dwBuildNumber);
#endif
        }

        [DllImport("kernel32")]
        internal static extern bool GetVersionEx(ref OSVERSIONINFOEX versionInfo);

        /// <summary>
        /// Contains operating system version information. The information includes major and minor version numbers, a build number, a platform identifier, and information about product suites and the latest Service Pack installed on the system. This structure is used with the GetVersionEx and VerifyVersionInfo functions.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct OSVERSIONINFOEX
        {
            /// <summary>
            /// The size of this data structure, in bytes. Set this member to sizeof(OSVERSIONINFOEX).
            /// </summary>
            public int dwOSVersionInfoSize;

            /// <summary>
            /// The major version number of the operating system. For more information, see Remarks.
            /// </summary>
            public int dwMajorVersion;

            /// <summary>
            /// The minor version number of the operating system. For more information, see Remarks.
            /// </summary>
            public int dwMinorVersion;

            /// <summary>
            /// The build number of the operating system.
            /// </summary>
            public int dwBuildNumber;

            /// <summary>
            /// The operating system platform. This member can be VER_PLATFORM_WIN32_NT (2).
            /// </summary>
            public int dwPlatformId;

            /// <summary>
            /// A null-terminated string, such as "Service Pack 3", that indicates the latest Service Pack installed on the system. If no Service Pack has been installed, the string is empty.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szCSDVersion;

            /// <summary>
            /// The major version number of the latest Service Pack installed on the system. For example, for Service Pack 3, the major version number is 3. If no Service Pack has been installed, the value is zero.
            /// </summary>
            public ushort wServicePackMajor;

            /// <summary>
            /// The minor version number of the latest Service Pack installed on the system. For example, for Service Pack 3, the minor version number is 0.
            /// </summary>
            public ushort wServicePackMinor;

            /// <summary>
            /// A bit mask that identifies the product suites available on the system. This member can be a combination of the following values.
            /// </summary>
            public OSVERIONINFOEX_SUITE_MASK wSuiteMask;

            /// <summary>
            /// Any additional information about the system. This member can be one of the following values.
            /// </summary>
            public OSVERSIONINFOEX_PRODUCT_TYPE wProductType;

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            public byte wReserved;
        }

        // TODO: If/when this project is updated to .NET Standard 2.0 we can remove this
        //       because it's only necessary as we don't have access to Environment.OSVersion.
        //       We need access to the NT version to determine whether RestartManager is available.
        /// <summary>
        ///
        /// </summary>
        [Flags]
        internal enum OSVERIONINFOEX_SUITE_MASK : ushort
        {
            /// <summary>
            /// Microsoft BackOffice components are installed.
            /// </summary>
            VER_SUITE_BACKOFFICE = 0x4,

            /// <summary>
            /// Windows Server 2003, Web Edition is installed.
            /// </summary>
            VER_SUITE_BLADE = 0x400,

            /// <summary>
            /// Windows Server 2003, Compute Cluster Edition is installed.
            /// </summary>
            VER_SUITE_COMPUTE_SERVER = 0x4000,

            /// <summary>
            /// Windows Server 2008 Datacenter, Windows Server 2003, Datacenter Edition, or Windows 2000 Datacenter Server is installed.
            /// </summary>
            VER_SUITE_DATACENTER = 0x80,

            /// <summary>
            /// Windows Server 2008 Enterprise, Windows Server 2003, Enterprise Edition, or Windows 2000 Advanced Server is installed. Refer to the Remarks section for more information about this bit flag.
            /// </summary>
            VER_SUITE_ENTERPRISE = 0x2,

            /// <summary>
            /// Windows XP Embedded is installed.
            /// </summary>
            VER_SUITE_EMBEDDEDNT = 0x40,

            /// <summary>
            /// Windows Vista Home Premium, Windows Vista Home Basic, or Windows XP Home Edition is installed.
            /// </summary>
            VER_SUITE_PERSONAL = 0x200,

            /// <summary>
            /// Remote Desktop is supported, but only one interactive session is supported. This value is set unless the system is running in application server mode.
            /// </summary>
            VER_SUITE_SINGLEUSERTS = 0x100,

            /// <summary>
            /// Microsoft Small Business Server was once installed on the system, but may have been upgraded to another version of Windows. Refer to the Remarks section for more information about this bit flag.
            /// </summary>
            VER_SUITE_SMALLBUSINESS = 0x1,

            /// <summary>
            /// Microsoft Small Business Server is installed with the restrictive client license in force. Refer to the Remarks section for more information about this bit flag.
            /// </summary>
            VER_SUITE_SMALLBUSINESS_RESTRICTED = 0x20,

            /// <summary>
            /// Windows Storage Server 2003 R2 or Windows Storage Server 2003is installed.
            /// </summary>
            VER_SUITE_STORAGE_SERVER = 0x2000,

            /// <summary>
            /// Terminal Services is installed. This value is always set. If <see cref="VER_SUITE_TERMINAL"/> is set but <see cref="VER_SUITE_SINGLEUSERTS"/> is not set, the system is running in application server mode.
            /// </summary>
            VER_SUITE_TERMINAL = 0x10,

            /// <summary>
            /// Windows Home Server is installed.
            /// </summary>
            VER_SUITE_WH_SERVER = 0x8000
        }

        [Flags]
        internal enum OSVERSIONINFOEX_PRODUCT_TYPE : byte
        {
            /// <summary>
            /// The system is a domain controller and the operating system is Windows Server 2012 , Windows Server 2008 R2, Windows Server 2008, Windows Server 2003, or Windows 2000 Server.
            /// </summary>
            VER_NT_DOMAIN_CONTROLLER = 0x2,

            /// <summary>
            /// The operating system is Windows Server 2012, Windows Server 2008 R2, Windows Server 2008, Windows Server 2003, or Windows 2000 Server. Note that a server that is also a domain controller is reported as <see cref="VER_NT_DOMAIN_CONTROLLER"/>, not <see cref="VER_NT_SERVER"/>.
            /// </summary>
            VER_NT_SERVER = 0x3,

            /// <summary>
            /// The operating system is Windows 8, Windows 7, Windows Vista, Windows XP Professional, Windows XP Home Edition, or Windows 2000 Professional.
            /// </summary>
            VER_NT_WORKSTATION = 0x1
        }
    }
}
