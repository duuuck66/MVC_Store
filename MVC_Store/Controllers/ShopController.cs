using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Shop;

namespace MVC_Store.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }

        //GET: CategoryMenuPartial
        public ActionResult CategoryMenuPartial()
        {
            //Declaration List<> CategoryVM
            List<CategoryVM> categoryVMList;

            //Initialize model via data
            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x))
                    .ToList();
            }

            //Return partial VM
            return PartialView("_CategoryMenuPartial", categoryVMList);
        }
        
        //GET: Category/name
        public ActionResult Category(string name)
        {
            //Declare list
            List<ProductVM> productVMList;

            using (Db db = new Db())
            {
                //Get category ID
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();

                int catId = categoryDTO.Id;
                
                //Initialize list by data
                productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x))
                    .ToList();

                //Get category name
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();

                //Check for null
                if (productCat == null)
                {
                    var catName = db.Categories.Where(x => x.Slug == name).Select(x => x.Name).FirstOrDefault();
                    ViewBag.CategoryName = catName;
                }
                else
                {
                    ViewBag.CategoryName = productCat.CategoryName;
                }
            }
            //Return view
            return View(productVMList);
        }

        //GET: Shop/product-details
        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            //Declare DTO and VM models
            ProductDTO dto;
            ProductVM model;

            //Initialize product ID
            int id = 0;

            using (Db db = new Db())
            {
                //Check for availability
                if (!db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }

                //Initialize productDTO by data
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                //Get ID
                id = dto.Id;

                //Initialize model VM by data
                model = new ProductVM(dto);
            }
            //Get image from gallery
            model.GalleryImages = Directory
                .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                .Select(fn => Path.GetFileName(fn));

            //Return model
            return View("ProductDetails", model);
        }
    }
}