using UnityEngine;

public class ShowUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject menuRoot;
    void Start()
    {
        InputHandler.OnRightClick += OpenMenu;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OpenMenu()
    {
        if (!menuRoot.activeSelf)
        {
            menuRoot.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
            menuRoot.SetActive(true);
        }
        else
        {
            menuRoot.SetActive(false);
        }
    }
}
