using UnityEngine;
using UnityEngine.Purchasing;

namespace Assets.IAPImplementation.Scripts
{
    public class IAPCore : MonoBehaviour, IStoreListener
    {
        public IStoreController StoreController { get; set; }
        public IExtensionProvider ExtensionProvider { get; set; }

        public virtual void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            throw new System.NotImplementedException();
        }

        public virtual void OnInitializeFailed(InitializationFailureReason error)
        {
            throw new System.NotImplementedException();
        }

        public virtual void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            throw new System.NotImplementedException();
        }

        public virtual PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            return PurchaseProcessingResult.Complete;
        }
    }
}