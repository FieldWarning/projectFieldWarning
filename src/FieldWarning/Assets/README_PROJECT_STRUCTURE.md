General rule: assets (including scripts) should only be used by entities placed in the same directory or a subdirectory from where the asset is declared. Any asset that is not used by files in its directory, including subdirectories, must be safe to delete.

The other side of this rule is: It is NOT allowed to use any asset if you have to go down the directory tree to do it. For example, Units/Tanks/ExampleTank.prefab is allowed to use assets from Units/Tanks/ and from Units/, but NOT from Units/Tanks/US/ or from Units/Textures (because those paths require going down into the US and Textures folders respectively).

This rule guarantees that if we are wondering about the purpose of a file in Units/Tanks/US, we only need to inspect that directory to know what it is used for.