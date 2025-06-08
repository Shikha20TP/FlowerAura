using System.Data.Entity;
using FlowerAura.Models;
using Microsoft.AspNetCore.Mvc;

namespace FlowerAura.Controllers
{
    public class AdminController : Controller
    {
        private readonly FloralAuraContext _context;
        private readonly object Session;

        public AdminController(FloralAuraContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("Login", "User");
            }

           
            int customerCount = _context.Registrations.Count();

           
            ViewBag.CustomerCount = customerCount;

            int feedbackCount = _context.Feedbacks.Count();

            
            ViewBag.FeedbackCount = feedbackCount;

            int flowerCount = _context.Flowers.Count();

            
            ViewBag.FlowerCount = flowerCount;

            return View();
        }
        public IActionResult Index()
        {
            var flowers = _context.Flowers.ToList();
            return View(flowers);
        }
        

    }
}