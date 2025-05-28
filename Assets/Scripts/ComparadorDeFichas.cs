using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ComparadorDeFichas : MonoBehaviour
{
    private Dictionary<char, string[]> _cadenasPorGrupo = new Dictionary<char, string[]>();

    private void Start()
    {
        InicializarGrupos();
        DragDrop2D.OnFichaMovida += ActualizarCadenas;
    }

    private void OnDestroy()
    {
        DragDrop2D.OnFichaMovida -= ActualizarCadenas;
    }

    private void InicializarGrupos()
    {
        _cadenasPorGrupo.Clear();
        _cadenasPorGrupo.Add('A', new string[3] { "NNNNNNNNNNNNNNNN", "NNNNNNNNNNNNNNNN", "NNNNNNNNNNNNNNNN" });
        _cadenasPorGrupo.Add('B', new string[3] { "NNNNNNNNNNNNNNNN", "NNNNNNNNNNNNNNNN", "NNNNNNNNNNNNNNNN" });
        _cadenasPorGrupo.Add('C', new string[3] { "NNNNNNNNNNNNNNNN", "NNNNNNNNNNNNNNNN", "NNNNNNNNNNNNNNNN" });
    }

    public void ActualizarCadenas()
    {
        ReiniciarCadenas();

        foreach (Meta meta in FindObjectsOfType<Meta>().Where(m => m.isFull && m.fichaAsignada != null))
        {
            if (!_cadenasPorGrupo.ContainsKey(meta.grupo)) continue;

            PiezaEstado fichaEstado = meta.fichaAsignada.GetComponent<PiezaEstado>();
            if (fichaEstado != null && fichaEstado.Codigo.Length == 16)
            {
                _cadenasPorGrupo[meta.grupo][meta.indiceEnGrupo] = fichaEstado.Codigo;
            }
        }
    }

    private void ReiniciarCadenas()
    {
        foreach (char grupo in _cadenasPorGrupo.Keys.ToList())
        {
            for (int i = 0; i < 3; i++)
                _cadenasPorGrupo[grupo][i] = new string('N', 16);
        }
    }

    public string CalcularResultado(char grupo)
    {
        if (!_cadenasPorGrupo.ContainsKey(grupo))
            return new string('N', 16);

        string[] cadenas = _cadenasPorGrupo[grupo];
        char[] resultado = new char[16];

        for (int i = 0; i < 16; i++)
        {
            char a = cadenas[0][i];
            char b = cadenas[1][i];
            char c = cadenas[2][i];

            bool isOverload = a == 'B' && b == 'B' && c == 'B';
            if (isOverload)
            {
                resultado[i] = 'R'; // Sobrecarga
            }
            else
            {
                resultado[i] = CompararCaracteres(a, b, c);
            }
        }

        return new string(resultado);
    }

    private char CompararCaracteres(char a, char b, char c)
    {
        char[] valores = new[] { a, b, c };

        int indexV = System.Array.IndexOf(valores, 'V');
        if (indexV != -1)
        {
            for (int i = indexV; i < valores.Length; i++)
                valores[i] = 'N';
        }

        return valores.Any(v => v == 'B') ? 'B' : 'N';
    }
}
