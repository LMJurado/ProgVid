using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class VerificadorFinal : MonoBehaviour
{
    [System.Serializable]
    public class ConfiguracionNivel
    {
        [Header("Patrones por Grupo (cada uno 16 letras)")]
        public string patronGrupoA = "NNNNNNNNNNNNNNNN";
        public string patronGrupoB = "NNNNNNNNNNNNNNNN";
        public string patronGrupoC = "NNNNNNNNNNNNNNNN";

        public string CadenaCompletaEsperada => patronGrupoA + patronGrupoB + patronGrupoC;
    }

    [Header("Configuración del Nivel")]
    public ConfiguracionNivel nivelActual;

    [Header("Configuración de Colores")]
    public Color colorBlanco = Color.white;
    public Color colorRojo = new Color(173f / 255f, 42f / 255f, 54f / 255f);
    public Color colorTransparente = new Color(0, 0, 0, 0);
    public Color colorVerde = Color.green;
    public Color colorFeedbackError = new Color(1f, 0.5f, 0.2f, 1f);

    [Header("Referencias Visuales")]
    public Transform contenedorVisualA;
    public Transform contenedorVisualB;
    public Transform contenedorVisualC;
    public GameObject prefabCuadradoVisual;

    [Header("Referencias")]
    public ComparadorDeFichas comparador;
    public SceneChanger sceneChanger;

    [Header("Bloqueador de Interacción (opcional)")]
    public GameObject bloqueadorDeInteraccion; // un panel con raycast blocker

    private SpriteRenderer[] visualizadoresA;
    private SpriteRenderer[] visualizadoresB;
    private SpriteRenderer[] visualizadoresC;

    public bool nivelCompletado { get; private set; }

    private void Start()
    {
        InicializarVisualizadores();
        DragDrop2D.OnFichaMovida += VerificarTodo;

        if (bloqueadorDeInteraccion != null)
            bloqueadorDeInteraccion.SetActive(false);

        //Fuerza la carga del estado actual desde las piezas precargadas
        comparador.ActualizarCadenas();

        //Ahora sí puede verificar correctamente
        VerificarTodo();
    }


    private void OnDisable()
    {
        DragDrop2D.OnFichaMovida -= VerificarTodo;
    }

    private void InicializarVisualizadores()
    {
        foreach (Transform child in contenedorVisualA) Destroy(child.gameObject);
        foreach (Transform child in contenedorVisualB) Destroy(child.gameObject);
        foreach (Transform child in contenedorVisualC) Destroy(child.gameObject);

        visualizadoresA = CrearVisualizadores(contenedorVisualA, nivelActual.patronGrupoA);
        visualizadoresB = CrearVisualizadores(contenedorVisualB, nivelActual.patronGrupoB);
        visualizadoresC = CrearVisualizadores(contenedorVisualC, nivelActual.patronGrupoC);
    }

    private SpriteRenderer[] CrearVisualizadores(Transform padre, string patron)
    {
        SpriteRenderer[] visualizadores = new SpriteRenderer[16];
        patron = patron.PadRight(16, 'N').Substring(0, 16);
        float anchoSprite = prefabCuadradoVisual.GetComponent<SpriteRenderer>().bounds.size.x;

        for (int i = 0; i < 16; i++)
        {
            GameObject cuadrado = Instantiate(prefabCuadradoVisual, padre);
            cuadrado.transform.localPosition = new Vector3(i * anchoSprite, 0, 0);
            SpriteRenderer sr = cuadrado.GetComponent<SpriteRenderer>();
            sr.color = patron[i] == 'B' ? Color.black : colorTransparente;
            visualizadores[i] = sr;
        }

        return visualizadores;
    }

    private void VerificarTodo()
    {
        string resultadoA = comparador.CalcularResultado('A');
        string resultadoB = comparador.CalcularResultado('B');
        string resultadoC = comparador.CalcularResultado('C');

        ActualizarVisualizacion(resultadoA, resultadoB, resultadoC);

        string codigoCompleto = resultadoA + resultadoB + resultadoC;
        nivelCompletado = codigoCompleto == nivelActual.CadenaCompletaEsperada;

        Debug.Log($"Código generado: {codigoCompleto}");
        Debug.Log($"Esperado: {nivelActual.CadenaCompletaEsperada}");

        if (nivelCompletado)
        {
            AudioController.Instance?.PlayConfirm();
            StartCoroutine(ProcesoPostExito());
        }
    }

    private void ActualizarVisualizacion(string resultadoA, string resultadoB, string resultadoC)
    {
        ActualizarGrupoVisual(visualizadoresA, resultadoA, nivelActual.patronGrupoA);
        ActualizarGrupoVisual(visualizadoresB, resultadoB, nivelActual.patronGrupoB);
        ActualizarGrupoVisual(visualizadoresC, resultadoC, nivelActual.patronGrupoC);
    }

    private void ActualizarGrupoVisual(SpriteRenderer[] visualizadores, string resultado, string patronEsperado)
    {
        for (int i = 0; i < 16; i++)
        {
            char esperado = patronEsperado[i];
            char actual = resultado[i];

            if (esperado == 'B')
            {
                if (actual == 'B')
                    visualizadores[i].color = colorBlanco;
                else if (actual == 'N')
                    visualizadores[i].color = Color.black;
                else if (actual == 'R')
                    visualizadores[i].color = colorRojo;
            }
            else if (esperado == 'N')
            {
                if (actual == 'N')
                    visualizadores[i].color = colorTransparente;
                else if (actual == 'B')
                    visualizadores[i].color = colorFeedbackError;
                else if (actual == 'R')
                    visualizadores[i].color = colorRojo;
            }
        }
    }

    private void MostrarExitoCompleto()
    {
        foreach (var r in visualizadoresA) StartCoroutine(Parpadear(r, colorVerde));
        foreach (var r in visualizadoresB) StartCoroutine(Parpadear(r, colorVerde));
        foreach (var r in visualizadoresC) StartCoroutine(Parpadear(r, colorVerde));
    }

    private IEnumerator Parpadear(SpriteRenderer renderer, Color colorDestino)
    {
        Color original = renderer.color;
        for (int i = 0; i < 3; i++)
        {
            renderer.color = colorDestino;
            yield return new WaitForSeconds(0.2f);
            renderer.color = original;
            yield return new WaitForSeconds(0.2f);
        }
        renderer.color = colorDestino;
    }

    private IEnumerator ProcesoPostExito()
    {
        MostrarExitoCompleto();

        if (bloqueadorDeInteraccion != null)
            bloqueadorDeInteraccion.SetActive(true); // Bloquea input al usuario

        yield return new WaitForSeconds(2.5f); // Esperar animación

        // Desbloquea siguiente nivel
        FindObjectOfType<SceneChanger>()?.CargarSiguienteNivel(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        // Ir automáticamente a selección de niveles
        FindObjectOfType<SceneChanger>()?.IrASeleccionDeNiveles();
    }

}
