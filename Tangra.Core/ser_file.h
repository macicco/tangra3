#pragma once

#include <string>
#include <stdio.h>
#include "cross_platform.h"

using namespace std;
using std::string;

namespace SerLib
{
	enum ColourFormat
	{
		ColourFormat_MONO = 0,
		ColourFormat_BAYER_RGGB = 8,
		ColourFormat_BAYER_GRBG = 9,
		ColourFormat_BAYER_GBRG = 10,
		ColourFormat_BAYER_BGGR = 11,
		ColourFormat_BAYER_CYYM = 16,
		ColourFormat_BAYER_YCMY = 17,
		ColourFormat_BAYER_YMCY = 18,
		ColourFormat_BAYER_MYYC = 19,
		ColourFormat_RGB = 100,
		ColourFormat_BGR = 101
	};
	
	struct SerFileInfo
	{
		long CameraId;
		long ColourId;
		long LittleEndian;
		long Width;
		long Height;
		long PixelDepthPerPlane;
		long CountFrames;
		long SequenceStartTimeLo;
		long SequenceStartTimeHi;
		long SequenceStartTimeUTCLo;
		long SequenceStartTimeUTCHi;
		unsigned long NormalisationValue;
	};
	
	struct SerFrameInfo
	{
		long TimeStampLo;
		long TimeStampHi;
		// NOTE: This is not marshalled back to the .NET structure
		unsigned __int64 TimeStamp64;
	};
	
	class SerFile {
		private:
			char* m_Telescope;
			FILE* m_File;
			long m_ColourId;
			long m_NumPlanes;
			long m_BytesPerPixel;
			long m_PixelsPerFrame;
			long m_CountFrames;
			__int64 m_TimeStampStartOffset;
		
		private:
			unsigned char* m_RawFrameBuffer;
			long m_RawFrameSize;
			HRESULT ProcessRawFrame(unsigned long* pixels, int cameraBitPix);
			
		public:
			long Width;
			long Height;
			long Bpp;
			bool LittleEndian;
			long NormalisationValue;
			bool HasTimeStamps;
		
		public:
			SerFile();
			~SerFile();
			
			void OpenFile(const char* filePath, SerLib::SerFileInfo* fileInfo, char* observer, char* instrument, char* telescope, bool checkMagic);
			void CloseFile();
			HRESULT GetFrame(int frameNo, unsigned long* pixels, int cameraBitPix, SerLib::SerFrameInfo* frameInfo);
	};
}

/* Make sure functions are exported with C linkage under C++ compilers. */
#ifdef __cplusplus
extern "C"
{
#endif

DLL_PUBLIC HRESULT SEROpenFile(char* fileName, SerLib::SerFileInfo* fileInfo, char* observer, char* instrument, char* telescope, bool checkMagic);
DLL_PUBLIC HRESULT SERCloseFile();
DLL_PUBLIC HRESULT SERGetFrame(int frameNo, unsigned long* pixels, BYTE* bitmapPixels, BYTE* bitmapBytes, int cameraBitPix, SerLib::SerFrameInfo* frameInfo);		
DLL_PUBLIC HRESULT SERGetIntegratedFrame(int startFrameNo, int framesToIntegrate, bool isSlidingIntegration, bool isMedianAveraging, unsigned long* pixels, BYTE* bitmapBytes, BYTE* bitmapDisplayBytes, int cameraBitPix, SerLib::SerFrameInfo* frameInfo);
		
#ifdef __cplusplus
} // __cplusplus defined.
#endif