using System.Linq;
using DG.Tweening;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.ReedemCode.Core
{
    public class RedeemCodePopupView : PopupBase
    {
        private RedeemCodePopupData _popupData;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _enterCodeButton;
        [SerializeField] private TMP_InputField _codeInputField;
        [SerializeField] private LocalizedText _errorText;

        public class RedeemCodePopupData : PopupData
        {
            public RedeemCodeFeatureConfigContainer Config;
            public Subject<EnterRedeemCodeData> OnEnterCodeClick;
            public Subject<Unit> OnCloseClick;
        }

        private void Awake()
        {
            _closeButton.onClick.AddListener(CloseButton_OnClick);
            _enterCodeButton.onClick.AddListener(EnterCodeButton);
        }

        public override void Initialize(PopupData data)
        {
            base.Initialize(data);
            _popupData = (RedeemCodePopupData)data;
        }

        public override void OnShow()
        {
            base.OnShow();
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutSine).SetLink(gameObject);
            ServiceLocator<SoundService>.Get()?.Play(SoundType.pop_up);
        }

        public override void OnHide()
        {
            canvasGroup.DOFade(0f, 0.15f)
                .SetEase(Ease.OutSine)
                .SetLink(gameObject)
                .OnComplete(() => base.OnHide());
        }

        public override void Close()
        {
            OnClose?.Invoke(this);
        }

        private void CloseButton_OnClick()
        {
            _popupData.OnCloseClick?.OnNext(Unit.Default);
            Close();
        }

        public void ChangeViewOnCodeEntered(PromoCodeValidationResult result)
        {
            var data = _popupData.Config.LocalizationsByResults.FirstOrDefault(d => d.Result == result);
            if (data != null)
            {
                _errorText.SetValue(data.LocalizationKeys);
                _errorText.gameObject.SetActive(true);
            }
        }

        private void EnterCodeButton()
        {
            _popupData.OnEnterCodeClick?.OnNext(new EnterRedeemCodeData()
            {
                InputCode = _codeInputField.text
            });
        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveListener(CloseButton_OnClick);
            _enterCodeButton.onClick.RemoveListener(EnterCodeButton);
        }
    }
}
