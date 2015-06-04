/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

#include "PixelMapUtils.h"
#include "CoreContext.h"
#include <stdlib.h>
#include "cross_platform.h"
#include <stdio.h>
#include <string.h>
#include <cmath>
#include <algorithm>


void CopyPixelsFromFormat16BPP(BYTE* pDIB, BITMAPINFOHEADER bih, long compression, unsigned long* pixels, BYTE* bitmapPixels, BYTE* bitmapBytes)
{
	long length = (bih.biWidth * bih.biHeight);
	BYTE* src = pDIB + sizeof(BITMAPINFOHEADER);

	unsigned int lineWidth = ((bih.biWidth * 16 / 8) + 3) & ~3;

	for (long y = bih.biHeight - 1; y >= 0; y--) {
		unsigned long* rowPixels = pixels + y * bih.biWidth;
		BYTE* rowBitmapPixels = bitmapBytes + y * bih.biWidth;

		for(long x = 0; x < bih.biWidth; x++) {
			BYTE val;
			unsigned char red;
			unsigned char green;
			unsigned char blue;

			if (compression == 0) {
				unsigned short color = *((unsigned short*) src);
				red = (unsigned char)(((color >> 10) & 0x1f) << 3);
				green = (unsigned char)(((color >> 5) & 0x1f) << 3);
				blue = (unsigned char)((color & 0x1f) << 3);

				src += 2;
			} else if (compression == 1) {
				// RLE 8. Not supported in 16 bit mode
			} else if (compression == 2) {
				// RLE 4. Not supported in 16 bit mode
			} else if (compression == 3) {
				// BITFIELDS. Not supported in 16 bit mode
			};

			if (s_COLOR_CHANNEL == Blue) {
				val = blue;
			} else if (s_COLOR_CHANNEL == Green) {
				val = green;
			} else if (s_COLOR_CHANNEL == Red) {
				val = red;
			} else if (s_COLOR_CHANNEL == GrayScale) {
				val = (unsigned char)(.299 * red + .587 * green + .114 * blue);
			}

			*rowPixels++= (unsigned long)val;
			*rowBitmapPixels++=val;

			*bitmapPixels++=val;
			*bitmapPixels++=val;
			*bitmapPixels++=val;
		}
	}
}

void CopyPixelsFromFormat16BPP(BYTE* pDIB, long width, long height, long compression, unsigned long* pixels)
{
	long length = width * height;
	BYTE* src = pDIB + sizeof(BITMAPINFOHEADER);

	unsigned int lineWidth = ((width * 16 / 8) + 3) & ~3;

	for (long y = height - 1; y >= 0; y--) {
		unsigned long* rowPixels = pixels + y * width;

		for(long x = 0; x < width; x++) {
			BYTE val;
			unsigned char red;
			unsigned char green;
			unsigned char blue;

			if (compression == 0) {
				unsigned short color = *((unsigned short*) src);
				red = (unsigned char)(((color >> 10) & 0x1f) << 3);
				green = (unsigned char)(((color >> 5) & 0x1f) << 3);
				blue = (unsigned char)((color & 0x1f) << 3);

				src += 2;
			} else if (compression == 1) {
				// RLE 8. Not supported in 16 bit mode
			} else if (compression == 2) {
				// RLE 4. Not supported in 16 bit mode
			} else if (compression == 3) {
				// BITFIELDS. Not supported in 16 bit mode
			};

			if (s_COLOR_CHANNEL == Blue) {
				val = blue;
			} else if (s_COLOR_CHANNEL == Green) {
				val = green;
			} else if (s_COLOR_CHANNEL == Red) {
				val = red;
			} else if (s_COLOR_CHANNEL == GrayScale) {
				val = (unsigned char)(.299 * red + .587 * green + .114 * blue);
			}

			*rowPixels++= (unsigned long)val;
		}
	}
}

void CopyPixelsFromFormat8BPP(BYTE* pDIB, BITMAPINFOHEADER bih, unsigned long* pixels, BYTE* bitmapPixels, BYTE* bitmapBytes)
{
	//PixelFormat.Format8bppIndexed

	long length = (bih.biWidth * bih.biHeight);
	BYTE* src = pDIB + sizeof(BITMAPINFOHEADER);
}

void CopyPixelsFromFormat4BPP(BYTE* pDIB, BITMAPINFOHEADER bih, unsigned long* pixels, BYTE* bitmapPixels, BYTE* bitmapBytes)
{
	//PixelFormat.Format4bppIndexed

	long length = (bih.biWidth * bih.biHeight);
	BYTE* src = pDIB + sizeof(BITMAPINFOHEADER);
}

