using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject panelGameOver;
    public GameObject HealthBar;
    public GameObject HeatBar;

    public void MostrarPanelGameOver()
    {
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
            HealthBar.SetActive(false);
            HeatBar.SetActive(false);
        }
    }

    public void ReiniciarNivel()
    {
        HealthBar.SetActive(true);
        HeatBar.SetActive(true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 🔹 NUEVA FUNCIÓN
    public void VolverAlMenuPrincipal()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
