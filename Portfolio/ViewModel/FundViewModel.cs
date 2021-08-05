using Portfolio.Models;
using Portfolio.Repositories;
using System.Collections.Generic;

namespace Portfolio.ViewModel
{
    public class FundViewModel : Bindable
    {
        private List<FundData> _fundDatas;

        public List<FundData> FundDatas
        {
            get => _fundDatas;
            set
            {
                _fundDatas = value;
                OnPropertyChanged(nameof(FundDatas));
            }
        }

        public void FetchFundDatas(int fundID)
        {
            FundDatas = FundRepository.GetFundDatas(fundID);
        }
    }
}
