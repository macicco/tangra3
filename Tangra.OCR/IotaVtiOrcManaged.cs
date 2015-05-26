﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using Tangra.Model.Config;
using Tangra.Model.Helpers;
using Tangra.Model.Image;
using Tangra.Model.VideoOperations;
using Tangra.OCR.IotaVtiOsdProcessor;
using Tangra.PInvoke;

namespace Tangra.OCR
{
	public class IotaVtiOrcManaged : ITimestampOcr
	{
	    private IVideoController m_VideoController;

		private TimestampOCRData m_InitializationData;
		private int m_FromLine;
		private int m_ToLine;
		private uint[] m_OddFieldPixels;
		private uint[] m_EvenFieldPixels;
        private uint[] m_OddFieldPixelsPreProcessed;
        private uint[] m_EvenFieldPixelsPreProcessed;
	    private uint[] m_OddFieldPixelsDebugNoLChD;
        private uint[] m_EvenFieldPixelsDebugNoLChD;
		private int m_FieldAreaHeight;
		private int m_FieldAreaWidth;

		private bool m_TVSafeModeGuess;
        private IotaVtiOcrProcessor m_Processor;

		private Dictionary<string, uint[]> m_CalibrationImages = new Dictionary<string, uint[]>();
		private List<string> m_CalibrationErrors = new List<string>(); 
	    private uint[] m_LatestFrameImage;
		private bool m_UseNativePreProcessing;

		private bool m_ForceErrorReport;

		private IotaVtiOcrCorrector m_Corrector = new IotaVtiOcrCorrector();

		public string NameAndVersion()
		{
			return "Generic IOTA-VTI OCR v1.0";
		}

		public string OSDType()
		{
			return "IOTA-VTI";
		}

		public void Initialize(TimestampOCRData initializationData, IVideoController videoController, int performanceMode)
		{
			m_InitializationData = initializationData;
		    m_VideoController = videoController;
            m_Processor = null;
			m_UseNativePreProcessing = performanceMode > 0;
			m_ForceErrorReport = initializationData.ForceErrorReport;
		}

		public TimestampOCRData InitializationData
		{
			get { return m_InitializationData; }
		}

		public void RefiningFrame(uint[] data, float refiningPercentageLeft)
		{ }

		public void PrepareForMeasurements(uint[] data)
		{
			
		}

