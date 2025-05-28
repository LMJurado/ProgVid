using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    [Header("Nombres de escenas de nivel (en orden)")]
    public string[] nombresDeNiveles = new string[]
    {
        "L37", "X23", "B62", "V13", "C06", "V77", "S12", "R80", "X09", "P12"
    };

    [Header("Escenas especiales")]
    public string escenaMenuPrincipal = "MainScene";
    public string escenaSeleccionNiveles = "LevelScene";

    [Header("Botones de nivel (solo en LevelScene)")]
    public Button[] botonesDeNivel; // Asignar en Unity por orden

    private int nivelDesbloqueado;

    private void Start()
    {
        CargarProgreso();

        // Si estamos en la escena de selección, aplicar bloqueo
        if (SceneManager.GetActiveScene().name == escenaSeleccionNiveles && botonesDeNivel.Length > 0)
        {
            for (int i = 0; i < botonesDeNivel.Length && i < nombresDeNiveles.Length; i++)
            {
                bool desbloqueado = i <= nivelDesbloqueado;
                botonesDeNivel[i].interactable = desbloqueado;

                string nivel = nombresDeNiveles[i];
                botonesDeNivel[i].onClick.RemoveAllListeners();
                botonesDeNivel[i].onClick.AddListener(() => CargarNivel(nivel));
            }
        }
    }

    private void CargarProgreso()
    {
        nivelDesbloqueado = PlayerPrefs.GetInt("NivelDesbloqueado", 0);
    }

    private void GuardarProgreso()
    {
        PlayerPrefs.SetInt("NivelDesbloqueado", nivelDesbloqueado);
        PlayerPrefs.Save();
    }

    public void CargarNivel(string nombreNivel)
    {
        SceneManager.LoadScene(nombreNivel);
    }

    public void CargarSiguienteNivel(string nombreActual)
    {
        int index = System.Array.IndexOf(nombresDeNiveles, nombreActual);
        if (index != -1 && index + 1 < nombresDeNiveles.Length)
        {
            if (index + 1 > nivelDesbloqueado)
            {
                nivelDesbloqueado = index + 1;
                GuardarProgreso();
            }
        }
    }

    public void IrAlMenuPrincipal()
    {
        SceneManager.LoadScene(escenaMenuPrincipal);
    }

    public void IrASeleccionDeNiveles()
    {
        SceneManager.LoadScene(escenaSeleccionNiveles);
    }

    public void IniciarDesdeElPrincipio()
    {
        if (nombresDeNiveles.Length > 0)
            SceneManager.LoadScene(nombresDeNiveles[0]);
    }

    public void Continuar()
    {
        if (nivelDesbloqueado < nombresDeNiveles.Length)
            SceneManager.LoadScene(nombresDeNiveles[nivelDesbloqueado]);
        else
            SceneManager.LoadScene(nombresDeNiveles[nombresDeNiveles.Length - 1]);
    }

    public void Salir()
    {
        Application.Quit();
    }
}
