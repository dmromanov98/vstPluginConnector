using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;

namespace rtuitlab_vst
{
	class Parameter
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public float Value { get; set; }
		public string Unit { get; set; }

		public Parameter(int id, string name, float value, string unit)
		{
			Id = id;
			Name = name;
			Value = value;
			Unit = unit;
		}

	}
}
