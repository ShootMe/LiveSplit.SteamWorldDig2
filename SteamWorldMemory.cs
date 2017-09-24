using LiveSplit.Memory;
using System;
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
			return Program.Read<double>(Program.MainModule.BaseAddress, 0x61F028, 0x0, 0x10, 0x60, 0x10);
		}
		public string Area() {
			return Program.ReadAscii((IntPtr)Program.Read<uint>(Program.MainModule.BaseAddress, 0x61F028, 0x0, 0x10, 0x60, 0x30));
		}
		public PointF Position() {
			float px = Program.Read<float>(Program.MainModule.BaseAddress, 0x61F028, 0x0, 0x10, 0x80, 0x19c);
			float py = Program.Read<float>(Program.MainModule.BaseAddress, 0x61F028, 0x0, 0x10, 0x80, 0x1a0);
			return new PointF(px, py);
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
		[Description("Sprint Hydraulics (Skill)"), ToolTip("Splits when obtaining the Sprint Hydraulics")]
		SprintHydraulics,
		[Description("Pressure Bomb (Skill)"), ToolTip("Splits when obtaining the Pressure Bomb")]
		PressureBomb,
		[Description("Jackhammer (Skill)"), ToolTip("Splits when obtaining the Jackhammer")]
		Jackhammer,
		[Description("Hookshot (Skill)"), ToolTip("Splits when obtaining the Hookshot")]
		Hookshot,
		[Description("Ignition Axe (Skill)"), ToolTip("Splits when obtaining the Ignition Axe")]
		IgnitionAxe,
		[Description("Jetpack (Skill)"), ToolTip("Splits when obtaining the Jetpack")]
		JetEngine,
		[Description("Ramjet Vigor (Skill)"), ToolTip("Splits when obtaining the Ramjet Vigor upgrade")]
		RamjetVigor,
		[Description("Ultra Composite Armor (Skill)"), ToolTip("Splits when obtaining the Ultra Composite Armor")]
		UltraCompositeArmor,

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
		[Description("Oasis Boss (Area)"), ToolTip("Splits when entering the Oasis final boss room")]
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