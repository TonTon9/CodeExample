using System;
using ReduxTools.Feature.Config;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.ReedemCode.Common
{
    public abstract class FeatureConfigContainer<T> : FeatureConfigContainer where T : class
    {
        public override Type ConfigType => typeof(T);

        public T Config => FetchRemote ? _remoteConfig ?? config : config;

        public override bool HasRemoteConfig() => _remoteConfig != null;

        [Title("Local Config")]
        [SerializeField] private T config;

        private T _remoteConfig;

        public override void SetRemoteConfig(object config)
        {
            if (!FetchRemote)
            {
                throw new InvalidOperationException("[configs] Remote config fetching is disabled");
            }

            _remoteConfig = (T)config;
        }

        public override object GetConfigAsObject()
        {
            return Config;
        }

        public override object GetLocalConfigAsObject()
        {
            return config;
        }

        private void OnDisable()
        {
            _remoteConfig = null;
        }
    }
}
