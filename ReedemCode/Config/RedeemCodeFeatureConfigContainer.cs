using Newtonsoft.Json;
using ReduxTools.Feature.Config;
using UnityEngine;
using VContainer;

namespace Game.ReedemCode
{
    [CreateAssetMenu(fileName = "ReedemCodeFeatureConfigContainer", menuName = "Content/ReedemCode/FeatureConfigContainer")]
    public class RedeemCodeFeatureConfigContainer : FeatureConfigContainer<RedeemCodeCoreConfig>
    {
        public override bool FeatureEnabled => Config.Enabled;
        public override string FeatureConfigName => "ReedemCodeConfig";

        [field: SerializeField] [JsonProperty] public RedeemCodeLocalizationByResult[] LocalizationsByResults;

        public override void RegisterToContainer(IContainerBuilder container)
        {
            container.RegisterInstance(this).AsImplementedInterfaces();
        }
    }
}
