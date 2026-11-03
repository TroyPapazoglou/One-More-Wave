using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PersistentMenuButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button menuButton;
    [SerializeField] private Image buttonImage;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private UIManager uiManager;

    [Header("Properties")]
    [SerializeField] private float duration = 2;

    private bool isInMainMenu = true;

    private void Awake()
    {
        menuButton.onClick.AddListener(OnMenuButtonPressed);
    }

    private void OnDestroy()
    {
        menuButton.onClick.RemoveListener(OnMenuButtonPressed);
    }

    private void Start()
    {
        Color newButtonColor = buttonImage.color;
        newButtonColor.a = 0;

        buttonImage.color = newButtonColor;
        text.alpha = 0;
    }  

    public void SetMainMenuState(bool value)
    {
        isInMainMenu = value;
    }

    private void OnMenuButtonPressed()
    {
        uiManager.OnPersistentMenuButtonPressed(isInMainMenu);
    }

    public void Enable()
    {
        menuButton.interactable = true;
        FadeIn();
    }

    public void FadeIn()
    {
        UIFader.Fade(buttonImage, 1, duration);      
        UIFader.Fade(text, 1, duration);
    }
}
