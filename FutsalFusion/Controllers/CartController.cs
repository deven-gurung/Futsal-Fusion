using FutsalFusion.Application.DTOs.Order;
using FutsalFusion.Application.Interfaces.Repositories.Base;
using FutsalFusion.Controllers.Base;
using FutsalFusion.Domain.Constants;
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

        var inShoppingCart = kitsInShoppingCart as Cart[] ?? kitsInShoppingCart.ToArray();
        
        var totalAmount = (from cart in inShoppingCart let product = _genericRepository.GetById<Kit>(cart.KitId) select product.Price * cart.Count).Sum();

        var result = new CartRequestDto()
        {
            Discount = 0,
            TotalAmount = totalAmount,
            EstimatedTax = totalAmount * (decimal)0.13d,
            ShippingCharge = 500,
            GrandTotal = 500m + totalAmount * 0.13m + totalAmount,
            CartProductsList = inShoppingCart.Select(x => new CartProducts()
            {   
                Id = x.KitId,
                CartId = x.Id,
                ImageUrl = _genericRepository.GetById<Kit>(x.KitId).ImageURL.Split(",").FirstOrDefault()!,
                Price = $"Rs {_genericRepository.GetById<Kit>(x.KitId).Price * x.Count}",
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

    [HttpPost]
    public IActionResult PlaceOrder(string? description)
    {
        var userId = UserDetail.UserId;

        var user = _genericRepository.GetById<AppUser>(userId);

        var shoppingCartProducts = _genericRepository.Get<Cart>(x => x.UserId == user.Id);

        var cartEntityList = shoppingCartProducts as Cart[] ?? shoppingCartProducts.ToArray();

        var kitId = cartEntityList.FirstOrDefault()!.KitId;

        var kit = _genericRepository.GetById<Kit>(kitId);

        var futsal = _genericRepository.GetById<Futsal>(kit.FutsalId);

        var futsalOwner = _genericRepository.GetById<AppUser>(futsal.FutsalOwnerId);
        
        var orderDetails = cartEntityList.Select(shoppingCartProduct => new OrderDetail()
            {
                KitId = shoppingCartProduct.KitId,
                Quantity = shoppingCartProduct.Count,
                KitTotalAmount = _genericRepository.GetById<Kit>(shoppingCartProduct.KitId).Price * shoppingCartProduct.Count,
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedBy = user.Id,
            })
            .ToList();
        
        var orderTotal = new Order()
        {
            Description = description ?? "Kit Requested for Play Arrangements.",
            OrderStatus = 1,
            PaymentStatus = 1,
            PaymentDate = null,
            CreatedAt = DateTime.Now,
            CreatedBy = user.Id,
            OrderedDate = DateTime.Now,
            UserId = user.Id,
            OrderTotal = orderDetails.Sum(x => x.KitTotalAmount),
            OrderDetails = orderDetails,
        };

        var orderId = _genericRepository.Insert(orderTotal);

        TempData["Success"] = "Kits Successfully Ordered";

        _genericRepository.RemoveMultipleEntity(cartEntityList);

        var notification = new Notification()
        {
            CreatedAt = DateTime.Now,
            CreatedBy = user.Id,
            IsActive = true,
            Title = "A new order has been placed",
            Content = $"A new order with the identifier {orderId.ToString()[..8]} has been placed at your futsal.",
            SenderId = user.Id,
            ReceiverId = futsalOwner.Id,
            ReceiverEntity = (int)Roles.Futsal,
            SenderEntity = (int)Roles.Player,
        };

        _genericRepository.Insert(notification);
        
        return RedirectToAction("Index", "Product");
    }
}