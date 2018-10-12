#pragma once

#include "D:\SDKs\AlteryxSDK\AlteryxPluginAPI\AlteryxPluginSdk.h"

namespace OmniBus
{
	class Helpers
	{
	public:
		static bool ReadValue(
			SRC::MiniXmlParser::TagInfo const &tagConfig,
			SRC::String const *pRequestedTagName,
			int &resultStore);
	};
}
