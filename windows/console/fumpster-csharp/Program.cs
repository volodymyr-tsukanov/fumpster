using System;
using System.IO;
using System.Collections.Generic;
using Fumpster.Files;
using Fumpster.Security;


namespace Fumpster
{
	/// <summary>
	/// MainClass (20/01/2024)
	/// by Volodymyr Tsukanov
	/// </summary>
	class MainClass {
		public static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");

			Signature signature = new Signature("C:/Users/VladiMr/Desktop/git/fumpster/windows/console/fumpster-csharp/bin/Debug/CryptIO.exe");
			Console.Write(signature.Verify());
		}
	}


	/// <summary>
	/// Dumper (21/01/2024)
	/// by Volodymyr Tsukanov
	/// </summary>
	public class Dumper {
		string path;
		FileStream settings;
		List<DumpedFile> files;

		protected internal const string PATH_ROOT = "dumper", PATH_FILES = "files", FILE_SETTINGS = "dumper.fpr";

		public string Path { get{ return path; } }


		public Dumper(){
			path = PATH_ROOT;

			if (!File.Exists(path + "/" + FILE_SETTINGS))
				initialize();
		}
		public Dumper(string path){
			this.path = path;

			if (!File.Exists(path + "/" + FILE_SETTINGS))
				initialize();
		}


		void initialize(){
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			if (!Directory.Exists(path + "/" + PATH_FILES))
				Directory.CreateDirectory(path + "/" + PATH_FILES);

			settings = new FileStream(path + "/" + FILE_SETTINGS, FileMode.Create);
			BinaryWriter bw = new BinaryWriter(settings);

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
