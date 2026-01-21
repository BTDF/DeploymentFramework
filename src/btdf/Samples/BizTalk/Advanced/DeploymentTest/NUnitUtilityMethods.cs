// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Net;
using System.Text;

public class NUnitUtility
{
	/// <summary>
	/// Perform an HTTP get with the provided url and return content.
	/// </summary>
	/// <param name="url">Url to get</param>
	/// <param name="exceptionOnFail">Exception thrown if not 200 status code.</param>
	/// <param name="responseString">Out parameter containing returned content.</param>
	/// <returns>HttpStatusCode</returns>
	public static HttpStatusCode HttpGet(
		string url, 
		bool exceptionOnFail, 
		out string responseString)
	{
		HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
		request.Method = "GET";
		HttpWebResponse response = (HttpWebResponse)request.GetResponse();
		StreamReader stream = new StreamReader(response.GetResponseStream());
		responseString = stream.ReadToEnd();

		if(exceptionOnFail)
		{
			if(response.StatusCode != HttpStatusCode.OK)
				throw(new Exception(string.Format("GET Request to url {0} failed: {1}",url,response.StatusCode.ToString())));
		}

		return response.StatusCode;
	}

	/// <summary>
	/// Perform an HTTP post with provided content.
	/// </summary>
	/// <param name="url">Url to post to.</param>
	/// <param name="postContent">Content that should be in the post.</param>
	/// <param name="exceptionOnFail">Exception thrown if not 200/202 status code.</param>
	/// <returns>HttpStatusCode</returns>
	public static HttpStatusCode HttpPost(
		string url, 
		string postContent, 
		out string postResult,
		bool exceptionOnFail)
	{
		HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
		request.Method = "POST";

		Stream stream = request.GetRequestStream();
		UTF8Encoding encoding = new UTF8Encoding();
		byte[] requestBytes = Encoding.UTF8.GetBytes(postContent);
		stream.Write(requestBytes,0,requestBytes.Length);
		stream.Flush();
		stream.Close();

		HttpWebResponse response = (HttpWebResponse)request.GetResponse();

		if(exceptionOnFail)
		{
			if(response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Accepted)
				throw(new Exception(string.Format("Post to url {0} failed: {1}",url,response.StatusCode.ToString())));
		}

		using(StreamReader responseData = new StreamReader(response.GetResponseStream()))
		{
			postResult = responseData.ReadToEnd();
		}
	

		return response.StatusCode;
	}

	/// <summary>
	/// Allows for a look at an event log for entries that match various parameters, within
	/// a time interval back from the present time.
	/// </summary>
	/// <param name="eventLogName">Name of the event log, such as "Application" or "System"</param>
	/// <param name="sourceNames">Array of possible event log source names.</param>
	/// <param name="computersToCheck">Array of computers to check.</param>
	/// <param name="lookBackInterval">Timespan indicating how far back to look in the event log.</param>
	/// <param name="matchingEntries">Set of messages from matching event log entries.</param>
	/// <returns>True of matches are found, false otherwise.</returns>
	public static bool CheckEventLog(
		string eventLogName,
		string[] sourceNames,
		string[] computersToCheck,
		TimeSpan lookBackInterval,
		out string matchingEntries)
	{
		StringBuilder matchedEntries = new StringBuilder();

		for(int i = 0;i<computersToCheck.Length;i++)
		{
			CheckEventLogOnMachine(
				eventLogName,
				sourceNames,
				lookBackInterval,
				matchedEntries,
				computersToCheck[i]);
		}

		matchingEntries = matchedEntries.ToString();
		if(matchedEntries.Length == 0)
			return false;
		else
			return true;
	}

	private static void CheckEventLogOnMachine(
		string eventLogName,
		string[] sourceNames,
		TimeSpan lookBackInterval,
		StringBuilder matchedEntries,
		string machineName)
	{
		int matchCount = 1;
		EventLog eventLog = new EventLog(eventLogName,machineName);
		EventLogEntryCollection logEntries = eventLog.Entries;

		int count = logEntries.Count;
		for(int i = 0;i<count;i++)
		{
			EventLogEntry entry = logEntries[i];
			if(entry.EntryType == EventLogEntryType.Error || 
				entry.EntryType == EventLogEntryType.Warning)
			{
				if((DateTime.Now - entry.TimeGenerated) < lookBackInterval)
				{
					for(int j = 0;j<sourceNames.Length; j++)
					{
						string source = entry.Source;
						if(source.Equals(sourceNames[j]))
						{
							matchedEntries.Append(string.Format("Machine {0} Match {1}: {2}\n",
								machineName,matchCount++,entry.Message));			
						}
					}
				}
			}
		}


	}

}

