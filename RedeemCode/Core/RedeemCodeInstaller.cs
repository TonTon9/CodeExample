using ReduxTools.Feature;
using VContainer;
using VContainer.Unity;

namespace Game.ReedemCode.Core
{
    public class RedeemCodeInstaller : IFeatureInstaller
    {
        public void InstallFeature(IContainerBuilder builder)
        {
            builder.UseEntryPoints(entryPoints =>
            {
                entryPoints.Add<RedeemCodeService>().AsSelf();
                entryPoints.Add<RedeemCodePresenter>().AsSelf();
                entryPoints.Add<RedeemCodeRewardPresenter>().AsSelf();
                entryPoints.Add<RedeemCodeSaver>().AsSelf();
            });
        }

        public void InstallShared(IContainerBuilder builder)
        {
        }
    }
}
