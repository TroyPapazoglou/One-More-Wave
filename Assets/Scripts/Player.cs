using PrimeTween;
using System;
using System.Collections;
using System.Data.SqlTypes;
using UnityEngine;

public enum PlayerState
{ 
    Alive,
    Dead
}


[RequireComponent(typeof(PlayerMovement))]
public class Player : MonoBehaviour
{
    [SerializeField] private SpriteRenderer namePin;
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRen;

    private PlayerState playerState = PlayerState.Alive;
    public PlayerState PlayerState => playerState;  
    public event Action OnPlayerKilled;

    private PlayerMovement playerMovement;
    public PlayerMovement Movement => playerMovement;    

    private bool isDying = false;
    private Sequence fadeSequence;


    protected void Reset()
    {
        gameObject.transform.position = Vector3.zero;
        playerState = PlayerState.Alive;
        isDying = false;
        rb.simulated = true;   

    }
    
    void Start()
    {   
        if (EnemyManager.Instance != null) 
            EnemyManager.Instance.SetPlayer(this);

        playerMovement = GetComponent<PlayerMovement>();

        FadeIn(2.5f, .5f);
    }

    private void Awake()
    {
        HideVisual();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDying) return;

        if (collision.gameObject.TryGetComponent(out Enemy enemy))
        {           
            KillPlayer();
            Vector2 contactPoint = collision.GetContact(0).point; 
            StartCoroutine(DeathRoutine(enemy, contactPoint));          
        }
    }

    public void KillPlayer()
    {      
        isDying = true;
        OnPlayerKilled?.Invoke();
        playerState = PlayerState.Dead;
        rb.simulated = false;
        rb.linearVelocity = Vector2.zero; 
    }

    public void KillPlayerCrushed(Heavy heavy)
    {
        if (isDying) return;

        KillPlayer();
        StartCoroutine(CrushDeathRoutine(heavy));
    }

    private IEnumerator DeathRoutine(Enemy enemy, Vector2 contactPoint)
    {
        yield return DeathFadeSequence.Instance.Play(this, enemy);

        enemy.PlayDeathAnimation(contactPoint, this, enemy);     
    }
    private IEnumerator CrushDeathRoutine(Heavy heavy)
    {   
        yield return DeathFadeSequence.Instance.Play(this, heavy);
     
    }

    public bool IsAlive()
    {
        return playerState == PlayerState.Alive;
    }

    
    public void Hide()
    {
        if (fadeSequence.isAlive)
            fadeSequence.Stop();

        transform.localScale = Vector3.zero;

        Color spriteColour = spriteRen.color;
        spriteColour.a = 0;

        spriteRen.color = spriteColour;

        fadeSequence = Sequence.Create(
            UIFader.FadeTween(canvasGroup, 0f, 1.5f)
        )
        .Group(UIFader.FadeTween(namePin, 0f, 1.5f));
    }

    private void HideVisual()
    {
        Color spriteColour = spriteRen.color;
        Color pinColour = namePin.color;

        spriteColour.a = 0;
        pinColour.a = 0;

        spriteRen.color = spriteColour;
        canvasGroup.alpha = 0;
        namePin.color = pinColour; 
    }

    public void HidePin()
    {
        if (fadeSequence.isAlive)
            fadeSequence.Stop();

        fadeSequence = Sequence.Create(
             UIFader.FadeTween(canvasGroup, 0f, 1.5f)
         )
         .Group(UIFader.FadeTween(namePin, 0f, 1.5f));
    }

    public void Show()
    {
        Reset();
        transform.localScale = Vector3.one;
        spriteRen.enabled = true;
        //canvasGroup.alpha = 1;

        //Color newPinColor = namePin.color;
        //newPinColor.a = 1;

        //namePin.color = newPinColor;
    }

    public void FadeIn(float timeToFade = 1f, float startDelay = 1.5f)
    {
        if (fadeSequence.isAlive)
            fadeSequence.Stop();

        if (transform.localScale == Vector3.zero)
            transform.localScale = Vector3.one;

        bool spriteNeedsFade = spriteRen.color.a < 0.999f;
        bool canvasNeedsFade = canvasGroup.alpha < 0.999f;
        bool namePinNeedsFade = namePin.color.a < 0.999f;

        if (!spriteNeedsFade && !canvasNeedsFade && !namePinNeedsFade)
            return;

        fadeSequence = Sequence.Create();

        if (spriteNeedsFade)
        {
            fadeSequence.Insert(startDelay,
                UIFader.FadeTween(spriteRen, 1f, timeToFade));
        }

        if (canvasNeedsFade)
        {
            fadeSequence.Insert(startDelay,
                UIFader.FadeTween(canvasGroup, 1f, timeToFade));
        }

        if (namePinNeedsFade)
        {
            fadeSequence.Insert(startDelay,
                UIFader.FadeTween(namePin, 1f, timeToFade));
        }
    }

    public void FadeOut(float timeToFade = 1, float startDelay = 0, float endDelay = 0, Action onComplete = null)
    {
        if (fadeSequence.isAlive)
            fadeSequence.Stop();

        fadeSequence = Sequence.Create(
            UIFader.FadeTween(spriteRen, 0f, timeToFade, startDelay, endDelay)
        )
        .Group(UIFader.FadeTween(canvasGroup, 0f, timeToFade, startDelay, endDelay))
        .Group(UIFader.FadeTween(namePin, 0f, timeToFade, startDelay, endDelay))
        .OnComplete(() =>
        {
            transform.localScale = Vector3.zero;
            onComplete?.Invoke();
        });
    }


}
