
using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plaza;
using static Plaza.InternalCalls;
using static Plaza.Input;

public class FunctionalBlockScript : Entity
{
    public void Destruct()
    {
        this.Delete();
    }
    public void OnLeftClick()
    {
        Console.WriteLine("Left Clicked");
    }
}
