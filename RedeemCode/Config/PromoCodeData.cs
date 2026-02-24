using System;

namespace Game.ReedemCode
{
    [Serializable]
    public class PromoCodeData
    {
        public string Code;
        public string StartTime;
        public string EndTime;
        public PromoRewardData[] Rewards;
    }
}
