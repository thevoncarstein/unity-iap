using IAPImplementation.Scripts;
using UnityEngine;

public class IAPSample : MonoBehaviour
{
    private void Start()
    {
        IAPManager.Instance.OnInitialized += OnIAPInitialized;
        IAPManager.Instance.Initialize();
    }

    private void OnIAPInitialized(bool success, string message)
    {
        Debug.Log($"[<color=#F00>UNITY IAP</color>] Initialized: {success}. Message: {message}");
    }
}
