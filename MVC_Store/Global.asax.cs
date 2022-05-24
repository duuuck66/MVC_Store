using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using MVC_Store.Models.Data;

namespace MVC_Store
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_AuthenticateRequest()
        {
            //Check authorization
            if (User == null)
                return;

            //Get username
            string userName = Context.User.Identity.Name;

            //Roles array declaration
            string[] roles = null;

            using (Db db = new Db())
            {
                //Fill array
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == userName);

                if (dto == null)
                    return;

                roles = db.UserRoles.Where(x => x.UserId == dto.Id).Select(x => x.Role.Name).ToArray();
            }
            //Create IPrincipal interface object 
            IIdentity userIdentity = new GenericIdentity(userName);
            IPrincipal newUserObj = new GenericPrincipal(userIdentity, roles);

            //Declare and initialize with data Context.User
            Context.User = newUserObj;
        }
    }
}
