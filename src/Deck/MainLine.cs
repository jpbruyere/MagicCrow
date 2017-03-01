namespace MagicCrow
{	
	class MainLine
	{
		public int count = 0;
		public string name = "unamed";
		public string code = "";
		public string ExpansionImg {
			get { return "#MagicCrow.images.expansions." + code + ".svg"; }
		}
		public override string ToString ()
		{
			return code + ":" + name;
		}
		public MagicCard Card {
			get { 
				MagicCard c = null;
				return MagicData.TryLoadCard (name, ref c) ? c : null;
			}
		}
	}
}
