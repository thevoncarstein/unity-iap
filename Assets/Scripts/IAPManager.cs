using System;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Assets.IAPImplementation.Scripts
{
    public class IAPManager : MonoBehaviour, IStoreListener
    {
        public bool IsInitialized => m_StoreController != null && m_StoreExtensionProvider != null;
        private static IStoreController m_StoreController;
        private static IExtensionProvider m_StoreExtensionProvider;
        private static Product test_product = null;
        private Boolean return_complete = true;

        void Start()
        {
            if (m_StoreController == null) InitializePurchasing();
        }

        public void InitializePurchasing()
        {
            if (IsInitialized) return;

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            // builder.AddProduct(GOLD_50, ProductType.Consumable);
            // builder.AddProduct(NO_ADS, ProductType.NonConsumable);
            // builder.AddProduct(SUB1, ProductType.Subscription);

            UnityPurchasing.Initialize(this, builder);
        }

        // private bool IsInitialized() => m_StoreController != null && m_StoreExtensionProvider != null;

        public void CompletePurchase()
        {
            if (test_product == null) Debug.Log("Cannot complete purchase, product not initialized.");
            else m_StoreController.ConfirmPendingPurchase(test_product);
        }

        public void ToggleComplete() => return_complete = !return_complete;

        public void RestorePurchases()
        {
            m_StoreExtensionProvider
            .GetExtension<IAppleExtensions>()
            .RestoreTransactions(result => 
            {
                if (result) Debug.Log("Restore purchases succeeded.");
                else Debug.Log("Restore purchases failed.");
            });
        }

        void BuyProductID(string productId)
        {
            if (IsInitialized)
            {
                Product product = m_StoreController.products.WithID(productId);

                if (product != null && product.availableToPurchase)
                {
                    Debug.Log(string.Format("Purchasing product:" + product.definition.id.ToString()));
                    m_StoreController.InitiatePurchase(product);
                }
                else
                {
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            else
            {
                Debug.Log("BuyProductID FAIL. Not initialized.");
            }
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("OnInitialized: PASS");

            m_StoreController = controller;
            m_StoreExtensionProvider = extensions;
        }


        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
        }


        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            test_product = args.purchasedProduct;

            if (return_complete)
            {
                Debug.Log(string.Format("ProcessPurchase: Complete. Product:" + args.purchasedProduct.definition.id + " - " + test_product.transactionID.ToString()));
                return PurchaseProcessingResult.Complete;
            }
            else
            {
                Debug.Log(string.Format("ProcessPurchase: Pending. Product:" + args.purchasedProduct.definition.id + " - " + test_product.transactionID.ToString()));
                return PurchaseProcessingResult.Pending;
            }

        }


        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        }
    }
}