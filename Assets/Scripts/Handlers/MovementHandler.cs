using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementHandler : MonoBehaviour
{
    List<Vector2> vector2BFS(Vector2 start, Vector2 end, double minHeight)
    {
        float curHeight = SelectedGameObject.transform.position.z;
        List<Vector2> output = new List<Vector2>();
        if (start == end) return output;
        List<Vector2> queue = new List<Vector2>();
        HashSet<Vector2> visited = new HashSet<Vector2>();
        Dictionary<Vector2, int> distances = new Dictionary<Vector2, int>();
        Dictionary<Vector2, Vector2> predecessor = new Dictionary<Vector2, Vector2>();
        queue.Add(start);
        distances[start] = 0;
        visited.Add(start);
        Vector2 closestVector2 = new Vector2(float.MaxValue, float.MaxValue);
        int numberChecked = 0;
        while (queue.Count > 0)
        {
            numberChecked += 1;
            if (numberChecked > 1000)
            {
                break;
            }
            Vector2 curVector2 = queue[0];
            if (Vector2.Distance(curVector2, end) < Vector2.Distance(closestVector2, end))
            {
                closestVector2 = curVector2;
            }
            queue.RemoveAt(0);
            var curTile = ScriptHandler.GetComponent<TerrainHandler>().GetTileAtPosition(curVector2);
            foreach (Vector2 curAdjacentVector2 in getAdjacentPoints(curVector2))
            {
                /*if(curAdjacentVector2 == end)
                {
                    Debug.Log(Math.Abs(ScriptHandler.GetComponent<GenerateMap>().map[curAdjacentVector2].height - ScriptHandler.GetComponent<GenerateMap>().map[curVector2].height));
                }*/
                var adjacentTile = ScriptHandler.GetComponent<TerrainHandler>().GetTileAtPosition(curAdjacentVector2);
                if ((!visited.Contains(curAdjacentVector2)) && adjacentTile != null
                    && !adjacentTile.isOccupied
                    && Math.Abs(adjacentTile.height - curTile.height) <= minHeight)
                {
                    visited.Add(curAdjacentVector2);
                    distances[curAdjacentVector2] = distances[curVector2] + 1;

                    predecessor[curAdjacentVector2] = curVector2;
                    queue.Add(curAdjacentVector2);
                    if (curAdjacentVector2.Equals(end))
                    {
                        Vector2 travelBack = curAdjacentVector2;
                        while (predecessor.ContainsKey(travelBack))
                        {
                            output.Add(travelBack);
                            travelBack = predecessor[travelBack];
                        }
                        output.Reverse();
                        return output;
                    }
                }
            }

        }
        Debug.Log(numberChecked);
        while (predecessor.ContainsKey(closestVector2))
        {
            output.Add(closestVector2);
            closestVector2 = predecessor[closestVector2];
        }
        output.Reverse();
        return output;
    }

    void SetSelectedTiles()
    {
        foreach (var currentSelectedTile in SelectedTiles)
        {
            Destroy(currentSelectedTile);
        }
        SelectedTiles = new List<GameObject>();
        foreach (var move in movesQueue)
        {
            GameObject currentSelectedTile = Instantiate(SelectedTiles);
            float newHeight = ScriptHandler.GetComponent<TerrainHandler>().GetTileAtPosition(new Vector2(move.x, move.y))?.height ?? 0.0f;
            currentSelectedTile.transform.position = new Vector3(move.x, move.y, newHeight - .6f);
            currentSelectedTile.SetActive(true);
            SelectedTiles.Add(currentSelectedTile);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public List<Vector2> MovesQueue;
    public GameObject SelectedGameObject;
    public bool LockInput = false;
    public bool RightClickHeld;
    public List<GameObject> SelectedTiles;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1) && !LockInput)
        {
            RightClickHeld = true;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Vector2 previous = new Vector2(SelectedGameObject.transform.position.x, SelectedGameObject.transform.position.y);
            if (Physics.Raycast(ray, out hit, 100))
            {

                MovesQueue = (vector2BFS(new Vector2(SelectedGameObject.transform.position.x, SelectedGameObject.transform.position.y),
                    new Vector2(hit.transform.gameObject.transform.position.x, hit.transform.gameObject.transform.position.y), 100.51));
                SetSelectedTiles();

            }
        }
    }
}
