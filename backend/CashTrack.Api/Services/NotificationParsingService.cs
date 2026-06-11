using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CashTrack.Api.Contracts;

namespace CashTrack.Api.Services;

public sealed class NotificationParsingService
{
    private static readonly Regex[] AmountPatterns =
    [
        new(@"([+-]?)[\s]*([\d,\.]+)\s*(?:VND|VNĐ|đ|đồng)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"([\d,\.]+)\s*(?:VND|VNĐ|đ|đồng)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"(?:số tiền|so tien|amount)[\s:]+([+-]?[\d,\.]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"(?:GD|giao dịch|giao dich)[\s:]*([+-]?[\d,\.]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)
    ];

    private static readonly string[] ExpenseKeywords =
    [
        "thanh toán", "thanh toan", "chi", "trừ", "tru", "chuyển đi", "chuyen di", "rút", "rut", "mua", "gd:", "-", "payment", "withdraw"
    ];

    private static readonly string[] IncomeKeywords =
    [
        "nhận", "nhan", "vào", "vao", "tiền vào", "tien vao", "cộng", "cong", "chuyển đến", "chuyen den", "+", "nạp", "nap", "received", "deposit", "salary", "lương"
    ];

    private static readonly string[] AdvertisementKeywords =
    [
        "khuyến mãi", "khuyen mai", "ưu đãi", "uu dai", "giảm giá", "giam gia", "sale", "promotion", "miễn phí", "mien phi", "free", "cashback", "voucher", "coupon", "mở thẻ", "mo the", "vay", "loan", "tín dụng", "tin dung"
    ];

    private static readonly string[] TransactionPriorityKeywords =
    [
        "số dư", "so du", "balance", "giao dịch thành công", "giao dich thanh cong", "biến động số dư", "bien dong so du", "chuyển khoản thành công", "chuyen khoan thanh cong", "thanh toán thành công", "thanh toan thanh cong", "đã nhận", "da nhan", "received", "gd:", "stk:", "tk:", "ma gd", "mã gd"
    ];

    private static readonly string[] BankingKeywords =
    [
        "vnd", "vnđ", "giao dịch", "gd:", "số dư", "thanh toán", "chuyển khoản", "nhận tiền", "rút tiền", "nạp tiền", "biến động số dư", "transaction", "balance", "stk", "tk ", "tài khoản", "tai khoan", "account"
    ];

    private static readonly Regex[] MerchantPatterns =
    [
        new(@"(?:tại|tai|at)\s+(.+?)(?:\s+(?:lúc|luc|vào|vao|ngày|ngay)|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"(?:từ|tu|from)\s+(.+?)(?:\s+(?:lúc|luc|vào|vao)|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"(?:đến|den|to)\s+(.+?)(?:\s+(?:lúc|luc|vào|vao)|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"(?:nơi giao dịch|noi giao dich)[\s:]+(.+?)(?:\s+(?:lúc|luc)|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled)
    ];

    private static readonly IReadOnlyDictionary<string, string> MerchantCategories = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["circle k"] = "food",
        ["gs25"] = "food",
        ["vinmart"] = "food",
        ["winmart"] = "food",
        ["grabfood"] = "food",
        ["coffee"] = "food",
        ["cafe"] = "food",
        ["restaurant"] = "food",
        ["shopee"] = "shopping",
        ["lazada"] = "shopping",
        ["tiki"] = "shopping",
        ["grab"] = "transport",
        ["gojek"] = "transport",
        ["taxi"] = "transport",
        ["cgv"] = "entertainment",
        ["netflix"] = "entertainment",
        ["spotify"] = "entertainment",
        ["evn"] = "bills",
        ["viettel"] = "bills",
        ["vnpt"] = "bills",
        ["vinmec"] = "health",
        ["hospital"] = "health"
    };

    public ParseNotificationResponse Parse(BankNotificationDto notification, IReadOnlyDictionary<string, string> packageToBankCode)
    {
        var title = notification.Title?.Trim() ?? string.Empty;
        var text = notification.Text?.Trim() ?? string.Empty;
        var app = notification.App?.Trim() ?? string.Empty;
        var rawText = $"{title} {text}".Trim();
        var normalized = rawText.ToLowerInvariant();
        var isAdvertisement = IsAdvertisement(normalized);
        var amountResult = ParseAmount(rawText);
        var isBanking = !isAdvertisement
            && amountResult is not null
            && BankingKeywords.Any(keyword => normalized.Contains(keyword, StringComparison.OrdinalIgnoreCase));

        if (!isBanking || amountResult is null)
        {
            return new ParseNotificationResponse(
                IsBankingNotification: false,
                IsAdvertisement: isAdvertisement,
                Parsed: null,
                SuggestedCategory: null,
                DuplicateKey: BuildDuplicateKey(rawText),
                Reason: isAdvertisement ? "advertisement" : "not-banking-notification");
        }

        var merchant = ParseMerchant(rawText);
        packageToBankCode.TryGetValue(app, out var bankCode);
        var notificationTime = ParseNotificationTime(notification.Time);
        var parsed = new ParsedTransactionDto(
            Amount: amountResult.Value.Amount,
            Type: amountResult.Value.Type,
            Merchant: merchant,
            Description: text.Length > 200 ? text[..200] : text,
            BankCode: bankCode,
            AccountNumber: null,
            Time: notificationTime,
            RawText: rawText);

        return new ParseNotificationResponse(
            IsBankingNotification: true,
            IsAdvertisement: false,
            Parsed: parsed,
            SuggestedCategory: SuggestCategory(parsed),
            DuplicateKey: BuildDuplicateKey(rawText),
            Reason: null);
    }

    public static DateTime ParseNotificationTime(JsonElement? value)
    {
        if (value is null)
        {
            return DateTime.UtcNow;
        }

        var element = value.Value;
        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out var epochMilliseconds))
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(epochMilliseconds).UtcDateTime;
        }

