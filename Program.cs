using System;
using NLog.Web;
using System.IO;
using System.Linq;
using NorthwindConsole.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NorthwindConsole
{
    class Program
    {
        // create static instance of Logger
        private static NLog.Logger logger = NLogBuilder.ConfigureNLog(Directory.GetCurrentDirectory() + "\\nlog.config").GetCurrentClassLogger();
        static void Main(string[] args)
        {
            logger.Info("Program started");

            try
            {
                string choice;
                do
                {
                    Console.WriteLine("1) Add/Edit/Display/Delete Categories");
                    Console.WriteLine("2) Add/Edit/Display/Delete Products");
                    Console.WriteLine("\"q\" to quit");
                    choice = Console.ReadLine();
                    
                    if(choice == "1")
                    {
                    Console.WriteLine("1) Display Categories");
                    Console.WriteLine("2) Add Category");
                    Console.WriteLine("3) Display Category and related Products");
                    Console.WriteLine("4) Display all Categories and their related Products");
                    Console.WriteLine("5) Edit a Category");
                    Console.WriteLine("6) Display all Categories and active Products");
                    Console.WriteLine("7) Display a specific Category and active Product data");
                    Console.WriteLine("8) Delete a Category");
                    Console.WriteLine("\"q\" to quit");
                    choice = Console.ReadLine();
                    Console.Clear();
                                        logger.Info($"Option {choice} selected");
                    if (choice == "1")
                    {
                        var db = new NWConsole_96_RDHContext();
                        var query = db.Categories.OrderBy(p => p.CategoryName);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{query.Count()} records returned");
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryName} - {item.Description}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (choice == "2")
                    {
                        Categories category = new Categories();
                        Console.WriteLine("Enter Category Name:");
                        category.CategoryName = Console.ReadLine();
                        Console.WriteLine("Enter the Category Description:");
                        category.Description = Console.ReadLine();

                        ValidationContext context = new ValidationContext(category, null, null);
                        List<ValidationResult> results = new List<ValidationResult>();

                        var isValid = Validator.TryValidateObject(category, context, results, true);
                        if (isValid)
                        {
                            var db = new NWConsole_96_RDHContext();
                            // check for unique name
                            if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                            {
                                // generate validation error
                                isValid = false;
                                results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                            }
                            else
                            {
                                logger.Info("Validation passed");
                                db.Categories.Add(category);
                                db.SaveChanges();
                                logger.Info("Category added - {CategoryName}", category.CategoryName);

                            }
                        }
                        if (!isValid)
                        {
                            foreach (var result in results)
                            {
                                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                            }
                        }
                    }
                    else if (choice == "3")
                    {
                        var db = new NWConsole_96_RDHContext();
                        var query = db.Categories.OrderBy(p => p.CategoryId);

                        Console.WriteLine("Select the category whose products you want to display:");
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        int id = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"CategoryId {id} selected");
                        Categories category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
                        Console.WriteLine($"{category.CategoryName} - {category.Description}");
                        foreach (Products p in category.Products)
                        {
                            Console.WriteLine(p.ProductName);
                        }
                    }
                    else if (choice == "4")
                    {
                        var db = new NWConsole_96_RDHContext();
                        var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryName}");
                            foreach (Products p in item.Products)
                            {
                                Console.WriteLine($"\t{p.ProductName}");
                            }
                        }
                    }

                    else if (choice == "5")
                    {

                        var db = new NWConsole_96_RDHContext();

                        Console.WriteLine("Choose a category to edit:");
                        var category1 = GetCategory(db);
                        if(category1 != null)
                        {
                            Categories UpdatedCategory = new Categories();
                            UpdatedCategory.CategoryId = category1.CategoryId;
                            Console.WriteLine("Enter a Category Name:");
                            UpdatedCategory.CategoryName = Console.ReadLine();
                            Console.WriteLine("Enter a description:");
                            UpdatedCategory.Description = Console.ReadLine();

                            Categories category = db.Categories.Find(UpdatedCategory.CategoryId);
                                category.CategoryName = UpdatedCategory.CategoryName;
                                category.Description = UpdatedCategory.Description;
                                db.SaveChanges();
                                logger.Info("Category updated - {CategoryName}", UpdatedCategory.CategoryName);

                        }


                    }

                    else if (choice == "6")
                    {
                        var db = new NWConsole_96_RDHContext();

                        var query2 =
                        from c in db.Categories
                        join p in db.Products on c.CategoryId equals p.CategoryId
                        where p.Discontinued == false
                        orderby c.CategoryId
                        select new {c.CategoryName, p.ProductName};
                        foreach(var i in query2)
                        {
                            Console.WriteLine($"{i.CategoryName}: {i.ProductName}",i.ProductName, i.CategoryName);
                        }

                    }

                    else if (choice == "7")
                    {
                        var db = new NWConsole_96_RDHContext();

                        Console.WriteLine("Choose a category to view active products for:");
                        var category1 = GetCategory(db);
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine($"{category1.CategoryName}:");
                        Console.ForegroundColor = ConsoleColor.White;
                        var products = db.Products.Where(p => p.CategoryId == category1.CategoryId && p.Discontinued == false);
                        foreach(var p in products)
                        {
                            Console.WriteLine(p.ProductName);
                        }
                        logger.Info($"Displayed all active products for {category1.CategoryName}", category1.CategoryName);

                    }

                    else if (choice == "8")
                    {
                            var db = new NWConsole_96_RDHContext();
                            Console.WriteLine("Choose a category to delete:");
                            var category = GetCategory(db);

                            var test = db.Products.Any(p => p.CategoryId == category.CategoryId);

                            if(test == false)
                            {
                               db.DeleteCategory(category);
                               logger.Info($"successfully deleted {category.CategoryName}", category.CategoryName); 
                            }

                            if(test == true)
                            {
                                logger.Error("Can't delete categories if they will leave an orphaned Products record");
                            }
                    }

                    Console.WriteLine();
                    }

                    if(choice == "2")
                    {
                        Console.WriteLine("1) Add Product");
                        Console.WriteLine("2) Edit Product");
                        Console.WriteLine("3) Display All Products");
                        Console.WriteLine("4) Display a Specific Product");
                        Console.WriteLine("5) Delete a Product");
                        Console.WriteLine("\"q\" to quit");
                        choice = Console.ReadLine();

                        if(choice == "1")
                        {
                            var db = new NWConsole_96_RDHContext();
                            Products product = new Products();
                            Console.WriteLine("Enter a product name:");
                            product.ProductName = Console.ReadLine();
                            var Suppliers = db.Suppliers.OrderBy(b => b.SupplierId);
                            foreach(Suppliers s in Suppliers)
                            {
                                Console.WriteLine($"{s.SupplierId}: {s.CompanyName}");
                            }
                            Console.WriteLine("Select a Supplier ID:");
                            product.SupplierId = Convert.ToInt32(Console.ReadLine());
                            var Categories = db.Categories.OrderBy(b => b.CategoryId);
                            foreach(Categories c in Categories)
                            {
                                Console.WriteLine($"{c.CategoryId}: {c.CategoryName}");
                            }
                            Console.WriteLine("Select a Category ID:"); 
                            product.CategoryId = Convert.ToInt32(Console.ReadLine());
                            Console.WriteLine("Enter a quantity per unit:");
                            product.QuantityPerUnit = Console.ReadLine();
                            Console.WriteLine("Enter the number of units in stock:");
                            product.UnitsInStock = Convert.ToInt16(Console.ReadLine());
                            Console.WriteLine("Enter a Unit Price:");
                            product.UnitPrice = Convert.ToInt32(Console.ReadLine());
                            Console.WriteLine("Enter the number of units on order:");
                            product.UnitsOnOrder = Convert.ToInt16(Console.ReadLine());
                            Console.WriteLine("Enter the number of units to reorder at:");
                            product.ReorderLevel = Convert.ToInt16(Console.ReadLine());
                            Console.WriteLine("This product is discontinued (true/false):");
                            product.Discontinued = Convert.ToBoolean(Console.ReadLine());

                        ValidationContext context = new ValidationContext(product, null, null);
                        List<ValidationResult> results = new List<ValidationResult>();

                        var isValid = Validator.TryValidateObject(product, context, results, true);
                        if (isValid)
                        {
                            // check for unique name
                            if (db.Products.Any(p => p.ProductName == product.ProductName))
                            {
                                // generate validation error
                                isValid = false;
                                results.Add(new ValidationResult("Name exists", new string[] { "ProductName" }));
                            }
                            else
                            {
                                logger.Info("Validation passed");
                                db.Products.Add(product);
                                db.SaveChanges();
                                logger.Info("Product added - {ProductName}", product.ProductName);
                            }
                        }
                        if (!isValid)
                        {
                            foreach (var result in results)
                            {
                                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                            }
                        }

                        }

                        if(choice == "2")
                        {
                            var db = new NWConsole_96_RDHContext();

                            Console.WriteLine("Choose a product to edit:");
                            var product1 = GetProduct(db);
                            if(product1 != null)
                            {
                            Products UpdatedProduct = new Products();
                            UpdatedProduct.ProductId = product1.ProductId;
                            Console.WriteLine("Enter a product name:");
                            UpdatedProduct.ProductName = Console.ReadLine();
                            var Suppliers = db.Suppliers.OrderBy(b => b.SupplierId);
                            foreach(Suppliers s in Suppliers)
                            {
                                Console.WriteLine($"{s.SupplierId}: {s.CompanyName}");
                            }
                            Console.WriteLine("Select a Supplier ID:");
                            UpdatedProduct.SupplierId = Convert.ToInt32(Console.ReadLine());
                            var Categories = db.Categories.OrderBy(b => b.CategoryId);
                            foreach(Categories c in Categories)
                            {
                                Console.WriteLine($"{c.CategoryId}: {c.CategoryName}");
                            }
                            Console.WriteLine("Select a Category ID:"); 
                            UpdatedProduct.CategoryId = Convert.ToInt32(Console.ReadLine());
                            Console.WriteLine("Enter a quantity per unit:");
                            UpdatedProduct.QuantityPerUnit = Console.ReadLine();
                            Console.WriteLine("Enter the number of units in stock:");
                            UpdatedProduct.UnitsInStock = Convert.ToInt16(Console.ReadLine());
                            Console.WriteLine("Enter a Unit Price:");
                            UpdatedProduct.UnitPrice = Convert.ToInt32(Console.ReadLine());
                            Console.WriteLine("Enter the number of units on order:");
                            UpdatedProduct.UnitsOnOrder = Convert.ToInt16(Console.ReadLine());
                            Console.WriteLine("Enter the number of units to reorder at:");
                            UpdatedProduct.ReorderLevel = Convert.ToInt16(Console.ReadLine());
                            Console.WriteLine("This product is discontinued (true/false):");
                            UpdatedProduct.Discontinued = Convert.ToBoolean(Console.ReadLine());


                                Products product = db.Products.Find(UpdatedProduct.ProductId);
                                product.ProductName = UpdatedProduct.ProductName;
                                product.SupplierId = UpdatedProduct.SupplierId;
                                product.CategoryId = UpdatedProduct.CategoryId;
                                product.QuantityPerUnit = UpdatedProduct.QuantityPerUnit;
                                product.UnitsInStock = UpdatedProduct.UnitsInStock;
                                product.UnitsOnOrder = UpdatedProduct.UnitsOnOrder;
                                product.ReorderLevel = UpdatedProduct.ReorderLevel;
                                product.Discontinued = UpdatedProduct.Discontinued;
                                db.SaveChanges();
                                logger.Info("Product updated - {ProductName}", UpdatedProduct.ProductName);
                            }

                        }

                        if(choice == "3")
                        {
                            Console.WriteLine("1) Display All Products");
                            Console.WriteLine("2) Display Active Products");
                            Console.WriteLine("3) Display Discontinued Products");
                            choice = Console.ReadLine();

                            if(choice == "1")
                            {
                                var db = new NWConsole_96_RDHContext();
                                Console.Clear();
                                Console.WriteLine("All Products:");

                                var products = db.Products.Where(p => p.Discontinued == true);


                                Console.ForegroundColor = ConsoleColor.Magenta;
                                Console.WriteLine("Discontinued Proudcts:");
                                foreach(Products p in products)
                                {
                                  Console.WriteLine(p.ProductName);
                                }

                                Console.WriteLine();

                                var products1 = db.Products.Where(p => p.Discontinued == false);

                                Console.ForegroundColor = ConsoleColor.Green;

                                Console.WriteLine("Active Products:");
                                foreach(Products p in products1)
                                {
                                  Console.WriteLine(p.ProductName);
                                }

                                Console.ForegroundColor = ConsoleColor.White;

                                Console.WriteLine();


                                logger.Info("Dispalyed all products");
                                Console.WriteLine();
                            }

                            if(choice == "2")
                            {
                                var db = new NWConsole_96_RDHContext();
                                Console.Clear();

                                Console.ForegroundColor = ConsoleColor.Green;

                                Console.WriteLine("Active Products:");

                                var products = db.Products.Where(p => p.Discontinued == false);

                                foreach(Products p in products)
                                {
                                  Console.WriteLine(p.ProductName);
                                }

                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine();


                                logger.Info("Displayed active products");
                                Console.WriteLine();
                               
                            } 

                            if(choice == "3")
                            {
                                var db = new NWConsole_96_RDHContext();
                                Console.Clear();
                                Console.ForegroundColor = ConsoleColor.Magenta;
                                Console.WriteLine("Discontinued Products:");

                                var products = db.Products.Where(p => p.Discontinued == true);

                                foreach(Products p in products)
                                {
                                  Console.WriteLine(p.ProductName);
                                }
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine();

                                logger.Info("Displayed discontinued products");
                                Console.WriteLine();
                            }

                        }

                        if(choice == "4")
                        {
                            var db = new NWConsole_96_RDHContext();

                            Console.WriteLine("Choose a product to view");
                            var product = GetProduct(db);
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine($"{product.ProductId} | {product.ProductName} | {product.QuantityPerUnit} | ${product.UnitPrice} | {product.UnitsInStock} | {product.UnitsOnOrder} | {product.ReorderLevel} | {product.Discontinued}");
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.White;
                            logger.Info($"Displayed {product.ProductName}");
                            Console.WriteLine();
                            
                        }

                        if(choice == "5")
                        {
                            var db = new NWConsole_96_RDHContext();
                            Console.WriteLine("Choose a product to delete:");
                            var product = GetProduct(db);

                            var test = db.OrderDetails.Any(o => o.ProductId == product.ProductId);

                            if(test == false)
                            {
                               db.DeleteProduct(product);
                               logger.Info($"successfully deleted {product.ProductName}", product.ProductName); 
                            }

                            if(test == true)
                            {
                                logger.Error("Can't delete products if they will leave an orphaned OrderDetails record");
                            }
                        }
                    }

                } while (choice.ToLower() != "q");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            logger.Info("Program ended");
        }

        public static Products GetProduct(NWConsole_96_RDHContext db)
        {

            var products = db.Products.OrderBy(b => b.ProductId);
            foreach (Products p in products)
            {
                Console.WriteLine($"{p.ProductId}: {p.ProductName}");
            }
            if (int.TryParse(Console.ReadLine(), out int ProductId))
            {
                Products product = db.Products.FirstOrDefault(b => b.ProductId == ProductId);
                if (product != null)
                {
                    return product;
                }
            }
            logger.Error("Invalid Product ID");
            return null;

            
        }

        public static Categories GetCategory(NWConsole_96_RDHContext db)
        {
            var categories = db.Categories.OrderBy(c => c.CategoryId);
            foreach (Categories c in categories)
            {
                Console.WriteLine($"{c.CategoryId}: {c.CategoryName}");

            }
            if (int.TryParse(Console.ReadLine(), out int CategoryId))
            {
                Categories category = db.Categories.FirstOrDefault(b => b.CategoryId == CategoryId);
                if (category != null)
                {
                    return category;
                }
            }
            logger.Error("Invalid Product ID");
            return null;
        }

        
    }
}
