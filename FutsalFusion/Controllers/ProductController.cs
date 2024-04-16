using FutsalFusion.Application.DTOs.Product;
using FutsalFusion.Application.Interfaces.Repositories.Base;
using FutsalFusion.Controllers.Base;
using FutsalFusion.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FutsalFusion.Controllers;

public class ProductController : BaseController<ProductController>
{
    private readonly IGenericRepository _genericRepository;

    public ProductController(IGenericRepository genericRepository)
    {
        _genericRepository = genericRepository;
    }

    public IActionResult Index()
    {
        var userId = UserDetail.UserId;

        var roleId = UserDetail.RoleId;

        var user = _genericRepository.GetById<AppUser>(userId);
        
        var role = _genericRepository.GetById<AppRole>(roleId);

        var roleName = role.Name;

        var products = role.Name == "Futsal"
            ? _genericRepository.Get<Kit>(x =>
                x.FutsalId == _genericRepository.GetFirstOrDefault<Futsal>(u => u.FutsalOwnerId == user.Id)!.Id)
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
                ImageURL = x.ImageURL ?? "sample-profile.png",
                Price = $"Rs {x.Price}",
                Title = x.Title,
                Quantity = new Random().Next(1, 100)
            }).ToList()
        };
        
        return View(result);
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
            ImageUrl = product.ImageURL,
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