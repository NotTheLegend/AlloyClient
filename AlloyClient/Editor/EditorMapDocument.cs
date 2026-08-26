namespace AlloyClient.Editor;

public sealed class EditorMapDocument {
    public readonly int Id;
    public readonly EditorController Controller;
    public readonly EditorMapData Map;

    public string JsonPath;
    public string WmapPath;
    public float PanX;
    public float PanY;
    public float Zoom = 100f;
    public bool Grid = true;

    public EditorMapDocument(int id, EditorMapData map) {
        Id = id;
        Map = map;
        Controller = new EditorController(map);
    }
}
