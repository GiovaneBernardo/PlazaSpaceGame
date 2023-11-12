
using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plaza;
using static Plaza.InternalCalls;
using static Plaza.Input;
using static ChunkManager;

public class ChunkManager : Entity
{
    public int scaleXZ = 20;
    public int sizeXZ = 256;
    public Dictionary<Vector2, Chunk> chunksDictionary = new Dictionary<Vector2, Chunk>();
    public float frequency = 0.025f;
    public float amplitude = 0.07f;
    public float octaves = 8;
    public float persistence = 3;
    static void Shuffle(int[] arrayToShuffle)
    {
        Random random = new Random();
        for (int e = arrayToShuffle.Length - 1; e > 0; e--)
        {
            int index = random.Next(0, e);
            int temp = arrayToShuffle[e];
            arrayToShuffle[e] = arrayToShuffle[index];
            arrayToShuffle[index] = temp;
        }
    }
    static int[] Permutation;
    static int[] MakePermutation()
    {
        int[] permutation = new int[512];
        for (int i = 0; i < 256; i++)
        {
            permutation[i] = i;
        }
        Shuffle(permutation);
        for (int i = 0; i < 256; i++)
        {
            permutation[i + 256] = permutation[i];
        }
        return permutation;
    }

    static Vector2 GetConstantVector(int v)
    {
        int h = v & 3;
        if (h == 0)
            return new Vector2(1.0f, 1.0f);
        else if (h == 1)
            return new Vector2(-1.0f, 1.0f);
        else if (h == 2)
            return new Vector2(-1.0f, -1.0f);
        else
            return new Vector2(1.0f, -1.0f);
    }

    static double Fade(double t)
    {
        return ((6 * t - 15) * t + 10) * t * t * t;
    }

    static double Lerp(double t, double a1, double a2)
    {
        return a1 + t * (a2 - a1);
    }

    static double Noise2D(double x, double y)
    {
        int X = (int)Math.Floor(x) & 255;
        int Y = (int)Math.Floor(y) & 255;

        double xf = x-Math.Floor(x);
        double yf = y-Math.Floor(y);

        Vector2 topRight = new Vector2((float)(xf - 1.0), (float)(yf - 1.0));
        Vector2 topLeft = new Vector2((float)xf, (float)(yf - 1.0));
        Vector2 bottomRight = new Vector2((float)(xf - 1.0), (float)yf);
        Vector2 bottomLeft = new Vector2((float)xf, (float)yf);

        int valueTopRight = Permutation[Permutation[X + 1] + Y + 1];
        int valueTopLeft = Permutation[Permutation[X] + Y + 1];
        int valueBottomRight = Permutation[Permutation[X + 1] + Y];
        int valueBottomLeft = Permutation[Permutation[X] + Y];

        double dotTopRight = Vector2.Dot(topRight, GetConstantVector(valueTopRight));
        double dotTopLeft = Vector2.Dot(topLeft, GetConstantVector(valueTopLeft));
        double dotBottomRight = Vector2.Dot(bottomRight, GetConstantVector(valueBottomRight));
        double dotBottomLeft = Vector2.Dot(bottomLeft, GetConstantVector(valueBottomLeft));

        double u = Fade(xf);
        double v = Fade(yf);

        return Lerp(u,
            Lerp(v, dotBottomLeft, dotTopLeft),
            Lerp(v, dotBottomRight, dotTopRight)
        );
    }

    public class Chunk
    {
        public Entity entity;
        public int x;
        public int z;
        public Vector3 position;
        public Vector3 size;
        public Vector3 scale;
        public Mesh mesh;
    }

    Vector3 scale = new Vector3(20.0f, 12.0f, 20.0f);
    Mesh GenerateTerrainMesh(float xDisplacement, float zDisplacement)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        int depth = 256;
        int width = 256;

        int seed = 235324; // Replace 42 with your desired seed value

