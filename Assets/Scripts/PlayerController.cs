
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

public class PlayerController : Entity
{
    Transform transform;
    Transform cameraTransform;
    Entity entityToInstantiate;
    private DateTime lastShot = DateTime.Now;
    public void OnStart()
    {
        transform = FindEntityByName("Body").GetComponent<Transform>();
        cameraTransform = FindEntityByName("Camera").GetComponent<Transform>();
        entityToInstantiate = FindEntityByName("Sphere");
        Cursor.Hide();
    }



    public void OnUpdate()
    {
        float speed = 10.0f;
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
    }

    public void OnRestart()
    {

    }
}