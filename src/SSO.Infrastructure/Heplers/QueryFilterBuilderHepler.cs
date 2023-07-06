using System.Text;
using System.Text.Json;

namespace SSO.Infrastructure.Heplers
{
    public class QueryFilterBuilderHepler
    {
        public static string FilterBuilder(string filterString)
        {
            if (string.IsNullOrEmpty(filterString))
            {
                return string.Empty;
            }

            var filters = JsonSerializer.Deserialize<ICollection<object[]>>(filterString)!;
            return FilterBuilder(filters);
        }

        public static string FilterBuilder(ICollection<object[]> filters)
        {
            if (filters == null || !filters.Any())
            {
                return string.Empty;
            }
            StringBuilder sbFilter = new StringBuilder();
            foreach (var item in filters)
            {
                if (item.Length == 0 || ((JsonElement)item[0]).ValueKind != JsonValueKind.String)
                {
                    continue;
                }
                if (item.Length == 3)
                {
                    if (((JsonElement)item[2]).ValueKind == JsonValueKind.String)
                    {
                        sbFilter.AppendLine(string.Join(" ", item[0], item[1], $"'{item[2]}'"));
                    }
                    else
                    {
                        sbFilter.AppendLine(string.Join(" ", item));
                    }
                }
                else if (item.Length == 2 && ((JsonElement)item[2]).ValueKind == JsonValueKind.Array)
                {
                    sbFilter.AppendLine(string.Join(" ", item[0], "(", FilterBuilder(item[1].ToString()), ")"));
                }
                else if (item.Length == 1)
                {
                    sbFilter.AppendLine(item[0].ToString());
                }
            }

            return sbFilter.ToString();
        }
    }
}
