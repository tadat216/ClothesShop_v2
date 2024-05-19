using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClothesShop.Models.EF;
using ClothesShop.Models;
using OfficeOpenXml;
using System.ComponentModel.DataAnnotations;

namespace ClothesShop.Areas.Admin.Controllers
{
    public class ProductImportModel
    {
        public string Category { get; set; }
        public string ProductCode { get; set; }
        public string Title { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }

        public string ColorCode { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
    }
    public class InsertProductController : Controller
    {
        public List<ProductImportModel> ReadExcelFile(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            List<ProductImportModel> products = new List<ProductImportModel>();

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {

                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 3; row <= rowCount; row++) 
                {
                    products.Add(new ProductImportModel
                    {
                        ProductCode = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                        Title = worksheet.Cells[row, 3].Value?.ToString().Trim(),
                        Size = worksheet.Cells[row, 4].Value?.ToString().Trim(),
                        Color = worksheet.Cells[row, 5].Value?.ToString().Trim(),
                        ColorCode = worksheet.Cells[row, 6].Value?.ToString().Trim(),
                        Quantity = int.Parse(worksheet.Cells[row, 7].Value?.ToString().Trim()),
                        Price = int.Parse(worksheet.Cells[row, 8].Value?.ToString().Trim()),
                        Category = worksheet.Cells[row, 9].Value?.ToString().Trim(),
                    });
                }
            }
            return products;
        }
        public void ProcessAndSaveData(List<ProductImportModel> products)
        {
            ApplicationDbContext db = new ApplicationDbContext();

            foreach (var item in products)
            {
                var category = db.ProductCategories.FirstOrDefault(c => c.Title == item.Category);
                if(category == null)
                {
                    category = new ProductCategory
                    {
                        Title = item.Category,
                        Level = 1,
                        IsActive = 1
                    };
                }
                var product = db.Products.FirstOrDefault(p => p.ProductCode == item.ProductCode);
                if (product == null)
                {
                    product = new Product
                    {
                        Title = item.Title,
                        ProductCode = item.ProductCode,
                        Price = item.Price,
                        PriceSale =1,
                        IsHome =true,
                        IsFeature =true,
                        IsHot =true,
                        IsActive=true,
                        IsSale= false,
                        ProductCategory = category,
                    };
                    db.Products.Add(product);
                }

                var color = db.Colors.FirstOrDefault(c => c.Code== item.ColorCode);
                if(color == null)
                {
                    color = new Color
                    {
                        Name = item.Color,
                        Code = item.ColorCode
                    };
                    db.Colors.Add(color);
                }
                var size = db.Sizes.FirstOrDefault(s => s.Name == item.Size);
                if (size == null)
                {
                    size = new Size
                    {
                        Name = item.Size,
                        Order = 1
                    };
                    db.Sizes.Add(size);
                }

                ProductVariant variantProduct = product.ProductVariants.FirstOrDefault(v => v.ColorId == color.Id && v.ProductId == product.Id);
                if (variantProduct == null)
                {
                    variantProduct = new ProductVariant
                    {
                        ProductId = product.Id,
                        ColorId = color.Id,
                        Product = product,
                        Color = color,
                        IsDefault = true
                    };
                    db.ProductVariants.Add(variantProduct);
                }

                VariantSize variantSize = db.VariantSizes.FirstOrDefault(v => v.ProductVariantId == variantProduct.Id && v.SizeId == size.Id);
                if(variantSize == null)
                {
                     variantSize = new VariantSize
                    {
                         ProductVariantId = variantProduct.Id,
                         SizeId = variantProduct.Id,
                        ProductVariant = variantProduct,
                        Size = size,
                        Amount = item.Quantity
                    };
                    db.VariantSizes.Add(variantSize);
                }
                else
                {
                    variantSize.Amount += item.Quantity;
                    db.Entry(variantSize).State = System.Data.Entity.EntityState.Modified;
                }
                db.SaveChanges();
            }
        }
        // GET: Admin/InsertProduct
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        
        public ActionResult InsertProduct(HttpPostedFileBase file)
        
        {
            if (file != null && file.ContentLength > 0)
            {
                // Lưu hoặc xử lý tệp tại đây
                // ví dụ: lưu tệp tạm trong server
                var path = Path.Combine(Server.MapPath("~/Uploads"), file.FileName);
                file.SaveAs(path);

                // Tiếp tục xử lý tệp sau khi lưu
                List<ProductImportModel> list = ReadExcelFile(path);
                ProcessAndSaveData(list);
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }
    }
}