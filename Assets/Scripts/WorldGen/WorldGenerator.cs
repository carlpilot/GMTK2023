using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class WorldGenerator : MonoBehaviour {

    [Header("Map Size")]
    public float mapWidth = 500.0f;
    public int numChunksPerSide = 9;
    public int meshResolution = 32;
    float chunkWidth;

    [Header("Appearance")]
    public Material terrainMaterial;

    [Header("Generation")]
    public float worldSeed = 0.0f;
    public float noiseHeight = 30.0f;
    public Noise noise;
    public float edgeEqualisationRadius = 10.0f;
    public float originFlatRadius = 25.0f;
    float originWorldHeight = 0.0f;

    [Header ("Population")]
    public Spawnable[] spawnables;

    Mesh blankMesh;
    Transform chunkParent;
    int n;

    Dictionary<(int, int), GameObject> chunks = new Dictionary<(int, int), GameObject> ();

    public enum RotationMode { RandomXYZ, RandomY, NoRotation, Normal }

    private void Awake () {
        if (numChunksPerSide / 2.0f == numChunksPerSide / 2) {
            Debug.Log ("Must have an odd number of chunks");
            numChunksPerSide++;
        }
        n = Mathf.FloorToInt (numChunksPerSide / 2f);
        if (worldSeed == 0.0f) worldSeed = Random.Range (-1e6f, 1e6f);
        Random.InitState ((int) worldSeed);
        originWorldHeight = GetWorldHeight (0, 0);
    }

    private void Start () {
        Generate ();
        Populate ();
        CreateNavMesh ();
    }

    void Generate () {
        chunkWidth = mapWidth / numChunksPerSide;
        blankMesh = MeshBuilder.GenerateMesh (meshResolution, chunkWidth);
        chunkParent = new GameObject ("Chunks").transform;

        for (int i = -n; i <= n; i++) {
            for(int j = -n; j <= n; j++) {
                GenerateChunk (i, j);
            }
        }
    }

    void GenerateChunk (int x, int z) {
        GameObject g = new GameObject ("Chunk " + x + ", " + z);
        g.transform.position = new Vector3 (x, 0f, z) * chunkWidth;
        g.isStatic = true;
        g.AddComponent<MeshFilter> ().mesh = TransformMesh (blankMesh, (x, z));
        g.AddComponent<MeshRenderer> ().material = terrainMaterial;
        g.AddComponent<MeshCollider> ().sharedMesh = g.GetComponent<MeshFilter> ().mesh;
        g.transform.parent = chunkParent;
        chunks.Add ((x, z), g);
    }

    void Populate () {
        foreach (Spawnable s in spawnables) {
            int numTreesPlaced = ScatterObjects (s.prefabs, s.numToSpawn, s.rotationMode, s.minSize, s.maxSize);
            print ("Spawned " + numTreesPlaced + " " + s.name);
        }
    }

    public int ScatterObjects (GameObject[] prefabs, int num, RotationMode rotationMode, float minSize, float maxSize) {
        int numAttempts = 0;
        int numPlaced = 0;
        while (numAttempts < num * 4) {
            RaycastHit hit;
            if (Physics.Raycast (new Vector3 (Random.Range (-mapWidth / 2f, mapWidth / 2f), 1000f, Random.Range (-mapWidth / 2f, mapWidth / 2f)), Vector3.down, out hit)) {
                float density = Mathf.Pow (Mathf.PerlinNoise (hit.point.x / 100.0f + num + worldSeed % 1000, hit.point.z / 100.0f - prefabs.Length), 2);
                if (hit.collider.gameObject.name.Contains ("Chunk") && Random.value < density && hit.point.x * hit.point.x + hit.point.z * hit.point.z >= originFlatRadius * originFlatRadius) {
                    Quaternion rotation;
                    if (rotationMode == RotationMode.RandomXYZ) rotation = Random.rotationUniform;
                    else if (rotationMode == RotationMode.RandomY) rotation = Quaternion.Euler (Vector3.up * Random.value * 360.0f);
                    else rotation = Quaternion.identity;
                    GameObject g = Instantiate (prefabs[Random.Range (0, prefabs.Length)], hit.point - Vector3.up * 0.1f, rotation);
                    g.transform.localScale *= Random.Range (minSize, maxSize);
                    g.transform.parent = hit.transform;

                    if (rotationMode == RotationMode.Normal) {
                        g.transform.LookAt (hit.point + hit.normal);
                        g.transform.Rotate (Vector3.right * 90.0f);
                    }

                    numPlaced++;
                    if (numPlaced == num) break;
                }
            }
            numAttempts++;
        }
        return numPlaced;
    }

    void CreateNavMesh () {
        //NavMeshSurface nms = chunks[(0, 0)].AddComponent<NavMeshSurface> ();
        NavMeshSurface nms = new GameObject ("Nav Mesh").AddComponent<NavMeshSurface> ();
        nms.size = Vector3.one * chunkWidth;
        nms.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
        nms.BuildNavMesh ();
    }

    public Mesh TransformMesh (Mesh m, (int, int) chunkNum) {

        Mesh m2 = new Mesh ();
        Vector3[] newVerts = m.vertices;

        for(int i = 0; i < newVerts.Length; i++) {
            float worldX = newVerts[i].x + chunkNum.Item1 * chunkWidth;
            float worldZ = newVerts[i].z + chunkNum.Item2 * chunkWidth;
            newVerts[i] += Vector3.up * GetWorldHeight (worldX, worldZ);
        }

        m2.vertices = newVerts;
        m2.triangles = m.triangles;
        m2.uv = m.uv;

        m2.RecalculateBounds ();
        m2.RecalculateNormals ();
        m2.RecalculateTangents ();
        return m2;
    }

    public float GetWorldHeight (float worldX, float worldZ) {
        //return Mathf.PerlinNoise (worldX / 50.0f + worldSeed, worldZ / 50.0f - worldSeed) * 10.0f;
        float n = (noise.Perlin3D (worldX, worldSeed, worldZ) * 2 - 1.0f) * noiseHeight - originWorldHeight;
        float edgeLimit = (mapWidth / 2f - Mathf.Max (Mathf.Abs (worldX), Mathf.Abs (worldZ))) / edgeEqualisationRadius;
        float sqrDistFromOrigin = worldX * worldX + worldZ * worldZ;
        float originLimit = sqrDistFromOrigin < originFlatRadius * originFlatRadius ? 0 : 0.0003f * (sqrDistFromOrigin - originFlatRadius * originFlatRadius);
        return n * Mathf.Min (1, Mathf.Min (originLimit, edgeLimit));
    }
}

[System.Serializable]
public class Spawnable {
    public string name;
    public GameObject[] prefabs;
    public int numToSpawn;
    public WorldGenerator.RotationMode rotationMode;
    public float minSize;
    public float maxSize;
}
