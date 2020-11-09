using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CustomerPortal.Models;
using CustomerPortal.Models.ViewModel;
using CustomerPortal.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CustomerPortal.Controllers
{
    public class PortFolioController : Controller
    {
        static readonly log4net.ILog _log4net = log4net.LogManager.GetLogger(typeof(PortFolioController));
        private ISaleRepository _saleRepository;
        private ICustomerRepository _customerRepository;
        

        /// <summary>
        /// Injecting the SaleRepository here
        /// </summary>
        /// <param name="saleRepository"></param>
        public PortFolioController(ISaleRepository saleRepository, ICustomerRepository customerRepository)
        {
            _saleRepository = saleRepository;
            _customerRepository = customerRepository;
        }


        /// <summary>
        /// This method logs out the user and clears the session
        /// </summary>
        /// <returns></returns>
        public IActionResult Logout()
        {
            try
            {
                string portfolioid = HttpContext.Session.GetString("Id");
                _log4net.Info(" User with Id = " + portfolioid + " is logging out");
                HttpContext.Session.Clear();
            }
            catch(Exception ex)
            {
                _log4net.Error("An exception occured while logging out the employee:"+ex.Message);
                return new StatusCodeResult(500);
            }
            return RedirectToAction("Login", "Home");
        } 


        /// <summary>
        /// This method checks whether the session token is null or not. Returns true if not, meaning that user is validated
        /// and returns false if it is nul, meaning the user is not validated and won't be procedded further
        /// </summary>
        /// <returns></returns>
        public bool CheckValid()
        {
            try
            {
                if (HttpContext.Session.GetString("JWTtoken") != null)
                {
                    _log4net.Info(" User with Id = " + HttpContext.Session.GetString("Id") + " is a valid user");
                    return true;
                }
            }
            catch(Exception ex)
            {
                _log4net.Error("Exception occured whille checking the validity of th token of user with id = " + HttpContext.Session.GetString("Id")+"The exception is:"+ex.Message);
            }
            return false;
        }


        /// <summary>
        /// This method shows the portfolio details of the user.
        /// </summary>
        /// <returns></returns>
        // GET: PortFolioController
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                NetWorth _netWorth = new NetWorth();
                if (CheckValid())
                {
                    int portfolioid = Convert.ToInt32(HttpContext.Session.GetString("Id"));
                    _log4net.Info("Showing the user with portfolio ID = " + portfolioid + "his networth and the assets he is currently holding");
                    CompleteDetails completeDetails = await _customerRepository.showPortFolio(portfolioid); 
                    return View(completeDetails);
                }
            }
            catch (Exception ex)
            {
                _log4net.Error("An exception occured while showing the portfolio details to the user. the message is :"+ex.Message);
            }
            return RedirectToAction("Index", "Home");
        }


        /// <summary>
        /// this method is invoked when user wants to sell a stock. It then returns a view asking them for how many
        /// stocks they want to sell.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IActionResult SellStock(string name)
        {
            try
            {
                StockDetails stockdetail = new StockDetails();
                stockdetail.StockName = name;
                return View(stockdetail);
            }
            catch(Exception ex)
            {
                _log4net.Error("An exception occured in the" + nameof(SellStock) + " while selling stocks. The message is:" +ex.Message);
                return RedirectToAction("Index", "Home");
            }

        }

        /// <summary>
        /// this method is invoked when user wants to sell a mutual Fund. It then returns a view asking them for how many
        /// mutual fund they want to Sell.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IActionResult SellMutualFund(string name)
        {
            try
            {
                MutualFundDetails mutualdetail = new MutualFundDetails();
                mutualdetail.MutualFundName = name;
                return View(mutualdetail);
            }
            catch (Exception ex)
            {
                _log4net.Error("An exception occured in the" + nameof(SellMutualFund) + " while selling MutualFunds. The message is:" + ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }


        /// <summary>
        /// This method takes the details of the stocks the user wants to sell. 
        /// It then reduces the networth and the the units of stocks from his portfolio
        /// </summary>
        /// <param name="stockdetails"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SellStock(StockDetails stockdetails)
        {
            try
            {
                if (CheckValid())
                {
                    int portfolioid = Convert.ToInt32(HttpContext.Session.GetString("Id"));
                    _log4net.Info("Selling the stocks of user with id = " + portfolioid);

                    AssetSaleResponse assetSaleResponse = await _customerRepository.SellStocks(portfolioid, stockdetails);
                    _log4net.Info("sale of stock of user with id" + portfolioid + "done");
                    Sale _sale = new Sale();
                    _sale.PortFolioID = portfolioid;
                    _sale.NetWorth = assetSaleResponse.Networth;
                    _sale.status = assetSaleResponse.SaleStatus;
                    _saleRepository.Add(_sale);

                    return View("Reciept", assetSaleResponse);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }

            }
            catch (Exception ex)
            {
                _log4net.Error("An exception occured in the" + nameof(SellStock) + " while selling mutualFund. The message is:" + ex.Message);
                return RedirectToAction("Index", "Home");
            }

        }


        /// <summary>
        /// This method takes the details of the mutual fund the user wants to sell. 
        /// It then reduces the networth and the the units of mutual funds from his portfolio
        /// </summary>
        /// <param name="mutualFundDetails"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SellMutualFund(MutualFundDetails mutualFundDetails)
        {
            try{
                if (CheckValid())
                {
                    int portfolioid = Convert.ToInt32(HttpContext.Session.GetString("Id"));
                    _log4net.Info("Selling mutual fund" + mutualFundDetails.MutualFundName + " of user with id:" + portfolioid);
                    AssetSaleResponse assetSaleResponse = await _customerRepository.sellMutualFunds(portfolioid,mutualFundDetails);
                    _log4net.Info("sale of  mutual fund of user with id" + portfolioid + "done");
                    Sale _sale = new Sale();
                    _sale.PortFolioID = portfolioid;
                    _sale.NetWorth = assetSaleResponse.Networth;
                    _sale.status = assetSaleResponse.SaleStatus;
                    _saleRepository.Add(_sale);

                    return View("Reciept", assetSaleResponse);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                _log4net.Error("An exception occured in the" + nameof(SellStock) + " while selling mutualFund. The message is:" + ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

    }
}
