namespace ScheduleR.Sdk
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    internal class Win32HttpUrlAclService : IHttpUrlAclService
    {
        [DllImport("httpapi.dll", SetLastError = true)]
        private static extern uint HttpInitialize(HTTPAPI_VERSION Version, uint Flags, IntPtr pReserved);

        [DllImport("httpapi.dll", SetLastError = true)]
        private static extern uint HttpSetServiceConfiguration(
             IntPtr ServiceIntPtr,
             HTTP_SERVICE_CONFIG_ID ConfigId,
             IntPtr pConfigInformation,
             int ConfigInformationLength,
             IntPtr pOverlapped);

        [DllImport("httpapi.dll", SetLastError = true)]
        private static extern uint HttpTerminate(uint Flags, IntPtr pReserved);

        private enum HTTP_SERVICE_CONFIG_ID
        {
            HttpServiceConfigIPListenList = 0,
            HttpServiceConfigSSLCertInfo,
            HttpServiceConfigUrlAclInfo,
            HttpServiceConfigMax
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private struct HTTPAPI_VERSION
        {
            public ushort HttpApiMajorVersion;
            public ushort HttpApiMinorVersion;

            public HTTPAPI_VERSION(ushort majorVersion, ushort minorVersion)
            {
                this.HttpApiMajorVersion = majorVersion;
                this.HttpApiMinorVersion = minorVersion;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HTTP_SERVICE_CONFIG_URLACL_SET
        {
            public HTTP_SERVICE_CONFIG_URLACL_KEY KeyDesc;
            public HTTP_SERVICE_CONFIG_URLACL_PARAM ParamDesc;

            public HTTP_SERVICE_CONFIG_URLACL_SET(HTTP_SERVICE_CONFIG_URLACL_KEY keyDesc, HTTP_SERVICE_CONFIG_URLACL_PARAM paramDesc)
            {
                this.KeyDesc = keyDesc;
                this.ParamDesc = paramDesc;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct HTTP_SERVICE_CONFIG_URLACL_KEY
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pUrlPrefix;

            public HTTP_SERVICE_CONFIG_URLACL_KEY(string urlPrefix)
            {
                this.pUrlPrefix = urlPrefix;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct HTTP_SERVICE_CONFIG_URLACL_PARAM
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pStringSecurityDescriptor;

            public HTTP_SERVICE_CONFIG_URLACL_PARAM(string securityDescriptor)
            {
                this.pStringSecurityDescriptor = securityDescriptor;
            }
        }

        private const uint NOERROR = 0;
        private const uint ERROR_ALREADY_EXISTS = 183;
        private const uint HTTP_INITIALIZE_CONFIG = 0x00000002;

        public bool TryReserve(IEnumerable<string> urls)
        {
            Guard.Against.NullOrEmptyOrNullElements(() => urls);

            return urls.All(TryReserve);
        }

        private bool TryReserve(string url)
        {
            var httpApiVersion = new HTTPAPI_VERSION(1, 0);

            var returnValue = HttpInitialize(httpApiVersion, HTTP_INITIALIZE_CONFIG, IntPtr.Zero);
            if (returnValue != NOERROR)
            {
                return false;
            }

            var inputConfigInfoSet = new HTTP_SERVICE_CONFIG_URLACL_SET(
                new HTTP_SERVICE_CONFIG_URLACL_KEY(url),
                new HTTP_SERVICE_CONFIG_URLACL_PARAM("D:(A;;GX;;;WD)"));

            var pInputConfigInfo = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(HTTP_SERVICE_CONFIG_URLACL_SET)));
            Marshal.StructureToPtr(inputConfigInfoSet, pInputConfigInfo, false);

            returnValue = HttpSetServiceConfiguration(IntPtr.Zero,
                HTTP_SERVICE_CONFIG_ID.HttpServiceConfigUrlAclInfo,
                pInputConfigInfo,
                Marshal.SizeOf(inputConfigInfoSet),
                IntPtr.Zero);

            if (returnValue == ERROR_ALREADY_EXISTS)
            {
                // TODO (Cameron): Do something different...?
                return false;
            }

            Marshal.FreeCoTaskMem(pInputConfigInfo);
            HttpTerminate(HTTP_INITIALIZE_CONFIG, IntPtr.Zero);

            return returnValue == NOERROR;
        }
    }
}
