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

using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;

using PFW.Model.Game;

public class VisibleBehavior //: IComponentData
{
    [SerializeField]
    private float max_spot_range = 800f;
    [SerializeField]
    private float stealth_pen_factor = 1f;
    [SerializeField]
    private float stealth_factor = 10f;

    private HashSet<VisibleBehavior> _spotters = new HashSet<VisibleBehavior>();
    private bool _isVisible = true;

    public UnitBehaviour UnitBehaviour;
    private GameObject _gameObject;

    private Team _team {
        get { return UnitBehaviour.Platoon.Owner.Team; }
    }

    private MatchSession _session {
        get { return UnitBehaviour.Platoon.Owner.Session; }
    }

    private VisibleBehavior() { }

    public VisibleBehavior(GameObject unit, UnitBehaviour unitBehaviour)
    {
        _gameObject = unit;
        UnitBehaviour = unitBehaviour;
    }

    // Alert all nearby enemy units that they may have to show themselves.
    // Only works if they have colliders!
    public void ScanForEnemies()
    {
        Collider[] hits = Physics.OverlapSphere(_gameObject.transform.position, max_spot_range, LayerMask.NameToLayer("Selectable"), QueryTriggerInteraction.Ignore);

        foreach (Collider c in hits) {
            GameObject go = c.gameObject;

            // this finds colliders, health bars and all other crap except units
            var unitBehaviour = go.GetComponentInParent<UnitBehaviour>();
            if (unitBehaviour == null || !unitBehaviour.enabled)
                continue;

            /* This assumes that all selectables with colliders have a visibility manager, which may be a bad assumption: */
            if (unitBehaviour.Platoon.Owner.Team != _team)
                unitBehaviour.VisibleBehavior.MaybeReveal(this);
        }
    }

    // Check if there are any enemies that can detect this unit 
    // and make it invisible if not.
    public void MaybeHideFromEnemies()
    {
        if (!_isVisible)
            return;

        _spotters.RemoveWhere(s => s == null || !s.CanDetect(this));
        if (_spotters.Count == 0)
            ToggleUnitVisibility(false);
    }

    // It is the responsibility of the defender to 
    // reveal themselves if necessary. This is done here.
    private void MaybeReveal(VisibleBehavior spotter)
    {
        if (spotter.CanDetect(this)) {
            if (_spotters.Count == 0) {
                ToggleUnitVisibility(true);
            }

            _spotters.Add(spotter);
        }
    }

    private bool CanDetect(VisibleBehavior target)
    {
        float distance = Vector3.Distance(_gameObject.transform.position, target._gameObject.transform.position);
        return distance < max_spot_range && distance < max_spot_range * stealth_pen_factor / target.stealth_factor;
    }

    public void ToggleUnitVisibility(bool revealUnit)
    {
        _isVisible = revealUnit;
        ToggleAllRenderers(_gameObject, revealUnit);
        MaybeTogglePlatoonVisibility(revealUnit);
    }

    private void MaybeTogglePlatoonVisibility(bool unitRevealed)
    {
        PlatoonBehaviour platoon = UnitBehaviour.Platoon;
        ToggleAllRenderers(platoon.gameObject,
            !platoon.Units.TrueForAll(
                u => !u.VisibleBehavior._isVisible));
    }

    private void ToggleAllRenderers(GameObject o, bool enable)
    {
        var allRenderers = o.GetComponentsInChildren<Renderer>();
        foreach (var childRenderer in allRenderers)
            childRenderer.enabled = enable;
    }
}
