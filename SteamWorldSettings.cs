using LiveSplit.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
namespace LiveSplit.SteamWorldDig2 {
	public partial class SteamWorldSettings : UserControl {
		public List<SplitName> Splits { get; private set; }
		private bool isLoading;
		private TimerModel model;
		public SteamWorldSettings(TimerModel model) {
			isLoading = true;
			InitializeComponent();

			Splits = new List<SplitName>();
			isLoading = false;
			this.model = model;
		}

		public bool HasSplit(SplitName split) {
			return Splits.Contains(split);
		}

		private void Settings_Load(object sender, EventArgs e) {
			LoadSettings();
		}
		public void LoadSettings() {
			isLoading = true;
			this.flowMain.SuspendLayout();

			for (int i = flowMain.Controls.Count - 1; i > 0; i--) {
				flowMain.Controls.RemoveAt(i);
			}

			for (int i = 0; i < Splits.Count; i++) {
				SplitName split = Splits[i];
				MemberInfo info = typeof(SplitName).GetMember(split.ToString())[0];
				DescriptionAttribute description = (DescriptionAttribute)info.GetCustomAttributes(typeof(DescriptionAttribute), false)[0];

				SteamWorldSplitSettings setting = new SteamWorldSplitSettings();
				setting.cboName.DataSource = GetAvailableSplits();
				setting.cboName.Text = description.Description;
				SetSplitDescription(setting, i);
				AddHandlers(setting);

				flowMain.Controls.Add(setting);
			}

			isLoading = false;
			this.flowMain.ResumeLayout(true);
		}
		private void AddHandlers(SteamWorldSplitSettings setting) {
			setting.cboName.SelectedIndexChanged += new EventHandler(ControlChanged);
			setting.btnRemove.Click += new EventHandler(btnRemove_Click);
		}
		private void RemoveHandlers(SteamWorldSplitSettings setting) {
			setting.cboName.SelectedIndexChanged -= ControlChanged;
			setting.btnRemove.Click -= btnRemove_Click;
		}
		public void btnRemove_Click(object sender, EventArgs e) {
			for (int i = flowMain.Controls.Count - 1; i > 0; i--) {
				if (flowMain.Controls[i].Contains((Control)sender)) {
					SteamWorldSplitSettings setting = (SteamWorldSplitSettings)((Button)sender).Parent;
					RemoveHandlers(setting);

					flowMain.Controls.RemoveAt(i);
					break;
				}
			}
			UpdateSplits();
		}
		public void ControlChanged(object sender, EventArgs e) {
			UpdateSplits();
		}
		public void UpdateSplits() {
			if (isLoading) return;

			Splits.Clear();
			for (int i = flowMain.Controls.Count - 1; i >= 0; i--) {
				Control c = flowMain.Controls[i];
				if (c is SteamWorldSplitSettings) {
					SteamWorldSplitSettings setting = (SteamWorldSplitSettings)c;
					if (!string.IsNullOrEmpty(setting.cboName.Text)) {
						SplitName split = SteamWorldSplitSettings.GetSplitName(setting.cboName.Text);
						Splits.Insert(0, split);
					}
					SetSplitDescription(setting, i - 1);
				}
			}
		}
		public XmlNode UpdateSettings(XmlDocument document) {
			XmlElement xmlSettings = document.CreateElement("Settings");

			XmlElement xmlSplits = document.CreateElement("Splits");
			xmlSettings.AppendChild(xmlSplits);

			foreach (SplitName split in Splits) {
				XmlElement xmlSplit = document.CreateElement("Split");
				xmlSplit.InnerText = split.ToString();

				xmlSplits.AppendChild(xmlSplit);
			}

			return xmlSettings;
		}
		public void SetSettings(XmlNode settings) {
			Splits.Clear();
			XmlNodeList splitNodes = settings.SelectNodes(".//Splits/Split");
			foreach (XmlNode splitNode in splitNodes) {
				string splitDescription = splitNode.InnerText;
				SplitName split = SteamWorldSplitSettings.GetSplitName(splitDescription);
				Splits.Add(split);
			}
		}
		private void btnAddSplit_Click(object sender, EventArgs e) {
			SteamWorldSplitSettings setting = new SteamWorldSplitSettings();
			List<string> splitNames = GetAvailableSplits();
			setting.cboName.DataSource = splitNames;
			setting.cboName.Text = splitNames[0];
			SetSplitDescription(setting, Splits.Count);
			AddHandlers(setting);

			flowMain.Controls.Add(setting);
			UpdateSplits();
		}
		private List<string> GetAvailableSplits() {
			List<string> splits = new List<string>();
			foreach (SplitName split in Enum.GetValues(typeof(SplitName))) {
				MemberInfo info = typeof(SplitName).GetMember(split.ToString())[0];
				DescriptionAttribute description = (DescriptionAttribute)info.GetCustomAttributes(typeof(DescriptionAttribute), false)[0];
				splits.Add(description.Description);
			}
			if (rdAlpha.Checked) {
				splits.Sort(delegate (string one, string two) {
					return one.CompareTo(two);
				});
			}
			return splits;
		}
		private void radio_CheckedChanged(object sender, EventArgs e) {
			foreach (Control c in flowMain.Controls) {
				if (c is SteamWorldSplitSettings) {
					SteamWorldSplitSettings setting = (SteamWorldSplitSettings)c;
					string text = setting.cboName.Text;
					setting.cboName.DataSource = GetAvailableSplits();
					setting.cboName.Text = text;
				}
			}
		}
		private void flowMain_DragDrop(object sender, DragEventArgs e) {
			UpdateSplits();
		}
		private void flowMain_DragEnter(object sender, DragEventArgs e) {
			e.Effect = DragDropEffects.Move;
		}
		private void flowMain_DragOver(object sender, DragEventArgs e) {
			SteamWorldSplitSettings setting = (SteamWorldSplitSettings)e.Data.GetData(typeof(SteamWorldSplitSettings));
			FlowLayoutPanel destination = (FlowLayoutPanel)sender;
			Point p = destination.PointToClient(new Point(e.X, e.Y));
			var item = destination.GetChildAtPoint(p);
			int index = destination.Controls.GetChildIndex(item, false);
			if (index == 0) {
				e.Effect = DragDropEffects.None;
			} else {
				e.Effect = DragDropEffects.Move;
				int oldIndex = destination.Controls.GetChildIndex(setting);
				if (oldIndex != index) {
					destination.Controls.SetChildIndex(setting, index);
					SetSplitDescription(setting, index - 1);
					SetSplitDescription((SteamWorldSplitSettings)item, oldIndex - 1);
					destination.Invalidate();
				}
			}
		}
		private void SetSplitDescription(SteamWorldSplitSettings setting, int index) {
			if (model != null && index + 1 < model.CurrentState.Run.Count) {
				setting.lblSplit.Text = (index + 1).ToString() + ": " + model.CurrentState.Run[index].Name;
			} else {
				setting.lblSplit.Text = "";
			}
		}
	}
}