using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace rtuitlab_vst_framework
{
	class WorkWithFile: NAudio.Wave.WaveStream
	{
		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}

		public override WaveFormat WaveFormat { get; }
		public override long Length { get; }
		public override long Position { get; set; }

		private WaveFileWriter writer;

		public void StreamMixToDisk(string FileName)
		{
			writer = new WaveFileWriter(FileName, this.WaveFormat);
			//writer.Write(buffer, offset, count);
		}
	}
}
