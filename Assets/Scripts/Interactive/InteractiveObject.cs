using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    public UIModalType modalType;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void Interact()
    {
        UIModalController.Instance.OpenModal(modalType);
    }

}
