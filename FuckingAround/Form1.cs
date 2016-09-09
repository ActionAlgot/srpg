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

		public Form1() {
			InitializeComponent();

			tileSet = new TileSet(30, 30);
			Beings = new Queue<Being>();
			Beings.Enqueue(new Being(5) { Place = tileSet[5, 5] });
			Beings.Enqueue(new Being(6) { Place = tileSet[10, 10] });
			Beings.Enqueue(new Being(7) { Place = tileSet[20, 17] });

			foreach (var b in Beings)
				b.TurnFinished += (s, e) => Beings.Enqueue(Beings.Dequeue());

			tileSet.TileClicked += (o, e) => this.activeBeing.Command(this, e);

			this.MouseClick += (s, e) => {
				tileSet.ClickTile(e);
				this.Refresh();
			};
			_OnPaint += (s, e) => {
				foreach (var tile in tileSet.AsEnumerable())
					tile.Draw(((PaintEventArgs)e).Graphics); };
		}

		public event EventHandler _OnPaint;
		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint(e);
			if(_OnPaint != null)
				_OnPaint(this, e);
			DrawShit();
		}

		private void DrawShit() {
			Graphics graphics = this.CreateGraphics();

			SolidBrush fuckyoubrush = new SolidBrush(Color.Blue);
			foreach (var tile in tileSet.GetShit(activeBeing.Place, activeBeing.MovementPoints))
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

			graphics.Dispose();
		}
	}
}