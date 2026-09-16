using Cicci.SampleManager.Measurements.Common;

namespace Cicci.SampleManager.Measurements.EQE;

public static class EqeDataFileReader
{
    public static async Task<EqePlotData> ReadAsync(string? dataPath)
    {
        var result = new EqePlotData();

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

        double[] wavelengths= file.Data[0];
        double[] EQE        = file.Data[1];
        double[] J_Device   = file.Data[2];
        double[] Jsc        = file.Data[3];

        for(int i = 0; i < wavelengths.Length; i++)
        {
            result.EQE.Add(new EqePlotPoint{
                X = wavelengths[i],
                Y = EQE[i]
                }
            );

            result.Jsc.Add(new EqePlotPoint{
                X = wavelengths[i],
                Y = double.IsNaN(Jsc[i]) ? 0 : Jsc[i]
                }
            );
        }
        return result;

    }

}