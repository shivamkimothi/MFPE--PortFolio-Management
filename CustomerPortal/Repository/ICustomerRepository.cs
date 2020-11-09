using CustomerPortal.Models;
using CustomerPortal.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerPortal.Repository
{
    public interface ICustomerRepository
    {

        public Task<CompleteDetails> showPortFolio(int id);

        public Task<AssetSaleResponse> SellStocks(int id,StockDetails stockDetails);

        public Task<AssetSaleResponse> sellMutualFunds(int id,MutualFundDetails mutualFundDetails);
    }
}
