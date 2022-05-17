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
            return View();
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
            return PartialView();
        }
    }
}