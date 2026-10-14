# More Vehicles Renew 2

An independent, modernized mod for **Cities: Skylines (1)** by **NgocDuy**.

## 🚗 Overview
Expands the hardcoded base game vehicle limits to the maximum mathematical boundary permitted by the 16-bit Cities: Skylines engine:
- **Active Vehicles**: `65,536` (Vanilla: `16,384`) — $4\times$ capacity!
- **Parked Vehicles**: `65,536` (Vanilla: `32,768`) — $2\times$ capacity!
- **Total Tracked Vehicles**: **131,072**!

## ⚙️ Key Technical Details
- **Architecture**: In Cities: Skylines 1, entity references (Citizen, Building, NetLane, Vehicle linkage) use unmanaged 16-bit unsigned integers (`ushort`, max value `65,535`). More Vehicles Renew 2 scales arrays to this absolute maximum ceiling ($2^{16}$).
- **Backwards & Forwards Compatible**: Reads and writes both `User/More Vehicles` (legacy) and `User/More Vehicles Renew 2` simulation metadata, allowing existing cities created with previous More Vehicles versions to load smoothly without losing vehicle data.
- **Standalone & Clean**: Completely embeds its patching and logging mechanisms, eliminating flaky external dependencies.
- **Automatic Mod Conflict Detection**: Automatically notifies if legacy More Vehicles versions are active so you can safely switch without duplicate hook overhead.

## 🛠️ Building
Run the included build script:
```bash
./build.sh
```
This compiles `MoreVehiclesRenew2.dll` and deploys it directly to your local Cities: Skylines mods directory (`Addons/Mods/MoreVehiclesRenew2`).
