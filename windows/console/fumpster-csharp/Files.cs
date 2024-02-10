using System;
using System.IO;
using System.IO.Compression;
using Fumpster;


namespace Fumpster.Files
{
	/// <summary>
	/// DumpedFile (10/02/2024)
	/// by Volodymyr Tsukanov
	/// </summary>
	public class DumpedFile {
		short reputation;
		long id;
		string sourcePath;
		Dumper dumper;

		protected internal const short REPUTATION_RESTORED = -7018, REPUTATION_DUMPED = 10, REPUTATION_MAX = 100;

		public short Reputation { get{ return reputation; } }
		public long Id { get{ return id; } }
		public string SorcePath { get{ return sourcePath; } }
		public bool IsDumped { get{ return reputation > 0; } }


		public DumpedFile(string data, bool fromData, Dumper dumper){
			string[] s = data.Split(';');
			id = long.Parse(s[0]);
			reputation = short.Parse(s[1]);
			sourcePath = s[2];
			this.dumper = dumper;
		}
		public DumpedFile(string filePath, Dumper dumper){
			this.id = Math.Abs(filePath.GetHashCode());
			this.reputation = REPUTATION_RESTORED;
			this.sourcePath = filePath;
			this.dumper = dumper;
		}
		public DumpedFile(string filePath, short reputation, Dumper dumper){
			this.id = Math.Abs(filePath.GetHashCode());
			this.reputation = REPUTATION_RESTORED;
			this.sourcePath = filePath;
			this.dumper = dumper;
		}


		public void Dump(){
			if (IsDumped) {
				reputation--;
				if (reputation < 1)
					Delete();
			} else {
				reputation = REPUTATION_DUMPED;
				dumper.DumperCompressor.Compress(this);
				File.Delete(sourcePath);
				dumper.DumperFiles.Add(this);
				dumper.Save();
			}
		}

		public void Restore(){
			if (IsDumped) {
				dumper.DumperCompressor.Decompress(this);
				reputation = REPUTATION_RESTORED;
				dumper.DumperFiles.Remove(this);
				dumper.Save();
			}
		}

		public void Delete(){
			dumper.DumperCompressor.Remove(this);
			dumper.DumperFiles.Remove(this);
			dumper.Save();
		}


		public override string ToString(){
			return id + ";" + reputation + ";" + sourcePath;
		}
	}


	/// <summary>
	/// Compressor (10/02/2024)
	/// by Volodymyr Tsukanov
	/// </summary>
	public class Compressor {
		Dumper dumper;


		public Compressor(Dumper dumper){
			this.dumper = dumper;
		}


		void CompressFile(FileStream input, FileStream output, CompressionMode mode, CompressionLevel level){
			if (mode == CompressionMode.Decompress)
				using (GZipStream gzs = new GZipStream(input, mode))
					gzs.CopyTo(output);
			else
				using (GZipStream gzs = new GZipStream(output, level))
					input.CopyTo(gzs);
		}


		public void Compress(DumpedFile dumpedFile){
			int percent = dumpedFile.Reputation / DumpedFile.REPUTATION_DUMPED * 100;

			using (FileStream input = new FileStream(dumpedFile.SorcePath, FileMode.Open)) {
				if (percent < 25) { //old
					if (File.Exists(dumper.Path + "/" + Dumper.PATH_DATA + "/dca" + dumper.FileExtestion))
						using (FileStream output = new FileStream(dumper.Path + "/" + Dumper.PATH_DATA + "/dca" + dumper.FileExtestion, FileMode.Append)) {
							BinaryWriter bw = new BinaryWriter(output);
							bw.Write(dumpedFile.Id);
							bw.Write(input.Length);
							byte[] buffer = new byte[8192];
							int n = 0;
							while((n = input.Read(buffer, 0, buffer.Length)) != -1)
								bw.Write(buffer, 0, n);
							bw.Close();
						}
					else
						using (FileStream output = new FileStream(dumper.Path + "/" + Dumper.PATH_DATA + "/dca" + dumper.FileExtestion, FileMode.Create)) {
							BinaryWriter bw = new BinaryWriter(output);
							bw.Write(dumpedFile.Id);
							bw.Write(input.Length);
							byte[] buffer = new byte[8192];
							int n = 0;
							while((n = input.Read(buffer, 0, buffer.Length)) != -1)
								bw.Write(buffer, 0, n);
							bw.Close();
						}
					using (FileStream output = new FileStream(dumper.Path + "/" + Dumper.PATH_DATA + "/dca" + dumper.FileExtestion, FileMode.Open))
						CompressFile(input, output, CompressionMode.Compress, CompressionLevel.Optimal);
				} else if (percent < 60) { //normal
					using (FileStream output = new FileStream(dumper.Path + "/" + Dumper.PATH_DATA + "/" + dumpedFile.Id.ToString(), FileMode.Create))
						CompressFile(input, output, CompressionMode.Compress, CompressionLevel.Fastest);
				} else { //new
					using (FileStream output = new FileStream(dumper.Path + "/" + Dumper.PATH_DATA + "/" + dumpedFile.Id.ToString(), FileMode.Create))
						input.CopyTo(output);
				}
			}
		}

		public void Decompress(DumpedFile dumpedFile){
			int percent = dumpedFile.Reputation / DumpedFile.REPUTATION_DUMPED * 100;

			using (FileStream output = new FileStream(dumpedFile.SorcePath, FileMode.OpenOrCreate)) {
				if (percent < 25) { //old
					if (File.Exists(dumper.Path + "/" + Dumper.PATH_DATA + "/dca" + dumper.FileExtestion)) {
						using (FileStream input = new FileStream(dumper.Path + "/" + Dumper.PATH_DATA + "/dca" + dumper.FileExtestion, FileMode.Open))
						using (FileStream tmp = new FileStream(dumper.Path + "/" + Dumper.PATH_DATA + "/dca" + Dumper.EXTENSION_TEMP, FileMode.Create))
							CompressFile(input, tmp, CompressionMode.Decompress, CompressionLevel.Optimal);
					}
				} else if (percent < 60) { //normal
					using (FileStream input = new FileStream(dumper.Path + "/" + Dumper.PATH_DATA + "/" + dumpedFile.Id.ToString(), FileMode.Open))
						CompressFile(input, output, CompressionMode.Decompress, CompressionLevel.Fastest);
				} else { //new
					using (FileStream input = new FileStream(dumper.Path + "/" + Dumper.PATH_DATA + "/" + dumpedFile.Id.ToString(), FileMode.Open))
						input.CopyTo(output);
				}
			}
		}

		public void Remove(DumpedFile dumpedFile){

		}
	}
}
