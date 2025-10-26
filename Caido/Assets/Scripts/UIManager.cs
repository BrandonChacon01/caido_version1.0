using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    // Una referencia a nuestro panel de Game Over que arrastraremos desde el editor.
    public GameObject panelGameOver;
    public GameObject HealthBar;
    public GameObject HeatBar;

    // Esta función pública será llamada por el jugador cuando su vida llegue a cero.
    public void MostrarPanelGameOver()
    {
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
            HealthBar.SetActive(false);
            HeatBar.SetActive(false);
        }
    }

    // Esta función pública será llamada por el botón de reinicio.
    public void ReiniciarNivel()
    {
        HealthBar.SetActive(true);
        HeatBar.SetActive(true);
        // Carga la escena que está actualmente activa, efectivamente reiniciándola.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}