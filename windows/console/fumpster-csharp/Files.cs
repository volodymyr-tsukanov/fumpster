using System;
using System.IO;
using System.IO.Compression;
using Fumpster;


namespace Fumpster.Files
{
	/// <summary>
	/// DumpedFile (19/02/2024)
	/// by Volodymyr Tsukanov
	/// </summary>
	public class DumpedFile {
		short reputation, reputation_max;
		long id;
		string sourcePath;
		Status status;
		Dumper dumper;

		protected internal const short REPUTATION_RESTORED = -7098, REPUTATION_DUMPED_DEFAULT = 10, REPUTATION_MAX = 100;
		protected internal const int REPUTATION_PERCENT_OLD = 25, REPUTATION_PERCENT_NEW = 80;

		public short Reputation { get{ return reputation; } }
		public short ReputationMax { get{ return reputation_max; } set{ reputation_max = value <= REPUTATION_MAX ? value : REPUTATION_MAX; } }
		public int ReputationPercent { get{ return (int)((float)(reputation) / reputation_max * 100); } }
		public Status ReputationStatus { get{ return status; } }
		public long Id { get{ return id; } }
		public string SorcePath { get{ return sourcePath; } }
		public bool IsDumped { get{ return reputation > 0; } }


		public DumpedFile(string data, bool fromData, Dumper dumper){
			string[] s = data.Split(';');
			id = long.Parse(s[0]);
			reputation = short.Parse(s[1]);
			reputation_max = short.Parse(s[2]);
			sourcePath = s[3];
			status = (Status)int.Parse(s[4]);
			this.dumper = dumper;
		}
		public DumpedFile(string filePath, Dumper dumper){
			this.id = Math.Abs(filePath.GetHashCode());
			this.reputation = REPUTATION_RESTORED;
			this.reputation_max = REPUTATION_DUMPED_DEFAULT;
			this.sourcePath = filePath;
			this.dumper = dumper;
		}
		public DumpedFile(string filePath, short reputationMax, Dumper dumper){
			this.id = Math.Abs(filePath.GetHashCode());
			this.reputation = REPUTATION_RESTORED;
			ReputationMax = reputationMax;
			this.sourcePath = filePath;
			this.dumper = dumper;
		}


		protected internal void setStatus(Status status){
			this.status = status;
		}


		public void Dump(){
			if (IsDumped) {
				reputation--;

				int percent = ReputationPercent;
				if (percent <= 0)
					Delete();
				else if (percent < REPUTATION_PERCENT_OLD) {
					dumper.DumperCompressor.Transform(this, Status.OLD);
				} else if (reputation < REPUTATION_PERCENT_NEW){
					dumper.DumperCompressor.Transform(this, Status.NORMAL);
				}
			} else {
				reputation = reputation_max;
				dumper.DumperCompressor.Compress(this);
			}
		}

		public void Restore(){
			if (IsDumped) {
				dumper.DumperCompressor.Decompress(this);
				reputation = REPUTATION_RESTORED;
			}
		}

		public void Delete(){
			dumper.DumperCompressor.Remove(this);
			dumper.DumperFiles.Remove(this);
			dumper.Save();
		}


		public override string ToString(){
			return id + ";" + reputation + ";" + reputation_max + ";" + sourcePath + ";" + (int)status;
		}


		public enum Status {NEW, NORMAL, OLD}
	}


