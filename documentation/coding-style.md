For non code files (xml, etc), our current best guidance is consistency. When editing files, keep new code and changes consistent with the style in the files. For new files, it should conform to the style for that component. If there is a completely new component, anything that is reasonably broadly accepted is fine.


C# Coding Style
===============

The best introduction to our coding style is with a (truncated) example:

``Ingame/UI/InputManager.cs``

```
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

using UnityEngine.EventSystems;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

using PFW.Ingame.Prototype;
using PFW.Model.Game;

namespace PFW.Ingame.UI
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField]
        private Texture2D FIRE_POS_TEXTURE;

        private List<SpawnPointBehaviour> _spawnPointList = new List<SpawnPointBehaviour>();
        private ClickManager _rightClickManager;

        public enum MouseMode { normal, purchasing, firePos };
        public MouseMode CurMouseMode { get; private set; } = MouseMode.normal;

        private MatchSession _session;
        public MatchSession Session {
            get {
                return _session;
            }

            set {
                if (_session == null)
                    _session = value;
            }
        }

        private Player _localPlayer {
            get {
                return Session.LocalPlayer;
            }
        }

        void Update()
        {
            switch (CurMouseMode) {

            case MouseMode.purchasing:

                RaycastHit hit;
                if (Util.GetTerrainClickLocation(out hit)
                    && hit.transform.gameObject.name.Equals("Terrain")) {

                    ShowGhostUnitsAndMaybePurchase(hit);
                }

                MaybeExitPurchasingMode();
                break;

            case MouseMode.normal:
                ApplyHotkeys();
                _rightClickManager.Update();
                break;

            case MouseMode.firePos:
                ApplyHotkeys();

                if (Input.GetMouseButtonDown(0))
                    Session.SelectionManager.DispatchFirePosCommand();

                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                    ExitFirePosMode();

                break;

            default:
                throw new Exception("impossible state");
            }
        }
        [..]
}

```

0. Every PFW-created file starts with the APv2 license disclaimer. 

1. Every PFW-created file is in the top-level `PFW` namespace.

2. We use [K&R style](https://en.wikipedia.org/wiki/Indentation_style#K&R) braces, where opening braces are on the same line except for methods and types. A single line statement block can go without braces but the block must be properly indented on its own line.

The purpose of K&R braces is to avoid "stretched" code where every expression is surrounded by empty lines, diluting the signaling value of empty lines. The purpose of K&R braces is not to save space by never separating code blocks. You are strongly encouraged to add an empty line between any two lines, and especially after any braces, if you find it improves readability (for an extreme case, note the empty lines around the first case of the switch above).

3. We use four spaces of indentation (no tabs).

4. We use `_camelCase` for internal and private fields and use `readonly` where possible. Prefix internal and private instance fields with `_`, static fields with `s_` and thread static fields with `t_`. When used on static fields, `readonly` should come after `static` (e.g. `static readonly` not `readonly static`).  Public fields use PascalCasing, methods use PascalCasing regardless of protection level.

5. We try very hard to keep as many fields and methods private as possible, and to avoid the use of `static`. We always specify the visibility, even if it's the default (e.g. `private string _foo` not `string _foo`). Visibility should be the first modifier (e.g. `public abstract` not `abstract public`). If you are only making a variable public so you can manipulate it in the Unity explorer, make it private with the `[SerializeField]` annotation instead.

6. We use ALL_CAPS to name all our constant local variables and fields. Unity generally makes it hard to use constants because they can't be exposed in the editor, so you can also use ALL_CAPS to refer to "semantic constants" - variables that lack the `readonly` keyword because they have to be visible in editor, but must not be changed at runtime. Setting the value of a variable named like this is, naturally, not allowed.

7. For non-const fields, we still try to limit their mutability as much as possible. We try to have fields without a setter and with a private getter. If that is not possible, we try to make setter only usable once (see the Session field in the example above).

8. We do not use setter and getter methods, instead we always use C# properties. There is only one case where the use of a getter/setter method is warranted, which is to signify that the operation has a side effect or that it is slow and should be cached by the caller. Even in those cases, there are better names (`CalculateDistance` instead of `GetDistance` if we are trying to show slowness for example).

9. We try to keep methods short and focused on doing one thing. A block of 3-4 expressions can usually be improved by a comment saying what that block is trying to achieve. A commented block of 3-4 expressions can be replaced by a method, where the method name contains what would have been the comment ("`ShowGhostUnitsAndMaybePurchase()`"). 

10. `this.` can make method invocations more expressive, but using it for field access is almost always wrong and hints at a naming issue.

11. Namespace imports should be specified at the top of the file, below the license disclaimer, *outside* of `namespace` declarations, and should be sorted alphabetically. PFW imports should be separated from engine imports by an empty line.
   
12. Avoid more than one empty line at any time. Two blank lines can rarely be used for emphasis, but the need for them hints that the code is doing too much and needs to be refactored.

13. Avoid spurious free spaces.
   For example avoid `if (someVar == 0)...`, where the dots mark the spurious free spaces.
   Consider enabling "View White Space (Ctrl+E, S)" if using Visual Studio to aid detection.
   Extra free space may however be used for alignment, e.g.:   
```C#
firstUnit  = NewUnit();
secondUnit = NewUnit();
```

14. If a file happens to differ in style from these guidelines (e.g. private members are named `m_member`
   rather than `_member`), the existing style in that file takes precedence.
   
15. We only use `var` when it's obvious what the variable type is (e.g. `var stream = new FileStream(...)` not `var stream = OpenStandardInput()`).

16. Fields should be specified at the top within type declarations.

17. Case labels are not indented. 

18. We seek to fail fast and do not program defensively. A method should not check its inputs for `null`. Instead it should directly use them and throw a `NullException` if it was provided faulty values. It is the responsibility of the caller to handle `null` values as soon as they occur - trying to recover from a bad input only makes it harder to find the source of the problem.

19. We prefer composition over inheritance.


We have provided a Visual Studio 2013 vssettings file (`coding-style.vssettings`) at the root of the repository, enabling C# auto-formatting conforming to the above guidelines. Ctrl+k Ctrl+d.


// TODO some sort of automatic formatting tool, i dont think codeformatter does the job since its not customizable enough
