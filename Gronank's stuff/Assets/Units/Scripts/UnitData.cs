using UnityEngine;
using System.Collections;
using PFW.Weapons;

public class UnitData
{
    //create from xml file or something
    public float movementSpeed 	= 5f;
    public float rotationSpeed 	= 50;
	public float maxHealth 		= 10f;
	public Weapon weapon;


	public static UnitData GenericUnit() //used in Unit Behaviour because both tanks and infantry have 10HP
	{
		var d = new UnitData();
		d.movementSpeed = 5f;
		d.rotationSpeed = 50;
		d.maxHealth = 10f;
		d.weapon = new Weapon ();
		return d;
	}

    public static UnitData Tank()
    {
        var d = new UnitData();
        d.movementSpeed = 5f;
        d.rotationSpeed = 50;
		d.weapon = new Weapon (2000,2,8,1,40); //will use tanks for the damage tests
        return d;
    }
    public static UnitData Infantry()
    {
        var d = new UnitData();
        d.movementSpeed = 3f;
        d.rotationSpeed = 50;
		d.weapon = new Weapon ();
        return d;
    }
}