void CopyPixelsInTriplets(BYTE* pDIB, BITMAPINFOHEADER bih, unsigned long* pixels, BYTE* bitmapPixels, BYTE* bitmapBytes)
{
	long bytesPerPixel = bih.biBitCount / 8;
	long length = (bih.biWidth * bih.biHeight);
	BYTE* src = pDIB + sizeof(BITMAPINFOHEADER);

	long width = bih.biWidth;
	long currLinePos = 0;
	pixels+=length + width;
	bitmapBytes+=length + width;

	BYTE val1, val2, val3, val4;
	unsigned long b12, b23, b34;

	unsigned long *lsrc = (unsigned long *)src;
	size_t triplets = length >> 2;
	while (triplets--) {
		b12 = *lsrc++;
		b23 = *lsrc++;
		b34 = *lsrc++;

		if (currLinePos == 0) {
			currLinePos = width;
			pixels-=2*width;
			bitmapBytes-=2*width;
		}

		//depends upon bigendian or little endian cpu

		if (s_COLOR_CHANNEL == Blue) {
			val1 = (unsigned char)(b12);
			val2 = (unsigned char)(b12 >> 24);
			val3 = (unsigned char)(b23 >> 16);
			val4 = (unsigned char)(b34 >> 8);
		} else if (s_COLOR_CHANNEL == Green) {
			val1 = (unsigned char)(b12 >> 8);
			val2 = (unsigned char)(b23);
			val3 = (unsigned char)(b23 >> 24);
			val4 = (unsigned char)(b34 >> 16);
		} else if (s_COLOR_CHANNEL == Red) {
			val1 = (unsigned char)(b12 >> 16);
			val2 = (unsigned char)(b23 >> 8);
			val3 = (unsigned char)(b34);
			val4 = (unsigned char)(b34 >> 24);
		} else if (s_COLOR_CHANNEL == GrayScale) {
			val1 = (unsigned char)(.299 * (unsigned char)(b12) + .587 * (unsigned char)(b12 >> 8) + .114 * (unsigned char)(b12 >> 16));
			val2 = (unsigned char)(.299 * (unsigned char)(b12 >> 24) + .587 * (unsigned char)(b23) + .114 * (unsigned char)(b23 >> 8));
			val3 = (unsigned char)(.299 * (unsigned char)(b23 >> 16) + .587 * (unsigned char)(b23 >> 16) + .114 * (unsigned char)(b34));
			val4 = (unsigned char)(.299 * (unsigned char)(b34 >> 8) + .587 * (unsigned char)(b34 >> 16) + .114 * (unsigned char)(b34 >> 24));
		}


		*pixels++=val1;
		*bitmapBytes++=val1;
		*bitmapPixels++=val1;
		*bitmapPixels++=val1;
		*bitmapPixels++=val1;
		currLinePos--;

		if (currLinePos == 0) {
			currLinePos = width;
			pixels-=width;
			bitmapBytes-=width;
		}

		*pixels++=val2;
		*bitmapBytes++=val2;
		*bitmapPixels++=val2;
		*bitmapPixels++=val2;
		*bitmapPixels++=val2;
		currLinePos--;

		if (currLinePos == 0) {
			currLinePos = width;
			pixels-=width;
			bitmapBytes-=width;
		}

		*pixels++=val3;
		*bitmapBytes++=val3;
		*bitmapPixels++=val3;
		*bitmapPixels++=val3;
		*bitmapPixels++=val3;
		currLinePos--;

		if (currLinePos == 0) {
			currLinePos = width;
			pixels-=width;
			bitmapBytes-=width;
		}

		*pixels++=val4;
		*bitmapBytes++=val4;
		*bitmapPixels++=val4;
		*bitmapPixels++=val4;
		*bitmapPixels++=val4;
		currLinePos--;
	}
}

void CopyPixelsInTriplets(BYTE* pDIB, long width, long height, unsigned long* pixels)
{
	long bytesPerPixel = 3;
	long length = (width * height);
	BYTE* src = pDIB + sizeof(BITMAPINFOHEADER);

	long currLinePos = 0;
	pixels+=length + width;

	BYTE val1, val2, val3, val4;
	unsigned long b12, b23, b34;

	unsigned long *lsrc = (unsigned long *)src;
	size_t triplets = length >> 2;
	while (triplets--) {
		b12 = *lsrc++;
		b23 = *lsrc++;
		b34 = *lsrc++;

		if (currLinePos == 0) {
			currLinePos = width;
			pixels-=2*width;
		}

		//depends upon bigendian or little endian cpu

		if (s_COLOR_CHANNEL == Blue) {
			val1 = (unsigned char)(b12);
			val2 = (unsigned char)(b12 >> 24);
			val3 = (unsigned char)(b23 >> 16);
			val4 = (unsigned char)(b34 >> 8);
		} else if (s_COLOR_CHANNEL == Green) {
			val1 = (unsigned char)(b12 >> 8);
			val2 = (unsigned char)(b23);
			val3 = (unsigned char)(b23 >> 24);
			val4 = (unsigned char)(b34 >> 16);
		} else if (s_COLOR_CHANNEL == Red) {
			val1 = (unsigned char)(b12 >> 16);
			val2 = (unsigned char)(b23 >> 8);
			val3 = (unsigned char)(b34);
			val4 = (unsigned char)(b34 >> 24);
		} else if (s_COLOR_CHANNEL == GrayScale) {
			val1 = (unsigned char)(.299 * (unsigned char)(b12) + .587 * (unsigned char)(b12 >> 8) + .114 * (unsigned char)(b12 >> 16));
			val2 = (unsigned char)(.299 * (unsigned char)(b12 >> 24) + .587 * (unsigned char)(b23) + .114 * (unsigned char)(b23 >> 8));
			val3 = (unsigned char)(.299 * (unsigned char)(b23 >> 16) + .587 * (unsigned char)(b23 >> 16) + .114 * (unsigned char)(b34));
			val4 = (unsigned char)(.299 * (unsigned char)(b34 >> 8) + .587 * (unsigned char)(b34 >> 16) + .114 * (unsigned char)(b34 >> 24));
		}


		*pixels++=val1;
		currLinePos--;

		if (currLinePos == 0) {
			currLinePos = width;
			pixels-=width;
		}

		*pixels++=val2;
		currLinePos--;

		if (currLinePos == 0) {
			currLinePos = width;
			pixels-=width;
		}

		*pixels++=val3;
		currLinePos--;

		if (currLinePos == 0) {
			currLinePos = width;
			pixels-=width;
		}

		*pixels++=val4;
		currLinePos--;
	}
}


