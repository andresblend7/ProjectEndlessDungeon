using UnityEngine;

public class DialogsController : MonoBehaviour
{
    public GameObject dialogContainer;
    public TMPro.TextMeshProUGUI dialogText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogContainer.SetActive(false);
    }


    public void ShowDialog(string text)
    {
        dialogText.text = text;
        dialogContainer.SetActive(true);
    }
    public void HideDialog()
    {
        dialogContainer.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
