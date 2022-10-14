using UnityEngine;
using UnityEngine.Events;

namespace Assets.IAPImplementation.Scripts
{
    public class IAPManager : MonoBehaviour
    {
        #region Singleton

        private static IAPManager _instance;

        public static IAPManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<IAPManager>() ??
                        (Instantiate(Resources.Load(nameof(IAPManager))) as GameObject)
                        .GetComponent<IAPManager>();
                }

                return _instance;
            }
        }

        #endregion

        public UnityAction<bool, string> OnInitialized { get; set; }
        public UnityAction<bool, string> OnPurchased { get; set; }
        public UnityAction<bool, string> OnRestored { get; set; }

        [SerializeField] private IAPInitialization _initialization;
        [SerializeField] private IAPPurchasing _purchase;
        [SerializeField] private IAPPurchaseRestoring _restoring;

        private void Awake()
        {
            ConfigSingleton();
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

            void ConfigSingleton()
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
        }

        public void PurchaseProduct(string productId) => 
            _purchase.BuyProduct(productId);

        public void RestoreProducts() =>
            _restoring.RestorePurchases();
    }
}