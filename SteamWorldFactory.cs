using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.Reflection;
namespace LiveSplit.SteamWorldDig2 {
	public class SteamWorldFactory : IComponentFactory {
		public string ComponentName { get { return "SteamWorld Dig 2 Autosplitter v" + this.Version.ToString(); } }
		public string Description { get { return "Autosplitter for SteamWorld Dig 2"; } }
		public ComponentCategory Category { get { return ComponentCategory.Control; } }
		public IComponent Create(LiveSplitState state) { return new SteamWorldComponent(state); }
		public string UpdateName { get { return this.ComponentName; } }
		public string UpdateURL { get { return "https://raw.githubusercontent.com/ShootMe/LiveSplit.SteamWorldDig2/master/"; } }
		public string XMLURL { get { return this.UpdateURL + "Components/Updates.xml"; } }
		public Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }
	}
}