        if (element.ValueKind == JsonValueKind.String)
        {
            var text = element.GetString();
            if (DateTimeOffset.TryParse(text, out var dateTimeOffset))
            {
                return dateTimeOffset.UtcDateTime;
            }
        }

        return DateTime.UtcNow;
    }

    private static bool IsAdvertisement(string normalizedText)
    {
        var hasTransactionPriority = TransactionPriorityKeywords.Any(keyword => normalizedText.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        if (hasTransactionPriority)
        {
            return false;
        }

        var adKeywordCount = AdvertisementKeywords.Count(keyword => normalizedText.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        return adKeywordCount >= 2
            || Regex.IsMatch(normalizedText, @"ưu đãi.*%", RegexOptions.IgnoreCase)
            || Regex.IsMatch(normalizedText, @"giảm.*%", RegexOptions.IgnoreCase)
            || Regex.IsMatch(normalizedText, @"hoàn tiền.*%", RegexOptions.IgnoreCase);
    }

    private static (long Amount, string Type)? ParseAmount(string text)
    {
        var normalized = text.ToLowerInvariant();
        var type = "expense";
        if (IncomeKeywords.Any(keyword => normalized.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
        {
            type = "income";
        }
        else if (ExpenseKeywords.Any(keyword => normalized.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
        {
            type = "expense";
        }

        foreach (var pattern in AmountPatterns)
        {
            var match = pattern.Match(text);
            if (!match.Success)
            {
                continue;
            }

            var sign = match.Groups.Count > 2 ? match.Groups[1].Value : string.Empty;
            var amountText = match.Groups.Count > 2 && !string.IsNullOrWhiteSpace(match.Groups[2].Value)
                ? match.Groups[2].Value
                : match.Groups[1].Value;

            if (sign == "-")
            {
                type = "expense";
            }
            else if (sign == "+")
            {
                type = "income";
            }

            var digits = new string(amountText.Where(char.IsDigit).ToArray());
            if (long.TryParse(digits, out var amount) && amount > 0)
            {
                return (amount, type);
            }
        }

        return null;
    }

    private static string? ParseMerchant(string text)
    {
        foreach (var pattern in MerchantPatterns)
        {
            var match = pattern.Match(text);
            if (!match.Success || match.Groups.Count < 2)
            {
                continue;
            }

            var merchant = Regex.Replace(match.Groups[1].Value.Trim(), @"\s*(lúc|luc|ngày|ngay|vào|vao)\s*\d.*$", string.Empty, RegexOptions.IgnoreCase).Trim();
            if (merchant.Length is > 2 and < 100)
            {
                return merchant;
            }
        }

        return null;
    }

    private static string SuggestCategory(ParsedTransactionDto parsed)
    {
        var text = parsed.RawText.ToLowerInvariant();
        if (parsed.Type == "income")
        {
            if (text.Contains("lương", StringComparison.OrdinalIgnoreCase) || text.Contains("luong", StringComparison.OrdinalIgnoreCase) || text.Contains("salary", StringComparison.OrdinalIgnoreCase))
            {
                return "salary";
            }

            if (text.Contains("quà", StringComparison.OrdinalIgnoreCase) || text.Contains("gift", StringComparison.OrdinalIgnoreCase))
            {
                return "gift";
            }

            if (text.Contains("đầu tư", StringComparison.OrdinalIgnoreCase) || text.Contains("investment", StringComparison.OrdinalIgnoreCase))
            {
                return "investment";
            }

            return "transfer";
        }

        if (!string.IsNullOrWhiteSpace(parsed.Merchant))
        {
            foreach (var item in MerchantCategories)
            {
                if (parsed.Merchant.Contains(item.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return item.Value;
                }
            }
        }

        if (text.Contains("grab", StringComparison.OrdinalIgnoreCase) || text.Contains("taxi", StringComparison.OrdinalIgnoreCase)) return "transport";
        if (text.Contains("bill", StringComparison.OrdinalIgnoreCase) || text.Contains("hóa đơn", StringComparison.OrdinalIgnoreCase) || text.Contains("hoa don", StringComparison.OrdinalIgnoreCase)) return "bills";
        if (text.Contains("shop", StringComparison.OrdinalIgnoreCase) || text.Contains("mua", StringComparison.OrdinalIgnoreCase)) return "shopping";
        if (text.Contains("cgv", StringComparison.OrdinalIgnoreCase) || text.Contains("netflix", StringComparison.OrdinalIgnoreCase)) return "entertainment";
        if (text.Contains("hospital", StringComparison.OrdinalIgnoreCase) || text.Contains("bệnh viện", StringComparison.OrdinalIgnoreCase)) return "health";
        if (text.Contains("school", StringComparison.OrdinalIgnoreCase) || text.Contains("học phí", StringComparison.OrdinalIgnoreCase)) return "education";
        if (text.Contains("coffee", StringComparison.OrdinalIgnoreCase) || text.Contains("ăn", StringComparison.OrdinalIgnoreCase) || text.Contains("food", StringComparison.OrdinalIgnoreCase)) return "food";

        return "other";
    }

    private static string BuildDuplicateKey(string rawText)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawText));
        return $"sha256:{Convert.ToHexString(hash).ToLowerInvariant()}";
    }
}
