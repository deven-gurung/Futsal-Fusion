using FutsalFusion.Application.DTOs.Order;
using FutsalFusion.Application.Interfaces.Repositories.Base;
using FutsalFusion.Controllers.Base;
using FutsalFusion.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FutsalFusion.Controllers;

public class CartController : BaseController<CartController>
{
    private readonly IGenericRepository _genericRepository;

    public CartController(IGenericRepository genericRepository)
    {
        _genericRepository = genericRepository;
    }

    public IActionResult Index()
    {
        var userId = UserDetail.UserId;

        var user = _genericRepository.GetById<AppUser>(userId);

        var kitsInShoppingCart = _genericRepository.Get<Cart>(x => x.UserId == user.Id);

        var products = _genericRepository.Get<Kit>(x => kitsInShoppingCart.Select(z => z.KitId).Contains(x.Id));

        var productDetails = products as Kit[] ?? products.ToArray();
        
        var result = new CartRequestDto()
        {
            Discount = 0,
            TotalAmount = productDetails.Sum(x => x.Price),
            EstimatedTax = productDetails.Sum(x => x.Price) * (decimal)0.13d,
            ShippingCharge = 500,
            GrandTotal = 500m + productDetails.Sum(x => x.Price) * 0.13m + productDetails.Sum(x => x.Price),
            CartProductsList = kitsInShoppingCart.Select(x => new CartProducts()
            {   
                Id = x.KitId,
                CartId = x.Id,
                ImageUrl = _genericRepository.GetById<Kit>(x.KitId).ImageURL,
                Price = $"Rs {_genericRepository.GetById<Kit>(x.KitId).Price}",
                Quantity = x.Count,
                Title = _genericRepository.GetById<Kit>(x.KitId).Title,
                AddedDate = x.CreatedAt.ToString("dd-MM-yyyy"),
                TotalAmount = x.Count * _genericRepository.GetById<Kit>(x.KitId).Price,
                UnitPrice = _genericRepository.GetById<Kit>(x.KitId).Price
            }).ToList()
        };
        
        return View(result);
    }

    public IActionResult DeleteItemsOnCart(Guid cartId)
    {
        var cartModel = _genericRepository.GetById<Cart>(cartId);

        _genericRepository.Delete(cartModel);

        TempData["Success"] = "Product & Kit Successfully Deleted from Cart";

        return RedirectToAction("Index");
    }
}