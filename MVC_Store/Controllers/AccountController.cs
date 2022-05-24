using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml.Schema;
using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Account;
using MVC_Store.Models.ViewModels.Shop;
using Newtonsoft.Json.Serialization;

namespace MVC_Store.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }

        // GET: account/create-account
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        // POST: account/create-account
        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            //Check for valid
            if (!ModelState.IsValid)
                return View("CreateAccount", model);

            //Check for pass
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Passwords don't match");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
                //Check username for unique
                if (db.Users.Any(x => x.Username.Equals(model.Username)))
                {
                    ModelState.AddModelError("", $"That username is taken");
                    model.Username = "";
                    return View("CreateAccount", model);
                }

                //Create class UserDTO
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    Username = model.Username,
                    Password = model.Password
                };

                //Add all data to class
                db.Users.Add(userDTO);

                //Save data
                db.SaveChanges();

                //Add role
                int id = userDTO.Id;

                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };

                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();
            }

            //Send message to TempData
            TempData["SM"] = "You are now registered and can login";

            //Redirect user
            return RedirectToAction("Login");
        }

        // GET: Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            //Check for authorized
            string userName = User.Identity.Name;

            if (!string.IsNullOrEmpty(userName))
                return RedirectToAction("user-profile");

            //Return
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            //Check for valid
            if (!ModelState.IsValid)
                return View(model);

            //Check user for valid
            bool isValid = false;

            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.Username.Equals(model.Username) && x.Password.Equals(model.Password)))
                    isValid = true;

                if (!isValid)
                {
                    ModelState.AddModelError("", "Invalid username or password");
                    return View(model);
                }
                else
                {
                    FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                    return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
                }
            }
        }

        //GET: /account/logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        public ActionResult UserNavPartial()
        {
            //Get username
            string userName = User.Identity.Name;

            //Declare model
            UserNavPartialVM model;

            using (Db db = new Db())
            {
                //Get user
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == userName);

                //Fill model with data from context (DTO)
                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };
            }
            //Return
            return PartialView(model);
        }

        //GET: /account/UserProfile
        [HttpGet]
        [ActionName("user-profile")]
        public ActionResult UserProfile()
        {
            //Get username
            string userName = User.Identity.Name;

            //Declare model
            UserProfileVM model;

            using (Db db = new Db())
            {
                //Get user
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == userName);

                //Initialize model with data
                model = new UserProfileVM(dto);
            }
            //Return model to view
            return View("UserProfile", model);
        }

        //POST: /account/UserProfile
        [HttpPost]
        [ActionName("user-profile")]
        public ActionResult UserProfile(UserProfileVM model)
        {
            bool userNameIsChanged = false;
            //Check model for valid
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            //Check password (if user changing)
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Passwords do not match");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {
                //Get username
                string userName = User.Identity.Name;

                //Check for name change
                if (userName != model.Username)
                {
                    userName = model.Username;
                    userNameIsChanged = true;
                }

                //Check username for unique
                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.Username == userName))
                {
                    ModelState.AddModelError("", $"Username already exists");
                    model.Username = "";
                    return View("UserProfile", model);
                }

                //Change model
                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.Username = model.Username;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }

                //Save changes
                db.SaveChanges();
            }

            //Set message to TempData
            TempData["SM"] = "You have successfully edited your profile";

            //Return view with model
            if (!userNameIsChanged)
                return View("UserProfile", model);
            else
                return RedirectToAction("Logout");
        }

        //GET: /account/Orders
        public ActionResult Orders()
        {
            //Initialize model OrdersForUserVM
            List<OrdersForUserVM> ordersForUser = new List<OrdersForUserVM>();

            using (Db db = new Db())
            {
                //Get user ID
                UserDTO user = db.Users.FirstOrDefault(x => x.Username == User.Identity.Name);
                int userId = user.Id;

                //Initialize OrderVM
                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x))
                    .ToList();

                //Sort products list in OrderVM
                foreach (var order in orders)
                {
                    //Initialize products dictionary
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                    //Declare sum
                    decimal total = 0m;

                    //Initialize OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsDto =
                        db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    //Sort OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsDto)
                    {
                        //Get product
                        ProductDTO product = db.Products.FirstOrDefault(x => x.Id == orderDetails.ProductId);

                        //Get price
                        decimal price = product.Price;

                        //Get name
                        string productName = product.Name;

                        //Add product to dictionary
                        productsAndQty.Add(productName, orderDetails.Quantity);

                        //Get total price
                        total += orderDetails.Quantity * price;
                    }
                    //Add data to model OrdersForUserVM
                    ordersForUser.Add(new OrdersForUserVM()
                    {
                        OrderNumber = order.OrderId,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }
            //Return view with model OrdersForUserVM
            return View(ordersForUser);
        }
    }
}
