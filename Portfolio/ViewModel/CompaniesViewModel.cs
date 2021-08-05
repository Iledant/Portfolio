using Portfolio.Models;
using Portfolio.Repositories;
using System.Collections.Generic;

namespace Portfolio.ViewModel
{
    public class CompaniesViewModel : Bindable
    {
        private List<Company> _companies;

        public List<Company> Companies
        {
            get => _companies;
            set
            {
                _companies = value;
                OnPropertyChanged(nameof(Companies));
            }
        }

        public void Fetch(string pattern)
        {
            Companies = CompanyRepository.Get(pattern);
        }

        public void Delete(Company company, string pattern)
        {
            CompanyRepository.Delete(company);
            Fetch(pattern);
        }
    }
}
