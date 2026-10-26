# Wind Triangles – a guide to the principles

This guide explains what WindTriangles calculates and why. It answers one practical question:

> I want to sail to a point at a given course and distance. **Which number should I steer on the compass?**

The course you want to make good over the ground is not the course you steer. Four things come between them, and each one is a correction:

| Step | Correction | Danish term | What causes it |
|------|------------|-------------|----------------|
| 1 | Current | Strøm (strømtrekant) | The water itself moves and carries the boat with it |
| 2 | Leeway | Afdrift | The wind pushes the boat sideways through the water |
| 3 | Variation | Misvisning | Magnetic north is not true north |
| 4 | Deviation | Deviation | The boat's own iron and electronics pull the compass needle |

The app works through them in that order, from the chart to the compass:

```
Course over ground (T) ──current──▶ Course through water (T) ──leeway──▶ True heading (T)
        ──variation──▶ Magnetic heading (M) ──deviation──▶ Compass course (C)
```

All directions are in degrees, measured clockwise from north (000°–359°). Speeds are in knots and distances in nautical miles.

---

## 1. Vectors

Every movement in the app is a **vector**: a direction plus a length (speed). Two vectors are added by putting the tail of the second at the head of the first. The sum goes from the start of the first to the end of the second.

That is all a "triangle" is in navigation: two vectors added together, with the sum as the third side. In the code (`Navigation/Vec.cs`) each vector is stored as an east and a north component:

```
east  = speed × sin(direction)
north = speed × cos(direction)
```

---

## 2. The current triangle (strømtrekant)

The boat moves through the water, and the water moves over the ground. What you see on the chart is the sum of the two:

```
Ground vector = Water vector + Current vector
   (COG, SOG)     (CTW, STW)      (set, drift)
```

| Term | Meaning |
|------|---------|
| **COG**, course over ground | The course you want to make good on the chart (input) |
| **SOG**, speed over ground | Your real speed towards the destination (result) |
| **CTW**, course through water | The course the boat must sail through the water (result) |
| **STW**, speed through water | The boat's speed through the water, from the log (input) |
| **Set** | The direction the current flows **towards** (input) |
| **Drift** | The current's speed (input) |

You know the direction of the ground vector but not its length. You know the length of the water vector but not its direction. The trick is to split the current into two parts:

- the part **along** your course, which only makes you faster or slower, and
- the part **across** your course, which pushes you sideways.

To stay on the line, the boat has to point slightly into the current so that the sideways part of its own speed cancels the sideways part of the current exactly:

```
cross-current = drift × sin(set − COG)
correction    = asin(−cross-current / STW)
CTW           = COG + correction
SOG           = STW × cos(correction) + drift × cos(set − COG)
```

Two things can go wrong, and the app warns about both:

- If the cross-current is stronger than the boat's speed (`|cross-current| > STW`), no heading can hold the course.
- If a foul current is stronger than the boat's speed along the course, SOG becomes zero or negative and you never arrive.

**Time to go** is simply `distance / SOG`.

> Rule of thumb: the correction is always **into** the current. A current setting to starboard means you steer to port of the course line.

---

## 3. Leeway (afdrift)

A sailing boat never moves exactly where it points. The wind pushes it a little sideways, to **leeward** (away from the wind). The angle between where the bow points and where the boat actually moves through the water is the **leeway angle**.

Leeway depends on the boat, the point of sail, the wind strength and the sea state. Typical values:

| Situation | Leeway |
|-----------|--------|
| Running or broad reach | 0°–2° |
| Beam reach | 2°–5° |
| Close-hauled, moderate wind | 4°–8° |
| Close-hauled, strong wind and waves | 8°–15° |

You enter leeway yourself. The app works out which side the wind is on and corrects **towards the wind**:

```
wind from starboard  →  boat slides to port       →  steer leeway° to starboard
wind from port       →  boat slides to starboard  →  steer leeway° to port
```

With the wind almost dead astern (more than 165° off the bow) there is no sideways push, so no leeway correction is applied.

The result is the **true heading**: the direction the bow must point, relative to true north.

---

## 4. Variation (misvisning)

A magnetic compass points to **magnetic north**, which is not the same as **true north** (the geographic pole the chart is drawn to). The angle between the two is the **variation**. It is printed on the chart's compass rose, together with the year and the yearly change. In Danish waters it is currently a few degrees east.

- **East** variation: magnetic north lies east of true north.
- **West** variation: magnetic north lies west of true north.

```
Magnetic = True − East variation
Magnetic = True + West variation
```

---

## 5. Deviation

The compass on board is also pulled by the boat's own iron (engine, keel) and electronics. That error is the **deviation**. It changes with the boat's heading, so it is read from the boat's **deviation table** (deviationstabel) for the heading you are about to steer.