		private void DebugPrepareOsdArea(uint[] dataIn, uint[] dataOut, int width, int height)
		{
			// NOTE: Because of different rounding in C++ and C# there may be a difference of "1" between pixels

			uint[] nativeIn = new uint[dataIn.Length];
			uint[] nativeOut = new uint[dataOut.Length];
			uint nativeAverage = 0;

			for (int i = 0; i < dataIn.Length; i++) nativeIn[i] = dataIn[i];

			uint median = dataIn.Median();
			for (int i = 0; i < dataIn.Length; i++)
			{
				int darkCorrectedValue = (int)dataIn[i] - (int)median;
				if (darkCorrectedValue < 0) darkCorrectedValue = 0;
				dataIn[i] = (uint)darkCorrectedValue;
			}

			TangraCore.PrepareImageForOCRSingleStep(nativeIn, nativeOut, width, height, 0, ref nativeAverage);
			Trace.Assert(Math.Abs(median - nativeAverage) <= 1);
			for (int i = 0; i < nativeOut.Length; i++) Trace.Assert(Math.Abs(nativeOut[i] - dataIn[i]) <= 1);

			for (int i = 0; i < dataIn.Length; i++) nativeIn[i] = dataIn[i];

			uint[] blurResult = BitmapFilter.GaussianBlur(dataIn, 8, width, height);

			TangraCore.PrepareImageForOCRSingleStep(nativeIn, nativeOut, width, height, 1, ref nativeAverage);
			for (int i = 0; i < nativeOut.Length; i++) Trace.Assert(Math.Abs(nativeOut[i] - blurResult[i]) <= 1);

			for (int i = 0; i < blurResult.Length; i++) nativeIn[i] = blurResult[i];

			uint average = 128;
			uint[] sharpenResult = BitmapFilter.Sharpen(blurResult, 8, width, height, out average);

			TangraCore.PrepareImageForOCRSingleStep(nativeIn, nativeOut, width, height, 2, ref nativeAverage);
			Trace.Assert(Math.Abs(average - nativeAverage) <= 1);			
			for (int i = 0; i < nativeOut.Length; i++) Trace.Assert(Math.Abs(nativeOut[i] - sharpenResult[i]) <= 1);


			for (int i = 0; i < sharpenResult.Length; i++) nativeIn[i] = sharpenResult[i];
			TangraCore.PrepareImageForOCRSingleStep(nativeIn, nativeOut, width, height, 3, ref nativeAverage);

			// Binerize and Inverse
			for (int i = 0; i < sharpenResult.Length; i++)
			{
				sharpenResult[i] = sharpenResult[i] > average ? (uint)0 : (uint)255;
			}

			for (int i = 0; i < nativeOut.Length; i++) Trace.Assert(Math.Abs(nativeOut[i] - sharpenResult[i]) <= 1);

			for (int i = 0; i < sharpenResult.Length; i++) nativeIn[i] = sharpenResult[i];
			TangraCore.PrepareImageForOCRSingleStep(nativeIn, nativeOut, width, height, 4, ref nativeAverage);
			
			uint[] denoised = BitmapFilter.Denoise(sharpenResult, 8, width, height, out average, false);

			for (int i = 0; i < denoised.Length; i++) Trace.Assert(Math.Abs(nativeOut[i] - denoised[i]) <= 1);

			for (int i = 0; i < denoised.Length; i++) nativeIn[i] = denoised[i];
			TangraCore.PrepareImageForOCRSingleStep(nativeIn, nativeOut, width, height, 5, ref nativeAverage);
			
			for (int i = 0; i < denoised.Length; i++)
			{
				dataOut[i] = denoised[i] < 127 ? (uint)0 : (uint)255;
			}

			for (int i = 0; i < denoised.Length; i++) Trace.Assert(Math.Abs(nativeOut[i] - dataOut[i]) <= 1);


			LargeChunkDenoiser.Process(false, dataOut, width, height);
			LargeChunkDenoiser.Process(true, nativeOut, width, height);

			for (int i = 0; i < dataOut.Length; i++) Trace.Assert(Math.Abs(nativeOut[i] - dataOut[i]) <= 1);
			for (int i = 0; i < nativeOut.Length; i++) dataOut[i] = nativeOut[i];
		}

        private void PrepareOsdArea(uint[] dataIn, uint[] dataOut, uint[] dataDebugNoLChD, int width, int height)
		{
			//DebugPrepareOsdArea(dataIn, dataOut, width, height);
			//return;

			// Split into fields only in the region where IOTA-VTI could be, Then threat as two separate images, and for each of them do:
			// 1) Gaussian blur (small) BitmapFilter.LOW_PASS_FILTER_MATRIX
			// 2) Sharpen BitmapFilter.SHARPEN_MATRIX
			// 3) Binarize - get Average, all below change to 0, all above change to Max (256)
			// 4) De-noise BitmapFilter.DENOISE_MATRIX

			if (m_UseNativePreProcessing)
			{
				TangraCore.PrepareImageForOCR(dataIn, dataOut, width, height);
			}
			else
			{
				uint median = dataIn.Median();
				for (int i = 0; i < dataIn.Length; i++)
				{
					int darkCorrectedValue = (int)dataIn[i] - (int)median;
					if (darkCorrectedValue < 0) darkCorrectedValue = 0;
					dataIn[i] = (uint)darkCorrectedValue;
				}

				uint[] blurResult = BitmapFilter.GaussianBlur(dataIn, 8, width, height);

				uint average = 128;
				uint[] sharpenResult = BitmapFilter.Sharpen(blurResult, 8, width, height, out average);

				// Binerize and Inverse
				for (int i = 0; i < sharpenResult.Length; i++)
				{
					sharpenResult[i] = sharpenResult[i] > average ? (uint)0 : (uint)255;
				}

				uint[] denoised = BitmapFilter.Denoise(sharpenResult, 8, width, height, out average, false);

				for (int i = 0; i < denoised.Length; i++)
				{
					dataOut[i] = denoised[i] < 127 ? (uint)0 : (uint)255;
				}

                Array.Copy(dataOut, dataDebugNoLChD, dataOut.Length);

				LargeChunkDenoiser.Process(false, dataOut, width, height);
			}
		}

