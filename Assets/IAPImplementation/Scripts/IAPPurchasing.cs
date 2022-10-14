using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;

namespace Assets.IAPImplementation.Scripts
{
    public class IAPPurchasing: IAPCore
    {
        public enum EPurchasingStatus
        {
            Complete,
            Pending,
            None
        }

        public UnityAction<bool, string> OnPurchased { get; set; }
        public EPurchasingStatus PurchasingStatus { get; set; }
        public Product CurrentProduct { get; set; }

        public void SetRequiredComponents
            (IStoreController controller, 
            IExtensionProvider provider)
        {
            StoreController = controller;
            ExtensionProvider = provider;

            OnPurchased += (status, message) => CurrentProduct = default;
        }

        public void BuyProduct(string productId)
        {
            CurrentProduct = GetProductById();

            if (IsAbleToPurchase())
            {
                StoreController.InitiatePurchase(CurrentProduct);
                OnPurchased?.Invoke(
                    true, 
                    $"Purchasing product: {CurrentProduct.definition.id.ToString()}"
                    );
            }
            else OnPurchased?.Invoke(
                false, 
                $"BuyProductID: FAIL. Not purchasing product, " + 
                $"either is not found or is not available for purchase"
                );

            #region Local functions

            Product GetProductById()
            {
                LocalizeProductId();

                return StoreController.products.WithID(productId);

                void LocalizeProductId()
                {
                    RuntimePlatform platform = Application.platform;
                    string applicationId = Application.identifier;
                    if (!productId.Contains(applicationId) &&
                        platform == RuntimePlatform.IPhonePlayer)
                        productId = $"{applicationId}.{productId}";
                }
            }

            bool IsAbleToPurchase() => CurrentProduct != null && CurrentProduct.availableToPurchase;

            #endregion
        }

        public override void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            OnPurchased?.Invoke(
                false,
                $"OnPurchaseFailed: FAIL. " +
                $"Product: '{product.definition.storeSpecificId}', " +
                $"PurchaseFailureReason: {failureReason}"
                );
        }

        public override PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            OnPurchased?.Invoke(
                true,
                $"Purchasing product: {purchaseEvent.purchasedProduct.definition.id.ToString()}"
                );
            return PurchaseProcessingResult.Complete;
        }
    }
}