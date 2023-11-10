
using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plaza;
using static Plaza.InternalCalls;
using static Plaza.Input;

public class TurretScript : Entity
{
    Entity bulletToInstantiate;
    public void OnStart()
    {
        bulletToInstantiate = FindEntityByName("Bullet");
    }
    public void OnUpdate()
    {

    }
    public void OnKeyPressedAndLooking()
    {
        Console.WriteLine("Key Pressed");
        Entity newBullet = Instantiate(bulletToInstantiate);
        newBullet.GetComponent<Transform>().Translation = this.GetComponent<Transform>().Translation + new Vector3(0.0f, 0.0f, 5.0f);
        newBullet.AddComponent<RigidBody>();
    }
}
