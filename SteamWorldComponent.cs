using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
namespace LiveSplit.SteamWorldDig2 {
	public class SteamWorldComponent : IComponent {
		public string ComponentName { get { return "SteamWorld Dig 2 Autosplitter"; } }
		public TimerModel Model { get; set; }
		public IDictionary<string, Action> ContextMenuControls { get { return null; } }
		internal static string[] keys = { "CurrentSplit", "State", "Area", "Pos", "GameTime" };
		private Dictionary<string, string> currentValues = new Dictionary<string, string>();
		private SteamWorldMemory mem;
		private int currentSplit = -1, state = 0, lastLogCheck = 0;
		private bool hasLog = false;
		private SteamWorldSettings settings;
		private HashSet<SplitName> splitsDone = new HashSet<SplitName>();
		private static string LOGFILE = "_SteamWorld2.log";

		public SteamWorldComponent(LiveSplitState state) {
			mem = new SteamWorldMemory();
			settings = new SteamWorldSettings();
			foreach (string key in keys) {
				currentValues[key] = "";
			}

			if (state != null) {
				Model = new TimerModel() { CurrentState = state };
				Model.InitializeGameTime();
				Model.CurrentState.IsGameTimePaused = true;
				state.OnReset += OnReset;
				state.OnPause += OnPause;
				state.OnResume += OnResume;
				state.OnStart += OnStart;
				state.OnSplit += OnSplit;
				state.OnUndoSplit += OnUndoSplit;
				state.OnSkipSplit += OnSkipSplit;
			}
		}

