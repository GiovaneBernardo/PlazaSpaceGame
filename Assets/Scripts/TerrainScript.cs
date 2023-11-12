
using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plaza;
using static Plaza.InternalCalls;
using static Plaza.Input;
using System.Reflection;
using System.IO;

public class TerrainScript : Entity
{
    private static int[] perm = Enumerable.Range(0, 256).OrderBy(_ => Guid.NewGuid()).ToArray();

    public static float PerlinNoise(float x, float y)
    {
        try
        {
            int X = (int)Math.Floor(x) & 255;
            int Y = (int)Math.Floor(y) & 255;

            x -= (float)Math.Floor(x);
            y -= (float)Math.Floor(y);

            float u = Fade(x);
            float v = Fade(y);

            int A = perm[X] + Y;
            int B = perm[X + 1] + Y;

            return Lerp(v, Lerp(u, Grad(perm[A], x, y), Grad(perm[B], x - 1, y)),
                               Lerp(u, Grad(perm[A + 1], x, y - 1), Grad(perm[B + 1], x - 1, y - 1)));
        }
        catch (Exception e)
        {
            return 0.0f;
        }

    }

    private static float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private static float Lerp(float t, float a, float b)
    {
        return a + t * (b - a);
    }

    private static float Grad(int hash, float x, float y)
    {
        int h = hash & 15;
        float grad = 1 + (h & 7); // Gradient value 1-8
        if ((h & 8) != 0) grad = -grad; // Randomly invert half of them
        return (h & 1) != 0 ? -grad : grad; // Use gradient value 1 or -1
    }
    Mesh GenerateTerrainMesh()
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        int depth = 256;
        int width = 256;

        int seed = 235324; // Replace 42 with your desired seed value

        Random random = new Random(seed);
        Console.WriteLine("Initial Vertices");
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                int boxX = random.Next(-3, 3);
                float a = 1.3f;
                float k = 0;
                float b = 70 * (float)random.NextDouble();
                float y1 = a * ((float)Math.Sin((x - depth) / b) * (float)random.NextDouble()) + k;

                float a2 = 1.3f;
                float k2 = 0;
                float b2 = 70 * (float)random.NextDouble();
                float y2 = a2 * ((float)Math.Sin((z - width) / b) * (float)random.NextDouble()) + k2;
                //float result = InternalCalls.MeshRenderer_GetHeight(this.Uuid, x, z);
                Vector3 scale = new Vector3(1.0f, 1.0f, 1.0f);
                float result = PerlinNoise(x * 0.05f, z * 0.05f) * 1.5f;
                vertices.Add(new Vector3(x * scale.X, result * scale.Y, z * scale.Z));
            }
        }

        Console.WriteLine("Triangles");
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
        Console.WriteLine("Uvs");
        for (int i = 0; i < vertices.Count; i++)
        {
            float completionPercent = 1 / (vertices.Count - 1);
            uvs.Add(new Vector2(completionPercent, completionPercent));
        }

        Console.WriteLine("Normals");
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
        Console.WriteLine("Normalize");
        for (int i = 0; i < vertices.Count; i++)
        {
            normals[i] = Vector3.Normalize(normals[i]);
        }

        /*        // Initialize normals list with zero vectors
                for (int i = 0; i < vertices.Count; i++)
                {
                    normals.Add(new Vector3(0.0f));
                }

                // Calculate normals for each face
                for (int i = 0; i < triangles.Count / 2; i += 3)
                {
                    int index0 = triangles[i];
                    int index1 = triangles[i + 1];
                    int index2 = triangles[i + 2];

                    Vector3 side1 = vertices[index1] - vertices[index0];
                    Vector3 side2 = vertices[index2] - vertices[index0];
                    Vector3 faceNormal = Vector3.Normalize(Vector3.Cross(side1, side2));
                    // Add the face normal to each vertex's normal
                    normals[index0] += faceNormal;
                    normals[index1] += faceNormal;
                    normals[index2] += faceNormal;
                }

                // Normalize the vertex normals
                for (int i = 0; i < normals.Count; i++)
                {
                    normals[i] = Vector3.Normalize(normals[i]);
                }*/

        Console.WriteLine("To array");
        mesh.Vertices = vertices.ToArray();
        mesh.Indices = triangles.ToArray();
        mesh.Normals = normals.ToArray();
        mesh.Uvs = uvs.ToArray();

        return mesh;
    }
    public void OnStart()
    {
        Console.WriteLine("Changing Mesh");
        Mesh newMesh = GenerateTerrainMesh();
        Console.WriteLine("Changing Mesh 2");
        this.GetComponent<MeshRenderer>().mesh = newMesh;
        //this.GetComponent<Collider>().AddShape(ColliderShapeEnum.MESH);
        Console.WriteLine("Finished");

/*        foreach(Vector3 vertex in newMesh.Vertices)
        {
            Instantiate(FindEntityByName("Cube")).GetComponent<Transform>().Translation = vertex;
        }*/
    }

    public void OnUpdate()
    {

    }

    public void OnRestart()
    {

    }
}
