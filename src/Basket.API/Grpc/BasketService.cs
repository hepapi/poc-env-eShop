using System.Diagnostics.CodeAnalysis;
using eShop.Basket.API.Repositories;
using eShop.Basket.API.Extensions;
using eShop.Basket.API.Model;
using Microsoft.Extensions.Logging;
using Grpc.Core;

namespace eShop.Basket.API.Grpc;

public class BasketService(
    IBasketRepository repository,
    ILogger<BasketService> logger) : Basket.BasketBase
{
    [AllowAnonymous]
    public override async Task<CustomerBasketResponse> GetBasket(
        GetBasketRequest request,
        ServerCallContext context)
    {
        var userId = context.GetUserIdentity();
        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning(
                "GetBasket called without authenticated user. Method={Method}",
                context.Method);

            return new();
        }

        logger.LogInformation(
            "GetBasket request started. UserId={UserId}, Method={Method}",
            userId,
            context.Method);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Fetching basket from repository. UserId={UserId}",
                userId);
        }

        var data = await repository.GetBasketAsync(userId);

        if (data is not null)
        {
            logger.LogInformation(
                "GetBasket completed successfully. UserId={UserId}, ItemCount={ItemCount}",
                userId,
                data.Items.Count);

            return MapToCustomerBasketResponse(data);
        }

        logger.LogInformation(
            "GetBasket completed. Basket not found. UserId={UserId}",
            userId);

        return new();
    }

    public override async Task<CustomerBasketResponse> UpdateBasket(
        UpdateBasketRequest request,
        ServerCallContext context)
    {
        var userId = context.GetUserIdentity();
        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning(
                "UpdateBasket called without authenticated user. Method={Method}",
                context.Method);

            ThrowNotAuthenticated();
        }

        logger.LogInformation(
            "UpdateBasket request started. UserId={UserId}, ItemCount={ItemCount}",
            userId,
            request.Items.Count);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Mapping UpdateBasketRequest to CustomerBasket. UserId={UserId}",
                userId);
        }

        var customerBasket = MapToCustomerBasket(userId, request);
        var response = await repository.UpdateBasketAsync(customerBasket);

        if (response is null)
        {
            logger.LogWarning(
                "UpdateBasket failed. Basket does not exist. UserId={UserId}",
                userId);

            ThrowBasketDoesNotExist(userId);
        }

        logger.LogInformation(
            "UpdateBasket completed successfully. UserId={UserId}",
            userId);

        return MapToCustomerBasketResponse(response);
    }

    public override async Task<DeleteBasketResponse> DeleteBasket(
        DeleteBasketRequest request,
        ServerCallContext context)
    {
        var userId = context.GetUserIdentity();
        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning(
                "DeleteBasket called without authenticated user. Method={Method}",
                context.Method);

            ThrowNotAuthenticated();
        }

        logger.LogInformation(
            "DeleteBasket request started. UserId={UserId}",
            userId);

        await repository.DeleteBasketAsync(userId);

        logger.LogInformation(
            "DeleteBasket completed successfully. UserId={UserId}",
            userId);

        return new();
    }

    [DoesNotReturn]
    private static void ThrowNotAuthenticated() =>
        throw new RpcException(
            new Status(
                StatusCode.Unauthenticated,
                "The caller is not authenticated."));

    [DoesNotReturn]
    private static void ThrowBasketDoesNotExist(string userId) =>
        throw new RpcException(
            new Status(
                StatusCode.NotFound,
                $"Basket with buyer id {userId} does not exist"));

    private static CustomerBasketResponse MapToCustomerBasketResponse(
        CustomerBasket customerBasket)
    {
        var response = new CustomerBasketResponse();

        foreach (var item in customerBasket.Items)
        {
            response.Items.Add(new BasketItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
            });
        }

        return response;
    }

    private static CustomerBasket MapToCustomerBasket(
        string userId,
        UpdateBasketRequest customerBasketRequest)
    {
        var response = new CustomerBasket
        {
            BuyerId = userId
        };

        foreach (var item in customerBasketRequest.Items)
        {
            response.Items.Add(new()
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
            });
        }

        return response;
    }
}
