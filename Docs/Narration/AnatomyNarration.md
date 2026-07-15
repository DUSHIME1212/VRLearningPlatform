# Anatomy Narration Scripts (English)

Kid-friendly narration for every interactive part in the six anatomy scenes. Feed the **Narration** text to your audio generator, then drop the resulting clips into `Assets/Audio/Narration/` and run **Tools ▸ Narration ▸ Assign Clips…** to attach them automatically.

**Clip file names** = the **Clip id** column below, with a `.wav` (or `.mp3` / `.ogg`) extension — e.g. `af_artery_wall.wav`, `Aorta.wav`. The assigner matches by each part's `PartId`; parts that have no `PartId` (the heart prefabs and the Diaphragm scene) fall back to their English name with spaces → underscores (e.g. `Left_Lung.wav`).

Voice direction: warm, curious, encouraging; second person; short sentences; ~15–25 seconds each. End several lines with an invitation to interact.

---

## 1. Blood Circulation — Heart Pump  (`BloodCirc_HeartPump`)
*These 8 parts are prefabs that already have names + descriptions; they only need audio clips.*

| Part | Clip id |
|---|---|
| Aorta | `Aorta` |
| Vena Cava | `Vena_Cava` |
| Pulmonary Artery | `Pulmonary_Artery` |
| Left Atrium | `Left_Atrium` |
| Right Atrium | `Right_Atrium` |
| Left Ventricle | `Left_Ventricle` |
| Right Ventricle | `Right_Ventricle` |
| Valve | `Valve` |

**Aorta** — "This big, curving tube is the aorta — the largest blood vessel in your whole body! Every time your heart squeezes, it pushes a wave of fresh, oxygen-filled blood out through here. From the aorta, the blood races off to your arms, legs, tummy, and all the way up to your brain. Watch it whoosh out when the heart beats!"

**Vena Cava** — "This wide tube is the vena cava. It's like a return road that brings tired, oxygen-poor blood back to the heart after it has traveled all around your body. The blood flows in gently here, ready to be pumped to the lungs for a fresh scoop of oxygen."

**Pulmonary Artery** — "Say hello to the pulmonary artery. It carries blood from the heart straight to your lungs. It's a bit unusual — it's the one artery that carries 'used', oxygen-poor blood. Don't worry, the lungs will fill it right back up with fresh oxygen!"

**Left Atrium** — "This is the left atrium, one of the heart's four rooms. It sits up top and catches oxygen-rich blood coming back from the lungs. When it's full, it gives a gentle squeeze and sends the blood down into the room below it."

**Right Atrium** — "Meet the right atrium, the top-right room of the heart. It collects oxygen-poor blood coming home from your body, then passes it down to the room below — which will send it off to the lungs for more oxygen."

**Left Ventricle** — "This is the left ventricle — the strongest room in your heart! Its walls are thick and muscly because it has the biggest job: pushing oxygen-rich blood all the way around your body. Give it a squeeze and feel how powerful that pump is."

**Right Ventricle** — "Here's the right ventricle. It pumps oxygen-poor blood on a short trip over to your lungs. It doesn't need to be as strong as the left ventricle, because the lungs are right next door."

**Valve** — "These little doors are the heart's valves. They snap open to let blood through, then snap shut so it can't slip backward. That 'lub-dub' sound your heart makes? That's the valves closing! They keep the blood flowing one way, like little one-way gates."

---

## 2. Blood Circulation — Arterial Flow  (`BloodCirc_ArterialFlow`)
*These 5 parts had empty text; this plan fills in their names, descriptions, and narration.*

| Part | PartId / Clip id |
|---|---|
| Artery Wall | `af_artery_wall` |
| Lumen | `af_lumen` |
| Smooth Muscle | `af_smooth_muscle` |
| Endothelium | `af_endothelium` |
| Red Blood Cell | `af_rbc` |

**Artery Wall** — "This is the artery wall — the thick, strong outer layer of a blood vessel. Arteries carry blood away from the heart with a big push, so their walls are stretchy and tough, like a bouncy garden hose. They squeeze a little with every heartbeat to keep the blood zooming along."

