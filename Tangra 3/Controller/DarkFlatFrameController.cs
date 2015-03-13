﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tangra.Helpers;
using Tangra.Model.Config;
using Tangra.Model.Context;
using Tangra.Model.Image;
using Tangra.PInvoke;
using nom.tam.fits;
using nom.tam.util;

namespace Tangra.Controller
{
	public class DarkFlatFrameController
	{
		private Form m_MainFormView;
		private VideoController m_VideoController;

		public DarkFlatFrameController(Form mainFormView, VideoController videoController)
		{
			m_MainFormView = mainFormView;
			m_VideoController = videoController;			
		}

		public bool HasDarkFrameBytes
		{
			get { return TangraCore.PreProcessors.PreProcessingHasDarkFrameSet(); }
		}

		private bool LoadDarkFlatOrBiasFrameInternal(string title, ref float[,] pixels, ref float medianValue)
		{
			string filter = "FITS Image 16 bit (*.fit;*.fits)|*.fit;*.fits";

			string fileName;
			if (m_VideoController.ShowOpenFileDialog(title, filter, out fileName) == DialogResult.OK &&
				File.Exists(fileName))
			{
				
				Type pixelDataType;

				return FITSHelper.LoadFloatingPointFitsFile(
					fileName,
					out pixels,
					out medianValue,
					out pixelDataType,
					delegate(BasicHDU imageHDU)
					{
						if (
							imageHDU.Axes.Count() != 2 ||
							imageHDU.Axes[0] != TangraContext.Current.FrameHeight ||
							imageHDU.Axes[1] != TangraContext.Current.FrameWidth)
						{
							m_VideoController.ShowMessageBox(
								"Selected image may has a different frame size from the currently loaded video.", "Tangra",
								MessageBoxButtons.OK, MessageBoxIcon.Error);

                            return false;
						}

						bool isFloatingPointImage = false;
						Array dataArray = (Array)imageHDU.Data.DataArray;
						object entry = dataArray.GetValue(0);
						if (entry is float[])
							isFloatingPointImage = true;
						else if (entry is Array)
						{
							isFloatingPointImage = ((Array)entry).GetValue(0) is float;
						}

						if (!isFloatingPointImage && imageHDU.BitPix != 16)
						{
							if (m_VideoController.ShowMessageBox(
								"Selected image data type may not be compatible with the currently loaded video. Do you wish to continue?", "Tangra",
								MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.No)
							{
								return false;
							}
						}

						return true;
					});
			}

			return false;
		}

		public void LoadDarkFrame()
		{
			float[,] pixels = new float[0, 0];
			float medianValue = 0;

			if (LoadDarkFlatOrBiasFrameInternal("Load dark frame ...", ref pixels, ref medianValue))
			{
				UsageStats.Instance.DarkFramesUsed++;
				UsageStats.Instance.Save();

				TangraCore.PreProcessors.AddDarkFrame(pixels, medianValue);

				TangraContext.Current.DarkFrameLoaded = true;
				m_VideoController.UpdateViews();

				m_VideoController.RefreshCurrentFrame();
			}
		}

		public void LoadFlatFrame()
		{
			float[,] pixels = new float[0, 0];
			float medianValue = 0;

			if (LoadDarkFlatOrBiasFrameInternal("Load flat frame ...", ref pixels, ref medianValue))
			{
				UsageStats.Instance.FlatFramesUsed++;
				UsageStats.Instance.Save();

				TangraCore.PreProcessors.AddFlatFrame(pixels, medianValue);

				TangraContext.Current.FlatFrameLoaded = true;
				m_VideoController.UpdateViews();

				m_VideoController.RefreshCurrentFrame();
			}
		}

        public void LoadBiasFrame()
        {
            float[,] pixels = new float[0, 0];
            float medianValue = 0;

            if (LoadDarkFlatOrBiasFrameInternal("Load bias frame ...", ref pixels, ref medianValue))
            {
                UsageStats.Instance.BiasFramesUsed++;
                UsageStats.Instance.Save();

                TangraCore.PreProcessors.AddBiasFrame(pixels, medianValue);

                TangraContext.Current.BiasFrameLoaded = true;
                m_VideoController.UpdateViews();

                m_VideoController.RefreshCurrentFrame();
            }   
        }
	}
}
