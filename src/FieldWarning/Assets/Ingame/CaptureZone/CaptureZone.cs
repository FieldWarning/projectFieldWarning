using PFW.Model.Game;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PFW.Ingame.UI
{
    public class CaptureZone : MonoBehaviour
    {
        public Material red;
        public Material blue;
        public Material neutral;
        //How many Points per tick this Zone gives
        public int Worth = 3;
        //Maybe take out all that owner stuff and simply use an int or otherwise shorten the code
        private PlayerData owner;
        //Vehicles Currently in the Zone (Maybe exclude all non-commander vehicles),maybe replace list with Array
        private List<VehicleBehaviour> _vehicles = new List<VehicleBehaviour>();
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            //Check if Blue Red None or Both Occupy the Zone
            bool redIncluded = false;
            bool blueIncluded = false;
            PlayerData newOwner = null;
            for (int i = 0; i<_vehicles.Count;i++)
            {
                VehicleBehaviour vehicle = _vehicles.ToArray()[i];
                if (vehicle.OrdersComplete())
                {
                    newOwner = vehicle.Platoon.Owner;
                    //Names are USSR and NATO
                    if (newOwner.Team.Name=="USSR")
                    {
                        redIncluded = true;
                    }
                    else
                    {
                        blueIncluded = true;
                    }
                }
            }
            if (redIncluded && blueIncluded ||(!redIncluded && !blueIncluded))
            {
                if (owner != null)
                {
                    changeOwner(null);
                }
            }
            else if (redIncluded)
            {
                if (owner != newOwner)
                {
                    changeOwner(newOwner);
                }
            }
            else
            {
                if (owner != newOwner)
                {
                    changeOwner(newOwner);
                }
            }
        }
        //needs to play sound
        private void changeOwner(PlayerData newOwner)
        {
            if (owner != null)
            {
                owner.IncomeTick -= Worth;
            }
            if (newOwner != null)
            {
                newOwner.IncomeTick += Worth;
            }
            owner = newOwner;
            if (owner != null)
            {
                if (owner.Team.Name == "USSR")
                {
                    this.GetComponent<MeshRenderer>().material = red;
                }
                else
                {
                    this.GetComponent<MeshRenderer>().material = blue;
                }
            }
            else
            {
                this.GetComponent<MeshRenderer>().material = neutral;
            }
           
            
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.parent != null &&other.transform.parent.GetComponent<VehicleBehaviour>() != null && other.transform.parent.GetComponent<VehicleBehaviour>().isActiveAndEnabled)
            {
                _vehicles.Add(other.transform.parent.GetComponent<VehicleBehaviour>());
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.transform.parent.GetComponent<VehicleBehaviour>() != null)
            {
                _vehicles.Remove(other.transform.parent.GetComponent<VehicleBehaviour>());
            }
        }
    }
}