HRESULT GetPixelMapPixelsOnly(BYTE* pDIB, long width, long height, unsigned long* pixels)
{
	BITMAPINFOHEADER bih;
	memmove(&bih.biSize, pDIB, sizeof(BITMAPINFOHEADER));

	if (bih.biBitCount == 24)
		CopyPixelsInTriplets(pDIB, width, height, pixels);
	else if (bih.biBitCount == 16)
		CopyPixelsFromFormat16BPP(pDIB, width, height, bih.biCompression, pixels);

	return S_OK;
}

HRESULT GetPixelMapBits(BYTE* pDIB, long* width, long* height, DWORD imageSize, unsigned long* pixels, BYTE* bitmapPixels, BYTE* bitmapBytes)
{
	HBITMAP hBitmap = NULL;
	return GetPixelMapBitsAndHBitmap(pDIB, width, height, imageSize, pixels, bitmapPixels, bitmapBytes, hBitmap);
}

HRESULT GetPixelMapBitsAndHBitmap(BYTE* pDIB, long* width, long* height, DWORD imageSize, unsigned long* pixels, BYTE* bitmapPixels, BYTE* bitmapBytes, HBITMAP hBitmap)
{
	//OutputDebugString("GetPixelMapBitsAndHBitmap.214");

	BITMAPINFOHEADER bih;
	memmove(&bih.biSize, pDIB, sizeof(BITMAPINFOHEADER));

	int originalBitCount = bih.biBitCount;
	int originalCompression = bih.biCompression;

	if (*width == 0) *width = bih.biWidth;
	if (*height == 0) *height = bih.biHeight;

	bih.biSize = sizeof(BITMAPINFOHEADER);
	bih.biPlanes = 1;
	bih.biBitCount = 24;                          // 24-bit
	bih.biCompression = BI_RGB;                   // no compression
	bih.biSizeImage = *width * ABS(*height) * 3;    // width * height * (RGB bytes)
	bih.biXPelsPerMeter = 0;
	bih.biYPelsPerMeter = 0;
	bih.biClrUsed = 0;
	bih.biClrImportant = 0;
	bih.biWidth = *width;                          // bitmap width
	bih.biHeight = *height;                        // bitmap height

	// and BitmapInfo variable-length UDT
	BYTE memBitmapInfo[40];
	memmove(memBitmapInfo, &bih, sizeof(bih));

	BITMAPFILEHEADER bfh;
	bfh.bfType=19778;    //BM header
	bfh.bfSize=55 + bih.biSizeImage;
	bfh.bfReserved1=0;
	bfh.bfReserved2=0;
	bfh.bfOffBits=sizeof(BITMAPINFOHEADER) + sizeof(BITMAPFILEHEADER); //54

	// Copy the display bitmap including the header
	memmove(bitmapPixels, &bfh, sizeof(bfh));
	memmove(bitmapPixels + sizeof(bfh), &memBitmapInfo, sizeof(memBitmapInfo));

	bitmapPixels = bitmapPixels + sizeof(bfh) + sizeof(memBitmapInfo);

	// See http://www.kalytta.com/bitmap.h for handling different bitmap formats
	if (originalBitCount == 24)
		CopyPixelsInTriplets(pDIB, bih, pixels, bitmapPixels, bitmapBytes);
	else if (originalBitCount == 16)
		CopyPixelsFromFormat16BPP(pDIB, bih, originalCompression, pixels, bitmapPixels, bitmapBytes);
	//else if (bih.biBitCount == 8)
	//	CopyPixelsFromFormat8BPP(pDIB, bih, pixels, bitmapPixels, bitmapBytes);
	//else if (bih.biBitCount == 4)
	//	CopyPixelsFromFormat4BPP(pDIB, bih, pixels, bitmapPixels, bitmapBytes);

	return S_OK;
}


