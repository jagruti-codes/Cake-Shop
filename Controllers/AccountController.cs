

using System.Text.Json;
using cake_shop.Data;
using cake_shop.Models;
using cake_shop.Services;
using Microsoft.AspNetCore.Mvc;

namespace cake_shop.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private const string SessionKey = "UserProfile";

        public AccountController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // ================= REGISTER =================
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (_context.Users.Any(x => x.Email == model.Email))
            {
                ModelState.AddModelError("", "Email already exists");
                return View(model);
            }

            var otp = GenerateOTP();

            var user = new AppUser
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                OTP = otp,
                OTPExpiry = DateTime.Now.AddMinutes(5),
                IsEmailVerified = false
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            SendOTPEmail(user.Email, otp);

            TempData["Email"] = user.Email;
            return RedirectToAction("VerifyOTP");
        }

        // ================= VERIFY OTP =================
        public IActionResult VerifyOTP()
        {
            ViewBag.Email = TempData["Email"];
            return View();
        }

        [HttpPost]
        public IActionResult VerifyOTP(string email, string otp)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == email);

            if (user == null) return Content("User not found");
            if (user.OTP != otp) return Content("Invalid OTP");
            if (user.OTPExpiry < DateTime.Now) return Content("OTP expired");

            user.IsEmailVerified = true;
            user.OTP = null;
            user.OTPExpiry = null;

            _context.SaveChanges();

            TempData["Message"] = "Email verified successfully!";
            return RedirectToAction("Login");
        }

        // ================= RESEND OTP =================
        public IActionResult ResendOTP(string email)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == email);
            if (user == null) return Content("User not found");

            var otp = GenerateOTP();

            user.OTP = otp;
            user.OTPExpiry = DateTime.Now.AddMinutes(5);

            _context.SaveChanges();

            SendOTPEmail(user.Email, otp);

            TempData["Email"] = user.Email;
            TempData["Message"] = "OTP resent successfully";

            return RedirectToAction("VerifyOTP");
        }

        // ================= LOGIN =================
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 🔍 Get user from database
            var user = _context.Users.FirstOrDefault(x => x.Email == model.Email);

            // ❌ Invalid login
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid credentials");
                return View(model);
            }

            // Email verification only for normal users
            if (user.Role == "User" && !user.IsEmailVerified)
            {
                TempData["Email"] = user.Email;
                return RedirectToAction("VerifyOTP");
            }

          
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("UserName", user.Name); 

            // Redirect based on role
            if (user.Role == "Admin")
                return RedirectToAction("Dashboard", "Admin");

            return RedirectToAction("Index", "Home");
        }


       

        // ================= FORGOT PASSWORD =================
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == email);
            if (user == null) return Content("Email not found");

            var otp = GenerateOTP();

            user.OTP = otp;
            user.OTPExpiry = DateTime.Now.AddMinutes(5);

            _context.SaveChanges();

            SendOTPEmail(user.Email, otp);

            TempData["Email"] = user.Email;
            return RedirectToAction("ResetPassword");
        }

        // ================= RESET PASSWORD =================
        public IActionResult ResetPassword()
        {
            ViewBag.Email = TempData["Email"];
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string email, string otp, string newPassword)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == email);

            if (user == null) return Content("User not found");
            if (user.OTP != otp) return Content("Invalid OTP");
            if (user.OTPExpiry < DateTime.Now) return Content("OTP expired");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.OTP = null;
            user.OTPExpiry = null;

            _context.SaveChanges();

            TempData["Message"] = "Password reset successful!";
            return RedirectToAction("Login");
        }

        // ================= HELPERS =================
        private string GenerateOTP()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        private void SendOTPEmail(string email, string otp)
        {
            _emailService.SendEmail(email, "OTP Verification", $"Your OTP is: {otp}");
        }



        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
   


    // ================= CHANGE PASSWORD =================
public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(x => x.Id == userId);

            if (user == null)
            {
                TempData["Error"] = "User not found";
                return View();
            }

            // check old password
            if (!BCrypt.Net.BCrypt.Verify(CurrentPassword, user.PasswordHash))
            {
                TempData["Error"] = "Current password is incorrect";
                return View();
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "New password and confirm password do not match";
                return View();
            }

            // update password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);

            _context.SaveChanges();

            TempData["Message"] = "Password changed successfully!";
            return View();
        }


    }
}
