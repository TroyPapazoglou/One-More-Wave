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

public class Player : MonoBehaviour
{
    [SerializeField] private SpriteRenderer namePin;
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRen;

    private PlayerState playerState = PlayerState.Alive;
    public PlayerState PlayerState => playerState;  
    public event Action OnPlayerKilled;
    
    private bool isDying = false;


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

        FadeIn(2.5f, .5f);
    }

    private void Awake()
    {
        HideVisual();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDying) return;

        if (collision.gameObject.TryGetComponent<Enemy>(out Enemy enemy))
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
        transform.localScale = Vector3.zero;      
        GameManager.Instance.UIManager.FadeAlphaIfNeeded(canvasGroup, 0, 1.5f);      
        GameManager.Instance.UIManager.FadeAlphaIfNeeded(namePin, 0, 1.5f);
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
        Tween.Alpha(canvasGroup, 0, 1.5f);
        Tween.Alpha(namePin, 0, 1.5f);
    }

    public void Show()
    {
        Reset();
        transform.localScale = Vector3.one;
        spriteRen.enabled = true;
        Tween.Alpha(canvasGroup, 1, 1.5f);
        Tween.Alpha(namePin, 1, 1.5f);
    }

    private void FadeIn(float timeToFade = 1, float startDelay = 1.5f)
    {        
        Tween.Alpha(spriteRen, 1, timeToFade, startDelay: startDelay);
        Tween.Alpha(canvasGroup, 1, timeToFade, startDelay: startDelay);
        Tween.Alpha(namePin, 1, timeToFade, startDelay: startDelay);
    }
}
