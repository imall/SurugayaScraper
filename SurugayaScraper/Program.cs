using System.Text;
using System.Text.Json;

namespace SurugayaScraper;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        
        Console.WriteLine("駿河屋商品爬蟲 - 批次處理");
        Console.WriteLine("=========================");
        Console.WriteLine();
        
        // 寫死的商品網址陣列
        var urls = new[]
        {
            "https://www.suruga-ya.jp/product/detail/873118848",
            "https://www.suruga-ya.jp/product/detail/873118847",
            "https://www.suruga-ya.jp/product/detail/873093949",
            "https://www.suruga-ya.jp/product/detail/561459059"
        };
        
        Console.WriteLine($"準備爬取 {urls.Length} 個商品...\n");
        
        var scraper = new SurugayaScraper();
        var products = new List<ProductInfo>();
        
        // 逐一爬取每個商品
        for (int i = 0; i < urls.Length; i++)
        {
            try
            {
                Console.WriteLine($"[{i + 1}/{urls.Length}] 正在處理...");
                var product = await scraper.ScrapeProductAsync(urls[i]);
                products.Add(product);
                Console.WriteLine($"✓ 完成: {product.Title}\n");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 錯誤: {ex.Message}\n");
            }
        }
        
        // 輸出 JSON
        Console.WriteLine("\n=========================");
        Console.WriteLine("JSON 輸出:");
        Console.WriteLine("=========================\n");
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        
        var json = JsonSerializer.Serialize(products, options);
        Console.WriteLine(json);
        
        // 儲存到檔案
        var outputFile = "products.json";
        await File.WriteAllTextAsync(outputFile, json);
        Console.WriteLine($"\n✓ JSON 已儲存到 {outputFile}");
        
        Console.WriteLine("\n按任意鍵結束...");
        Console.ReadKey();
    }
}