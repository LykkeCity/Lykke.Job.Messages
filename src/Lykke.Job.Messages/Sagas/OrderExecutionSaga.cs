using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
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
using Lykke.Service.EmailSender;
using Lykke.Service.PushNotifications.Contract;
using Lykke.Service.TemplateFormatter.Client;

namespace Lykke.Job.Messages.Sagas
{
    public class OrderExecutionSaga
    {
        private readonly IAssetsServiceWithCache _assetsService;
        private readonly IClientAccountClient _clientAccountClient;
        [NotNull] private readonly ITemplateFormatter _templateFormatter;
        readonly Dictionary<Guid, bool> _walletsByType = new Dictionary<Guid, bool>();
        private readonly ILog _log;

        public OrderExecutionSaga(
            [NotNull] IAssetsServiceWithCache assetsService, 
            [NotNull] IClientAccountClient clientAccountClient,
            [NotNull] ITemplateFormatter templateFormatter,
            ILogFactory logFactory)
        {
            _assetsService = assetsService ?? throw new ArgumentNullException(nameof(assetsService));
            _clientAccountClient = clientAccountClient ?? throw new ArgumentNullException(nameof(clientAccountClient));
            _templateFormatter = templateFormatter ?? throw new ArgumentNullException(nameof(templateFormatter));
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

            string clientId = order.WalletId.ToString();
            
            var wallet = await _clientAccountClient.GetWalletAsync(clientId);

            if (wallet != null)
                clientId = wallet.ClientId;
            
            var clientAccount = await _clientAccountClient.GetByIdAsync(clientId);

            if (clientAccount == null)
            {
                _log.Warning(nameof(ManualOrderTradeProcessedEvent), $"Client not found (clientId = {clientId})");
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
            
            if (!pushSettings.Enabled || string.IsNullOrEmpty(clientAccount.NotificationsId))
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

            string status;
            EmailMessage template;
            
            switch (order.Status)
            {
                case OrderStatus.PartiallyMatched:
                    template = await _templateFormatter.FormatAsync("PushLimitOrderPartiallyExecutedTemplate", clientAccount.PartnerId, "EN", 
                        new
                        {
                            OrderSide = orderSide,
                            AssetPairId = order.AssetPairId,
                            Volume = order.Volume,
                            Price = order.Price,
                            AssetDisplayId = priceAsset.DisplayId,
                            ExecutedSum = executedSum,
                            ReservedAssetDisplayId = receivedAssetEntity.DisplayId
                        });
                    status = "Processing";
                    break;
                case OrderStatus.Cancelled:
                    template = await _templateFormatter.FormatAsync("PushLimitOrderExecutedTemplate", clientAccount.PartnerId, "EN", 
                        new
                        {
                            OrderSide = orderSide,
                            AssetPairId = order.AssetPairId,
                            Volume = order.Volume,
                            Price = order.Price,
                            AssetDisplayId = priceAsset.DisplayId,
                            ExecutedSum = executedSum,
                            ReservedAssetDisplayId = receivedAssetEntity.DisplayId
                        });
                    status = "Cancelled";
                    break;
                case OrderStatus.Matched:
                    template = await _templateFormatter.FormatAsync("PushLimitOrderExecutedTemplate", clientAccount.PartnerId, "EN", 
                        new
                        {
                            OrderSide = orderSide,
                            AssetPairId = order.AssetPairId,
                            Volume = order.Volume,
                            Price = order.Price,
                            AssetDisplayId = priceAsset.DisplayId,
                            ExecutedSum = executedSum,
                            ReservedAssetDisplayId = receivedAssetEntity.DisplayId
                        });
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

            if (template != null)
            {
                commandSender.SendCommand(new LimitOrderNotificationCommand
                {
                    NotificationIds = new[] {clientAccount.NotificationsId},
                    Message = template.Subject,
                    OrderStatus = status,
                    OrderType = order.Side.ToString()
                }, PushNotificationsBoundedContext.Name);
            }
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
