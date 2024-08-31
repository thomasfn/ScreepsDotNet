using System;
using System.Collections.Generic;

namespace ScreepsDotNet.API.World
{
    public enum OrderType
    {
        Buy,
        Sell
    }

    public readonly struct TransactionDetails
    {
        public readonly struct OrderInfo
        {
            public readonly string Id;
            public readonly OrderType Type;
            public readonly double Price;

            public OrderInfo(string id, OrderType type, double price)
            {
                Id = id;
                Type = type;
                Price = price;
            }
        }

        public readonly string TransactionId;
        public readonly int Time;
        public readonly string SenderUsername;
        public readonly string RecipientUsername;
        public readonly ResourceType ResourceType;
        public readonly int Amount;
        public readonly RoomCoord From;
        public readonly RoomCoord To;
        public readonly string Description;
        public readonly OrderInfo? Order;

        public TransactionDetails(string transactionId, int time, string senderUsername, string recipientUsername, ResourceType resourceType, int amount, RoomCoord from, RoomCoord to, string description, OrderInfo? order)
        {
            TransactionId = transactionId;
            Time = time;
            SenderUsername = senderUsername;
            RecipientUsername = recipientUsername;
            ResourceType = resourceType;
            Amount = amount;
            From = from;
            To = to;
            Description = description;
            Order = order;
        }
    }

    public readonly struct MyOrderDetails
    {
        public readonly OrderDetails OrderDetails;
        public readonly bool Active;
        public readonly int TotalAmount;

        public MyOrderDetails(OrderDetails orderDetails, bool active, int totalAmount)
        {
            OrderDetails = orderDetails;
            Active = active;
            TotalAmount = totalAmount;
        }
    }

    public readonly struct OrderDetails
    {
        /// <summary>
        /// The unique order ID.
        /// </summary>
        public readonly string Id;
        /// <summary>
        /// The order creation time in game ticks. This property is absent for orders of the inter-shard market.
        /// </summary>
        public readonly int Created;
        /// <summary>
        /// The order creation time in milliseconds since UNIX epoch time. This property is absent for old orders.
        /// </summary>
        public readonly DateTime? CreatedTimestamp;
        /// <summary>
        /// Either ORDER_SELL or ORDER_BUY.
        /// </summary>
        public readonly OrderType Type;
        /// <summary>
        /// Either one of the RESOURCE_* constants or one of account-bound resources (See INTERSHARD_RESOURCES constant).
        /// </summary>
        public readonly ResourceType ResourceType;
        /// <summary>
        /// The room where this order is placed.
        /// </summary>
        public readonly RoomCoord? Room;
        /// <summary>
        /// Currently available amount to trade.
        /// </summary>
        public readonly int Amount;
        /// <summary>
        /// How many resources are left to trade via this order.
        /// </summary>
        public readonly int RemainingAmount;
        /// <summary>
        /// The current price per unit.
        /// </summary>
        public readonly double Price;

        public OrderDetails(string id, int created, DateTime? createdTimestamp, OrderType type, ResourceType resourceType, RoomCoord? room, int amount, int remainingAmount, double price)
        {
            Id = id;
            Created = created;
            CreatedTimestamp = createdTimestamp;
            Type = type;
            ResourceType = resourceType;
            Room = room;
            Amount = amount;
            RemainingAmount = remainingAmount;
            Price = price;
        }
    }

    public readonly struct PriceHistory
    {
        public readonly ResourceType ResourceType;
        public readonly DateOnly Date;
        public readonly int Transactions;
        public readonly int Volume;
        public readonly double AvgPrice;
        public readonly double StddevPrice;

        public PriceHistory(ResourceType resourceType, DateOnly date, int transactions, int volume, double avgPrice, double stddevPrice)
        {
            ResourceType = resourceType;
            Date = date;
            Transactions = transactions;
            Volume = volume;
            AvgPrice = avgPrice;
            StddevPrice = stddevPrice;
        }
    }

