using Portfolio.Models;
using Portfolio.Repositories;
using System.Collections.Generic;

namespace Portfolio.ViewModel
{
    public class PortFoliosViewModel : Bindable
    {
        private List<PortFolio> _portFolioLines;

        public List<PortFolio> PortFolios
        {
            get => _portFolioLines;
            set
            {
                _portFolioLines = value;
                OnPropertyChanged(nameof(PortFolios));
            }
        }

        public void Fetch(string pattern)
        {
            PortFolios = PortfolioRepository.Get(pattern);
        }

        public void Delete(PortFolio portfolio, string pattern)
        {
            PortfolioRepository.Delete(portfolio);
            Fetch(pattern);
        }
    }
}