**Lumen** — "The lumen is the open tunnel right in the middle of the vessel — it's the road the blood actually travels on! When the lumen is wide, lots of blood flows through easily. When it gets narrow, the blood has to squeeze together and it speeds up. Try changing its size and watch what happens to the flow!"

**Smooth Muscle** — "Wrapped around the vessel is a layer of smooth muscle. It works all by itself — you never have to think about it! It can gently squeeze to make the vessel narrower, or relax to make it wider. That's how your body sends extra blood to places that need it, like your legs when you run."

**Endothelium** — "The endothelium is the super-smooth lining on the inside of every blood vessel. It's slippery on purpose, so blood glides along without getting stuck. It also helps decide what can pass in and out of your blood. Think of it as a slick, non-stick coating for your pipes."

**Red Blood Cell** — "These little red discs are red blood cells, and you have millions of them! Each one is like a tiny delivery truck carrying oxygen from your lungs to the rest of your body. Their flat, dented shape lets them bend and squeeze through even the narrowest vessels."

---

## 3. Blood Circulation — Capillary Exchange  (`BloodCirc_Capillary`)
*These 5 parts had empty text; this plan fills them in.*

| Part | PartId / Clip id |
|---|---|
| Capillary Wall | `cap_wall` |
| Red Blood Cell | `cap_rbc` |
| Tissue Cell | `cap_tissue_cell` |
| Oxygen | `cap_oxygen` |
| Carbon Dioxide | `cap_co2` |

**Capillary Wall** — "This is the wall of a capillary — the tiniest blood vessel of all. It's so thin that oxygen and food can slip right through it into your body's cells, and waste can pass back in. This is the exact spot where your blood does its most important job: the big trade!"

**Red Blood Cell** — "Watch the red blood cells line up single file to squeeze through the capillary — it's that narrow! Right here they drop off the oxygen they've been carrying and pick up carbon-dioxide waste to take away. It's like a delivery truck making its very last stop."

**Tissue Cell** — "This is a tissue cell — one of the tiny building blocks that make up you! It's hungry for oxygen and food. As blood flows past in the capillary, this cell grabs the oxygen it needs and hands back the waste it wants to get rid of."

**Oxygen** — "These bubbles are oxygen. You breathe them in from the air, and your blood carries them to every cell in your body. Cells use oxygen like fuel to make the energy you need to run, jump, and think. Watch the oxygen hop out of the blood and into the cell!"

**Carbon Dioxide** — "These bubbles are carbon dioxide — a waste gas your cells make when they use up oxygen. Your blood picks it up and carries it back to your lungs so you can breathe it out. Breathe in the good oxygen, breathe out the carbon dioxide!"

---

## 4. Breathing — Diaphragm  (`Breathing_Diaphragm`)
*These 6 parts already have names + descriptions; they only need audio clips.*

| Part | Clip id |
|---|---|
| Left Lung | `Left_Lung` |
| Right Lung | `Right_Lung` |
| Trachea | `Trachea` |
| Bronchi | `Bronchi` |
| Diaphragm | `Diaphragm` |
| Rib Cage | `Rib_Cage` |

**Left Lung** — "This is your left lung, a soft, spongy balloon that fills with air when you breathe in. It's a little smaller than the right lung, to leave a cozy space for your heart to sit. Watch it puff up and shrink as you breathe!"

**Right Lung** — "This is your right lung, a little bigger than the left one. Deep inside are millions of tiny air pockets where oxygen from your breath sneaks into your blood. Every time you breathe in, this lung fills up like a stretchy balloon."

**Trachea** — "This is the trachea, or windpipe. It carries your breath from your throat down toward your lungs. Feel the front of your neck — that bumpy tube is your trachea! It has little rings to keep it from squishing shut."

**Bronchi** — "Where the windpipe splits into two, you get the bronchi. One tube goes into the left lung and one into the right, like a road that forks in two. From here the air keeps branching into smaller and smaller tubes, like an upside-down tree."

**Diaphragm** — "Meet the diaphragm, a big dome-shaped muscle under your lungs — the real hero of breathing! When it pulls down flat, it makes room for your lungs to fill with air. When it relaxes back up, the air puffs out. Try pulling it down and watch the lungs grow bigger!"

