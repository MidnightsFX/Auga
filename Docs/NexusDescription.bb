[center][img]https://raw.githubusercontent.com/MidnightsFX/Auga/main/Docs/Heading1.png[/img][/center]

[size=4][b]Project Auga is a completely re-imagined, modder-friendly UI overhaul for Valheim. Every last piece of UI was considered and reworked from the ground up to create a more helpful and immersive player experience, all while remaining familiar to Valheim veterans.[/b][/size]

[center]by RandyKnapp / n4 / Vapok / MidnightsFX[/center]

[center][img]https://raw.githubusercontent.com/MidnightsFX/Auga/main/Docs/Screenshots/screenshot5.png[/img][/center]

[size=5][b]Updated for Valheim 1.0[/b][/size]

Auga has been rebuilt for Valheim 1.0, including the game's new start game screens, compendium pages and build menu. Highlights since then:

[list]
[*]Every screen works with Valheim 1.0, and the HUD, inventory and tooltips do far less work each frame, so there is less stutter with lots of status effects or items.[/*]
[*]Enemy health bars are slimmer with their level stars just below, and the boss health bar now matches the look of your own health bar.[/*]
[*]Tooltips too tall for the screen, such as Epic Loot items with many effects, scroll instead of flickering.[/*]
[*]Other mods can add their own buttons under your character's name, such as the Clan button from Clans.[/*]
[*]Errors from other mods on Auga's screens are always written to the log, and no longer take a whole screen down.[/*]
[*]A server running Auga decides whether players get the trash button.[/*]
[/list]

The full list is on the Changelogs tab.

[center][img]https://raw.githubusercontent.com/MidnightsFX/Auga/main/Docs/Heading2.png[/img][/center]

[size=4][b]Basically everything:[/b][/size]

[list]
[*][b]New player HUD[/b]: health, stamina and eitr bars, food, status effects, hotbar and minimap, each of which can be moved and scaled.[/*]
[*][b]Redesigned player and container inventories[/b] with full item, food and status effect tooltips.[/*]
[*][b]One consolidated player panel[/b] for your character, skills and PvP.[/*]
[*][b]Improved crafting and upgrade panels[/b] at every crafting station, with mods' tabs alongside.[/*]
[*][b]Expanded character select, new character and start game screens.[/b][/*]
[*][b]Overhauled loading screens, main menu, pause menu and settings.[/b][/*]
[*][b]Auga-style everything else[/b]: the map, chat, compendium, build menu, traders, enemy health bars, damage numbers, signs and popups.[/*]
[/list]

[center][img]https://raw.githubusercontent.com/MidnightsFX/Auga/main/Docs/Screenshots/screenshot16.png[/img][/center]

[spoiler]
[center][img]https://raw.githubusercontent.com/MidnightsFX/Auga/main/Docs/Screenshots/screenshot0.png[/img]

[img]https://raw.githubusercontent.com/MidnightsFX/Auga/main/Docs/Screenshots/clip1.png[/img]

[img]https://raw.githubusercontent.com/MidnightsFX/Auga/main/Docs/Screenshots/screenshot1.png[/img]

[img]https://raw.githubusercontent.com/MidnightsFX/Auga/main/Docs/Screenshots/screenshot8.png[/img]

[img]https://raw.githubusercontent.com/MidnightsFX/Auga/main/Docs/Screenshots/screenshot9.png[/img]

[img]https://raw.githubusercontent.com/MidnightsFX/Auga/main/Docs/Screenshots/screenshot14.png[/img]

[img]https://raw.githubusercontent.com/MidnightsFX/Auga/main/Docs/Screenshots/screenshot15.png[/img][/center]
[/spoiler]

[center][img]https://raw.githubusercontent.com/MidnightsFX/Auga/main/Docs/Heading3.png[/img][/center]

[size=4][b]With a mod manager[/b][/size]

Install Auga from [url=https://thunderstore.io/c/valheim/p/RandyKnapp/Auga/]Thunderstore[/url] with r2modman, Gale or the Thunderstore Mod Manager. It installs Jötunn and Json.NET for you.

[size=4][b]By hand[/b][/size]

[list=1]
[*]Install [url=https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/]BepInEx for Valheim[/url].[/*]
[*]Install [url=https://valheim.thunderstore.io/package/ValheimModding/Jotunn/]Jötunn[/url] and [url=https://valheim.thunderstore.io/package/ValheimModding/JsonDotNET/]Json.NET[/url], which Auga needs.[/*]
[*]Download the main file from the Files tab.[/*]
[*]Put [font=Courier New]Auga.dll[/font] from the zip in [font=Courier New]<Your Valheim folder>\BepInEx\plugins\Auga[/font].[/*]
[/list]

[b]Updating from an older version?[/b] Delete the old [font=Courier New]BepInEx\plugins\Auga[/font] folder first. Leftover files from older versions there are loaded in place of the new ones.

[size=4][b]Multiplayer[/b][/size]

Auga is not required on the server or by other players. When the server runs Auga, it decides for everyone who joins whether the trash button is available.

[size=4][b]Configuration[/b][/size]

Settings are in [font=Courier New]BepInEx\config\randyknapp.mods.auga.cfg[/font], or in game with the Configuration Manager: how the health, stamina and eitr bars and their numbers are shown, where each HUD piece sits and how large it is, where enemy level stars go, and whether to use Auga's chat.

[center][img]https://raw.githubusercontent.com/MidnightsFX/Auga/main/Docs/Heading4.png[/img][/center]

Project Auga changes most of the Valheim UI, so other mods that change the UI may not work with it.

[size=4][b]Works with[/b][/size] (tested on Valheim 1.0)

[list]
[*][b]Epic Loot: [color=#00ff00]YES[/color][/b][/*]
[*][b]Equipment and Quick Slots: [color=#00ff00]YES[/color][/b][/*]
[*][b]Item Favorite Framework: [color=#00ff00]YES[/color][/b][/*]
[*][b]Clans: [color=#00ff00]YES[/color][/b] (its Clan button sits under your character's name)[/*]
[*][b]AAA Build Menu: [color=#00ff00]YES[/color][/b][/*]
[*][b]TargetPortal: [color=#00ff00]YES[/color][/b][/*]
[*][b]PlantEasily: [color=#00ff00]YES[/color][/b][/*]
[*][b]StarLevelSystem: [color=#ffff00]LIMITED[/color][/b] (enemy health bars and stars work; the boss health bar can look off until a StarLevelSystem update)[/*]
[*][b]Controller: [color=#ffff00]LIMITED[/color][/b] (not every menu supports a controller yet)[/*]
[/list]

[size=4][b]Not tested since Valheim 1.0 yet[/b][/size]

Auga has worked with or had support for these before, but they have not been checked on Valheim 1.0: Jewelcrafting, MultiCraft, Chatter, BetterTrader, Recycle N Reclaim, Quick Stack Store, AAA Crafting, Better Archery, Adventure Backpacks, Backpacks, TradersExtended, VNEI and Valheim Plus. Let us know how they behave.

[size=4][b]Does not work with[/b][/size]

[list]
[*][b]SeneaL UI, BetterUI Reforged, BetterUI (Zenox), Minimal UI, AugaLite: [color=#ff0000]NO[/color][/b]. These replace the same screens, so Auga does not load while one of them is installed (the BepInEx log says so).[/*]
[*][b]MyLittleUI, ExtraSlots, AzuExtendedPlayerInventory, Almanac: [color=#ff0000]NO[/color][/b]. These mods do not load while Auga is installed.[/*]
[/list]

[size=4][b]Bugs and mod conflicts[/b][/size]

Please report them on the [url=https://github.com/MidnightsFX/Auga/issues]GitHub Issues page[/url], or drop by the Discord.

[url=https://discord.gg/ZNhYeavv3C][img]https://i.imgur.com/uE6umQE.png[/img][/url] [url=https://github.com/MidnightsFX/Auga][img]https://i.imgur.com/lvbP5OF.png[/img][/url]

Website: [url=https://projectauga.com/]ProjectAuga.com[/url]

[b]AI policy:[/b] we accept AI-assisted changes. All code and assets are reviewed and tested before they are merged or released.

[center][img]https://raw.githubusercontent.com/MidnightsFX/Auga/main/Docs/Heading5.png[/img][/center]

Project Auga comes with an API that lets other mods use its features and build UI in the Auga style: tooltips, panels, buttons, tabs on the player panel and at crafting stations, buttons under the player's name, and restyling a mod's own panels to match. It is open source on GitHub.

[b]Auga API:[/b] [url=https://github.com/MidnightsFX/Auga/blob/main/Docs/AugaAPI.md]https://github.com/MidnightsFX/Auga/blob/main/Docs/AugaAPI.md[/url]
[b]Source:[/b] [url=https://github.com/MidnightsFX/Auga]https://github.com/MidnightsFX/Auga[/url]