HRESULT GetBitmapPixels(long width, long height, unsigned long* pixels, BYTE* bitmapPixels, BYTE* bitmapBytes, bool isLittleEndian, int bpp, unsigned long normVal)
{
	BYTE* pp = bitmapPixels;

	// define the bitmap information header
	BITMAPINFOHEADER bih;
	bih.biSize = sizeof(BITMAPINFOHEADER);
	bih.biPlanes = 1;
	bih.biBitCount = 24;                          // 24-bit
	bih.biCompression = BI_RGB;                   // no compression
	bih.biSizeImage = width * ABS(height) * 3;    // width * height * (RGB bytes)
	bih.biXPelsPerMeter = 0;
	bih.biYPelsPerMeter = 0;
	bih.biClrUsed = 0;
	bih.biClrImportant = 0;
	bih.biWidth = width;                          // bitmap width
	bih.biHeight = height;                        // bitmap height

	// and BitmapInfo variable-length UDT
	BYTE memBitmapInfo[40];
	memmove(memBitmapInfo, &bih, sizeof(bih));

	BITMAPFILEHEADER bfh;
	bfh.bfType=19778;    //BM header
	bfh.bfSize=55 + bih.biSizeImage;
	bfh.bfReserved1=0;
	bfh.bfReserved2=0;
	bfh.bfOffBits=sizeof(BITMAPINFOHEADER) + sizeof(BITMAPFILEHEADER); //54

	// Copy the display bitmap including the header
	memmove(bitmapPixels, &bfh, sizeof(bfh));
	memmove(bitmapPixels + sizeof(bfh), &memBitmapInfo, sizeof(memBitmapInfo));

	bitmapPixels = bitmapPixels + sizeof(bfh) + sizeof(memBitmapInfo);

	long currLinePos = 0;
	int length = width * height;
	bitmapPixels+=3 * (length + width);

	int shiftVal = 0;
	if (bpp == 8) shiftVal = 0;
	else if (bpp == 9) shiftVal = 1;
	else if (bpp == 10) shiftVal = 2;
	else if (bpp == 11) shiftVal = 3;
	else if (bpp == 12) shiftVal = 4;
	else if (bpp == 13) shiftVal = 5;
	else if (bpp == 14) shiftVal = 6;
	else if (bpp == 15) shiftVal = 7;
	else if (bpp == 16) shiftVal = 8;

	int total = width * height;
	while(total--) {
		if (currLinePos == 0) {
			currLinePos = width;
			bitmapPixels-=6*width;
		};

		unsigned int val = *pixels;
		pixels++;

		unsigned int dblVal;
		if (bpp == 8) {
			dblVal = val;
		} else if (normVal > 0) {
			dblVal = (unsigned int)(255.0 * val / normVal);
		} else {
			dblVal = val >> shiftVal;
		}


		BYTE btVal = (BYTE)(dblVal & 0xFF);

		*bitmapPixels = btVal;
		*(bitmapPixels + 1) = btVal;
		*(bitmapPixels + 2) = btVal;
		bitmapPixels+=3;

		*bitmapBytes++ = btVal;

		currLinePos--;
	}

	return S_OK;
}

long GetNewLinePosition(long line, long firstOsdLine, long lastOsdLine)
{
	if (line < firstOsdLine || line > lastOsdLine)
		return -1;
	else
		return firstOsdLine + ((line - firstOsdLine) / 2) + ((line - 1) % 2) * ((lastOsdLine - firstOsdLine) / 2);
}

HRESULT BitmapSplitFieldsOSD(BYTE* bitmapPixels, long firstOsdLine, long lastOsdLine)
{
	BITMAPINFOHEADER bih;
	memmove(&bih, bitmapPixels + sizeof(BITMAPFILEHEADER), sizeof(bih));

	if (firstOsdLine >= lastOsdLine || firstOsdLine < 0 || lastOsdLine > bih.biHeight)
		return E_FAIL;

	int stride = bih.biWidth * (bih.biBitCount / 8);

	int moveFrom = firstOsdLine;
	int moveTo = -1;
	int buffedLine = -1;

	BYTE* buffer1 = (BYTE*)malloc(stride);
	BYTE* buffer2 = (BYTE*)malloc(stride);

	long a = bih.biHeight;
	long b = -1;

	bool* movedLines = (bool*)malloc(lastOsdLine - firstOsdLine);

	for (int i = 0; i <= lastOsdLine - firstOsdLine; i++) movedLines[i] = false;

	BYTE* ppFirstBitmapPixel = bitmapPixels + 55;

	for (int counter = firstOsdLine; counter <= lastOsdLine; counter++) {
		if (movedLines[counter - firstOsdLine])
			continue;

		moveFrom = counter;
		memmove(buffer1, ppFirstBitmapPixel + (a + b * moveFrom) * stride, stride);

		do {
			moveTo = GetNewLinePosition(moveFrom, firstOsdLine, lastOsdLine);

			if (moveTo != -1) {
				if (moveFrom != moveTo && !movedLines[moveTo - firstOsdLine]) {
					memmove(buffer2, ppFirstBitmapPixel + (a + b * moveTo) * stride, stride);
					buffedLine = moveTo;

					memmove(ppFirstBitmapPixel + (a + b * moveTo) * stride, buffer1, stride);
					memmove(buffer1, buffer2, stride);

					movedLines[moveTo - firstOsdLine] = true;
				} else {
					movedLines[moveTo - firstOsdLine] = true;
					break;
				}
			}

			moveFrom = moveTo;
		} while(true);
	};

	delete buffer1;
	delete buffer2;
	delete movedLines;

	return S_OK;
}

void GetMinMaxValuesForBpp(int bpp, unsigned long normVal, int* minValue, int* maxValue)
{
	*minValue = 0;
	*maxValue = 0;

	if (bpp == 8)
		*maxValue = 0xFF;
	else if (bpp == 12)
		*maxValue = 0xFFF;
	else if (bpp == 14)
		*maxValue = 0x3FFF;
	else if (bpp == 16)
		*maxValue = normVal > 0 ? normVal : 0xFFFF;
	else
		*maxValue = (1 << bpp) - 1;
}

int s_GammaTableBpp = 0;
float s_GammaTableEncodingGamma = 1.0f;
unsigned int* s_GammaTable = NULL;

