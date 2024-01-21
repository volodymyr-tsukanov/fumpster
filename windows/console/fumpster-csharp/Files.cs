using System;
using System.IO;


namespace Fumpster.Files
{
	/// <summary>
	/// DumpedFile (21/01/2024)
	/// by Volodymyr Tsukanov
	/// </summary>
	public class DumpedFile {
		short reputation;
		long id;
		string sourcePath;

		protected internal const short REPUTATION_RESTORED = -7018, REPUTATION_DUMPED = 10;

		public short Reputation { get{ return reputation; } }
		public long Id { get{ return id; } }
		public string SorcePath { get{ return sourcePath; } }
		public bool IsDumped { get{ return reputation != REPUTATION_RESTORED; } }


		public DumpedFile(string data, bool fromData){

		}
		public DumpedFile(string filePath){
			this.id = filePath.GetHashCode();
			this.reputation = REPUTATION_RESTORED;
			this.sourcePath = filePath;
		}


		public void Dump(){
			if (IsDumped) {

			} else {
				reputation = REPUTATION_DUMPED;
			}
		}

		public void Restore(){
			if (IsDumped) {
				reputation = REPUTATION_RESTORED;
			} else {

			}
		}


		public override string ToString(){
			return id + ";";
		}
	}
}
