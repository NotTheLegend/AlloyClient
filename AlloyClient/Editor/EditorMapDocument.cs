namespace AlloyClient.Editor;

public sealed class EditorMapDocument(int id, EditorMapData map) {
    public readonly int Id = id;
    public readonly EditorController Controller = new(map);
    public readonly EditorMapData Map = map;

    public string JsonPath;
    public string WmapPath;
    public float PanX;
    public float PanY;
    public float Zoom = 100f;
    public bool Grid = true;

}