using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Cart;

namespace MVC_Store.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            //Declare CartVM list
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            //Check cart for null
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "You cart is empty";
                return View();
            }

            //Sum cart (if not null) and send to ViewBag
            decimal total = 0m;
            foreach (var item in cart)
            {
                total += item.Total;
            }
            ViewBag.GrandTotal = total;

            //Return list
            return View(cart);
        }

        public ActionResult CartPartial()
        {
            //Declare CartVM model
            CartVM model = new CartVM();

            //Declare new variable for quantity
            int qty = 0;

            //Declare new variable for price
            decimal price = 0m;

            //Check cart session
            if (Session["cart"] != null)
            {
                //Get total price and products
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }

                model.Quantity = qty;
                model.Price = price;
            }
            else
            {
                //Or set 0
                model.Quantity = 0;
                model.Price = 0m;
            }
            //Return partial view
            return PartialView("_CartPartial",model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            //Declare CartVM List
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            //Declare CartVM model
            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                //Get product via ID
                ProductDTO product = db.Products.Find(id);

                //Check product in cart (is already added)
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                //Add if false
                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                }
                //Add another one if true
                else
                {
                    productInCart.Quantity++;
                }
            }
            //Get total, price and add to model
            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            //Save cart in session
            Session["cart"] = cart;

            //Return
            return PartialView("_AddToCartPartial", model);
        }

        //GET: /cart/IncrementProduct
        public JsonResult IncrementProduct(int productId)
        {
            //Declare cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //Get CartVM model from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                //Add qty
                model.Quantity++;

                //Save all data
                var result = new { qty = model.Quantity, price = model.Price };

                //Return JSON response with data
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        //GET: /cart/DecrementProduct
        public ActionResult DecrementProduct(int productId)
        {
            //Declare cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //Get CartVM model from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                //Del qty
                if (model.Quantity > 1)
                    model.Quantity--;
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }

                //Save all data
                var result = new { qty = model.Quantity, price = model.Price };

                //Return JSON response with data
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }

        public void RemoveProduct(int productId)
        {
            //Declare cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //Get CartVM model from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                cart.Remove(model);
            }
        }

        public ActionResult PaypalPartial()
        {
            //Get product list from cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            //Return partial view
            return PartialView(cart);
        }

        //POST: /cart/PlaceOrder
        [HttpPost]
        public void PlaceOrder()
        {
            //Get product list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            //Get username
            string userName = User.Identity.Name;

            //Declare variable for orderId
            int orderId = 0;

            using (Db db = new Db())
            {
                //Declare model OrderDTO
                OrderDTO orderDto = new OrderDTO();

                //Get user ID
                var q = db.Users.FirstOrDefault(x => x.Username == userName);
                int userId = q.Id;

                //Fill OrderDTO with data and save
                orderDto.UserId = userId;
                orderDto.CreatedAt = DateTime.Now;

                db.Orders.Add(orderDto);
                db.SaveChanges();

                //Get orderID
                orderId = orderDto.OrderId;

                //Declare model OrderDetails DTO
                OrderDetailsDTO orderDetailsDto = new OrderDetailsDTO();

                //Add data
                foreach (var item in cart)
                {
                    orderDetailsDto.OrderId = orderId;
                    orderDetailsDto.UserId = userId;
                    orderDetailsDto.ProductId = item.ProductId;
                    orderDetailsDto.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDto);
                    db.SaveChanges();
                }
            }
            //Send msg to admin mail
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("9313168b2b5e76", "1c60241c2a52ed"),
                EnableSsl = true
            };
            client.Send("shop@duck.com", "admin@duck.com", "New Order", $"You have a new order. Order number:  {orderId}");

            //Reload session
            Session["cart"] = null;
        }
    }
}