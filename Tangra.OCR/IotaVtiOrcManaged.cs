﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Tangra.Model.Config;
using Tangra.Model.Image;
using Tangra.Model.VideoOperations;
using Tangra.OCR.IotaVtiOsdProcessor;

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
		private int m_FieldAreaHeight;
		private int m_FieldAreaWidth;

		private bool m_TVSafeMode;
        private IotaVtiOcrProcessor m_Processor;

		private Dictionary<string, uint[]> m_CalibrationImages = new Dictionary<string, uint[]>();
	    private uint[] m_LatestFrameImage;

		public string NameAndVersion()
		{
			return "Generic IOTA-VTI OCR v1.0";
		}

		public string OSDType()
		{
			return "IOTA-VTI";
		}

		public void Initialize(TimestampOCRData initializationData, IVideoController videoController)
		{
			m_InitializationData = initializationData;
		    m_VideoController = videoController;
            m_Processor = null;
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

        private void PrepareOsdVideoFields(uint[] data)
        {
            for (int y = m_FromLine; y < m_ToLine; y++)
            {
                bool isOddLine = y % 2 == 1;
                int lineNo = (y - m_FromLine) / 2;
                if (isOddLine)
                    Array.Copy(data, y * m_FieldAreaWidth, m_OddFieldPixels, lineNo * m_FieldAreaWidth, m_FieldAreaWidth);
                else
                    Array.Copy(data, y * m_FieldAreaWidth, m_EvenFieldPixels, lineNo * m_FieldAreaWidth, m_FieldAreaWidth);
            }

            // Split into fields only in the region where IOTA-VTI could be, Then threat as two separate images, and for each of them do:
            // 1) Gaussian blur (small) BitmapFilter.LOW_PASS_FILTER_MATRIX
            // 2) Sharpen BitmapFilter.SHARPEN_MATRIX
            // 3) Binarize - get Average, all below change to 0, all above change to Max (256)
            // 4) De-noise BitmapFilter.DENOISE_MATRIX

            // TODO: Move the Covariance operations into C++ land

	        uint median = m_OddFieldPixels.Median();
			for (int i = 0; i < m_OddFieldPixels.Length; i++)
			{
				int darkCorrectedValue = (int) m_OddFieldPixels[i] - (int) median;
				if (darkCorrectedValue < 0) darkCorrectedValue = 0;
				m_OddFieldPixels[i] = (uint) darkCorrectedValue;
			}

            uint[] blurResult = BitmapFilter.GaussianBlur(m_OddFieldPixels, 8, m_InitializationData.FrameWidth, m_FieldAreaHeight);
            uint average = 128;
            uint[] sharpenResult = BitmapFilter.Sharpen(blurResult, 8, m_InitializationData.FrameWidth, m_FieldAreaHeight, out average);

            // Binerize and Inverse
            for (int i = 0; i < sharpenResult.Length; i++)
            {
                sharpenResult[i] = sharpenResult[i] > average ? (uint)0 : (uint)255;
            }
            uint[] denoised = BitmapFilter.Denoise(sharpenResult, 8, m_InitializationData.FrameWidth, m_FieldAreaHeight, out average, false);

            for (int i = 0; i < denoised.Length; i++)
            {
                m_OddFieldPixelsPreProcessed[i] = denoised[i] < 127 ? (uint)0 : (uint)255;
            }

			median = m_EvenFieldPixels.Median();
			for (int i = 0; i < m_EvenFieldPixels.Length; i++)
			{
				int darkCorrectedValue = (int)m_EvenFieldPixels[i] - (int)median;
				if (darkCorrectedValue < 0) darkCorrectedValue = 0;
				m_EvenFieldPixels[i] = (uint)darkCorrectedValue;
			}
            blurResult = BitmapFilter.GaussianBlur(m_EvenFieldPixels, 8, m_InitializationData.FrameWidth, m_FieldAreaHeight);
            average = 128;
            sharpenResult = BitmapFilter.Sharpen(blurResult, 8, m_InitializationData.FrameWidth, m_FieldAreaHeight, out average);

            // Binerize and Inverse
            for (int i = 0; i < sharpenResult.Length; i++)
            {
                sharpenResult[i] = sharpenResult[i] > average ? (uint)0 : (uint)255;
            }
            denoised = BitmapFilter.Denoise(sharpenResult, 8, m_InitializationData.FrameWidth, m_FieldAreaHeight, out average, false);

            for (int i = 0; i < denoised.Length; i++)
            {
                m_EvenFieldPixelsPreProcessed[i] = denoised[i] < 127 ? (uint)0 : (uint)255;
            }

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

        private void LocateTimestampPosition(uint[] data)
        {
            int bestTopPosition = -1;
            int bestTopScope = -1;
            int bestBottomPosition = -1;
            int bestBottomScope = -1;

            int imageWidth = m_InitializationData.FrameWidth;

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

            for (int y = m_InitializationData.FrameHeight / 2 + 1; y < m_InitializationData.FrameHeight - 1; y++)
            {
                int topScore = 0;
                int bottomScore = 0;

                for (int x = 0; x < m_InitializationData.FrameWidth; x++)
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

            m_FromLine = bestTopPosition - 10;
            m_ToLine = bestBottomPosition + 10;
            if (m_ToLine > m_InitializationData.FrameHeight)
                m_ToLine = m_InitializationData.FrameHeight - 2;

            // NOTE: This is MAGIC!! It ensures that the TOP and BOTTOM position of the OSD in each even and odd field will be the same
            // TODO: This should be ensured by finding the TOP and BOTTOM position of the fields
            if ((m_ToLine - m_FromLine) %2 == 1)
            {
                if (m_FromLine % 2 == 1)
                    m_FromLine--;
                else
                    m_ToLine++;
            }

            m_TVSafeMode = m_ToLine + (m_ToLine - m_FromLine) / 2 < m_InitializationData.FrameHeight;
        }

        private void EnsureProcessorInitialized(uint[] data)
        {
            if (m_Processor == null)
            {
                LocateTimestampPosition(data);

                m_FieldAreaHeight = (m_ToLine - m_FromLine) / 2;
                m_FieldAreaWidth = m_InitializationData.FrameWidth;
                m_OddFieldPixels = new uint[m_InitializationData.FrameWidth * m_FieldAreaHeight];
                m_EvenFieldPixels = new uint[m_InitializationData.FrameWidth * m_FieldAreaHeight];
                m_OddFieldPixelsPreProcessed = new uint[m_InitializationData.FrameWidth * m_FieldAreaHeight];
                m_EvenFieldPixelsPreProcessed = new uint[m_InitializationData.FrameWidth * m_FieldAreaHeight];

                m_InitializationData.OSDFrame.Width = m_FieldAreaWidth;
                m_InitializationData.OSDFrame.Height = m_FieldAreaHeight;

                m_Processor = new IotaVtiOcrProcessor(m_TVSafeMode);
            }
        }

		public bool ProcessCalibrationFrame(int frameNo, uint[] data)
		{
            if (m_Processor == null)
		        EnsureProcessorInitialized(data);

		    if (!m_Processor.IsCalibrated)
		        PrepareOsdVideoFields(data);

            if (!m_Processor.IsCalibrated)
                m_Processor.Process(m_OddFieldPixelsPreProcessed, m_FieldAreaWidth, m_FieldAreaHeight, null, frameNo, true);

            if (!m_Processor.IsCalibrated)
                m_Processor.Process(m_EvenFieldPixelsPreProcessed, m_FieldAreaWidth, m_FieldAreaHeight, null, frameNo, false);

            if (!m_Processor.IsCalibrated)
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
            }

		    return m_Processor.IsCalibrated;
		}

	    public bool ProcessCalibrationFrame(int frameNo, uint[] oddPixels, uint[] evenPixels, int width, int height)
	    {
            if (!m_Processor.IsCalibrated)
                m_Processor.Process(oddPixels, width, height, null, frameNo, true);

            if (!m_Processor.IsCalibrated)
                m_Processor.Process(evenPixels, width, height, null, frameNo, false);

            return m_Processor.IsCalibrated;
	    }

	    public Dictionary<string, uint[]> GetCalibrationReportImages()
		{
			return m_CalibrationImages;
		}

        public uint[] GetLastUnmodifiedImage()
        {
            return m_LatestFrameImage;
        }

		public void AddConfiguration(uint[] data, OCRConfigEntry config)
		{ }

	    private int m_LastVideoFrameNo = -1;
	    private int m_LatestOsdFrameNo = -1;
	    private long m_LastFieldLargerTimestampTicks = -1;

        private DateTime ExtractDateTime(int frameNo, IotaVtiTimeStamp oddFieldOSD, IotaVtiTimeStamp evenFieldOSD)
        {
            bool failedValidation = false;
            if (m_LastVideoFrameNo + 1 == frameNo && m_LatestOsdFrameNo > -1)
            {
                if (m_LatestOsdFrameNo + 1 != Math.Min(oddFieldOSD.FrameNumber, evenFieldOSD.FrameNumber))
                    // Video fields are not consequtive (from previous frame)
                    failedValidation = true;
            }
            m_LatestOsdFrameNo = Math.Max(oddFieldOSD.FrameNumber, evenFieldOSD.FrameNumber);
            m_LastVideoFrameNo = frameNo;

            if (oddFieldOSD.FrameNumber != evenFieldOSD.FrameNumber - 1 &&
                oddFieldOSD.FrameNumber != evenFieldOSD.FrameNumber + 1)
            {
                // Video fields are not consequtive
                failedValidation = true;
            }

            DateTime oddFieldTimestamp = new DateTime(1, 1, 1, oddFieldOSD.Hours, oddFieldOSD.Minutes, oddFieldOSD.Seconds, (int)Math.Round(oddFieldOSD.Milliseconds10 / 10.0f));
            DateTime evenFieldTimestamp = new DateTime(1, 1, 1, evenFieldOSD.Hours, evenFieldOSD.Minutes, evenFieldOSD.Seconds, (int)Math.Round(evenFieldOSD.Milliseconds10 / 10.0f));

            double fieldDuration = Math.Abs(new TimeSpan(oddFieldTimestamp.Ticks - evenFieldTimestamp.Ticks).TotalMilliseconds);
            double fieldDurationFromLastFrame = double.NaN;

            if (m_LastFieldLargerTimestampTicks > -1)
                fieldDurationFromLastFrame = Math.Abs(new TimeSpan(Math.Min(oddFieldTimestamp.Ticks, evenFieldTimestamp.Ticks) - m_LastFieldLargerTimestampTicks).TotalMilliseconds);

            if (m_Processor.VideoFormat.Value == VideoFormat.PAL &&
                (Math.Abs(fieldDuration - IotaVtiOcrProcessor.FIELD_DURATION_PAL) > 1.0 ||
                (!double.IsNaN(fieldDurationFromLastFrame) && Math.Abs(fieldDurationFromLastFrame - IotaVtiOcrProcessor.FIELD_DURATION_PAL) > 1.0)))
            {
                // PAL field is not 20ms
                failedValidation = true;
            }

            if (m_Processor.VideoFormat.Value == VideoFormat.NTSC &&
                (Math.Abs(fieldDuration - IotaVtiOcrProcessor.FIELD_DURATION_NTSC) > 1.0 ||
                (!double.IsNaN(fieldDurationFromLastFrame) && Math.Abs(fieldDurationFromLastFrame - IotaVtiOcrProcessor.FIELD_DURATION_NTSC) > 1.0)))
            {
                // NTSC field is not 20ms
                failedValidation = true;
            }

            if (failedValidation)
            {
                if (m_VideoController != null)
                    m_VideoController.RegisterOcrError();

                return DateTime.MinValue;
            }
            else
            {
                if (oddFieldOSD.FrameNumber == evenFieldOSD.FrameNumber - 1)
                {
                    m_LastFieldLargerTimestampTicks = evenFieldTimestamp.Ticks;
                    return oddFieldTimestamp;
                }
                else
                {
                    m_LastFieldLargerTimestampTicks = oddFieldTimestamp.Ticks;
                    return evenFieldTimestamp;
                }
            }
        }
	}
}