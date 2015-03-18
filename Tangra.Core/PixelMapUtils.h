/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

#ifndef PIXELMAP_UTILS
#define PIXELMAP_UTILS

#include "cross_platform.h"
#include "PreProcessing.h"

/* Make sure functions are exported with C linkage under C++ compilers. */
#ifdef __cplusplus
extern "C"
{
#endif

DLL_PUBLIC HRESULT GetPixelMapBits(BYTE* pDIB, long* width, long* height, DWORD imageSize, unsigned long* pixels, BYTE* bitmapPixels, BYTE* bitmapBytes);
DLL_PUBLIC HRESULT GetPixelMapBitsAndHBitmap(BYTE* pDIB, long* width, long* height, DWORD imageSize, unsigned long* pixels, BYTE* bitmapPixels, BYTE* bitmapBytes, HBITMAP hBitmap);

// Only returns the pixelmap pixels, does not create the bitmap structures
DLL_PUBLIC HRESULT GetPixelMapPixelsOnly(BYTE* pDIB, long width, long height, unsigned long* pixels);
DLL_PUBLIC HRESULT GetBitmapPixels(long width, long height, unsigned long* pixels, BYTE* bitmapPixels, BYTE* bitmapBytes, bool isLittleEndian, int bpp, unsigned long normVal);
DLL_PUBLIC HRESULT BitmapSplitFieldsOSD(BYTE* bitmapPixels, long firstOsdLine, long lastOsdLine);


// Pre-Processing 
DLL_PUBLIC HRESULT PreProcessingFlipRotate(unsigned long* pixels, long width, long height, int bpp, enum RotateFlipType flipRotateType);
DLL_PUBLIC HRESULT PreProcessingStretch(unsigned long* pixels, long width, long height, int bpp, unsigned long normVal, int fromValue, int toValue);
DLL_PUBLIC HRESULT PreProcessingClip(unsigned long* pixels, long width, long height, int bpp, unsigned long normVal, int fromValue, int toValue);
DLL_PUBLIC HRESULT PreProcessingBrightnessContrast(unsigned long* pixels, long width, long height, int bpp, unsigned long normVal, long brightness, long cotrast);
DLL_PUBLIC HRESULT PreProcessingGamma(unsigned long* pixels, long width, long height, int bpp, unsigned long normVal, float gamma);
DLL_PUBLIC HRESULT PreProcessingApplyBiasDarkFlatFrame(
	unsigned long* pixels,
	long width, 
	long height, 
	int bpp, 
	unsigned long normVal,
	float* biasPixels, float* darkPixels, float* flatPixels, 
	float scienseExposure, float darkExposure, bool darkFrameIsBiasCorrected, bool isSameExposureDarkFrame, float flatMedian);
DLL_PUBLIC HRESULT PreProcessingLowPassFilter(unsigned long* pixels, long width, long height, int bpp, unsigned long normVal);
DLL_PUBLIC HRESULT PreProcessingLowPassDifferenceFilter(unsigned long* pixels, long width, long height, int bpp, unsigned long normVal);


#ifdef __cplusplus
} // __cplusplus defined.
#endif

__uint64 GetUInt64Average(__uint64 a, __uint64 b);
__uint64 GetUInt64Average(unsigned long aLo, unsigned long aHi, unsigned long bLo, unsigned long bHi);

#endif