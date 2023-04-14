using EntityFramework_Slider.Data;
using EntityFramework_Slider.Models;
using EntityFramework_Slider.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.ContentModel;
using System.Diagnostics;

namespace EntityFramework_Slider.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        public HomeController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {

            List<Slider> sliders = await _context.Sliders.ToListAsync();

            SliderInfo sliderInfo = await _context.SliderInfos.FirstOrDefaultAsync();

            IEnumerable<Blog> blogs = await _context.Blogs.Where(m=>!m.SoftDelete).ToListAsync();

            IEnumerable<Category> categories = await _context.Categories.Where(m => !m.SoftDelete).ToListAsync();

            IEnumerable<Product> products = await _context.Products.Include(m=>m.Images).ToListAsync();


            HomeVM model = new()
            {
                Sliders = sliders,
                SliderInfo = sliderInfo,
                Blogs = blogs,
                Categories = categories,
                Products = products
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>  AddBasket(int? id)
        {
            if (id is null) return BadRequest();

            Product? dbProduct = await GetProductById((int)id);

            if(dbProduct == null) return NotFound();


            List<BasketVM> basket = GetBasketDatas();
       
            BasketVM? existProduct = basket?.FirstOrDefault(m => m.Id == dbProduct.Id);

            AddProductToBasket(existProduct, dbProduct, basket);

            return RedirectToAction(nameof(Index));

        }


        private async Task<Product> GetProductById(int id)
        {
            return await _context.Products.FindAsync(id);
        }


        private List<BasketVM> GetBasketDatas()
        {
            List<BasketVM> basket;

           
            if (Request.Cookies["basket"] != null)
            {
                basket = JsonConvert.DeserializeObject<List<BasketVM>>(Request.Cookies["basket"]);
            }
            else
            {
                basket = new List<BasketVM>();
            }

            return basket;
        }

        private void AddProductToBasket(BasketVM existProduct,Product product, List<BasketVM> basket)
        {
            if (existProduct == null)
            {
                basket?.Add(new BasketVM
                {
                    Id = product.Id,

                    Count = 1

                });
            }
            else
            {
                existProduct.Count++;
            }




            Response.Cookies.Append("basket", JsonConvert.SerializeObject(basket));
        }


    }



}