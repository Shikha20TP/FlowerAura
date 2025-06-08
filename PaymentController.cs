using Microsoft.AspNetCore.Mvc;
using Razorpay.Api;
using System.Security.Cryptography;
using System.Text;
using FlowerAura.Helpers;
using FlowerAura.Models;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;



namespace FlowerAura.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly FloralAuraContext _context;

        public PaymentController(IConfiguration configuration, FloralAuraContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public IActionResult Proceed()
        {
            return View("Checkout");
        }
        [Authorize]
        [HttpPost]
        public IActionResult Checkout(decimal amount)
        {
            
            ViewBag.Amount = amount;
            return View();
        }


        [HttpPost]
        public IActionResult CreateOrder(string amount)
        {
            
            int amountInPaise = int.Parse(amount) * 100;

            RazorpayClient client = new RazorpayClient(
                _configuration["Razorpay:Key"],
                _configuration["Razorpay:Secret"]
            );

            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("amount", amountInPaise);
            options.Add("currency", "INR");
            options.Add("receipt", "order_rcptid_11");
            options.Add("payment_capture", 1);

            Order order = client.Order.Create(options);
            ViewBag.OrderId = order["id"].ToString();
            ViewBag.Key = _configuration["Razorpay:Key"];
            ViewBag.Amount = amountInPaise;

            return View("Checkout");
        }
        [HttpPost]
        public IActionResult PaymentCallback(string razorpay_payment_id, string razorpay_order_id, string razorpay_signature)
        {
            string secret = _configuration["Razorpay:Secret"];
            string generatedSignature = Helpers.Utils.GetHash(razorpay_order_id + "|" + razorpay_payment_id, secret);

            if (generatedSignature == razorpay_signature)
            {
                
                TempData["PaymentId"] = razorpay_payment_id;
                TempData["OrderId"] = razorpay_order_id;
                TempData["Message"] = "Payment successful!";

                
                return RedirectToAction("home","Home");
            }
            else
            {
                ViewBag.Message = "Payment verification failed!";
                return View("Checkout");
            }
        }
        [HttpPost]
        public IActionResult GenerateBill(string FlowerName, string Category, string Amount, int Quantity)
        {
            
            int? userId = HttpContext.Session.GetInt32("UserId");
            Console.WriteLine("DEBUG >> Logged in UserId: " + userId);

            
            if (userId == null)
            {
                TempData["Error"] = "Please login to generate a bill.";
                return RedirectToAction("Login", "Account");
            }

            
            var user = _context.Registrations.FirstOrDefault(u => u.UserId == userId.Value);
            Console.WriteLine("DEBUG >> Found user: " + user?.Name);

            
            if (user == null)
            {
                TempData["Error"] = "User not found. Please login again.";
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





    }
}
