//---------------------------------------------------------------------
// This file is part of the Microsoft BizTalk Server 2009 SDK
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// This source code is intended only as a supplement to Microsoft BizTalk
// Server 2009 release and/or on-line documentation. See these other
// materials for detailed information regarding Microsoft code samples.
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, WHETHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
// PURPOSE.
//---------------------------------------------------------------------

using System;
using System.Collections;
using System.Xml;
using System.IO;
using Microsoft.BizTalk.Bam.EventObservation;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Component.Interop;

namespace  DeploymentFramework.Samples.Bam.PipelineComponents
{
	/// <summary>
    /// Summary description for BamEndToEnd_PipelineComponent.
	/// </summary>
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[ComponentCategory(CategoryTypes.CATID_Validate)]
	[System.Runtime.InteropServices.Guid("0bea5a6e-afb3-49c5-8cd7-c041b8110d05")]
	public class BamEndToEnd_PipelineComponent : IBaseComponent, IComponent, IComponentUI
	{
		public BamEndToEnd_PipelineComponent()
		{
		}

		#region IBaseComponent Members
		public string Description
		{
			get { return("BAM End-to-End Pipeline Component"); }
		}

		public string Name
		{
			get	{ return("BamEndToEnd_PipelineComponent"); }
		}

		public string Version
		{
			get { return("100"); }
		}
		#endregion
	
		#region IComponent Members

		public Microsoft.BizTalk.Message.Interop.IBaseMessage Execute(Microsoft.BizTalk.Component.Interop.IPipelineContext context, Microsoft.BizTalk.Message.Interop.IBaseMessage inMsg)
		{
			EventStream			BAMes;
			IBaseMessageFactory			msgFactory;
			IBaseMessage				outMsg;
			IBaseMessagePart			outBodyPart, outBAMPart;
			XmlDocument					BAMdoc;
			MemoryStream				BAMdocStream;
			string						ActivityID;

			//Create a new, unique activity identifier to use as the ActivityID in BAM and to flow
			//with the message in the new BAM message part.
			ActivityID="seq_"+Guid.NewGuid().ToString();
			
			// --- Send some data using the BAM API ---
			//Create a BAM event stream object and give it a DB connection string
            BAMes = context.GetEventStream();

			//Start the activity record identified by ActivityID in BAM and 
			//update it by setting the "Received" milestone.
			BAMes.BeginActivity("EndToEndActivity",ActivityID);
			BAMes.UpdateActivity("EndToEndActivity",ActivityID,"Received",DateTime.UtcNow);

			//Since this won't be the last update to the activity, 
			//keep the activity "active" by enabling continuation.
			BAMes.EnableContinuation("EndToEndActivity",ActivityID,"Orch1_"+ActivityID);

			//End the session with the activity (even though the activity can still be updated
			//later because we have continuation enabled.
			BAMes.EndActivity("EndToEndActivity",ActivityID);
			BAMes.Flush();

			//----------------------------------------------------------------------
			//-- Since messages are immutable, we must create a new message in order
			//-- to add the BAM part to it.
			//----------------------------------------------------------------------

			//get a message factory to use for creating the new message and it's parts
			msgFactory= context.GetMessageFactory();

			//Create new parts for the message body (taken from the original message) and the new BAM message part
			outBodyPart= msgFactory.CreateMessagePart();
			outBAMPart= msgFactory.CreateMessagePart();
 
			//Copy the original message data stream to the new outMsg body part
			outBodyPart.Data= inMsg.BodyPart.GetOriginalDataStream();
			outBodyPart.PartProperties= inMsg.BodyPart.PartProperties;

			//load the BAM message part schema as an xml document, set the value of Docuemtn ID
			//and use that to populate the BAM message part.
			BAMdoc= new XmlDataDocument();
			BAMdoc.LoadXml("<ns0:BAMPart xmlns:ns0=\"urn:DeploymentFramework.Samples.Bam.Services.BAMPartSchema\" />");
			BAMdoc.DocumentElement.SetAttribute("DocumentID", ActivityID);
			BAMdocStream= new MemoryStream();
			BAMdoc.Save(BAMdocStream);
			BAMdocStream.Seek(0, SeekOrigin.Begin);
			outBAMPart.Data= BAMdocStream;

			//Create the new message object and add the parts to it
			outMsg= msgFactory.CreateMessage();
			outMsg.Context = inMsg.Context;
			outMsg.AddPart(inMsg.BodyPartName, outBodyPart, true);
			outMsg.AddPart("BAMPart", outBAMPart, false);
			
			return(outMsg);
		}

		#endregion
	
		#region IComponentUI Members
		public System.Collections.IEnumerator Validate(object projectSystem)
		{
			return null;
		}

		public System.IntPtr Icon
		{
			get { return(IntPtr.Zero); }
		}
		#endregion

	}
}
