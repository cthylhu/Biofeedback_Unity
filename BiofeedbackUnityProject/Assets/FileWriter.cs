using UnityEngine;
using System.Collections;
using System.IO;


/// <summary>
/// Simple file writer that puts data files in a known foder with a known suffix.
/// </summary>
public class FileWriter
{
	static public string dataPath = @"../Data/";
	static public string dataSuffix = @".txt";
	
	static public void TxtSaveByStr(string saveName, string txtStr)
	{
		string path = dataPath + saveName + dataSuffix;
		// This text is added only once to the file. 
		if (!File.Exists(path)) 
		{
			// Create a file to write to. 
			using (StreamWriter swrite = File.CreateText(path)) 
			{}
		}		
		// This text is always added, making the file longer over time if it is not deleted. 
		using (StreamWriter swrite = File.AppendText(path)) 
		{
			swrite.WriteLine(txtStr);
		}
	}
	
}
