using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal static class NativeMarketExtensions
    {
        public static TransactionDetails ToTransactionDetails(this JSObject obj)
            => new(
                    transactionId: obj.GetPropertyAsString("transactionId")!,
                    time: obj.GetPropertyAsInt32("time"),
                    senderUsername: obj.GetPropertyAsJSObject("sender")!.GetPropertyAsString("username")!,
                    recipientUsername: obj.GetPropertyAsJSObject("recipient")!.GetPropertyAsString("username")!,
                    resourceType: obj.GetPropertyAsString("resourceType")!.ParseResourceType(),
                    amount: obj.GetPropertyAsInt32("amount"),
                    from: new(obj.GetPropertyAsString("from")!),
                    to: new(obj.GetPropertyAsString("to")!),
                    description: obj.GetPropertyAsString("description")!,
                    order: obj.GetPropertyAsJSObject("order")?.ToTransactionDetailsOrderInfo()
                );

        public static TransactionDetails.OrderInfo ToTransactionDetailsOrderInfo(this JSObject obj)
            => new(
                    id: obj.GetPropertyAsString("id")!,
                    type: obj.GetPropertyAsString("sell")!.ParseOrderType(),
                    price: obj.GetPropertyAsDouble("price")!
                );

        public static OrderType ParseOrderType(this string str)
            => str switch
            {
                "buy" => OrderType.Buy,
                "sell" => OrderType.Sell,
                _ => throw new ArgumentException($"Unknown order type '{str}'", nameof(str))
            };

        public static string ToJS(this OrderType orderType)
            => orderType switch
            {
                OrderType.Buy => "buy",
                OrderType.Sell => "sell",
                _ => throw new ArgumentException($"Unknown order type '{orderType}'", nameof(orderType))
            };

        public static OrderDetails ToOrderDetails(this JSObject obj)
            => new(
                    id: obj.GetPropertyAsString("id")!,
                    created: obj.GetPropertyAsInt32("created"),
                    createdTimestamp: obj.HasProperty("createdTimestamp") ? DateTime.UnixEpoch + TimeSpan.FromMilliseconds(obj.GetPropertyAsDouble("createdTimestamp")) : null,
                    type: obj.GetPropertyAsString("type")!.ParseOrderType(),
                    resourceType: obj.GetPropertyAsString("resourceType")!.ParseResourceType(),
                    room: RoomCoord.ParseNullSafe(obj.GetPropertyAsString("roomName")),
                    amount: obj.GetPropertyAsInt32("amount"),
                    remainingAmount: obj.GetPropertyAsInt32("remainingAmount"),
                    price: obj.GetPropertyAsDouble("price")
                );

        public static MyOrderDetails ToMyOrderDetails(this JSObject obj)
            => new(
                    orderDetails: obj.ToOrderDetails(),
                    active: obj.GetPropertyAsBoolean("active"),
                    totalAmount: obj.GetPropertyAsInt32("totalAmount")
                );

        public static PriceHistory ToPriceHistory(this JSObject obj)
            => new(
                    resourceType: obj.GetPropertyAsString("resourceType")!.ParseResourceType(),
                    date: DateOnly.Parse(obj.GetPropertyAsString("date")!),
                    transactions: obj.GetPropertyAsInt32("transactions"),
                    volume: obj.GetPropertyAsInt32("volume"),
                    avgPrice: obj.GetPropertyAsDouble("avgPrice"),
                    stddevPrice: obj.GetPropertyAsDouble("stddevPrice")
                );
    }

    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeMarket : IMarket
    {
        #region Imports

        [JSImport("market.calcTransactionCost", "game")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_CalcTransactionCost([JSMarshalAs<JSType.Number>] int amount, [JSMarshalAs<JSType.String>] string roomName1, [JSMarshalAs<JSType.String>] string roomName2);

        [JSImport("market.cancelOrder", "game")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_CancelOrder([JSMarshalAs<JSType.String>] string orderId);

        [JSImport("market.changeOrderPrice", "game")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_ChangeOrderPrice([JSMarshalAs<JSType.String>] string orderId, [JSMarshalAs<JSType.Number>] double newPrice);

        [JSImport("market.createOrder", "game")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_CreateOrder([JSMarshalAs<JSType.Object>] JSObject @params);

        [JSImport("market.deal", "game")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Deal([JSMarshalAs<JSType.String>] string orderId, [JSMarshalAs<JSType.Number>] int amount, [JSMarshalAs<JSType.String>] string? yourRoomName);

        [JSImport("market.extendOrder", "game")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_ExtendOrder([JSMarshalAs<JSType.String>] string orderId, [JSMarshalAs<JSType.Number>] int addAmount);

        [JSImport("market.getAllOrders", "game")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.Object>>]
        internal static partial JSObject[] Native_GetAllOrders([JSMarshalAs<JSType.Object>] JSObject? filter);

        [JSImport("market.getHistory", "game")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.Object>>]
        internal static partial JSObject[] Native_GetHistory([JSMarshalAs<JSType.String>] string? resourceType);

        [JSImport("market.getOrderById", "game")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject? Native_GetOrderById([JSMarshalAs<JSType.String>] string id);

        #endregion

        private JSObject proxyObject;

        internal JSObject ProxyObject
        {
            get => proxyObject;
            set
            {
                proxyObject = value;
                ordersCache = null;
            }
        }

        private Dictionary<string, MyOrderDetails>? ordersCache;

        public int Credits => ProxyObject.GetPropertyAsInt32("credits");

        public IEnumerable<TransactionDetails> IncomingTransactions
        {
            get
            {
                var incomingTransactionsObj = JSUtils.GetObjectArrayOnObject(ProxyObject, "incomingTransactions");
                if (incomingTransactionsObj == null) { return Enumerable.Empty<TransactionDetails>(); }
                return incomingTransactionsObj.Select(x => x.ToTransactionDetails());
            }
        }

        public IEnumerable<TransactionDetails> OutgoingTransactions
        {
            get
            {
                var outgoingTransactionsObj = JSUtils.GetObjectArrayOnObject(ProxyObject, "outgoingTransactions");
                if (outgoingTransactionsObj == null) { return Enumerable.Empty<TransactionDetails>(); }
                return outgoingTransactionsObj.Select(x => x.ToTransactionDetails());
            }
        }

        public IReadOnlyDictionary<string, MyOrderDetails> Orders => ordersCache ??= GetOrders();

        public NativeMarket(JSObject proxyObject)
        {
            this.proxyObject = proxyObject;
        }

        public int CalcTransactionCost(int amount, RoomCoord room1, RoomCoord room2)
            => Native_CalcTransactionCost(amount, room1.ToString(), room2.ToString());

        public MarketCancelOrderResult CancelOrder(string orderId)
            => (MarketCancelOrderResult)Native_CancelOrder(orderId);

        public MarketChangeOrderPriceResult ChangeOrderPrice(string orderId, double newPrice)
            => (MarketChangeOrderPriceResult)Native_ChangeOrderPrice(orderId, newPrice);

        public MarketCreateOrderResult CreateOrder(OrderType type, ResourceType resourceType, double price, int totalAmount, RoomCoord? roomName)
        {
            using var paramsObj = JSUtils.CreateObject(null);
            paramsObj.SetProperty("type", type.ToJS());
            paramsObj.SetProperty("resourceType", resourceType.ToJS());
            paramsObj.SetProperty("price", price);
            paramsObj.SetProperty("totalAmount", totalAmount);
            if (roomName != null) { paramsObj.SetProperty("roomName", roomName.Value.ToString()); }
            return (MarketCreateOrderResult)Native_CreateOrder(paramsObj);
        }

        public MarketDealResult Deal(string orderId, int amount, RoomCoord? yourRoomName)
            => (MarketDealResult)Native_Deal(orderId, amount, yourRoomName?.ToString());

        public MarketExtendOrderResult ExtendOrder(string orderId, int addAmount)
            => (MarketExtendOrderResult)Native_ExtendOrder(orderId, addAmount);

        public IEnumerable<OrderDetails> GetAllOrders(OrderType? filterOrderType = null, ResourceType? filterResourceType = null)
        {
            using var filterObj = JSUtils.CreateObject(null);
            if (filterOrderType != null) { filterObj.SetProperty("type", filterOrderType.Value.ToJS()); }
            if (filterResourceType != null) { filterObj.SetProperty("resourceType", filterResourceType.Value.ToJS()); }
            return Native_GetAllOrders(filterObj).Select(x => x.ToOrderDetails());
        }

        public IEnumerable<PriceHistory> GetHistory(ResourceType? resourceType = null)
            => Native_GetHistory(resourceType?.ToJS()).Select(x => x.ToPriceHistory());

        public OrderDetails? GetOrderById(string id)
        {
            using var obj = Native_GetOrderById(id);
            return obj?.ToOrderDetails();
        }

        private Dictionary<string, MyOrderDetails> GetOrders()
        {
            using var ordersObj = ProxyObject.GetPropertyAsJSObject("orders");
            var keys = JSUtils.GetKeysOf(ordersObj!);
            var result = new Dictionary<string, MyOrderDetails>();
            foreach (var key in keys)
            {
                using var orderObj = ordersObj!.GetPropertyAsJSObject(key);
                if (orderObj == null) { continue; }
                result.Add(key, orderObj.ToMyOrderDetails());
            }
            return result;
        }
    }
}
