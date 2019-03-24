using System;
using System.Collections.Generic;
using System.Text;
using Jacobi.Vst.Interop.Host;
using Newtonsoft.Json;

namespace rtuitlab_vst
{
	class Plugin
	{
		public int Id { get; set; }
		public string Name { get; set; }

		[JsonIgnore]
		public VstPluginContext PluginContext { get; set; }

		public List<Parameter> Parameters { get; set; }

		public Plugin(int id, string name, VstPluginContext pluginContext, List<Parameter> parameters)
		{
			Id = id;
			Name = name;
			PluginContext = pluginContext;
			Parameters = parameters;
		}
	}
}
