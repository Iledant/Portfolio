using Portfolio.Models;
using Portfolio.Repositories;
using System.Collections.Generic;

namespace Portfolio.ViewModel
{
    public class PortFolioLineViewModel : Bindable
    {
        private List<PortFolioLine> _portFolioLines;

        public List<PortFolioLine> PortFolioLines
        {
            get => _portFolioLines;
            set
            {
                _portFolioLines = value;
                OnPropertyChanged(nameof(PortFolioLines));
            }
        }

        public void Fetch(string pattern)
        {
            PortFolioLines = PortfolioLineRepository.Get(pattern);
        }

        public void Delete(PortFolioLine portFolioLine, string pattern)
        {
            PortfolioLineRepository.Delete(portFolioLine);
            Fetch(pattern);
        }
    }
}
