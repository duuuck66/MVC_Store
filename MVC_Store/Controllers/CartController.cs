using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
    }
}