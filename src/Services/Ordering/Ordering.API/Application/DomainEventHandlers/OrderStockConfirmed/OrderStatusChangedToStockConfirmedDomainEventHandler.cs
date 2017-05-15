﻿namespace Ordering.API.Application.DomainEventHandlers.OrderStockConfirmed
{
    using MediatR;
    using Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.OrderAggregate;
    using Microsoft.Extensions.Logging;
    using Domain.Events;
    using System;
    using System.Threading.Tasks;
    using Ordering.API.Application.IntegrationCommands.Commands;
    using Ordering.API.Application.IntegrationEvents;

    public class OrderStatusChangedToStockConfirmedDomainEventHandler
                   : IAsyncNotificationHandler<OrderStatusChangedToStockConfirmedDomainEvent>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILoggerFactory _logger;
        private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;

        public OrderStatusChangedToStockConfirmedDomainEventHandler(
            IOrderRepository orderRepository, ILoggerFactory logger,
            IOrderingIntegrationEventService orderingIntegrationEventService)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _orderingIntegrationEventService = orderingIntegrationEventService;
        }

        public async Task Handle(OrderStatusChangedToStockConfirmedDomainEvent orderStatusChangedToStockConfirmedDomainEvent)
        {
            await _orderRepository.UnitOfWork.SaveEntitiesAsync();

            _logger.CreateLogger(nameof(OrderStatusChangedToStockConfirmedDomainEventHandler))
                .LogTrace($"Order with Id: {orderStatusChangedToStockConfirmedDomainEvent.OrderId} has been successfully updated with " +
                          $"a status order id: {OrderStatus.StockConfirmed.Id}");

            var payOrderCommandMsg = new PayOrderCommandMsg(orderStatusChangedToStockConfirmedDomainEvent.OrderId);
            await _orderingIntegrationEventService.SaveEventAndOrderingContextChangesAsync(payOrderCommandMsg);
            await _orderingIntegrationEventService.PublishThroughEventBusAsync(payOrderCommandMsg);
        }
    }  
}