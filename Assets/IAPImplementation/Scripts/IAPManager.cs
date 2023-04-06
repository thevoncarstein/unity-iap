using System;
using System.Collections;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

namespace IAPImplementation.Scripts
{
    public class IAPManager : MonoBehaviour, IStoreListener
    {
        #region Fields and Properties

        public static IAPManager Instance { get; private set; }

        public UnityAction<bool, string> OnInitialized { get; set; }
        public UnityAction<bool, string> OnPurchased { get; set; }
        public UnityAction<bool, string> OnRestored { get; set; }

        private IStoreController _storeController;
        private IExtensionProvider _extensionProvider;
        private Product _currentProduct;
        private CrossPlatformValidator _validator;
        private string _environment = "production";
        private readonly float _dueTime = 30f;
        private IEnumerator _timingOut;
        private bool _isTimedOut;

        #endregion

        private void Awake()
        {
            SetupSingleton();
        }

        private void SetupSingleton()
        {
            bool isExist = Instance != null && Instance != this;
            if (isExist)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PurchaseProduct(string productId) => BuyProduct(productId);

        public void RestoreProducts() => RestorePurchases();

        public string GetProductPriceById(string productId) =>
            GetProductById(productId).metadata.localizedPriceString;

        public string GetProductValueById(string productId) =>
            GetProductById(productId).definition.payout.quantity.ToString();

        private Product GetProductById(string id) => 
            _storeController.products.WithID(id);

        public void ResetCallbacks() => OnPurchased = null;

        #region Local Members

        #region Initialization

        internal void Initialize() => InitializeServices();

        private async void InitializeServices()
        {
            try
            {
                var options = new InitializationOptions().SetEnvironmentName(_environment);
                await UnityServices.InitializeAsync(options);

                InitializePurchaseService();

                void InitializePurchaseService()
                {
                    var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
                    IAPConfigurationHelper
                        .PopulateConfigurationBuilder(
                        ref builder, ProductCatalog.LoadDefaultCatalog()
                        );
                    UnityPurchasing.Initialize(this, builder);
                }
            }
            catch (Exception exception)
            {
                OnInitialized?.Invoke(false, $"Initialized failed by {exception.Message}");
            }
        }

        void IStoreListener.OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
            _extensionProvider = extensions;

            OnInitialized?.Invoke(true, "Initialized Successfully.");
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            OnInitialized?.Invoke(false, $"Initialized failed by {error}");
        }

        #endregion

        #region Purchase

        private void BuyProduct(string productId)
        {
            StartTimingOut();

            _currentProduct = _storeController.products.WithID(productId);

            if (IsAbleToPurchase()) _storeController.InitiatePurchase(_currentProduct);
            else OnPurchased?.Invoke(
                false, 
                $"BuyProductID: FAIL. Not purchasing product, " + 
                $"either is not found or is not available for purchase"
                );
                
            bool IsAbleToPurchase() => _currentProduct != null && _currentProduct.availableToPurchase;
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            if (_isTimedOut) throw new Exception("Transaction failed. Timed out");

            StopTimingOut();

            Product product = purchaseEvent.purchasedProduct;

            try
            {
                if (IsPurchaseValid(in product))
                {
                    AppStore currentStore = StandardPurchasingModule.Instance().appStore;
                    bool isDeferred = 
                        _extensionProvider
                        .GetExtension<IGooglePlayStoreExtensions>()
                        .IsPurchasedProductDeferred(product);

                    if (currentStore == AppStore.GooglePlay && isDeferred)
                        return PurchaseProcessingResult.Pending;

                    string productId = @$"{purchaseEvent
                    .purchasedProduct
                    .definition
                    .id}";

                    OnPurchased?.Invoke(true, $"Purchased product: {productId}");
                }
            }
            catch
            {
                OnPurchaseFailed(product, PurchaseFailureReason.SignatureInvalid);
            }

            return PurchaseProcessingResult.Complete;
        }

        private bool IsPurchaseValid(in Product product)
        {
            if (_validator != null)
            {
                try
                {
                    _validator.Validate( product.receipt );
                }
                catch( IAPSecurityException reason )
                {
                    Debug.LogWarning( "Invalid IAP receipt: " + reason );
                    return false;
                }
            }

            return true;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            StopTimingOut();

            OnPurchased?.Invoke(
                false,
                $"OnPurchaseFailed: FAIL. " +
                $"Product: '{product.definition.storeSpecificId}', " +
                $"PurchaseFailureReason: {failureReason}"
                );
        }

        #endregion

        #region Purchase Restore
        
        private void RestorePurchases()
        {
            StartTimingOut();

            _extensionProvider
                .GetExtension<IAppleExtensions>()
                .RestoreTransactions(isRestored =>
                {
                    if (!isRestored) OnRestored?.Invoke(false, "Unable to request purchase restoration.");
                    else OnRestored.Invoke(true, "Restoring purchase requested successfully.");
                });
        }

        #endregion

        #region Time Out

        private void StartTimingOut()
        {
            _isTimedOut = false;
            _timingOut = TimingOut(_dueTime);
            StartCoroutine(_timingOut);
        }

        private void StopTimingOut()
        {
            if (_timingOut == null) return;

            StopCoroutine(_timingOut);
        }
        
        private IEnumerator TimingOut(float dueTime)
        {
            float currentTime = default;

            while (currentTime < dueTime)
            {
                currentTime += Time.deltaTime;
                yield return null;
            }

            _isTimedOut = true;
            OnPurchased?.Invoke(false, "Transaction failed. Timed out.");
        }

        #endregion

        #endregion
    }
}