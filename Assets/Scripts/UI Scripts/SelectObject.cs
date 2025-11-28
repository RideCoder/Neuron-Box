using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SelectObject : MonoBehaviour
{
    [Header("Dependencies")]
    
    private Camera mainCamera;

    [Header("Selection Settings")]
    public Material outlineMaterial;   // Material for Hover
    public Material selectionMaterial; // Material for Click

    // The variable you asked for to track what is clicked
    public static GameObject currentlySelectedObject = null;

    // Internal tracking
    private GameObject currentlyOutlinedObject = null;
    private Dictionary<Renderer, Material> outlineOriginalMaterials = new Dictionary<Renderer, Material>();
    private Dictionary<Renderer, Material> selectionOriginalMaterials = new Dictionary<Renderer, Material>();

    public static event Action<GameObject> OnSelectionChanged;

    void Start()
    {
        mainCamera = Camera.main;
       
       
    }

    void Update()
    {
        // 1. Check for Left Click (Selection)
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            HandleSelection();
            
        }

        // 2. Handle Hovering
        // We only outline things that aren't currently selected (optional preference)
        HandleHover();
    }

    private void HandleSelection()
    {
        GameObject hitObject = GetOrganismUnderMouse();

        // If we clicked a different object or clicked into empty space
        if (hitObject != currentlySelectedObject)
        {
            // 1. Deselect the previous object (if any)
            if (currentlySelectedObject != null)
            {
                RestoreMaterials(currentlySelectedObject, selectionOriginalMaterials);
                selectionOriginalMaterials.Clear();
             
            }

            // 2. Set the new selection
            currentlySelectedObject = hitObject;
            OnSelectionChanged?.Invoke(currentlySelectedObject);
            // 3. Apply selection material to new object (if we hit something)
            if (currentlySelectedObject != null)
            {
                // CRITICAL FIX: If we select an object that is currently Outlined/Hovered,
                // we must reset the outline first. Otherwise, we might accidentally save 
                // the "Outline Material" as the "Original Material".
                if (currentlyOutlinedObject == currentlySelectedObject)
                {
                    RestoreMaterials(currentlyOutlinedObject, outlineOriginalMaterials);
                    outlineOriginalMaterials.Clear();
                    currentlyOutlinedObject = null;
                }

                // Now save the TRUE original materials and apply selection
                ApplyMaterial(currentlySelectedObject, selectionMaterial, selectionOriginalMaterials);
           
            }
        }
    }

    private void HandleHover()
    {
        GameObject hitObject = GetOrganismUnderMouse();

        // Only change outline if we moved to a new object
        if (hitObject != currentlyOutlinedObject)
        {
            // 1. Reset previous outlined object
            if (currentlyOutlinedObject != null)
            {
                // Don't restore materials if this object is the currently Selected one
                // (Because the Selection logic handles its own materials)
                if (currentlyOutlinedObject != currentlySelectedObject)
                {
                    RestoreMaterials(currentlyOutlinedObject, outlineOriginalMaterials);
                }
                outlineOriginalMaterials.Clear();
            }

            currentlyOutlinedObject = hitObject;

            // 2. Apply outline to new object
            if (currentlyOutlinedObject != null)
            {
                // Don't apply outline if the object is already Selected
                // (Selection material usually takes priority over hover)
                if (currentlyOutlinedObject != currentlySelectedObject)
                {
                    ApplyMaterial(currentlyOutlinedObject, outlineMaterial, outlineOriginalMaterials);
                }
            }
        }
    }

    // --- Helper Methods ---

    // Extracted the physics logic to a helper so both Hover and Click can use it
    private GameObject GetOrganismUnderMouse()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(
            new Vector3(mousePos.x, mousePos.y, Mathf.Abs(mainCamera.transform.position.z))
        );

        Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);

        if (hit != null && hit.gameObject != null)
        {
            GameObject rootObject = hit.transform.root.gameObject;
            // Assuming your Organism script is on the root or children
            Organism organism = rootObject.GetComponentInChildren<Organism>();

            if (organism != null)
            {
                return rootObject;
            }
        }
        return null;
    }

    // Generic method to swap materials and save originals to a specific dictionary
    private void ApplyMaterial(GameObject target, Material newMat, Dictionary<Renderer, Material> dictToSaveTo)
    {
        Renderer[] childRenderers = target.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in childRenderers)
        {
            if (!dictToSaveTo.ContainsKey(rend))
                dictToSaveTo[rend] = rend.material;

            rend.material = newMat;
        }
    }

    // Generic method to restore materials from a specific dictionary
    private void RestoreMaterials(GameObject target, Dictionary<Renderer, Material> dictToReadFrom)
    {
        foreach (var kvp in dictToReadFrom)
        {
            if (kvp.Key != null)
                kvp.Key.material = kvp.Value;
        }
    }

   
}