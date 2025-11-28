using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIFunctions : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // 
    [SerializeField] private Transform contentArea;
    [SerializeField] private GameObject moduleButtonPrefab;


    void Start()
    {
    
        GUIFunctions.OnShowModulesClicked += ShowModules;
       
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

 
    private void ClearUI()
    {
        foreach (Transform child in contentArea)
            Destroy(child.gameObject);
    }

    private void CreateModuleButton(Module module)
    {
        GameObject buttonObj = Instantiate(moduleButtonPrefab, contentArea);

        Button button = buttonObj.GetComponent<Button>();
        TMP_Text label = buttonObj.GetComponentInChildren<TMP_Text>();

        label.text = module.name;

        // When button clicked → Show inspector for this module
      
    }
    private void ShowModules()
    {
        if (SelectObject.currentlySelectedObject != null)
        {
            ClearUI();

            if (SelectObject.currentlySelectedObject == null)
                return;

            Module[] modules = SelectObject.currentlySelectedObject.GetComponentsInChildren<Module>();

            foreach (var module in modules)
            {
                CreateModuleButton(module);
            }
        }
      
    }
   
}
