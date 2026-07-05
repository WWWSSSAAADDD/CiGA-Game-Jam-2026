using Game.Gameplay.Ship;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private ShipStats shipStats;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip gameOverSfx;

    private bool isGameOver;

    private void Awake()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (shipStats != null)
            shipStats.OnDied += HandlePlayerDied;
    }

    private void OnDestroy()
    {
        if (shipStats != null)
            shipStats.OnDied -= HandlePlayerDied;
    }

    private void HandlePlayerDied()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        BGMManager.instance.BGMplay(BGMManager.instance.bgmLib.audioClips[1]);
        BGMManager.instance.SFXplay(gameOverSfx);

        //Time.timeScale = 0f;
    }
}
