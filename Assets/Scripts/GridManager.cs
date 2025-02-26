using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    private const float START_X = -5.0F;
    private const float START_Y = 0f;
    private const float START_Z = -12.3f;
    private const int DEFAULT_COLUMN_COUNT = 4;
    private const int DEFAULT_ROW_COUNT = 4;
    private const float TILE_SIZE_X = 3.5F;
    private const float TILE_SIZE_Z = 3.5F;
    
    private int rows;
    private int columns;
    public TileNode[,] grid;

    private GameObject bread;
    private GameObject[] cheeseVariants;
    private GameObject avocado;
    private GameObject tomato;
    private GameObject empty;
    private GameObject bacon;
    private GameObject egg;
    private GameObject groundBeef;
    private GameObject ham;
    private GameObject onionRing;
    private GameObject pickle;
    private GameObject lettuce;
    private GameObject salmon;

    void MakeInstance()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        MakeInstance();
        LoadObjectTypes();
        CreateGrid();
        CameraPositioner cameraPos = FindObjectOfType<CameraPositioner>();
        if (cameraPos != null)
        {
            cameraPos.RepositionCameraBasedOnIngredients();
        }
    }

    private void LoadObjectTypes()
    {
        bread = Resources.Load<GameObject>("Bread");
        cheeseVariants = new GameObject[]
        {
        Resources.Load<GameObject>("Cheddar"),
        Resources.Load<GameObject>("CheddarHole"),
        Resources.Load<GameObject>("Cheese"),
        Resources.Load<GameObject>("CheeseHole")
        };
        tomato = Resources.Load<GameObject>("Tomato");
        avocado = Resources.Load<GameObject>("Avocado");
        empty = Resources.Load<GameObject>("Empty");
        bacon = Resources.Load<GameObject>("Bacon");
        egg = Resources.Load<GameObject>("Egg");
        ham = Resources.Load<GameObject>("Ham");
        pickle = Resources.Load<GameObject>("Pickle");
        onionRing = Resources.Load<GameObject>("OnionRing");
        lettuce = Resources.Load<GameObject>("Lettuce");
        salmon = Resources.Load<GameObject>("Salmon");
        groundBeef = Resources.Load<GameObject>("GroundBeef");
    }

    private void CreateTile(GridData gridData, TileData tileData, int j, int i)
    {
        var tileType = ChooseObjectByTileStatus(tileData.tileState);

        GameObject tileObj = Instantiate(tileType
            ,new Vector3(START_X + TILE_SIZE_X * i, START_Y, START_Z + TILE_SIZE_Z * j)
            , Quaternion.identity) as GameObject;

        tileObj.AddComponent<TileNodeBridge>();

        var tileNode = new TileNode();
        tileNode.tile = tileData;
        tileNode.sceneObject = tileObj;
        
        if (tileData.tileState == TileData.TileState.EMPTY)
            tileNode.isAvailable = false;

        tileObj.GetComponent<TileNodeBridge>().tileNode = tileNode;

        grid[tileData.row, tileData.column] = tileNode;
    }

    private void CreateGrid()
    {
        rows = DEFAULT_ROW_COUNT;
        columns = DEFAULT_COLUMN_COUNT;

        grid = new TileNode[rows, columns];

        var gridData = LevelManager.instance.GetCurrentLevelData();
        if (gridData != null)
        {
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    var tileData = GetTileData(gridData, j, i);

                    CreateTile(gridData, tileData, j, i);
                }
            }
            CompleteTileNodeInfos();
        }
        else
        {
            Debug.LogError("Levels scriptable object is null or empty");
        }
    }

    private TileData GetTileData(GridData gridData, int row, int column)
    {
        foreach (var tile in gridData.tiles)
        {
            if (tile.row == row && tile.column == column)
            {
                return tile;
            }
        }
        return null;
    }

    private void CompleteTileNodeInfos()
    {
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                var tileNode = grid[x, y];
                tileNode.left = GetLeft(grid, x, y);
                tileNode.right = GetRight(grid, x, y);
                tileNode.up = GetTop(grid, x, y);
                tileNode.down = GetBottom(grid, x, y);
            }
        }
    }

    private TileNode GetLeft(TileNode[,] grid, int row, int column)
    {
        int newColumn = column - 1;
        if (newColumn < 0)
        {
            return null;
        }
        return grid[row, newColumn];
    }

    private TileNode GetRight(TileNode[,] grid, int row, int column)
    {
        int newColumn = column + 1;
        if (newColumn >= DEFAULT_COLUMN_COUNT)
        {
            return null;
        }
        return grid[row, newColumn];
    }

    private TileNode GetTop(TileNode[,] grid, int row, int column)
    {
        int newRow = row + 1;
        if (newRow >= DEFAULT_ROW_COUNT)
        {
            return null;
        }

        return grid[newRow, column];
    }

    private TileNode GetBottom(TileNode[,] grid, int row, int column)
    {
        int newRow = row - 1;
        if (newRow < 0)
        {
            return null;
        }
        return grid[newRow, column];
    }

    private GameObject ChooseObjectByTileStatus(TileData.TileState tileState)
    {
        switch (tileState)
        {
            case TileData.TileState.EMPTY:
                return empty;
            case TileData.TileState.BREAD:
                return bread;
            case TileData.TileState.CHEESE:
                int randomC = Random.Range(0, cheeseVariants.Length);
                return cheeseVariants[randomC];
            case TileData.TileState.AVOCADO:
                return avocado;
            case TileData.TileState.TOMATO:
                return tomato;
            case TileData.TileState.BACON:
                return bacon;
            case TileData.TileState.EGG:
                return egg;
            case TileData.TileState.HAM:
                return ham;
            case TileData.TileState.PICKLE:
                return pickle;
            case TileData.TileState.ONION:
                return onionRing;
            case TileData.TileState.LETTUCE:
                return lettuce;
            case TileData.TileState.SALMON:
                return salmon;
            case TileData.TileState.BEEF:
                return groundBeef;
        }
        return null;
    }
}