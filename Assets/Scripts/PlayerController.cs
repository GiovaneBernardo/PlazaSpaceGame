
using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plaza;
using static Plaza.InternalCalls;
using static Plaza.Input;
using System.Globalization;
using System.Diagnostics;

public class PlayerController : Entity
{
    public float speed = 10.0f;
    Transform transform;
    Transform cameraTransform;
    Entity entityToInstantiate;
    private DateTime lastShot = DateTime.Now;
    ChunkManager chunkManager;
    public void OnStart()
    {
        transform = FindEntityByName("Body").GetComponent<Transform>();
        cameraTransform = FindEntityByName("Camera").GetComponent<Transform>();
        entityToInstantiate = FindEntityByName("Sphere");
        chunkManager = FindEntityByName("ChunkManager").GetScript<ChunkManager>();
        //Cursor.Hide();
    }

    public Vector2 lastChunkQuery = new Vector2(100000, 100000);

    static float RoundToClosestMultiple(double number, double multiple)
    {
        return (float)(Math.Round(number / multiple) * multiple);
    }
    int generatedChunks = 0;
    public void CheckNearbyChunks()
    {
        generatedChunks = 0;
        Vector2 closestChunkCoordinate = new Vector2(RoundToClosestMultiple(this.transform.Translation.X, chunkManager.sizeXZ * chunkManager.scaleXZ), RoundToClosestMultiple(this.transform.Translation.Z, chunkManager.sizeXZ * chunkManager.scaleXZ));
        //closestChunkCoordinate.Y += chunkManager.sizeXZ * chunkManager.scaleXZ;
/*        if (!chunkManager.chunksDictionary.ContainsKey(closestChunkCoordinate))
        {
            chunkManager.GenerateChunkAt(new Vector2(closestChunkCoordinate.X / (chunkManager.sizeXZ * chunkManager.scaleXZ), closestChunkCoordinate.Y / (chunkManager.sizeXZ * chunkManager.scaleXZ)));
        }*/

        /* Check if there are more empty chunks on the sides */
        Vector2 distance = new Vector2(chunkManager.sizeXZ * chunkManager.scaleXZ);
        for(int x = -1; x <= 1; x++)
        {
            for(int z = -1; z <= 1; z++)
            {
                CheckChunkAt(closestChunkCoordinate + new Vector2(chunkManager.sizeXZ * chunkManager.scaleXZ * x, chunkManager.sizeXZ * chunkManager.scaleXZ * z));
            }
        }
/*        for(int i = 0; i < 1; ++i)
        {
            CheckChunkAt(closestChunkCoordinate + new Vector2(distance.X, 0.0f));
            CheckChunkAt(closestChunkCoordinate + new Vector2(-distance.X, 0.0f));
            CheckChunkAt(closestChunkCoordinate + new Vector2(0.0f, distance.Y));
            CheckChunkAt(closestChunkCoordinate + new Vector2(0.0f, -distance.Y));
        }*/
        Console.WriteLine("Generated Chunks: " + generatedChunks);
    }

    public void CheckChunkAt(Vector2 position)
    {
        if (!chunkManager.chunksDictionary.ContainsKey(position))
        {
            chunkManager.GenerateChunkAt(new Vector2(position.X / (chunkManager.sizeXZ * chunkManager.scaleXZ), position.Y / (chunkManager.sizeXZ * chunkManager.scaleXZ)));
            generatedChunks++;
        }
    }


    public void OnUpdate()
    {
        float sensitivity = 10.0f;
        if (Input.IsKeyDown(KeyCode.W))
        {
            transform.MoveTowards(new Vector3(0.0f, 0.0f, 1.0f * speed * Time.deltaTime));
        }
        if (Input.IsKeyDown(KeyCode.S))
        {
            transform.MoveTowards(new Vector3(0.0f, 0.0f, -1.0f * speed * Time.deltaTime));
        }
        if (Input.IsKeyDown(KeyCode.A))
        {
            transform.MoveTowards(new Vector3(-1.0f * speed * Time.deltaTime, 0.0f, 0.0f));
        }
        if (Input.IsKeyDown(KeyCode.D))
        {
            transform.MoveTowards(new Vector3(1.0f * speed * Time.deltaTime, 0.0f, 0.0f));
        }
        if (Input.IsKeyDown(KeyCode.Space))
        {
            transform.MoveTowards(new Vector3(0.0f, 1.0f * speed * Time.deltaTime, 0.0f));
        }
        if (Input.IsKeyDown(KeyCode.C))
        {
            transform.MoveTowards(new Vector3(0.0f, -1.0f * speed * Time.deltaTime, 0.0f));
        }

        transform.Rotation += new Vector3(0.0f, (-Input.MouseDeltaX() * sensitivity) * Time.deltaTime, 0.0f);
        cameraTransform.Rotation += new Vector3(0.0f, 0.0f, (-Input.MouseDeltaY() * sensitivity) * Time.deltaTime);

        if (Input.IsMouseDown(1))
        {
            Physics.RaycastHit hit = Physics.Raycast(transform.Translation, Vector3.Normalize(cameraTransform.LeftVector), 1000);
            if (hit.point.X != 0.0f || hit.point.Y != 0.0f || hit.point.Z != 0.0f)
            {
                Entity newEntity = Instantiate(entityToInstantiate);
                newEntity.GetComponent<Transform>().Translation = hit.point;
                newEntity.AddComponent<RigidBody>();
            }
        }
        if (Input.IsMouseDown(0))
        {
            Physics.RaycastHit hit = Physics.Raycast(transform.Translation, Vector3.Normalize(cameraTransform.LeftVector), 1000);
            Console.WriteLine(hit.hitUuid);
            if (hit.hitUuid != 0)
                new Entity(hit.hitUuid).GetScript<FunctionalBlockScript>().Destruct();
        }

        if ((DateTime.Now - lastShot).TotalSeconds > 3 && Input.IsKeyDown(KeyCode.F))
        {
            lastShot = DateTime.Now;
            Console.WriteLine("F");
            Physics.RaycastHit hit = Physics.Raycast(transform.Translation, Vector3.Normalize(cameraTransform.LeftVector), 1000);
            if (hit.hitUuid != 0)
                new Entity(hit.hitUuid).GetScript<TurretScript>().OnKeyPressedAndLooking();
        }

        float chunkSize = chunkManager.sizeXZ * chunkManager.scaleXZ;
        Vector3 currentChunk = new Vector3((float)Math.Round(this.GetComponent<Transform>().Translation.X / chunkSize), 100.0f, (float)Math.Round(this.GetComponent<Transform>().Translation.Z / chunkSize));
        //FindEntityByName("PlaneDebug").GetComponent<Transform>().Translation = new Vector3(currentChunk.X * chunkSize, 100.0f, currentChunk.Z * chunkSize);

        if (Vector2.Distance(new Vector2(this.GetComponent<Transform>().Translation.X, this.GetComponent<Transform>().Translation.Z), lastChunkQuery) > (chunkManager.sizeXZ * chunkManager.scaleXZ / 2))
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            lastChunkQuery = new Vector2(this.GetComponent<Transform>().Translation.X, this.GetComponent<Transform>().Translation.Z);
            CheckNearbyChunks();
            stopWatch.Stop();
            Console.WriteLine("Completed at: " + stopWatch.Elapsed.TotalMilliseconds);
        }
    }

    public void OnRestart()
    {

    }
}
