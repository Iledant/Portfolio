using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Portfolio.ViewModel
{
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

        public async void PickStocks(string pattern)
        {
            string url = $"https://www.morningstar.fr/fr/util/SecuritySearch.ashx?" +
                $"ifIncludeAds=False&q={pattern}&limit=100";

            try
            {
                HttpResponseMessage response = await _client.GetAsync(url);
                string content = await response.Content.ReadAsStringAsync();
                ParsePickStocksResponse(content);
            }
            catch (Exception e)
            {
                Log.AddLine(e.ToString(), LogState.Error);
                PickStockLines = new List<PickStockLine>();
            }
        }

        private void ParsePickStocksResponse(string content)
        {
            string[] lines = content.Split("\n");
            List<PickStockLine> pickStocks = new();
            foreach (string line in lines)
            {
                string[] fields = line.Split('|');
                if (fields.Length < 6)
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
                pickStocks.Add(new PickStockLine { 
                    Name = fields[0], 
                    MorningStarID = fields[1].Substring(left, right - left), 
                    Category = fields[5], 
                    Place = fields[4], 
                    Abbreviation = fields[3] });
            }
            PickStockLines = pickStocks;
        }
    }
}
