// OmniBusSequence.cpp : Defines the entry point for the console application.
//

#include "OmniBusSequence.h"
#include "stdafx.h"

namespace SRC {
	namespace Alteryx {
		OmniBusSequenceInterface::OmniBusSequenceInterface(
			int nToolId,
			EngineInterface *pEngineInterface) : 
			m_nToolId(nToolId),
			m_pEngineInterface(pEngineInterface),
			m_outgoingConnections(nToolId, pEngineInterface)
		{
			m_recordInfoOut.SetGenericEngine(&m_GenericEngine_Alteryx);
			m_maximumRecords = 10000;
			m_fieldName = L"RowId";
			m_fieldType = E_FT_Int64;
		}

		void OmniBusSequenceInterface::PI_Init(const wchar_t *pXmlProperties)
		{
			m_pEngineInterface->OutputMessage(m_nToolId, STATUS_Info, L"PI_Init");
			
			// Cache the XML data.
			m_strXmlProperties = pXmlProperties;

			// Is it acceptable?  If not, then we should throw an error.
			MiniXmlParser::TagInfo tagConfig;
			if (!MiniXmlParser::FindXmlTag(pXmlProperties, tagConfig, L"Configuration"))
				throw Error(L"This tool has not been configured.");

			MiniXmlParser::TagInfo numberOfRecordsTag;
			if (MiniXmlParser::FindXmlTag(tagConfig, numberOfRecordsTag, L"NumberOfRecords"))
			{
				auto countTagXML = MiniXmlParser::GetInnerXml(numberOfRecordsTag);
				if (!countTagXML.IsEmpty()) {
					m_maximumRecords = countTagXML.ConvertToInt64();
				}
			}

			MiniXmlParser::TagInfo fieldTypeTag;
			if (MiniXmlParser::FindXmlTag(tagConfig, fieldTypeTag, L"FieldType"))
			{
				auto fieldTypeTagXML = MiniXmlParser::GetInnerXml(fieldTypeTag);
				if (!fieldTypeTagXML.IsEmpty()) {
					const auto fieldTypeValue = GetFieldTypeFromName(fieldTypeTagXML);
					m_fieldType = fieldTypeValue == E_FT_Unknown ? E_FT_Int64 : fieldTypeValue;
				}
			}

			MiniXmlParser::TagInfo fieldNameTag;
			if (MiniXmlParser::FindXmlTag(tagConfig, fieldNameTag, L"FieldName"))
			{
				auto fieldNameTagXML = MiniXmlParser::GetInnerXml(fieldNameTag);
				if (!fieldNameTagXML.IsEmpty()) {
					m_fieldName = fieldNameTagXML;
				}
			}
		}

		inline void OmniBusSequenceInterface::PI_Close(bool /*bHasErrors*/)
		{
			m_pEngineInterface->OutputMessage(m_nToolId, STATUS_Info, L"PI_Close");
		}

		inline long OmniBusSequenceInterface::PI_AddIncomingConnection(
			const wchar_t* /*pIncomingConnectionType*/,
			const wchar_t* /*pIncomingConnectionName*/,
			IncomingConnectionInterface* /*r_IncConnInt*/)
		{
			throw Error(L"Input tools cannot have incoming connections.");
		}

		long OmniBusSequenceInterface::PI_AddOutgoingConnection(
			const wchar_t* /*pOutgoingConnectionName*/,
			IncomingConnectionInterface *pIncConnInt)
		{
			m_pEngineInterface->OutputMessage(m_nToolId, STATUS_Info, L"PI_AddOutgoingConnection");
			m_outgoingConnections.AddOutgoingConnection(pIncConnInt);
			return 1;
		}