void BuildGammaTableForBpp(int bpp, unsigned long normVal, float gamma)
{
	if (s_GammaTableBpp != bpp ||
	    ABS(s_GammaTableEncodingGamma - gamma) >= 0.01) {
		if (NULL != s_GammaTable) {
			delete s_GammaTable;
			s_GammaTable = NULL;
		}

		int minValue;
		int maxValue;

		GetMinMaxValuesForBpp(bpp, normVal, &minValue, &maxValue);

		float decodingGamma = 1.0f / gamma;
		float gammaPixelConvCoeff = maxValue / pow(maxValue, decodingGamma);

		s_GammaTable = (unsigned int*)malloc((maxValue + 1) * sizeof(unsigned int));

		unsigned int* itt = s_GammaTable;

		for (int idx = 0; idx <= maxValue; idx++) {
			float conversionValue =  gammaPixelConvCoeff * pow(idx, decodingGamma);
			if (conversionValue + 0.5 >= maxValue)
				*itt = maxValue;
			else if (conversionValue + 0.5 < minValue)
				*itt = minValue;
			else
				*itt = (unsigned int)(conversionValue + 0.5); // rounded to nearest int

			itt++;
		}

		s_GammaTableBpp = bpp;
		s_GammaTableEncodingGamma = gamma;
	}
}

HRESULT PreProcessingFlipRotate(unsigned long* pixels, long width, long height, int bpp, enum RotateFlipType flipRotateType)
{
	if (flipRotateType == 0) {
		//Rotate180FlipXY = 0,
		//RotateNoneFlipNone = 0,

		// NOTE: Nothing to do
	} else if (flipRotateType == 1) {
		//Rotate270FlipXY = 1,
		//Rotate90FlipNone = 1,

		// NOTE: Not supported
	} else if (flipRotateType == 2) {
		//Rotate180FlipNone = 2,
		//RotateNoneFlipXY = 2,

		for	(long x = 0; x < width / 2; x ++) {
			for	(long y = 0; y < height; y ++) {
				unsigned long tmp = pixels[width * y + x];
				pixels[width * y + x] = pixels[width * (height - 1 - y) + (width - 1 - x)];
				pixels[width * (height - 1 - y) + (width - 1 - x)] = tmp;
			}
		}
	} else if (flipRotateType == 3) {
		//Rotate270FlipNone = 3,
		//Rotate90FlipXY = 3,

		// NOTE: Not supported
	} else if (flipRotateType == 4) {
		//DRotate180FlipY = 4,
		//RotateNoneFlipX = 4,

		for	(long x = 0; x < width / 2; x ++) {
			for	(long y = 0; y < height; y ++) {
				unsigned long tmp = pixels[width * y + x];
				pixels[width * y + x] = pixels[width * y + (width - 1 - x)];
				pixels[width * y + (width - 1 - x)] = tmp;
			}
		}
	} else if (flipRotateType == 5) {
		//Rotate270FlipY = 5,
		//Rotate90FlipX = 5,

		// NOTE: Not supported
	} else if (flipRotateType == 6) {
		//Rotate180FlipX = 6,
		//RotateNoneFlipY = 6,

		for	(long y = 0; y < height / 2; y ++) {
			for	(long x = 0; x < width; x ++) {
				unsigned long tmp = pixels[width * y + x];
				pixels[width * y + x] = pixels[width * (height - 1 - y) + x];
				pixels[width * (height - 1 - y) + x] = tmp;
			}
		}
	} else if (flipRotateType == 6) {
		//Rotate270FlipX = 7,
		//Rotate90FlipY = 7

		// NOTE: Not supported
	}

	return S_OK;
}

HRESULT PreProcessingStretch(unsigned long* pixels, long width, long height, int bpp, unsigned long normVal, int fromValue, int toValue)
{
	int minValue, maxValue;
	GetMinMaxValuesForBpp(bpp, normVal, &minValue, &maxValue);

	if (fromValue < minValue) fromValue = minValue;
	if (toValue > maxValue) toValue = maxValue;

	long totalPixels = width * height;
	unsigned long* pPixels = pixels;
	float coeff = ((float)(maxValue - minValue)) / (toValue - fromValue);

	while(totalPixels--) {
		if (*pPixels < fromValue)
			*pPixels = minValue;
		else if (*pPixels > toValue)
			*pPixels = maxValue;
		else {
			float newValue = coeff * (*pPixels - fromValue);
			if (newValue < minValue) newValue = minValue;
			if (newValue > maxValue) newValue = maxValue;
			*pPixels = (long)newValue;
		}
		pPixels++;
	}

	return S_OK;
}

HRESULT PreProcessingClip(unsigned long* pixels, long width, long height, int bpp, unsigned long normVal, int fromValue, int toValue)
{
	int minValue, maxValue;
	GetMinMaxValuesForBpp(bpp, normVal, &minValue, &maxValue);

	if (fromValue < minValue) fromValue = minValue;
	if (toValue > maxValue) toValue = maxValue;

	long totalPixels = width * height;
	unsigned long* pPixels = pixels;

	while(totalPixels--) {
		if (*pPixels < fromValue)
			*pPixels = fromValue;
		else if (*pPixels > toValue)
			*pPixels = toValue;

		pPixels++;
	}

	return S_OK;
}

HRESULT PreProcessingBrightnessContrast(unsigned long* pixels, long width, long height, int bpp, unsigned long normVal, long brightness, long cotrast)
{
	int minValue, maxValue;
	GetMinMaxValuesForBpp(bpp, normVal, &minValue, &maxValue);

	if (brightness < -255) brightness = -255;
	if (brightness > 255) brightness = 255;

	double bppBrightness = brightness;
	if (bpp == 12) bppBrightness *= 0xF;
	if (bpp == 14) bppBrightness *= 0x3F;
	if (bpp == 16) bppBrightness *= 0xFF;

	long totalPixels = width * height;
	unsigned long* pPixels = pixels;

	if (cotrast < -100) cotrast = -100;
	if (cotrast > 100) cotrast = 100;

	double contrastCoeff = (100.0 + cotrast) / 100.0;
	contrastCoeff *= contrastCoeff;

	while(totalPixels--) {
		double pixel = *pPixels + bppBrightness;
		if (pixel < minValue) pixel = minValue;
		if (pixel > maxValue) pixel = maxValue;

		pixel = pixel * 1.0 / maxValue;
		pixel -= 0.5;
		pixel *= contrastCoeff;
		pixel += 0.5;
		pixel *= maxValue;
		if (pixel < minValue) pixel = minValue;
		if (pixel > maxValue) pixel = maxValue;

		*pPixels = (long)pixel;

		pPixels++;
	}

	return S_OK;
}