        private void PrepareOsdVideoFields(uint[] data)
        {
            for (int y = m_FromLine; y < m_ToLine; y++)
            {
                bool isOddLine = (y - 1) % 2 == 1; // y is zero-based so add/even calculations should be performed counting from zero
                int lineNo = (y - m_FromLine) / 2;
                if (isOddLine)
                    Array.Copy(data, y * m_FieldAreaWidth, m_OddFieldPixels, lineNo * m_FieldAreaWidth, m_FieldAreaWidth);
                else
                    Array.Copy(data, y * m_FieldAreaWidth, m_EvenFieldPixels, lineNo * m_FieldAreaWidth, m_FieldAreaWidth);
            }

            PrepareOsdArea(m_OddFieldPixels, m_OddFieldPixelsPreProcessed, m_OddFieldPixelsDebugNoLChD, m_InitializationData.FrameWidth, m_FieldAreaHeight);
            PrepareOsdArea(m_EvenFieldPixels, m_EvenFieldPixelsPreProcessed, m_EvenFieldPixelsDebugNoLChD, m_InitializationData.FrameWidth, m_FieldAreaHeight);

            m_LatestFrameImage = data;
        }


		public bool ExtractTime(int frameNo, uint[] data, out DateTime time)
		{
            if (m_Processor.IsCalibrated)
            {
                if (m_VideoController != null)
                    m_VideoController.RegisterExtractingOcrTimestamps();

                PrepareOsdVideoFields(data);

                m_Processor.Process(m_OddFieldPixelsPreProcessed, m_FieldAreaWidth, m_FieldAreaHeight, null, frameNo, true);
                IotaVtiTimeStamp oddFieldOSD = m_Processor.CurrentOcredTimeStamp;

                m_Processor.Process(m_EvenFieldPixelsPreProcessed, m_FieldAreaWidth, m_FieldAreaHeight, null, frameNo, false);
                IotaVtiTimeStamp evenFieldOSD = m_Processor.CurrentOcredTimeStamp;

                time = ExtractDateTime(frameNo, oddFieldOSD, evenFieldOSD);
            }
            else
                time = DateTime.MinValue;

            return time != DateTime.MinValue;
		}

        public bool ExtractTime(int frameNo, uint[] oddPixels, uint[] evenPixels, int width, int height, out DateTime time)
	    {
            if (m_Processor.IsCalibrated)
            {
                m_Processor.Process(oddPixels, width, height, null, frameNo, true);
                IotaVtiTimeStamp oddFieldOSD = m_Processor.CurrentOcredTimeStamp;

                m_Processor.Process(evenPixels, width, height, null, frameNo, false);
                IotaVtiTimeStamp evenFieldOSD = m_Processor.CurrentOcredTimeStamp;

                time = ExtractDateTime(frameNo, oddFieldOSD, evenFieldOSD);
            }
            else
                time = DateTime.MinValue;

            return time != DateTime.MinValue;
	    }

        public List<uint[]> GetLearntDigitPatterns()
        {
            var rv = new List<uint[]>();

            if (m_Processor != null && 
                m_Processor.IsCalibrated)
            {
                rv.Add(m_Processor.ZeroDigitPattern);
                rv.Add(m_Processor.OneDigitPattern);
                rv.Add(m_Processor.TwoDigitPattern);
                rv.Add(m_Processor.ThreeDigitPattern);
                rv.Add(m_Processor.FourDigitPattern);
                rv.Add(m_Processor.FiveDigitPattern);
                rv.Add(m_Processor.SixDigitPattern);
                rv.Add(m_Processor.SevenDigitPattern);
                rv.Add(m_Processor.EightDigitPattern);
                rv.Add(m_Processor.NineDigitPattern);
                rv.Add(m_Processor.ThreeEightXorPattern);
                rv.Add(m_Processor.SixEightXorPattern);
                rv.Add(m_Processor.NineEightXorPattern);
            }

            return rv;
        }

