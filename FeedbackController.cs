using FlowerAura.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowerAura.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly FloralAuraContext _context;

        public FeedbackController(FloralAuraContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var feedbackList = _context.Feedbacks.Include(f => f.Registration).ToList();
            return View(feedbackList);
        }

        public IActionResult Create()
        {
           
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "User");
            }

            var user = _context.Registrations.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            var feedbackModel = new Feedback
            {
                Name = user.Name,
                Email = user.Email
            };

            return View(feedbackModel);
        }

        [HttpPost]
        public IActionResult Create(Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Registrations.FirstOrDefault(u => u.Email == feedback.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "User not found. Please register first.");
                    return View(feedback);
                }

                feedback.UserId = user.UserId;

                _context.Feedbacks.Add(feedback);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Feedback submitted successfully!";
                return RedirectToAction("Home", "Home");
            }
            return View(feedback);
        }
        public IActionResult Details(int id)
        {
            var feedback = _context.Feedbacks
                .Include(f => f.Registration)
                .FirstOrDefault(f => f.UserId == id);

            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        public IActionResult Delete(int id)
        {
            return View(_context.Feedbacks.Find(id));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _context.Feedbacks.Remove(_context.Feedbacks.Find(id));
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}