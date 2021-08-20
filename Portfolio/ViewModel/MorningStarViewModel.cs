using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Portfolio.ViewModel
{
    public class HistoryDetail
    {
        public string EndDate { get; set; }
        public string Value { get; set; }
    }

    public class Security
    {
        public List<HistoryDetail> HistoryDetail { get; set; }
        public string Id { get; set; }
    }

    public class TimeSeries
    {
        public List<Security> Security { get; set; }
    }

    public class MorningStarPayloadRoot
    {
        public TimeSeries TimeSeries { get; set; }
    }

    public class PickStockLine
    {
        public string Name;
        public string MorningStarID;
        public string Category;
        public string Place;
        public string Abbreviation;
    }

    public class MorningStarViewModel : Bindable
    {
        private static readonly HttpClient _client = new();
        private List<PickStockLine> _pickStockLines;

        public List<PickStockLine> PickStockLines
        {
            get => _pickStockLines;
            set
            {
                _pickStockLines = value;
                OnPropertyChanged(nameof(PickStockLines));
            }
        }

        public async void PickStocks(string pattern, DateTime? begin = null, DateTime? end = null)
        {
            string endDate = (end ?? DateTime.Now).ToString("yyyy-MM-dd");
            string beginDate = (begin ?? new DateTime(1991, 11, 29)).ToString("yyyy-MM-dd");
            string url = $"https://tools.morningstar.fr/api/rest.svc/timeseries_price/ok91jeenoo?id={pattern}" +
                $"&currencyId=EUR&idtype=Morningstar&frequency=daily&startDate={beginDate}&endDate={endDate}&outputType=JSON";

            try
            {
                HttpResponseMessage response = await _client.GetAsync(url);
                string content = await response.Content.ReadAsStringAsync();
                string[] lines = content.Split("\n");
                List<PickStockLine> pickStocks = new();
                foreach (string line in lines)
                {
                    string[] fields = line.Split('|');
                    if (fields.Length <6)
                    {
                        continue;
                    }
                    int left = fields[1].IndexOf("\"i\":\"") + 5;
                    int right = fields[1].IndexOf("\",\"");
                    if (left == -1 || right == -1 || right <= left)
                    {
                        Log.AddLine("Erreur de format dans MonrningStar PickStock sur la ligne : " + line);
                        continue;
                    }
                    pickStocks.Add(new PickStockLine { Name = fields[0], MorningStarID = fields[1].Substring(left, right-left), Category = fields[5], Place = fields[4], Abbreviation = fields[3] });
                }
                PickStockLines = pickStocks;
            }
            catch (Exception e)
            {
                Log.AddLine(e.ToString(), LogState.Error);
                PickStockLines = new List<PickStockLine>();
            }
        }
    }
}
