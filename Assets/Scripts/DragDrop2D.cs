using System.Collections.Generic;
using UnityEngine;

public class DragDrop2D : MonoBehaviour
{
    public static event System.Action OnFichaMovida;

    private RaycastHit2D hit;
    private Camera cam;

    private Dictionary<Transform, Transform> pieceToGhostMap = new Dictionary<Transform, Transform>();

    private Transform draggingGhost = null;
    private Transform draggingOriginal = null;

    private Transform previousValidGoal = null;
    private Vector3 previousGoalPosition;
    private Vector3 originalSpawnPosition;

    private int partsLayer;
    private int ghostLayer;
    private int goalLayer;

    private void OnEnable()
    {
        ReasignarCamara();
    }

    private void ReasignarCamara()
    {
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("Main Camera no encontrada. Asegúrate de que tenga la etiqueta 'MainCamera'.");
        }
    }

    private void Start()
    {
        partsLayer = LayerMask.GetMask("Parts");
        ghostLayer = LayerMask.GetMask("Ghost");
        goalLayer = LayerMask.GetMask("Goal");
    }

    private void Update()
    {
        if (cam == null)
        {
            ReasignarCamara();
            if (cam == null) return;
        }

        HandleLeftClick();
        HandleDrag();
        HandleDrop();
        HandleRightClick();
    }

    private void HandleLeftClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        int mask = partsLayer | ghostLayer;
        hit = Physics2D.GetRayIntersection(cam.ScreenPointToRay(Input.mousePosition), Mathf.Infinity, mask);
        if (hit.collider == null) return;

        Transform clicked = hit.transform;

        if (clicked.name.Contains("_Ghost"))
        {
            BeginDraggingGhost(clicked);
        }
        else
        {
            PiezaEstado estado = clicked.GetComponent<PiezaEstado>();
            if (estado == null || estado.hasClone) return;

            draggingOriginal = clicked;
            originalSpawnPosition = clicked.position;

            if (!pieceToGhostMap.ContainsKey(draggingOriginal))
                CreateGhostFromOriginal();

            draggingGhost = pieceToGhostMap[draggingOriginal];
        }
    }

    private void BeginDraggingGhost(Transform ghost)
    {
        draggingGhost = ghost;
        draggingOriginal = GetOriginalFromGhost(ghost);

        previousValidGoal = GetCurrentGoal(ghost);
        if (previousValidGoal != null)
        {
            previousGoalPosition = previousValidGoal.position;
            Meta metaPrev = previousValidGoal.GetComponent<Meta>();
            if (metaPrev != null)
            {
                metaPrev.isFull = false;
                metaPrev.fichaAsignada = null;
            }
        }
    }

    private void CreateGhostFromOriginal()
    {
        Transform ghost = Instantiate(draggingOriginal, draggingOriginal.position, draggingOriginal.rotation);
        ghost.name = draggingOriginal.name + "_Ghost";
        SetLayerRecursively(ghost.gameObject, LayerMask.NameToLayer("Ghost"));

        SpriteRenderer sr = ghost.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = new Color(0.7f, 0.7f, 0.7f, 0.7f);

        pieceToGhostMap[draggingOriginal] = ghost;
        AudioController.Instance?.PlaySelect();
    }

    private void HandleDrag()
    {
        if (draggingGhost == null || !Input.GetMouseButton(0)) return;
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -cam.transform.position.z;
        draggingGhost.position = cam.ScreenToWorldPoint(mousePos);
    }

    private void HandleDrop()
    {
        if (!Input.GetMouseButtonUp(0) || draggingGhost == null) return;
        TryPlaceOrMoveGhost(draggingGhost, draggingOriginal);
        draggingGhost = null;
        draggingOriginal = null;
    }

    private void TryPlaceOrMoveGhost(Transform ghost, Transform original)
    {
        Collider2D[] posiblesMetas = Physics2D.OverlapCircleAll(
            ghost.position,
            0.3f,
            goalLayer
        );

        Meta metaSeleccionada = null;
        float menorDistancia = float.MaxValue;

        foreach (Collider2D col in posiblesMetas)
        {
            Meta meta = col.GetComponent<Meta>();
            if (meta != null && !meta.isFull)
            {
                float distancia = Vector2.Distance(ghost.position, meta.transform.position);
                if (distancia < menorDistancia)
                {
                    menorDistancia = distancia;
                    metaSeleccionada = meta;
                }
            }
        }

        if (metaSeleccionada != null)
        {
            ghost.position = metaSeleccionada.transform.position;
            ghost.SetParent(metaSeleccionada.transform);

            PiezaEstado estado = original.GetComponent<PiezaEstado>();
            if (estado != null) estado.hasClone = true;

            metaSeleccionada.isFull = true;
            metaSeleccionada.fichaAsignada = ghost.gameObject;

            previousValidGoal = metaSeleccionada.transform;
            previousGoalPosition = metaSeleccionada.transform.position;

            OnFichaMovida?.Invoke();
            return;
        }

        ReturnToPreviousPosition(ghost);
    }

    private void ReturnToPreviousPosition(Transform ghost)
    {
        if (previousValidGoal != null && previousValidGoal.GetComponent<Meta>()?.isFull == false)
        {
            ghost.position = previousGoalPosition;
            Meta m = previousValidGoal.GetComponent<Meta>();
            if (m != null)
            {
                m.isFull = true;
                m.fichaAsignada = ghost.gameObject;
            }
        }
        else
        {
            Transform orig = GetOriginalFromGhost(ghost);
            if (orig != null)
            {
                PiezaEstado estado = orig.GetComponent<PiezaEstado>();
                if (estado != null) estado.hasClone = false;
                pieceToGhostMap.Remove(orig);
            }
            Destroy(ghost.gameObject);
        }
    }

    private void HandleRightClick()
    {
        if (!Input.GetMouseButtonDown(1)) return;
        hit = Physics2D.GetRayIntersection(cam.ScreenPointToRay(Input.mousePosition), Mathf.Infinity, ghostLayer);
        if (hit.collider == null) return;

        Transform ghostToDelete = hit.transform;
        Transform originalPiece = GetOriginalFromGhost(ghostToDelete);

        if (originalPiece != null)
        {
            PiezaEstado estado = originalPiece.GetComponent<PiezaEstado>();
            if (estado != null) estado.hasClone = false;
            pieceToGhostMap.Remove(originalPiece);
        }

        AudioController.Instance?.PlayDeselect();

        Meta meta = ghostToDelete.GetComponentInParent<Meta>();
        if (meta != null)
        {
            meta.isFull = false;
            meta.fichaAsignada = null;
        }
        else
        {
            Collider2D metaHit = Physics2D.OverlapBox(
                ghostToDelete.position,
                ghostToDelete.GetComponent<BoxCollider2D>().size,
                0,
                goalLayer
            );

            if (metaHit != null)
            {
                Meta metaFallback = metaHit.GetComponent<Meta>();
                if (metaFallback != null)
                {
                    metaFallback.isFull = false;
                    metaFallback.fichaAsignada = null;
                }
            }
        }

        previousValidGoal = null;
        previousGoalPosition = Vector3.zero;
        Destroy(ghostToDelete.gameObject);
        OnFichaMovida?.Invoke();
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, newLayer);
    }

    private Transform GetCurrentGoal(Transform piece)
    {
        Collider2D metaHit = Physics2D.OverlapBox(
            piece.position,
            piece.GetComponent<BoxCollider2D>().size,
            0,
            goalLayer
        );
        return metaHit != null ? metaHit.transform : null;
    }

    private Transform GetOriginalFromGhost(Transform ghost)
    {
        foreach (var pair in pieceToGhostMap)
            if (pair.Value == ghost)
                return pair.Key;
        return null;
    }
}
