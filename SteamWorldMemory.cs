using LiveSplit.Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
namespace LiveSplit.SteamWorldDig2 {
	public partial class SteamWorldMemory {
		public Process Program { get; set; }
		public bool IsHooked { get; set; } = false;
		private DateTime lastHooked;

		public SteamWorldMemory() {
			lastHooked = DateTime.MinValue;
		}

		public double GameTime() {
			double time = Program.Read<double>(Program.MainModule.BaseAddress, 0x61F028, 0x0, 0x10, 0x60, 0x10);
			return time < 0 ? 0 : time > 999999 ? 0 : time;
		}
		public int CurrentGold() {
			int gold = Program.Read<int>(Program.MainModule.BaseAddress, 0x61F028, 0x0, 0x10, 0x60, 0x24);
			return gold < 0 ? 0 : gold > 999999 ? 0 : gold;
		}
		public int TotalGold() {
			int gold = Program.Read<int>(Program.MainModule.BaseAddress, 0x61F028, 0x0, 0x10, 0x60, 0x1c);
			return gold < 0 ? 0 : gold > 999999 ? 0 : gold;
		}
		public int TotalCogs() {
			int cogs = Program.Read<int>(Program.MainModule.BaseAddress, 0x61F028, 0x0, 0x10, 0x60, 0x28);
			return cogs < 0 ? 0 : cogs > 100 ? 0 : cogs;
		}
		public int TotalDeaths() {
			int cogs = Program.Read<int>(Program.MainModule.BaseAddress, 0x61F028, 0x0, 0x10, 0x60, 0x18);
			return cogs < 0 ? 0 : cogs > 100 ? 0 : cogs;
		}
		public int TotalArtifacts() {
			int cogs = Program.Read<int>(Program.MainModule.BaseAddress, 0x61F028, 0x0, 0x10, 0x60, 0x2c);
			return cogs < 0 ? 0 : cogs > 100 ? 0 : cogs;
		}
		public string Area() {
			return Program.ReadAscii((IntPtr)Program.Read<uint>(Program.MainModule.BaseAddress, 0x61F028, 0x0, 0x10, 0x60, 0x30));
		}
		public PointF Position() {
			float px = Program.Read<float>(Program.MainModule.BaseAddress, 0x61F028, 0x0, 0x10, 0x80, 0x19c);
			px = px < -9999999 ? 0 : px > 9999999 ? 0 : px;
			float py = Program.Read<float>(Program.MainModule.BaseAddress, 0x61F028, 0x0, 0x10, 0x80, 0x1a0);
			py = py < -9999999 ? 0 : py > 9999999 ? 0 : py;
			return new PointF(px, py);
		}
		public HashSet<string> AquiredFlags() {
			HashSet<string> flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			int nLists = Program.Read<int>(Program.MainModule.BaseAddress, 0x61F028, 0x0, 0x10, 0x60, 0x16c);
			for (int i = 0; i <= nLists; i++) {
				IntPtr start = (IntPtr)Program.Read<uint>(Program.MainModule.BaseAddress, 0x61F028, 0x0, 0x10, 0x60, 0x168, 4 * i);
				while (start != IntPtr.Zero) {
					int upgrade = Program.Read<int>(start - 0x4);
					string currentFlag = Program.ReadAscii((IntPtr)Program.Read<uint>(start - 0x8));
					if (!string.IsNullOrEmpty(currentFlag)) {
						flags.Add(currentFlag + (upgrade == 1 ? "" : upgrade.ToString()));
					}
					start = (IntPtr)Program.Read<uint>(start);
				}
			}

			return flags;
		}
		public bool HookProcess() {
			if ((Program == null || Program.HasExited) && DateTime.Now > lastHooked.AddSeconds(1)) {
				lastHooked = DateTime.Now;
				Process[] processes = Process.GetProcessesByName("Dig2");
				Program = processes.Length == 0 ? null : processes[0];
				IsHooked = true;
			}

			if (Program == null || Program.HasExited) {
				IsHooked = false;
			}

			return IsHooked;
		}
		public void Dispose() {
			if (Program != null) {
				Program.Dispose();
			}
		}
	}
	public enum SplitName {
		[Description("Manual Split (Not Automatic)"), ToolTip("Does not split automatically. Use this for custom splits not yet defined.")]
		ManualSplit,