    public enum MarketCancelOrderResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// The order ID is not valid.
        /// </summary>
        InvalidArgs = -10
    }

    public enum MarketChangeOrderPriceResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of the room's terminal or there is no terminal.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// You don't have enough credits to pay a fee.
        /// </summary>
        NotEnoughResources = -6,
        /// <summary>
        /// The arguments provided are invalid.
        /// </summary>
        InvalidArgs = -10
    }

    public enum MarketCreateOrderResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of the room's terminal or there is no terminal.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// You don't have enough credits or resource units.
        /// </summary>
        NotEnoughResources = -6,
        /// <summary>
        /// You cannot execute more than 10 deals during one tick.
        /// </summary>
        Full = -8,
        /// <summary>
        /// The arguments provided are invalid.
        /// </summary>
        InvalidArgs = -10,
        /// <summary>
        /// The target terminal is still cooling down.
        /// </summary>
        Tired = -11
    }

    public enum MarketDealResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You don't have a terminal in the target room.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// You don't have enough credits or resource units.
        /// </summary>
        NotEnoughResources = -6,
        /// <summary>
        /// You cannot execute more than 10 deals during one tick.
        /// </summary>
        Full = -8,
        /// <summary>
        /// The arguments provided are invalid.
        /// </summary>
        InvalidArgs = -10,
        /// <summary>
        /// The target terminal is still cooling down.
        /// </summary>
        Tired = -11
    }

    public enum MarketExtendOrderResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You don't have enough credits to pay a fee.
        /// </summary>
        NotEnoughResources = -6,
        /// <summary>
        /// The arguments provided are invalid.
        /// </summary>
        InvalidArgs = -10
    }

    public interface IMarket
    {
        /// <summary>
        /// Your current credits balance.
        /// </summary>
        double Credits { get; }

        /// <summary>
        /// An array of the last 100 incoming transactions to your terminals.
        /// </summary>
        IEnumerable<TransactionDetails> IncomingTransactions { get; }

        /// <summary>
        /// An array of the last 100 outgoing transactions from your terminals.
        /// </summary>
        IEnumerable<TransactionDetails> OutgoingTransactions { get; }

        /// <summary>
        /// An object with your active and inactive buy/sell orders on the market.
        /// </summary>
        IReadOnlyDictionary<string, MyOrderDetails> Orders { get; }

        /// <summary>
        /// Estimate the energy transaction cost of StructureTerminal.send and Game.market.deal methods.
        /// </summary>
        /// <param name="amount">Amount of resources to be sent.</param>
        /// <param name="room1">The name of the first room.</param>
        /// <param name="room2">The name of the second room.</param>
        /// <returns></returns>
        int CalcTransactionCost(int amount, RoomCoord room1, RoomCoord room2);

        /// <summary>
        /// Cancel a previously created order. The 5% fee is not returned.
        /// </summary>
        /// <param name="orderId">The order ID as provided in Game.market.orders.</param>
        /// <returns></returns>
        MarketCancelOrderResult CancelOrder(string orderId);

        /// <summary>
        /// Change the price of an existing order. If newPrice is greater than old price, you will be charged (newPrice-oldPrice)*remainingAmount*0.05 credits.
        /// </summary>
        /// <param name="orderId">The order ID as provided in Game.market.orders.</param>
        /// <param name="newPrice">The new order price.</param>
        /// <returns></returns>
        MarketChangeOrderPriceResult ChangeOrderPrice(string orderId, double newPrice);

        /// <summary>
        /// Create a market order in your terminal.
        /// You will be charged price*amount*0.05 credits when the order is placed.
        /// The maximum orders count is 300 per player.
        /// You can create an order at any time with any amount, it will be automatically activated and deactivated depending on the resource/credits availability.
        /// </summary>
        /// <param name="type">The order type, either ORDER_SELL or ORDER_BUY.</param>
        /// <param name="resourceType">Either one of the RESOURCE_* constants or one of account-bound resources (See INTERSHARD_RESOURCES constant). If your Terminal doesn't have the specified resource, the order will be temporary inactive.</param>
        /// <param name="price">The price for one resource unit in credits. Can be a decimal number.</param>
        /// <param name="totalAmount">The amount of resources to be traded in total.</param>
        /// <param name="roomName">The room where your order will be created. You must have your own Terminal structure in this room, otherwise the created order will be temporary inactive. This argument is not used when resourceType is one of account-bound resources (See INTERSHARD_RESOURCES constant).</param>
        /// <returns></returns>
        MarketCreateOrderResult CreateOrder(OrderType type, ResourceType resourceType, double price, int totalAmount, RoomCoord? roomName);

        /// <summary>
        /// Execute a trade deal from your Terminal in yourRoomName to another player's Terminal using the specified buy/sell order.
        /// Your Terminal will be charged energy units of transfer cost regardless of the order resource type.
        /// You can use Game.market.calcTransactionCost method to estimate it.
        /// When multiple players try to execute the same deal, the one with the shortest distance takes precedence.
        /// You cannot execute more than 10 deals during one tick.
        /// </summary>
        /// <param name="orderId">The order ID as provided in Game.market.getAllOrders.</param>
        /// <param name="amount">The amount of resources to transfer.</param>
        /// <param name="yourRoomName">The name of your room which has to contain an active Terminal with enough amount of energy. This argument is not used when the order resource type is one of account-bound resources (See INTERSHARD_RESOURCES constant).</param>
        /// <returns></returns>
        MarketDealResult Deal(string orderId, int amount, RoomCoord? yourRoomName);

        /// <summary>
        /// Add more capacity to an existing order.
        /// It will affect remainingAmount and totalAmount properties.
        /// You will be charged price*addAmount*0.05 credits.
        /// </summary>
        /// <param name="orderId">The order ID as provided in Game.market.orders.</param>
        /// <param name="addAmount">How much capacity to add. Cannot be a negative value.</param>
        /// <returns></returns>
        MarketExtendOrderResult ExtendOrder(string orderId, int addAmount);

        /// <summary>
        /// Get other players' orders currently active on the market.
        /// It is recommended to use at least a resource type filter as fetching all orders is very slow.
        /// </summary>
        /// <param name="filterOrderType"></param>
        /// <param name="filterResourceType"></param>
        /// <returns></returns>
        IEnumerable<OrderDetails> GetAllOrders(OrderType? filterOrderType = null, ResourceType? filterResourceType = null);

        /// <summary>
        /// Get daily price history of the specified resource on the market for the last 14 days.
        /// </summary>
        /// <param name="resourceType">One of the RESOURCE_* constants. If undefined, returns history data for all resources.</param>
        /// <returns></returns>
        IEnumerable<PriceHistory> GetHistory(ResourceType? resourceType = null);

        /// <summary>
        /// Retrieve info for specific market order.
        /// </summary>
        /// <param name="id">The order ID.</param>
        /// <returns></returns>
        OrderDetails? GetOrderById(string id);
    }
}
