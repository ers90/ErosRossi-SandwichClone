using System;

[Serializable]
public class TileData
{
    public enum TileState{
        EMPTY,
        BREAD,
        TOMATO,
        CHEESE,
        BACON,
        EGG,
        AVOCADO,
        ONION,
        HAM,
        PICKLE,
        LETTUCE,
        SALMON,
        BEEF
    } 
    
    public int row;
    public int column;
    public TileState tileState;
}
