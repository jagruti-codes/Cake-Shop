

using System.Diagnostics;
using System.Text.Json;
using cake_shop.Models;
using cake_shop.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cake_shop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string UserProfileSessionKey = "UserProfile";
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        //CAKES PAGE
       
        public IActionResult Cakes()
        {
            var products = _context.Products
                .Include(p => p.Category)
                .ToList();

            // Approved reviews only
            var reviews = _context.RatingReviews
                .Include(r => r.User)
                .Where(r => r.IsApproved)
                .ToList();

            ViewBag.Reviews = reviews;

            return View(products);
        }



        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpGet]
        public IActionResult Contact()
        {
            var contactInfo = _context.Contacts.FirstOrDefault();
            ViewBag.ContactInfo = contactInfo;

            return View(new ContactMessage());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(ContactMessage model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ContactInfo = _context.Contacts.FirstOrDefault();
                return View(model);
            }

            _context.ContactMessages.Add(model);
            _context.SaveChanges();

            TempData["ContactSuccess"] = "Thank you! Your message has been sent.";

            return RedirectToAction("Contact");
        }

        // ================= WISHLIST =================

        // VIEW WISHLIST
        public IActionResult Wishlist()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var wishlist = _context.Wishlists
                .Include(w => w.Product)
                .Where(w => w.UserId == userId)
                .ToList();

            return View(wishlist);
        }


      
        [HttpPost]
        public IActionResult AddToWishlist([FromBody] JsonElement data)
        {
            int productId = data.GetProperty("productId").GetInt32();

            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return Json(new { success = false, message = "Login required" });

            // CHECK IF ALREADY EXISTS
            bool exists = _context.Wishlists
                .Any(w => w.UserId == userId && w.ProductId == productId);

            if (exists)
            {
                return Json(new { success = false, message = "Product already exists in wishlist" });
            }

            var wishlist = new Wishlist
            {
                UserId = userId.Value,
                ProductId = productId
            };

            _context.Wishlists.Add(wishlist);
            _context.SaveChanges();

            return Json(new { success = true, message = "Added to wishlist" });
        }

        // REMOVE FROM WISHLIST
        public IActionResult RemoveWishlist(int id)
        {
            var item = _context.Wishlists.Find(id);

            if (item != null)
            {
                _context.Wishlists.Remove(item);
                _context.SaveChanges();
            }

            return RedirectToAction("Wishlist");
        }


        // ADD TO CART FROM WISHLIST

        public IActionResult AddToCartFromWishlist(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var wishlistItem = _context.Wishlists
                .Include(w => w.Product)
                .FirstOrDefault(w => w.Id == id && w.UserId == userId);

            if (wishlistItem == null)
                return RedirectToAction("Wishlist");

            // check if item already exists in cart
            var cartItem = _context.Carts
                .FirstOrDefault(c => c.UserId == userId && c.ProductId == wishlistItem.ProductId);

            if (cartItem != null)
            {
                cartItem.Quantity += 1;
            }
            else
            {
                _context.Carts.Add(new Cart
                {
                    UserId = userId.Value,
                    ProductId = wishlistItem.ProductId,
                    Quantity = 1
                });
            }

            // remove from wishlist
            _context.Wishlists.Remove(wishlistItem);

            _context.SaveChanges();

            return RedirectToAction("Cart");
        }
       
        //Cart Functionality 🔹 ADD TO CART (AJAX)

        [HttpPost]
        public IActionResult AddToCart([FromBody] JsonElement data)
        {
            int productId = data.GetProperty("productId").GetInt32();

            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return Json(new { success = false, message = "Login required" });

            var cartItem = _context.Carts
                .FirstOrDefault(c => c.UserId == userId && c.ProductId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity += 1;
            }
            else
            {
                _context.Carts.Add(new Cart
                {
                    UserId = userId.Value,
                    ProductId = productId,
                    Quantity = 1
                });
            }

            _context.SaveChanges();

            return Json(new { success = true, message = "Added to cart" });
        }


        //view cart

        public IActionResult Cart()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var cart = _context.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToList();

            return View(cart);
        }

        //Remove item from cart

        public IActionResult RemoveCart(int id)
        {
            var item = _context.Carts.Find(id);

            if (item != null)
            {
                _context.Carts.Remove(item);
                _context.SaveChanges();
            }

            return RedirectToAction("Cart");
        }

        //Change QUANTITY


        [HttpPost]
        public IActionResult UpdateCart(int id, int qty)
        {
            var item = _context.Carts.Find(id);

            if (item != null && qty > 0)
            {
                item.Quantity = qty;
                _context.SaveChanges();
            }

            return RedirectToAction("Cart");
        }
        //Cancel Order
        public IActionResult CancelOrder(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var order = _context.Orders
                .FirstOrDefault(o => o.Id == id && o.UserId == userId);

            if (order == null)
                return RedirectToAction("OrderHistory");

            if (order.Status != "Cancelled")
            {
                order.Status = "Cancelled";
                _context.SaveChanges();
            }

            return RedirectToAction("OrderHistory");
        }


        //checkout

        [HttpGet]
        public IActionResult Checkout(string ids)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var selectedProductIds = ids?
                .Split(',')
                .Select(int.Parse)
                .ToList();

            var cartQuery = _context.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId);

            if (selectedProductIds != null && selectedProductIds.Any())
            {
                cartQuery = cartQuery.Where(c => selectedProductIds.Contains(c.ProductId));
            }

            var cart = cartQuery.ToList();

            // Buy Now fallback
            if (!cart.Any() && selectedProductIds != null)
            {
                var products = _context.Products
                    .Where(p => selectedProductIds.Contains(p.Id))
                    .ToList();

                cart = products.Select(p => new Cart
                {
                    ProductId = p.Id,
                    Product = p,
                    Quantity = 1
                }).ToList();
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            var model = new CheckoutViewModel
            {
                Name = user?.Name,
                Phone = user?.Phone,
                CartItems = cart,
                TotalAmount = cart.Sum(x => x.Product!.Price * x.Quantity)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(CheckoutViewModel model, string ids)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            var selectedProductIds = ids?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            var cartQuery = _context.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId);

            if (selectedProductIds != null && selectedProductIds.Any())
            {
                cartQuery = cartQuery.Where(c => selectedProductIds.Contains(c.ProductId));
            }

            var cart = cartQuery.ToList();

            // BUY NOW fallback support
            if (!cart.Any() && selectedProductIds != null && selectedProductIds.Any())
            {
                var products = _context.Products
                    .Where(p => selectedProductIds.Contains(p.Id))
                    .ToList();

                cart = products.Select(p => new Cart
                {
                    ProductId = p.Id,
                    Product = p,
                    Quantity = 1,
                    UserId = userId.Value
                }).ToList();
            }

            if (!cart.Any())
                return RedirectToAction("Cart");

            // ================= ORDER =================
            var order = new Order
            {
                UserId = userId.Value,
                Address = model.Address,
                City = model.City,
                Phone = model.Phone,
                PaymentMethod = "COD",
                PaymentStatus = "Pending",
                Status = "Pending",
                TotalAmount = cart.Sum(x => x.Product!.Price * x.Quantity),
                CreatedAt = DateTime.Now,
                Items = new List<OrderItem>()
            };

            foreach (var item in cart)
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product!.Price
                });
            }

            // 1️⃣ SAVE ORDER FIRST
            _context.Orders.Add(order);
            _context.SaveChanges(); // OrderId generated here

            // ================= PAYMENT AUTO CREATE =================
            var payment = new Payment
            {
                OrderId = order.Id,
                CustomerName = model.Name,
                Email = user?.Email,
                Phone = model.Phone,
                Amount = order.TotalAmount,
                Method = "COD",
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.Payments.Add(payment);

            // ================= SHIPPING AUTO CREATE =================
            var shipping = new Shipping
            {
                OrderId = order.Id,
                CustomerName = model.Name,
                Email = user?.Email,
                Phone = model.Phone,
                Destination = model.Address + ", " + model.City,
                Method = "Standard",
                Status = "Pending",
                ShippingCode = "SHP-" + order.Id + "-" + new Random().Next(1000, 9999)
            };

            _context.Shippings.Add(shipping);

            // ================= REMOVE CART ITEMS =================
            var cartItemsToRemove = _context.Carts
                .Where(c => c.UserId == userId &&
                            selectedProductIds.Contains(c.ProductId))
                .ToList();

            _context.Carts.RemoveRange(cartItemsToRemove);

            // 2️⃣ SAVE EVERYTHING
            _context.SaveChanges();

            // SUCCESS PAGE
            return View("OrderSuccess", order);
        }


        [HttpGet]
        public IActionResult OrderSuccess()
        {
            if (TempData["OrderSuccess"] is not string)
            {
                return RedirectToAction(nameof(Checkout));
            }

            return View();
        }
        //order history

      

        public IActionResult OrderHistory()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var orders = _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            // PASS REVIEWS
            var reviews = _context.RatingReviews
                .Where(r => r.UserId == userId)
                .ToList();

            ViewBag.Reviews = reviews;

            return View(orders);
        }


        public IActionResult UserDashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = _context.Users.FirstOrDefault(x => x.Id == userId);

            var orders = _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            ViewBag.Orders = orders;

            return View(user);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UserDashboard(AppUser model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = _context.Users.FirstOrDefault(x => x.Id == userId);

            if (user == null)
                return RedirectToAction("Login", "Account");

            user.Name = model.Name;
            user.Email = model.Email;
            user.Phone = model.Phone;

            _context.SaveChanges();

            TempData["ProfileUpdated"] = "Profile updated successfully!";
            return RedirectToAction("UserDashboard");
        }



        // ================= ADD REVIEW =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddReview(int productId, int rating, string review)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Invalid rating.";
                return RedirectToAction("OrderHistory");
            }

            // check if purchased
            var purchased = _context.Orders
                .Include(o => o.Items)
                .Any(o => o.UserId == userId &&
                          o.Items.Any(i => i.ProductId == productId));

            if (!purchased)
            {
                TempData["Error"] = "You can only review purchased products.";
                return RedirectToAction("OrderHistory");
            }

            // prevent duplicate
            var exists = _context.RatingReviews
                .Any(r => r.UserId == userId && r.ProductId == productId);

            if (exists)
            {
                TempData["Error"] = "Already reviewed.";
                return RedirectToAction("OrderHistory");
            }

            var data = new RatingReview
            {
                UserId = userId.Value,
                ProductId = productId,
                Rating = rating,
                Review = review,
                CreatedAt = DateTime.Now
            };

            _context.RatingReviews.Add(data);
            _context.SaveChanges();

            TempData["Success"] = "Review submitted!";
            return RedirectToAction("OrderHistory");
        }

        // ================= EDIT REVIEW (GET) =================
        public IActionResult EditReview(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");


            var review = _context.RatingReviews
    .FirstOrDefault(r =>
        r.Id == id &&
        r.UserId == userId &&
        !r.IsApproved);


            if (review == null)
                return RedirectToAction("OrderHistory");

            return View(review);
        }


        // ================= EDIT REVIEW (POST) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditReview(RatingReview model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");


            var review = _context.RatingReviews
    .FirstOrDefault(r =>
        r.Id == model.Id &&
        r.UserId == userId &&
        !r.IsApproved);


            if (review == null)
                return RedirectToAction("OrderHistory");

            if (model.Rating < 1 || model.Rating > 5)
            {
                TempData["Error"] = "Invalid rating.";
                return RedirectToAction("OrderHistory");
            }

            review.Rating = model.Rating;
            review.Review = model.Review;

            _context.SaveChanges();

            TempData["Success"] = "Review updated!";
            return RedirectToAction("OrderHistory");
        }


        // ================= DELETE REVIEW =================
        public IActionResult DeleteReview(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            
            var review = _context.RatingReviews
      .FirstOrDefault(r =>
          r.Id == id &&
          r.UserId == userId &&
          !r.IsApproved);

            if (review == null)
                return RedirectToAction("OrderHistory");

            _context.RatingReviews.Remove(review);
            _context.SaveChanges();

            TempData["Success"] = "Review deleted!";
            return RedirectToAction("OrderHistory");
        }


        [HttpGet]
        public IActionResult Search(string query)
        {
            if (string.IsNullOrEmpty(query))
                return Json(new List<object>());

            var products = _context.Products
                .Where(p => p.Name.ToLower().Contains(query.ToLower()))
                .Select(p => new {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.ImagePath,
                    Category = p.Category.Name
                })
                .ToList();

            return Json(products);
        }



    }

}
