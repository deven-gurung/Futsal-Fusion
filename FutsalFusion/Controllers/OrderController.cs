using FutsalFusion.Application.DTOs.Order;
using FutsalFusion.Application.Interfaces.Repositories.Base;
using FutsalFusion.Controllers.Base;
using FutsalFusion.Domain.Constants;
using FutsalFusion.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FutsalFusion.Controllers;

public class OrderController : BaseController<OrderController>
{
    private readonly IGenericRepository _genericRepository;

    public OrderController(IGenericRepository genericRepository)
    {
        _genericRepository = genericRepository;
    }

    public IActionResult Index()
    {
        var userId = UserDetail.UserId;

        var user = _genericRepository.GetById<AppUser>(userId);

        var role = _genericRepository.GetById<AppRole>(user.RoleId);

        List<OrderDetailsDto> orders;

        if (role.Name == "Futsal")
        {
            var futsal = _genericRepository.GetFirstOrDefault<Futsal>(x => x.FutsalOwnerId == user.Id);

            var products = _genericRepository.Get<Kit>(x => x.FutsalId == futsal!.Id);

            var orderDetails = _genericRepository.Get<OrderDetail>(x => products.Select(z => z.Id).Contains(x.KitId));

            var orderModels =
                _genericRepository.Get<Order>(x => orderDetails.Select(z => z.OrderId).Distinct().Contains(x.Id));

            orders = orderModels.Select(x => new OrderDetailsDto()
            {
                Id = x.Id.ToString()[..8],
                OrderId = x.Id,
                CustomerName = _genericRepository.GetById<AppUser>(x.UserId).FullName,
                OrderedDate = x.OrderedDate.ToString("MMMM dd yyyy"),
                OrderedTime = x.OrderedDate.ToString("hh:mm tt"),
                OrderStatus = x.OrderStatus,
                PaymentStatus = x.PaymentStatus,
                OrderTotal = $"Rs {x.OrderTotal}"
            }).ToList();
        }
        else
        {
            var orderModels = _genericRepository.Get<Order>(x => x.UserId == user.Id);
            
            orders = orderModels.Select(x => new OrderDetailsDto()
            {
                Id = x.Id.ToString()[..8],
                OrderId = x.Id,
                CustomerName = _genericRepository.GetById<AppUser>(x.UserId).FullName,
                OrderedDate = x.OrderedDate.ToString("MMMM dd yyyy"),
                OrderedTime = x.OrderedDate.ToString("hh:mm tt"),
                OrderStatus = x.OrderStatus,
                PaymentStatus = x.PaymentStatus,
                OrderTotal = $"Rs {x.OrderTotal}"
            }).ToList();
        }

        return View(orders);
    }

    public IActionResult OrderDetails(Guid orderId)
    {
        var userId = UserDetail.UserId;

        var user = _genericRepository.GetById<AppUser>(userId);

        var role = _genericRepository.GetById<AppRole>(user.RoleId);
        
        var order = _genericRepository.GetById<Order>(orderId);

        var orderDetails = _genericRepository.Get<OrderDetail>(x => x.OrderId == order.Id);

        var orderDetailsList = orderDetails as OrderDetail[] ?? orderDetails.ToArray();

        var result = new OrderResponseDto()
        {
            Id = order.Id.ToString()[..8],
            OrderId = order.Id,
            IsEditable = role.Name == "Futsal",
            OrderStatus = order.OrderStatus,
            Description = order.Description,
            OrderedDate = order.OrderedDate.ToString("dd-MM-yyyy hh:mm:ss tt"),
            Discount = 0,
            TotalAmount = orderDetailsList.Sum(x => x.KitTotalAmount),
            EstimatedTax = orderDetailsList.Sum(x => x.KitTotalAmount) * (decimal)0.13d,
            ShippingCharge = 500,
            GrandTotal = 500m + orderDetailsList.Sum(x => x.KitTotalAmount) * 0.13m +
                         orderDetailsList.Sum(x => x.KitTotalAmount),
            ProductsList = orderDetailsList.Select(x => new Products()
            {
                Id = x.KitId,
                TotalAmount = x.KitTotalAmount,
                Title = _genericRepository.GetById<Kit>(x.KitId).Title,
                AddedDate = x.CreatedAt.ToString("dd-MM-yyyy hh:mm:ss tt"),
                ImageUrl = _genericRepository.GetById<Kit>(x.KitId).ImageURL.Split(",").FirstOrDefault() ?? "sample-profile.png",
                UnitPrice = $"Rs {_genericRepository.GetById<Kit>(x.KitId).Price}",
                Quantity = x.Quantity
            }).ToList(),
        };

        return View(result);
    }
    
    public IActionResult UpdateOrderDetailsStatus(Guid orderId)
    {
        var userId = UserDetail.UserId;

        var user = _genericRepository.GetById<AppUser>(userId);
        
        var order = _genericRepository.GetById<Order>(orderId);

        order.OrderStatus += 1;
        
        _genericRepository.Update(order);

        var player = _genericRepository.GetById<AppUser>(order.UserId);

        var notification = new Notification()
        {
            Content = "An update to your order has been made, please check your orders indices",
            Title = "Order Update",
            SenderId = user.Id,
            ReceiverId = player.Id,
            SenderEntity = (int)Roles.Futsal,
            ReceiverEntity = (int)Roles.Player,
            CreatedAt = DateTime.Now,
            CreatedBy = user.Id,
            IsActive = true,
        };

        _genericRepository.Insert(notification);

        TempData["Success"] = "Order Successfully Updated";
        
        return RedirectToAction("Index");
    }
    
    public IActionResult CancelOrderDetailsStatus(Guid orderId)
    {
        var userId = UserDetail.UserId;

        var user = _genericRepository.GetById<AppUser>(userId);
        
        var order = _genericRepository.GetById<Order>(orderId);

        order.OrderStatus = 4;
        
        _genericRepository.Update(order);

        var player = _genericRepository.GetById<AppUser>(order.UserId);

        var notification = new Notification()
        {
            Content = "An update to your order has been made, please check your orders indices",
            Title = "Order Update",
            SenderId = user.Id,
            ReceiverId = player.Id,
            SenderEntity = (int)Roles.Futsal,
            ReceiverEntity = (int)Roles.Player,
            CreatedAt = DateTime.Now,
            CreatedBy = user.Id,
            IsActive = true,
        };

        _genericRepository.Insert(notification);

        TempData["Success"] = "Order Successfully Updated";
        
        return RedirectToAction("Index");
    }
}