```
Compass = Magnetic − East deviation
Compass = Magnetic + West deviation
```

### Remembering the signs

Going **from the chart to the compass** (True → Magnetic → Compass): **East is least, West is best**. Subtract east, add west.

Going **from the compass to the chart** (Compass → Magnetic → True), do the opposite: add east, subtract west.

The English memory aid for the order is **T**rue **V**irgins **M**ake **D**ull **C**ompanions: True, Variation, Magnetic, Deviation, Compass.

---

## 6. The wind triangle: true and apparent wind

The wind you feel on board, and that the sails and the wind instrument respond to, is the **apparent wind**. It is the **true wind** combined with the headwind the boat makes by moving:

```
Apparent wind = True wind − Boat's motion over ground
```

| Term | Meaning |
|------|---------|
| **True wind** | The wind over the ground, as given in the forecast (input: direction it blows **from**, and speed) |
| **Apparent wind** | The wind measured on the moving boat (result) |
| **TWA**, true wind angle | The angle between the bow and the true wind |
| **AWA**, apparent wind angle | The angle between the bow and the apparent wind |

On any point of sail except a dead run, the apparent wind comes from further forward than the true wind, and it is stronger when you sail towards it.

### The no-go zone

No sailing boat can sail straight into the wind. Most cruising boats cannot point higher than about 40°–45° to the true wind. If the true wind angle on the heading you need is less than 40°, the app warns that the destination is upwind and that you will have to **tack** (kryds) towards it.

> Note that the wind enters the calculation of the compass course **only through leeway**. The wind triangle doesn't change the course to steer. It shows what the wind will be like on board, and whether the course can be sailed at all.

---

## 7. Worked example

These are the app's default values.

| Input | Value |
|-------|-------|
| Desired course over ground | 045° T |
| Boat speed through water | 6 kn |
| Distance | 12 nm |
| Current | setting towards 135°, 1.5 kn |
| Wind | from 340°, 12 kn |
| Leeway | 5° |
| Variation | 4° E |
| Deviation | 2° W |

**1. Current.** The current sets at 135° − 045° = 90° to the course, straight across it to starboard.

```
cross-current = 1.5 × sin 90° = 1.5 kn
correction    = asin(−1.5 / 6) = −14.5°
CTW           = 045° − 14.5° = 030.5° T
SOG           = 6 × cos 14.5° + 1.5 × cos 90° = 5.8 kn
time to go    = 12 / 5.8 ≈ 2 h 04 min
```

**2. Leeway.** The wind from 340° is 50° on the port side of 030.5°, so the boat slides to starboard. Steer 5° to port:

```
True heading = 030.5° − 5° = 025.5° T
```

**3. Variation 4° E.** East is least:

```
Magnetic = 025.5° − 4° = 021.5° M
```

**4. Deviation 2° W.** West is best:

```
Compass = 021.5° + 2° = 023.5°  →  steer 024° C
```

**Wind on board.** The boat moves 5.8 kn towards 045°. Combined with the 12 kn true wind from 340°, this gives an apparent wind of about **15.4 kn from 000°**. That is 26° on the port bow (true wind angle 46° port), so the boat is close-hauled but just outside the no-go zone.

---

## 8. Reading the plot

The compass rose is drawn with true north up, 1° tick marks and a label every 30°. The four quarters (NE, SE, SW, NW) are shaded alternately. All vectors start at the centre.

| Element | Meaning |
|---------|---------|
| Green arrow | Course and speed over ground: where you actually go |
| Blue arrow | Course and speed through the water |
| Orange arrow | Current, drawn from the tip of the blue arrow; blue + orange = green |
| Purple dashed line | True heading, including leeway, extended to the rim |
| Teal arrow | True wind (pointing the way it blows) |
| Grey dashed arrow | Headwind from the boat's own motion |
| Red arrow | Apparent wind = teal + grey dashed |
| Red marker on the rim | Magnetic north |
| Black marker on the rim | Compass north |

The angle from the black marker (compass north) to the purple marker (heading) is the compass course.

Use the **Current triangle / Wind triangle / Both** switch to choose what is drawn. Each view is scaled so its largest vector fills about 80 % of the circle. The wind is usually much faster than the boat, so in the **Both** view the current triangle becomes small.

---

## 9. Limitations

- **Current and wind are assumed to be constant** for the whole leg. On a long passage, or where tidal streams turn, split the leg into parts and calculate each one separately.
- **Leeway is your own estimate.** The app does not derive it from wind speed or boat type.
- **Deviation is a single value.** Read it from the deviation table for a heading close to the result. If the result changes a lot, look up the deviation again and recalculate.
- The calculation uses flat-plane geometry, which is fine for coastal distances. It does not account for the curvature of the earth or for great-circle routes.
- This is a learning and planning aid. It does not replace proper navigation and seamanship.
