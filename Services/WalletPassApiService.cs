using Damanak.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Damanak.Services
{
    public interface IWalletPassApiService
    {
        Task<string> CreateWarrantyPassAsync(Guarantee guarantee);
    }

    public class WalletPassApiService : IWalletPassApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public WalletPassApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> CreateWarrantyPassAsync(Guarantee guarantee)
        {
            var baseUrl = _configuration["WalletPassApi:BaseUrl"];
            var endpoint = _configuration["WalletPassApi:CreatePassEndpoint"];
            var apiKey = _configuration["WalletPassApi:ApiKey"];
            var publicBaseUrl = _configuration["WalletPassApi:PublicBaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl) ||
                string.IsNullOrWhiteSpace(endpoint) ||
                string.IsNullOrWhiteSpace(apiKey) ||
                apiKey == "PUT_YOUR_API_KEY_HERE")
            {
                throw new InvalidOperationException("مفتاح Wallet API غير مضاف في appsettings.json");
            }

            if (string.IsNullOrWhiteSpace(publicBaseUrl) ||
                publicBaseUrl == "https://your-live-domain.com")
            {
                throw new InvalidOperationException("PublicBaseUrl غير مضبوط. لازم تحطين رابط الموقع بعد النشر.");
            }

            var statusText = guarantee.Status switch
            {
                "Active" => "ساري",
                "ExpiringSoon" => "ينتهي قريبًا",
                "Expired" => "منتهي",
                _ => "ساري"
            };

            var logoUrl = $"{publicBaseUrl}/images/damank1.png";
            var warrantyDetailsUrl = $"{publicBaseUrl}/Guarantees/Details/{guarantee.Id}";

            var payload = new
            {
                cardTitle = "ضمانك",
                header = guarantee.ProductName,
                subheader = $"ضمان من {guarantee.StoreName}",

                logoUrl = logoUrl,
                heroImage = logoUrl,
                appleHeroImage = logoUrl,
                googleHeroImage = logoUrl,
                rectangleLogo = logoUrl,

                hexBackgroundColor = "#123458",
                appleFontColor = "#FFFFFF",

                textModulesData = new[]
                {
                    new
                    {
                        id = "store",
                        header = "المتجر",
                        body = guarantee.StoreName
                    },
                    new
                    {
                        id = "status",
                        header = "الحالة",
                        body = statusText
                    },
                    new
                    {
                        id = "warrantyEnd",
                        header = "انتهاء الضمان",
                        body = guarantee.WarrantyEndDate.ToString("yyyy/MM/dd")
                    },
                    new
                    {
                        id = "brand",
                        header = "الماركة",
                        body = string.IsNullOrEmpty(guarantee.Brand) ? "غير مسجلة" : guarantee.Brand
                    },
                    new
                    {
                        id = "model",
                        header = "الموديل",
                        body = string.IsNullOrEmpty(guarantee.ModelName) ? "غير مسجل" : guarantee.ModelName
                    },
                    new
                    {
                        id = "serial",
                        header = "الرقم التسلسلي",
                        body = string.IsNullOrEmpty(guarantee.SerialNumber) ? "غير مسجل" : guarantee.SerialNumber
                    }
                },

                linksModuleData = new[]
                {
                    new
                    {
                        id = "details",
                        description = "عرض تفاصيل الضمان",
                        uri = warrantyDetailsUrl
                    }
                },

                barcodeType = "QR_CODE",
                barcodeValue = warrantyDetailsUrl,
                barcodeAltText = $"Damanak-{guarantee.Id}",

                startDate = guarantee.PurchaseDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"),
                endDate = guarantee.WarrantyEndDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            var requestUrl = $"{baseUrl}{endpoint}";

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

            request.Headers.Add("apikey", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var json = JsonSerializer.Serialize(payload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"فشل إنشاء بطاقة Wallet: {responseBody}");
            }

            using var document = JsonDocument.Parse(responseBody);

            if (document.RootElement.TryGetProperty("shareUrl", out var shareUrlElement))
            {
                var shareUrl = shareUrlElement.GetString();

                if (!string.IsNullOrWhiteSpace(shareUrl))
                {
                    return shareUrl;
                }
            }

            throw new InvalidOperationException("الخدمة رجعت رد بدون shareUrl.");
        }
    }
}