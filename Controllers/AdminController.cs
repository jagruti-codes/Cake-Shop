using cake_shop.Data;
using cake_shop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Text;

namespace cake_shop.Controllers
{
    public class AdminController : Controller
    {


        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }




        public IActionResult Dashboard(string range = "30")
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin")
                return RedirectToAction("Login", "Account");

            //  Date filter
            DateTime fromDate = range switch
            {
                "7" => DateTime.Now.AddDays(-7),
                "30" => DateTime.Now.AddDays(-30),
                "90" => DateTime.Now.AddDays(-90),
                _ => DateTime.Now.AddDays(-30)
            };

            var ordersQuery = _context.Orders
                .Where(o => o.CreatedAt >= fromDate);  

            var topProducts = _context.OrderItems
                .Include(x => x.Product)
                .Where(x => x.Order.CreatedAt >= fromDate)
                .GroupBy(x => new
                {
                    x.ProductId,
                    x.Product.Name,
                    x.Product.ImagePath,
                    x.Product.Price
                })
                .Select(g => new TopProductVM
                {
                    Name = g.Key.Name,
                    ImagePath = g.Key.ImagePath,
                    Price = g.Key.Price,
                    TotalSold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(4)
                .ToList();

            var vm = new AdminDashboardViewModel
            {
                TotalUsers = _context.Users.Count(x => x.Role == "User"),
                TotalProducts = _context.Products.Count(),
                TotalOrders = ordersQuery.Count(),
                TotalCategories = _context.Categories.Count(),

                TotalRevenue = ordersQuery
                    .Where(o => o.Status == "Delivered")
                    .Sum(o => o.TotalAmount),

                RecentOrders = ordersQuery
                    .Include(o => o.User)
                    .OrderByDescending(o => o.Id)
                    .Take(5)
                    .ToList(),

                TopProducts = topProducts
            };

            ViewBag.Range = range; 
            return View(vm);
        }
       

        public IActionResult ExportReport(string range = "30")
        {
            var days = int.TryParse(range, out var r) ? r : 30;
            var fromDate = DateTime.Now.AddDays(-days);

            var orders = _context.Orders
                .Include(o => o.User)
                .Where(o => o.CreatedAt >= fromDate)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            var csv = new StringBuilder();

            csv.AppendLine("OrderId,Customer,Status,TotalAmount,Date");

            foreach (var o in orders)
            {
                csv.AppendLine(
                    $"{o.Id}," +
                    $"\"{o.User?.Name}\"," +
                    $"{o.Status}," +
                    $"{o.TotalAmount}," +
                    $"{o.CreatedAt:yyyy-MM-dd HH:mm}"
                );
            }

            var bytes = Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(csv.ToString()))
                .ToArray();

            return File(bytes, "text/csv", $"Orders_Report_{range}.csv");
        }


        //  Logout (optional but recommended)
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

       

        public IActionResult Category()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            // Fetch all categories from database
            var categories = _context.Categories.ToList();

