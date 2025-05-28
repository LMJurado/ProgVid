using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PiezaEstado : MonoBehaviour
{
    [Header("Estado de Clonación")]
    public bool hasClone = false;

    [Header("Código de la Pieza (16 letras: N, B, V)")]
    [SerializeField]
    private string codigo = "NNNNNNNNNNNNNNNN";

    [Header("Colores")]
    [SerializeField]
    private Color colorClon = new Color(0.16f, 0.99f, 0.69f);

    private SpriteRenderer spriteRenderer;
    private Color colorOriginal;

    public string Codigo => codigo;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        colorOriginal = spriteRenderer.color;
        // Validación adicional en runtime
        codigo = new string(codigo.Select(c => c == 'N' || c == 'B' || c == 'V' ? c : 'N').ToArray());
        if (codigo.Length != 16)
            codigo = codigo.PadRight(16, 'N').Substring(0, 16);
    }

    private void Update()
    {
        spriteRenderer.color = hasClone ? colorClon : colorOriginal;
    }
}

