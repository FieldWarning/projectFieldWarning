List of current formulas implemented into the engine.

Current vision formula
A unit can be spotted if:

1. No building and less than 250m of forest is in the way
AND
2a. The unit is closer than 300m.
OR
2b. MaxSpottingRange of spotter > (distance to target + 500 [if target is in a forest] + length of forest on the way * 4) + (distance to target * 0.3 + 500 [if target is in a forest] + length of forest on the way * 4) * penaltyBonus where penaltyBonus is either (1 + stealth - stealth pen) [if stealth >= pen] or ((stealth + 1)  /  (stealth pen + 1)) [if pen > stealth]