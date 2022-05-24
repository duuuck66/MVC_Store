using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Shop;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using MVC_Store.Areas.Admin.Models.ViewModels.Shop;

namespace MVC_Store.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        // GET: Admin/Shop
        public ActionResult Categories()
        {
            //Model declaration (List type)
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                //Initialize model
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }
            //Return List in VM
            return View(categoryVMList);
        }

        // POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            //Declaration string variable ID
            string id;

            using (Db db = new Db())
            {
                //Check for unique
                if (db.Categories.Any(x => x.Name == catName))
                    return "titletaken";

                //Initialize DTO model
                CategoryDTO dto = new CategoryDTO();

                //Fill data
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                //Save
                db.Categories.Add(dto);
                db.SaveChanges();

                //Get ID
                id = dto.Id.ToString();
            }
            //Return view
            return id;
        }

        // POST: Admin/Shop/ReorderCategories
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                //Add counter
                int count = 1;

                //Initialize model
                CategoryDTO dto;

                //Set sorting type for every page
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
        }

        // GET: Admin/Shop/DeleteCategory/id
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                //Get category
                CategoryDTO dto = db.Categories.Find(id);

                //Delete category
                db.Categories.Remove(dto);

                //Save changes
                db.SaveChanges();

            }

            //Return message
            TempData["SM"] = "You have deleted a category";

            //Return to index
            return RedirectToAction("Categories");
        }

        // POST: Admin/Shop/RenameCategory/id
        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                //Check name for unique
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "titletaken";

                //Get DTO model
                CategoryDTO dto = db.Categories.Find(id);

                //Edit model
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                //Save changes
                db.SaveChanges();
            }
            //Return anything
            return "ok";
        }

        // GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            //Declare model
            ProductVM model = new ProductVM();

            //Add categories
            using (Db db = new Db())
            {
                //model.Categories = new SelectList(db.Categories.ToList(), dataValueField:"id", dataTextField:"Name");
                model.Categories = new SelectList(db.Categories.ToList(), "id", "Name");
            }

            //Return model
            return View(model);
        }

        // POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            //Check model valid
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            //Check name for unique
            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "This product name already is taken");
                    return View(model);
                }
            }

            //Declare ProductID
            int id;

            //Initialize and save model
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();
                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();
                id = product.Id;
            }

            //Return message in TempData
            TempData["SM"] = "You have added a product";

            #region Upload Image

            //Add all paths for images
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            //Check path link
            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);

            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);

            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);

            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);

            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            //Check file upload
            if (file != null && file.ContentLength > 0)
            {
                //Get extension
                string ext = file.ContentType.ToLower();

                //Check .extension file
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/gif" &&
                    ext != "image/png" &&
                    ext != "image/webp")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "The image was not uploaded ( wrong image extension)");
                        return View(model);
                    }
                }

                //Declare variable with image name
                string imageName = file.FileName;

                //Save image name in model
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                //Set paths to images
                var path = string.Format($"{pathString2}\\{imageName}");
                var path2 = string.Format($"{pathString3}\\{imageName}");

                //Save image
                file.SaveAs(path);

                //Add and save thumbs
                WebImage img = new WebImage(file.InputStream);
                img.Resize(150, 150).Crop(1, 1);
                img.Save(path2);
            }
            #endregion

            //Redirect user
            return RedirectToAction("AddProduct");
        }

        // GET: Admin/Shop/Products
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            //Declaration | List ProductVM
            List<ProductVM> listOfProductVM;

            //Set page number
            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                //Initialize list and fill it data
                listOfProductVM = db.Products.ToArray()
                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                    .Select(x => new ProductVM(x))
                    .ToList();

                //Fill categories
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //Set category
                ViewBag.SelectedCat = catId.ToString();
            }

            //Set page-by-page navigation
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.onePageOfProducts = onePageOfProducts;

            //Return into VM
            return View(listOfProductVM);
        }

        // GET: Admin/Shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            //Declaration model
            ProductVM model;

            using (Db db = new Db())
            {
                //Get product
                ProductDTO dto = db.Products.Find(id);

                //Check for availability
                if (dto == null)
                {
                    return Content("That product does not exist");
                }

                //Initialize model by data
                model = new ProductVM(dto);

                //Create list of categories
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //Get all images from gallery
                model.GalleryImages = Directory
                    .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            }
            //Return model
            return View(model);
        }

        // POST: Admin/Shop/EditProduct
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            //Get product ID
            int id = model.Id;

            //Fill list
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            model.GalleryImages = Directory
                    .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));

            //Check model for valid
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //Check product name for unique
            using (Db db = new Db())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "That product name is taken");
                    return View(model);
                }
            }

            //Update product in db
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;

                db.SaveChanges();
            }

            //Set message in TempData
            TempData["SM"] = "You have edited the product";

            //Add images processing logic
            #region Image Upload

            //Check for file upload
            if (file != null && file.ContentLength > 0)
            {

                //Get file extension
                string ext = file.ContentType.ToLower();

                //Check file extension
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/gif" &&
                    ext != "image/png" &&
                    ext != "image/webp")
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "The image was not uploaded ( wrong image extension)");
                        return View(model);
                    }
                }

                //Set upload paths
                var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                //Delete existing images and directories
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (var file2 in di1.GetFiles())
                {
                    file2.Delete();
                }

                foreach (var file3 in di2.GetFiles())
                {
                    file3.Delete();
                }

                //Save image name
                string imageName = file.FileName;

                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                //Save original and preview
                var path = string.Format($"{pathString1}\\{imageName}");
                var path2 = string.Format($"{pathString2}\\{imageName}");

                //Save image
                file.SaveAs(path);

                //Add and save thumbs
                WebImage img = new WebImage(file.InputStream);
                img.Resize(150, 150).Crop(1, 1);
                img.Save(path2);
            }

            #endregion

            //Redirect user
            return RedirectToAction("EditProduct");
        }

        // POST: Admin/Shop/DeleteProduct/id
        public ActionResult DeleteProduct(int id)
        {
            //Delete product from db
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);

                db.SaveChanges();
            }
            //Delete directories
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
            var pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString, true);

            //Redirect user
            return RedirectToAction("Products");
        }

        // POST: Admin/Shop/SaveGalleryImages/id
        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            //Sort files
            foreach (string fileName in Request.Files)
            {
                //Initialize files
                HttpPostedFileBase file = Request.Files[fileName];

                //Check for NULL
                if (file != null && file.ContentLength > 0)
                {
                    //Set paths to directories
                    var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

                    string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    //Set images paths
                    var path = string.Format($"{pathString1}\\{file.FileName}");
                    var path2 = string.Format($"{pathString2}\\{file.FileName}");

                    //Save images and thumbs
                    file.SaveAs(path);

                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(150, 150).Crop(1, 1);
                    img.Save(path2);
                }
            }
        }

        // POST: Admin/Shop/DeleteImage/id/imageName
        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);
        }

        // GET: Admin/Shop/Orders
        public ActionResult Orders()
        {
            //Initialize OrdersForAdminVM
            List<OrdersForAdminVM> ordersForAdmin = new List<OrdersForAdminVM>();

            using (Db db = new Db())
            {
                //Initialize OrderVM
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

                //Sort all products list and fill OrderVM
                foreach (var order in orders)
                {
                    //Initialize products dictionary
                    Dictionary<string, int> productAndQty = new Dictionary<string, int>();

                    //Add variable for sum
                    decimal total = 0m;

                    //Initialize OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsList =
                        db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    //Get username
                    UserDTO user = db.Users.FirstOrDefault(x => x.Id == order.UserId);
                    string username = user.Username;

                    //Sort all products to this user
                    foreach (var orderDetails in orderDetailsList)
                    {
                        //Get product
                        ProductDTO product = db.Products.FirstOrDefault(x => x.Id == orderDetails.ProductId);

                        //Get price for product
                        decimal price = product.Price;

                        //Get product name
                        string productName = product.Name;

                        //Add product to dictionary
                        productAndQty.Add(productName, orderDetails.Quantity);

                        //Get all sum
                        total += orderDetails.Quantity * price;
                    }
                    //Add data to OrdersForAdminVM
                    ordersForAdmin.Add(new OrdersForAdminVM()
                    {
                        OrderNumber = order.OrderId,
                        UserName = username,
                        Total = total,
                        ProductsAndQty = productAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }
            //Return
            return View(ordersForAdmin);
        }
    }
}