using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class SelectionManager{

    public static void endSelection(this List<PlatoonBehaviour> l)
    {
        l.ForEach(x => x.setSelected(false));
        l.Clear();
    }
    public static void update(this List<PlatoonBehaviour> l)
    {
        l.ForEach(x => x.setSelected(true));
    }
}
