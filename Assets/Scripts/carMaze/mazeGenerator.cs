// MazeGenerator.cs
using System.Collections.Generic;
using UnityEngine;

namespace BrakingBad.Gameplay
{
    public class mazeGenerator : MonoBehaviour
    {
        [SerializeField] private int width = 15;
        [SerializeField] private int height = 15;
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private float cellSize = 2f;

        private bool[,] visited;
        private Dictionary<Vector2Int, bool[]> walls = new();

        public Vector2Int StartCell { get; private set; }
        public Vector2Int ExitCell { get; private set; }

        public void Generate(int seed)
        {
            Random.InitState(seed);
            visited = new bool[width, height];
            walls.Clear();

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    walls[new Vector2Int(x, y)] = new bool[] { true, true, true, true };

            StartCell = new Vector2Int(0, 0);
            Carve(StartCell);

            ExitCell = FindFarthestCell(StartCell); 
            BuildWallGeometry();
        }

        private void Carve(Vector2Int cell)
        {
            visited[cell.x, cell.y] = true;
            var dirs = new List<int> { 0, 1, 2, 3 };
            Shuffle(dirs);

            foreach (int dir in dirs)
            {
                Vector2Int next = cell + DirOffset(dir);
                if (!InBounds(next) || visited[next.x, next.y]) continue;

                walls[cell][dir] = false;
                walls[next][(dir + 2) % 4] = false; 
                Carve(next);
            }
        }

        public Dictionary<Vector2Int, int> BFSDistances(Vector2Int from)
        {
            var dist = new Dictionary<Vector2Int, int> { [from] = 0 };
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(from);

            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                for (int dir = 0; dir < 4; dir++)
                {
                    if (walls[cur][dir]) continue;
                    var next = cur + DirOffset(dir);
                    if (!dist.ContainsKey(next))
                    {
                        dist[next] = dist[cur] + 1;
                        queue.Enqueue(next);
                    }
                }
            }
            return dist;
        }

        private Vector2Int FindFarthestCell(Vector2Int from)
        {
            var dist = BFSDistances(from);
            Vector2Int farthest = from;
            int max = -1;
            foreach (var kv in dist)
                if (kv.Value > max) { max = kv.Value; farthest = kv.Key; }
            return farthest;
        }

        private Vector2Int DirOffset(int dir) => dir switch
        {
            0 => Vector2Int.up, 1 => Vector2Int.right,
            2 => Vector2Int.down, 3 => Vector2Int.left, _ => Vector2Int.zero
        };

        private bool InBounds(Vector2Int c) => c.x >= 0 && c.y >= 0 && c.x < width && c.y < height;

        private void Shuffle(List<int> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private void BuildWallGeometry()
        {
            foreach (var kv in walls)
            {
                Vector2Int cell = kv.Key;
                bool[] cellWalls = kv.Value;

                for (int dir = 0; dir < 4; dir++)
                {
                    if (!cellWalls[dir]) continue;

                    Vector3 pos = new Vector3(cell.x * cellSize, cell.y * cellSize, 0);
                    Quaternion rot = Quaternion.Euler(0, 0, dir * 90);
                    Instantiate(wallPrefab, pos, rot, transform);
                }
            }
            
        }
    }
}