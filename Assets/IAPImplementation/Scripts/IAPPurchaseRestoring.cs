using UnityEngine.Events;
using UnityEngine.Purchasing;

namespace Assets.IAPImplementation.Scripts
{
    public class IAPPurchaseRestoring : IAPCore
    {
        public UnityAction<bool, string> OnRestored { get; set; }

        public void SetRequiredComponents(IExtensionProvider provider)
        {
            ExtensionProvider = provider;
        }

        public void RestorePurchases()
        {
            ExtensionProvider
                .GetExtension<IAppleExtensions>()
                .RestoreTransactions(isRestored =>
                {
                    if (isRestored) OnRestored?.Invoke(true, "Restore purchases succeeded.");
                    else OnRestored?.Invoke(false, "Restore purchases failed.");
                });
        }
    }
}