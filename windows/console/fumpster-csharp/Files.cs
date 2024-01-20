using System;
using System.IO;


namespace Fumpster.Files
{
	/// <summary>
	/// DumpedFile (20/01/2024)
	/// by Volodymyr Tsukanov
	/// </summary>
	public class DumpedFile {
		string path, sourcePath;

		public string Path { get{ return path; } set{ path = value; } }
		public string SorcePath { get{ return sourcePath; } }
		public bool IsDumped { get{ return !path.Equals(sourcePath); } }


		public DumpedFile(string data){

		}
		public DumpedFile(string filePath){
			this.path = filePath;
			this.sourcePath = filePath;
		}


		public void Dump(){
			if (IsDumped) {

			} else {
				path = "";
			}
		}

		public void Restore(){
			if (IsDumped) {
				path = sourcePath;
			} else {

			}
		}


		public override string ToString(){

		}
	}
}
