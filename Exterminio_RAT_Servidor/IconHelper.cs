using System;
using System.Drawing;
using System.Windows.Forms;

public static class IconHelper
{
    public static Bitmap GetStockIcon(NativeMethods.SHSTOCKICONID iconId, bool smallIcon = true)
    {
        try
        {
            NativeMethods.SHSTOCKICONINFO sii = new NativeMethods.SHSTOCKICONINFO();
            sii.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(sii);

            NativeMethods.SHGSI flags = NativeMethods.SHGSI.SHGSI_ICON;
            if (smallIcon)
                flags |= NativeMethods.SHGSI.SHGSI_SMALLICON;
            else
                flags |= NativeMethods.SHGSI.SHGSI_LARGEICON;

            int result = NativeMethods.SHGetStockIconInfo(iconId, flags, ref sii);
            
            if (result == 0 && sii.hIcon != IntPtr.Zero)
            {
                using (Icon icon = Icon.FromHandle(sii.hIcon))
                {
                    return icon.ToBitmap();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error obteniendo icono nativo {iconId}: {ex.Message}");
        }

        // Fallback a iconos del sistema
        return GetFallbackIcon(iconId);
    }

    private static Bitmap GetFallbackIcon(NativeMethods.SHSTOCKICONID iconId)
    {
        switch (iconId)
        {
            case NativeMethods.SHSTOCKICONID.SIID_FOLDER:
            case NativeMethods.SHSTOCKICONID.SIID_FOLDEROPEN:
                return SystemIcons.WinLogo.ToBitmap();
            
            case NativeMethods.SHSTOCKICONID.SIID_APPLICATION:
            case NativeMethods.SHSTOCKICONID.SIID_SOFTWARE:
                return SystemIcons.Application.ToBitmap();
            
            case NativeMethods.SHSTOCKICONID.SIID_DOCNOASSOC:
            case NativeMethods.SHSTOCKICONID.SIID_DOCASSOC:
                return SystemIcons.Information.ToBitmap();
            
            case NativeMethods.SHSTOCKICONID.SIID_AUDIOFILES:
                return SystemIcons.Question.ToBitmap();
            
            case NativeMethods.SHSTOCKICONID.SIID_IMAGEFILES:
                return SystemIcons.Asterisk.ToBitmap();
            
            case NativeMethods.SHSTOCKICONID.SIID_VIDEOFILES:
                return SystemIcons.Question.ToBitmap();
            
            case NativeMethods.SHSTOCKICONID.SIID_ZIPFILE:
                return SystemIcons.Exclamation.ToBitmap();
            
            default:
                return SystemIcons.Application.ToBitmap();
        }
    }
}
