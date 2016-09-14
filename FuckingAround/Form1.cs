using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace FuckingAround {
	public partial class Form1 : Form {

		private TileSet tileSet;
		private Queue<Being> Beings;
		private Being activeBeing { get { return Beings.Peek(); } }
		private Menu BeingMenu;
		private MenuItem SkillMenu;
		private TextBox txtbx= new TextBox();

		private int TileSetOffsetX = 0;
		private int TileSetOffsetY = 0;


		public Form1() {
			InitializeComponent();

			tileSet = new TileSet(30, 30);
			Beings = new Queue<Being>();
			Beings.Enqueue(new Being(1, 5) { Place = tileSet[5, 6], Weapon = new Weapon { Damage = 2, Range = 5 } });
			var b1 = new Being(1, 6) { Place = tileSet[10, 10] };
			b1.Skills = new Skill[] { new Blackify(b1) };
			Beings.Enqueue(b1);
			Beings.Enqueue(new Being(2, 7) { Place = tileSet[20, 17] });

			foreach (var b in Beings)
				b.TurnFinished += (s, e) => {
					Beings.Enqueue(Beings.Dequeue());
					SkillMenu.MenuItems.Clear();
					foreach (var skill in activeBeing.Skills)
						SkillMenu.MenuItems.Add(
							new MenuItem(skill.Name,
								(s2, e2) => { if(!activeBeing.ActionTaken) activeBeing.SelectedAction = skill; Refresh(); }));
				};

			tileSet.TileClicked += (o, e) => this.activeBeing.Command(this, e);

			this.MouseClick += (s, e) => {
				//modify e.X and Y for grpahic offset
				tileSet.ClickTile(new MouseEventArgs(e.Button, e.Clicks, e.X - TileSetOffsetX, e.Y - TileSetOffsetY , e.Delta));
				this.Refresh();
			};

			SkillMenu = new MenuItem("Skills");
			BeingMenu = new MainMenu();
			BeingMenu.MenuItems.Add(new MenuItem("End turn", (s, e) => {
				activeBeing.EndTurn();
				this.Refresh();
				}));
			BeingMenu.MenuItems.Add(SkillMenu);
			
			Menu = (MainMenu)BeingMenu;

			this.Controls.Add(txtbx);
			txtbx.Multiline = true;
			txtbx.ScrollBars = ScrollBars.Vertical;
			txtbx.Size = new Size(300, 400);
			txtbx.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			ConsoleLoggerHandlerOrWhatever.OnLog += (sender, s) => txtbx.Text += s + "\r\n";
			GameEventLogger.OnNewLog += (s, ge) => txtbx.Text += ge.ToString() + "\r\n";
		}

		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint(e);
			DrawShit();
		}

		private void DrawShit() {
			Graphics graphics = this.CreateGraphics();
			var grafconatber = graphics.BeginContainer();
			graphics.TranslateTransform(TileSetOffsetX, TileSetOffsetY);

			foreach (var tile in tileSet.AsEnumerable())
				tile.Draw(graphics);

			SolidBrush fuckyoubrush = new SolidBrush(Color.FromArgb(128, 0, 0, 255));
			foreach (var tile in activeBeing.SelectedAction == null ? tileSet.GetTraversalArea(activeBeing.Place, activeBeing, activeBeing.MovementPoints) : activeBeing.Place.GetArea(activeBeing.SelectedAction.Range))
				graphics.FillRectangle(fuckyoubrush, tile.Rectangle);
			fuckyoubrush.Dispose();
			if (tileSet.ClickedTile != null)
				graphics.FillRectangle(tileSet.SelectedBrush, tileSet.ClickedTile.Rectangle);

			Pen lineGridPen = new Pen(Color.Black);
			for (int x = 0; x <= tileSet.XLength; x++)
				graphics.DrawLine(lineGridPen,
					x * Tile.Size,
					0,
					x * Tile.Size,
					tileSet.YLength * Tile.Size);
			for (int y = 0; y <= tileSet.YLength; y++)
				graphics.DrawLine(lineGridPen,
					0,
					y * Tile.Size,
					tileSet.XLength * Tile.Size,
					y * Tile.Size);
			lineGridPen.Dispose();

			foreach (var t in Beings)
				t.Draw(graphics);

			graphics.EndContainer(grafconatber);

			graphics.Dispose();
		}
	}
}