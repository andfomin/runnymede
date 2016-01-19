using Runnymede.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace Runnymede.Website.Utils
{
    public class LuckyYouHelper
    {

        public async Task<IEnumerable<KeyValuePair<DateTime, IEnumerable<KeyValuePair<string, string>>>>> ProcessRates()
        {
            var html = await Download();
            var table = ExtractTable(html);
            var xml = ParseTable(table);
            return ProcessXml(xml);
        }

        private async Task<string> Download()
        {
            var url = "http://www.bankofengland.co.uk/BOEAPPS/IADB/Rates.asp?Travel=NIxRSx&into=GBP&rateview=L";
            using (var client = new HttpClient())
            {
                return await client.GetStringAsync(url);
            }
        }

        private string ExtractTable(string html)
        {
            var start = html.LastIndexOf("<table ");
            var finish = html.IndexOf("</table>", start);
            var table = html.Substring(start, finish - start + "</table>".Length);
            return table;
        }

        private XElement ParseTable(string table)
        {
            var t = table
            .Replace("&pound;", "£")
            .Replace("&", String.Empty)
            ;

            // Broken XHTML! Opening <td> tags have no closing tags. We fix it.
            var lines = GeneralUtils.ReadLines(t);
            var lines2 = lines
                .Select(i =>
                {
                    return ((i.IndexOf("<td") != -1) && (i.IndexOf("</td>") == -1))
                         ? i + " </td>"
                         : i;
                });
            var table2 = String.Join(String.Empty, lines2);

            var xml = XElement.Parse(table2);
            return xml;
        }

        private IEnumerable<KeyValuePair<DateTime, IEnumerable<KeyValuePair<string, string>>>> ProcessXml(XElement xml)
        {
            var rows = xml.Elements("tr");

            var dates = ParseDates(rows);
            var currencies = ParseCurrencies(rows);

            var d1 = dates
                .Select((i, idx) =>
                {
                    var list = new List<KeyValuePair<string, string>>();
                    foreach (var c in currencies)
                    {
                        var kvp = c.Skip(idx).Take(1).Single();
                        list.Add(kvp);
                    }
                    return new KeyValuePair<DateTime, IEnumerable<KeyValuePair<string, string>>>(i, list);
                })
                ;

            // Rates are missing for weekends. We fill continguos dates.
            var maxDate = dates.First();
            var minDate = dates.Last();
            var duration = (maxDate - minDate).Days;
            var d2 = Enumerable.Range(0, duration)
                .Select(i => maxDate.AddDays(-i))
                ;

            // Missing dates get rates from the next available date.
            var d3 = d2.
                Select(i =>
                {
                    var q1 = d1
                        .Where(j => j.Key >= i)
                        .OrderBy(j => j.Key)
                        .First();
                    return new KeyValuePair<DateTime, IEnumerable<KeyValuePair<string, string>>>(i, q1.Value);
                })
                ;
            return d3;
        }

        private IEnumerable<DateTime> ParseDates(IEnumerable<XElement> rows)
        {
            var header = rows.First();
            var d = header.Elements("th")
                .Skip(1)
                .Select(i =>
                {
                    var start = i.Value.IndexOf("£1");
                    var t = i.Value.Substring(start + 2).Trim();
                    var date = DateTime.Parse(t);
                    return date;
                });
            return d;
        }

        IEnumerable<IEnumerable<KeyValuePair<string, string>>> ParseCurrencies(IEnumerable<XElement> rows)
        {
            var c = rows
                .Skip(1).Take(3)
                .Select(i => i.Elements("td").Select(j => j.Value.Trim()))
                .Select(i =>
                {
                    var currency = i.First();
                    var rates = i.Skip(1);
                    return rates.Select(j => new KeyValuePair<string, string>(currency, j));
                })
                ;
            return c;
        }





    }
}