# Weapons and first bandit encounter

Open `Assets/Movement/Scenes/ProceduralRailwayPlayground.unity` and press Play. Newly generated worlds create the outfitters and encounter automatically.

## Controls and preparation

E picks up a weapon. M1 fires or swings; R reloads a gun. Tap Q to drop, hold Q to throw. Hands grip the held weapon and recoil/swing with it. Control buttons take priority when hovered, preventing an accidental shot while purchasing supplies. Guns start empty. Reloading reserves ammunition; dropping during reload refunds those reserved rounds.

The train starts with a free club on the right side of the coal wagon. Redfort has a practice range and a Guns & Field Supplies console behind the marketplace; Bluefort and inhabited villages/towns also have outfitters. Press E or M1 on an individual purchase button. Weapons and bottles require empty hands; ammunition goes into crew stock even while holding a gun. Standard credit rules and village markup apply.

| Item | Base Gold | Magazine / use |
| --- | ---: | --- |
| Revolver | 300 | 6 rounds, 24 damage |
| Rifle | 350 | 5 rounds, 45 damage |
| Shotgun | 325 | 2 shells, 6 pellets of 8 damage |
| Club | 40 | 20 damage, 1.8 m reach |
| Hatchet | 75 | 32 damage, 2.1 m reach |
| Ammunition | 50 | 10 shared rounds; a shotgun shot consumes one |
| Medical bottle | 60 | M1 restores 35 Health |
| Recovery tonic | 150 | M1 restores up to 100 Health |

Player Health starts at 100. A medical item is consumed only when it heals. Recovery is a stronger heal in this solo prototype; teammate revival is not implemented. Player weapons cannot damage passengers or other players. Walls, train bodies and friendly colliders block shots. M1 is one attack per click, with separate weapon cooldowns and reload times. Sounds are generated placeholder effects, with no external audio dependency.

## First hostile camp

One main high-risk branch has a three-bandit camp, an approach warning and a physical toll button. The seeded toll is 100–300 Gold. Stop within 70 m of the camp and use its button before entering the barricade area to pay once. Paying prevents aggression unless you attack them. Passing into the camp without paying, or shooting a bandit, makes it hostile. Escape by continuing more than 180 m past the camp, or defeat all three bandits. Routes and the ledger show the encounter's state.

Bandits have 80 Health, shoot every 1.6 seconds for 12 damage, pursue locally, and respect solid cover. Shots blocked by the train cause light component wear and passenger distress; exposed freight can be damaged. Each defeated bandit drops one physical salvage box worth 40 Gold before sale modifiers. The camp warning itself reduces onboard passenger happiness by 8. Weapons held by bandits are visual equipment, not collectible drops in this pass.

When the entire active crew is unconscious, the current run fails and the train stops. The day-end service ledger remains accessible so a fresh run can be started. No friendly fire, multiplayer networking, kidnapping, advanced enemy navigation or mid-journey combat save has been added.

## Validation and previews

PlayMode checks cover ammunition accounting, interruption refunds, damage/cooldowns, solid cover, player/passenger protection, medical consumption, purchase stock, toll payment, one-time salvage and crew defeat. The preview gallery uses actual Unity camera renders of generated weapon models, shops and the encounter. The lineup bench exists only in the capture scene; buy the actual weapons in game. Camera renders do not capture the runtime Health/ammo GUI. These values are provisional for playtesting.
