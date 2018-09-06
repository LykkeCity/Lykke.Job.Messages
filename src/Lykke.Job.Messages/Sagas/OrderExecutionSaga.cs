using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Job.Messages.Resources;
using Lykke.Service.Assets.Client;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Models;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Models.Enums;
using Lykke.Service.PushNotifications.Contract.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Job.Messages.Sagas
{
    public class OrderExecutionSaga
    {
        private readonly IAssetsServiceWithCache _assetsService;
        private readonly IClientAccountClient _clientAccountClient;
        readonly Dictionary<Guid, bool> _walletsByType = new Dictionary<Guid, bool>();
        private readonly ILog _log;

        public OrderExecutionSaga([NotNull] IAssetsServiceWithCache assetsService, [NotNull] IClientAccountClient clientAccountClient, ILogFactory logFactory)
        {
            _assetsService = assetsService ?? throw new ArgumentNullException(nameof(assetsService));
            _clientAccountClient = clientAccountClient ?? throw new ArgumentNullException(nameof(clientAccountClient));
            _log = logFactory.CreateLog(this);
        }

        [UsedImplicitly]
        public async Task Handle(ManualOrderTradeProcessedEvent evt, ICommandSender commandSender)
        {
            var order = evt.Order;
            var walletId = order.WalletId;

            if (order.Type != OrderType.Limit && order.Type != OrderType.StopLimit)
                return;

            if (order.Trades == null || !order.Trades.Any())
            {
                _log.Warning(nameof(ManualOrderTradeProcessedEvent), "The order has no trades.");
                return;
            }

            #region Checking if this wallet is used for manual trading
            // todo: This filtering logic should be transfered into PostProcessing 
            if (!_walletsByType.ContainsKey(walletId))
                _walletsByType[walletId] = (await _clientAccountClient.IsTrustedAsync(walletId.ToString())).Value;

            var isRobot = _walletsByType[walletId];
            if (isRobot)
                return;
            #endregion

            var pushSettings = await _clientAccountClient.GetPushNotificationAsync(walletId.ToString());
            if (!pushSettings.Enabled)
                return;

            var aggregatedSwaps = AggregateSwaps(order.Trades);

            var assetPair = await _assetsService.TryGetAssetPairAsync(order.AssetPairId);

            var receivedAsset = order.Side == OrderSide.Buy ? assetPair.BaseAssetId : assetPair.QuotingAssetId;
            var receivedAssetEntity = await _assetsService.TryGetAssetAsync(receivedAsset);

            var priceAsset = await _assetsService.TryGetAssetAsync(assetPair.QuotingAssetId);

            var executedSum = Math.Abs(aggregatedSwaps.Where(x => x.WalletId == walletId && x.AssetId == receivedAsset)
                .Select(x => x.Amount)
                .DefaultIfEmpty(0)
                .Sum()).TruncateDecimalPlaces(receivedAssetEntity.Accuracy);

            var orderSide = order.Side.ToString().ToLower();

            string message;
            string status;
            switch (order.Status)
            {
                case OrderStatus.PartiallyMatched:
                    message = string.Format(PushResources.LimitOrderPartiallyExecuted, orderSide, order.AssetPairId, order.Volume, order.Price, priceAsset.DisplayId, executedSum, receivedAssetEntity.DisplayId);
                    status = "Processing";
                    break;
                case OrderStatus.Cancelled:
                    message = string.Format(PushResources.LimitOrderExecuted, orderSide, order.AssetPairId, order.Volume, order.Price, priceAsset.DisplayId, executedSum, receivedAssetEntity.DisplayId);
                    status = "Cancelled";
                    break;
                case OrderStatus.Matched:
                    message = string.Format(PushResources.LimitOrderExecuted, orderSide, order.AssetPairId, order.Volume, order.Price, priceAsset.DisplayId, executedSum, receivedAssetEntity.DisplayId);
                    status = "Matched";
                    break;
                case OrderStatus.Replaced:
                case OrderStatus.Pending:
                case OrderStatus.Placed:
                case OrderStatus.Rejected:
                    _log.Warning(nameof(ManualOrderTradeProcessedEvent), $"The order has unexpected status {order.Status}.");
                    return;
                default:
                    // ReSharper disable once NotResolvedInText
                    throw new ArgumentOutOfRangeException("evt.Orders.Status", order.Status, nameof(OrderStatus));
            }
            var notificationIds = new[] { (await _clientAccountClient.GetByIdAsync(order.WalletId.ToString())).NotificationsId };
            var command = new LimitOrderNotificationCommand
            {
                NotificationIds = notificationIds,
                Message = message,
                OrderStatus = status,
                OrderType = order.Side.ToString()
            };
            commandSender.SendCommand(command, "push-notifications");
        }

        private List<AggregatedTransfer> AggregateSwaps(IEnumerable<TradeModel> trades)
        {
            var list = new List<AggregatedTransfer>();

            if (trades != null)
            {
                foreach (var swap in trades)
                {
                    var amount1 = Convert.ToDecimal(swap.BaseVolume);
                    var amount2 = Convert.ToDecimal(swap.QuotingVolume);

                    AddAmount(list, swap.WalletId, swap.BaseAssetId, -amount1);
                    AddAmount(list, swap.OppositeWalletId, swap.BaseAssetId, amount1);

                    AddAmount(list, swap.OppositeWalletId, swap.QuotingAssetId, -amount2);
                    AddAmount(list, swap.WalletId, swap.QuotingAssetId, amount2);
                }
            }

            return list;
        }

        private void AddAmount(ICollection<AggregatedTransfer> list, Guid walletId, string asset, decimal amount)
        {
            var client1 = list.FirstOrDefault(x => x.WalletId == walletId && x.AssetId == asset);
            if (client1 != null)
                client1.Amount += amount;
            else
                list.Add(new AggregatedTransfer
                {
                    Amount = amount,
                    WalletId = walletId,
                    AssetId = asset
                });
        }
    }

    class AggregatedTransfer
    {
        public Guid WalletId { get; set; }

        public string AssetId { get; set; }

        public decimal Amount { get; set; }
    }
}
