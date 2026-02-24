using System.Collections.Generic;
using System.Linq;
using ReduxTools.CustomLogger;
using ReduxTools.Utils;

namespace Game.ReedemCode.Core
{
    public class RedeemCodeRewardPresenter
    {
        private const string REDEEM_CODE_ANALYSIS_KEY = "promo_code";

        private readonly RedeemCodeFeatureConfigContainer _config;
        private readonly RewardBaseFactory _rewardBaseFactory;
        private readonly GameData _gameData;
        private readonly Wallet _wallet;

        public RedeemCodeRewardPresenter(Wallet wallet, GameData gameData, RedeemCodeFeatureConfigContainer config)
        {
            _gameData = gameData;
            _rewardBaseFactory = RewardBaseFactory.Instance;
            _wallet = wallet;
            _config = config;
        }

        public void ClaimRewards(EnterRedeemCodeData data)
        {
            var rewardList = GetRewardBaseList(data);
            foreach (var reward in rewardList)
            {
                var walletParams = new Wallet.WalletParams
                {
                    EventList = new List<IEvent>(),
                    Source = REDEEM_CODE_ANALYSIS_KEY,
                    Params = new Dictionary<string, string> { [Wallet.CONTEXT] = REDEEM_CODE_ANALYSIS_KEY }
                };

                _wallet.AccrueWithConvert(reward.GetBaseRewardID(), reward.Count, walletParams);
            }

            CustomLogger.Log("RedeemCodeRewardPresenter: rewards claimed");

            ShowRewardClaimPopup(rewardList);
            SendAnalytic(data.InputCode);
        }

        private void ShowRewardClaimPopup(List<RewardBase> rewardBases)
        {
            var rewards = new Dictionary<string, RewardBase>();
            foreach (var reward in rewardBases)
            {
                rewards.Add(reward.GetBaseRewardID(), reward);
            }

            if (rewards.Count == 0)
            {
                CustomLogger.Log("RedeemCodeRewardPresenter: rewards not found ");
                return;
            }

            UIController.Instance.PopupManager.HideCurrentAndShow(PopupType.RewardClaimPopup, popup =>
            {
                popup.Initialize(new RewardClaimPopup.RewardClaimPopupData
                {
                    rewardList = rewards,
                    titleTextKey = LocalizationKeys.ui_title_reward_window_cascade
                });
            });

            CustomLogger.Log("RedeemCodeRewardPresenter: rewards claim popup show called");
        }

        private void SendAnalytic(string promoCode)
        {
            string extra = ServiceLocator<Analytics>.Get().GetContext();
            SayKit.trackEvent(REDEEM_CODE_ANALYSIS_KEY, promoCode, extra);
        }

        private List<RewardBase> GetRewardBaseList(EnterRedeemCodeData data)
        {
            var normalizedInput = NormalizeCode(data.InputCode);

            var promoCodeData = _config.Config.Codes
                .FirstOrDefault(c => NormalizeCode(c.Code) == normalizedInput);

            if (promoCodeData == null)
            {
                AnalyticsErrorSender.SendAnalyticsError("RedeemCodeRewardPresenter: Promo code not found");
                return null;
            }

            List<RewardBase> rewards = new List<RewardBase>();
            foreach (var reward in promoCodeData.Rewards)
            {
                var rewardBase = GetCurrencyReward(reward);
                if (rewardBase == null)
                {
                    AnalyticsErrorSender.SendAnalyticsError("RedeemCodeRewardPresenter: Reward base not found");
                }
                else
                {
                    rewards.Add(rewardBase);
                }
            }

            return rewards;
        }

        private RewardBase GetCurrencyReward(PromoRewardData data)
        {
            var hotelId = _gameData.currentHotel.Id;
            RewardBase rewardBase;
            if (_gameData.CurrencyData.IsSoftCurrency(data.ID))
            {
                var currency = _gameData.HotelsData.GetCurrencyByHotel(hotelId);
                rewardBase = _rewardBaseFactory.GetRewardBaseByID(currency.ToString());
                if (rewardBase != null)
                {
                    var softAmount = CalculateRewardAmount(currency, data.Count);
                    rewardBase.Count = softAmount;
                }
            }
            else
            {
                rewardBase = _rewardBaseFactory.GetRewardBaseByID(data.ID);
                if (rewardBase != null)
                    rewardBase.Count = data.Count;
            }

            return rewardBase;
        }

        private float CalculateRewardAmount(Currency currency, float value)
        {
            if (_gameData.CurrencyData.IsSoftCurrency(currency))
            {
                var session = ServiceLocator<GameSession>.Get();
                if (session == null || session.Hotel == null)
                {
                    AnalyticsErrorSender.SendAnalyticsError("RedeemCodeRewardPresenter: cant find game session or hotel");
                    return 0;
                }

                var maxPrice = MaxPriceSearcher.CalculateMaxPrice(session.Hotel, currency);
                float rewardAmount = maxPrice * value;
                return (int)rewardAmount;
            }

            return value;
        }

        private static string NormalizeCode(string code)
        {
            return code?.Trim().ToUpperInvariant();
        }
    }
}
