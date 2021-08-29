using Portfolio.Models;
using System.Collections.Generic;

namespace Portfolio.ViewModel
{
    public class FundsLookUpViewModel : Bindable
    {
        private PortFolio _portfolio;
        private List<FundLookUp> _funds;

        public PortFolio PortFolio
        {
            get => _portfolio;
            set
            {
                _portfolio = value;
                OnPropertyChanged(nameof(PortFolio));
            }
        }

        public List<FundLookUp> Funds
        {
            get => _funds;
            set
            {
                _funds = value;
                OnPropertyChanged(nameof(Funds));
            }
        }

        public void FetchFunds()
        {

        }
    }
}