**Rib Cage** — "These curved bones are your rib cage. They make a strong cage that protects your soft lungs and heart, like a helmet for your chest. The ribs also lift up and out when you breathe in, making even more room for air. Tap your chest and feel them!"

---

## 5. Breathing — Gas Exchange  (`Breathing_GasExchange`)
*These 6 parts had empty text; this plan fills them in.*

| Part | PartId / Clip id |
|---|---|
| Alveolus | `gx_alveolus` |
| Alveolar Wall | `gx_alveolar_wall` |
| Capillary | `gx_capillary` |
| Red Blood Cell | `gx_rbc` |
| Oxygen | `gx_oxygen` |
| Carbon Dioxide | `gx_co2` |

**Alveolus** — "This little bubble is an alveolus — one of the millions of tiny air sacs deep inside your lungs. When you breathe in, it fills with fresh air. Its walls are super thin so oxygen can pass right through into your blood. If you spread all your air sacs flat, they'd cover a whole tennis court!"

**Alveolar Wall** — "This is the alveolar wall — the thin barrier between the air in your lungs and your blood. It's so thin and delicate that gases can zip straight across it: oxygen goes into the blood, and carbon dioxide comes out to be breathed away. It's the perfect trading window!"

**Capillary** — "Wrapped snugly around the air sac is a capillary, the tiniest blood vessel. Blood flows through it right next to the air. This is where the magic swap happens — the blood grabs oxygen and drops off carbon dioxide. They're so close, the gases only have a whisker to travel!"

**Red Blood Cell** — "Here come the red blood cells, flowing through the capillary. As they pass the air sac, each one grabs a fresh load of oxygen — turning bright red! — and lets go of its carbon dioxide. Then they rush off to deliver that oxygen all around your body."

**Oxygen** — "These are oxygen molecules from the air you breathed in. Watch them move out of the air sac and into the blood, hopping onto the red blood cells for a ride. Your body needs a fresh supply with every single breath!"

**Carbon Dioxide** — "These are carbon-dioxide molecules — the waste your body wants to get rid of. Watch them leave the blood and float into the air sac so you can breathe them out. Breathe in oxygen, breathe out carbon dioxide — that's the whole point of breathing!"

---

## 6. Breathing — Lung Expansion  (`Breathing_LungExpansion`)
*These 6 parts had empty text; this plan fills them in.*

| Part | PartId / Clip id |
|---|---|
| Trachea | `le_trachea` |
| Bronchi | `le_bronchi` |
| Left Lung | `le_left_lung` |
| Right Lung | `le_right_lung` |
| Rib Cage | `le_rib_cage` |
| Diaphragm | `le_diaphragm` |

**Trachea** — "This is the trachea, your windpipe — the main tube air travels through on its way to your lungs. When your chest expands to breathe in, air rushes down this tube. Strong little rings keep it open so it never collapses, even when you bend your neck."

**Bronchi** — "These are the bronchi, the two tubes that branch off the windpipe — one for each lung. As your lungs expand, air flows down the bronchi and spreads into thousands of smaller tubes inside, like air traveling down a tree from trunk to branches to twigs."

**Left Lung** — "This is the left lung. Watch how it grows bigger when you breathe in and shrinks when you breathe out. It stretches like a balloon to pull air deep inside. It's a bit smaller than the right lung, so your heart has room to sit beside it."

**Right Lung** — "Here's the right lung, the bigger of the two. When your rib cage lifts and your diaphragm drops, this lung expands and sucks in fresh air. When they relax, it gently pushes the air back out. Watch it fill and empty with each breath!"

**Rib Cage** — "This is the rib cage. When you breathe in, muscles pull the ribs up and outward, making your chest bigger — that extra space is what pulls air into your lungs. When you breathe out, the ribs settle back down. Put your hands on your ribs and feel them move!"

**Diaphragm** — "This dome-shaped muscle is the diaphragm, sitting right under your lungs. When it tightens and flattens downward, it makes the chest bigger and pulls air in. When it relaxes back into a dome, it pushes air out. Try moving it and watch the lungs expand and shrink!"
