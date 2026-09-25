using Cicci.SampleManager.Measurements.Common;

namespace Cicci.SampleManager.Measurements.DarkJV;

public static class DarkJvDataFileReader
{
    public static async Task<DarkJvPlotData> ReadAsync(string? dataPath)
    {
        var result = new DarkJvPlotData();

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

        double[] voltage = file.Data[0];
        double[] current = file.Data[1];

        for(int i = 0; i < voltage.Length; i++)
        {
            result.DarkJV.Add(new DarkJvPlotPoint{
                X = voltage[i],
                Y = current[i]
                }
            );
        }
        return result;

    }

}