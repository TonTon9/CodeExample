using System;
using R3;
using VContainer.Unity;

namespace Game.ReedemCode.Core
{
    public class RedeemCodePresenter : IInitializable, IDisposable
    {
        private const float DELAY_BTW_CLICKS = 0.1f;

        private readonly ReactiveData.ReactiveData.HudLoadChannel _hudLoadChannel;
        private readonly RedeemCodeService _service;
        private readonly RedeemCodeFeatureConfigContainer _config;
        private IDisposable _unblockButtonSubscription;
        private DisposableBag _disposable = new();
        private RedeemCodePopupView _redeemCodePopupView;

        private HudScreen _hudScreen;
        private bool _lockButton = false;

        private readonly Subject<EnterRedeemCodeData> _onEnterCodeClick = new();
        private readonly Subject<Unit> _onCloseClick = new();

        public RedeemCodePresenter(RedeemCodeService service,
            RedeemCodeFeatureConfigContainer config, ReactiveData.ReactiveData.HudLoadChannel hudLoadChannel)
        {
            _service = service;
            _config = config;
            _hudLoadChannel = hudLoadChannel;
            ServiceLocator<RedeemCodePresenter>.Register(this);
        }

        public void Initialize()
        {
            _hudLoadChannel.OnLoadHudScreen.SubscribeWithSkip(x => OnHudLoaded(x)).AddTo(ref _disposable);
            _service.OnCodeEntered.Subscribe(OnCodeEntered).AddTo(ref _disposable);
            ;
            _onEnterCodeClick.Subscribe(EnterCodeClick).AddTo(ref _disposable);
            ;
            _onCloseClick.Subscribe(_ => OnCLose()).AddTo(ref _disposable);
            ;
        }

        public void ShowPopup()
        {
            if (_lockButton) return;
            _lockButton = true;
            _unblockButtonSubscription?.Dispose();
            _unblockButtonSubscription = Observable.Timer(TimeSpan.FromSeconds(DELAY_BTW_CLICKS)).Subscribe(_ => UnblockButton());
            var popupManager = UIController.Instance.PopupManager;

            popupManager.HideCurrentAndShow(PopupType.RedeemCodePopup, InitializePopup);
        }

        private void OnCodeEntered(PromoCodeValidationResult result)
        {
            if (_redeemCodePopupView != null)
                _redeemCodePopupView.ChangeViewOnCodeEntered(result);
        }

        private void EnterCodeClick(EnterRedeemCodeData data)
        {
            _service.EnterCode(data);
        }

        private void OnHudLoaded(IHud hud)
        {
            _hudScreen = (HudScreen)hud;
        }

        private void UnblockButton()
        {
            _lockButton = false;
        }

        private void InitializePopup(PopupBase popup)
        {
            if (popup is RedeemCodePopupView redeemCodePopup)
            {
                _redeemCodePopupView = redeemCodePopup;
                _redeemCodePopupView.Initialize(new RedeemCodePopupView.RedeemCodePopupData
                {
                    Config = _config,
                    OnEnterCodeClick = _onEnterCodeClick,
                    OnCloseClick = _onCloseClick
                });
            }
        }

        private void OnCLose()
        {
            var session = ServiceLocator<GameSession>.Get();
            if (_hudScreen != null && session != null)
            {
                session.GameEvents.TryShowEvent(GameEventType.SettingsPopup, this, () =>
                {
                    _hudScreen.ShowSettingsMenu();
                }, new GameEventTrackerData());
            }
        }

        public void Dispose()
        {
            _disposable.Dispose();
            _unblockButtonSubscription?.Dispose();
        }
    }
}
