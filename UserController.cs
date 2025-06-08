using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FlowerAura.Models;

namespace FlowerAura.Controllers
{
    public class UserController : Controller
    {
        private readonly FloralAuraContext _context;

        public UserController(FloralAuraContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            string trimmedEmail = email?.Trim();
            string trimmedPassword = password?.Trim();

            if (string.IsNullOrEmpty(trimmedEmail) || string.IsNullOrEmpty(trimmedPassword))
            {
                ModelState.AddModelError("", "Email and Password are required.");
                return View();
            }

            
            string adminEmail = "admin@floweraura.com";
            string adminPassword = "Admin@123";

            if (trimmedEmail == adminEmail && trimmedPassword == adminPassword)
            {
                HttpContext.Session.SetString("UserEmail", trimmedEmail);
                HttpContext.Session.SetString("UserRole", "Admin");
                TempData["SuccessMessage"] = "Admin Login Successful!";
                return RedirectToAction("Dashboard", "Admin");
            }

            
            var existingUser = _context.Registrations
                .FirstOrDefault(u => u.Email == trimmedEmail && u.Password == trimmedPassword);

            if (existingUser != null)
            {
                HttpContext.Session.SetString("UserEmail", existingUser.Email);
                HttpContext.Session.SetString("UserRole", "User");
                return RedirectToAction("Home", "Home");
            }

            ModelState.AddModelError("", "Invalid Email or Password.");
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Email is required.");
                return View();
            }

            var user = _context.Registrations.FirstOrDefault(u => u.Email == email.Trim());
            if (user == null)
            {
                ModelState.AddModelError("", "Email not found.");
                return View();
            }

            int otp = new Random().Next(100000, 999999);
            HttpContext.Session.SetString("ResetEmail", email);
            HttpContext.Session.SetString("ResetOtp", otp.ToString());

            SendOtpEmail(email, otp);
            return RedirectToAction("VerifyOtp", new { email });
        }

        public IActionResult VerifyOtp(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public IActionResult VerifyOtp(string email, string otp)
        {
            var storedEmail = HttpContext.Session.GetString("ResetEmail");
            var storedOtp = HttpContext.Session.GetString("ResetOtp");

            if (storedEmail == email && storedOtp == otp)
            {
                HttpContext.Session.SetString("OtpVerifiedEmail", email);
                return RedirectToAction("ResetPassword", new { email });
            }

            ModelState.AddModelError("", "Invalid OTP.");
            ViewBag.Email = email;
            return View();
        }

        public IActionResult ResetPassword(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string email, string newPassword)
        {
            var verifiedEmail = HttpContext.Session.GetString("OtpVerifiedEmail");
            if (verifiedEmail != email)
            {
                TempData["ErrorMessage"] = "Unauthorized password reset attempt.";
                return RedirectToAction("ForgotPassword");
            }

            var user = _context.Registrations.FirstOrDefault(u => u.Email == email.Trim());
            if (user != null)
            {
                user.Password = newPassword?.Trim(); 
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Password reset successfully!";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", "User not found.");
            ViewBag.Email = email;
            return View();
        }

       
        private void SendOtpEmail(string toEmail, int otp)
        {
            string fromEmail = "22bmiit040@gmail.com";
            string fromPassword = "pljm gebg tiym hllb";

            MailMessage message = new MailMessage();
            message.From = new MailAddress(fromEmail);
            message.To.Add(toEmail);
            message.Subject = "Your OTP for Password Reset";
            message.Body = $"Your OTP is: {otp}";
            message.IsBodyHtml = false;

            using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.Credentials = new NetworkCredential(fromEmail, fromPassword);
                smtp.EnableSsl = true;
                smtp.Send(message);
            }
        }
        
        public IActionResult UpdateProfile()
        {
            string? email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }

            var user = _context.Registrations.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }

       
        [HttpPost]
        public IActionResult UpdateProfile(Registration model)
        {
            
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.Remove("Password");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            
            var user = _context.Registrations.FirstOrDefault(u => u.UserId == model.UserId);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            user.Name = model.Name;
            user.Email = model.Email.Trim(); 
            user.Address = model.Address;
            user.Mobilenumber = model.Mobilenumber;
            user.Gender = model.Gender;

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.Password = model.Password;
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Password updated. Please login again.";
                HttpContext.Session.Clear(); 
                return RedirectToAction("Login");
            }

            _context.SaveChanges();

           
            HttpContext.Session.SetString("UserEmail", user.Email);

            TempData["SuccessMessage"] = "Profile updated!";
            return RedirectToAction("Home", "Home");
        }


    }
}