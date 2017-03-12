using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace srpg {
	public class SkillNode {
		private List<Mod> _Mods = new List<Mod>();
		public IEnumerable<Mod> Mods { get { return _Mods.AsEnumerable(); } }
		private List<SkillTreePath> _Paths = new List<SkillTreePath>();
		public IEnumerable<SkillTreePath> Paths { get { return _Paths.AsEnumerable(); } }

		public SkillNode(Mod mod) {
			_Mods.Add(mod);
		}
		public SkillNode(IEnumerable<Mod> mods) {
			_Mods.AddRange(mods);
		}

		public void AddPath(SkillTreePath path) {
			_Paths.Add(path);
		}
	}

	public class SkillTreePath {
		public SkillNode Node0 { get; private set; }
		public SkillNode Node1 { get; private set; }
		public bool OneWay { get; private set; }
		public SkillTreePath(SkillNode node0, SkillNode node1)
			: this(node0, node1, false) { }
		public SkillTreePath(SkillNode node0, SkillNode node1, bool oneWay) {
			Node0 = node0;
			Node1 = node1;
			OneWay = oneWay;

			Node0.AddPath(this);
			Node1.AddPath(this);
		}
	}

	public class SkillTree {
		public SkillNode Start { get; private set; }
		private List<SkillNode> _AllNodes = new List<SkillNode>();
		private HashSet<SkillTreePath> _AllPaths = new HashSet<SkillTreePath>();
		public IEnumerable<SkillTreePath> AllPaths { get { return _AllPaths.AsEnumerable(); } }
		public IEnumerable<SkillNode> AllNodes { get { return _AllNodes.AsEnumerable(); } }
		public void AddNode(SkillNode node) {
			_AllNodes.Add(node);
			foreach (var p in node.Paths)
				_AllPaths.Add(p);	}
		public void AddNodes(IEnumerable<SkillNode> nodes) {
			foreach (var n in nodes)
				AddNode(n);
		}

		public SkillTree(SkillNode start) {
			Start = start;
			AddNode(Start);
		}

		public IEnumerable<int> GetNodesForSave(IEnumerable<SkillNode> nodes) {
			for (int i = 0; i < _AllNodes.Count; i++) {
				foreach (var n in nodes) {
					if (_AllNodes[i] == n) {
						yield return i;
						break;
			}	}	}
		}
		public IEnumerable<SkillNode> GetNodes(IEnumerable<int> indexs) {
			return indexs.Select(i => _AllNodes[i]);
		}
	}

	public static class SkillTreeshit {
		private static SkillTree _basic;
		public static SkillTree Basic { get {
				if (_basic == null)
					BuildBasic();
				return _basic;
		}	}
		private static void BuildBasic() {
			_basic = new SkillTree( new SkillNode(new Mod[] {
				new AdditionMod(StatType.Strength, 10),
				new AdditionMod(StatType.Speed, 5),
				new AdditionMod(StatType.HP, 20),
				new AdditionMod(StatType.ChannelingSpeed,  4),
				new ConversionToAdditiveMultiplierMod( StatType.PhysicalDamage, 0.05, StatType.Strength),
				new AdditionMod(StatType.MovementPoints, 5)
			}));
			var nodes = new SkillNode[] {
				/*0*/new SkillNode(new AdditionMod(StatType.HP, 5)),
				/*1*/new SkillNode(new AdditionMod(StatType.Speed, 1)),
				/*2*/new SkillNode(new AdditiveMultiplierMod(StatType.Strength, 0.20)),
				/*3*/new SkillNode(new GainMod(StatType.FireDamage, 0.10, StatType.PhysicalDamage)),
				/*4*/new SkillNode(new ConversionMod(StatType.FireDamage, 0.50, StatType.PhysicalDamage)),
				/*5*/new SkillNode(new AdditionMod(StatType.FireResistance, 0.50))
			};
			new SkillTreePath(Basic.Start, nodes[0]);
			new SkillTreePath(_basic.Start, nodes[1]);
			new SkillTreePath(_basic.Start, nodes[2]);
			new SkillTreePath(nodes[2], nodes[3]);
			new SkillTreePath(nodes[2], nodes[4]);
			new SkillTreePath(nodes[0], nodes[5]);

			Basic.AddNodes(nodes);
		}
	}

	public class SkillTreeFiller {
		public SkillTree Target { get; private set; }
		private Dictionary<SkillNode, bool> TakenDic = new Dictionary<SkillNode, bool>();
		public IEnumerable<SkillNode> Taken {
			get { return TakenDic.Where(kvp => kvp.Value).Select(kvp => kvp.Key); } }
		private Dictionary<SkillNode, bool> AvailableDic = new Dictionary<SkillNode, bool>();
		public IEnumerable<SkillNode> Available {
			get { return AvailableDic.Where(kvp => kvp.Value).Select(kvp => kvp.Key); } }

		public bool Take(SkillNode skN) {
			if (TakenDic.ContainsKey(skN) && TakenDic[skN] == true)
				return false;
			if (AvailableDic.ContainsKey(skN) == false
				|| AvailableDic[skN] == false)
				return false;

			TakenDic[skN] = true;
			AvailableDic[skN] = false;
			foreach (var p in skN.Paths)
				if (p.Node0 == skN)
					AvailableDic[p.Node1] = true;
				else if (!p.OneWay)
					AvailableDic[p.Node0] = true;
			return true;
		}

		public SkillTreeFiller(SkillTree target) {
			Target = target;
			AvailableDic[Target.Start] = true;
			Take(Target.Start);
		}

		public SkillTreeFiller(SkillTreeFillerSave sts) {
			//Target = GetTarget(sts.Target);
			foreach (var n in Target.GetNodes(sts.Taken))
				TakenDic[n] = true;
			foreach (var n in Target.GetNodes(sts.Available))
				AvailableDic[n] = true;
		}

		public class SkillTreeFillerSave {
			public int Target;
			public List<int> Taken = new List<int>();
			public List<int> Available = new List<int>();
		}
	}
}
