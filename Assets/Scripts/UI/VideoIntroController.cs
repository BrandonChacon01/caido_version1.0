using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

/// <summary>
/// Controlador para reproducir un video introductorio antes de iniciar el juego
/// </summary>
public class VideoIntroController : MonoBehaviour
{
    [Header("Video Configuration")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string nextSceneName = "SampleScene"; // Escena del juego

    [Header("Skip Options")]
    [SerializeField] private bool allowSkip = true;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;
    [SerializeField] private KeyCode escapeKey = KeyCode.Escape;

    private bool videoFinished = false;
    private bool isLoadingNextScene = false;

    private void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoPlayer == null)
        {
            Debug.LogError("[VideoIntroController] No se encontró VideoPlayer. Cargando escena directamente...");
            LoadNextScene();
            return;
        }

        // Obtener la escena de destino desde LoadingPayload si existe
        if (!string.IsNullOrEmpty(LoadingPayload.NextScene))
        {
            nextSceneName = LoadingPayload.NextScene;
        }

        // Configurar eventos del VideoPlayer
        videoPlayer.loopPointReached += OnVideoFinished;

        // Iniciar reproducción
        videoPlayer.Play();
        Debug.Log("[VideoIntroController] Reproduciendo video intro...");
    }

    private void Update()
    {
        // Permitir saltar el video si está habilitado
        if (allowSkip && !isLoadingNextScene && !videoFinished)
        {
            if (Input.GetKeyDown(skipKey) || Input.GetKeyDown(escapeKey) || Input.anyKeyDown)
            {
                SkipVideo();
            }
        }
    }

    /// <summary>
    /// Se llama cuando el video termina de reproducirse
    /// </summary>
    private void OnVideoFinished(VideoPlayer vp)
    {
        videoFinished = true;
        Debug.Log("[VideoIntroController] Video terminado. Cargando juego...");
        LoadNextScene();
    }

    /// <summary>
    /// Salta el video y carga la siguiente escena
    /// </summary>
    public void SkipVideo()
    {
        if (isLoadingNextScene) return;

        Debug.Log("[VideoIntroController] Video saltado por el usuario.");
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }
        LoadNextScene();
    }

    /// <summary>
    /// Carga la siguiente escena (el juego)
    /// </summary>
    private void LoadNextScene()
    {
        if (isLoadingNextScene) return;

        isLoadingNextScene = true;
        Debug.Log($"[VideoIntroController] Cargando escena: {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }

    private void OnDestroy()
    {
        // Limpiar el evento
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}
