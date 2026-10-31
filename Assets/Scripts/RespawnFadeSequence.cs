using UnityEngine;
using PrimeTween;

public class RespawnFadeSequence : MonoBehaviour
{
    public static RespawnFadeSequence Instance { get; private set; }
  
    [SerializeField] private GameObject[] walls;
    [SerializeField] private float fadeDuration = 1f;

    private Sequence respawnSequence;

    private void Awake() => Instance = this; 

    public void Play()
    {
        if (walls == null || walls.Length == 0)
            return;

        GameManager.Instance.CameraManager.ResetCamera();

        respawnSequence = Sequence.Create();

        foreach (GameObject wall in walls)
        {
            if (wall == null)
                continue;
            
            if (!wall.TryGetComponent<SpriteRenderer>(out var wallRenderer))
                continue;

            respawnSequence.Group
            (
                Tween.Alpha(wallRenderer, 1, fadeDuration)
            );
        }

        respawnSequence.OnComplete(() =>
        {
            GameManager.Instance.GetPlayer().Show();
            GameManager.Instance.UIManager.ShowMenuCanvas();
        });
    }
}