	    public int BlockWidth
	    {
            get
            {
                if (m_Processor != null &&
                    m_Processor.IsCalibrated)
                {
                    return m_Processor.BlockWidth;
                }

                return -1;
            }
    	}

        public int BlockHeight
        {
            get
            {
                if (m_Processor != null &&
                    m_Processor.IsCalibrated)
                {
                    return m_Processor.BlockHeight;
                }

                return -1;
            }
        }

	    public bool RequiresConfiguring
		{
			get { return false; }
		}

		public bool RequiresCalibration
		{
			get { return true; }
		}

		public void TryToAutoConfigure(uint[] data)
		{
		}

		private void LocateTopAndBottomLineOfTimestamp(uint[] preProcessedPixels, int imageWidth, int fromHeight, int toHeight, out int bestTopPosition, out int bestBottomPosition)
		{
			int bestTopScope = -1;
			bestBottomPosition = -1;
			bestTopPosition = -1;			
			int bestBottomScope = -1;


			for (int y = fromHeight + 1; y < toHeight - 1; y++)
			{
				int topScore = 0;
				int bottomScore = 0;

				for (int x = 0; x < imageWidth; x++)
				{
					if (preProcessedPixels[x + imageWidth * (y + 1)] < 127 && preProcessedPixels[x + imageWidth * y] > 127)
					{
						topScore++;
					}

					if (preProcessedPixels[x + imageWidth * (y - 1)] < 127 && preProcessedPixels[x + imageWidth * y] > 127)
					{
						bottomScore++;
					}
				}

				if (topScore > bestTopScope)
				{
					bestTopScope = topScore;
					bestTopPosition = y;
				}

				if (bottomScore > bestBottomScope)
				{
					bestBottomScope = bottomScore;
					bestBottomPosition = y;
				}
			}			
		}

