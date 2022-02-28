using AdventureWorks.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace AdventureWorks
{
    class Program
    {
        static void Main(string[] args)
        {


            Console.WriteLine("----- YHDISTÄÄ -----");

            Console.WriteLine(ProductCategoryList(new AdventureWorksLTContext())); // haetaan tiedot

            Console.WriteLine(ProductModelList(new AdventureWorksLTContext(), 3)); // haetaan tiedot

            // SqlQuery(new AdventureWorksLTContext(), 1);

            DbDataReader reader = SqlQuery(new AdventureWorksLTContext(), 29485);

            while (reader.Read())
            {
                Console.WriteLine($"{reader.GetString(0)} {reader.GetString(1)}, {reader.GetString(2)}, {reader.GetString(3)}");
            }

            Console.WriteLine("----- VALAMIS -----");

        }

        static string ProductCategoryList(AdventureWorksLTContext context)
        {
            string palautettavaMerkkijono = "";

            // haetaan listaan kaikki parent product categories
            var allParentProductCategories = context.ProductCategories
                .Where(p => p.ParentProductCategory == null)
                .ToList();

            // jokaiselle parent product categorylle haetaan subcategoryt
            foreach (ProductCategory p in allParentProductCategories)
            {
                palautettavaMerkkijono = palautettavaMerkkijono + "--" + p.ProductCategoryId + ": " + p.Name + "--\n"; // lisätään parent

                var allSubcategoriesOfParent = context.ProductCategories
                    .Where(c => c.ParentProductCategoryId == p.ProductCategoryId)
                    .ToList(); // haetaan subcategories

                foreach (ProductCategory s in allSubcategoriesOfParent)
                {
                    palautettavaMerkkijono = palautettavaMerkkijono + "\t" + s.ProductCategoryId + ": " + s.Name + "\n"; // lisätään jokainen subcategory
                }
            }

            return palautettavaMerkkijono;
        }


        static string ProductModelList(AdventureWorksLTContext context, int maara)
        {
            string palautettavaMerkkijono = "";

            var allProductModels = context.ProductModels // hakee kaikki modelit
                .OrderBy(model => model.Name) // aakkosjärjestyksessä
                .ToList();

            for (int i = 0; i < maara; i++) // hakee halutun määrän descriptioneita
            {
                palautettavaMerkkijono = palautettavaMerkkijono + allProductModels[i].Name + ": "; // lisää modelin nimen

                var modelDescriptionId = context.ProductModelProductDescriptions
                    .Where(md => md.ProductModelId == allProductModels[i].ProductModelId) // mikä model
                    .Where(md => md.Culture == "en") // englanniksi vain
                    .Select(md => md.ProductDescriptionId) // mikä id toisessa taulussa
                    .ToList();

                var description = context.ProductDescriptions // hakee kuvauksen
                    .Where(d => d.ProductDescriptionId == modelDescriptionId[0])
                    .Select(d => d.Description)
                    .ToList();

                palautettavaMerkkijono = palautettavaMerkkijono + description[0] + "\n"; // lisää kuvauksen
            }

            return palautettavaMerkkijono;
        }

        static DbDataReader SqlQuery(AdventureWorksLTContext context, int id)
        {
            string queryString = "SELECT c.FirstName, c.LastName, a.AddressLine1, a.City " +
                "FROM SalesLT.Customer c " +
                "JOIN SalesLT.CustomerAddress ca ON c.CustomerId = ca.CustomerID " +
                "JOIN SalesLT.Address a On ca.AddressID = a.AddressID " +
                "WHERE c.CustomerID=" + id;

            var connection = context.Database.GetDbConnection();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = queryString;
                connection.Open();
                return command.ExecuteReader();
                
            }     
        }
    }
}
