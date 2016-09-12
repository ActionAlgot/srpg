using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {
	public abstract class Skill {
		protected string _name;
		protected int _range;
		public string Name { get { return _name; } }
		public int Range { get { return _range; } }
		public abstract bool Apply(Tile Target);
	}
	public class Blackify : Skill {
		public Blackify() {
			_name = "Blackify";
			_range = 5;
		}

		public override bool Apply(Tile Target) {
			var l = new List<Being>();
			if (Target.Inhabitant != null)
				l.Add(Target.Inhabitant);
			l.AddRange(Target.Adjacent.Where(t => t.Inhabitant != null).Select(t => t.Inhabitant));
			if (l.Any()) {
				foreach (var b in l)
					b.Brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
				return true;
			} else return false;
		}
	}
}
