using System.ComponentModel.DataAnnotations;

namespace SahinSoft.Domain.Enums;

public enum PrinterConnectionType
{
    [Display(Name = "Windows Yazıcı (USB/Spooler)")]
    WindowsSpooler = 1,
    [Display(Name = "Ağ (TCP/IP, Port 9100)")]
    NetworkRaw9100 = 2
}
