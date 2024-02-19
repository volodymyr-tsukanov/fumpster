using System;
using System.IO;
using System.Collections.Generic;
using Fumpster.Files;
using Fumpster.Security;


namespace Fumpster
{
	/// <summary>
	/// MainClass (19/02/2024)
	/// by Volodymyr Tsukanov
	/// </summary>
	class MainClass {
		const int STATUS_EXIT = -1, STATUS_ERROR = -2, STATUS_DEFAULT = 1, STATUS_SECURITY = 3;

		static Dumper dumper;


		public static void Main(string[] args)
		{
			int status = STATUS_DEFAULT;
			string prevTitle = Console.Title;
			style();

			Console.WriteLine("Fumster\nv.0.1");

			Signature signature = new Signature();
			if (!signature.Verify()) {
				Console.WriteLine("! Program got modifications !");
				status = STATUS_SECURITY;
				//Environment.Exit(STATUS_SECURITY);
			}

			dumper = new Dumper();
			if (args.Length == 0) {
				while (status >= STATUS_DEFAULT) {
					Console.Write("fmp: ");
					status = command(Console.ReadLine().Split());
				}
			} else {
				status = command(args);
			}

			Environment.Exit(status);
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

		static int command(string[] cmds){
			bool b;
			int output = STATUS_DEFAULT;
			long l;
			try {
				switch (cmds[0]) {
				case "?":
				case "help":
					Console.WriteLine("commands:");
					Console.WriteLine("  help / ?\tshow commands list");
					Console.WriteLine("  dump / d\tmove file to dumper\n    pattern:\t[filePath] [reputation(optional)]");

					break;
				case "dump":
					if(long.TryParse(cmds[1], out l)) b = dumper.DumpFile(l);
					else b = dumper.DumpFile(cmds[1]);
					Console.WriteLine(b);
					break;
				case "restore":
					if(long.TryParse(cmds[1], out l)) b = dumper.RestoreFile(l);
					else b = dumper.RestoreFile(cmds[1]);
					Console.WriteLine(b);
					break;
				case "print":
					Console.WriteLine("Printing dumped files:");
					foreach(DumpedFile df in dumper.DumperFiles)
						Console.WriteLine("Id:" + df.Id + "\tRep:" + df.Reputation + "\tRepM:" + df.ReputationMax + "\tSource:" + df.SorcePath + "\tStatus:" + df.ReputationStatus);
					break;
				default:
					Console.WriteLine("! no such command ! (? for help)");
					break;
				}
			} catch (Exception exc) {
				Console.WriteLine("! Error: " + exc.Message + " !");
				output = STATUS_ERROR;
			}
			return output;
		}
	}


	/// <summary>
	/// Dumper (19/02/2024)
	/// by Volodymyr Tsukanov
	/// </summary>
	public class Dumper {
		bool debug;
		short version;
		string path, fileExtestion;
		Compressor compressor;
		List<DumpedFile> files;

		protected internal const short VERSION_ACTUAL = 1, VERSION_LATEST = 1;
		protected internal const string PATH_ROOT = "dumper", PATH_DATA = "data", FILE_SETTINGS = "dumper.fpr",
										EXTENSION_ACTUAL = ".fmr", EXTENSION_TEMP = ".tmp";

		public bool Debug { get{ return debug; } set{ debug = value; } }
		public string Path { get{ return path; } }
		public Compressor DumperCompressor { get{ return compressor; } }
		protected internal string FileExtestion { get{ return fileExtestion; } }
		protected internal List<DumpedFile> DumperFiles { get{ return files; } }


		public Dumper(){
			debug = true;
			version = VERSION_ACTUAL;
			path = PATH_ROOT;
			fileExtestion = EXTENSION_ACTUAL;
			initialize();
		}
		public Dumper(string path){
			debug = true;
			version = VERSION_ACTUAL;
			this.path = path;
			fileExtestion = EXTENSION_ACTUAL;
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
				Console.WriteLine("--- initialized ---\n(? - for help)");
			}
		}


		protected internal void Save(){
			using (FileStream settings = new FileStream(path + "/" + FILE_SETTINGS, FileMode.Open))
			using (BinaryWriter bw = new BinaryWriter(settings)) {
				bw.Write(version);
				if (files.Count == 0)
					bw.Write("NULL");
				else {
					string fls = files[0].ToString();
					for (int i = 1; i < files.Count; i++)
						fls += (char)29 + files[i].ToString();
					bw.Write(fls);
				}
			}
		}
		protected internal void Load(){
			using (FileStream settings = new FileStream(path + "/" + FILE_SETTINGS, FileMode.Open))
			using (BinaryReader br = new BinaryReader(settings)) {
				version = br.ReadInt16();
				string fls = br.ReadString();
				files.Clear();
				if (!fls.Equals("NULL"))
					foreach (string f in fls.Split((char)29)) {
						DumpedFile df = new DumpedFile(f, true, this);
						files.Add(df);
					}
			}
		}


		public bool DumpFile(long fileId){
			if(debug) Console.Write("Dumping file by id " + fileId);
			DumpedFile df = files.Find(x => x.Id == fileId);
			if (df != null) {
				if(debug) Console.Write(" -> file has been already dumped");
				df.Dump();
				Save();
				if(debug) Console.Write(" >>> ");
				return true;
			}
			return false;
		}
		public bool DumpFile(string path){
			if(debug) Console.Write("Dumping file by path " + path);
			if (path.EndsWith(EXTENSION_ACTUAL)) {
				if(debug) Console.Write(" -> fumpster file -> trying to find file by id >> ");
				long id = long.Parse(File.ReadAllText(path));
				return DumpFile(id);
			} else {
				if(debug) Console.Write(" -> creating new dumped file");
				DumpedFile df = files.Find(x => x.SorcePath == path);
				if (df != null) {
					df.Dump();
				} else {
					df = new DumpedFile(path, this);
					df.Dump();
					files.Add(df);
					File.Delete(path);
				}
				Save();
				if(debug) Console.Write(" >>> ");
				return true;
			}
			return false;
		}

		public bool RestoreFile(long fileId){
			if(debug) Console.Write("Restoring file by id");
			DumpedFile df = files.Find(x => x.Id == fileId);
			if (df != null) {
				df.Restore();
				Console.WriteLine("restore dl:" + files.Remove(df));
				Save();
				return true;
			}
			return false;
		}
		public bool RestoreFile(string path){
			DumpedFile df = files.Find(x => path.Equals(x.SorcePath));
			if (df != null) {
				df.Restore();
				Console.WriteLine("restore dl:" + files.Remove(df));
				Save();
				return true;
			}
			return false;
		}

		public bool DeleteFile(long fileId){
			if(debug) Console.Write("Deleting file by id");
			DumpedFile df = files.Find(x => x.Id == fileId);
			if (df != null) {
				df.Delete();
				return true;
			}
			return false;
		}
	}
}
