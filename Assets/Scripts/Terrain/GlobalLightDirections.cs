using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum LightDirection { Day, Night }

public class GlobalLightDirections
{

    public static Vector3 dayLightDirection = new Vector3(0.5f, -0.5f, 0.5f);
    public static Vector3 nightLightDirection = new Vector3(0.5f, -0.5f, -0.5f);

    public static Dictionary<LightDirection, Vector3> lightDirections = new Dictionary<LightDirection, Vector3>() {
        {LightDirection.Day, dayLightDirection.normalized},
        {LightDirection.Night, nightLightDirection.normalized}
    };

    public static Vector3 GetLightDirection(LightDirection lightDirection) {
        return lightDirections.GetValueOrDefault(lightDirection);
    }

}
 