		[Description("Sprint Hydraulics (Skill)"), ToolTip("Splits when obtaining the Sprint Hydraulics")]
		SprintHydraulics,
		[Description("Fen / Fate (Skill)"), ToolTip("Splits when obtaining saving Fen")]
		Fen,
		[Description("Pressure Bomb (Skill)"), ToolTip("Splits when obtaining the Pressure Bomb")]
		PressureBomb,
		[Description("Pressure Grenade (Skill)"), ToolTip("Splits when obtaining the Pressure Bomb Grenade upgrade")]
		PressureGrenade,
		[Description("Jackhammer (Skill)"), ToolTip("Splits when obtaining the Jackhammer")]
		Jackhammer,
		[Description("Hook Shot (Skill)"), ToolTip("Splits when obtaining the Hook Shot")]
		HookShot,
		[Description("Long-Range Grappler (Skill)"), ToolTip("Splits when obtaining the Long-Range Grappler")]
		HookShotLong,
		[Description("Ignition Axe (Skill)"), ToolTip("Splits when obtaining the Ignition Axe")]
		IgnitionAxe,
		[Description("Jet Engine (Skill)"), ToolTip("Splits when obtaining the Jet Engine")]
		JetEngine,
		[Description("Ramjet Vigor (Skill)"), ToolTip("Splits when obtaining the Ramjet Vigor upgrade")]
		RamjetVigor,
		[Description("Ultra Composite Armor (Skill)"), ToolTip("Splits when obtaining the Ultra Composite Armor")]
		UltraCompositeArmor,

		[Description("Pickaxe 2 (Shop)"), ToolTip("Splits when obtaining the Pickaxe 2 from the shop")]
		Pickaxe2,
		[Description("Pickaxe 3 (Shop)"), ToolTip("Splits when obtaining the Pickaxe 3 from the shop")]
		Pickaxe3,
		[Description("Pickaxe 4 (Shop)"), ToolTip("Splits when obtaining the Pickaxe 4 from the shop")]
		Pickaxe4,
		[Description("Pickaxe 5 (Shop)"), ToolTip("Splits when obtaining the Pickaxe 5 from the shop")]
		Pickaxe5,
		[Description("Pickaxe 6 (Shop)"), ToolTip("Splits when obtaining the Pickaxe 6 from the shop")]
		Pickaxe6,
		[Description("Pickaxe 7 (Shop)"), ToolTip("Splits when obtaining the Pickaxe 7 from the shop")]
		Pickaxe7,
		[Description("Pickaxe 8 (Shop)"), ToolTip("Splits when obtaining the Pickaxe 8 from the shop")]
		Pickaxe8,
		[Description("Backpack 2 (Shop)"), ToolTip("Splits when obtaining the Backpack 2 from the shop")]
		Backpack2,
		[Description("Backpack 3 (Shop)"), ToolTip("Splits when obtaining the Backpack 3 from the shop")]
		Backpack3,
		[Description("Backpack 4 (Shop)"), ToolTip("Splits when obtaining the Backpack 4 from the shop")]
		Backpack4,
		[Description("Backpack 5 (Shop)"), ToolTip("Splits when obtaining the Backpack 5 from the shop")]
		Backpack5,
		[Description("Backpack 6 (Shop)"), ToolTip("Splits when obtaining the Backpack 6 from the shop")]
		Backpack6,
		[Description("Backpack 7 (Shop)"), ToolTip("Splits when obtaining the Backpack 7 from the shop")]
		Backpack7,
		[Description("Backpack 8 (Shop)"), ToolTip("Splits when obtaining the Backpack 8 from the shop")]
		Backpack8,
		[Description("Backpack 9 (Shop)"), ToolTip("Splits when obtaining the Backpack 9 from the shop")]
		Backpack9,
		[Description("Backpack 10 (Shop)"), ToolTip("Splits when obtaining the Backpack 10 from the shop")]
		Backpack10,
		[Description("Lamp 2 (Shop)"), ToolTip("Splits when obtaining the Lamp 2 from the shop")]
		Lamp2,
		[Description("Lamp 3 (Shop)"), ToolTip("Splits when obtaining the Lamp 3 from the shop")]
		Lamp3,
		[Description("Lamp 4 (Shop)"), ToolTip("Splits when obtaining the Lamp 4 from the shop")]
		Lamp4,
		[Description("Lamp 5 (Shop)"), ToolTip("Splits when obtaining the Lamp 5 from the shop")]
		Lamp5,
		[Description("Lamp 6 (Shop)"), ToolTip("Splits when obtaining the Lamp 6 from the shop")]
		Lamp6,
		[Description("Lamp 7 (Shop)"), ToolTip("Splits when obtaining the Lamp 7 from the shop")]
		Lamp7,
		[Description("Lamp 8 (Shop)"), ToolTip("Splits when obtaining the Lamp 8 from the shop")]
		Lamp8,
		[Description("Armor 2 (Shop)"), ToolTip("Splits when obtaining the Armor 2 from the shop")]
		Armor2,
		[Description("Armor 3 (Shop)"), ToolTip("Splits when obtaining the Armor 3 from the shop")]
		Armor3,
		[Description("Armor 4 (Shop)"), ToolTip("Splits when obtaining the Armor 4 from the shop")]
		Armor4,
		[Description("Armor 5 (Shop)"), ToolTip("Splits when obtaining the Armor 5 from the shop")]
		Armor5,
		[Description("Armor 6 (Shop)"), ToolTip("Splits when obtaining the Armor 6 from the shop")]
		Armor6,
		[Description("Armor 7 (Shop)"), ToolTip("Splits when obtaining the Armor 7 from the shop")]
		Armor7,
		[Description("Armor 8 (Shop)"), ToolTip("Splits when obtaining the Armor 8 from the shop")]
		Armor8,
		[Description("Water Tank 2 (Shop)"), ToolTip("Splits when obtaining the Water Tank 2 from the shop")]
		WaterTank2,
		[Description("Water Tank 3 (Shop)"), ToolTip("Splits when obtaining the Water Tank 3 from the shop")]
		WaterTank3,
		[Description("Water Tank 4 (Shop)"), ToolTip("Splits when obtaining the Water Tank 4 from the shop")]
		WaterTank4,
		[Description("Water Tank 5 (Shop)"), ToolTip("Splits when obtaining the Water Tank 5 from the shop")]
		WaterTank5,
		[Description("Pressure Bomb 2 (Shop)"), ToolTip("Splits when obtaining the Pressure Bomb 2 from the shop")]
		PressureBomb2,
		[Description("Pressure Bomb 3 (Shop)"), ToolTip("Splits when obtaining the Pressure Bomb 3 from the shop")]
		PressureBomb3,
		[Description("Pressure Bomb 4 (Shop)"), ToolTip("Splits when obtaining the Pressure Bomb 4 from the shop")]
		PressureBomb4,
		[Description("Jackhammer 2 (Shop)"), ToolTip("Splits when obtaining the Jackhammer 2 from the shop")]
		Jackhammer2,
		[Description("Jackhammer 3 (Shop)"), ToolTip("Splits when obtaining the Jackhammer 3 from the shop")]
		Jackhammer3,
		[Description("Jackhammer 4 (Shop)"), ToolTip("Splits when obtaining the Jackhammer 4 from the shop")]
		Jackhammer4,
		[Description("Jet Engine 2 (Shop)"), ToolTip("Splits when obtaining the Jet Engine 2 from the shop")]
		JetEngine2,
		[Description("Jet Engine 3 (Shop)"), ToolTip("Splits when obtaining the Jet Engine 3 from the shop")]
		JetEngine3,

