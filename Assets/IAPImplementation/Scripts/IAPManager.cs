using System;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

namespace Assets.IAPImplementation.Scripts
{
    public class IAPManager : MonoBehaviour, IStoreListener
    {
        #region Fields and Properties

        #region Singleton

        private static IAPManager _instance;

        public static IAPManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<IAPManager>() ??
                        new GameObject(nameof(IAPManager)).AddComponent<IAPManager>();
                }

                return _instance;
            }
        }

        #endregion

        public UnityAction<bool, string> OnInitialized { get; set; }
        public UnityAction<bool, string> OnPurchased { get; set; }
        public UnityAction<bool, string> OnRestored { get; set; }

        private IStoreController _storeController;
        private IExtensionProvider _extensionProvider;
        private Product _currentProduct;
        private CrossPlatformValidator _validator;
        public string _environment = "production";

        #endregion

        private void Awake()
        {
            #region Setup Singleton

            if (IsExist())
            {
                Destroy(this.gameObject);
                return;
            }
            else 
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }

            bool IsExist() => _instance != null && _instance != this;

            #endregion
        }

        private void Start() => Initialize();

        public void PurchaseProduct(string productId) => BuyProduct(productId);

        public void RestoreProducts() => RestorePurchases();

        public string GetProductPriceById(string productId) =>
            GetProductById(productId).metadata.localizedPriceString;

        public string GetProductValueById(string productId) =>
            GetProductById(productId).definition.payout.quantity.ToString();

        private Product GetProductById(string id) => 
            _storeController.products.WithID(id);

        #region Local Members

        #region Initialization

        private void Initialize() => InitializeServices();

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
            _currentProduct = GetProductById();

            if (IsAbleToPurchase()) _storeController.InitiatePurchase(_currentProduct);
            else OnPurchased?.Invoke(
                false, 
                $"BuyProductID: FAIL. Not purchasing product, " + 
                $"either is not found or is not available for purchase"
                );

            #region Local functions

            Product GetProductById()
            {
                LocalizeProductId();

                return _storeController.products.WithID(productId);

                void LocalizeProductId()
                {
                    RuntimePlatform platform = Application.platform;
                    string applicationId = Application.identifier;
                    if (!productId.Contains(applicationId) &&
                        platform == RuntimePlatform.IPhonePlayer)
                        productId = $"{applicationId}.{productId}";
                }
            }

            bool IsAbleToPurchase() => _currentProduct != null && _currentProduct.availableToPurchase;

            #endregion
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
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
                    .id.ToString()}";

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
            _extensionProvider
                .GetExtension<IAppleExtensions>()
                .RestoreTransactions(isRestored =>
                {
                    if (isRestored) OnRestored?.Invoke(true, "Restore purchases succeeded.");
                    else OnRestored?.Invoke(false, "Restore purchases failed.");
                });
        }

        #endregion

        #endregion
    }
}