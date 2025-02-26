using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using System.Reflection.Emit;

public class GameManager : MonoBehaviour
{
    public enum DIRECTIONS
    {
        DEFAULT,
        LEFT,
        RIGHT,
        UP,
        DOWN
    }

    public static GameManager instance;

    private const float REQUIRED_ARC = 45.0F;
    private const float TILE_HEIGHT = 0.30F;

    private GameObject selectedObject;
    private List<MoveData> moveHistory;

    public int unSandwichedElementCount;

    // Eventi di callback 
    public Action onLevelFailed;
    public Action onLevelCompleted;
    public Action onFinalAnimationsCanFired;

    private bool canMove = true;
    
    // Variabili per il tocco
    private Vector2 startTouchPos;
    private bool isSwiping = false;


    void MakeInstance()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Awake()
    {
        MakeInstance();
    }

    void Start()
    {
        moveHistory = new List<MoveData>();

        onLevelFailed += OnLevelFailed;
        onLevelCompleted += OnLevelCompleted;
    }

    private void Update()
    {
        //--------------------------------------------------
        // 1) GESTIONE MOUSE
        //--------------------------------------------------
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPos = Input.mousePosition;
            isSwiping = true;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f))
            {
                selectedObject = hit.collider.gameObject;
            }
            else
            {
                selectedObject = null;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isSwiping && selectedObject != null)
            {
                Vector2 endPos = (Vector2)Input.mousePosition;
                Vector2 finalDelta = endPos - startTouchPos;

                HandleSwipe(finalDelta);
            }
            isSwiping = false;
        }

        //--------------------------------------------------
        // 2) GESTIONE TOUCH
        //--------------------------------------------------
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);

            switch (t.phase)
            {
                case TouchPhase.Began:
                    startTouchPos = t.position;
                    isSwiping = true;

                    Ray touchRay = Camera.main.ScreenPointToRay(t.position);
                    RaycastHit hit;
                    if (Physics.Raycast(touchRay, out hit, 100f))
                    {
                        selectedObject = hit.collider.gameObject;
                    }
                    else
                    {
                        selectedObject = null;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (isSwiping && selectedObject != null)
                    {
                        Vector2 endTouchPos = t.position;
                        Vector2 finalDelta = endTouchPos - startTouchPos;

                        HandleSwipe(finalDelta);
                    }
                    isSwiping = false;
                    break;
            }
        }
    }

    private void HandleSwipe(Vector2 finalDelta)
    {
        if (selectedObject == null) return;

        var selectedTileNode = selectedObject.GetComponent<TileNodeBridge>()?.tileNode;
        if (selectedTileNode == null) return;

        // Logica di calcolo angolare e direzione
        if (AngleIsValid(0.0f, finalDelta) || AngleIsValid(45.0f, finalDelta))
        {
            // Up
            selectedTileNode = selectedTileNode.ParentOfAll;
            if (selectedTileNode.up != null && selectedTileNode.up.isAvailable)
            {
                MoveToTargetTile(selectedTileNode, selectedTileNode.up, DIRECTIONS.UP);
            }
            Debug.Log("Up");
        }
        else if (AngleIsValid(90.0f, finalDelta) || AngleIsValid(135.0f, finalDelta))
        {
            // Right
            selectedTileNode = selectedTileNode.ParentOfAll;
            if (selectedTileNode.right != null && selectedTileNode.right.isAvailable)
            {
                MoveToTargetTile(selectedTileNode, selectedTileNode.right, DIRECTIONS.RIGHT);
            }
            Debug.Log("Right");
        }
        else if (AngleIsValid(180.0f, finalDelta) || AngleIsValid(225.0f, finalDelta))
        {
            // Down
            selectedTileNode = selectedTileNode.ParentOfAll;
            if (selectedTileNode.down != null && selectedTileNode.down.isAvailable)
            {
                MoveToTargetTile(selectedTileNode, selectedTileNode.down, DIRECTIONS.DOWN);
            }
            Debug.Log("Down");
        }
        else if (AngleIsValid(270.0f, finalDelta) || AngleIsValid(315.0f, finalDelta))
        {
            // Left
            selectedTileNode = selectedTileNode.ParentOfAll;
            if (selectedTileNode.left != null && selectedTileNode.left.isAvailable)
            {
                MoveToTargetTile(selectedTileNode, selectedTileNode.left, DIRECTIONS.LEFT);
            }
            Debug.Log("Left");
        }
        else
        {
            Debug.Log("No Direction");
        }
    }

    public void OnLevelDataReady(int sandwichElementCount)
    {
        unSandwichedElementCount = sandwichElementCount;
    }

    private void MoveToTargetTile(TileNode selectedTileNode, TileNode targetTileNode, DIRECTIONS direction)
    {
        // 1. Verifico se sto per spostare BREAD su BREAD
        var movingState = selectedTileNode.tile.tileState;
        var targetState = targetTileNode.tile.tileState;

        bool movingIsJustBread = (movingState == TileData.TileState.BREAD && selectedTileNode.ChildCount == 0);
        bool targetIsJustBread = (targetState == TileData.TileState.BREAD && targetTileNode.ChildCount == 0);

        if (movingIsJustBread && targetIsJustBread)
        {
            Debug.Log("Non puoi impilare due fette di pane direttamente!");
            return; 
        }

        if (!canMove)
        {
            return;
        }

        unSandwichedElementCount--;

        canMove = false;
        var targetHeight = CalculateHeight(targetTileNode);
        var selectedHeight = CalculateHeight(selectedTileNode);
        selectedTileNode.isAvailable = false;

        Debug.Log("Target Node : " + targetTileNode.tile.tileState);

        targetTileNode.children.Add(selectedTileNode);
        selectedTileNode.parent = targetTileNode;

        moveHistory.Insert(0, new MoveData
        {
            node = targetTileNode,
            direction = direction,
            previousPosition = selectedTileNode.sceneObject.transform.position
        });

        Debug.LogWarning($"History Direction = ({moveHistory[0].direction})");
        Debug.LogWarning($"History Node = ({moveHistory[0].node.tile.tileState})");
        Debug.LogWarning($"History Prev Position = ({moveHistory[0].previousPosition})");


        selectedTileNode.sceneObject.transform
            .DORotate(
                selectedTileNode.sceneObject.transform.rotation.eulerAngles + new Vector3(GetRotationAngle(direction).x,
                    0, GetRotationAngle(direction).z), 0.3f, RotateMode.Fast);

        selectedTileNode.sceneObject.transform 
            .DOMove(
                new Vector3(targetTileNode.sceneObject.transform.position.x, targetHeight + selectedHeight,
                    targetTileNode.sceneObject.transform.position.z), 0.3f)
            .OnComplete(() =>
            {
                canMove = true;
                
                    
                selectedTileNode.sceneObject.transform.SetParent(targetTileNode.sceneObject.transform);

                
                if (unSandwichedElementCount == 1)
                {
                    selectedTileNode.isOnTheTop = true;
                    if (CheckIfTheLevelCompleted(selectedTileNode))
                    {
                        onLevelCompleted?.Invoke();
                    }
                    else
                    {
                        onLevelFailed?.Invoke();
                    }
                } 
                

                // Al termine dello spostamento, controlla la configurazione del panino
                if (CheckIfTheLevelCompleted(selectedTileNode))
                {
                    onLevelCompleted?.Invoke();
                }
                else
                {
                    onLevelFailed?.Invoke();
                }
            });
    }
   
    private bool CheckIfTheLevelCompleted(TileNode tileOnTheTop)
    {
        var tileNodes = GridManager.instance.grid;

        int breadCount = 0;
        foreach (var tileNode in tileNodes)
        {
            if (tileNode.tile.tileState != TileData.TileState.EMPTY && tileNode.isAvailable)
            {
                if (tileNode.tile.tileState != TileData.TileState.BREAD)
                {
                    return false;
                }

                breadCount++;
            }
        }

        if (tileOnTheTop.tile.tileState != TileData.TileState.BREAD)
            return false;

        if (breadCount == 0 || breadCount > 1)
        {
            return false;
        }

        return true;
    }

    private float CalculateHeight(TileNode tileNode)
    {
        var height = TILE_HEIGHT / 2.0f;
        var childCount = tileNode.ChildCount;
        height += childCount * TILE_HEIGHT;

        Debug.Log($"Target {tileNode.sceneObject} Child Count = {(childCount)}, TILE_HEIGHT = {TILE_HEIGHT}, Calculated Height: {height}");

        return height;
    }

    protected bool AngleIsValid(float requiredAngle, Vector2 vector)
    {
        var angle = Mathf.Atan2(vector.x, vector.y) * Mathf.Rad2Deg;
        var angleDelta = Mathf.DeltaAngle(angle, requiredAngle);

        if (angleDelta < REQUIRED_ARC * -0.5f || angleDelta >= REQUIRED_ARC * 0.5f)
        {
            return false;
        }

        return true;
    }

    public Vector3 GetRotationAngle(DIRECTIONS direction)
    {
        switch (direction)
        {
            case DIRECTIONS.UP:
                return new Vector3(180.0f, 0, 0);
            case DIRECTIONS.RIGHT:
                return new Vector3(0, 0, -180.0f);
            case DIRECTIONS.LEFT:
                return new Vector3(0, 0, 180.0f);
            case DIRECTIONS.DOWN:
                return new Vector3(-180.0f, 0, 0);
        }

        return Vector3.zero;
    }

    public void OnUndo()
    {
        canMove = false;
        ExecuteUndo().OnComplete(() => canMove = true);
    }

    private Tween ExecuteUndo()
    {
        if (moveHistory.Count == 0)
            return null;

        var moveData = moveHistory[0];
        moveHistory.RemoveAt(0);

        return UndoMoveData(moveData);
    }

    private Tween UndoMoveData(MoveData moveData)
    {
        unSandwichedElementCount++;
        var tileNode = moveData.node.children[moveData.node.children.Count - 1];

        var direction = moveData.direction;
        moveData.node.children.Remove(tileNode);
        tileNode.parent = null;
        tileNode.isAvailable = true;

        tileNode.sceneObject.transform
            .DORotate(
                tileNode.sceneObject.transform.rotation.eulerAngles + new Vector3(GetRotationAngle(direction).x, 0,
                    -GetRotationAngle(direction).z), 0.3f, RotateMode.Fast)
            .SetEase(Ease.Linear);

        var tween = tileNode.sceneObject.transform
            .DOMove(new Vector3(moveData.previousPosition.x, 0, moveData.previousPosition.z), 0.3f)
            .OnStart(() => { tileNode.sceneObject.transform.SetParent(null); });

        return tween;
    }

    public void Restart()
    {
        canMove = false;
        ChainedUndo();
    }

    private void ChainedUndo()
    {
        var tween = ExecuteUndo();
        if (tween != null)
        {
            tween.OnComplete(() => ChainedUndo());
        }
        else
        {
            canMove = true;
        }
    }

    private void OnLevelCompleted()
    {
        Debug.Log("LEVEL COMPLETED");
        GUIManager.instance.ShowWinPanel();
    }

    private void OnLevelFailed()
    {
        Debug.Log("LEVEL FAILED");
    }
}