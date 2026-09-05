namespace SahinSoft.Web.Models;

// Personel PIN giriş ekranı (2026-09-05 teknik doküman madde 12) - personel/kasiyer LİSTESİ
// TAMAMEN KALDIRILDI, tek alan PIN'dir (bkz. RestaurantAuthController.Login).
public sealed class RestaurantPinLoginViewModel
{
    public string ReturnUrl { get; set; } = "/";
}