		long OmniBusSequenceInterface::PI_PushAllRecords(__int64 nRecordLimit)
		{
			const String fieldDef(L"<Field name=\"" + m_fieldName + L"\" type=\"" + GetNameFromFieldType(m_fieldType) + L"\">");
			m_recordInfoOut.AddField(fieldDef);

			bool bUpdateConfig = false;
			const String strOutputMetaInfo = ConfigUtility::MakeAndCompareMetaInfo(m_recordInfoOut,
				L"Output",
				L"",
				m_strXmlProperties,
				bUpdateConfig);
			if (bUpdateConfig)
			{
				// Tell Alteryx that we are sending our RecordInfo structure.
				m_pEngineInterface->OutputMessage(m_nToolId,
					STATUS_UpdateOutputMetaInfoXml,
					strOutputMetaInfo);
			}

			m_outgoingConnections.Init(strOutputMetaInfo);


			if (nRecordLimit > 0)
			{
				// Push Data
				__int64 nRecordsSent = 0;
				m_pRecordOut = m_recordInfoOut.CreateRecord();

				nRecordLimit = nRecordLimit > m_maximumRecords ? m_maximumRecords : nRecordLimit;
				while (nRecordsSent < nRecordLimit)
				{
					const FieldBase* pField = m_recordInfoOut[0];
					pField->SetFromInt64(m_pRecordOut.Get(), nRecordsSent);

					if (!m_outgoingConnections.PushRecord(m_pRecordOut->GetRecord()))
					{
						break;
					}

					++nRecordsSent;
				}
			}

			m_pEngineInterface->OutputMessage(m_nToolId, STATUS_Complete, L"");

			// Close all of our outgoing connections.
			m_outgoingConnections.Close();

			// Return 1 to signify that we successfully pushed all of our records.
			return 1;
		}

		// --------------- Public Plugin Interface Starts Here ---------------

		// This is the single entry point for construction of an instance of this tool.
		// We are provided:
		//   A unique tool id
		//   A set of configuration parameters as an XML dictionary
		//   An EngineInterface for communicating with other tools, providing status, etc.
		//   An empty PluginInterface.
		// We must:
		//   Instantiate ourself
		//   Initialize ourself
		//   Fill in the PluginInterface with information about ourself

		extern "C" __declspec(dllexport) long _stdcall AlteryxOmniBusSequence(int nToolID,
			const wchar_t *pXmlProperties,
			EngineInterface *pEngineInterface,
			PluginInterface *r_pluginInterface)
		{
			// To ensure that errors in tool construction and initialization don't
			// cause cascade problems in Alteryx, well behaved tools always wrap this
			// code in a try/catch block.
			try
			{
				// Construct a new instance of our tool.
				// We use a smart pointer here so that if the subsequent PI_Init call
				// throws an exception, the instance will get deleted when the pointer
				// falls out of scope in the catch block.
				std::auto_ptr<OmniBusSequenceInterface> pInterface(new OmniBusSequenceInterface(nToolID, pEngineInterface));

				// Initialize our tool with the XML dictionary.
				pInterface->PI_Init(pXmlProperties);

				// Fill in the PluginInterface.
				// It is important that we use an unmanaged pointer to ourself, and
				// not the smart pointer.  We will have Alteryx manage our instance
				// and free us when necessary.
				ImplementPluginInterface<OmniBusSequenceInterface>::Init(
					nToolID,                // our tool id
					pEngineInterface,       // our EngineInterface
					r_pluginInterface,      // the PluginInterface to fill in
					pInterface.release(),   // an unmanaged pointer to ourself
					true);                  // we want Alteryx to free us

				// Return 1 to signify success.
				return 1;
			}
			catch (const Error &e)
			{
				// This error was likely an error during the PI_Init call, but not
				// necessarily.  Regardless, it is important for us to notify Alteryx
				// as to what the error actually was.
				pEngineInterface->OutputMessage(
					nToolID,
					STATUS_Error,
					ConvertToWString(e.GetErrorDescription()));

				// Return 0 to signify failure.
				return 0;
			}
		}
	}
}