using System.ComponentModel.DataAnnotations;

namespace Damanak.Models;

public class Warranty
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Product name is required")]
    public string ProductName { get; set; } = string.Empty;

    public string? Brand { get; set; }

    public string? SerialNumber { get; set; }

    public string? StoreName { get; set; }

    [Required(ErrorMessage = "Purchase date is required")]
    public DateTime PurchaseDate { get; set; }

    [Required(ErrorMessage = "Expiry date is required")]
    public DateTime ExpiryDate { get; set; }

    public string? InvoiceImagePath { get; set; }

    public string? WarrantyCardPath { get; set; }

    public string? QrCodePath { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}