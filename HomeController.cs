using System.Diagnostics;
using FlowerAura.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions; 
using System.Globalization;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Globalization;

namespace FlowerAura.Controllers
{
    public class HomeController : Controller
    {
        private readonly FloralAuraContext _context;
        private readonly object Session;

        public HomeController(FloralAuraContext context)
        {
            _context = context;
        }

        public IActionResult Home()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail")))
            {
                return RedirectToAction("Login", "User");
            }

            var flowers = _context.Flowers
                .Select(f => new FlowerViewModel
                {
                    Id = f.Id, 
                    FlowerName = f.FlowerName,
                    Image = f.Image,
                    Amount = f.Amount
                })
                .ToList();

            return View(flowers);
        }



        public IActionResult Welcome()
        {
            return View();
        }

        public IActionResult Index()
        {
            var registrations = _context.Registrations.ToList();
            return View(registrations);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Registration registration)
        {
            try
            {
                if (_context.Registrations.Any(u => u.Email == registration.Email))
                {
                    ModelState.AddModelError("Email", "Email is already registered.");
                }

                if (!ModelState.IsValid)
                {
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                    return View(registration);
                }

                _context.Registrations.Add(registration);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Registration Successful!";
                return RedirectToAction("Login", "User");

            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"DbUpdateException: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                ModelState.AddModelError("", "An error occurred while saving. Try again.");
                return View(registration);
            }
        }

        public IActionResult Details(int id)
        {
            var registration = _context.Registrations.Find(id);
            if (registration == null)
            {
                return NotFound();
            }
            return View(registration);
        }

        
       

        public IActionResult Delete(int id)
        {
            var registration = _context.Registrations.Find(id);
            if (registration == null)
            {
                return NotFound();
            }
            return View(registration);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var registration = _context.Registrations.Find(id);
            if (registration != null)
            {
                _context.Registrations.Remove(registration);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        public IActionResult Display(int id)
        {
            var flower = _context.Flowers
                .Where(f => f.Id == id)
                .Select(f => new FlowerViewModel
                {
                    Id = f.Id,
                    FlowerName = f.FlowerName,
                    Image = f.Image,
                    Amount = f.Amount,
                    Category = f.Category,
                    Description = f.Description
                })
                .FirstOrDefault();

            if (flower == null)
            {
                return NotFound();
            }

            return View(flower);
        }
        [HttpGet]
        public JsonResult SearchSuggestions(string term)
        {
            var suggestions = _context.Flowers
                .Where(f => f.FlowerName.Contains(term))
                .Select(f => new { label = f.FlowerName, value = f.Id }) 
                .Take(5) 
                .ToList();

            return Json(suggestions);
        }


        public IActionResult About()
        {
            return View();
        }
        public IActionResult BuyNow(int id)
        {
            var flower = _context.Flowers.FirstOrDefault(f => f.Id == id);
            if (flower == null) return NotFound();

            TempData["FlowerId"] = flower.Id;
            TempData["FlowerName"] = flower.FlowerName;
            TempData["Amount"] = flower.Amount; 
            TempData["Category"] = flower.Category;

            return RedirectToAction("EnterQuantity");
        }

        [HttpGet]
        public IActionResult EnterQuantity()
        {
            ViewBag.FlowerId = TempData["FlowerId"];
            ViewBag.FlowerName = TempData["FlowerName"];
            ViewBag.Category = TempData["Category"];
            ViewBag.Amount = TempData["Amount"]; 
            return View();
        }

        [HttpPost]
        public IActionResult GenerateBill(string FlowerName, string Category, string Amount, int Quantity)
        {
            if (Quantity <= 0)
            {
                TempData["Error"] = "Quantity must be greater than 0.";
                return RedirectToAction("EnterQuantity");
            }
            var user = _context.Registrations.FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            
            string numericAmount = new string(Amount.Where(c => char.IsDigit(c) || c == '.').ToArray());
            decimal parsedAmount = decimal.TryParse(numericAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var amt) ? amt : 0;

            decimal totalAmount = parsedAmount * Quantity;

            var bill = new BillViewModel
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Address = user.Address,
                MobileNo = user.Mobilenumber,
                Gender = user.Gender,
                FlowerName = FlowerName,
                Category = Category,
                Quantity = Quantity,
                Amount = totalAmount
            };

            ViewBag.OriginalAmount = Amount;
            return View("Bill", bill);
        }





        public ActionResult Payment(decimal amount)
        {
            ViewBag.Amount = amount;
            return View(); 
        }
      


        

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
