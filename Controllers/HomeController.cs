using Microsoft.AspNetCore.Mvc;
using BlockchainTest.Interfaces;
using BlockchainTest.Models;
using System.Collections.Generic;

namespace BlockchainTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly BlockchainService _blockchainService;

        public HomeController(BlockchainService blockchainService)
        {
            _blockchainService = blockchainService;
        }

        public IActionResult Index()
        {
            return View(_blockchainService.Blockchain);
        }

        [HttpPost]
        public IActionResult AddTransaction(Transaction model)
        {
            if (ModelState.IsValid)
            {
                _blockchainService.AddBlock(new List<ITransaction> { model });
                return RedirectToAction("Index");
            }
            return View("Index", _blockchainService.Blockchain);
        }
    }
}