        private bool LocateTimestampPosition(uint[] data)
        {
            uint[] preProcessedPixels = new uint[data.Length];
            Array.Copy(data, preProcessedPixels, data.Length);
			
			// Process the image
			uint median = preProcessedPixels.Median();
			for (int i = 0; i < preProcessedPixels.Length; i++)
			{
				int darkCorrectedValue = (int)preProcessedPixels[i] - (int)median;
				if (darkCorrectedValue < 0) darkCorrectedValue = 0;
				preProcessedPixels[i] = (uint)darkCorrectedValue;
			}

			if (median > 250)
			{
				InitiazliationError = "The background is too bright.";
				return false;
			}

			uint[] blurResult = BitmapFilter.GaussianBlur(preProcessedPixels, 8, m_InitializationData.FrameWidth, m_InitializationData.FrameHeight);
			uint average = 128;
			uint[] sharpenResult = BitmapFilter.Sharpen(blurResult, 8, m_InitializationData.FrameWidth, m_InitializationData.FrameHeight, out average);

			// Binerize and Inverse
			for (int i = 0; i < sharpenResult.Length; i++)
			{
				sharpenResult[i] = sharpenResult[i] > average ? (uint)0 : (uint)255;
			}
			uint[] denoised = BitmapFilter.Denoise(sharpenResult, 8, m_InitializationData.FrameWidth, m_InitializationData.FrameHeight, out average, false);

			for (int i = 0; i < denoised.Length; i++)
			{
				preProcessedPixels[i] = denoised[i] < 127 ? (uint)0 : (uint)255;
			}

			int bestBottomPosition = -1;
			int bestTopPosition = -1;
			LocateTopAndBottomLineOfTimestamp(
				preProcessedPixels, 
				m_InitializationData.FrameWidth,  
				m_InitializationData.FrameHeight / 2 + 1, 
				m_InitializationData.FrameHeight, 
				out bestTopPosition, 
				out bestBottomPosition);

	        if (bestBottomPosition - bestTopPosition < 10 || bestBottomPosition - bestTopPosition > 60)
	        {
				if (m_ForceErrorReport)
				{
					if (m_ForceErrorReport &&
						!m_CalibrationImages.ContainsKey("LocateTimestampPositionOrg.bmp"))
					{
						uint[] pixelsOriginal = new uint[data.Length];
						Array.Copy(data, pixelsOriginal, data.Length);
						m_CalibrationImages.Add("LocateTimestampPositionOrg.bmp", pixelsOriginal);

						uint[] pixelsPreProcessed = new uint[data.Length];
						Array.Copy(preProcessedPixels, pixelsPreProcessed, data.Length);
						m_CalibrationImages.Add("LocateTimestampPositionProcessed.bmp", pixelsPreProcessed);
					}					
				}

				InitiazliationError = "Cannot locate the OSD timestamp on the frame.";
		        return false;
	        }

	        m_FromLine = bestTopPosition - 10;
            m_ToLine = bestBottomPosition + 10;
            if (m_ToLine > m_InitializationData.FrameHeight)
                m_ToLine = m_InitializationData.FrameHeight - 2;

            if ((m_ToLine - m_FromLine) %2 == 1)
            {
                if (m_FromLine % 2 == 1)
                    m_FromLine--;
                else
                    m_ToLine++;
            }

			#region We need to make sure that the two fields have the same top and bottom lines

			// Create temporary arrays so the top/bottom position per field can be further refined
			m_FieldAreaHeight = (m_ToLine - m_FromLine) / 2;
			m_FieldAreaWidth = m_InitializationData.FrameWidth;
			m_OddFieldPixels = new uint[m_InitializationData.FrameWidth * m_FieldAreaHeight];
			m_EvenFieldPixels = new uint[m_InitializationData.FrameWidth * m_FieldAreaHeight];
			m_OddFieldPixelsPreProcessed = new uint[m_InitializationData.FrameWidth * m_FieldAreaHeight];
			m_EvenFieldPixelsPreProcessed = new uint[m_InitializationData.FrameWidth * m_FieldAreaHeight];
            m_OddFieldPixelsDebugNoLChD = new uint[m_InitializationData.FrameWidth * m_FieldAreaHeight];
            m_EvenFieldPixelsDebugNoLChD = new uint[m_InitializationData.FrameWidth * m_FieldAreaHeight];

			int[] DELTAS = new int[] { 0, -1, 1 };
	        int fromLineBase = m_FromLine;
			int toLineBase = m_ToLine;
	        bool matchFound = false;

			PrepareOsdVideoFields(data);

	        for (int deltaIdx = 0; deltaIdx < DELTAS.Length; deltaIdx++)
	        {
		        m_FromLine = fromLineBase + DELTAS[deltaIdx];
				m_ToLine = toLineBase + DELTAS[deltaIdx];
				
				int bestBottomPositionOdd = -1;
				int bestTopPositionOdd = -1;
				int bestBottomPositionEven = -1;
				int bestTopPositionEven = -1;

				LocateTopAndBottomLineOfTimestamp(
					m_OddFieldPixelsPreProcessed,
					m_FieldAreaWidth, 1, m_FieldAreaHeight - 1, 
					out bestTopPositionOdd, out bestBottomPositionOdd);

				LocateTopAndBottomLineOfTimestamp(
					m_EvenFieldPixelsPreProcessed,
					m_FieldAreaWidth, 1, m_FieldAreaHeight - 1, 
					out bestTopPositionEven, out bestBottomPositionEven);

				if (bestBottomPositionOdd == bestBottomPositionEven &&
				    bestTopPositionOdd == bestTopPositionEven)
				{
					matchFound = true;
					m_FromLine = fromLineBase;
					m_ToLine = toLineBase;

					break;
				}
			}
			#endregion

			m_TVSafeModeGuess = m_ToLine + (m_ToLine - m_FromLine) / 2 < m_InitializationData.FrameHeight;

            return true;
        }

