using System;
using System.Collections.Generic;
using System.Linq;

namespace srpg {
	public class Battle {
		public TileSet TileSet { get; private set; }
		private TurnTracker _TurnTracker = new TurnTracker();
		private ITurnHaver CurrentTurnHaver { get { return _TurnTracker.CurrentTurnHaver; } }
		public Being activeBeing { get { return CurrentTurnHaver as Being; } } //return null if current turn does not belong to a being

		private List<Being> _Beings = new List<Being>();
		public IEnumerable<Being> Beings { get { return _Beings.AsEnumerable(); } }

		private List<ChannelingInstance> _ChannelingInstances = new List<ChannelingInstance>();
		public IEnumerable<ChannelingInstance> ChannelingInstances { get { return _ChannelingInstances.AsEnumerable(); } }

		public event EventHandler TurnStarted;
		private void InvokeTurnStarted(object s, EventArgs e) {
			if (TurnStarted != null) TurnStarted(s, e);
		}

		public event EventHandler<Being.MovedArgs> BeingMoved;
		private void InvokeBeingMoved(object s, Being.MovedArgs e) {
			if (BeingMoved != null) BeingMoved(s, e);
		}

		public void Add(ITurnHaver ith) {
			if (ith is Being) AddB(ith as Being);
			else if (ith is ChannelingInstance) AddCI(ith as ChannelingInstance);

			_TurnTracker.Add(ith);
		}
		public void Remove(ITurnHaver ith) {
			if (ith is ChannelingInstance) RemoveCI(ith as ChannelingInstance);

			_TurnTracker.Remove(ith);
		}

		private void AddB(Being being) {
			_Beings.Add(being);
			being.MoveStarted += InvokeBeingMoved;
		}

		private void AddCI(ChannelingInstance ci) {
			_ChannelingInstances.Add(ci);
		}
		private void RemoveCI(ChannelingInstance ci) {
			_ChannelingInstances.Remove(ci);
			_TurnTracker.Remove(ci);
		}

		public void EndTurn() {
			if (activeBeing != null) activeBeing.EndTurn();
		}

		public Battle() {
			TileSet = new TileSet(30, 30);

			var b = new Being(this, 1, "being0", 5, 5) { Place = TileSet[5, 6] };
			b.AddPassiveSkill(Passives.All[3]);
			b.AddPassiveSkill(Passives.All[4]);
			b.Inventory[0] = new Spear(12);
			new Being(this, 1, "being1", 7, 6) { Place = TileSet[7, 8] };
			new Being(this, 2, "being2", 8, 7) { Place = TileSet[9, 10] };
			
			_TurnTracker.TurnStarted += InvokeTurnStarted;

			TileSet.TileClicked += (o, e) => {
				if (activeBeing != null)
					activeBeing.Command(this, e);
			};
		}
	}
}
