using Portfolio.Models;
using Portfolio.Repositories;
using System.Collections.Generic;

namespace Portfolio.ViewModel
{
    public class FundsViewModel : Bindable
    {
        private List<Fund> _funds;

        public List<Fund> Funds
        {
            get => _funds;
            set
            {
                _funds = value;
                OnPropertyChanged(nameof(Funds));
            }
        }

        public void Fetch(string pattern)
        {
            Funds = FundRepository.Get(pattern);
        }

        public void Delete(Fund Fund, string pattern)
        {
            FundRepository.Delete(Fund);
            Fetch(pattern);
        }
    }
}
