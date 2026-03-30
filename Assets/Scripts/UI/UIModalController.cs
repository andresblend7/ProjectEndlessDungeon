using System;
using UnityEngine;
using DG.Tweening;

public class UIModalController : MonoBehaviour
{
    public static UIModalController Instance;

    public CanvasGroup modalContainer;
    [SerializeField] public GameObject furnacePanel;
    [SerializeField] public GameObject shopPanel;
    [SerializeField] public GameObject SharpeningStone;
    public TMPro.TextMeshProUGUI modalTitle;

    // EFFECTS
    public float fadeTime = 1f;

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

        FadeInModal();
        
    }

    private void FadeInModal()
    {
        modalContainer.alpha = 0f;
        modalContainer.gameObject.SetActive(true);
        modalContainer.DOFade(1f, fadeTime).OnComplete(() =>
        {
            Time.timeScale = 0f; // opcional
            currentModal.SetActive(true);
        });

        //currentModal.SetActive(true);
    }

    public void CloseCurrent()
    {
        if (currentModal != null)
        {
            modalContainer.DOFade(0f, fadeTime);
            modalContainer.gameObject.SetActive(false);
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
