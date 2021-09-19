using Portfolio.Models;
using Portfolio.Repositories;
using System;
using System.Collections.Generic;

namespace Portfolio.ViewModel
{
    public class PortFolioViewModel : Bindable
    {
        private List<PortFolioLine> _lines;
        private double _cash;
        private List<MonetaryAccountBalance> _monetaryAccountBalances;

        public List<PortFolioLine> Lines
        {
            get => _lines;
            set
            {
                _lines = value;
                OnPropertyChanged(nameof(Lines));
            }
        }

        public double Cash
        {
            get => _cash;
            set
            {
                _cash = value;
                OnPropertyChanged(nameof(Cash));
            }
        }

        public List<MonetaryAccountBalance> MonetaryAccountBalances
        {
            get => _monetaryAccountBalances;
            set
            {
                _monetaryAccountBalances = value;
                OnPropertyChanged(nameof(MonetaryAccountBalances));
            }
        }

        public void Fetch(PortFolio portfolio, string pattern)
        {
            Lines = PortfolioLineRepository.GetFromPortFolio(portfolio, pattern);
        }

        public void FetchMonetaryAccountBalances(PortFolio portfolio)
        {
            MonetaryAccountBalances = MonetaryAccountRepository.GetBalancesFromPortfolio(portfolio.ID);
        }

        public void FetchCash(PortFolio portFolio)
        {
            Cash = PortfolioLineRepository.GetCashFromPortfolio(portFolio);
        }

        public void Delete(PortFolio portfolio, PortFolioLine line, string pattern)
        {
            PortfolioLineRepository.Delete(line);
            Fetch(portfolio, pattern);
        }

        public void DeleteMonetaryAccount(MonetaryAccount account, PortFolio portfolio)
        {
            MonetaryAccountRepository.Delete(account);
            FetchMonetaryAccountBalances(portfolio);
        }
    }
}
