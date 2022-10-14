using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Assets.IAPImplementation.Scripts
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