HRESULT PreProcessingGamma(unsigned long* pixels, long width, long height, int bpp, unsigned long normVal, float gamma)
{
	BuildGammaTableForBpp(bpp, normVal, gamma);

	long totalPixels = width * height;
	unsigned long* pPixels = pixels;

	while(totalPixels--) {
		*pPixels = *(s_GammaTable + *pPixels);
		pPixels++;
	}

	return S_OK;
}

HRESULT PreProcessingApplyBiasDarkFlatFrame(
    unsigned long* pixels, long width, long height, int bpp, unsigned long normVal,
    float* biasPixels, float* darkPixels, float* flatPixels, 
	float scienseExposure, float darkExposure, bool darkFrameIsBiasCorrected, bool isSameExposureDarkFrame, float flatMedian)
{
	int minValue, maxValue;
	GetMinMaxValuesForBpp(bpp, normVal, &minValue, &maxValue);

	long totalPixels = width * height;
	unsigned long* pPixels = pixels;
	float* pBiasPixels = biasPixels;
	float* pDarkPixels = darkPixels;
	float* pFlatPixels = flatPixels;
	
	float coeffDarkExposureScaling = 1;
	if (darkExposure > 0 && scienseExposure > 0) coeffDarkExposureScaling = scienseExposure / darkExposure;
	
	while(totalPixels--) 
	{
		double pixelValue = *pPixels;

		//          original - bias - (T_image/T_dark) * dark
		// Final = ------------------------------------------- * MEDIAN (flat)
		//                          flat
			
			
		if (NULL != biasPixels && !isSameExposureDarkFrame)	
		{
			pixelValue = pixelValue - *pBiasPixels;
			if (pixelValue < minValue) pixelValue = minValue;
		}
		
		if (NULL != darkPixels) 
		{
			if (isSameExposureDarkFrame)
				pixelValue = pixelValue - *pDarkPixels;	
			else if (darkFrameIsBiasCorrected)
				pixelValue = pixelValue - (*pDarkPixels * coeffDarkExposureScaling);
			else if (!darkFrameIsBiasCorrected)
			{
				double participatingDarkCurrent = NULL != biasPixels ? (*pDarkPixels - *pBiasPixels) : *pDarkPixels;
				if (participatingDarkCurrent < minValue) participatingDarkCurrent = minValue;
				pixelValue = pixelValue - (participatingDarkCurrent * coeffDarkExposureScaling);				
			}
		}
			
		if (NULL != flatPixels) pixelValue = pixelValue * (double)flatMedian / *pFlatPixels;

		if ((long)pixelValue > maxValue)
			*pPixels = maxValue;
		else if ((long)pixelValue < minValue)
			*pPixels = minValue;
		else
			*pPixels = (unsigned long)(pixelValue + 0.5);

		pPixels++;
		
		if (NULL != biasPixels) pBiasPixels++;
		if (NULL != darkPixels) pDarkPixels++;
		if (NULL != flatPixels) pFlatPixels++;
	}

	return S_OK;
}

struct ConvMatrix {
	ConvMatrix() {
		TopLeft = 0;
		TopMid = 0;
		TopRight = 0;
		MidLeft = 0;
		Pixel = 1;
		MidRight = 0;
		BottomLeft = 0;
		BottomMid = 0;
		BottomRight = 0;
		Factor = 1;
		Offset = 0;
	}

	float TopLeft;
	float TopMid;
	float TopRight;
	float MidLeft;
	float Pixel;
	float MidRight;
	float BottomLeft;
	float BottomMid;
	float BottomRight;
	int Factor;
	int Offset;
};

bool convMatrixConstantsInitialized = false;
ConvMatrix LOW_PASS_FILTER_MATRIX;

void EnsureConvMatrixConstantsInitialized()
{
	if (!convMatrixConstantsInitialized) {
		// http://www.echoview.com/WebHelp/Reference/Algorithms/Operators/Convolution_algorithms.htm
		LOW_PASS_FILTER_MATRIX.TopLeft = 1/16.0f;
		LOW_PASS_FILTER_MATRIX.TopMid = 1/8.0f;
		LOW_PASS_FILTER_MATRIX.TopRight = 1/16.0f;
		LOW_PASS_FILTER_MATRIX.MidLeft = 1/8.0f;
		LOW_PASS_FILTER_MATRIX.Pixel = 1/4.0f;
		LOW_PASS_FILTER_MATRIX.MidRight = 1/8.0f;
		LOW_PASS_FILTER_MATRIX.BottomLeft = 1/16.0f;
		LOW_PASS_FILTER_MATRIX.BottomMid = 1/8.0f;
		LOW_PASS_FILTER_MATRIX.BottomRight = 1/16.0f;

		convMatrixConstantsInitialized = true;
	}
}