		public void GetValues() {
			if (!mem.HookProcess()) { return; }

			if (Model != null) {
				HandleSplits();
			}

			LogValues();
		}
		private void HandleSplits() {
			bool shouldSplit = false;
			string currentArea = mem.Area();
			PointF pos = mem.Position();

			if (currentSplit == -1) {
				double gameTime = mem.GameTime();
				shouldSplit = currentArea == "west_desert" && gameTime > 0 && gameTime < 2 && PositionNear(pos, 2340, 5910);
			} else if (Model.CurrentState.CurrentPhase == TimerPhase.Running) {
				if (currentSplit + 1 < Model.CurrentState.Run.Count) {
					foreach (SplitName split in settings.Splits) {
						if (splitsDone.Contains(split)) { continue; }

						switch (split) {
							case SplitName.Archaea: shouldSplit = currentArea == "archaea_1"; break;
							case SplitName.Device0: shouldSplit = currentArea == "firetemple_cave_generator"; break;
							case SplitName.Device1: shouldSplit = currentArea == "temple_of_guidance_2_cave_generator"; break;
							case SplitName.Device2: shouldSplit = currentArea == "yarrow_cave_generator"; break;
							case SplitName.Device3: shouldSplit = currentArea == "firetemple_cave_generator2"; break;
							case SplitName.EasternTemple: shouldSplit = currentArea == "fire_temple"; break;
							case SplitName.ElMachino: shouldSplit = currentArea == "el_machino"; break;
							case SplitName.Hookshot: shouldSplit = currentArea == "the_hub_cave_grapplinghook" && PositionNear(pos, 1380, 3630); break;
							case SplitName.IgnitionAxe: shouldSplit = currentArea == "firetemple_cave_flamer" && PositionNear(pos, 6540, 3270); break;
							case SplitName.Jackhammer: shouldSplit = currentArea == "archaea_cave_jackhammer" && PositionNear(pos, 1380, 3990); break;
							case SplitName.JetEngine: shouldSplit = currentArea == "archaea_cave_vectron_entrance" && PositionNear(pos, 4980, 5910); break;
							case SplitName.Oasis: shouldSplit = currentArea == "the_hub"; break;
							case SplitName.OasisBoss: shouldSplit = currentArea == "the_hub_cave_boss"; break;
							case SplitName.OasisBossBeaten: shouldSplit = currentArea == "the_hub_cave_boss_saving_rusty"; break;
							case SplitName.PressureBomb: shouldSplit = currentArea == "archaea_cave_pressurebomb" && PositionNear(pos, 2580, 4230); break;
							case SplitName.RamjetVigor: shouldSplit = currentArea == "yarrow_cave_steampack_slayer" && PositionNear(pos, 4620, 6390); break;
							case SplitName.SprintHydraulics: shouldSplit = currentArea == "temple_of_guidance" && PositionNear(pos, 1390, 5550); break;
							case SplitName.TempleOfGuidance: shouldSplit = currentArea == "temple_of_guidance"; break;
							case SplitName.UltraCompositeArmor: shouldSplit = currentArea == "firetemple_cave_armor" && PositionNear(pos, 1800, 2910); break;
							case SplitName.Vectron: shouldSplit = currentArea == "vectron_1"; break;
							case SplitName.WindyPlains: shouldSplit = currentArea == "east_desert"; break;
							case SplitName.Yarrow: shouldSplit = currentArea == "yarrow"; break;
						}

						if (shouldSplit) {
							splitsDone.Add(split);
							break;
						}
					}
				} else if (currentArea == "el_machino" && pos.X > -0.1 && pos.X < 0.1 && pos.Y > -0.1 && pos.Y < 0.1) {
					shouldSplit = true;
				}
			}

			HandleSplit(shouldSplit, false);
		}
		private bool PositionNear(PointF pos, float x, float y) {
			return x - 20 < pos.X && pos.X < x + 20 && y - 10 < pos.Y && pos.Y < y + 10;
		}
		private void HandleGameTimes() {
			if (currentSplit >= 0 && currentSplit <= Model.CurrentState.Run.Count) {
				TimeSpan gameTime = TimeSpan.FromSeconds(mem.GameTime());
				if (currentSplit == Model.CurrentState.Run.Count) {
					Time t = Model.CurrentState.Run[currentSplit - 1].SplitTime;
					Model.CurrentState.Run[currentSplit - 1].SplitTime = new Time(t.RealTime, gameTime);
				} else {
					Model.CurrentState.SetGameTime(gameTime);
				}
			}
		}
		private void HandleSplit(bool shouldSplit, bool shouldReset = false) {
			if (shouldReset) {
				if (currentSplit >= 0) {
					Model.Reset();
				}
			} else if (shouldSplit) {
				if (currentSplit < 0) {
					Model.Start();
				} else {
					Model.Split();
				}
			}
		}
		private void LogValues() {
			if (lastLogCheck == 0) {
				hasLog = File.Exists(LOGFILE);
				lastLogCheck = 300;
			}
			lastLogCheck--;

			if (hasLog || !Console.IsOutputRedirected) {
				string prev = "", curr = "";
				foreach (string key in keys) {
					prev = currentValues[key];

					switch (key) {
						case "CurrentSplit": curr = currentSplit.ToString(); break;
						case "State": curr = state.ToString(); break;
						case "Area": curr = mem.Area(); break;
						case "GameTime": curr = mem.GameTime().ToString("0"); break;
						case "Pos":
							PointF pos = mem.Position();
							curr = (pos.X / 100).ToString("0") + "," + (pos.Y / 100).ToString("0"); break;
						default: curr = ""; break;
					}

					if (!prev.Equals(curr)) {
						WriteLogWithTime(key + ": ".PadRight(16 - key.Length, ' ') + prev.PadLeft(25, ' ') + " -> " + curr);

						currentValues[key] = curr;
					}
				}
			}
		}
		public void Update(IInvalidator invalidator, LiveSplitState lvstate, float width, float height, LayoutMode mode) {
			GetValues();
		}
		public void OnReset(object sender, TimerPhase e) {
			currentSplit = -1;
			state = 0;
			splitsDone.Clear();
			Model.CurrentState.IsGameTimePaused = true;
			WriteLog("---------Reset----------------------------------");
		}
		public void OnResume(object sender, EventArgs e) {
			WriteLog("---------Resumed--------------------------------");
		}
		public void OnPause(object sender, EventArgs e) {
			WriteLog("---------Paused---------------------------------");
		}
		public void OnStart(object sender, EventArgs e) {
			currentSplit = 0;
			state = 0;
			splitsDone.Clear();
			Model.CurrentState.IsGameTimePaused = false;
			WriteLog("---------New Game-------------------------------");
		}
		public void OnUndoSplit(object sender, EventArgs e) {
			currentSplit--;
			state = 0;
		}
		public void OnSkipSplit(object sender, EventArgs e) {
			currentSplit++;
			state = 0;
		}
		public void OnSplit(object sender, EventArgs e) {
			currentSplit++;
			state = 0;
			HandleGameTimes();
		}
		private void WriteLog(string data) {
			if (hasLog || !Console.IsOutputRedirected) {
				if (Console.IsOutputRedirected) {
					using (StreamWriter wr = new StreamWriter(LOGFILE, true)) {
						wr.WriteLine(data);
					}
				} else {
					Console.WriteLine(data);
				}
			}
		}
		private void WriteLogWithTime(string data) {
			WriteLog(DateTime.Now.ToString(@"HH\:mm\:ss.fff") + (Model != null && Model.CurrentState.CurrentTime.RealTime.HasValue ? " | " + Model.CurrentState.CurrentTime.RealTime.Value.ToString("G").Substring(3, 11) : "") + ": " + data);
		}

		public Control GetSettingsControl(LayoutMode mode) { return settings; }
		public void SetSettings(XmlNode document) { settings.SetSettings(document); }
		public XmlNode GetSettings(XmlDocument document) { return settings.UpdateSettings(document); }
		public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion) { }
		public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion) { }
		public float HorizontalWidth { get { return 0; } }
		public float MinimumHeight { get { return 0; } }
		public float MinimumWidth { get { return 0; } }
		public float PaddingBottom { get { return 0; } }
		public float PaddingLeft { get { return 0; } }
		public float PaddingRight { get { return 0; } }
		public float PaddingTop { get { return 0; } }
		public float VerticalHeight { get { return 0; } }
		public void Dispose() { }
	}
}