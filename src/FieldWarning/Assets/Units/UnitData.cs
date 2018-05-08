using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using PFW.Weapons;

public class UnitData
{
    //create from xml file or something
    public float movementSpeed 	  = 5f;
	public float accelRate        = 1.5f;
    public float rotationSpeed 	  = 50f;
	public float maxHealth 		  = 10f;
	public List<WeaponData> weaponData;
	public float radius           = 0.5f;  // Used for pathfinding and collisions
	public MobilityType mobility = MobilityType.Track;

	public UnitData ()
	{
		weaponData = new List<WeaponData> ();
	}

	public static UnitData GenericUnit() //used in Unit Behaviour because both tanks and infantry have 10HP
	{
		var d = new UnitData();
		d.movementSpeed = 5f;
		d.rotationSpeed = 50;
		d.maxHealth = 10f;
		d.weaponData.Add (new WeaponData ());
		return d;
	}

    public static UnitData Tank()
    {
        var d = new UnitData();
        d.movementSpeed = 5f;
        d.rotationSpeed = 50;
		d.weaponData.Add (new WeaponData (200,2,8,1,30)); //will use tanks for the damage tests
		d.weaponData.Add (new WeaponData (20,0,1.5f,1,40)); // minigun
        return d;
    }
    public static UnitData Infantry()
    {
        var d = new UnitData();
        d.movementSpeed = 3f;
        d.rotationSpeed = 50;
		d.weaponData.Add (new WeaponData ());
        return d;
    }
}

public enum MobilityType
{
	Inf,
	InfAmphib,
	Wheel,
	WheelAmphib,
	Track,
	TrackAmphib
};