        private void TryInitializeProcessor(uint[] data)
        {
	        InitiazliationError = null;

            if (m_Processor == null)
            {
                if (LocateTimestampPosition(data))
                {
                    m_FieldAreaHeight = (m_ToLine - m_FromLine) / 2;
                    m_FieldAreaWidth = m_InitializationData.FrameWidth;
                    m_OddFieldPixels = new uint[m_InitializationData.FrameWidth * m_FieldAreaHeight];
                    m_EvenFieldPixels = new uint[m_InitializationData.FrameWidth * m_FieldAreaHeight];
                    m_OddFieldPixelsPreProcessed = new uint[m_InitializationData.FrameWidth * m_FieldAreaHeight];
                    m_EvenFieldPixelsPreProcessed = new uint[m_InitializationData.FrameWidth * m_FieldAreaHeight];
                    m_OddFieldPixelsDebugNoLChD = new uint[m_InitializationData.FrameWidth * m_FieldAreaHeight];
                    m_EvenFieldPixelsDebugNoLChD = new uint[m_InitializationData.FrameWidth * m_FieldAreaHeight];

                    m_InitializationData.OSDFrame.Width = m_FieldAreaWidth;
                    m_InitializationData.OSDFrame.Height = m_FieldAreaHeight;

					m_Processor = new IotaVtiOcrProcessor(m_TVSafeModeGuess);
                }
            }
        }

		public string InitiazliationError { get; private set; }

		public bool ProcessCalibrationFrame(int frameNo, uint[] data)
		{			
            if (m_Processor == null)
		        TryInitializeProcessor(data);

			if (m_Processor == null)
				return false;

			bool wasCalibrated = m_Processor.IsCalibrated;

		    if (m_ForceErrorReport || !m_Processor.IsCalibrated)
		        PrepareOsdVideoFields(data);

			if (m_ForceErrorReport || !m_Processor.IsCalibrated)
                m_Processor.Process(m_OddFieldPixelsPreProcessed, m_FieldAreaWidth, m_FieldAreaHeight, null, frameNo, true);

			if (m_ForceErrorReport || !m_Processor.IsCalibrated)
                m_Processor.Process(m_EvenFieldPixelsPreProcessed, m_FieldAreaWidth, m_FieldAreaHeight, null, frameNo, false);

			if (m_ForceErrorReport || !m_Processor.IsCalibrated)
            {
				uint[] pixelsEven = new uint[m_FieldAreaWidth * m_FieldAreaHeight];
				Array.Copy(m_EvenFieldPixelsPreProcessed, pixelsEven, m_EvenFieldPixelsPreProcessed.Length);
				m_CalibrationImages.Add(string.Format(@"{0}-even.bmp", frameNo.ToString("0000000")), pixelsEven);

				uint[] pixelsOdd = new uint[m_FieldAreaWidth * m_FieldAreaHeight];
				Array.Copy(m_OddFieldPixelsPreProcessed, pixelsOdd, m_OddFieldPixelsPreProcessed.Length);
				m_CalibrationImages.Add(string.Format(@"{0}-odd.bmp", frameNo.ToString("0000000")), pixelsOdd);

				uint[] pixelsEvenOrg = new uint[m_FieldAreaWidth * m_FieldAreaHeight];
				Array.Copy(m_EvenFieldPixels, pixelsEvenOrg, m_EvenFieldPixels.Length);
				m_CalibrationImages.Add(string.Format(@"ORG-{0}-even.bmp", frameNo.ToString("0000000")), pixelsEvenOrg);

				uint[] pixelsOddOrg = new uint[m_FieldAreaWidth * m_FieldAreaHeight];
				Array.Copy(m_OddFieldPixels, pixelsOddOrg, m_OddFieldPixelsPreProcessed.Length);
				m_CalibrationImages.Add(string.Format(@"ORG-{0}-odd.bmp", frameNo.ToString("0000000")), pixelsOddOrg);

                uint[] pixelsEvenNoLChD = new uint[m_FieldAreaWidth * m_FieldAreaHeight];
                Array.Copy(m_EvenFieldPixelsDebugNoLChD, pixelsEvenNoLChD, m_EvenFieldPixels.Length);
                m_CalibrationImages.Add(string.Format(@"NLChD-{0}-even.bmp", frameNo.ToString("0000000")), pixelsEvenNoLChD);

                uint[] pixelsOddNoLChD = new uint[m_FieldAreaWidth * m_FieldAreaHeight];
                Array.Copy(m_OddFieldPixelsDebugNoLChD, pixelsOddNoLChD, m_OddFieldPixelsPreProcessed.Length);
                m_CalibrationImages.Add(string.Format(@"NLChD-{0}-odd.bmp", frameNo.ToString("0000000")), pixelsOddNoLChD);
            }

			if (!wasCalibrated && m_Processor.IsCalibrated)
				m_Corrector.Reset(m_Processor.VideoFormat);

		    return m_Processor.IsCalibrated && !m_ForceErrorReport;
		}

