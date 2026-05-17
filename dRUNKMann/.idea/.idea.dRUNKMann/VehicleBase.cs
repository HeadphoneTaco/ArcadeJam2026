using UnityEngine;

public class VehicleBase : MonoBehaviour
{
    protected int wheels = 4;
    protected float speed = 5f;
    protected string color = "White";

    protected void Go()
    {
        Debug.Log("The color is " + color);
        Debug.Log("The speed is " + speed); 
        Debug.Log("The amount of wheels are " + wheels);
    }
    
}
