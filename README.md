## Introduction

This package allows you to easily configure the stage director credit scaling for multiplayer. By default, the per-player scaling and flat bonus are both `0.5` (when normalizing to a value of 1.0 for singleplayer). For example, here's a table of the stage director's credits when spawning in *Siphoned Forest* on stage 1:

| Player Count | Credits | Normalized Value | Credits Per Person
|
| `1` | `300` | `1.0` | `300`
| `2` | `450` | `1.5` | `225`
| `3` | `600` | `2.0` | `200`
| `4` | `750` | `2.5` | `187.5`
| `5` | `900` | `3.0` | `180`
| `6` | `1050`| `3.5` | `175`
| `999` | `150000` | `500.0` | `~150`

Observe that `[Normalized Value] = 0.5 * [Player Count] + 0.5`. This is why I consider `0.5` the default value for both the per-player and flat scaling. If you instead want the credits per person to remain constant, you can adjust the settings in `Mod Options -> Mohr Credits` and set `Flat` to `0` and `Scalar` to `1`. Basically the whole reason I made this mod was to do that.

Note that this mod does **not** adjust the bonus credits granted from things like the "vault" opening in *Abyssal Depths* by default, matching default behavior, though this does add a toggle to enable that behavior in the settings.

It is recommended that you install with [*r2modman*](https://thunderstore.io/package/ebkr/r2modman/).

Mostly stolen from [*MultitudesDifficulty*](https://thunderstore.io/package/6thmoon/MultitudesDifficulty/) by **6thmoon**, this builds on their stage credit adjusting system.

## Version History

#### `1.0.0`
- Release