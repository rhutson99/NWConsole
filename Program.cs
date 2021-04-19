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
                    Console.WriteLine("1) Add/Edit/Display Categories");
                    Console.WriteLine("2) Add/Edit/Display Products");
                    Console.WriteLine("\"q\" to quit");
                    choice = Console.ReadLine();
                    
                    if(choice == "1")
                    {
                    Console.WriteLine("1) Display Categories");
                    Console.WriteLine("2) Add Category");
                    Console.WriteLine("3) Display Category and related products");
                    Console.WriteLine("4) Display all Categories and their related products");
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
                                // TODO: save category to db
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
                    Console.WriteLine();
                    }

                    if(choice == "2")
                    {
                        Console.WriteLine("1) Add Product");
                        Console.WriteLine("2) Edit Product");
                        Console.WriteLine("3) Display All Products");
                        Console.WriteLine("4) Display a Specific Product");
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







                        }

                        if(choice == "2")
                        {

                        }

                        if(choice == "3")
                        {

                        }

                        if(choice == "4")
                        {
                            
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

    }
}
