using Newtonsoft.Json;
using Portfolio.Models;
using Portfolio.Repositories;
using System;
using System.Collections.Generic;
using System.Net.Http;
using YahooFinanceApi;

namespace Portfolio.ViewModel
{
    public class Quote
    {
        public string Exchange { get; set; }
        public string Shortname { get; set; }
        public string QuoteType { get; set; }
        public string Symbol { get; set; }
        public string Index { get; set; }
        public double Score { get; set; }
        public string TypeDisp { get; set; }
        public string Longname { get; set; }
        public bool IsYahooFinance { get; set; }
    }

    public class Root
    {
        public List<object> Explains { get; set; }
        public int Count { get; set; }
        public List<Quote> Quotes { get; set; }
        public List<object> News { get; set; }
        public List<object> Nav { get; set; }
        public List<object> Lists { get; set; }
        public List<object> ResearchReports { get; set; }
        public List<object> ScreenerFieldResults { get; set; }
        public int TotalTime { get; set; }
        public int TimeTakenForQuotes { get; set; }
        public int TimeTakenForNews { get; set; }
        public int TimeTakenForAlgowatchlist { get; set; }
        public int TimeTakenForPredefinedScreener { get; set; }
        public int TimeTakenForCrunchbase { get; set; }
        public int TimeTakenForNav { get; set; }
        public int TimeTakenForResearchReports { get; set; }
        public int TimeTakenForScreenerField { get; set; }
    }

    public class MorningstarResponseLine
    {
        public string Name;
        public string MorningStarID;
        public string Category;
        public string Place;
        public string Abbreviation;
    }

    public class APIFinanceViewModel : Bindable
    {
        private IReadOnlyList<Candle> _history;
        private string _errorMessage;
        private List<Fund> _funds;
        private static readonly HttpClient _client = new();
        private List<Quote> _quotes;
        private List<Company> _companies;
        private List<MorningstarResponseLine> _morningstarResponses;

        public IReadOnlyList<Candle> History
        {
            get => _history;
            set
            {
                _history = value;
                OnPropertyChanged(nameof(History));
            }
        }

        public List<Fund> Funds
        {
            get => _funds;
            set
            {
                _funds = value;
                OnPropertyChanged(nameof(Funds));
            }
        }

        public List<MorningstarResponseLine> MorningstarResponses
        {
            get => _morningstarResponses;
            set
            {
                _morningstarResponses = value;
                OnPropertyChanged(nameof(MorningstarResponses));
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public List<Quote> Quotes
        {
            get => _quotes;
            set
            {
                _quotes = value;
                OnPropertyChanged(nameof(Quotes));
            }
        }

        public List<Company> Companies
        {
            get => _companies;
            set
            {
                _companies = value;
                OnPropertyChanged(nameof(Companies));
            }
        }

        public async void GetHistorical(string pattern, DateTime startTime, DateTime? endTime = null)
        {
            Yahoo.IgnoreEmptyRows = true;
            try
            {
                History = await Yahoo.GetHistoricalAsync(pattern, startTime, endTime);
                ErrorMessage = "";
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Invalid ticker"))
                {
                    ErrorMessage = "Impossible de trouver la valeur";
                }
            }
        }

        public async void PickStocks(string pattern)
        {
            string url = $"https://query2.finance.yahoo.com/v1/finance/search?q={pattern}" +
                "&quotesCount=10&newsCount=0&enableFuzzyQuery=false&quotesQueryId=tss_match_phrase_query" +
                "&multiQuoteQueryId=multi_quote_single_token_query&newsQueryId=news_ss_symbols&enableCb=false" +
                "&enableNavLinks=false&vespaNewsTimeoutMs=500";

            try
            {
                HttpResponseMessage response = await _client.GetAsync(url);
                string content = await response.Content.ReadAsStringAsync();
                Root root = JsonConvert.DeserializeObject<Root>(content);
                Quotes = root?.Quotes;
            }
            catch (Exception e)
            {
                Log.AddLine(e.ToString(), LogState.Error);
            }
        }

        public void GetFunds()
        {
            Funds = FundRepository.Get("");
        }

        public void GetCompanies()
        {
            Companies = CompanyRepository.Get("");
        }

        public async void MorningstarSearch(string pattern)
        {
            string url = $"https://www.morningstar.fr/fr/util/SecuritySearch.ashx?" +
                $"ifIncludeAds=False&q={pattern}&limit=100";

            try
            {
                HttpResponseMessage response = await _client.GetAsync(url);
                string content = await response.Content.ReadAsStringAsync();
                ParseMorningstarResponse(content);
            }
            catch (Exception e)
            {
                Log.AddLine(e.ToString(), LogState.Error);
                MorningstarResponses = new List<MorningstarResponseLine>();
            }
        }

        private void ParseMorningstarResponse(string content)
        {
            string[] lines = content.Split("\n");
            List<MorningstarResponseLine> pickStocks = new();
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
                pickStocks.Add(new MorningstarResponseLine
                {
                    Name = fields[0],
                    MorningStarID = fields[1].Substring(left, right - left),
                    Category = fields[5],
                    Place = fields[4],
                    Abbreviation = fields[3]
                });
            }
            MorningstarResponses = pickStocks;
        }
    }
}
