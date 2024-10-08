using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridPlaneMeshGenerator : MonoBehaviour
/// <summary>
{

    [Header("Terrain Generation")]
    [SerializeField] private float scale = 1;
    [SerializeField] private float height = 1;

    [Tooltip("The rate at which the frequency of the Perlin noise increases")]
    [SerializeField] private float lacunarity = 2.0f;
    [Tooltip("The rate at which the amplitude of the Perlin noise decreases")]
    [SerializeField] private float persistence = 0.5f;
    [Tooltip("The number of octaves of Perlin noise to apply")]
    [SerializeField] private int octaves = 2;

    [Tooltip("Controls the altidude of the terrain colour transitions")]
    [SerializeField] private float textureElevation = 1.0f;

    [Header("Terrain Curves")]
    [Tooltip("Applies a modifier to the terrain based on its height")]
    [SerializeField] private AnimationCurve heightCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [Tooltip("Applies a modifier to the terrain altitude based on its distance from the centre")]
    [SerializeField] private AnimationCurve islandCurve = AnimationCurve.Linear(0, 0, 1, 1);


    [Header("Performance")]
    [Tooltip("The number of vertices in the x and z directions (total vertices = (vertexResolution + 1)^2)")]
    [SerializeField] private int vertexResolution = 10;
    [Tooltip("The number of lines of vertices to generate per frame. Total vertices per frame will be vertexResolution*generatePerFrame")]
    [SerializeField] private int generatePerFrame = 0;

    private MeshFilter meshFilter;
    private Coroutine generateCoroutine;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        GenerateGridPlaneMesh();

    }

    void Update()
    {
        if (generateCoroutine == null)
        {
            generateCoroutine = StartCoroutine(nameof(GenerateGridPlaneMesh));
        }
    }


    /// <summary>
    /// Generates a grid plane mesh with the specified vertex resolution and height. Built as a coroutine 
    /// so its operations can be split up across multiples frames for performance.
    /// </summary>
    /// <returns></returns>
    private IEnumerator GenerateGridPlaneMesh()
    {
        GetComponent<MeshRenderer>().material.SetFloat(Shader.PropertyToID("_HeightMin"), 0);
        GetComponent<MeshRenderer>().material.SetFloat(Shader.PropertyToID("_HeightMax"), height*transform.localScale.y*textureElevation);
        Mesh mesh = new Mesh();
        mesh.name = "Generated Grid Plane";
        mesh.Clear();

        int vertexCount = (vertexResolution + 1) * (vertexResolution + 1);
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int[] triangles = new int[vertexResolution * vertexResolution * 6];

        float squareSize = 1f / vertexResolution;
        float minAltitude = Mathf.Infinity;
        float maxAltitude = Mathf.NegativeInfinity;

        // Generate vertices, UVs, and track min/max altitude for coloring
        int vertIndex = 0;
        for (int i = 0; i <= vertexResolution; i++)
        {
            for (int j = 0; j <= vertexResolution; j++)
            {
                float noiseTotal = 0;
                for (int octave = 0; octave < octaves; octave++)
                {
                    float frequency = Mathf.Pow(lacunarity, octave);
                    float amplitude = Mathf.Pow(persistence, octave);

                    float noise = Mathf.PerlinNoise(100000+(j-vertexResolution/2) * squareSize * frequency/scale, 100000+(i-vertexResolution/2) * squareSize * frequency/scale) * amplitude;
                    noiseTotal += heightCurve.Evaluate(noise);
                }
                float distFromCentre = Mathf.Sqrt(Mathf.Pow(j - vertexResolution / 2, 2) + Mathf.Pow(i - vertexResolution / 2, 2));
                noiseTotal = noiseTotal * islandCurve.Evaluate(distFromCentre/(vertexResolution/2));

                vertices[vertIndex] = new Vector3(j * squareSize - 0.5f, scale*height*noiseTotal, i * squareSize - 0.5f);
                uvs[vertIndex] = new Vector2((float)j / vertexResolution, (float)i / vertexResolution);

                // Track min and max altitude
                minAltitude = Mathf.Min(minAltitude, noiseTotal);
                maxAltitude = Mathf.Max(maxAltitude, noiseTotal);

                vertIndex++;
            }
            if (i % generatePerFrame == 0) {
                yield return null;
            }
        }

        // Generate triangles
        int triIndex = 0;
        for (int i = 0; i < vertexResolution; i++)
        {
            for (int j = 0; j < vertexResolution; j++)
            {
                int topLeft = i * (vertexResolution + 1) + j;
                int topRight = topLeft + 1;
                int bottomLeft = topLeft + (vertexResolution + 1);
                int bottomRight = bottomLeft + 1;

                // First triangle
                triangles[triIndex++] = topLeft;
                triangles[triIndex++] = bottomLeft;
                triangles[triIndex++] = topRight;

                // Second triangle
                triangles[triIndex++] = topRight;
                triangles[triIndex++] = bottomLeft;
                triangles[triIndex++] = bottomRight;
            }
        }

        // Assign data to mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.colors = Enumerable.Repeat(Color.white, mesh.vertexCount).ToArray();

        // Recalculate normals for lighting
        mesh.RecalculateNormals();

        // Assign mesh to the MeshFilter component
        meshFilter.mesh = mesh;
        yield return null;
        generateCoroutine = null;
    }
}