using CustomerPortal.Models;
using CustomerPortal.Models.ViewModel;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CustomerPortal.Repository
{
    public class CustomerRepository : ICustomerRepository
    {
        IConfiguration _configuration;
        static readonly log4net.ILog _log4net = log4net.LogManager.GetLogger(typeof(CustomerRepository));
        public CustomerRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        /// <summary>
        /// Takes the details of mutual funds you want to sell and then sends a sale response method back
        /// </summary>
        /// <param name="portfolioid"></param>
        /// <param name="mutualFundDetails"></param>
        /// <returns></returns>
        public async Task<AssetSaleResponse> sellMutualFunds(int portfolioid,MutualFundDetails mutualFundDetails)
        {
            try
            {
                var fetchPortFolio = _configuration["FetchingPortFolioById"];
                var fetchNetWorth = _configuration["FetchingNetWorthByPortFolio"];
                var fetchResponse = _configuration["FetchingSellDetails"];
                PortFolioDetails current = new PortFolioDetails();
                PortFolioDetails toSell = new PortFolioDetails();
                _log4net.Info("Selling mutual fund" + mutualFundDetails.MutualFundName + " of user with id:" + portfolioid);
                using (var client = new HttpClient())
                {
                    using (var response = await client.GetAsync(fetchPortFolio + portfolioid))
                    {
                        _log4net.Info("Fetching the portFolio of user with portFolio ID = " + portfolioid + " from " + nameof(sellMutualFunds));
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        current = JsonConvert.DeserializeObject<PortFolioDetails>(apiResponse);
                    }
                }
                toSell.PortFolioId = portfolioid;
                toSell.MutualFundList = new List<MutualFundDetails>
                        {
                            mutualFundDetails
                        };
                toSell.StockList = new List<StockDetails>();

                List<PortFolioDetails> list = new List<PortFolioDetails>
                        {
                            current,
                            toSell
                        };

                AssetSaleResponse assetSaleResponse = new AssetSaleResponse();
                StringContent content = new StringContent(JsonConvert.SerializeObject(list), Encoding.UTF8, "application/json");
                using (var client = new HttpClient())
                {
                    using (var response = await client.PostAsync(fetchResponse, content))
                    {
                        _log4net.Info("Fetching response of the sale of mutual fund done by the user " + portfolioid);
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        assetSaleResponse = JsonConvert.DeserializeObject<AssetSaleResponse>(apiResponse);
                        _log4net.Info("The response is " + JsonConvert.SerializeObject(assetSaleResponse));
                    }
                }
                return assetSaleResponse;
            }
            catch(Exception ex)
            {
                _log4net.Info("An exception occured " + ex.Message);
                return null;
            }
            
        }


        /// <summary>
        /// Takes as input the stock you want to sell and sends a sale response object back
        /// </summary>
        /// <param name="portfolioid"></param>
        /// <param name="stockdetails"></param>
        /// <returns></returns>
        public async Task<AssetSaleResponse> SellStocks(int portfolioid,StockDetails stockdetails)
        {
            try
            {
                var fetchPortFolio = _configuration["FetchingPortFolioById"];
                var fetchNetWorth = _configuration["FetchingNetWorthByPortFolio"];
                var fetchResponse = _configuration["FetchingSellDetails"];
                PortFolioDetails current = new PortFolioDetails();
                PortFolioDetails toSell = new PortFolioDetails();
                _log4net.Info("Selling Stock " + stockdetails.StockName + " of user  with id = " + portfolioid);
                using (var client = new HttpClient())
                {
                    using (var response = await client.GetAsync(fetchPortFolio + portfolioid))
                    {
                        _log4net.Info("Fetching his portFolio");
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        current = JsonConvert.DeserializeObject<PortFolioDetails>(apiResponse);
                        _log4net.Info("The portfolio " + JsonConvert.SerializeObject(current));
                    }
                }
                toSell.PortFolioId = portfolioid;
                toSell.StockList = new List<StockDetails>
                        {
                            stockdetails
                        };
                toSell.MutualFundList = new List<MutualFundDetails>() { };

                List<PortFolioDetails> list = new List<PortFolioDetails>
                        {
                            current,
                            toSell
                        };

                AssetSaleResponse assetSaleResponse = new AssetSaleResponse();
                StringContent content = new StringContent(JsonConvert.SerializeObject(list), Encoding.UTF8, "application/json");
                using (var client = new HttpClient())
                {
                    using (var response = await client.PostAsync(fetchResponse, content))
                    {
                        _log4net.Info("Fetching the response of sale of stock " + stockdetails.StockName + " of user with id " + portfolioid);
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        assetSaleResponse = JsonConvert.DeserializeObject<AssetSaleResponse>(apiResponse);
                        _log4net.Info("The response is " + JsonConvert.SerializeObject(assetSaleResponse));
                    }
                }
                return assetSaleResponse;
            }
            catch(Exception ex)
            {
                _log4net.Info("An exception occured:" + ex.Message);
                return null;
            }
        }


        /// <summary>
        /// Takes as input the ID and sends the Portfolio with that ID back
        /// </summary>
        /// <param name="portfolioid"></param>
        /// <returns></returns>
        public async Task<CompleteDetails> showPortFolio(int portfolioid)
        {
            try
            {
                CompleteDetails completeDetails = new CompleteDetails();
                PortFolioDetails portFolioDetails = new PortFolioDetails();
                NetWorth _netWorth = new NetWorth();

                var fetchPortFolio = _configuration["FetchingPortFolioById"];
                var fetchNetWorth = _configuration["FetchingNetWorthByPortFolio"];
                var fetchStock = _configuration["FetchStock"];
                var fetchMutualFund = _configuration["FetchMutualFund"];

                using (var client = new HttpClient())
                {
                    using (var response = await client.GetAsync(fetchPortFolio + portfolioid))
                    {
                        _log4net.Info("Calling the Calculate Networth Api for id" + portfolioid);
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        portFolioDetails = JsonConvert.DeserializeObject<PortFolioDetails>(apiResponse);
                    }
                }
                StringContent content = new StringContent(JsonConvert.SerializeObject(portFolioDetails), Encoding.UTF8, "application/json");
                using (var client = new HttpClient())
                {
                    using (var response = await client.PostAsync(fetchNetWorth, content))
                    {
                        _log4net.Info("Calling the Networth api to return the networth of sent portfolio: " + content);
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        _netWorth = JsonConvert.DeserializeObject<NetWorth>(apiResponse);
                        _log4net.Info(" The networth is " + JsonConvert.SerializeObject(_netWorth));

                    }
                }
                completeDetails.PFId = portFolioDetails.PortFolioId;
                completeDetails.FinalMutualFundList = new List<CompleteMutualFundDetails>();
                completeDetails.FinalStockList = new List<CompleteStockDetails>();
                Stock stock = new Stock();
                MutualFundViewModel mutualFundViewModel = new MutualFundViewModel();
                foreach (StockDetails stockDetails in portFolioDetails.StockList)
                {
                    using (var client = new HttpClient())
                    {
                        using (var response = await client.GetAsync(fetchStock + stockDetails.StockName))
                        {
                            _log4net.Info("Calling the StockPriceApi from:" + nameof(Index) + " for fetching the details of stock" + stockDetails.StockName);
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            stock = JsonConvert.DeserializeObject<Stock>(apiResponse);
                            _log4net.Info("the stock details are " + JsonConvert.SerializeObject(stock));
                        }

                        CompleteStockDetails completeStockDetails = new CompleteStockDetails();
                        completeStockDetails.StockName = stockDetails.StockName;
                        completeStockDetails.StockCount = stockDetails.StockCount;
                        completeStockDetails.StockPrice = stock.StockValue;

                        completeDetails.FinalStockList.Add(completeStockDetails);
                    }

                }

                foreach (MutualFundDetails mutualFundDetails in portFolioDetails.MutualFundList)
                {
                    using (var client = new HttpClient())
                    {
                        using (var response = await client.GetAsync(fetchMutualFund + mutualFundDetails.MutualFundName))
                        {
                            _log4net.Info("Calling the MutualFundPriceApi from:" + nameof(Index) + " for fetching the details of the mutual fund:" + mutualFundDetails.MutualFundName);
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            mutualFundViewModel = JsonConvert.DeserializeObject<MutualFundViewModel>(apiResponse);
                            _log4net.Info("The mutual fund details are " + JsonConvert.SerializeObject(mutualFundViewModel));
                        }

                        CompleteMutualFundDetails completeMutualFundDetails = new CompleteMutualFundDetails();
                        completeMutualFundDetails.MutualFundName = mutualFundDetails.MutualFundName;
                        completeMutualFundDetails.MutualFundUnits = mutualFundDetails.MutualFundUnits;
                        completeMutualFundDetails.MutualFundPrice = mutualFundViewModel.MutualFundValue;

                        completeDetails.FinalMutualFundList.Add(completeMutualFundDetails);
                    }

                }
                completeDetails.NetWorth = _netWorth.networth;
                return completeDetails;
            }
            catch(Exception ex)
            {
                _log4net.Info("An exception occured "+ex.Message);
                return null;
            }
        }
    }
}
