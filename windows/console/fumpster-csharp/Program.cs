using System;
using System.IO;
using System.Collections.Generic;
using Fumpster.Files;
using Fumpster.Security;


namespace Fumpster
{
	/// <summary>
	/// MainClass (22/01/2024)
	/// by Volodymyr Tsukanov
	/// </summary>
	class MainClass {
		const int STATUS_EXIT = -1, STATUS_ERROR = -2, STATUS_DEFAULT = 1, STATUS_SECURITY = 3;


		public static void Main(string[] args)
		{
			int status;
			string prevTitle = Console.Title;
			style();

			Console.WriteLine("Fumster");

			Signature signature = new Signature();
			if (!signature.Verify()) {
				//Console.WriteLine("! Program got modifications !");
				//Environment.Exit(STATUS_SECURITY);
			}

			Dumper dumper = new Dumper();

			dumper.DumpFile("1.txt");
			dumper.DumpFile("1.png");
			Console.ReadLine();
			dumper.Restore(new FileInfo("1.txt").FullName);

			if (args.Length == 0) {

			} else {

			}
		}


		static void style(){
			Console.Title = "Fumpster";
			Console.BackgroundColor = ConsoleColor.DarkGray;
			Console.ForegroundColor = ConsoleColor.Gray;
			try {
				Console.SetWindowSize(70, 27);
			} catch (ArgumentOutOfRangeException e) {
				Console.Beep(777, 777);
			}
		}
	}


	/// <summary>
	/// Dumper (10/02/2024)
	/// by Volodymyr Tsukanov
	/// </summary>
	public class Dumper {
		short version;
		string path, fileExtention;
		Compressor compressor;
		List<DumpedFile> files;

		protected internal const short VERSION_ACTUAL = 1, VERSION_LATEST = 1;
		protected internal const string PATH_ROOT = "dumper", PATH_DATA = "data", FILE_SETTINGS = "dumper.fpr",
										EXTENSION_ACTUAL = ".fmr", EXTENSION_TEMP = ".tmp";

		public string Path { get{ return path; } }
		public Compressor DumperCompressor { get{ return compressor; } }
		protected internal List<DumpedFile> DumperFiles { get{ return files; } }


		public Dumper(){
			version = VERSION_ACTUAL;
			path = PATH_ROOT;
			fileExtention = EXTENSION_ACTUAL;
			initialize();
		}
		public Dumper(string path){
			version = VERSION_ACTUAL;
			this.path = path;
			fileExtention = EXTENSION_ACTUAL;
			initialize();
		}


		void initialize(){
			compressor = new Compressor(this);
			files = new List<DumpedFile>();

			if (File.Exists(path + "/" + FILE_SETTINGS))
				Load();
			else {
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
				if (!Directory.Exists(path + "/" + PATH_DATA))
					Directory.CreateDirectory(path + "/" + PATH_DATA);
				Save();
			}
		}


		protected internal void Save(){
			using (FileStream settings = new FileStream(path + "/" + FILE_SETTINGS, FileMode.Create))
			using (BinaryWriter bw = new BinaryWriter(settings)) {
				bw.Write(version);
				if (files.Count == 0)
					bw.Write("NULL");
				else {
					string fls = files[0].ToString();
					for (int i = 1; i < files.Count; i++)
						fls += (char) 30 + files[i].ToString();
					bw.Write(fls);
				}
			}
		}
		protected internal void Load(){
			using (FileStream settings = new FileStream(path + "/" + FILE_SETTINGS, FileMode.Create))
			using (BinaryReader br = new BinaryReader(settings)) {
				version = br.ReadInt16();
				string fls = br.ReadString();
				files.Clear();
				if (!fls.Equals("NULL"))
					foreach (string f in fls.Split((char)30)) {
						DumpedFile df = new DumpedFile(f, true, this);
						files.Add(df);
					}
			}
		}


		public void DumpFile(string path){
			if (File.Exists(path)) {
				DumpedFile df = new DumpedFile(path, this);
				df.Dump();
			}
		}

		public void Restore(long fileId){
			DumpedFile df = files.Find(x => x.Id == fileId);
			if (df != null)
				df.Restore();
		}
		public void Restore(string path){
			DumpedFile df = files.Find(x => path.Equals(x.SorcePath));
			if (df != null)
				df.Restore();
		}
	}
}
