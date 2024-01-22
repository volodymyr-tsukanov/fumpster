using System;
using System.IO;
using System.Collections.Generic;
using Fumpster.Files;
using Fumpster.Security;


namespace Fumpster
{
	/// <summary>
	/// MainClass (21/01/2024)
	/// by Volodymyr Tsukanov
	/// </summary>
	class MainClass {
		const int STATUS_EXIT = -1, STATUS_ERROR = -2, STATUS_DEFAULT = 1, STATUS_SECURITY = 3;
		const string PATH_LNK = "template-link.fpr";


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
	/// Dumper (21/01/2024)
	/// by Volodymyr Tsukanov
	/// </summary>
	public class Dumper {
		short version;
		string path;
		FileStream settings;
		List<DumpedFile> files;

		protected internal const short VERSION_ACTUAL = 1, VERSION_LATEST = 1;
		protected internal const string PATH_ROOT = "dumper", PATH_FILES = "files", FILE_SETTINGS = "dumper.fpr", PATH_LNK = "template-link.fpr";

		public string Path { get{ return path; } }


		public Dumper(){
			version = VERSION_ACTUAL;
			path = PATH_ROOT;
			files = new List<DumpedFile>();

			if (!File.Exists(path + "/" + FILE_SETTINGS))
				initialize();
		}
		public Dumper(string path){
			version = VERSION_ACTUAL;
			this.path = path;
			files = new List<DumpedFile>();

			if (!File.Exists(path + "/" + FILE_SETTINGS))
				initialize();
		}


		void initialize(){
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			if (!Directory.Exists(path + "/" + PATH_FILES))
				Directory.CreateDirectory(path + "/" + PATH_FILES);

			string linkPath = "";
			Console.WriteLine("Create shortcut for 'fumper.exe', open it`s properties and insert '$fumpster$' (without ') in comment field of this shortcut");
			while (String.IsNullOrEmpty(linkPath)) {
				Console.Write("Paste path to shortcut here: ");
				linkPath = Console.ReadLine();
				if (!File.Exists(linkPath)) {
					Console.WriteLine("No such link: " + linkPath);
					linkPath = "";
				} else {
					/*if (!File.ReadAllText(linkPath).Contains("Ї")) {
						Console.WriteLine("No 'Ї' found in comments");
						linkPath = "";
					}*/
				}
			}
			File.WriteAllBytes(path + "/" + PATH_LNK, File.ReadAllBytes(linkPath));

			settings = new FileStream(path + "/" + FILE_SETTINGS, FileMode.Create);
			BinaryWriter bw = new BinaryWriter(settings);
			bw.Write(version);
			bw.Close();
		}


		public void DumpFile(string path){
			if (File.Exists(path)) {
				DumpedFile df = new DumpedFile(path);
				files.Add(df);
			}
		}

		public void Restore(long fileId){

		}
		public void Restore(string path){

		}
	}
}
