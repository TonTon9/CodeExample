using System;
using System.Globalization;
using System.Linq;
using Game.ReedemCode.Common;
using R3;
using ReduxTools.Utils;

namespace Game.ReedemCode.Core
{
    public class RedeemCodeService
    {
        private readonly RedeemCodeFeatureConfigContainer _config;
        private readonly RedeemCodeSaver _saver;
        private readonly ServerTimeService _timeService;
        private readonly RedeemCodeRewardPresenter _rewardPresenter;

        public readonly Subject<PromoCodeValidationResult> OnCodeEntered = new();

        public RedeemCodeService(RedeemCodeFeatureConfigContainer config, RedeemCodeSaver saver, ServerTimeService timeService,
            RedeemCodeRewardPresenter rewardPresenter)
        {
            _config = config;
            _saver = saver;
            _timeService = timeService;
            _rewardPresenter = rewardPresenter;
        }

        public void EnterCode(EnterRedeemCodeData data)
        {
            var result = TryRedeem(data.InputCode);
            if (result == PromoCodeValidationResult.Success)
            {
                _rewardPresenter.ClaimRewards(data);
            }

            OnCodeEntered.OnNext(result);
        }

        private PromoCodeValidationResult TryRedeem(string inputCode)
        {
            if (!_timeService.IsOnline.CurrentValue)
                return PromoCodeValidationResult.NoInternet;

            if (string.IsNullOrWhiteSpace(inputCode))
                return PromoCodeValidationResult.NotFound;

            var normalizedCode = NormalizeCode(inputCode);

            var promoCodeData = _config.Config.Codes
                .FirstOrDefault(c => NormalizeCode(c.Code) == normalizedCode);

            if (promoCodeData == null)
                return PromoCodeValidationResult.NotFound;

            if (_saver.IsUsed(normalizedCode))
                return PromoCodeValidationResult.AlreadyUsed;

            if (IsExpired(promoCodeData))
                return PromoCodeValidationResult.Expired;

            _saver.MarkAsUsed(normalizedCode);
            return PromoCodeValidationResult.Success;
        }

        private bool IsExpired(PromoCodeData data)
        {
            if (data == null)
            {
                AnalyticsErrorSender.SendAnalyticsError("RedeemCodeService: IsExpired called without data");
                return true;
            }


            var now = _timeService.CurrentTime;

            if (!TryParsePromoTime(data.StartTime, out var startTime))
            {
                AnalyticsErrorSender.SendAnalyticsError("Cant parse redeem code StartTime");
                return true;
            }

            if (!TryParsePromoTime(data.EndTime, out var endTime))
            {
                AnalyticsErrorSender.SendAnalyticsError("Cant parse redeem code EndTime");
                return true;
            }

            return now < startTime || now > endTime;
        }

        private bool TryParsePromoTime(string value, out DateTime result)
        {
            return DateTime.TryParseExact(
                value,
                CommonTimeFormats.UNIVERSAL_FORMATS,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal,
                out result
            );
        }

        private static string NormalizeCode(string code)
        {
            return code?.Trim().ToUpperInvariant();
        }
    }
}
