///////////////////////////////////////////////////////////////////////////////
//
// Based on 
// (C) 2006 SRC, LLC  -   All rights reserved
//
///////////////////////////////////////////////////////////////////////////////
//
// Module: OmniBusSequence.cpp
//
///////////////////////////////////////////////////////////////////////////////

#pragma once

#include "D:\SDKs\AlteryxSDK\AlteryxPluginAPI\AlteryxPluginSdk.h"

namespace SRC {
	namespace Alteryx {

		class OmniBusSequenceInterface
		{
		private:
			// Unique Tool Id (assigned at construction)
			int const m_nToolId;

			// Engine Interface (pointer assigned at construction)
		    EngineInterface * const m_pEngineInterface;

			// Object need for record Info class
			GenericEngine_Alteryx m_GenericEngine_Alteryx;

			// Our list of downstream tools to which we will send our data.
			PluginOutputConnectionHelper m_outgoingConnections;

			// Our configuration data, as an XML dictionary.  This is provided
			// by Alteryx from the UI portion of our tool during initialization.
			String m_strXmlProperties;

			// We will use this to hold the outgoing data structure.
			RecordInfo m_recordInfoOut;

			// We will use this to hold an outgoing record.
			SmartPointerRefObj<Record> m_pRecordOut;

			// Configuration Paramaeters
			__int64 m_maximumRecords;
			String m_fieldName;
			E_FieldType m_fieldType;

			// It is important that we prevent copying of this object.
			// We will do this by forcing the copy constructors to be private.
			OmniBusSequenceInterface(const OmniBusSequenceInterface &);
			OmniBusSequenceInterface& operator=(const OmniBusSequenceInterface &);

			// - - - - - - - - - - - - - - - - - - - - - - - - - - -

		public:
			// Constructor.  We are provided a tool id and an EngineInterface.
			OmniBusSequenceInterface(int nToolId, EngineInterface *pEngineInterface);

			// - - - - - - - - - - - - - - - - - - - - - - - - - - -

			// All plugins must implement the following five Plugin Interface methods.
			// These do not have to be C++ member functions, nor do they have to follow
			// this naming convention.  However, the SDK provides code for greatly
			// simplifying the tool initialization process which does require following
			// this naming convention exactly.

			// Called when the tool is initialized.  This is when we are presented with
			// the XML configuration dictionary.
			void PI_Init(const wchar_t *pXmlProperties);

			// Called just prior to the destruction of a tool object.  This can be
			// used to clean up, if necessary.
			void PI_Close(bool bHasErrors);

			// Called when another tool is attempting to connect to our input.
			// This gives us the opportunity to reject this connection.
			long PI_AddIncomingConnection(const wchar_t* /*pIncomingConnectionType*/,
				const wchar_t* /*pIncomingConnectionName*/,
				IncomingConnectionInterface* /*r_IncConnInt*/);

			// Called when another tool is attempting to connect to our output.
			// This gives us the opportunity to reject this connection.
			long PI_AddOutgoingConnection(const wchar_t* /*pOutgoingConnectionName*/,
				IncomingConnectionInterface* /*pIncConnInt*/);

			// Called when it is time for us to process our file.  This only happens to
			// tools which have no upstream (input) tools connected to them.  Tools which
			// process data coming in from other tools never need to handle this method.
			long PI_PushAllRecords(__int64 nRecordLimit);
		};

		// End of SRC and SRC.Alteryx namespaces
	}    // namespace Alteryx
}