	/// <summary>
	/// Compressor (15/02/2024)
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
			using (FileStream input = new FileStream(dumpedFile.SorcePath, FileMode.Open)) {
				switch (dumpedFile.ReputationStatus) {
				case DumpedFile.Status.OLD:
					if (File.Exists(dumper.Path + "/" + Dumper.PATH_DATA + "/dca" + dumper.FileExtestion))
						using (FileStream output = new FileStream(dumper.Path + "/" + Dumper.PATH_DATA + "/dca" + dumper.FileExtestion, FileMode.Append)) {
							BinaryWriter bw = new BinaryWriter(output);
							bw.Write(dumpedFile.Id);
							bw.Write(input.Length);
							byte[] buffer = new byte[8192];
							int n = 0;
							while ((n = input.Read(buffer, 0, buffer.Length)) != -1)
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
							while ((n = input.Read(buffer, 0, buffer.Length)) != -1)
								bw.Write(buffer, 0, n);
							bw.Close();
						}
					using (FileStream output = new FileStream(dumper.Path + "/" + Dumper.PATH_DATA + "/dca" + dumper.FileExtestion, FileMode.Open))
						CompressFile(input, output, CompressionMode.Compress, CompressionLevel.Optimal);
					break;
				case DumpedFile.Status.NORMAL:
					using (FileStream output = new FileStream(dumper.Path + "/" + Dumper.PATH_DATA + "/" + dumpedFile.Id.ToString(), FileMode.Create))
						CompressFile(input, output, CompressionMode.Compress, CompressionLevel.Fastest);
					break;
				case DumpedFile.Status.NEW:
					using (FileStream output = new FileStream(dumper.Path + "/" + Dumper.PATH_DATA + "/" + dumpedFile.Id.ToString(), FileMode.Create))
						input.CopyTo(output);
					break;
				}
			}
		}

		public void Decompress(DumpedFile dumpedFile){
			using (FileStream output = new FileStream(dumpedFile.SorcePath, FileMode.OpenOrCreate)) {
				switch (dumpedFile.ReputationStatus) {
				case DumpedFile.Status.OLD:
					if (File.Exists(dumper.Path + "/" + Dumper.PATH_DATA + "/dca" + dumper.FileExtestion)) {
						using (FileStream input = new FileStream(dumper.Path + "/" + Dumper.PATH_DATA + "/dca" + dumper.FileExtestion, FileMode.Open))
						using (FileStream tmp = new FileStream(dumper.Path + "/" + Dumper.PATH_DATA + "/dca" + Dumper.EXTENSION_TEMP, FileMode.Create))
							CompressFile(input, tmp, CompressionMode.Decompress, CompressionLevel.Optimal);
					}
					break;
				case DumpedFile.Status.NORMAL:
					using (FileStream input = new FileStream(dumper.Path + "/" + Dumper.PATH_DATA + "/" + dumpedFile.Id.ToString(), FileMode.Open))
						CompressFile(input, output, CompressionMode.Decompress, CompressionLevel.Fastest);
					break;
				case DumpedFile.Status.NEW:
					using (FileStream input = new FileStream(dumper.Path + "/" + Dumper.PATH_DATA + "/" + dumpedFile.Id.ToString(), FileMode.Open))
						input.CopyTo(output);
					break;
				}
			}
		}

		public void Transform(DumpedFile dumpedFile, DumpedFile.Status statusTo){
			switch (statusTo) {
			case DumpedFile.Status.NEW:
				using (FileStream input = new FileStream(dumper.Path + "/" + Dumper.PATH_DATA + "/" + dumpedFile.Id.ToString(), FileMode.Open)) {
					switch (dumpedFile.ReputationStatus) {
					case DumpedFile.Status.NORMAL:
						using (FileStream output = new FileStream(dumper.Path + "/" + Dumper.PATH_DATA + "/" + dumpedFile.Id.ToString(), FileMode.Open))
							CompressFile(input, output, CompressionMode.Compress, CompressionLevel.Fastest);
						break;
					case DumpedFile.Status.OLD:
						break;
					}
				}
				Console.Write("Transformed from new -> normal");
				break;
			case DumpedFile.Status.NORMAL:
				switch (dumpedFile.ReputationStatus) {
				case DumpedFile.Status.NEW:
					break;
				case DumpedFile.Status.OLD:
					break;
				}
				break;
			case DumpedFile.Status.OLD:
				switch (dumpedFile.ReputationStatus) {
				case DumpedFile.Status.NORMAL:
					break;
				case DumpedFile.Status.NEW:
					break;
				}
				break;
			}
			dumpedFile.setStatus(statusTo);
		}

		public void Remove(DumpedFile dumpedFile){

		}
	}
}
