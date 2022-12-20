using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum Content
{
    Empty,
    Unit,
    Obstacle
}
public class HexGridLayout : MonoBehaviour
{
    private int gridRadius = 7;
    private GameObject gridObject;
    private float SQRT_3 = Hex.SQRT_3;

    [Header("Grid Setting")]
    public Vector3 center = new Vector3(0,0,0);
    

    [Header("Tile Setting")]
    public float outerSize = 1f;
    public float innerSize = 0.5f;
    public float height = 0.2f;
    public bool isFlatTop;
    public Material material;

    [HideInInspector]
    private Dictionary<Hex, GameObject> objectMap = new();
    public Dictionary<Hex, Content> obstacleMap = new();
    public List<Hex> spawnpoint = new();


    private void OnEnable()
    {
        LayoutGrid();
    }
    private void OnDisable()
    {
        //foreach(var child in gridObject.transform)
        //{
        //    Destroy((Object)child);
        //}
        Destroy(gridObject);
    }

    private void LayoutGrid()
    {
        gridObject = new GameObject("Grid");
        gridObject.transform.position = center;
        generateCell(new Hex(0, 0));
        for(int radius = 1; radius <= gridRadius; radius++)
        {
            for(int i = 0; i < radius; i++)
            {
                generateCell(new Hex(i, -radius));
                generateCell(new Hex(-i, radius));

                generateCell(new Hex(radius - i, i));
                generateCell(new Hex(-radius + i, -i));

                generateCell(new Hex(radius, i - radius));
                generateCell(new Hex(-radius, radius - i));
            }
        }
        spawnpoint.Add(new Hex(-4, 2));
        spawnpoint.Add(new Hex(-2, 2));
        spawnpoint.Add(new Hex(0, 2));
        spawnpoint.Add(new Hex(2, 2));
        spawnpoint.Add(new Hex(-4, 4));
        spawnpoint.Add(new Hex(-2, 4));
        spawnpoint.Add(new Hex(0, 4));
    }

    private void generateCell(Hex hex)
    {
        GameObject tile = new GameObject($"Hex({hex.q},{hex.r})", typeof(HexRenderer));
        tile.transform.position = GetPositionFromHex(hex);
        tile.transform.parent = gridObject.transform;
        objectMap.Add(hex, tile);
        obstacleMap.Add(hex, Content.Empty);

        HexRenderer hexRenderer = tile.GetComponent<HexRenderer>();
        hexRenderer.isFlapTop = isFlatTop;
        hexRenderer.height = height;
        hexRenderer.outerSize = outerSize;
        hexRenderer.innerSize = innerSize;
        hexRenderer.SetMaterial(material);
        hexRenderer.DrawMesh();
    }

    public void SetColor(Hex hex, Color color)
    {
        if (objectMap.ContainsKey(hex))
        {
            MeshRenderer renderer = objectMap[hex].GetComponent<MeshRenderer>();
            renderer.material.color = color;
        }
    }

    public Vector3 GetPositionFromHex(Hex hex)
    {
        float xPosition;
        float yPosition;
        float size = outerSize;
        if (!isFlatTop)
        {
            xPosition = size * (SQRT_3 * hex.q + SQRT_3 / 2 * hex.r);
            yPosition = size * (3f / 2 * hex.r);
        }
        else
        {
            xPosition = size * (3f / 2 * hex.q);
            yPosition = size * (SQRT_3 / 2 * hex.q + SQRT_3 * hex.r);
        }
        Vector3 result = new Vector3(xPosition, 0, yPosition) + center;
        //result.y += 5;
        //LayerMask mask = LayerMask.GetMask("Board");
        //if (Physics.Raycast(result, Vector3.down, out RaycastHit hit, mask))
        //{
        //    result = hit.point;
        //}
        return result;
    }

    public Hex GetHexFromPosition(Vector3 position)
    {
        float xPosition = position.x - center.x;
        float yPosition = position.z - center.z;
        float size = outerSize;

        float f_q;
        float f_r;
        if (!isFlatTop)
        {
            f_q = (SQRT_3 / 3 * xPosition - 1f / 3 * yPosition) / size;
            f_r = (2f / 3 * yPosition) / size;
        }
        else
        {
            f_q = (2 / 3 * xPosition) / size;
            f_r = (-1f/3 * xPosition + SQRT_3 / 3 * yPosition) / size;
        }
        return Hex.Round(f_q, f_r);
    }

    public HashSet<Hex> GetReachable(Hex origin, int movement)
    {
        return Hex.GetReachable(origin, movement, obstacleMap);
    }

    public List<Hex> PathFinding(Hex start, Hex goal)
    {
        return Hex.PathFinding(start, goal, obstacleMap);
    }
}
