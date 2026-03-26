using UnityEngine;

public class UIModalController : MonoBehaviour
{
    public static UIModalController Instance;

    public GameObject modalContainer;
    [SerializeField] public GameObject furnacePanel;
    [SerializeField] public GameObject shopPanel;
    [SerializeField] public GameObject SharpeningStone;
    public TMPro.TextMeshProUGUI modalTitle;

    private GameObject currentModal;

    private void Awake()
    {
        Instance = this;
    }

    public void OpenModal(UIModalType type)
    {
        CloseCurrent();

        switch (type)
        {
            case UIModalType.Furnace:
                currentModal = furnacePanel;
                break;

            case UIModalType.Shop:
                currentModal = shopPanel;
                break;
            case UIModalType.SharpeningStone:
                currentModal = SharpeningStone;
                break;
        }

        modalContainer.SetActive(true);
        currentModal.SetActive(true);
        Time.timeScale = 0f; // opcional
    }

    public void CloseCurrent()
    {
        if (currentModal != null)
        {
            modalContainer.SetActive(false); 
            currentModal.SetActive(false);
            currentModal = null;
            Time.timeScale = 1f;
        }
    }
}

public enum UIModalType
{
    Furnace,
    Shop,
    SharpeningStone
}
