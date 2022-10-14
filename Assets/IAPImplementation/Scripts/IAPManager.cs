using UnityEngine;
using UnityEngine.Events;

namespace Assets.IAPImplementation.Scripts
{
    public class IAPManager : MonoBehaviour
    {
        public UnityAction<bool, string> OnInitialized;
        public UnityAction<bool, string> OnPurchased;
        public UnityAction<bool, string> OnRestored;

        [SerializeField] private IAPInitialization _initialization;
        [SerializeField] private IAPPurchasing _purchase;
        [SerializeField] private IAPPurchaseRestoring _restoring;

        private void Awake()
        {
            InitializeService();
            InitializeButtonBehaviour();
            InitializeRestoringBehaviour();

            void InitializeService()
            {
                _initialization.OnInitializedCallback += OnInitialized;
                _initialization.OnInitializedCallback += (status, message) => 
                    Debug.Log($"Initializing status: {status} | {message}");
                _initialization.Initialize();
            }

            void InitializeButtonBehaviour()
            {
                _purchase.OnPurchased += OnPurchased;
                _purchase.OnPurchased += (status, message) => 
                    Debug.Log($"Purchasing status: {status} | {message}");
                _purchase.SetRequiredComponents(_initialization.StoreController, _initialization.ExtensionProvider);
            }

            void InitializeRestoringBehaviour()
            {
                _restoring.OnRestored += OnRestored;
                _restoring.OnRestored += (status, message) =>
                    Debug.Log($"Restoring status: {status} | {message}");
                _restoring.SetRequiredComponents(_initialization.ExtensionProvider);
            }
        }
    }
}