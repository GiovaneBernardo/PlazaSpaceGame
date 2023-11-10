
using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plaza;
using static Plaza.InternalCalls;
using static Plaza.Input;

public class BulletScript : Entity
{
    public void OnStart()
    {

    }

    public void OnUpdate()
    {
        this.GetComponent<Transform>().Translation += new Vector3(0.0f, 0.0f, 1.0f);
    }

    public void OnCollide(UInt64 collidedUuid, Vector3 vector)
    {
        if (collidedUuid != this.Uuid)
        {
            Entity collidedEntity = new Entity(collidedUuid);
            collidedEntity.Delete();
        }
    }

    public void OnRestart()
    {

    }
}
