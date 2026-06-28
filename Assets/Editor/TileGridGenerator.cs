#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class TileGridGenerator : EditorWindow
{
    private GameObject tilePrefab;
    private int gridWidth = 10;
    private int gridHeight = 10;
    private float tileSpacing = 1.1f;
    private Vector2 startPosition = Vector2.zero;

    [MenuItem("Tools/Generate Tile Grid")]
    public static void ShowWindow()
    {
        GetWindow<TileGridGenerator>("Tile Grid Generator");
    }

    private void OnGUI()
    {
        tilePrefab = (GameObject)EditorGUILayout.ObjectField("Tile Prefab", tilePrefab, typeof(GameObject), false);
        gridWidth = EditorGUILayout.IntField("Grid Width", gridWidth);
        gridHeight = EditorGUILayout.IntField("Grid Height", gridHeight);
        tileSpacing = EditorGUILayout.FloatField("Tile Spacing", tileSpacing);
        startPosition = EditorGUILayout.Vector2Field("Start Position", startPosition);

        if (GUILayout.Button("Generate Grid"))
        {
            GenerateGrid();
        }
    }

    private void GenerateGrid()
    {
        if (tilePrefab == null)
        {
            Debug.LogWarning("Tile prefab belum di-assign!");
            return;
        }

        GameObject parent = new GameObject("LavaTileGrid");

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 pos = startPosition + new Vector2(x * tileSpacing, y * tileSpacing);
                GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(tilePrefab);
                tile.transform.position = pos;
                tile.transform.SetParent(parent.transform);
                tile.name = $"LavaTile_{x}_{y}";
            }
        }

        Debug.Log($"Generated {gridWidth * gridHeight} tiles.");
    }
}
#endif