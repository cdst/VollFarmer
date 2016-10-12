#VollFarmer

##About
Upon entering The Dried Lake, combatrange is set to 0 and it's left up to OldGrindBot to explore the area. Once Voll is found, combatrange is increased to 50 and Voll is killed, looted then the bot returns to town to start a new run.

A run takes anywhere from 30s to 3min, depending on how quick Voll is found.

Your character should be tanky and strong enough to survive Dried Lake without dying and killing Voll in a short amount of time. The bot will not engage in combat unless Voll is nearby, you will get hit, you will take damage, you might die - make sure your character can handle it.
This plugin is released as-is. It works for me, it might not work for you. If it doesn't work for you then the source is there for you to make it better.

This Plugin was only tested with the OldRoutine combat routine. It will probably not work with any of the other community routines out there.

##Setting things up

Required Plugins:
- EXtensions (comes bundled with Exilebuddy now)
- CommunityLib

Recommended Plugins:
- Alcor75PlayerMover - Make sure to check Ignore Mobs in Alcor75PlayerMover settings
- AdvancedItemFilter

Copy VollFarmer-0.0.0.X.zip to 3rdParty

Create an empty 3rdparty-required.txt in the following plugin:
3rdParty\OldRoutine

Start ExileBuddy
Settings > Plugins > VollFarmer > Enable

Settings > Bots > OldGrindBot > GlobalChestsToIgnore > Check All

Settings > Bots > OldGrindBot > TakeCorruptedAreas > Uncheck

Settings > Plugins > Alcor75PlayerMover > Ignore Mobs > Check

