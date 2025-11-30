using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SelectObject : MonoBehaviour
{
    [Header("Dependencies")]
    private Camera mainCamera;

    [Header("Highlight Materials")]
    public Material outlineMaterial;          // Hover
    public Material selectionMaterial;        // Single click
    public Material multiSelectionMaterial;   // Multi-select drag

    public static GameObject currentlySelectedObject = null;
    public static List<GameObject> currentlyMultiSelectedObjects = new List<GameObject>();
    // For drawing screen rectangle
    private Texture2D selectionTexture;

    private GameObject currentlyOutlinedObject = null;

    private Dictionary<Renderer, Material> outlineOriginalMaterials = new Dictionary<Renderer, Material>();
    private Dictionary<Renderer, Material> selectionOriginalMaterials = new Dictionary<Renderer, Material>();
    private Dictionary<GameObject, Dictionary<Renderer, Material>> multiOriginalMaterials =
        new Dictionary<GameObject, Dictionary<Renderer, Material>>();

    public static event Action<GameObject> OnSelectionChanged;

    // Drag selection variables
    private bool isDragging = false;
    private Vector2 dragStartPos;

    void Start()
    {
        mainCamera = Camera.main;

        // Create a simple 1×1 white texture
        selectionTexture = new Texture2D(1, 1);
        selectionTexture.SetPixel(0, 0, new Color(1, 1, 1, 0.25f)); // 25% opacity
        selectionTexture.Apply();
    }

    void Update()
    {
        // Start drag
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            dragStartPos = Mouse.current.position.ReadValue();
            isDragging = true;
        }

        // End drag
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (isDragging)
            {
                Vector2 dragEndPos = Mouse.current.position.ReadValue();

                if (Vector2.Distance(dragStartPos, dragEndPos) < 10)
                {
                    // Not a drag, normal single click
                    isDragging = false;
                    HandleSingleSelection();
                }
                else
                {
                    // Drag selection
                    HandleMultiSelection(dragStartPos, dragEndPos);
                }
            }
        }

        // Update hover
        if (!isDragging)
            HandleHover();
    }
    void OnGUI()
    {
        if (isDragging)
        {
            Vector2 currentMousePos = Mouse.current.position.ReadValue();
            Rect rawRect = GetScreenRect(dragStartPos, currentMousePos);

            // Convert to GUI space
            Rect guiRect = ConvertToGUISpace(rawRect);

            // Draw filled rectangle
            GUI.DrawTexture(guiRect, selectionTexture);

            // Draw border
            DrawScreenRectBorder(guiRect, 2, Color.white);
        }
    }

    private Rect ConvertToGUISpace(Rect rect)
    {
        // Convert Y to top-left GUI space
        return new Rect(
            rect.xMin,
            Screen.height - rect.yMax, // flip rect vertically
            rect.width,
            rect.height
        );
    }

    private void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        // top
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, thickness), selectionTexture);
        // left
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, thickness, rect.height), selectionTexture);
        // right
        GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), selectionTexture);
        // bottom
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), selectionTexture);
    }

    // --------------------------
    // SINGLE CLICK SELECTION
    // --------------------------
    private void HandleSingleSelection()
    {
        CleanupMultiSelection();

        GameObject hitObject = GetObjectUnderMouse();

        if (hitObject != currentlySelectedObject)
        {
            if (currentlySelectedObject != null)
            {
                RestoreMaterials(currentlySelectedObject, selectionOriginalMaterials);
                selectionOriginalMaterials.Clear();
            }

            currentlySelectedObject = hitObject;
            OnSelectionChanged?.Invoke(currentlySelectedObject);

            if (currentlySelectedObject != null)
            {
                if (currentlyOutlinedObject == currentlySelectedObject)
                {
                    RestoreMaterials(currentlyOutlinedObject, outlineOriginalMaterials);
                    outlineOriginalMaterials.Clear();
                    currentlyOutlinedObject = null;
                }

                ApplyMaterial(currentlySelectedObject, selectionMaterial, selectionOriginalMaterials);
            }
        }
    }

    // --------------------------
    // MULTI-SELECT DRAG
    // --------------------------
    private void HandleMultiSelection(Vector2 start, Vector2 end)
    {
        isDragging = false;

        // Clear single selection
        if (currentlySelectedObject != null)
        {
            RestoreMaterials(currentlySelectedObject, selectionOriginalMaterials);
            selectionOriginalMaterials.Clear();
            currentlySelectedObject = null;
        }

        CleanupMultiSelection();

        currentlyMultiSelectedObjects.Clear();
        multiOriginalMaterials.Clear();

        Rect selectionRect = GetScreenRect(start, end);

        Collider2D[] allColliders = FindObjectsOfType<Collider2D>();

        foreach (Collider2D col in allColliders)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(col.transform.position);

            if (selectionRect.Contains(screenPos))
            {
                GameObject obj = col.gameObject;
                currentlyMultiSelectedObjects.Add(obj);

                // Save originals per object
                multiOriginalMaterials[obj] = new Dictionary<Renderer, Material>();
                ApplyMaterial(obj, multiSelectionMaterial, multiOriginalMaterials[obj]);
            }
        }
    }

    private Rect GetScreenRect(Vector2 p1, Vector2 p2)
    {
        return new Rect(
            Mathf.Min(p1.x, p2.x),
            Mathf.Min(p1.y, p2.y),
            Mathf.Abs(p1.x - p2.x),
            Mathf.Abs(p1.y - p2.y));
    }

    private void CleanupMultiSelection()
    {
        foreach (var kvp in multiOriginalMaterials)
        {
            RestoreMaterials(kvp.Key, kvp.Value);
        }
        multiOriginalMaterials.Clear();
        currentlyMultiSelectedObjects.Clear();
    }

    // --------------------------
    // HOVER LOGIC (UNCHANGED)
    // --------------------------
    private void HandleHover()
    {
        GameObject hitObject = GetObjectUnderMouse();

        if (hitObject != currentlyOutlinedObject)
        {
            if (currentlyOutlinedObject != null)
            {
                if (currentlyOutlinedObject != currentlySelectedObject &&
                    !currentlyMultiSelectedObjects.Contains(currentlyOutlinedObject))
                {
                    RestoreMaterials(currentlyOutlinedObject, outlineOriginalMaterials);
                }
                outlineOriginalMaterials.Clear();
            }

            currentlyOutlinedObject = hitObject;

            if (currentlyOutlinedObject != null)
            {
                if (currentlyOutlinedObject != currentlySelectedObject &&
                    !currentlyMultiSelectedObjects.Contains(currentlyOutlinedObject))
                {
                    ApplyMaterial(currentlyOutlinedObject, outlineMaterial, outlineOriginalMaterials);
                }
            }
        }
    }

    private GameObject GetObjectUnderMouse()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Mathf.Abs(mainCamera.transform.position.z)));

        Collider2D hit = Physics2D.OverlapPoint(worldPos);

        if (hit != null)
            return hit.transform.gameObject;

        return null;
    }

    private void ApplyMaterial(GameObject target, Material newMat, Dictionary<Renderer, Material> dictToSaveTo)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            if (!dictToSaveTo.ContainsKey(rend))
                dictToSaveTo[rend] = rend.material;

            rend.material = newMat;
        }
    }

    private void RestoreMaterials(GameObject target, Dictionary<Renderer, Material> dictToReadFrom)
    {
        foreach (var kvp in dictToReadFrom)
        {
            if (kvp.Key != null)
                kvp.Key.material = kvp.Value;
        }
    }
}
