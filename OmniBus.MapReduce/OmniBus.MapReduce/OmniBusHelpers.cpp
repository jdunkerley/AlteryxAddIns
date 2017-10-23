#include "OmniBusHelpers.h"

namespace OmniBus
{
	class Helpers
	{
		static bool ReadValue(
			SRC::MiniXmlParser::TagInfo const &tagConfig,
			const SRC::String pRequestedTagName,
			SRC::String &resultStore)
		{
			SRC::MiniXmlParser::TagInfo tagMatched;
			if (!SRC::MiniXmlParser::FindXmlTag(tagConfig, tagMatched, pRequestedTagName))
				return false;

			auto innerXml = SRC::MiniXmlParser::GetInnerXml(tagMatched);
			if (innerXml.IsEmpty())
			{
				// Check Value Attribute
			}

			if (innerXml.IsEmpty())
				return false;

			resultStore = innerXml;
			return true;
		}

		static bool ReadValue(
			SRC::MiniXmlParser::TagInfo const &tagConfig,
			const SRC::String pRequestedTagName,
			SRC::String &resultStore)
		{
			SRC::String innerXml
		}
	};
}