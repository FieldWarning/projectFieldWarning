using UnityEngine;
using System.Collections;

public class UnitData{
    //create from xml file or something
    public float movementSpeed = 5f;
    public float rotationSpeed = 50;
    public static UnitData Tank()
    {
        var d = new UnitData();
        d.movementSpeed = 5f;
        d.rotationSpeed = 50;
        return d;
    }
    public static UnitData Infantry()
    {
        var d = new UnitData();
        d.movementSpeed = 3f;
        d.rotationSpeed = 50;
        return d;
    }
}
