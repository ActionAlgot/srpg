using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {
	public class Battle {
		public TileSet TileSet { get; private set; }
		private List<ITurnHaver> Turners;
		private ITurnHaver CurrentTurnHaver { get { return TurnTracker.CurrentTurnHaver; } }
		public Being activeBeing { get { return CurrentTurnHaver as Being; } } //return null if current turn does not belong to a being
		public IEnumerable<Being> Beings { get { return Turners.Where(t => t is Being).Cast<Being>(); } }   //TODO kill me

		public event EventHandler<Being.MovedArgs> BeingMoved;
		private void InvokeBeingMoved(object s, Being.MovedArgs e) {
			if (BeingMoved != null) BeingMoved(s, e);
		}

		public void EndTurn() {
			if (activeBeing != null) activeBeing.EndTurn();
		}

		public Battle() {
			TileSet = new TileSet(30, 30);

			Turners = new List<ITurnHaver>();
			Turners.Add(new Being(1, 5, 5) { Place = TileSet[5, 6] });
			((Being)Turners[0]).AddPassiveSkill(Passives.All[3]);
			((Being)Turners[0]).AddPassiveSkill(Passives.All[4]);
			((Being)Turners[0]).Inventory[0] = new Spear(12);
			var b1 = new Being(1, 7, 6) { Place = TileSet[7, 8] };
			Turners.Add(b1);
			var b2 = new Being(2, 8, 7) { Place = TileSet[9, 10] };
			Turners.Add(b2);

			foreach (var b in Beings)
				b.MoveStarted += InvokeBeingMoved;

			TurnTracker.AddRange(Turners);

			TileSet.TileClicked += (o, e) => {
				if (activeBeing != null)
					activeBeing.Command(this, e);
			};
		}
	}
}
