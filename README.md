# SimpleRPG
This was a side project started to try my hand at TDD, C# Events, and structuring a larger project than I had previously developed for my own use.
I envisioned a turned based RPG, with multiple viable play styles- There was a plan to add a system where different types of actions would please or displease
the deities of the setting- e.g. a player that favored luck-based techniques would unlock abilities in battle that had a wider range of effects, or a magic-based
player may recover MP faster.

## Structure
The driver of player actions were menu systems. Most menus would be given a list of options the player could select- some would instead allow the user to freely type their input-
and would also be able to verify if an input was valid. Once a valid input was received, the Menu would return a MenuSelection. The MenuManager would consume these selections in some way.
This could mean navigating to a sub-menu (such as a player selecting the "attack" option, next they shall have to select the target of their attack).

Because the focus was on allowing different choices with unique effects, eventually the plan was to have various types of enemies that could be dealt with in different ways,
such as the first boss- the Mega Chicken laying different elemental eggs and, if targetted by the corresponding magic, the player would receive a bonus, perhaps a healing effect or a boost to attack.