		[Description("Archaea (Area)"), ToolTip("Splits when entering Archaea")]
		Archaea,
		[Description("Device 0 Eastern (Area)"), ToolTip("Splits when entering the first Device's room in the Eastern Temple")]
		Device0,
		[Description("Device 1 Guidance (Area)"), ToolTip("Splits when entering the Device's room in the Temple of Guidance")]
		Device1,
		[Description("Device 2 Yarrow (Area)"), ToolTip("Splits when entering the Device's room in Yarrow")]
		Device2,
		[Description("Device 3 Eastern (Area)"), ToolTip("Splits when entering the final Device's room in the Eastern Temple")]
		Device3,
		[Description("Eastern Temple (Area)"), ToolTip("Splits when entering Eastern Temple")]
		EasternTemple,
		[Description("El Machino (Area)"), ToolTip("Splits when entering El Machino")]
		ElMachino,
		[Description("Oasis (Area)"), ToolTip("Splits when entering The Oasis")]
		Oasis,
		[Description("Oasis Boss Room (Area)"), ToolTip("Splits when entering the Oasis final boss room")]
		OasisBoss,
		[Description("Oasis Boss Killed (Area)"), ToolTip("Splits when entering the area after killing the Oasis final boss")]
		OasisBossBeaten,
		[Description("Temple of Guidance (Area)"), ToolTip("Splits when entering Temple of Guidance")]
		TempleOfGuidance,
		[Description("Vectron (Area)"), ToolTip("Splits when entering Vectron")]
		Vectron,
		[Description("Windy Plains (Area)"), ToolTip("Splits when entering Windy Plains")]
		WindyPlains,
		[Description("Yarrow (Area)"), ToolTip("Splits when entering Yarrow")]
		Yarrow,
	}
}