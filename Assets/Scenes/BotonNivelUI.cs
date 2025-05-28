using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BotonNivelUI : MonoBehaviour
{
    public string nombreNivel; // Ej: "L37"
    public TextMeshProUGUI estadoTexto;

    private Button boton;
    private Image fondoBoton;

    void Start()
    {
        boton = GetComponent<Button>();
        fondoBoton = GetComponent<Image>();
        SceneChanger sceneChanger = FindObjectOfType<SceneChanger>();

        int index = System.Array.IndexOf(sceneChanger.nombresDeNiveles, nombreNivel);
        int desbloqueadoHasta = PlayerPrefs.GetInt("NivelDesbloqueado", 0);

        if (index < 0)
        {
            Debug.LogWarning($"Nivel {nombreNivel} no está en la lista de SceneChanger.");
            return;
        }

        if (index < desbloqueadoHasta)
        {
            // Completado
            estadoTexto.text = "Completado";
            boton.interactable = true;
            fondoBoton.color = HexToColor("#29FC8B");
        }
        else if (index == desbloqueadoHasta)
        {
            // Disponible
            estadoTexto.text = "Disponible";
            boton.interactable = true;
            fondoBoton.color = Color.white;
        }
        else
        {
            // Bloqueado
            estadoTexto.text = "Bloqueado";
            boton.interactable = false;
            fondoBoton.color = HexToColor("#D9D9D9");
        }

        // Asignar evento al botón
        boton.onClick.RemoveAllListeners();
        boton.onClick.AddListener(() => sceneChanger.CargarNivel(nombreNivel));
    }

    private Color HexToColor(string hex)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(hex, out color))
        {
            return color;
        }
        return Color.white;
    }
}
