using System.IO;


/// <summary>
/// Simple file writer that puts data files in a known folder with a known suffix.
/// </summary>
public class FileWriter
{
	static public string dataPath = @"../Data/";
	static public string dataSuffix = @".txt";
	
	static public void TxtSaveByStr(string saveName, string txtStr)
	{
		string path = dataPath + saveName + dataSuffix;
		// Text is always added, making the file longer over time if it is not deleted.
		// If the file doesn't exist, it's created.
		using (StreamWriter swrite = File.AppendText(path)) 
		{
			swrite.WriteLine(txtStr);
		}
	}
	
}
