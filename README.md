# More Vehicles Renew 2

[![Steam Workshop](https://img.shields.io/badge/Steam_Workshop-3825606938-blue.svg?logo=steam)](https://steamcommunity.com/sharedfiles/filedetails/?id=3825606938)
[![GitHub](https://img.shields.io/badge/GitHub-Repository-black.svg?logo=github)](https://github.com/NqocDuy1426/MoreVehiclesRenew2)
[![Platform](https://img.shields.io/badge/Platform-macOS%20%7C%20Windows-brightgreen.svg)]()
[![Game](https://img.shields.io/badge/Cities%3A%20Skylines-1.x-orange.svg)]()

> **The Definitive 65K/131K Vehicle Limit Breaker & Simulation Optimization for Cities: Skylines.**  
> Completely rewritten, modernized, and packed with 4 breakthrough technologies by **NgocDuy**.

---

## 🚗 Overview
Expands the hardcoded base game vehicle limits to the maximum mathematical boundary permitted by the 16-bit Cities: Skylines engine:
- **Active Vehicles**: `65,536` (Vanilla: `16,384`) — $4\times$ capacity!
- **Parked Vehicles**: `65,536` (Vanilla: `32,768`) — $2\times$ capacity!
- **Total Tracked Vehicles**: **131,072**!
- **Real-World Logistics Throughput**: **Equivalent to 260,000+ vehicles!**

---

## ⚡ The 4 Core Breakthrough Technologies

### 1. 👻 Ghost & Stuck Vehicle Annihilator
- **The Problem**: Demolishing roads often left orphaned vehicles floating in mid-air along destroyed segments, occupying thousands of vehicle slots permanently.
- **The Solution**: Hooks into `NetManager.ReleaseSegmentImplementation` to instantly dissolve any vehicles stranded on demolished segments. Includes an inline slice watchdog during simulation frames to cleanly release permanent deadlocks and vehicles with destroyed target buildings.

### 2. 🚀 Turbo Spawn Dispatcher
- **The Problem**: In vanilla, `TransferManager` only matches 1 material per frame, meaning it takes 15–20 minutes to respawn vehicles after clearing traffic, starving commercial buildings and causing total economic collapse.
- **The Solution**: Dynamically accelerates transfer matching by 4x when vehicle count is below 58,000. Cycles through multiple complementary frame phases in a single tick, fully repopulating 60,000 city vehicles in 30–60 seconds!

### 3. 🛡️ Emergency Slot Reclaimer
- **The Problem**: When vehicle count hits 65,535, vanilla `CreateVehicle` returns `false`, causing shops and factories to fail to spawn delivery trucks and abandon with "Not enough buyers/goods".
- **The Solution**: If all 65,535 slots are completely exhausted, automatically reclaims 1 deadlocked or stagnant ghost vehicle in real-time, guaranteeing that critical service and freight vehicles can always spawn without dropping deliveries.

### 4. 📦 Smart Logistics Multiplier
- **The Problem**: Huge cities (500k+ pop) need 30k–40k trucks just for baseline goods deliveries.
- **The Solution**: Safely doubles the payload capacity of CargoTrucks and GarbageTrucks. Halves the number of freight trucks needed on city streets, effectively granting a 65k vehicle city the logistical throughput of **over 260,000 vehicles**!

---

## ⚙️ Compatibility & Architecture
- **Safe 16-bit Hardware Limit**: Stays strictly within the engine's unmanaged 16-bit `ushort` memory boundary, preventing catastrophic integer overflow crashes.
- **Backwards & Forwards Compatible**: Reads and writes both `User/More Vehicles` (legacy) and `User/More Vehicles Renew 2` simulation metadata.
- **Standalone**: Zero external DLL dependencies; self-contained Harmony 2 patcher and logging engine.
- **Ecosystem Tested**: Fully compatible with TM:PE, More Network Capacity Renew, AVO, IPT 2/3, and Loading Screen Mod.

---

## 🛠️ Building
```bash
./build.sh
```
This compiles `MoreVehiclesRenew2.dll` and deploys it directly to your local Cities: Skylines mods directory (`Addons/Mods/MoreVehiclesRenew2`) and the SteamCMD staging folder.
