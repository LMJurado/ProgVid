using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Meta : MonoBehaviour
{
    [Header("Configuración")]
    public char grupo = 'A';  // A, B o C
    [Range(0, 2)] public int indiceEnGrupo = 0;
    public bool isFull = false;

    [Header("Referencia")]
    public GameObject fichaAsignada;

    private void Reset()
    {
        AjustarTamañoCollider();
    }

    private void OnValidate()
    {
       // AjustarTamañoCollider();
    }

    private void AjustarTamañoCollider()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (box != null && sr != null)
        {
            Vector2 size = sr.bounds.size * 0.6f; // 60% del tamaño visual
            box.size = size;
            box.offset = Vector2.zero;
        }
    }
}
