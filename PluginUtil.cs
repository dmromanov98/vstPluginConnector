using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using Jacobi.Vst.Core;
using Jacobi.Vst.Interop.Host;
using Jacobi.Vst.Samples.Host;
using NAudio.Lame;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace rtuitlab_vst
{
	class PluginUtil
	{
		private Dictionary<int, Plugin> Plugins = new Dictionary<int, Plugin>();

		//private void OutPluginInfo(VstPluginContext ctx)
		//{
		//		Debug.Print("Plugin: " + ctx.PluginCommandStub.GetEffectName() + " "
		//		            + ctx.PluginCommandStub.GetProductString()
		//		            + "Version: " + ctx.PluginCommandStub.GetVendorVersion().ToString() + "\n"
		//		            + "***********************************************************************\n");
		//}

		public VstPluginContext OpenPlugin(string pluginPath)
		{

			try
			{
				HostCommandStub hostCmdStub = new HostCommandStub();
				hostCmdStub.PluginCalled += new EventHandler<PluginCalledEventArgs>(HostCmdStub_PluginCalled);

				VstPluginContext ctx = VstPluginContext.Create(pluginPath, hostCmdStub);

				// add custom data to the context
				ctx.Set("PluginPath", pluginPath);
				ctx.Set("HostCmdStub", hostCmdStub);

				// actually open the plugin itself
				ctx.PluginCommandStub.Open();
				Plugin plugin = new Plugin(ctx.PluginInfo.PluginID, ctx.PluginCommandStub.GetProductString(), ctx,
					GetParameters(ctx));
				Plugins.Add(ctx.PluginInfo.PluginID, plugin);
				Debug.Print("Plugin: " + pluginPath + " Loaded");
				return ctx;
			}
			catch (Exception e)
			{
				Debug.Print(e.Message);
			}

			return null;
		}

		private List<Parameter> GetParameters(VstPluginContext pluginContext)
		{
			List<Parameter> parameters = new List<Parameter>();
			for (int i = 0; i < pluginContext.PluginInfo.ParameterCount; i++)
			{
				Parameter parameter = new Parameter(
					i,
					pluginContext.PluginCommandStub.GetParameterName(i),
					pluginContext.PluginCommandStub.GetParameter(i),
					pluginContext.PluginCommandStub.GetParameterDisplay(i));
				parameters.Add(parameter);
			}

			return parameters;
		}

		public void ReleaseAllPlugins()
		{
			foreach (int key in Plugins.Keys)
			{
				// dispose of all (unmanaged) resources
				Plugins[key].PluginContext.Dispose();
			}

			Plugins.Clear();
		}

		private void HostCmdStub_PluginCalled(object sender, PluginCalledEventArgs e)
		{
			HostCommandStub hostCmdStub = (HostCommandStub) sender;

			// can be null when called from inside the plugin main entry point.
			if (hostCmdStub.PluginContext.PluginInfo != null)
			{
				Debug.WriteLine("Plugin " + hostCmdStub.PluginContext.PluginInfo.PluginID + " called:" + e.Message);
			}
			else
			{
				Debug.WriteLine("The loading Plugin called:" + e.Message);
			}
		}

		//public void OutFullPluginInfo(VstPluginContext PluginContext)
		//{
		//	// plugin product
		//	Debug.Print("Plugin Name: "+ PluginContext.PluginCommandStub.GetEffectName());
		//	Debug.Print("Product: " + PluginContext.PluginCommandStub.GetProductString());
		//	Debug.Print("Vendor: " + PluginContext.PluginCommandStub.GetVendorString());
		//	Debug.Print("Vendor Version: " + PluginContext.PluginCommandStub.GetVendorVersion().ToString());
		//	Debug.Print("Vst Support: " + PluginContext.PluginCommandStub.GetVstVersion().ToString());
		//	Debug.Print("Plugin Category: " + PluginContext.PluginCommandStub.GetCategory().ToString());

		//	// plugin info
		//	Debug.Print("Flags: " + PluginContext.PluginInfo.Flags.ToString());
		//	Debug.Print("Plugin ID: " + PluginContext.PluginInfo.PluginID.ToString());
		//	Debug.Print("Plugin Version: " + PluginContext.PluginInfo.PluginVersion.ToString());
		//	Debug.Print("Audio Input Count: " + PluginContext.PluginInfo.AudioInputCount.ToString());
		//	Debug.Print("Audio Output Count: "+ PluginContext.PluginInfo.AudioOutputCount.ToString());
		//	Debug.Print("Initial Delay: " + PluginContext.PluginInfo.InitialDelay.ToString());
		//	Debug.Print("Program Count: " + PluginContext.PluginInfo.ProgramCount.ToString());
		//	Debug.Print("Parameter Count: " + PluginContext.PluginInfo.ParameterCount.ToString());
		//	Debug.Print("Tail Size: " + PluginContext.PluginCommandStub.GetTailSize().ToString());

		//	// can do
		//	Debug.Print("CanDo: " + VstPluginCanDo.Bypass+" "+ PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Bypass)).ToString());
		//	Debug.Print("CanDo: " + VstPluginCanDo.MidiProgramNames + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.MidiProgramNames)).ToString());
		//	Debug.Print("CanDo: " + VstPluginCanDo.Offline + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Offline)).ToString());
		//	Debug.Print("CanDo: " + VstPluginCanDo.ReceiveVstEvents + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ReceiveVstEvents)).ToString());
		//	Debug.Print("CanDo: " + VstPluginCanDo.ReceiveVstMidiEvent + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ReceiveVstMidiEvent)).ToString());
		//	Debug.Print("CanDo: " + VstPluginCanDo.ReceiveVstTimeInfo + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ReceiveVstTimeInfo)).ToString());
		//	Debug.Print("CanDo: " + VstPluginCanDo.SendVstEvents + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.SendVstEvents)).ToString());
		//	Debug.Print("CanDo: " + VstPluginCanDo.SendVstMidiEvent + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.SendVstMidiEvent)).ToString());
		//	Debug.Print("CanDo: " + VstPluginCanDo.ConformsToWindowRules + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ConformsToWindowRules)).ToString());
		//	Debug.Print("CanDo: " + VstPluginCanDo.Metapass + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Metapass)).ToString());
		//	Debug.Print("CanDo:" + VstPluginCanDo.MixDryWet + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.MixDryWet)).ToString());
		//	Debug.Print("CanDo:" + VstPluginCanDo.Multipass + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Multipass)).ToString());
		//	Debug.Print("CanDo:" + VstPluginCanDo.NoRealTime + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.NoRealTime)).ToString());
		//	Debug.Print("CanDo:" + VstPluginCanDo.PlugAsChannelInsert + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.PlugAsChannelInsert)).ToString());
		//	Debug.Print("CanDo:" + VstPluginCanDo.PlugAsSend + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.PlugAsSend)).ToString());
		//	Debug.Print("CanDo:" + VstPluginCanDo.SendVstTimeInfo + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.SendVstTimeInfo)).ToString());
		//	Debug.Print("CanDo:" + VstPluginCanDo.x1in1out + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x1in1out)).ToString());
		//	Debug.Print("CanDo:" + VstPluginCanDo.x1in2out + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x1in2out)).ToString());
		//	Debug.Print("CanDo:" + VstPluginCanDo.x2in1out + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x2in1out)).ToString());
		//	Debug.Print("CanDo:" + VstPluginCanDo.x2in2out + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x2in2out)).ToString());
		//	Debug.Print("CanDo:" + VstPluginCanDo.x2in4out + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x2in4out)).ToString());
		//	Debug.Print("CanDo:" + VstPluginCanDo.x4in2out + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x4in2out)).ToString());
		//	Debug.Print("CanDo:" + VstPluginCanDo.x4in4out + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x4in4out)).ToString());
		//	Debug.Print("CanDo:" + VstPluginCanDo.x4in8out + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x4in8out)).ToString());
		//	Debug.Print("CanDo:" + VstPluginCanDo.x8in4out + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x8in4out)).ToString());
		//	Debug.Print("CanDo:" + VstPluginCanDo.x8in8out + " " + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x8in8out)).ToString());
		//}


		public void DeletePlugin(int id)
		{
			if (Plugins[id].PluginContext != null)
			{
				Plugins[id].PluginContext.Dispose();
				Plugins.Remove(id);
			}
		}

		public string GetPluginInfoByJson(int id)
		{
			string output = JsonConvert.SerializeObject(Plugins[id]);
			return output;
		}

		public void ChangeParametersValue(string jsonPlugin)
		{
			Plugin plugin = JsonConvert.DeserializeObject<Plugin>(jsonPlugin);

			foreach (Parameter parameter in plugin.Parameters)
			{
				//Console.WriteLine(float.Parse(parameter.Value, CultureInfo.InvariantCulture.NumberFormat));
				Plugins[plugin.Id].PluginContext.PluginCommandStub.SetParameter(parameter.Id, parameter.Value);
				//Console.WriteLine(JsonConvert.SerializeObject(Plugins[plugin.Id].Parameters,Formatting.Indented));
			}

			Plugins[plugin.Id].Parameters = GetParameters(Plugins[plugin.Id].PluginContext);

		}

		public string GetListOfPlugins()
		{
			string output = JsonConvert.SerializeObject(Plugins, Formatting.Indented);
			return output;
		}

		private float[] ConvertByteToFloat16(byte[] array)
		{
			float[] floatArr = new float[array.Length / 2];
			for (int i = 0; i < floatArr.Length; i++)
			{
				if (BitConverter.IsLittleEndian)
				{
					Array.Reverse(array, i * 2, 2);
				}

				floatArr[i] = (float) (BitConverter.ToInt16(array, i * 2) / 32767f);
			}

			return floatArr;
		}

		public void AudioProcessing(byte[] audio)
		{
			VstPluginContext pluginContext = Plugins[1381135922].PluginContext;
			//foreach (var plugin in Plugins.Values)
			//{
			//	pluginContext = plugin.PluginContext;
			//}
			// plugin does not support processing audio
			if ((pluginContext.PluginInfo.Flags & VstPluginFlags.CanReplacing) == 0)
			{
				Debug.Print("This plugin does not process any audio.");
				return;
			}

			//TODO : converting byte array to float array
			//Console.WriteLine(audio.Length);
			//var floatArray = audio.Select(b => (float)Convert.ToDouble(b)).ToArray();
			//var floatArray = new float[audio.Length / sizeof(float)];
			//Buffer.BlockCopy(audio, 0, floatArray, 0, audio.Length);
			var floatArray = ConvertByteToFloat16(audio);
			//var floatArray2 = audio.Select(b => (float)Convert.ToDouble(b)).ToArray();

			int inputCount = pluginContext.PluginInfo.AudioInputCount;
			Debug.Print(pluginContext.PluginCommandStub.GetProductString() + " inputCount = " + inputCount);
			int outputCount = pluginContext.PluginInfo.AudioOutputCount;
			Debug.Print(pluginContext.PluginCommandStub.GetProductString() + " outputCount = " + inputCount);
			int blockSize = floatArray.Length;
			Console.WriteLine("Bytes = " + audio.Length + " Floats = " + blockSize);

			// wrap these in using statements to automatically call Dispose and cleanup the unmanaged memory.
			using (VstAudioBufferManager inputMgr = new VstAudioBufferManager(inputCount, blockSize))
			{
				using (VstAudioBufferManager outputMgr = new VstAudioBufferManager(outputCount, blockSize))
				{
					Console.WriteLine(inputMgr.BufferCount + " " + inputMgr.BufferSize);
					foreach (VstAudioBuffer buffer in inputMgr)
					{
						//buffer = ;
						Random rnd = new Random((int) DateTime.Now.Ticks);

						for (int i = 0; i < blockSize; i++)
						{
							buffer[i] = floatArray[i];
						}
					}

					pluginContext.PluginCommandStub.SetBlockSize(blockSize);
					pluginContext.PluginCommandStub.SetSampleRate(44100f);

					//VstAudioBuffer input = new VstAudioBuffer(floatArray,blockSize,true);
					VstAudioBuffer[] inputBuffers = inputMgr.ToArray();
					VstAudioBuffer[] outputBuffers = outputMgr.ToArray();

					pluginContext.PluginCommandStub.MainsChanged(true);
					pluginContext.PluginCommandStub.StartProcess();
					pluginContext.PluginCommandStub.ProcessReplacing(inputBuffers, outputBuffers);
					pluginContext.PluginCommandStub.StopProcess();
					pluginContext.PluginCommandStub.MainsChanged(false);

					//for (int i = 0; i < inputBuffers.Length && i < outputBuffers.Length; i++)
					//{
					Console.WriteLine(outputBuffers[0].SampleCount + " " + inputBuffers[0].SampleCount);
					float[] array = new float[outputBuffers[0].SampleCount];
					for (int j = 0; j < blockSize; j++)
					{
						array[j] = outputBuffers[0][j];
						//if (inputBuffers[i][j] != outputBuffers[i][j])
						//{
						//	if (outputBuffers[i][j] != 0.0)
						//	{
						//		Debug.Print("The plugin has processed the audio.");
						//		Console.WriteLine(outputBuffers.Length+" "+outputBuffers[i]);
						//		return;
						//	}
						//}
						//Console.WriteLine(inputBuffers[i][j] + " " + outputBuffers[i][j]);
					}
					var pcm = new byte[array.Length * 2];
					int sampleIndex = 0,
						pcmIndex = 0;

					while (sampleIndex < array.Length)
					{
						var outsample = (short)(array[sampleIndex] * short.MaxValue);
						pcm[pcmIndex] = (byte)(outsample & 0xff);
						pcm[pcmIndex + 1] = (byte)((outsample >> 8) & 0xff);

						sampleIndex++;
						pcmIndex += 2;
					}
					//}
					//var byteArray = new byte[outputBuffers[0].SampleCount * 4];
					//Buffer.BlockCopy(array, 0, byteArray, 0, byteArray.Length);
					//var byteArray = new byte[array.Length * 2];
					//Buffer.BlockCopy(array, 0, byteArray, 0, byteArray.Length);
					//Console.WriteLine(byteArray.Length);
					//var byteArray = array.Select(f => Convert.ToByte(f)).ToArray();
					File.WriteAllBytes("Audio/ffffa20.mp3", pcm);
					Debug.Print("The plugin has passed the audio unchanged to its outputs.");
				}
			}
		}

		public void AudioProcessing(float[] audio)
		{
			VstPluginContext pluginContext = Plugins[1381135922].PluginContext;
			foreach (var plugin in Plugins.Values)
			{
				pluginContext = plugin.PluginContext;
			}

			//plugin does not support processing audio
			if ((pluginContext.PluginInfo.Flags & VstPluginFlags.CanReplacing) == 0)
			{
				Debug.Print("This plugin does not process any audio.");
				return;
			}


			int inputCount = pluginContext.PluginInfo.AudioInputCount;
			Debug.Print(pluginContext.PluginCommandStub.GetProductString() + " inputCount = " + inputCount);
			int outputCount = pluginContext.PluginInfo.AudioOutputCount;
			Debug.Print(pluginContext.PluginCommandStub.GetProductString() + " outputCount = " + inputCount);
			int blockSize = audio.Length;

			using (VstAudioBufferManager inputMgr = new VstAudioBufferManager(inputCount, blockSize))
			{
				using (VstAudioBufferManager outputMgr = new VstAudioBufferManager(outputCount, blockSize))
				{
					Console.WriteLine(inputMgr.BufferCount + " " + inputMgr.BufferSize);
					foreach (VstAudioBuffer buffer in inputMgr)
					{
						//buffer = ;
						Random rnd = new Random((int) DateTime.Now.Ticks);

						for (int i = 0; i < blockSize; i++)
						{
							buffer[i] = audio[i];
						}
					}

					pluginContext.PluginCommandStub.SetBlockSize(blockSize);
					pluginContext.PluginCommandStub.SetSampleRate(44100f);

					VstAudioBuffer[] inputBuffers = inputMgr.ToArray();
					VstAudioBuffer[] outputBuffers = outputMgr.ToArray();

					pluginContext.PluginCommandStub.MainsChanged(true);
					pluginContext.PluginCommandStub.StartProcess();
					pluginContext.PluginCommandStub.ProcessReplacing(inputBuffers, outputBuffers);
					pluginContext.PluginCommandStub.StopProcess();
					pluginContext.PluginCommandStub.MainsChanged(false);
					float[] array = new float[outputBuffers[0].SampleCount];
					//for (int i = 0; i < inputBuffers.Length && i < outputBuffers.Length; i++)
					//{

						for (int j = 0; j < blockSize; j++)
						{
							array[j] = outputBuffers[0][j];
							//if (inputBuffers[i][j] != outputBuffers[i][j])
							//{
							//	if (outputBuffers[i][j] != 0.0)
							//	{
							//		Debug.Print("The plugin has processed the audio.");
							//		Console.WriteLine(outputBuffers.Length + " " + outputBuffers[i]);
							//		return;
							//	}
							//}

							//Console.WriteLine(inputBuffers[i][j] + " " + outputBuffers[i][j]);
						}
					//}

					var pcm = new byte[array.Length * 2];
					int sampleIndex = 0,
						pcmIndex = 0;

					while (sampleIndex < array.Length)
					{
						var outsample = (short) (audio[sampleIndex] * short.MaxValue);
						pcm[pcmIndex] = (byte) (outsample & 0xff);
						pcm[pcmIndex + 1] = (byte) ((outsample >> 8) & 0xff);

						sampleIndex++;
						pcmIndex += 2;
					}

					//WaveFileWriter writer = new WaveFileWriter("example.mp3", this.);

					using (FileStream bytetoimage = File.Create("Audio/Music.wav"))
					{
						bytetoimage.Write(pcm, 0, pcm.Length);//pass byte array here
					}

					using (var retMs = new MemoryStream())
					using (var ms = new MemoryStream(pcm))
					using (var rdr = new WaveFileReader(ms))
					using (var wtr = new LameMP3FileWriter(retMs, rdr.WaveFormat, 128))
					{
						rdr.CopyTo(wtr);
					}

					File.WriteAllBytes("Audio/someSong.mp3", pcm);
					Debug.Print("The plugin has passed the audio unchanged to its outputs.");
				}
			}
		}
	}
}

