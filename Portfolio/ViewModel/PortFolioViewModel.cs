using Portfolio.Models;
using Portfolio.Repositories;
using System;
using System.Collections.Generic;

namespace Portfolio.ViewModel
{
    public class PortFolioViewModel : Bindable
    {
        private List<PortFolioLine> _lines;

        public List<PortFolioLine> Lines
        {
            get => _lines;
            set
            {
                _lines = value;
                OnPropertyChanged(nameof(Lines));
            }
        }

        public void Fetch(PortFolio portfolio, string pattern)
        {
            Lines = PortfolioLineRepository.GetFromPortFolio(portfolio, pattern);
        }

        public void Delete(PortFolio portfolio, PortFolioLine line, string pattern)
        {
            PortfolioLineRepository.Delete(line);
            Fetch(portfolio, pattern);
        }
    }
}
