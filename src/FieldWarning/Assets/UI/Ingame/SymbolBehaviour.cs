/**
 * Copyright (c) 2017-present, PFW Contributors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in
 * compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software distributed under the License is
 * distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See
 * the License for the specific language governing permissions and limitations under the License.
 */

using UnityEngine;

/**
 * Responsible for the NATO symbol on the platoon icons.
 */
public class SymbolBehaviour : MonoBehaviour
{
    [SerializeField]
    private Material[] _materials = null;

    public void SetIcon(UnitType t)
    {
        int i = 0;
        switch (t) {
        case UnitType.Infantry:
            i = 0;
            break;
        case UnitType.Tank:
            i = 1;
            break;
        case UnitType.AFV:
            i = 2;
            break;
        }

        Material mat = _materials[i];
        GetComponent<Renderer>().material = mat;
    }

    public enum UnitType
    {
        Tank,
        Infantry,
        AFV,
        Arty
    }
}
