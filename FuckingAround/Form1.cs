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
		private List<ITurnHaver> Turners;
		private TurnFuckYouFuckThatFuckEverything fuckpiss;
		//private ITurnHaver _currentTurnHaver;
		private ITurnHaver CurrentTurnHaver { get { return fuckpiss.CurrentTurnHaver; } }
		private Being activeBeing { get { return CurrentTurnHaver as Being; } }	//return null if current turn does not belong to a being
		private IEnumerable<Being> Beings { get { return Turners.Where(t => t is Being).Cast<Being>(); } }
		private Menu BeingMenu;
		private MenuItem SkillMenu;
		private TextBox txtbx= new TextBox();
		private event EventHandler MouseHoverChanged;
		private Tile _mouseHover;
		private Tile MMouseHover {
			get { return _mouseHover; }
			set {
				if (value != _mouseHover) {
					_mouseHover = value;
					if(MouseHoverChanged != null)MouseHoverChanged(this, EventArgs.Empty);
				}
			}
		}

		private int TileSetOffsetX = 0;
		private int TileSetOffsetY = 0;


		private event EventHandler tajmEvent;

		delegate void fuckinghellCallback(object sender, EventArgs e);
		private System.Threading.Timer tajmer;

		public Form1() {
			InitializeComponent();
			this.Width = 550;
			DoubleBuffered = true;
			tajmEvent += (s, e) => Invalidate();
			tajmer = new System.Threading.Timer(
				(o) => {
					try {	//TODO something better than try catch?
						this.Invoke(new fuckinghellCallback(tajmEvent), new object[]{o, EventArgs.Empty});
					} catch (ObjectDisposedException) {
						tajmer.Dispose();
					}
				},
				null, 100, 1000/60);
			Disposed += (s, e) => tajmer.Dispose();
			tajmEvent +=
				(s, o) => Refresh();
			tileSet = new TileSet(30, 30);
			MouseMove += (s, e) => MMouseHover = tileSet.SelectTile(e.X - TileSetOffsetX, e.Y - TileSetOffsetY);

			fuckpiss = new TurnFuckYouFuckThatFuckEverything();

			Turners = new List<ITurnHaver>();
			Turners.Add(new Being(1, 5, 5) { Place = tileSet[5, 6], Weapon = new Weapon { Damage = new Damage { PhysDmg = 2 }, Range = 5 } });
			var b1 = new Being(1, 7, 6) { Place = tileSet[10, 10] };
			b1.Skills = new Skill[] { new Blackify(b1), new SpeedupChanneling(b1) };
			Turners.Add(b1);
			var b2 = new Being(2, 8, 7) { Place = tileSet[20, 17] };
			b2.Skills = new Skill[] { new ChannelingSpell(b2, new Blackify(b2), t => () => t, fuckpiss) };
			Turners.Add(b2);

			fuckpiss.AddRange(Turners);
			
			foreach (var t in Turners)
				t.TurnFinished += (s, e) => {
					if (CurrentTurnHaver is ChannelingInstance)
						((ChannelingInstance)CurrentTurnHaver).Do();
					if (activeBeing != null) {
						SkillMenu.MenuItems.Clear();
						foreach (var skill in activeBeing.Skills)
							SkillMenu.MenuItems.Add(
								new MenuItem(skill.Name,
									(s2, e2) => { if (!activeBeing.ActionTaken) activeBeing.SelectedAction = skill; Refresh(); }));
					}
				};

			foreach (var b in Beings) {
				EventHandler fun = (s, e) => b.GraphicMove(5);
				b.MoveStarted += (s, e) => tajmEvent += fun;
				b.MoveFinished += (s, e) => tajmEvent -= fun;
			}

			tileSet.TileClicked += (o, e) => { if (activeBeing != null) activeBeing.Command(this, e); };

			this.MouseClick += (s, e) => {
				//modify e.X and Y for grpahic offset
				tileSet.ClickTile(new MouseEventArgs(e.Button, e.Clicks, e.X - TileSetOffsetX, e.Y - TileSetOffsetY , e.Delta));
				this.Invalidate();
			};

			SkillMenu = new MenuItem("Skills");
			BeingMenu = new MainMenu();
			BeingMenu.MenuItems.Add(new MenuItem("End turn", (s, e) => {
				if(activeBeing != null) activeBeing.EndTurn();
				this.Refresh();
				}));
			BeingMenu.MenuItems.Add(SkillMenu);
			
			Menu = (MainMenu)BeingMenu;

			this.Controls.Add(txtbx);
			txtbx.Multiline = true;
			txtbx.ScrollBars = ScrollBars.Vertical;
			txtbx.Size = new Size(500, 400);
			txtbx.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			ConsoleLoggerHandlerOrWhatever.OnLog += (sender, s) => txtbx.Text += s + "\r\n";
			GameEventLogger.OnNewLog += (s, ge) => txtbx.Text += ge.ToString() + "\r\n";
		}

		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint(e);
			DrawShit(e);
		}

		private void DrawShit(PaintEventArgs e) {
			Graphics graphics = e.Graphics;
			//var grafconatber = graphics.BeginContainer();
			//graphics.TranslateTransform(TileSetOffsetX, TileSetOffsetY);
			
			foreach (var tile in tileSet.AsEnumerable())
				tile.Draw(graphics);
			
			SolidBrush fuckyoubrush = new SolidBrush(Color.FromArgb(128, 0, 0, 255));
			if (activeBeing != null) {
				foreach (var tile in activeBeing.SelectedAction == null
						? tileSet.GetTraversalArea(activeBeing.Place, activeBeing, activeBeing.MovementPoints)
						: activeBeing.Place.GetArea(activeBeing.SelectedAction.Range))
					graphics.FillRectangle(fuckyoubrush, tile.Rectangle);
				fuckyoubrush.Dispose();
			}
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

			foreach (var fuck in fuckpiss.GETCHANNELINGINSTANCES())
				fuck.Draw(graphics);

			if (activeBeing != null && MMouseHover != null && activeBeing.SelectedAction == null) {
				var gfsde = new SolidBrush(Color.Black);
				var fuck = tileSet.GetPath(activeBeing.Place, MMouseHover, activeBeing.GetTraversalCost);
				foreach(var fgdsa in fuck){
					graphics.FillRectangle(gfsde, fgdsa.Rectangle);
				}
			}
		}
	}
}