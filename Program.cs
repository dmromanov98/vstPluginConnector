using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using rtuitlab_vst;
using System.Threading;
using NAudio.Lame;
using NAudio.Wave;


namespace rtuitlab_vst_framework
{
	class Program
	{
		static void Main(string[] args)
		{
			PluginUtil pluginUtil = new PluginUtil();
			pluginUtil.OpenPlugin(@"Plugins\RoughRider2.dll");
			pluginUtil.OpenPlugin(@"Plugins\TAL-Reverb-4-64.dll");
			//pluginUtil.ChangeParametersValue();
			//Console.WriteLine(pluginUtil.GetListOfPlugins());
			var url = "http://localhost:5000/";
			var listener = new HttpListener();
			listener.Prefixes.Add(url);

			while (true)
			{
				listener.Start();
				var context = listener.GetContext();
				var request = context.Request;
				if (request.HttpMethod.Equals("GET"))
				{
					if (request.Url.Equals(url + "getPlugins"))
						Task.Factory.StartNew(() =>
						{
							var response = context.Response;
							response.AddHeader("Content-Type", "application/json");
							var responseString = pluginUtil.GetListOfPlugins();
							var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
							response.ContentLength64 = buffer.Length;
							var output = response.OutputStream;
							output.Write(buffer, 0, buffer.Length);
							output.Close();
						});

				}
				else if (request.HttpMethod.Equals("POST"))
				{
					if (request.Url.Equals(url + "changePluginsParameters"))
					{
						Task.Factory.StartNew(() =>
						{
							var response = context.Response;
							string body;
							using (var reader = new StreamReader(request.InputStream))
								body = reader.ReadToEnd();

							pluginUtil.ChangeParametersValue(body);
							var buffer = System.Text.Encoding.UTF8.GetBytes("");
							response.ContentLength64 = buffer.Length;
							response.StatusCode = 200;
							var output = response.OutputStream;
							output.Write(buffer, 0, buffer.Length);
							output.Close();
						});
					}
					else if (request.Url.Equals(url + "audioProcessing"))
					{
						Task.Factory.StartNew(() =>
						{
							//var bytes = File.ReadAllBytes("Audio/Adele - Hello.mp3");
							var response = context.Response;
							//string body;
							//using (var reader = new StreamReader(request.InputStream))
							//	body = reader.ReadToEnd();
							//pluginUtil.AudioProcessing(bytes);
							//var floatArray2 = new float[bytes.Length / sizeof(float)];
							//Buffer.BlockCopy(bytes, 0, floatArray2, 0, floatArray2.Length);
							//var mp3 = new Mp3FileReader("Audio/Adele - Hello.mp3");
							//byte[] buffer = new byte[reader.Length];
							//int read = reader.Read(buffer, 0, buffer.Length);
							//short[] sampleBuffer = new short[read / 2];
							//Buffer.BlockCopy(buffer, 0, sampleBuffer, 0, read);
							//ISampleProvider audio = new AudioFileReader("Audio/Adele - Hello.mp3");
							//float[] arr = new float[1024];
							//audio.Read(arr, 1024, 1);
							//var byteArray = new byte[floatArray2.Length * sizeof(float)];
							//Buffer.BlockCopy(floatArray2, 0, byteArray, 0, byteArray.Length);
							//var floatArray = bytes.Select(b => (float)Convert.ToDouble(b)).ToArray();
							//var byteArray = floatArray.Select(f => Convert.ToByte(f)).ToArray();

							float[] floatBuffer;
							using (MediaFoundationReader media = new MediaFoundationReader("Audio/Eminem - Venom (Official Music Video).mp3"))
							{
								int _byteBuffer32_length = (int)media.Length * 2;
								int _floatBuffer_length = _byteBuffer32_length / sizeof(float);

								IWaveProvider stream32 = new Wave16ToFloatProvider(media);
								WaveBuffer _waveBuffer = new WaveBuffer(_byteBuffer32_length);
								stream32.Read(_waveBuffer, 0, (int)_byteBuffer32_length);
								floatBuffer = new float[_floatBuffer_length];

								for (int i = 0; i < _floatBuffer_length; i++)
								{
									floatBuffer[i] = _waveBuffer.FloatBuffer[i];
								}
							}



							var pcm = new byte[floatBuffer.Length * 2];
							int sampleIndex = 0,
								pcmIndex = 0;

							while (sampleIndex < floatBuffer.Length)
							{
								var outsample = (short)(floatBuffer[sampleIndex] * short.MaxValue);
								pcm[pcmIndex] = (byte)(outsample & 0xff);
								pcm[pcmIndex + 1] = (byte)((outsample >> 8) & 0xff);

								sampleIndex++;
								pcmIndex += 2;
							}
							Console.WriteLine("Here");

							//pluginUtil.AudioProcessing(floatBuffer);
							using (MemoryStream ms = new MemoryStream(pcm))
							//{
							//	// Construct the sound player
							//	SoundPlayer player = new SoundPlayer(ms);
							//	Console.WriteLine("Playing");
							//	player.Play();
							//	Console.WriteLine("End Playing");
							//}

							using (FileStream fs = File.Create("myFile.wav"))
							{
								fs.Write(pcm, 0, pcm.Length);
							}

							//using (var writer = new LameMP3FileWriter(mp3FileName, reader.WaveFormat, bitRate))
							//	reader.CopyTo(writer);
							//File.WriteAllBytes("Audio/eqwad.mp3", pcm);
							//Console.WriteLine(floatBuffer.Length);

							//pluginUtil.AudioProcessing(floatBuffer);

							//Console.WriteLine("Bytes = "+new FloatByteMp3().GetBytes("Audio/Adele - Hello.mp3").Length);
							//Console.WriteLine("Floats = "+new FloatByteMp3().GetFloat("Audio/Adele - Hello.mp3").Length);

							//if (byteArray.Equals(bytes)) Console.WriteLine(true);
							//else
							//{
							//	Console.WriteLine(false);
							//}

							//File.WriteAllBytes("Audio/a10.mp3", bytes);
							var buffer = System.Text.Encoding.UTF8.GetBytes("");
							
							response.ContentLength64 = buffer.Length;
							response.StatusCode = 200;
							var output = response.OutputStream;
							output.Write(buffer, 0, buffer.Length);
							output.Close();
						});
					}
				}
			}

		}
	}


}
