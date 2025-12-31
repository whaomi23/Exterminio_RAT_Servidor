using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Exterminio_RAT_Servidor
{
    internal class IconosUniversalesGestorArchivos
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(
                string pszPath,
                uint dwFileAttributes,
                ref SHFILEINFO psfi,
                uint cbFileInfo,
                uint uFlags);

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x1;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x10;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;

        // Cache de íconos por extensión
        private static readonly Dictionary<string, Icon> iconCache = new Dictionary<string, Icon>();

        public static Icon GetIcon(string pathOrExtension, bool largeIcon = true)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            uint flags = SHGFI_ICON;

            flags |= largeIcon ? SHGFI_LARGEICON : SHGFI_SMALLICON;

            uint attributes;
            string key = pathOrExtension.ToLower();

            if (Directory.Exists(pathOrExtension))
            {
                attributes = FILE_ATTRIBUTE_DIRECTORY;
                key = "folder"; // clave para carpetas
            }
            else if (File.Exists(pathOrExtension))
            {
                attributes = FILE_ATTRIBUTE_NORMAL;
                key = Path.GetExtension(pathOrExtension).ToLower();
            }
            else
            {
                if (!pathOrExtension.StartsWith("."))
                    pathOrExtension = "." + pathOrExtension;
                attributes = FILE_ATTRIBUTE_NORMAL;
                flags |= SHGFI_USEFILEATTRIBUTES;
            }

            if (iconCache.ContainsKey(key))
                return iconCache[key];

            SHGetFileInfo(pathOrExtension, attributes, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags);
            Icon icon = Icon.FromHandle(shinfo.hIcon).Clone() as Icon;
            iconCache[key] = icon;

            return icon;
        }
    }
}

