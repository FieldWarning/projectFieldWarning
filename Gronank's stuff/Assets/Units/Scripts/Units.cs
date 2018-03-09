using UnityEngine;

public static class Units {
    public static GameObject getUnit(UnitType type) {
        switch (type) {
            case UnitType.Tank:
                return Resources.Load<GameObject>("Tank");
            case UnitType.AFV:
                return Resources.Load<GameObject>("AFV");
            case UnitType.Infantry:
                var obj = new GameObject();
                var b=obj.AddComponent<InfantryBehaviour>();
                b.enabled = false;
                return obj;             
        }
        return null;
    }
    public static GameObject tank {
        get {
            return Resources.Load<GameObject>("Unit");
        }
    }
}
public enum UnitType
{
    Tank,
    Infantry,
    AFV
}