		public bool ProcessCalibrationFrame(int frameNo, uint[] oddPixels, uint[] evenPixels, int width, int height, bool isTVSafeModeGuess)
	    {
			if (m_Processor == null)
			{
				m_FieldAreaHeight = height;
				m_FieldAreaWidth = width;
				m_OddFieldPixels = new uint[m_FieldAreaWidth * m_FieldAreaHeight];
				m_EvenFieldPixels = new uint[m_FieldAreaWidth * m_FieldAreaHeight];
				m_OddFieldPixelsPreProcessed = new uint[m_FieldAreaWidth * m_FieldAreaHeight];
				m_EvenFieldPixelsPreProcessed = new uint[m_FieldAreaWidth * m_FieldAreaHeight];

				m_Processor = new IotaVtiOcrProcessor(isTVSafeModeGuess);
			}

			bool wasCalibrated = m_Processor.IsCalibrated;

            if (!m_Processor.IsCalibrated)
                m_Processor.Process(oddPixels, width, height, null, frameNo, true);

            if (!m_Processor.IsCalibrated)
                m_Processor.Process(evenPixels, width, height, null, frameNo, false);

			if (!wasCalibrated && m_Processor.IsCalibrated)
				m_Corrector.Reset(m_Processor.VideoFormat);

            return m_Processor.IsCalibrated;
	    }

	    public Dictionary<string, uint[]> GetCalibrationReportImages()
		{
			return m_CalibrationImages;
		}

		public List<string> GetCalibrationErrors()
		{
			return m_CalibrationErrors;
		}

        public uint[] GetLastUnmodifiedImage()
        {
            return m_LatestFrameImage;
        }
	     
