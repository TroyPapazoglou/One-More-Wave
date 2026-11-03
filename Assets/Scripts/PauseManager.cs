using PrimeTween;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class PauseManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [SerializeField] Volume volume;
    [SerializeField] float pausedValue = 1f;
    [SerializeField] float unpausedValue = 10f;
    [SerializeField] float tweenDuration = 0.25f;
    [SerializeField] Ease ease = Ease.OutQuad;

    DepthOfField dof;
    Tween apertureTween;

    void Awake()
    {
        IsPaused = false;     

        if (volume.profile.TryGet(out dof))
        {
            dof.aperture.overrideState = true;
            dof.aperture.value = unpausedValue;
        }
        else
        {
            Debug.LogWarning("No Depth Of Field override on this Volume's profile.");
        }
    }   

    void OnDestroy()
    {
        apertureTween.Stop();
        IsPaused = false;
        Time.timeScale = 1f;
    }
      
    public void Pause() => SetPaused(true);
    public void Resume() => SetPaused(false);
    public void Toggle() => SetPaused(!IsPaused);

    public void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;

        if (dof == null) return;
             
        apertureTween.Stop();

        float target = paused ? pausedValue : unpausedValue;
              
        apertureTween = Tween.Custom(
            this,
            dof.focusDistance.value,
            target,
            tweenDuration,
            (self, value) => self.dof.focusDistance.value = value,
            ease,
            useUnscaledTime: true);
    }
   
}