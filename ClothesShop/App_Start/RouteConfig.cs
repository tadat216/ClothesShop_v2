using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ClothesShop
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
                name: "Home",
                url: "trang-chu",
                defaults: new { controller = "Home", action = "Index", alias = UrlParameter.Optional },
                namespaces: new[] { "ClothesShop.Controllers" }
            );
            routes.MapRoute(
                name: "About",
                url: "gioi-thieu",
                defaults: new { controller = "Home", action = "About", alias = UrlParameter.Optional },
                namespaces: new[] { "ClothesShop.Controllers" }
            );
            routes.MapRoute(
                name: "Contact",
                url: "lien-he",
                defaults: new { controller = "Home", action = "Contact", alias = UrlParameter.Optional },
                namespaces: new[] { "ClothesShop.Controllers" }
            );
            routes.MapRoute(
                name: "NewsDetail",
                url: "chi-tiet/{alias}-n{id}",
                defaults: new { controller = "News", action = "Detail", id = UrlParameter.Optional },
                namespaces: new[] { "ClothesShop.Controllers" }
            );
            routes.MapRoute(
                name: "News",
                url: "tin-tuc",
                defaults: new { controller = "News", action = "Index", alias = UrlParameter.Optional },
                namespaces: new[] { "ClothesShop.Controllers" }
            );
            routes.MapRoute(
                name: "ShoppingCart",
                url: "gio-hang",
                defaults: new { controller = "ShoppingCart", action = "Index", alias = UrlParameter.Optional },
                namespaces: new[] { "ClothesShop.Controllers" }
            );
            routes.MapRoute(
                name: "CheckOut",
                url: "thanh-toan",
                defaults: new { controller = "ShoppingCart", action = "CheckOut", alias = UrlParameter.Optional },
                namespaces: new[] { "ClothesShop.Controllers" }
            );
            routes.MapRoute(
                name: "ProductCategory",
                url: "danh-muc-san-pham/{cateAlias}-{cateId}",
                defaults: new { controller = "ProductCategories", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClothesShop.Controllers" }
            );
            routes.MapRoute(
                name: "Product",
                url: "san-pham/{alias}-p{Id}",
                defaults: new { controller = "Products", action = "Index", alias = UrlParameter.Optional },
                namespaces: new[] { "ClothesShop.Controllers" }
            );
            routes.MapRoute(
                name: "Products",
                url: "danh-muc-san-pham",
                defaults: new { controller = "ProductCategories", action = "Index", alias = UrlParameter.Optional },
                namespaces: new[] { "ClothesShop.Controllers" }
            );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClothesShop.Controllers" }
            );
        }
    }
}
