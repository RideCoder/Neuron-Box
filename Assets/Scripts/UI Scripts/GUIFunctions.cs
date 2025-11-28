using UnityEngine;
using System;

public class GUIFunctions : MonoBehaviour
{
    public static event Action OnShowModulesClicked;
    public static event Action OnShowNetworkClicked;
   

    public void ShowModules()
    {
        OnShowModulesClicked?.Invoke();
    }

    public void ShowNetwork()
    {
        OnShowNetworkClicked?.Invoke();
    }

 


}
