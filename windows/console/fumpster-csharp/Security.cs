using System;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;


namespace Fumpster.Security
{
	public class Signature {
		string path;


		public Signature(){
			path = Process.GetCurrentProcess().MainModule.FileName;
			Console.WriteLine(path);
		}
		public Signature(string path){
			this.path = path;
		}


		public bool Verify(){
			bool result = false;

			if (File.Exists(path)) {
				try {
					X509Certificate2 cert = new X509Certificate2(path);

					using (FileStream fs = new FileStream(path, FileMode.Open)) {
						HashAlgorithm hashAlgorithm = HashAlgorithm.Create("SHA256");
						byte[] hash = hashAlgorithm.ComputeHash(fs);

						Console.ReadKey();
						Console.WriteLine(BitConverter.ToString(hash));
						Console.WriteLine(cert.SignatureAlgorithm.FriendlyName);
						Console.WriteLine(BitConverter.ToString(cert.GetCertHash()));

						result = true;
						int i = hash.Length-1;
						while(result && i >= 0){
							result = cert.GetCertHash()[i] == hash[i];
						}
					}

					// Print the results
					Console.WriteLine("Issuer: " + cert.Issuer);
					Console.WriteLine("SerialNumber: " + cert.GetSerialNumberString());
				} catch (Exception e) {
					result = false;
					Console.WriteLine("Error: " + e.Message + e.StackTrace);
				}
			}
			
			return result;
		}
	}
}
