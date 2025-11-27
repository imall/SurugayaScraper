namespace SurugayaScraper;

public class ProductInfo
{
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal? SalePrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}
