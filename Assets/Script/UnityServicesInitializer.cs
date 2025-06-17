using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class UnityServicesInitializer : MonoBehaviour
{
    async void Awake()
    {
        //Unity Services ‚ğ‰Šú‰»‚µA“½–¼ƒƒOƒCƒ“‚·‚é
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log("Signed in anonymously.");
    }
}
