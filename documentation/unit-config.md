Do you want to make units for the game but are afraid you don't have technical/art skills? No worries!

## Basic info

For moddability, we have designed a unit config that allows you to create units that are immediately buyable in the game by just writing a text file.

You can find the config files for all units that are purchasable in the game in the folder src\FieldWarning\Assets\Configuration\Resources\UnitConfigs. To add a new unit, just create a new json file in that folder. Subfolders can also be added and used freely.

To aid reuse, you can write shared configuration files and then have the game automatically use their contents in other units you define. To do this you need to put the shared content in a unit config as usual, but put that config into the UnitConfigTemplates folder instead of UnitConfigs. Any units that will then use that file should refer to it by name in their 'Inherits' field.

## Misc details

The inherits field can contain multiple file names. The files are applied from the first to the last, but only for fields that have not yet been filled. So if your config defines a Price of 50, and you inherit from a file with a price of 100, the final result will be a price of 50 (because that field was filled in the initial config before any inheritance). If you child config does not define a price but inherits from two configs, each with their own price, the price of the first one will be used (at which point the field will be filled and ignored when inheriting from the second config).

Care should be taken with boolean fields such as 'LeavesExplodingWreck'. The code does not have a special case for 'No value' in non-object fields. So in reality, to detect whether a field is filled or not, we just check for a default value. In the boolean case we use the value 'false' - so if any config in your inheritance hierarchy sets a field to 'true', your final unit will use the value 'true'.

Template configs are themselves allowed to inherit. This feature is currently implemented naively and may lead to bugs. If your unit doesn't behave as expected in the game, check if using only non-inheriting template configs fixes it.