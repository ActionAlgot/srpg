using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace srpg {
	public partial class Form1 : Form {

		private Menu BeingMenu;
		private MenuItem SkillMenu;
		private TextBox txtbx = new TextBox();

		private Battle Battle;
		private TileSet tileSet { get { return Battle.TileSet; } }
		private Being activeBeing { get { return Battle.activeBeing; } }

		private event EventHandler MouseHoverChanged;
		private Tile _mouseHover;
		private Tile MMouseHover {
			get { return _mouseHover; }
			set {
				if (value != _mouseHover) {
					_mouseHover = value;
					if (MouseHoverChanged != null) MouseHoverChanged(this, EventArgs.Empty);
				}
			}
		}

		public int TileSize = 15;
		private int TileSetOffsetX = 0;
		private int TileSetOffsetY = 0;

		private event EventHandler tajmEvent;
		delegate void fuckinghellCallback(object sender, EventArgs e);
		private System.Threading.Timer tajmer;

		private void SetSkillMenu(object s, EventArgs e) {
			if (s is Being) {
				Being newBeing = s as Being;
				SkillMenu.MenuItems.Clear();
				foreach (var skill in newBeing.Skills) {
					SkillMenu.MenuItems.Add(
						new MenuItem(
							skill.Name,
							(s2, e2) => {   //TODO don't create a new func every single fucking time
								if (!newBeing.ActionTaken) newBeing.SelectedAction = skill;
								Refresh();
							}
						));
				}
			}
		}

		private Tile GetMousedTile(MouseEventArgs e) {
			int x = (e.X - TileSetOffsetX) / TileSize;
			int y = (e.Y - TileSetOffsetY) / TileSize;
			if (x >= 0 && x < tileSet.XLength && y >= 0 && y < tileSet.YLength)   //within tileset area
				return tileSet[x, y];
			else return null;
		}

		public Form1(Battle battle) {
			InitializeComponent();

			Battle = battle;

			this.Width = 550;
			DoubleBuffered = true;
			tajmEvent += (s, e) => Invalidate();
			MouseMove += (s, e) => MMouseHover = GetMousedTile(e);

			Battle.TurnStarted += SetSkillMenu;
			Battle.BeingMoved += BeginDrawingBeingMove;

			this.MouseClick += (s, e) => {
				var tile = GetMousedTile(e);
				if (tile != null) {
					tileSet.SelectTile(tile);
					this.Invalidate();
				}
			};

			SkillMenu = new MenuItem("Skills");
			BeingMenu = new MainMenu();
			BeingMenu.MenuItems.Add(new MenuItem("End turn", (s, e) => {
				Battle.EndTurn();
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


			tajmer = new System.Threading.Timer(
				(o) => {    //TODO something better than try catch?
					try { this.Invoke(new fuckinghellCallback(tajmEvent), new object[] { o, EventArgs.Empty }); }
					catch (ObjectDisposedException) { tajmer.Dispose(); }
				},
				null, 100, 1000 / 60);
			Disposed += (s, e) => tajmer.Dispose();
		}

		private class MovingBeingStuff {
			public List<Tile> Path;
			public int PathIndex;
			public Rectangle MovingRect;
			
			public MovingBeingStuff(List<Tile> path, Rectangle movingRect) {
				Path = path;
				PathIndex = 0;
				MovingRect = movingRect;
			}
		}
		private Dictionary<Being, MovingBeingStuff> movingBeingStuffs = new Dictionary<Being, MovingBeingStuff>();
		private void BeginDrawingBeingMove(object s, Being.MovedArgs e) {
			Being being = s as Being;
			movingBeingStuffs[being] = new MovingBeingStuff(e.Path, GetRectangle(being.Place));
		}

		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint(e);
			DrawShit(e);
		}
		
		private void DrawShit(PaintEventArgs e) {
			var graphics = e.Graphics;

			foreach (var tile in tileSet.AsEnumerable())
				Draw(tile, graphics);

			//Highlight area available for movement or skill usage
			if (activeBeing != null) {
				var tiles = activeBeing.SelectedAction == null
					? tileSet.GetTraversalArea(activeBeing.Place, activeBeing, activeBeing.MovementPoints)
					: activeBeing.SelectedAction.Range(activeBeing);
				HighlightArea(tiles, graphics);
			}

			//if (tileSet.ClickedTile != null)
			//	currentGraphics.FillRectangle(tileSet.SelectedBrush, GetRectangle(tileSet.ClickedTile));

			DrawLineGrid(graphics);

			foreach (var being in Battle.Beings)
				Draw(being, graphics);

			foreach (var ci in Battle.ChannelingInstances)
				Draw(ci, graphics);

			HighlightPath(graphics);

			graphics = null;
		}

		Rectangle GetRectangle(Tile tile) {
			return new Rectangle(
				tile.X * TileSize,
				tile.Y * TileSize,
				TileSize, TileSize);
		}

		private SolidBrush TileBrush = new SolidBrush(Color.BlanchedAlmond);
		private void Draw(Tile tile, Graphics graphics) {
			graphics.FillRectangle(TileBrush, GetRectangle(tile));
		}

		private SolidBrush BeingBrush = new SolidBrush(Color.Green);
		private SolidBrush DeadBeingBrush = new SolidBrush(Color.DarkOrange);
		private void Draw(Being being, Graphics graphics) {
			var UsedBrush = being.IsAlive ? BeingBrush : DeadBeingBrush;
			if (movingBeingStuffs.ContainsKey(being)
				&& GraphicMoveBeing(being))
				graphics.FillEllipse(UsedBrush, movingBeingStuffs[being].MovingRect);
			else graphics.FillEllipse(UsedBrush, GetRectangle(being.Place));
		}
		private int MoveSpeed = 3;
		private bool GraphicMoveBeing(Being being) {
			var stuff = movingBeingStuffs[being];
			var Path = stuff.Path;
			var PathIndex = stuff.PathIndex;
			var MovingRect = stuff.MovingRect;

			int move = MoveSpeed;   //TODO apply time.delta
			while (true) {
				if (PathIndex+1 >= Path.Count) {
					movingBeingStuffs.Remove(being);
					return false;
				}
				int xDiff = Path[PathIndex].X - Path[PathIndex + 1].X;
				if (xDiff != 0) {
					if (xDiff < 0) {
						MovingRect.X += move;
						if (MovingRect.X >= GetRectangle(Path[PathIndex + 1]).X) {
							move = MovingRect.X - GetRectangle(Path[PathIndex + 1]).X;
							MovingRect.X = GetRectangle(Path[PathIndex + 1]).X;
							PathIndex++;
							continue;
						}
					}
					else {
						MovingRect.X -= move;
						if (MovingRect.X <= GetRectangle(Path[PathIndex + 1]).X) {
							move = -(MovingRect.X - GetRectangle(Path[PathIndex + 1]).X);
							MovingRect.X = GetRectangle(Path[PathIndex + 1]).X;
							PathIndex++;
							continue;
						}
					}
				}
				else {
					int yDiff = Path[PathIndex].Y - Path[PathIndex + 1].Y;
					if (yDiff < 0) {
						MovingRect.Y += move;
						if (MovingRect.Y >= GetRectangle(Path[PathIndex + 1]).Y) {
							move = MovingRect.Y - GetRectangle(Path[PathIndex + 1]).Y;
							MovingRect.Y = GetRectangle(Path[PathIndex + 1]).Y;
							PathIndex++;
							continue;
						}
					}
					else {
						MovingRect.Y -= move;
						if (MovingRect.Y <= GetRectangle(Path[PathIndex + 1]).Y) {
							move = -(MovingRect.Y - GetRectangle(Path[PathIndex + 1]).Y);
							MovingRect.Y = GetRectangle(Path[PathIndex + 1]).Y;
							PathIndex++;
							continue;
						}
					}
				}
				break;
			}
			stuff.MovingRect = MovingRect;
			return true;
		}

		private Pen LineGridPen = new Pen(Color.Black);
		private void DrawLineGrid(Graphics graphics) {
			for (int x = 0; x <= tileSet.XLength; x++)
				graphics.DrawLine(LineGridPen,
					x * TileSize,
					0,
					x * TileSize,
					tileSet.YLength * TileSize);
			for (int y = 0; y <= tileSet.YLength; y++)
				graphics.DrawLine(LineGridPen,
					0,
					y * TileSize,
					tileSet.XLength * TileSize,
					y * TileSize);
		}

		private SolidBrush PathHighlightBrush = new SolidBrush(Color.Black);
		private void HighlightPath(Graphics graphics) {
			if (activeBeing != null && MMouseHover != null && activeBeing.SelectedAction == null) {
				var tiles = tileSet.GetPath(activeBeing.Place, MMouseHover, activeBeing.GetTraversalCost);
				foreach (var tile in tiles) {
					graphics.FillRectangle(PathHighlightBrush, GetRectangle(tile));
				}
			}
		}

		private SolidBrush AreaHighlightBrush = new SolidBrush(Color.FromArgb(128, 0, 0, 255));
		private void HighlightArea(IEnumerable<Tile> tiles, Graphics graphics) {
			foreach (var tile in tiles)
				graphics.FillRectangle(AreaHighlightBrush, GetRectangle(tile));
		}

		private SolidBrush ChannelingBrush = new SolidBrush(Color.Pink);
		private void Draw(ChannelingInstance ci, Graphics graphics) {
			graphics.FillRectangle(ChannelingBrush, GetRectangle(ci.Place));
		}

	}
}