            return View(categories);
        }


        // GET: Show Add Category form
        public IActionResult AddCategory()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCategory(Category model, IFormFile imageFile)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                //  HANDLE IMAGE UPLOAD
                if (imageFile != null && imageFile.Length > 0)
                {
                    string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    model.ImagePath = "/images/" + fileName;
                }

                _context.Categories.Add(model);
                _context.SaveChanges();

                
                TempData["success"] = "Category added successfully!";
                return RedirectToAction("Category");
            }

            return View(model);
        }



        //Edit Category

        // GET: Edit Category
        public IActionResult EditCategory(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var category = _context.Categories.Find(id);

            if (category == null)
                return NotFound();

            return View(category);
        }

       [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult EditCategory(Category model, IFormFile imageFile)
{
    if (HttpContext.Session.GetString("Role") != "Admin")
        return RedirectToAction("Login", "Account");

    var category = _context.Categories.Find(model.Id);

    if (category == null)
        return NotFound();

    // UPDATE ALL FIELDS
    category.Name = model.Name;
    category.Description = model.Description;
    category.ItemCount = model.ItemCount;
    category.IsActive = model.IsActive;

    // IMAGE OPTIONAL (NO FORCE)
    if (imageFile != null && imageFile.Length > 0)
    {
        string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
        string filePath = Path.Combine(folder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            imageFile.CopyTo(stream);
        }

        category.ImagePath = "/images/" + fileName;
    }

    _context.SaveChanges();

    TempData["success"] = "Category updated successfully!";
    return RedirectToAction("Category");
}


        public IActionResult DeleteCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
            }

            // return RedirectToAction("Category");
            TempData["success"] = "Category deleted successfully!";
            return RedirectToAction("Category");
        }


        //Product

        public IActionResult Product()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var products = _context.Products
                .Include(p => p.Category)
                .ToList();

            return View(products);
        }


        //add product

        public IActionResult AddProduct()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddProduct(Product model, IFormFile imageFile)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");
            // Remove Category from validation
            ModelState.Remove("Category");
            if (!ModelState.IsValid)
            {
                // Debug: See what's failing
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"{error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }

                ViewBag.Categories = _context.Categories.ToList();
                return View(model);
            }

            // IMAGE UPLOAD
            if (imageFile != null && imageFile.Length > 0)
            {
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                model.ImagePath = "/images/" + fileName;
            }

            // DEFAULT VALUES FIX (VERY IMPORTANT)
            model.Status = model.Status ?? "Active";
            model.Quantity = model.Quantity ?? "1";

            _context.Products.Add(model);
            _context.SaveChanges();

            TempData["success"] = "Product added successfully!";
            return RedirectToAction("Product");
        }



        //edit product

        public IActionResult EditProduct(int id)
        {
            var product = _context.Products.Find(id);

            if (product == null)
                return NotFound();

            ViewBag.Categories = _context.Categories.ToList();

            return View(product);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProduct(Product model, IFormFile imageFile)
        {
            var product = _context.Products.Find(model.Id);

            if (product == null)
                return NotFound();

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.Stock = model.Stock;
            product.Quantity = model.Quantity;
            product.Status = model.Status;
            product.CategoryId = model.CategoryId;

            // image update (optional)
            if (imageFile != null && imageFile.Length > 0)
            {
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                product.ImagePath = "/images/" + fileName;
            }

            _context.SaveChanges();

            TempData["success"] = "Product updated successfully!";
            return RedirectToAction("Product");
        }


        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);

            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();

                TempData["success"] = "Product deleted successfully!";
            }

            return RedirectToAction("Product");
        }

        public IActionResult AddOrder()
        {
            return View();
        }


        public IActionResult Order(string search, string status)
        {
            var orders = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .AsQueryable();

            // SEARCH (customer name or product name)
            if (!string.IsNullOrEmpty(search))
            {
                orders = orders.Where(o =>
                    o.User.Name.Contains(search) ||
                    o.Items.Any(i => i.Product.Name.Contains(search))
                );
            }

            // STATUS FILTER
            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.Status == status);
            }

            return View(orders.ToList());
        }

        //Edit Order
        public IActionResult EditOrder(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var order = _context.Orders.FirstOrDefault(o => o.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditOrder(Order model)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == model.Id);

            if (order == null)
                return NotFound();

            // ONLY STATUS UPDATE (safe way)
            order.Status = model.Status;

            _context.SaveChanges();

            TempData["success"] = "Order status updated successfully!";
            return RedirectToAction("Order");
        }




        // DELETE ORDER
        public IActionResult DeleteOrder(int id)
        {
            var order = _context.Orders
                .Include(o => o.Items)
                .FirstOrDefault(o => o.Id == id);

            if (order != null)
            {
                _context.Orders.Remove(order);
                _context.SaveChanges();

                TempData["success"] = "Order deleted successfully!";
            }

            return RedirectToAction("Order");
        }

        // ======================= SHIPPING =======================

        public IActionResult Shipping()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var shipments = _context.Shippings
                .Include(x => x.Order)
                .ThenInclude(o => o.User)
                .OrderByDescending(x => x.Id)
                .ToList();

            return View(shipments);
        }

        // CREATE SHIPPING FROM ORDER (SAFE - NO DUPLICATE)
        public IActionResult CreateShipping(int orderId)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var order = _context.Orders
                .Include(o => o.User)
                .FirstOrDefault(x => x.Id == orderId);

            if (order == null)
                return NotFound();

            //  PREVENT DUPLICATE SHIPPING
            var existing = _context.Shippings.FirstOrDefault(s => s.OrderId == orderId);

            if (existing != null)
            {
                TempData["success"] = "Shipping already exists for this order!";
                return RedirectToAction("Shipping");
            }

            var shipping = new Shipping
            {
                OrderId = order.Id,
                ShippingCode = "SHP-" + order.Id + "-" + new Random().Next(1000, 9999),
                CustomerName = order.User?.Name,
                Email = order.User?.Email,
                Phone = order.Phone,
                Destination = order.Address + ", " + order.City,
                Method = "Standard",
                Status = "Pending"
            };

            _context.Shippings.Add(shipping);
            _context.SaveChanges();

            TempData["success"] = "Shipping created successfully!";
            return RedirectToAction("Shipping");
        }

        
        public IActionResult UpdateShippingStatus(int id, string status)
        {
            var shipping = _context.Shippings.FirstOrDefault(x => x.Id == id);

            if (shipping != null)
            {
                shipping.Status = status;

                // ALSO UPDATE ORDER STATUS
                var order = _context.Orders.FirstOrDefault(o => o.Id == shipping.OrderId);

                if (order != null)
                {
                    order.Status = status; 
                }

                _context.SaveChanges();
            }

            TempData["success"] = "Shipping status updated";
            return RedirectToAction("Shipping");
        }

        // DELETE SHIPPING
        public IActionResult DeleteShipping(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var ship = _context.Shippings.FirstOrDefault(x => x.Id == id);

            if (ship != null)
            {
                _context.Shippings.Remove(ship);
                _context.SaveChanges();

                TempData["success"] = "Shipping deleted!";
            }

            return RedirectToAction("Shipping");
        }
       

        //change Status


        public IActionResult ChangeStatus(int id)
        {
            var order = _context.Orders.Find(id);

            if (order != null)
            {
                order.Status = "Delivered";
                _context.SaveChanges();
            }

            return RedirectToAction("Orders");
        }


        public IActionResult Payment(string search, string status)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var payments = _context.Payments
                .Include(p => p.Order)
                .AsQueryable();

            //  SEARCH
            if (!string.IsNullOrEmpty(search))
            {
                payments = payments.Where(p =>
                    p.CustomerName.Contains(search) ||
                    p.Email.Contains(search) ||
                    p.OrderId.ToString().Contains(search));
            }

            //  STATUS FILTER
            if (!string.IsNullOrEmpty(status))
            {
                payments = payments.Where(p => p.Status == status);
            }

            return View(payments.OrderByDescending(x => x.Id).ToList());
        }
       


        public IActionResult UpdatePaymentStatus(int id, string status)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var payment = _context.Payments
                .Include(p => p.Order)
                .FirstOrDefault(x => x.Id == id);

            if (payment == null)
                return NotFound();

            // Update Payment Status
            payment.Status = status;

            //  Sync Order Status (IMPORTANT)
            if (payment.Order != null)
            {
                payment.Order.Status = status == "Completed"
                    ? "Delivered"
                    : status == "Failed"
                        ? "Cancelled"
                        : payment.Order.Status;
            }

            _context.SaveChanges();

            TempData["success"] = "Payment status updated successfully!";
            return RedirectToAction("Payment");
        }


        // ======================= CUSTOMERS =======================

        public IActionResult Customers(string search, string status)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var customers = _context.Users
                .Where(x => x.Role == "User")
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                customers = customers.Where(x =>
                    x.Name.Contains(search) ||
                    x.Email.Contains(search) ||
                    x.Phone.Contains(search));
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "Active")
                    customers = customers.Where(x => x.IsEmailVerified == true);
                else if (status == "Inactive")
                    customers = customers.Where(x => x.IsEmailVerified == false);
            }

            return View(customers.OrderByDescending(x => x.Id).ToList());
        }


        // ======================= EDIT CUSTOMER (ONLY STATUS EDIT) =======================

        public IActionResult EditCustomer(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var customer = _context.Users.FirstOrDefault(x => x.Id == id && x.Role == "User");

            if (customer == null)
                return NotFound();

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCustomer(int id, bool IsEmailVerified)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var customer = _context.Users.FirstOrDefault(x => x.Id == id && x.Role == "User");

            if (customer == null)
                return NotFound();

            //  ONLY STATUS UPDATE
            customer.IsEmailVerified = IsEmailVerified;

            _context.SaveChanges();

            TempData["success"] = "Customer status updated successfully!";
            return RedirectToAction("Customers");
        }


        // ======================= DELETE CUSTOMER =======================

        public IActionResult DeleteCustomer(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var customer = _context.Users.FirstOrDefault(x => x.Id == id && x.Role == "User");

            if (customer != null)
            {
                _context.Users.Remove(customer);
                _context.SaveChanges();

                TempData["success"] = "Customer deleted successfully!";
            }

            return RedirectToAction("Customers");
        }


        public IActionResult Reviews()
        {
            // Admin check
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            // ⭐ Fetch all reviews with User + Product details
            var reviews = _context.RatingReviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            return View(reviews);
        }



        public IActionResult ApproveReview(int id)
        {
            var review = _context.RatingReviews
                .FirstOrDefault(r => r.Id == id);

            if (review == null)
                return NotFound();

            review.IsApproved = true;
            review.Status = "Approved";

            _context.SaveChanges();

            TempData["success"] = "Review approved!";

            return RedirectToAction("Reviews");
        }




        //delete

        public IActionResult DeleteReview(int id)
        {
            var review = _context.RatingReviews.FirstOrDefault(x => x.Id == id);

            if (review != null)
            {
                _context.RatingReviews.Remove(review);
                _context.SaveChanges();
            }

            TempData["success"] = "Review deleted!";
            return RedirectToAction("Reviews");
        }



   
        // ================= CONTACT (ADMIN SETTINGS) =================

        // GET
        public IActionResult Contact()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var contact = _context.Contacts.FirstOrDefault();

            if (contact == null)
            {
                contact = new Contact
                {
                    Email = "",
                    Phone = "",
                    Address = ""
                };
            }

            return View(contact);
        }


        // POST (INSERT OR UPDATE SINGLE RECORD)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(Contact model)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var contact = _context.Contacts.FirstOrDefault();

            // IF NO DATA EXISTS → CREATE FIRST ROW
            if (contact == null)
            {
                _context.Contacts.Add(model);
            }
            else
            {
                // UPDATE EXISTING ROW
                contact.Email = model.Email;
                contact.Phone = model.Phone;
                contact.Address = model.Address;

                contact.WeekdayStart = model.WeekdayStart;
                contact.WeekdayEnd = model.WeekdayEnd;

                contact.WeekendStart = model.WeekendStart;
                contact.WeekendEnd = model.WeekendEnd;

                contact.ApplyHolidayHours = model.ApplyHolidayHours;
            }

            _context.SaveChanges();

            TempData["success"] = "Contact settings updated successfully!";
            return RedirectToAction("Contact");
        }


        public IActionResult Setting()
        {
            var admin = _context.Users.FirstOrDefault(x => x.Role == "Admin");

            if (admin == null)
                return NotFound();

            return View(admin);
        }

        [HttpPost]
        public IActionResult Setting(AppUser model)
        {
            var admin = _context.Users.FirstOrDefault(x => x.Id == model.Id);

            if (admin == null)
                return NotFound();

            // ONLY ALLOWED UPDATES
            admin.Name = model.Name;
            admin.Phone = model.Phone;

            //Email NOT updated

            _context.SaveChanges();

            TempData["success"] = "Profile updated successfully!";
            return RedirectToAction("Setting");
        }


    }
}