        Random random = new Random(seed);
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                //float result = InternalCalls.MeshRenderer_GetHeight(this.Uuid, x, z);
                float n = 0.0f;
                float a = amplitude;
                float f = frequency;
                for(int i = 0; i < octaves; ++i)
                {
                    float v = a * (float)Noise2D((x + xDisplacement) * f, (z + zDisplacement) * f);
                    n += v;
                    a *= 0.5f;
                    f *= 2.0f;
                }
                float result = 255*n;//(float)Noise2D((double)x, (double)z);
                //float result = (float)perlin.OctavePerlin((double)x / width, (double)z / depth, (int)octaves, persistence) * 15.0f;//Noise2d.Noise(x * 0.05f, z * 0.05f);//PerlinNoise(x * 0.05f, z * 0.05f) * 1.5f;
                vertices.Add(new Vector3(x * scale.X, result * scale.Y, z * scale.Z));
            }
        }

        List<int> triangles = new List<int>();
        for (int z = 0; z < depth - 1; z++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                int bottomLeft = z * width + x;
                int topleft = (z + 1) * width + x;
                int topRight = (z + 1) * width + x + 1;
                int bottomRight = z * width + x + 1;

                // First triangle
                triangles.Add(bottomLeft);
                triangles.Add(topleft);
                triangles.Add(topRight);

                // Second triangle
                triangles.Add(bottomLeft);
                triangles.Add(topRight);
                triangles.Add(bottomRight);
            }
        }

        // Create the uvs
        List<Vector2> uvs = new List<Vector2>();
        for (int i = 0; i < vertices.Count; i++)
        {
            float completionPercent = 1 / (vertices.Count - 1);
            uvs.Add(new Vector2(completionPercent, completionPercent));
        }
        Vector3[] normals = new Vector3[vertices.Count];
        for (int i = 0; i < triangles.Count; i += 3)
        {
            int index0 = triangles[i];
            int index1 = triangles[i + 1];
            int index2 = triangles[i + 2];
            Vector3 v1 = vertices[index1] - vertices[index0];
            Vector3 v2 = vertices[index2] - vertices[index0];
            Vector3 normal = Vector3.Cross(v1, v2);
            normal = Vector3.Normalize(normal);
            normals[index0] += normal;
            normals[index1] += normal;
            normals[index2] += normal;
        }

        mesh.Vertices = vertices.ToArray();
        mesh.Indices = triangles.ToArray();
        mesh.Normals = normals.ToArray();
        mesh.Uvs = uvs.ToArray();

        return mesh;
    }

    List<Chunk> chunks = new List<Chunk>();
    Transform playerTransform;
    Vector3 playerLastPosition;

    static float RoundToClosestMultiple(double number, double multiple)
    {
        return (float)(Math.Round(number / multiple) * multiple);
    }

    public void GenerateChunkAt(Vector2 position)
    {
        Entity newChunk = Instantiate(FindEntityByName("EmptyChunk"));
        float finalSize = sizeXZ * scaleXZ;
        newChunk.Name = "Chunk" + (position.X / finalSize + position.Y / finalSize);
        newChunk.GetComponent<MeshRenderer>().mesh = GenerateTerrainMesh(position.X * 255.0f, position.Y * 255.0f);
        newChunk.GetComponent<MeshRenderer>().SetMaterial(0);
        newChunk.GetComponent<Transform>().Translation = new Vector3(255.0f * scale.X * position.X, 0.0f, 255.0f * scale.Z * position.Y);
        newChunk.GetComponent<Collider>().AddShape(ColliderShapeEnum.MESH);

        Chunk chunk = new Chunk();
        chunk.entity = newChunk;
        chunk.x = (int)(position.X);
        chunk.z = (int)(position.Y);
        chunks.Add(chunk);
        chunksDictionary.Add(new Vector2(position.X * finalSize, position.Y * finalSize), chunk);
    }

    public void OnStart()
    {
        Permutation = MakePermutation();
        playerTransform = FindEntityByName("Body").GetComponent<Transform>();
        scale = new Vector3(20.0f, 12.0f, 20.0f);

        for (int x = 1; x < 4; x++)
        {
            for (int z = 1; z < 4; z++)
            {
                Entity newChunk = Instantiate(FindEntityByName("EmptyChunk"));
                newChunk.Name = "Chunk" + (x + z);
                newChunk.GetComponent<MeshRenderer>().mesh = GenerateTerrainMesh(x * 255.0f, z * 255.0f);
                newChunk.GetComponent<MeshRenderer>().SetMaterial(0);
                newChunk.GetComponent<Transform>().Translation = new Vector3(255.0f * scale.X * x, 0.0f, 255.0f * scale.Z * z);
                Chunk chunk = new Chunk();
                chunk.entity = newChunk;
                chunk.x = x;
                chunk.z = z;
                chunks.Add(chunk);
                chunksDictionary.Add(new Vector2(newChunk.GetComponent<Transform>().Translation.X, newChunk.GetComponent<Transform>().Translation.Z), chunk);
            }
        }
        Console.WriteLine("2a");
        /*        Entity newChunk2 = Entity.NewEntity();
                newChunk2.AddComponent<MeshRenderer>().mesh = GenerateTerrainMesh();
                newChunk2.GetComponent<MeshRenderer>().SetMaterial(0);*/
        //newChunk.AddComponent<TerrainScript>();
    }

    public void CheckNearbyChunks()
    {
        foreach (Chunk chunk in chunks)
        {

        }
    }

    public void OnUpdate()
    {
        if (Input.IsKeyDown(KeyCode.H))
        {
            foreach (Chunk chunk in chunks)
            {
                chunk.entity.GetComponent<MeshRenderer>().mesh = GenerateTerrainMesh(255.0f * chunk.x, 255.0f * chunk.z);
                chunk.entity.GetComponent<MeshRenderer>().SetMaterial(0);
            }
        }
        /*        if(Vector3.Distance(playerTransform.Translation, playerLastPosition) > 100.0f)
                {
                    CheckNearbyChunks();
                }*/
    }

    public void OnRestart()
    {

    }
}
