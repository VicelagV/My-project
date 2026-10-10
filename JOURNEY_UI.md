# Journey UI and vendor trading

In `Assets/Movement/Scenes/ProceduralRailwayPlayground.unity`, press Play. Visit Redfort’s marketplace or the outfitter, look at the colourful shopkeeper, and press E. Buy buttons show prices, affordability and feedback. Esc closes the menu; Tab opens the railway ledger.

## Changes

- Native Unity UI Toolkit HUD, vendor menus, journey/route page, passenger manifest, freight contracts, supplies, accounts and workshop pages.
- Shopkeeper characters replace world purchase buttons as the shop interaction. Old purchase services remain the transaction backend; their physical buttons and colliders are hidden.
- Purchases no longer require empty hands. Tools, coal, weapons and bottles appear at a collection tray. One uncollected physical item per tray prevents overlapping piles. Steel, repair parts and ammunition go directly into their existing ledgers.
- Selling salvage is an explicit menu action. Opening a vendor or buying another item never consumes held salvage.
- Item-specific HUD hints cover medical use, coal fuelling, freight condition, repair tools, weapons, drops, throws and attachment. Repair and throw progress remain visible.
- Menus release the cursor and suppress player movement, aiming and firing. The close click cannot also fire a held weapon. The journey simulation continues while browsing.
- The day-end screen shows starting funds, income, purchases, entry, other cash spending, credit, obligations, the quota charged, repairs and saved progression. Successful runs can prepare the next day; failed runs can restart.
- A broken speedometer also hides the HUD speed readout. The accounts page keeps passenger/cargo/repair information available.

The existing fare, steam, repair and combat rules are retained. Bandit encounters already occupy high-risk routes. No friendly fire remains enabled.

## Validation

Unity 6000.6.4f1: all 165 PlayMode tests passed, including 26 focused UI and day-accounting checks. The suite includes the safe-route passenger journey through actual settlement checkpoints, entry/quota settlement, rail travel and junctions, physical repairs, fuel, cargo and hostile encounters.

The screenshots are native UI renders from the playable scene. A queued mouse press and release in the normal Play Mode scene activated the real Buy button exactly once: Gold changed from 500 to 450 and steel from 20 to 25 metres. This input check ran in the normal scene capture, separately from the test-runner suite. The report screenshot uses demonstration earnings to expose the completed-day layout; it is not a recorded full-length journey. A continuous human playthrough is still useful for judging pace and feel.

An explicit UI EventSystem with InputSystemUIInputModule supplies mouse and navigation bindings. See [Unity’s runtime UI input documentation](https://docs.unity.com/en-us/engine/6000.6/manual/uitoolkits/uielements/uie-support-for-runtime-ui/uie-render-runtime-ui/uie-runtime-event-system).
