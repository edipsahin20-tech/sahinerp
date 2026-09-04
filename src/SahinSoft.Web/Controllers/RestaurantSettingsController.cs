using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SahinSoft.Domain.Constants;

namespace SahinSoft.Web.Controllers;

// Restoran Ayarları hub'ı (Edip, 2026-09-04, madde 26) - "Sol ana menüde Ayarlar, Mutfak
// bölümünün üstünde yer almalıdır ... restoran/POS için gerekli tanımlar tek merkezden
// yönetilebilmelidir". Bu ekran restoran shell'inden (POS operasyon ekranları) ana ERP'nin
// tanım/ayar sayfalarına köprü - kartlar arttıkça (kasa tanımları, tahsilat carileri,
// entegrasyonlar vb.) buraya eklenecek. Aynı veri kaynağı - ayrı bir "restoran-only" kopya
// parametre YOK, hepsi ana ERP'nin kendi tablolarını kullanıyor.
[Authorize(Roles = $"{AppRoles.Administrator},{AppRoles.RestaurantManager}")]
public sealed class RestaurantSettingsController : Controller
{
    public IActionResult Index() => View();
}
