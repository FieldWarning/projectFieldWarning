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
using System.Collections.Generic;

using PFW.Model.Armory;
using PFW.Model.Game;
using PFW.UI.Ingame;
using PFW.Units.Component.Movement;

namespace PFW.Units
{

    /**
     * When showing previews of move orders or purchases, we need 'ghost' units that
     * are grayed out and not functional. This is where the GhostPlatoon comes in.
     */
    public class GhostPlatoonBehaviour : MonoBehaviour
    {
        public float FinalHeading;

        [SerializeField]
        private IconBehaviour _icon = null;
        private Unit _unit;
        private PlayerData _owner;
        private List<GameObject> _units = new List<GameObject>();

        // Call after creating an object of this class, pretend it is a constructor
        public void Initialize(Unit unit, PlayerData owner)
        {
            _owner = owner;
            _unit = unit;
            transform.position = 100 * Vector3.down;

            InitializeIcon(_icon);
        }

        // Each GhostPlatoon gameobject has an icon under it, spawned in the prefab.
        private void InitializeIcon(IconBehaviour icon)
        {
            _icon.BaseColor = _owner.Team.Color;
            _icon.SetGhost();
        }

        public void AddSingleUnit()
        {
            GameObject _unitPrefab = _unit.Prefab;
            GameObject unit = MatchSession.Current.Factory.MakeGhostUnit(
                    gameObject, _unitPrefab);
            unit.GetComponent<MovementComponent>().InitData(MatchSession.Current.TerrainMap);
            _units.Add(unit);
        }

        public void SetOrientation(Vector3 center, float heading)
        {
            FinalHeading = heading;
            transform.position = center;

            var positions = Formations.GetLineFormation(center, heading, _units.Count);
            for (int i = 0; i < _units.Count; i++) {
                _units[i].GetComponent<MovementComponent>()
                        .SetOriginalOrientation(positions[i], Mathf.PI / 2 - heading);
            }
        }

        public void SetVisible(bool vis)
        {
            _icon.SetVisible(vis);
            _units.ForEach(x => x.GetComponent<MovementComponent>().SetVisible(vis));

            // FIXME: It looks like UnitLabelAttacher looks for a GameObject ("UIWrapper") that
            //      no longer exists in the scene. Is this deprecated? Should it be removed?
            // _units.ForEach(x => x.GetComponent<UnitLabelAttacher>().SetVisibility(vis));
        }

        public void Destroy()
        {
            foreach (var u in _units)
                Destroy(u);

            Destroy(gameObject);
        }

        public void RemoveOneGhostUnit()
        {
            GameObject u = _units[0];
            _units.Remove(u);
            Destroy(u);
            if (_units.Count == 0)
                Destroy();
        }
    }
}
