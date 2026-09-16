using Cicci.SampleManager.Measurements.Common;

namespace Cicci.SampleManager.Measurements.EIS;

public static class EisDataFileReader
{
    public static async Task<EisPlotData> ReadAsync(string? dataPath)
    {
        var result = new EisPlotData();

        if (string.IsNullOrWhiteSpace(dataPath))
        {
            result.Error = "No data file is associated with this measurement.";
            return result;
        }

        dataPath = dataPath.Trim();

        if (!File.Exists(dataPath))
        {
            result.Error = "The data file could not be found.";
            return result;
        }

        ArkeoFile file = await ArkeoFileReader.ReadAsync(dataPath);

        double[] freq       = file.Data[0];
        double[] z_real     = file.Data[1];
        double[] z_imag     = file.Data[2];
        double[] magnitude  = file.Data[3];
        double[] phase      = file.Data[4];

        for(int i = 0; i < freq.Length; i++)
        {
            result.Nyquist.Add(new EisPlotPoint{
                X = z_real[i],
                Y = z_imag[i]
                }
            );

            result.BodeMagnitude.Add(new EisPlotPoint{
                X = freq[i],
                Y = magnitude[i]
                }
            );

            result.BodePhase.Add(new EisPlotPoint{
                X = freq[i],
                Y = phase[i]
                }
            );
        }
        return result;

    }

}