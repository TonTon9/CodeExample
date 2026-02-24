using System.Collections.Generic;
using VContainer.Unity;

namespace Game.ReedemCode.Core
{
    public class RedeemCodeSaver : IInitializable
    {
        private const string USED_REDEEM_CODES_KEY = "used_promo_codes";

        public void Initialize()
        {
            LoadUsedCodes();
        }

        public void MarkAsUsed(string code)
        {
            var used = LoadUsedCodes();
            used.Add(NormalizeCode(code));
            SaveUsedCodes(used);
        }

        public bool IsUsed(string code)
        {
            var used = LoadUsedCodes();
            return used.Contains(NormalizeCode(code));
        }

        private HashSet<string> LoadUsedCodes()
        {
            var raw = CloudPlayerPrefs.GetString(USED_REDEEM_CODES_KEY, string.Empty);
            if (string.IsNullOrEmpty(raw))
                return new HashSet<string>();

            return new HashSet<string>(raw.Split('|'));
        }

        private void SaveUsedCodes(HashSet<string> codes)
        {
            var raw = string.Join("|", codes);
            CloudPlayerPrefs.SetString(USED_REDEEM_CODES_KEY, raw);
            CloudPlayerPrefs.Save();
        }

        private static string NormalizeCode(string code)
        {
            return code?.Trim().ToUpperInvariant();
        }
    }
}
