using UnityEngine;
using UnityEngine.Purchasing;

namespace IAPImplementation.Scripts
{
    public class IAPFakeStore : MonoBehaviour
    {
        private void Awake()
        {
            StandardPurchasingModule.Instance().useFakeStoreAlways = true;
            StandardPurchasingModule.Instance().useFakeStoreUIMode = FakeStoreUIMode.DeveloperUser;
        }
    }
}