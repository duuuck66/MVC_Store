using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Store.Controllers
{
    public class PagesController : Controller
    {
        // GET: Index/{page}
        public ActionResult Index(string page = "")
        {
            //Get || Set slug
            if (page == "")
                page = "home";

            //Declare DTO model and class
            PageVM model;
            PagesDTO dto;

            //Check current page for reachable
            using (Db db = new Db())
            {
                if (!db.Pages.Any(x => x.Slug.Equals(page)))
                    return RedirectToAction("Index", new { page = "" });
            }

            //Get page DTO
            using (Db db = new Db())
            {
                dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();
            }

            //Set titles
            ViewBag.PageTitle = dto.Title;

            //Check sidebar
            if (dto.HasSidebar == true)
            {
                ViewBag.Sidebar = "Yes";
            }
            else
            {
                ViewBag.Sidebar = "No";
            }

            //Fill model
            model = new PageVM(dto);

            //Return VM
            return View(model);
        }

        public ActionResult PagesMenuPartial()
        {
            //Initialize PageVM list
            List<PageVM> pageVMList;

            // Get all page, except home
            using (Db db = new Db())
            {
                pageVMList = db.Pages.ToArray().OrderBy(x => x.Sorting).Where(x => x.Slug != "home").Select(x => new PageVM(x)).ToList();
            }

            //Return partial VM with list of data
            return PartialView("_PagesMenuPartial", pageVMList);
        }

        public ActionResult SidebarPartial()
        {
            //Declare model
            SidebarVM model;

            //Initialize model
            using (Db db = new Db())
            {
                SidebarDTO dto = db.Sidebars.Find(1);

                model = new SidebarVM(dto);
            }

            //Return model
            return PartialView("_SidebarPartial", model);
        }
    }
}