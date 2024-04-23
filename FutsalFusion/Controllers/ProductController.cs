using FutsalFusion.Application.DTOs.Product;
using FutsalFusion.Application.Interfaces.Repositories.Base;
using FutsalFusion.Application.Interfaces.Services;
using FutsalFusion.Attribute;
using FutsalFusion.Controllers.Base;
using FutsalFusion.Domain.Constants;
using FutsalFusion.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FutsalFusion.Controllers;

public class ProductController : BaseController<ProductController>
{
    private readonly IGenericRepository _genericRepository;
    private readonly IFileUploadService _fileUploadService;

    public ProductController(IGenericRepository genericRepository, IFileUploadService fileUploadService)
    {
        _genericRepository = genericRepository;
        _fileUploadService = fileUploadService;
    }

    public IActionResult Index()
    {
        var userId = UserDetail.UserId;

        var roleId = UserDetail.RoleId;

        var user = _genericRepository.GetById<AppUser>(userId);
        
        var role = _genericRepository.GetById<AppRole>(roleId);

        var roleName = role.Name;

        var futsal = _genericRepository.GetFirstOrDefault<Futsal>(u => u.FutsalOwnerId == user.Id);
        
        var products = role.Name == "Futsal"
            ? _genericRepository.Get<Kit>(x =>
                x.FutsalId == futsal!.Id)
            : _genericRepository.Get<Kit>();

        var result = new ProductResponseDto()
        {
            Role = roleName,
            IsEditable = roleName == "Futsal",
            ProductDetailsList = products.Select(x => new ProductDetails()
            {
                Category = GetCategoryName(x.Type),
                Id = x.Id,
                RegisteredDate = x.CreatedAt.ToString("dd-MM-yyyy"),
                IsActive = x.IsActive,
                ImageURL = x.ImageURL.Split(",").FirstOrDefault() ?? "sample-profile.png",
                Price = $"Rs {x.Price}",
                Title = x.Title,
                Quantity = new Random().Next(1, 100)
            }).ToList()
        };
        
            return View(result);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new ProductRequestDto());
    }
    
    [HttpPost]
    public IActionResult Upload()
    {
        var files = HttpContext.Request.Form.Files;
        
        if (files.Any())
        {
            foreach (var file in files)
            {
                var imageFilePath = _fileUploadService.UploadDocument(Constants.FilePath.ProductImagesFilePath, file);

                var images = HttpContext.Session.GetComplexData<List<string>?>("images");

                images ??= [];
                
                images.Add(imageFilePath);
                
                HttpContext.Session.SetComplexData("images", images);
            }
        }

        return Json(new
        {
            success = 1
        });
    }
    
    [HttpPost]
    public IActionResult Create(ProductRequestDto product)
    {
        var userId = UserDetail.UserId;

        var user = _genericRepository.GetById<AppUser>(userId);

        var futsal = _genericRepository.GetFirstOrDefault<Futsal>(x => x.FutsalOwnerId == user.Id);

        var images = HttpContext.Session.GetComplexData<List<string>?>("images") ?? [];

        var productModel = new Kit()
        {
            CreatedAt = DateTime.Now,
            CreatedBy = user.Id,
            FutsalId = futsal!.Id,
            Description = product.Description,
            Type = product.Type,
            Price = product.Price,
            Unit = product.Unit,
            IsActive = true,
            Title = product.Title,
            ImageURL = string.Join(",", images)
        };

        _genericRepository.Insert(productModel);

        TempData["Success"] = "Product / Kit Successfully Created";

        return RedirectToAction("Index");
    }
    
    public IActionResult Detail(Guid productId)
    {
        var product = _genericRepository.GetById<Kit>(productId);

        var orders = _genericRepository.Get<OrderDetail>(x => x.KitId == product.Id);

        var orderDetails = orders as OrderDetail[] ?? orders.ToArray();
        
        var result = new ProductDetailsDto()
        {
            Id = product.Id,
            Name = product.Title,
            Description = product.Description,
            ImageUrl = product.ImageURL.Split(",").FirstOrDefault() ?? "sample-profile.png",
            Images = product.ImageURL.Split(",").ToList(),
            Date = product.CreatedAt.ToString("dd-MM-yyyy"),
            Revenue = $"Rs {orderDetails.Sum(x => x.KitTotalAmount)}",
            AvailableStock = new Random().Next(1, 100),
            RetailPrice = $"Rs {product.Price}",
            IsDiscountAvailable = false,
            NumberOfOrders = orderDetails.Count()
        };
        
        return View(result);
    }

    [HttpPost]
    public IActionResult AddProductsToCart(ProductDetailsDto productDetails)
    {
        var userId = UserDetail.UserId;
        
        var user = _genericRepository.GetById<AppUser>(userId);

        var product = _genericRepository.GetById<Kit>(productDetails.Id);

        var shoppingCart =
            _genericRepository.GetFirstOrDefault<Cart>(x => x.KitId == product.Id && x.UserId == user.Id);

        if (shoppingCart == null)
        {
            var cartModel = new Cart()
            {
                Count = productDetails.Quantity,
                UserId = user.Id,
                KitId = product.Id,
                CreatedAt = DateTime.Now,
                CreatedBy = user.Id,
                IsActive = true
            };

            _genericRepository.Insert(cartModel);
        }
        else
        {
            shoppingCart.Count += productDetails.Quantity;
            
            _genericRepository.Update(shoppingCart);
        }

        TempData["Success"] = "Kits and Accessories successfully added to cart.";
        
        return RedirectToAction("Index");
    }
    
    private string GetCategoryName(int categoryId)
    {
        var category = categoryId switch
        {
            1 => "Boots",
            2 => "Kits / Accessories",
            3 => "Jerseys",
            4 => "Miscellaneous",
            _ => ""
        };

        return category;
    }
}