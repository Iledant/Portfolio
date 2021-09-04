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

        public void Fetch(PortFolio portfolio, string pattern)
        {
            (Lines, Cash) = PortfolioLineRepository.GetFromPortFolio(portfolio, pattern);

        }

        public void Delete(PortFolio portfolio, PortFolioLine line, string pattern)
        {
            PortfolioLineRepository.Delete(line);
            Fetch(portfolio, pattern);
        }
    }
}
