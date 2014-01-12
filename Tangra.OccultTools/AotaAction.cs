﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tangra.OccultTools.OccultWrappers;
using Tangra.SDK;

namespace Tangra.OccultTools
{
	[Serializable]
	public class AotaAction : MarshalByRefObject, ITangraAddinAction
	{
		private ITangraHost m_TangraHost;
		private OccultToolsAddinSettings m_Settings;
		private OccultToolsAddin m_Addin;
	    private IOccultWrapper m_OccultWrapper;

		internal AotaAction(OccultToolsAddinSettings settings, ITangraHost tangraHost, IOccultWrapper occultWrapper, OccultToolsAddin addin)
		{
			m_Addin = addin;
			m_Settings = settings;
			m_TangraHost = tangraHost;
		    m_OccultWrapper = occultWrapper;
		}

		public AddinActionType ActionType
		{
			get { return AddinActionType.LightCurveEventTimeExtractor; }
		}

		public IntPtr Icon
		{
			get { return Properties.Resource.Occult.ToBitmap().GetHbitmap(); }
		}

		public int IconTransparentColorARGB
		{
			get { return System.Drawing.Color.Transparent.ToArgb(); }
		}

	    public void Execute()
	    {
	        ILightCurveDataProvider dataProvider = m_TangraHost.GetLightCurveDataProvider();

	        if (!Directory.Exists(m_Settings.OccultLocation))
	        {
	            string locationFromOW = OccultWatcherHelper.GetConfiguredOccultLocation();
                if (Directory.Exists(locationFromOW))
	            {
	                m_Settings.OccultLocation = locationFromOW;
	                m_Settings.Save();
	            }
                else
	                m_Addin.Configure();
	        }

            if (!m_OccultWrapper.HasSupportedVersionOfOccult(m_Settings.OccultLocation))
	        {
	            ShowIncompatibleOccultVersionErrorMessage();
	            m_Addin.Configure();
	        }

	        if (dataProvider != null)
	        {
                if (m_OccultWrapper.HasSupportedVersionOfOccult(m_Settings.OccultLocation))
		        {
                    AotaReturnValue result = m_OccultWrapper.RunAOTA(dataProvider, m_TangraHost.ParentWindow);

					if (result != null &&
                        result.AreResultsAvailable)
					{
                        dataProvider.SetTimeExtractionEngine(result.AOTAVersion);

						if (result.IsMiss)
						{
							dataProvider.SetNoOccultationEvents();
						}
						else
						{
							for (int i = 0; i < 5; i++)
							{
								if (!result.EventResults[i].IsNonEvent)
								{
									dataProvider.SetFoundOccultationEvent(
										i,
										result.EventResults[i].D_Frame,
										result.EventResults[i].R_Frame,
										result.EventResults[i].D_FrameUncertMinus,
										result.EventResults[i].D_FrameUncertPlus,
										result.EventResults[i].R_FrameUncertMinus,
										result.EventResults[i].R_FrameUncertPlus,
										result.EventResults[i].D_UTC,
										result.EventResults[i].R_UTC);
								}
							}
						}
					}
		        }
		        else
			        ShowIncompatibleOccultVersionErrorMessage();
	        }
	    }

        public void Finalise()
        {
            m_OccultWrapper.EnsureAOTAClosed();
        }

	    private void ShowIncompatibleOccultVersionErrorMessage()
		{
			MessageBox.Show(
				m_TangraHost.ParentWindow,
				"Cannot find a compatible version of Occult in the configured location. Occult version 4.1.0.12 or later is required.",
				"Occult Tools for Tangra",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);			
		}

		public string DisplayName
		{
			get { return "Extract Event Times with AOTA"; }
		}
	}
}
