using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPositioner : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera mainCam;           
    public float verticalOffset = 10f; 
    public float distanceFactor = 0.8f; 
    public float padding = 1f;         


    private void Start()
    {
        if (mainCam == null)
        {
            mainCam = Camera.main;
        }

        // Se GridManager.instance è null, aspetta un frame e riprova
        if (GridManager.instance == null)
        {
            Invoke(nameof(RepositionCameraBasedOnIngredients), 0.1f);
        }
        else
        {
            RepositionCameraBasedOnIngredients();
        }
    }

    public void RepositionCameraBasedOnIngredients()
    {
        TileNode[,] grid = GridManager.instance.grid;
        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;
        bool foundAny = false;

        // Scansiona tutti i tile non vuoti
        foreach (TileNode tile in grid)
        {
            if (tile == null || tile.sceneObject == null)
                continue;
            if (tile.tile.tileState == TileData.TileState.EMPTY)
                continue;

            foundAny = true;
            Vector3 pos = tile.sceneObject.transform.position;
            if (pos.x < minX) minX = pos.x;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.z < minZ) minZ = pos.z;
            if (pos.z > maxZ) maxZ = pos.z;
        }

        if (!foundAny)
        {
            Debug.LogWarning("Nessun ingrediente trovato per riposizionare la camera.");
            return;
        }

        
        minX -= padding;
        maxX += padding;
        minZ -= padding;
        maxZ += padding;

       
        float centerX = (minX + maxX) * 0.5f;
        float centerZ = (minZ + maxZ) * 0.5f;
        
        float width = maxX - minX;
        float depth = maxZ - minZ;
        float maxExtent = Mathf.Max(width, depth);

      
        float distance = maxExtent * distanceFactor;

       
        Vector3 camPos = new Vector3(centerX, verticalOffset, centerZ - distance);
        mainCam.transform.position = camPos;

       
        mainCam.transform.LookAt(new Vector3(centerX, 0f, centerZ));
    }
}