void Conv3x3(unsigned long* pixels, long width, long height, int bpp, unsigned long normVal, ConvMatrix* m)
{
	// Avoid divide by zero errors
	if (0 == m->Factor)
		return;

	int minValue, maxValue;
	GetMinMaxValuesForBpp(bpp, normVal, &minValue, &maxValue);

	for (int y = 0; y < height - 2; ++y) {
		for (int x = 0; x < width - 2; ++x) {
			double dblPixel = (unsigned long)(
			                      (
			                          (
			                              ((double)*(pixels + x + y * width) * m->TopLeft) +
			                              ((double)*(pixels + (x + 1) + y * width) * m->TopMid) +
			                              ((double)*(pixels + (x + 2) + y * width) * m->TopRight) +
			                              ((double)*(pixels + x + (y + 1) * width) * m->MidLeft) +
			                              ((double)*(pixels + (x + 1) + (y + 1) * width) * m->Pixel) +
			                              ((double)*(pixels + (x + 2) + (y + 1) * width) * m->MidRight) +
			                              ((double)*(pixels + x + (y + 2) * width) * m->BottomLeft) +
			                              ((double)*(pixels + (x + 1) + (y + 2) * width) * m->BottomMid) +
			                              ((double)*(pixels + (x + 2) + (y + 2) * width) * m->BottomRight)
			                          ) / m->Factor
			                      ) + m->Offset
			                      + 0.5 /*rounded*/ );

			if (dblPixel < 0) dblPixel = 0;
			if (dblPixel > maxValue) dblPixel = maxValue;
			if (dblPixel < minValue) dblPixel = minValue;

			*(pixels + (x + 1) + (y  + 1) * width) = (unsigned long)dblPixel;
		}
	}
}

DLL_PUBLIC HRESULT PreProcessingLowPassFilter(unsigned long* pixels, long width, long height, int bpp, unsigned long normVal)
{
	Conv3x3(pixels, width, height, bpp, normVal, &LOW_PASS_FILTER_MATRIX);

	return S_OK;
}

// TODO: Use STL collection that supports sorting!!!
unsigned long g_Median5x5ValuesBuffer[25];
int g_IdxMedian5x5ValuesBuffer = 0;
unsigned long* g_LowPassDataLPDBuffer = NULL;

long g_LPDBufferWidth = -1;
long g_LPDBufferHeight = -1;

void InitializeLowPassDifferenceFilter(long width, long height)
{
	if (g_LPDBufferWidth != width ||
	    g_LPDBufferHeight != height) {
		if (NULL != g_LowPassDataLPDBuffer) {
			delete g_LowPassDataLPDBuffer;
			g_LowPassDataLPDBuffer = NULL;
		}

		g_LowPassDataLPDBuffer = (unsigned long*)malloc(width * height * sizeof(unsigned long));

		g_LPDBufferWidth = width;
		g_LPDBufferHeight = height;
	}
}

void AddValueForMedianComp(unsigned long val)
{
	g_Median5x5ValuesBuffer[g_IdxMedian5x5ValuesBuffer] = val;
	g_IdxMedian5x5ValuesBuffer++;
}

void ClearMedian5x5ValuesBuffer()
{
	g_IdxMedian5x5ValuesBuffer = 0;
}

unsigned long GetMedianValueFromBuffer()
{
	unsigned long median = 0;

	std::sort(g_Median5x5ValuesBuffer, g_Median5x5ValuesBuffer + g_IdxMedian5x5ValuesBuffer + 1);

	int middleCal = (g_IdxMedian5x5ValuesBuffer + 1) / 2;
	if ((g_IdxMedian5x5ValuesBuffer + 1) % 2 == 1)
		median = g_Median5x5ValuesBuffer[middleCal];
	else if (g_IdxMedian5x5ValuesBuffer > 1)
		median = (unsigned long)(((float)g_Median5x5ValuesBuffer[middleCal] + (float)g_Median5x5ValuesBuffer[middleCal - 1]) / 2.0);
	else if (g_IdxMedian5x5ValuesBuffer == 1)
		median = g_Median5x5ValuesBuffer[0];

	return median;
}

