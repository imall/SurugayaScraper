using HtmlAgilityPack;

namespace SurugayaScraper;

public class SurugayaScraper
{
    private readonly HttpClient _httpClient;

    public SurugayaScraper()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        _httpClient.DefaultRequestHeaders.Add("Accept-Language", "ja-JP,ja;q=0.9,en-US;q=0.8,en;q=0.7");
    }

    public async Task<ProductInfo> ScrapeProductAsync(string url)
    {
        // 下載網頁內容
        var html = await _httpClient.GetStringAsync(url);

        // 使用 HtmlAgilityPack 解析 HTML
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var product = new ProductInfo
        {
            Url = url,
            LastUpdated = DateTime.Now
        };

        // 抓取商品標題 - 使用駿河屋的實際 class 名稱
        var titleNode = doc.DocumentNode.SelectSingleNode("//h1[@class='h1_title_product']")
                       ?? doc.DocumentNode.SelectSingleNode("//h1[@id='item_title']")
                       ?? doc.DocumentNode.SelectSingleNode("//h1");
        product.Title = titleNode?.InnerText.Trim() ?? "找不到標題";

        // 抓取商品圖片
        var imageNode = doc.DocumentNode.SelectSingleNode("//div[@class='item_img']//img")
                       ?? doc.DocumentNode.SelectSingleNode("//img[contains(@class,'img-fluid')]");
        if (imageNode != null)
        {
            var imgSrc = imageNode.GetAttributeValue("src", "");
            if (!string.IsNullOrEmpty(imgSrc))
            {
                if (!imgSrc.StartsWith("http"))
                {
                    product.ImageUrl = "https://www.suruga-ya.jp" + imgSrc;
                }
                else
                {
                    product.ImageUrl = imgSrc;
                }
            }
        }

        // 先檢查是否售完
        var outOfStockNode = doc.DocumentNode.SelectSingleNode("//div[@class='mgnB5 out-of-stock-text']");
        bool isOutOfStock = outOfStockNode != null;

        if (isOutOfStock)
        {
            // 商品已售完，設定狀態
            product.Status = outOfStockNode?.InnerText.Trim() ?? "品切れ中";
            product.CurrentPrice = 0;
            product.SalePrice = null;
        }
        else
        {
            // 商品有庫存，抓取價格資訊

            // 抓取原價 (price-old) - 劃掉的價格
            var oldPriceNode = doc.DocumentNode.SelectSingleNode("//span[@class='text-price-detail price-old']");
            if (oldPriceNode != null)
            {
                var oldPriceText = oldPriceNode.InnerText
                    .Replace("¥", "")
                    .Replace(",", "")
                    .Replace("円", "")
                    .Replace("(税込)", "")
                    .Trim();
                if (decimal.TryParse(oldPriceText, out var oldPrice))
                {
                    product.CurrentPrice = oldPrice;
                }
            }

            // 抓取優惠價格 (price-buy) - 實際購買價格
            var buyPriceNode = doc.DocumentNode.SelectSingleNode("//span[@class='text-price-detail price-buy']");
            if (buyPriceNode != null)
            {
                var buyPriceText = buyPriceNode.InnerText
                    .Replace("¥", "")
                    .Replace(",", "")
                    .Replace("円", "")
                    .Replace("(税込)", "")
                    .Trim();
                if (decimal.TryParse(buyPriceText, out var buyPrice))
                {
                    // 如果有原價，則 buyPrice 是優惠價
                    if (product.CurrentPrice > 0)
                    {
                        product.SalePrice = buyPrice;
                    }
                    else
                    {
                        // 如果沒有原價，則 buyPrice 就是當前價格
                        product.CurrentPrice = buyPrice;
                    }
                }
            }

            // 抓取庫存狀態
            var stockNode = doc.DocumentNode.SelectSingleNode("//span[@class='tag_product blue-light']/span");
            if (stockNode != null)
            {
                product.Status = stockNode.InnerText.Trim();
            }
            else
            {
                // 如果找不到庫存標籤，嘗試其他可能的狀態
                var statusNode = doc.DocumentNode.SelectSingleNode("//p[@class='status']");
                product.Status = statusNode?.InnerText.Trim() ?? "未知";
            }
        }

        // 檢查是否有 Flash Sale (タイムセール)
        var flashSaleNode = doc.DocumentNode.SelectSingleNode("//div[@class='flash_sale d-flex justify-content-between border']");
        if (flashSaleNode != null)
        {
            product.Status += " (タイムセール中)";
        }

        return product;
    }
}
