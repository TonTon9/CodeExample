using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.ReedemCode
{
    [Serializable]
    public class RedeemCodeCoreConfig
    {
        [field: SerializeField] [JsonProperty] public bool Enabled { get; protected set; }
        [field: SerializeField] [JsonProperty] public PromoCodeData[] Codes;
    }
}
