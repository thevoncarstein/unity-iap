using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;

namespace Assets.IAPImplementation.Scripts
{
    public class IAPPurchasing: IAPCore
    {
        public UnityAction<bool, string> OnPurchased { get; set; }

        private IStoreController _storeController;
        private IExtensionProvider _extensionProvider;

        public void SetRequiredComponents
            (ref IStoreController controller, 
            ref IExtensionProvider provider)
        {
            _storeController = controller;
            _extensionProvider = provider;
        }

        public void BuyProduct(string productId)
        {
            Product product = GetProductById();            

            if (IsAbleToPurchase())
            {
                _storeController.InitiatePurchase(product);
                OnPurchased?.Invoke(
                    true, 
                    $"Purchasing product: {product.definition.id.ToString()}"
                    );
            }
            else OnPurchased?.Invoke(
                false, 
                "BuyProductID: FAIL. Not purchasing product, " + 
                "either is not found or is not available for purchase"
                );

            #region Local functions

            Product GetProductById() => _storeController.products.WithID(productId);

            bool IsAbleToPurchase() => product != null && product.availableToPurchase;

            #endregion
        }
    }
}