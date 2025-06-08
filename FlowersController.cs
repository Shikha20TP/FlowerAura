using CsvHelper;
using CsvHelper.Configuration;
using FlowerAura.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FlowerAura.Controllers
{
    public class FlowersController : Controller
    {
        private readonly FloralAuraContext _context;
        private readonly IWebHostEnvironment _environment;

        public object JsonRequestBehavior { get; private set; }

        public FlowersController(FloralAuraContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;

        }
        [HttpGet]
        public IActionResult Addflower()
        {
            return View();
        }


        public IActionResult UploadCsv() => View();

        [HttpPost]
        public async Task<IActionResult> UploadCsv(FileUploadViewModel model)
        {
            if (model.CsvFile == null || model.CsvFile.Length == 0)
                return BadRequest("Please select a valid CSV file.");

            var filePath = Path.Combine("wwwroot/uploads", "flowers.csv");

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.CsvFile.CopyToAsync(fileStream);
            }

            return RedirectToAction(nameof(ProcessCsv));
        }

        [HttpGet]
        public async Task<IActionResult> ProcessCsv()
        {
            var filePath = Path.Combine("wwwroot/uploads", "flowers.csv");

            if (!System.IO.File.Exists(filePath))
                return BadRequest("CSV file not found. Please upload the file first.");

            using var reader = new StreamReader(filePath);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,   
                MissingFieldFound = null  
            };

            using var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<FlowerMap>();  

            var flowers = csv.GetRecords<Flowers>().ToList();
            if (flowers.Any())
            {
                await _context.Flowers.AddRangeAsync(flowers);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        public IActionResult List()
        {
            var flowers = _context.Flowers.ToList();
            return View(flowers);
        }


        [HttpPost]
        public async Task<IActionResult> Addflower(Flowers model)
        {
            if (ModelState.IsValid)
            {
                
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "images");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder); 
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    
                    model.Image = Path.Combine("images", uniqueFileName).Replace("\\", "/");
                }
                else
                {
                    model.Image = null; 
                }

                _context.Flowers.Add(model);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Flower added successfully!";
                return RedirectToAction("Addflower");
            }

            return View(model); 
        }



       


        public IActionResult Index()
        {
            var flowers = _context.Flowers.ToList();
            return View(flowers);
        }

        public IActionResult Details(int id)
        {
            var flower = _context.Flowers.FirstOrDefault(f => f.Id == id);
            if (flower == null)
            {
                return NotFound();
            }

            return View(flower);
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var flower = _context.Flowers.FirstOrDefault(f => f.Id == id);
            if (flower == null)
            {
                return NotFound();
            }

            return View(flower);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, Flowers model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var flower = await _context.Flowers.FindAsync(id);
                if (flower == null)
                    return NotFound();

                
                flower.FlowerName = model.FlowerName;
                flower.Category = model.Category;
                flower.Amount = model.Amount;
                flower.Description = model.Description;

                
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "images");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                   
                    if (!string.IsNullOrEmpty(flower.Image))
                    {
                        string oldImagePath = Path.Combine(_environment.WebRootPath, flower.Image);
                        if (System.IO.File.Exists(oldImagePath))
                            System.IO.File.Delete(oldImagePath);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(stream);
                    }

                    flower.Image = Path.Combine("images", uniqueFileName).Replace("\\", "/");
                }
                else
                {
                    
                    flower.Image = model.Image;
                }

                await _context.SaveChangesAsync();
                TempData["Message"] = "Flower updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }



        [HttpGet]
        public IActionResult Delete(int id)
        {
            var flower = _context.Flowers.FirstOrDefault(f => f.Id == id);
            if (flower == null)
            {
                return NotFound();
            }

            return View(flower);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var flower = await _context.Flowers.FindAsync(id);
            if (flower == null)
            {
                return NotFound();
            }

            _context.Flowers.Remove(flower);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Flower deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult SearchByCategory(string category)
        {
            var flowers = string.IsNullOrEmpty(category)
                ? _context.Flowers.ToList()
                : _context.Flowers.Where(f => f.Category != null && f.Category.ToLower().Contains(category.ToLower())).ToList();

            ViewBag.Category = category;
            return View("Index", flowers); 
        }



    }

}