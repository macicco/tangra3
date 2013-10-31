#ifndef PSFFIT_H
#define PSFFIT_H

#define CERTAINTY_CONST (0.5 / 0.03)

enum PSFFittingMethod
{
	NonLinearFit = 0,
	LinearFitOfAveragedModel = 1,
	NonLinearAsymetricFit = 2
};

enum PSFFittingDataRange
{
	DataRange8Bit = 0,
	DataRange12Bit = 1,
	DataRange14Bit = 2
};
	
class PsfFit
{
private:
	long m_xCenter;
	long m_yCenter;

	long m_HalfWidth;
	long m_MatrixSize;
	bool m_IsSolved;
	long m_Saturation;

	double m_X0;
	double m_Y0;
	double m_IBackground;
	double m_IStarMax;
	
	double* m_Residuals;
	
	void SetNewFieldCenterFrom17PixMatrix(int x, int y);
	void SetNewFieldCenterFrom35PixMatrix(int x, int y);
	double GetPSFValueInternal(double x, double y);
	double GetPSFValueInternalAsymetric(double x, double y);
	
	void DoNonLinearFit(unsigned long* intensity, long width);
	void DoNonLinearAsymetricFit(unsigned long* intensity, long width);
	void DoLinearFitOfAveragedModel(unsigned long* intensity, long width);
	
public:
	PsfFit(long xCenter, long yCenter);
	~PsfFit();

	PSFFittingDataRange DataRange;
	PSFFittingMethod FittingMethod;
	
	double R0;
	double RX0;
	double RY0;
	
	bool IsSolved();
	double GetValue(double x, double y);
	double XCenter();
	double YCenter();
	
	long MatrixSize();
	long X0();
	long Y0();
	double X0_Matrix();
	double Y0_Matrix();
	double I0();
	double IMax();
	
	unsigned long Brightness();
	double FWHM();
	double ElongationPercentage();
	
	double Certainty();
	
	void Fit(unsigned long* intensity, long width);
};

#endif // PSFFIT_H