DLL_PUBLIC HRESULT PreProcessingLowPassDifferenceFilter(unsigned long* pixels, long width, long height, int bpp, unsigned long normVal)
{
	InitializeLowPassDifferenceFilter(width, height);

	unsigned long* lowPassData = g_LowPassDataLPDBuffer;

	memcpy(lowPassData, pixels, width * height * sizeof(unsigned long));

	Conv3x3(lowPassData, width, height, bpp, normVal, &LOW_PASS_FILTER_MATRIX);

	for (int y = 0; y < height; ++y) {
		for (int x = 0; x < width; ++x) {
			// The median 5x5 value is the median of all values in the 5x5 region around the point
			ClearMedian5x5ValuesBuffer();

			if (x - 2 >= 0) {
				if (y - 2 >= 0) AddValueForMedianComp(lowPassData[x - 2 + (y - 2)*width]);
				if (y - 1 >= 0) AddValueForMedianComp(lowPassData[x - 2 + (y - 1)*width]);
				AddValueForMedianComp(lowPassData[x - 2 + y*width]);
				if (y + 1 < height) AddValueForMedianComp(lowPassData[x - 2 + (y + 1)*width]);
				if (y + 2 < height) AddValueForMedianComp(lowPassData[x - 2 + (y + 2)*width]);
			}

			if (x - 1 >= 0) {
				if (y - 2 >= 0) AddValueForMedianComp(lowPassData[x - 1 + (y - 2)*width]);
				if (y - 1 >= 0) AddValueForMedianComp(lowPassData[x - 1 + (y - 1)*width]);
				AddValueForMedianComp(lowPassData[x - 1 + y*width]);
				if (y + 1 < height) AddValueForMedianComp(lowPassData[x - 1 + (y + 1)*width]);
				if (y + 2 < height) AddValueForMedianComp(lowPassData[x - 1 + (y + 2)*width]);
			}

			AddValueForMedianComp(lowPassData[x + y*width]);

			if (x + 1 < width) {
				if (y - 2 >= 0) AddValueForMedianComp(lowPassData[x + 1 + (y - 2)*width]);
				if (y - 1 >= 0) AddValueForMedianComp(lowPassData[x + 1 + (y - 1)*width]);
				AddValueForMedianComp(lowPassData[x + 1 + y*width]);
				if (y + 1 < height) AddValueForMedianComp(lowPassData[x + 1 + (y + 1)*width]);
				if (y + 2 < height) AddValueForMedianComp(lowPassData[x + 1 + (y + 2)*width]);
			}

			if (x + 2 < width) {
				if (y - 2 >= 0) AddValueForMedianComp(lowPassData[x + 2 + (y - 2)*width]);
				if (y - 1 >= 0) AddValueForMedianComp(lowPassData[x + 2 + (y - 1)*width]);
				AddValueForMedianComp(lowPassData[x + 2 + y*width]);
				if (y + 1 < height) AddValueForMedianComp(lowPassData[x + 2 + (y + 1)*width]);
				if (y + 2 < height) AddValueForMedianComp(lowPassData[x + 2 + (y + 2)*width]);
			}

			unsigned long medianValue = GetMedianValueFromBuffer();

			if (medianValue > lowPassData[x + y*width])
				pixels[x + y*width] = 0;
			else
				pixels[x + y*width] = (unsigned long)(lowPassData[x + y*width] - medianValue);
		}
	}

	return S_OK;
}


HRESULT GetRotatedFrameDimentions(int width, int height, double angleDegrees, int* newWidth, int* newHeight)
{
/*

Graphics::TBitmap *SrcBitmap=new Graphics::TBitmap; 
Graphics::TBitmap *DestBitmap=new Graphics::TBitmap; 
SrcBitmap->LoadFromFile("Crayon.bmp"); 
//Convert degrees to radians 
float radians=(2*3.1416*angle)/360; 

float cosine=(float)cos(radians); 
float sine=(float)sin(radians); 

float Point1x=(-SrcBitmap->Height*sine); 
float Point1y=(SrcBitmap->Height*cosine); 
float Point2x=(SrcBitmap->Width*cosine-SrcBitmap->Height*sine); 
float Point2y=(SrcBitmap->Height*cosine+SrcBitmap->Width*sine); 
float Point3x=(SrcBitmap->Width*cosine); 
float Point3y=(SrcBitmap->Width*sine); 

float minx=min(0,min(Point1x,min(Point2x,Point3x))); 
float miny=min(0,min(Point1y,min(Point2y,Point3y))); 
float maxx=max(Point1x,max(Point2x,Point3x)); 
float maxy=max(Point1y,max(Point2y,Point3y)); 

int DestBitmapWidth=(int)ceil(fabs(maxx)-minx); 
int DestBitmapHeight=(int)ceil(fabs(maxy)-miny); 

DestBitmap->Height=DestBitmapHeight; 
DestBitmap->Width=DestBitmapWidth;

*/
	return E_NOTIMPL;
}

HRESULT RotateFrame(int width, int height, double angleDegrees, unsigned long* originalPixels, unsigned long* pixels, BYTE* bitmapPixels, BYTE* bitmapBytes)
{
	// http://docs.opencv.org/doc/tutorials/imgproc/imgtrans/warp_affine/warp_affine.html?highlight=warpaffine
	// http://stackoverflow.com/questions/2278414/rotating-an-image-in-c-c
	// http://www.leunen.com/cbuilder/rotbmp.html
	// newx=x*cos(angle)+y*sin(angle) 
	// newy=y*cos(angle)-x*sin(angle)

/*

for(int x=0;x<DestBitmapWidth;x++) 
{ 
  for(int y=0;y<DestBitmapHeight;y++) 
  { 
    int SrcBitmapx=(int)((x+minx)*cosine+(y+miny)*sine); 
    int SrcBitmapy=(int)((y+miny)*cosine-(x+minx)*sine); 
    if(SrcBitmapx>=0&&SrcBitmapx<SrcBitmap->Width&&SrcBitmapy>=0&& 
         SrcBitmapy<SrcBitmap->Height) 
    { 
      DestBitmap->Canvas->Pixels[x][y]= 
          SrcBitmap->Canvas->Pixels[SrcBitmapx][SrcBitmapy]; 
    } 
  } 
} 
//Show the rotated bitmap 
Image1->Picture->Bitmap=DestBitmap; 
delete DestBitmap; 
delete SrcBitmap;

*/
	return E_NOTIMPL;
}


__uint64 GetUInt64Average(__uint64 a, __uint64 b)
{
	return (a >> 1) + (b >> 1) + (a & b & 0x1);
};

__uint64 GetUInt64Average(unsigned long aLo, unsigned long aHi, unsigned long bLo, unsigned long bHi)
{
	return GetUInt64Average(((__uint64)aHi) << 32 + (__uint64)aLo, ((__uint64)bHi) << 32 + (__uint64)bLo);
};
