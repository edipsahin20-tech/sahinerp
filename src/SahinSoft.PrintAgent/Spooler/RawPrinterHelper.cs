using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace SahinSoft.PrintAgent.Spooler;

// Windows Print Spooler'a "RAW" veri tipi ile yazma - standart, iyi bilinen bir P/Invoke deseni
// (winspool.drv). ESC/POS baytları hiç yorumlanmadan/dönüştürülmeden doğrudan yazıcı kuyruğuna
// gönderilir - spooler bunu olduğu gibi fiziksel yazıcıya iletir.
[SupportedOSPlatform("windows")]
public static class RawPrinterHelper
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DOCINFOA
    {
        [MarshalAs(UnmanagedType.LPStr)] public string pDocName;
        [MarshalAs(UnmanagedType.LPStr)] public string? pOutputFile;
        [MarshalAs(UnmanagedType.LPStr)] public string pDataType;
    }

    [DllImport("winspool.drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true)]
    private static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

    [DllImport("winspool.drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true)]
    private static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true)]
    private static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In] ref DOCINFOA pDocInfo);

    [DllImport("winspool.drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true)]
    private static extern bool EndDocPrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true)]
    private static extern bool StartPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true)]
    private static extern bool EndPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true)]
    private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

    public static (bool Success, string? Error) SendRawBytes(string printerName, byte[] bytes)
    {
        if (!OpenPrinter(printerName, out var hPrinter, IntPtr.Zero))
        {
            return (false, $"Yazıcı açılamadı: '{printerName}' (Win32 hata: {Marshal.GetLastWin32Error()})");
        }

        try
        {
            var docInfo = new DOCINFOA
            {
                pDocName = "SahinSoft Print Job",
                pOutputFile = null,
                pDataType = "RAW"
            };

            if (!StartDocPrinter(hPrinter, 1, ref docInfo))
            {
                return (false, $"Doküman başlatılamadı (Win32 hata: {Marshal.GetLastWin32Error()})");
            }

            try
            {
                if (!StartPagePrinter(hPrinter))
                {
                    return (false, $"Sayfa başlatılamadı (Win32 hata: {Marshal.GetLastWin32Error()})");
                }

                var unmanagedBytes = Marshal.AllocCoTaskMem(bytes.Length);
                try
                {
                    Marshal.Copy(bytes, 0, unmanagedBytes, bytes.Length);
                    if (!WritePrinter(hPrinter, unmanagedBytes, bytes.Length, out var written) || written != bytes.Length)
                    {
                        return (false, $"Yazma başarısız (Win32 hata: {Marshal.GetLastWin32Error()})");
                    }
                }
                finally
                {
                    Marshal.FreeCoTaskMem(unmanagedBytes);
                }

                EndPagePrinter(hPrinter);
            }
            finally
            {
                EndDocPrinter(hPrinter);
            }

            return (true, null);
        }
        finally
        {
            ClosePrinter(hPrinter);
        }
    }

    public static string[] GetInstalledPrinterNames()
    {
        return System.Drawing.Printing.PrinterSettings.InstalledPrinters.Cast<string>().ToArray();
    }
}
