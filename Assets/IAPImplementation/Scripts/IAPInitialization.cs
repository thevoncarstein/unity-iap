using UnityEngine.Events;
using UnityEngine.Purchasing;

namespace Assets.IAPImplementation.Scripts
{
    public class IAPInitialization : IAPCore
    {
        public UnityAction<bool, string> OnInitializedCallback { get; set; }

        public void Initialize()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            IAPConfigurationHelper
                .PopulateConfigurationBuilder(
                ref builder, ProductCatalog.LoadDefaultCatalog()
                );
            UnityPurchasing.Initialize(this, builder);
        }

        public override void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            StoreController = controller;
            ExtensionProvider = extensions;

            OnInitializedCallback?.Invoke(true, "Initialized Successfully.");
        }

        public override void OnInitializeFailed(InitializationFailureReason error)
        {
            OnInitializedCallback?.Invoke(false, $"Initialized failed by {error}");
        }
    }
}