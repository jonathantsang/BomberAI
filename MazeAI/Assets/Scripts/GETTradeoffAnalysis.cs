using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class GETTradeoffAnalysis : MonoBehaviour {

	void Start () {
		WWWForm form = new WWWForm();
		form.AddField( "name", "value" );
		Dictionary<string, string> headers = new Dictionary<string, string>();

		byte[] pData = Encoding.ASCII.GetBytes(LoadResourceTextfile("movement.json").ToCharArray());

		// Add a custom header to the request.
		// In this case a basic authentication to access a password protected resource.
		headers["Method"] = "POST";
		headers["Authorization"] = "Basic " + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes( "3a0cadaf-fd9a-409d-a1c8-2732953bd3af" + ":" + "3iq0Qn2iDdzc"));
		headers["Content-Type"] = "application/json";

		string url = "https://gateway.watsonplatform.net/tradeoff-analytics/api/v1/dilemmas";
		Debug.Log ("start");
		WWW www = new WWW(url, pData, headers);
		StartCoroutine(WaitForRequest(www));

	}

	IEnumerator WaitForRequest(WWW www)
	{
		yield return www;

		// check for errors
		if (www.error == null)
		{	
			Debug.Log("WWW Ok!: " + www.text);
		} else {
			Debug.Log("WWW Error: "+ www.error);
		}    
	}

	public static string LoadResourceTextfile(string path)
	{

		string filePath = path.Replace(".json", "");
		Debug.Log (filePath);

		TextAsset targetFile = Resources.Load<TextAsset>(filePath);
		Debug.Log (targetFile);

		return targetFile.text;
	}


}
