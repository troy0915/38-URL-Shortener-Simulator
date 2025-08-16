using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class ShortUrl
{
    private static readonly string Base62Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    private static readonly Random Rng = new Random();
    private static readonly Regex UrlPattern = new Regex(@"^https?:\/\/", RegexOptions.IgnoreCase);

    public string OriginalUrl { get; }
    public string Code { get; private set; }
    public List<DateTime> Hits { get; } = new List<DateTime>();

    public ShortUrl(string originalUrl, string customCode = null, HashSet<string> existingCodes = null)
    {
        if (!UrlPattern.IsMatch(originalUrl))
            throw new ArgumentException("Invalid URL. Must start with http:// or https://");

        OriginalUrl = originalUrl;

        if (!string.IsNullOrWhiteSpace(customCode))
        {
            Code = customCode;
        }
        else
        {
            Code = GenerateUniqueCode(existingCodes ?? new HashSet<string>());
        }
    }

    private string GenerateUniqueCode(HashSet<string> existingCodes)
    {
        string code;
        do
        {
            code = GenerateRandomBase62(6);
        } while (existingCodes.Contains(code));
        return code;
    }

    private string GenerateRandomBase62(int length)
    {
        var sb = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            sb.Append(Base62Chars[Rng.Next(Base62Chars.Length)]);
        }
        return sb.ToString();
    }

    public void Visit()
    {
        Hits.Add(DateTime.UtcNow.Date);
    }

    public int TotalVisits => Hits.Count;

    public Dictionary<DateTime, int> DailyStats =>
        Hits.GroupBy(d => d)
            .ToDictionary(g => g.Key, g => g.Count());
}

public class UrlShortener
{
    private Dictionary<string, ShortUrl> urls = new Dictionary<string, ShortUrl>();

    public void AddUrl(string originalUrl, string customAlias = null)
    {
        if (!string.IsNullOrWhiteSpace(customAlias) && urls.ContainsKey(customAlias))
        {
            Console.WriteLine("❌ Custom alias already exists.");
            return;
        }

        var existingCodes = urls.Keys.ToHashSet();
        var shortUrl = new ShortUrl(originalUrl, customAlias, existingCodes);
        urls[shortUrl.Code] = shortUrl;

        Console.WriteLine($"✅ Short URL created: {shortUrl.Code} -> {shortUrl.OriginalUrl}");
    }

    public void VisitUrl(string code)
    {
        if (urls.TryGetValue(code, out var shortUrl))
        {
            shortUrl.Visit();
            Console.WriteLine($"🌐 Visiting {shortUrl.OriginalUrl}");
        }
        else
        {
            Console.WriteLine("❌ Code not found.");
        }
    }

    public void ShowStats(string code)
    {
        if (urls.TryGetValue(code, out var shortUrl))
        {
            Console.WriteLine($"📊 Stats for {code} ({shortUrl.OriginalUrl}):");
            Console.WriteLine($"Total visits: {shortUrl.TotalVisits}");
            foreach (var entry in shortUrl.DailyStats.OrderBy(e => e.Key))
            {
                Console.WriteLine($"{entry.Key:yyyy-MM-dd}: {entry.Value} hits");
            }
        }
        else
        {
            Console.WriteLine("❌ Code not found.");
        }
    }
}
namespace _38__URL_Shortener_Simulator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var shortener = new UrlShortener();

            while (true)
            {
                Console.WriteLine("\n--- URL Shortener Menu ---");
                Console.WriteLine("1. Add URL");
                Console.WriteLine("2. Visit URL");
                Console.WriteLine("3. View Stats");
                Console.WriteLine("4. Exit");
                Console.Write("Choose: ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Enter original URL: ");
                        var url = Console.ReadLine();
                        Console.Write("Enter custom alias (optional): ");
                        var alias = Console.ReadLine();
                        shortener.AddUrl(url, string.IsNullOrWhiteSpace(alias) ? null : alias);
                        break;

                    case "2":
                        Console.Write("Enter code: ");
                        shortener.VisitUrl(Console.ReadLine());
                        break;

                    case "3":
                        Console.Write("Enter code: ");
                        shortener.ShowStats(Console.ReadLine());
                        break;

                    case "4":
                        return;

                    default:
                        Console.WriteLine("❌ Invalid choice.");
                        break;
                }
            }
        }
    }
}
