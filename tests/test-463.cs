using System.ComponentModel;

public class PropertySorter : ExpandableObjectConverter {
}

[TypeConverter(typeof(PathItemBase.TypeConverter))]
class PathItemBase {
	internal class TypeConverter : PropertySorter {
	}
}

class X {
	static void Main () {}
}
