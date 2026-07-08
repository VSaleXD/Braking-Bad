using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BrakingBad.Gameplay
{
    public class MazeGenerator : MonoBehaviour
    {
        [Header("Maze Settings")]
        [Tooltip("Width of the maze in cells.")]
        public int mazeWidth = 15;
        [Tooltip("Height of the maze in cells.")]
        public int mazeHeight = 15;
        [Tooltip("Size of the central spawn area in cells.")]
        public int spawnAreaSize = 3;

        [Header("References")]
        public Tilemap wallTilemap;
        public TileBase wallTile;
        public GameObject escapeZonePrefab;

        private int[,] maze; // 0 = path, 1 = wall

        private void Start()
        {
            GenerateMaze();
        }

        public void GenerateMaze()
        {
            if (wallTilemap == null || wallTile == null)
            {
                Debug.LogError("MazeGenerator: Tilemap or WallTile is not assigned.");
                return;
            }

            int gridWidth = mazeWidth * 2 + 1;
            int gridHeight = mazeHeight * 2 + 1;
            maze = new int[gridWidth, gridHeight];

            // Initialize all with walls
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    maze[x, y] = 1;
                }
            }

            // Recursive Backtracker
            Stack<Vector2Int> stack = new Stack<Vector2Int>();
            bool[,] visited = new bool[mazeWidth, mazeHeight];
            
            // Start at a corner or near center
            Vector2Int startCell = new Vector2Int(0, 0);
            visited[startCell.x, startCell.y] = true;
            stack.Push(startCell);
            
            // Path logic: cell(x, y) maps to maze(x*2+1, y*2+1)
            maze[startCell.x * 2 + 1, startCell.y * 2 + 1] = 0;

            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            while (stack.Count > 0)
            {
                Vector2Int current = stack.Pop();
                List<Vector2Int> unvisitedNeighbors = new List<Vector2Int>();

                foreach (var dir in directions)
                {
                    Vector2Int neighbor = current + dir;
                    if (neighbor.x >= 0 && neighbor.x < mazeWidth && neighbor.y >= 0 && neighbor.y < mazeHeight)
                    {
                        if (!visited[neighbor.x, neighbor.y])
                        {
                            unvisitedNeighbors.Add(neighbor);
                        }
                    }
                }

                if (unvisitedNeighbors.Count > 0)
                {
                    stack.Push(current);
                    Vector2Int chosen = unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)];
                    
                    // Remove wall between current and chosen
                    int wallX = current.x * 2 + 1 + (chosen.x - current.x);
                    int wallY = current.y * 2 + 1 + (chosen.y - current.y);
                    maze[wallX, wallY] = 0;
                    
                    // Mark chosen as path
                    maze[chosen.x * 2 + 1, chosen.y * 2 + 1] = 0;
                    
                    visited[chosen.x, chosen.y] = true;
                    stack.Push(chosen);
                }
            }

            // Carve central spawn area
            int centerCellX = mazeWidth / 2;
            int centerCellY = mazeHeight / 2;
            int halfSpawn = spawnAreaSize / 2;

            for (int cx = centerCellX - halfSpawn; cx <= centerCellX + halfSpawn; cx++)
            {
                for (int cy = centerCellY - halfSpawn; cy <= centerCellY + halfSpawn; cy++)
                {
                    if (cx >= 0 && cx < mazeWidth && cy >= 0 && cy < mazeHeight)
                    {
                        int mx = cx * 2 + 1;
                        int my = cy * 2 + 1;
                        maze[mx, my] = 0;
                        // Carve walls around it to make an open area
                        if (mx + 1 < gridWidth) maze[mx + 1, my] = 0;
                        if (mx - 1 >= 0) maze[mx - 1, my] = 0;
                        if (my + 1 < gridHeight) maze[mx, my + 1] = 0;
                        if (my - 1 >= 0) maze[mx, my - 1] = 0;
                        if (mx + 1 < gridWidth && my + 1 < gridHeight) maze[mx + 1, my + 1] = 0;
                        if (mx - 1 >= 0 && my - 1 >= 0) maze[mx - 1, my - 1] = 0;
                        if (mx + 1 < gridWidth && my - 1 >= 0) maze[mx + 1, my - 1] = 0;
                        if (mx - 1 >= 0 && my + 1 < gridHeight) maze[mx - 1, my + 1] = 0;
                    }
                }
            }

            // Create an exit at the top edge (or any edge)
            int exitX = Random.Range(1, mazeWidth) * 2 + 1;
            int exitY = gridHeight - 1; // Top edge
            maze[exitX, exitY] = 0; // Remove outer wall
            maze[exitX, exitY - 1] = 0; // Ensure path connects to exit

            // Render to Tilemap
            wallTilemap.ClearAllTiles();
            Vector3Int offset = new Vector3Int(-gridWidth / 2, -gridHeight / 2, 0);

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (maze[x, y] == 1)
                    {
                        wallTilemap.SetTile(new Vector3Int(x, y, 0) + offset, wallTile);
                    }
                }
            }

            // Place Escape Zone Trigger
            if (escapeZonePrefab != null)
            {
                // Convert grid pos to world pos
                Vector3Int cellPos = new Vector3Int(exitX, exitY, 0) + offset;
                Vector3 worldPos = wallTilemap.GetCellCenterWorld(cellPos);
                Instantiate(escapeZonePrefab, worldPos, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("MazeGenerator: No EscapeZonePrefab assigned. You need to place it manually.");
            }

            // Auto-adjust Camera to fit the maze
            if (Camera.main != null && Camera.main.orthographic)
            {
                // Calculate physical dimensions based on grid scaling
                float scaledWidth = gridWidth * wallTilemap.transform.lossyScale.x;
                float scaledHeight = gridHeight * wallTilemap.transform.lossyScale.y;
                
                // Add a little padding (e.g., 2 units)
                float padding = 2f;
                
                // Orthographic size is half of the vertical height
                float sizeByHeight = (scaledHeight / 2f) + padding;
                // To fit width, we divide by the screen's aspect ratio
                float sizeByWidth = (scaledWidth / 2f / Camera.main.aspect) + padding;

                // Pick the larger size so everything fits
                Camera.main.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);
                
                // Center the camera
                Vector3 camPos = Camera.main.transform.position;
                camPos.x = 0;
                camPos.y = 0;
                Camera.main.transform.position = camPos;
            }
        }
    }
}