        private DateTime ExtractDateTime(int frameNo, IotaVtiTimeStamp oddFieldOSD, IotaVtiTimeStamp evenFieldOSD)
        {
            bool failedValidation = false;
            string failedReason = null;

	        if (oddFieldOSD == null || evenFieldOSD == null)
		        return DateTime.MinValue;

            if (oddFieldOSD.FrameNumber != evenFieldOSD.FrameNumber - 1 &&
                oddFieldOSD.FrameNumber != evenFieldOSD.FrameNumber + 1)
            {
                // Video fields are not consequtive
                failedValidation = true;
                failedReason = "Video fields are not consequtive";
            }

            try
            {
				DateTime oddFieldTimestamp = new DateTime(1, 1, 1, oddFieldOSD.Hours, oddFieldOSD.Minutes, oddFieldOSD.Seconds).AddMilliseconds(Math.Min(10000, oddFieldOSD.Milliseconds10) / 10.0f);
				DateTime evenFieldTimestamp = new DateTime(1, 1, 1, evenFieldOSD.Hours, evenFieldOSD.Minutes, evenFieldOSD.Seconds).AddMilliseconds(Math.Min(10000, evenFieldOSD.Milliseconds10) / 10.0f);

                double fieldDuration = Math.Abs(new TimeSpan(oddFieldTimestamp.Ticks - evenFieldTimestamp.Ticks).TotalMilliseconds);

                if (m_Processor.VideoFormat.Value == VideoFormat.PAL &&
                    (Math.Abs(fieldDuration - IotaVtiOcrProcessor.FIELD_DURATION_PAL) > 1.0))
                {
                    // PAL field is not 20 ms
                    failedValidation = true;
                    failedReason = string.Format("PAL field is not 20 ms. It is {0} ms", fieldDuration);
                }

                if (m_Processor.VideoFormat.Value == VideoFormat.NTSC &&
                    (Math.Abs(fieldDuration - IotaVtiOcrProcessor.FIELD_DURATION_NTSC) > 1.0))
                {
                    // NTSC field is not 16.68 ms
                    failedValidation = true;
                    failedReason = string.Format("NTSC field is not 16.68 ms. It is {0} ms", fieldDuration);
                }

                int oddEvenFieldDirection = oddFieldTimestamp.Ticks - evenFieldTimestamp.Ticks < 0 ? -1 : 1;
				if (m_Corrector.OddEvenFieldDirectionIsKnown() && 
					oddEvenFieldDirection != m_Corrector.GetOddEvenFieldDirection())
                {
                    // Field timestamps are wrong have changed order (did they swap)?
                    failedValidation = true;
                    failedReason = "Field timestamps are wrong have changed order (did they swap)?";
                }

				if (failedValidation)
				{
				    string correctionInfo;
                    failedValidation = !m_Corrector.TryToCorrect(frameNo, oddFieldOSD, evenFieldOSD, m_Processor.EvenBeforeOdd, ref oddFieldTimestamp, ref evenFieldTimestamp, out correctionInfo);
				    failedReason += ". " + correctionInfo;
				}

                if (failedValidation)
                {
					string errorText = string.Format("ORC ERR: FrameNo: {0}, Odd Timestamp: {1}:{2}:{3}.{4} {5}, Even Timestamp: {6}:{7}:{8}.{9} {10}, {11}",
						frameNo, oddFieldOSD.Hours, oddFieldOSD.Minutes, oddFieldOSD.Seconds, oddFieldOSD.Milliseconds10, oddFieldOSD.FrameNumber,
                        evenFieldOSD.Hours, evenFieldOSD.Minutes, evenFieldOSD.Seconds, evenFieldOSD.Milliseconds10, evenFieldOSD.FrameNumber, failedReason);

	                Trace.WriteLine(errorText);

					if (m_CalibrationErrors.Count < 16)
					{
					    var copy = new List<uint>();
					    copy.AddRange(m_Processor.CurrentImage);
                        m_CalibrationImages.Add(string.Format("ocrerr_{0}.bmp", frameNo), copy.ToArray());
						m_CalibrationErrors.Add(errorText);
					}

	                if (m_VideoController != null)
						m_VideoController.RegisterOcrError();
                }
                else
                    m_Corrector.RegisterSuccessfulTimestamp(frameNo, oddFieldOSD, evenFieldOSD, oddFieldTimestamp, evenFieldTimestamp);

                if (oddFieldOSD.FrameNumber == evenFieldOSD.FrameNumber - 1)
                {
                    return failedValidation ? DateTime.MinValue : oddFieldTimestamp;
                }
                else
                {
                    return failedValidation ? DateTime.MinValue : evenFieldTimestamp;
                }
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.GetFullStackTrace());

                return DateTime.MinValue;
            }
        }

		public void DrawLegend(Graphics graphics)
		{
			if (m_Processor != null &&
			    m_Processor.IsCalibrated)
			{
				int blockWidth = m_Processor.BlockWidth;
				int blockHeight = 2 * m_Processor.BlockHeight;
				int yTop = m_FromLine + 2 * m_Processor.BlockOffsetY(false /* It doesn't matter which one we use here even or odd */ );

				int[] blockIdsToDraw = new int[] { 3, 4, 6, 7, 9, 10, 12, 13, 14, 15, 17, 18, 19, 20 };

				for (int i = 0; i < blockIdsToDraw.Length; i++)
				{
					int xLeft = m_Processor.BlockOffsetsX[blockIdsToDraw[i]];

					if (xLeft > 0)
						graphics.DrawRectangle(Pens.LightSlateGray, xLeft, yTop, blockWidth, blockHeight);
				}

				blockIdsToDraw = new int[] { 1, 22, 23, 24, 25, 26, 27, 28 };

				for (int i = 0; i < blockIdsToDraw.Length; i++)
				{
					int xLeft = m_Processor.BlockOffsetsX[blockIdsToDraw[i]];

					if (xLeft > 0)
						graphics.DrawRectangle(Pens.LightSlateGray, xLeft, yTop, blockWidth, blockHeight);
				}
				
			}
		}
	}
}
