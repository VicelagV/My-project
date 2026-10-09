# First economy pass

The attached `message (3).txt` is used as the economy design brief. These are initial tuning values, not final balancing. `Assets/Movement/Resources/EconomyDefaults.asset` contains prices, passenger rewards, quotas, credit limits, repair bills and future system prices in one editable Unity asset.

## Playable now

- Days start with 500 Gold and 20 metres of steel. Existing coal/starting tools are preserved.
- Vendors use physical M1/E buttons. Steel costs 50 Gold per 5 m, coal 20, replacement saw 25, connector 15. Villages add 20%. Extra town shops offer 15% discounts and a deterministic mixed selection. Scrap, screws and wood from these shops are recorded in the shared inventory; their future crafting uses are not active yet.
- Purchases may borrow a total outstanding 100 Gold. Cash never goes negative. Credit becomes a day-end obligation. Failed item pickup does not charge money or borrow credit. Held salvage sells for full value to a matching or general merchant, 60% for broadly relevant goods and 30% to an unrelated specialist.
- The normal prototype starts with 14 passengers and supports at most 16 ticket places across both coaches. Four initially want the safe route's town, four Bluefort and six Terminum. This deliberately makes the current safe journey testable. Random destinations across every route, boarding replacement passengers and complete risk-route balancing are subsequent work.
- Look at a passenger for their destination and happiness. Stopping within 35 m of the correct station resolves that passenger once, removes the model and pays job value + destination reward + happiness tip. E can force an early exit at a valid stopped settlement for 50 Gold.
- Destination rewards: Terminum 100, town 200, village 300, isolated station 500. Job bands, tips and special-passenger rewards are configured centrally. Failed/abandoned journeys and negative happiness pay nothing.
- Terminum entry costs 1,000 Gold before its passenger fares can be collected. Without enough cash, the train stops at the approach checkpoint. Reverse and choose the bypass if needed. The fee cannot use credit and is charged once.
- Stopping at Terminum, or reaching the return section on the bypass, settles the day. Remaining undelivered passengers are abandoned. Obligations and credit are deducted first, then the quota, then component repair bills. Remaining Gold converts to Blumt. Quota failure cannot become a successful settlement.
- The quota starts at 1,500, grows by 18% per day and rounds to the nearest 50. Unpaid repair costs after a successful quota carry into the next quota. Normal/medium/hard Blumt multipliers are 1 / 1.25 / 1.5. Settlement is idempotent.
- A small HUD shows money, debt, quota, Blumt and remaining passengers. The repair locker also shows current credit. Free test coal at supply stops is retained from the previously agreed prototype; turn off `freePrototypeCoal` to charge the configured price.

## Hooks and reserved values

The ledger supports intact/damaged/failed cargo rewards and penalties, charging obligations, special-passenger payouts and component damage billing. Configured medical, weapon, ammunition, component, storage carriage, fine and toll prices are reserved for the corresponding mechanics. They do not create medical/combat/crafting systems, bandits or buyable extra carriages. Cargo contract acceptance/delivery gameplay and happiness events are not yet wired; prototype passengers begin at a fixed happiness. Do not describe those future systems as playable.

There is no automatic next day or saved-run persistence yet. `BeginDay(day)` preserves accumulated Blumt and carried repair debt within the current ledger, but restarting Play mode starts a new run. This preserves the requested stop-at-day-end prototype behaviour.

## Verification

Play mode checks cover capped credit over multiple purchases, valid/failed fare outcomes, cargo penalties, entry fee idempotence, accounting order, repair carry, model camera visibility and a journey through the actual generated stops. Existing movement, interaction, train, model integrity and world-generation tests remain part of the suite. Unity captures show the character variants, passenger coach and real marketplace buttons.

Final Unity 6000.6.4f1 validation: **125/125 Play mode tests passed**.
