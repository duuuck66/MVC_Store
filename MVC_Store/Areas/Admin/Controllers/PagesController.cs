using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Store.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //List declaration for View (PageVm)
            List<PageVM> pageList;

            //Initialize list
            using (Db db = new Db())
            {
                pageList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }

            //List back to view
            return View(pageList);
        }

        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        // POST: Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //View validation
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {

                //Create new variable for description(slug)
                string slug;

                //Initialize PageDTO class
                PagesDTO dto = new PagesDTO();

                //Add model header
                dto.Title = model.Title.ToUpper();

                //Check slug. if false => add
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

                //Check slug and desc for unique
                if (db.Pages.Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "That title already exist.");
                    return View(model);
                }
                else if (db.Pages.Any(x => x.Slug == model.Slug))
                {
                    ModelState.AddModelError("", "That slug already exist.");
                    return View(model);
                }

                //Add other model values
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;

                //Save model to db
                db.Pages.Add(dto);
                db.SaveChanges();

            }

            //Return message
            TempData["SM"] = "You have addea a new page!";

            //Redirect user to index method
            return RedirectToAction("Index");
        }

        // GET: Admin/Pages/EditPage/id
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            //PageVM declaration
            PageVM model;

            using (Db db = new Db())
            {
                //Get page ID
                PagesDTO dto = db.Pages.Find(id);

                //Check for page available
                if (dto == null)
                {
                    return Content("This page does not exist.");
                }
                //Initialize 
                model = new PageVM(dto);
            }

            //Return model to VM
            return View(model);
        }

        // POST: Admin/Pages/AddPage
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            //Check for valid
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                //Get page ID
                int id = model.Id;

                //Temp variable declaration
                string slug = "home";

                //Get page by ID
                PagesDTO dto = db.Pages.Find(id);

                //Assign name from model in DTO
                dto.Title = model.Title;

                //Check slug, if null => write
                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }

                //Check slug and title for unique 
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "That title already exist.");
                    return View(model);
                }
                else if (db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That slug already exist.");
                    return View(model);
                }

                //Assign other variables in DTO
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;

                //Save to db
                db.SaveChanges();

            }
            //Return success message | TempData
            TempData["SM"] = "You have edited the page.";

            //Return user
            return RedirectToAction("EditPage");
        }

        // GET: Admin/Pages/PageDetails/id
        public ActionResult PageDetails(int id)
        {
            //Model declaration
            PageVM model;

            using (Db db = new Db())
            {
                //Get page
                PagesDTO dto = db.Pages.Find(id);

                //Check page for availability
                if (dto == null)
                {
                    return Content("The page does not available.");
                }

                //Assign info from db to model
                model = new PageVM(dto);

            }
            //Return model
            return View(model);
        }

        // GET: Admin/Pages/DeletePage/id
        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db())
            {
                //Get page
                PagesDTO dto = db.Pages.Find(id);

                //Delete page
                db.Pages.Remove(dto);

                //Save changes
                db.SaveChanges();

            }

            //Return message
            TempData["SM"] = "You have deleted a page";

            //Return to index
            return RedirectToAction("Index");
        }

        // GET: Admin/Pages/ReorderPages
        [HttpPost]
        public void ReorderPages(int [] id)
        {
            using (Db db = new Db())
            {
                //Add counter
                int count = 1;

                //Initialize model
                PagesDTO dto;

                //Set sorting type for every page
                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
        }

        // GET: Admin/Pages/EditSidebar
        [HttpGet]
        public ActionResult EditSidebar()
        {
            //Initiailize model
            SidebarVM model;

            using (Db db = new Db())
            {
                //Get all data from DTO
                SidebarDTO dto = db.Sidebars.Find(1);

                //Fill model
                model = new SidebarVM(dto);
            }
            //Return model
            return View(model);
        }

        // POST: Admin/Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (Db db = new Db())
            {
                //Get data from db
                SidebarDTO dto = db.Sidebars.Find(1);

                //Fill data | Body
                dto.Body = model.Body;

                //Save
                db.SaveChanges();
            }
            //Return message
            TempData["SM"] = "You have edited the sidebar";

            //Return user
            return RedirectToAction("EditSidebar");
        }
    }
}
