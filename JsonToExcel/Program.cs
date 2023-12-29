using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JsonToExcel
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string jsonPath = @"E:\oxai\data\current\listings\listing_master_mirror.json";
            string excelPath = @"E:\oxai\data\current\listings\listing_master_mirror.xlsx";
            var jsonString = File.ReadAllText(jsonPath);

            JObject jsonObject = JObject.Parse(jsonString);
            JArray listings = jsonObject["listings"] as JArray;
            string[] exclude_listing = new string[] { 
                "merged",
                "sequence",
                "owconnect_category_id",
                "owconnect_category_name",
                "hq_lat",
                "hq_lon",
                "hq_pos"
            };
            string[] exclude_branch= new string[] { "listing_id" };
            string[] exclude_people = new string[] {"id", "branch_id" };
            /*
        "website_url": "unknown-company-website.html",
        "logo": "img/corp/generic-corp-logo-13.png",
        */

            List<Dictionary<string, object>> flattenedListings = new();
            foreach (var listing in listings)
            {
                // Initialize a new dictionary for each flattened listing
                var flattenedListing = new Dictionary<string, object>();

                // Add listing fields to the flattenedListing
                foreach (var prop in listing.Children<JProperty>())
                {
                    var key = prop.Name;
                    var value = prop.Value;
                    if (exclude_listing.Contains(key)) { continue; }
                    if (key == "website_url" &&
                        (string)value == "unknown-company-website.html")
                    {
                        value = null;
                    }
                    if (key == "logo" &&
                        ((string)value).StartsWith("img/corp/generic-"))
                    {
                        value = null;
                    }
                    if (key == "contact_thumb" &&
                        ((string)value).StartsWith("img/people/thumb/generic-person"))
                    {
                        value = null;
                    }
                    if (key == "goods_and_services" && ((JArray)value).Count > 0)
                    {
                        if (((JArray)value).Count > 0)
                        {
                            value = string.Join("|", value.ToObject<string[]>().Select(s => s.Trim()));
                        }
                        else
                        {
                            value = null;
                        }
                    }
                    if (key == "why_featured")
                    {
                        if (((JArray)value).Count > 0)
                        {
                            value = string.Join("|", value.ToObject<string[]>().Select(s => s.Trim()));
                        }
                        else
                        {
                            value = null;
                        }
                    }

                    if (prop.Name != "branches")
                    {
                        flattenedListing.Add(key, value);
                    }
                }

                // Handle branches
                int branchIndex = 1;
                foreach (var branch in listing["branches"])
                {
                    // Add branch fields to the flattenedListing with prefix
                    foreach (var prop in branch.Children<JProperty>())
                    {
                        if (exclude_branch.Contains(prop.Name)) { continue; }
                        if (prop.Name == "people" || prop.Name == "geocode") continue; // Skip people and geocode here
                        string keyName = $"branch_{branchIndex}_{prop.Name}";
                        flattenedListing.Add(keyName, prop.Value);
                    }

                    // Flatten geocode address
                    if (branch["geocode"] != null)
                    {
                        if (branch["geocode"]["results"] != null)
                        {
                            if (branch["geocode"]["results"].Count() > 0)
                            {
                                if (branch["geocode"]["results"][0]["address"] != null)
                                {
                                    var address = branch["geocode"]["results"][0]["address"];
                                    foreach (var prop in address.Children<JProperty>())
                                    {
                                        string keyName = $"branch_{branchIndex}_{prop.Name}";
                                        flattenedListing.Add(keyName, prop.Value);
                                    }
                                }
                                if (branch["geocode"]["results"][0]["position"] != null)
                                {
                                    var position = branch["geocode"]["results"][0]["position"];
                                    var lat = position["lat"];
                                    var lon = position["lon"];
                                    flattenedListing.Add($"branch_{branchIndex}_lat", lat);
                                    flattenedListing.Add($"branch_{branchIndex}_lon", lon);
                                }
                            }
                        }
                    }
                    
                    // Handle people
                    int peopleIndex = 1;
                    var people = branch["people"];
                    if (people != null)
                    {
                        foreach (var person in people)
                        {
                            foreach (var prop in person.Children<JProperty>())
                            {
                                if (exclude_people.Contains(prop.Name)) { continue; }
                                string keyName = $"branch_{branchIndex}_people_{peopleIndex}_{prop.Name}";
                                flattenedListing.Add(keyName, prop.Value);
                            }
                            peopleIndex++;
                        }
                    }

                    branchIndex++;
                }

                flattenedListings.Add(flattenedListing);

                // At this point, 'flattenedListing' is a single object with all listing, branch, geocode, and people fields flattened
                // You can now add this to a larger collection, or use it as needed

                // Example: Print out the flattened listing
                //foreach (var kvp in flattenedListing)
                //{
                //    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
                //}
            }
            //foreach(var listing in flattenedListings)
            //{
            //    foreach(var kvp in listing)
            //    {
            //        Console.WriteLine($"{kvp.Key}: {kvp.Value}");
            //    }
            //}
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Or your appropriate license context
            using (var package = new ExcelPackage())
            {
                // Add a new worksheet to the empty workbook
                var worksheet = package.Workbook.Worksheets.Add("Listings");

                // Determine the superset of all keys/column headers
                HashSet<string> allKeys = new HashSet<string>();
                foreach (var listing in flattenedListings)
                {
                    foreach (var key in listing.Keys)
                    {
                        allKeys.Add(key);
                    }
                }

                // Create the headers in the first row
                int columnIndex = 1;
                foreach (string header in allKeys)
                {
                    worksheet.Cells[1, columnIndex].Value = header;
                    columnIndex++;
                }

                // Fill in the data
                int rowIndex = 2; // Start from the second row, as the first row is headers
                foreach (var listing in flattenedListings)
                {
                    columnIndex = 1; // Reset for each listing
                    foreach (string header in allKeys)
                    {
                        worksheet.Cells[rowIndex, columnIndex].Value = listing.ContainsKey(header) ? listing[header]?.ToString() : null;
                        columnIndex++;
                    }
                    rowIndex++;
                }

                // Save to the specified path
                FileInfo excelFileInfo = new FileInfo(excelPath);
                package.SaveAs(excelFileInfo);
            }

            if (File.Exists(excelPath))
            {
                // Start Excel and open the file in read-only mode
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "EXCEL.EXE",
                    Arguments = $"/r \"{excelPath}\"",
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
            else
            {
                Console.WriteLine("Excel file not found.");
            }
            Console.WriteLine($"Excel file created at {excelPath}");
        }
    }
}
