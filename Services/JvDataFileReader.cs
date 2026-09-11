using System.Globalization;
using Cicci.SampleManager.Models.Plotting;

namespace Cicci.SampleManager.Services;

public static class JvDataFileReader
{
    // Reads the raw JV curve from one ARKEO data file.
    public static async Task<JvPlotData> ReadAsync(string? dataPath)
    {
        var result = new JvPlotData();

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

        using var reader = new StreamReader(dataPath);

        var insideDataSection = false;
        string[]? headers = null;

        int forwardVoltageIndex = -1;
        int forwardCurrentIndex = -1;
        int reverseVoltageIndex = -1;
        int reverseCurrentIndex = -1;

        double forwardCurrentScale = 1;
        double reverseCurrentScale = 1;

        string? line;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            var trimmedLine = line.Trim();

            if (!insideDataSection)
            {
                if (trimmedLine.Equals(
                    "## Data ##",
                    StringComparison.OrdinalIgnoreCase))
                {
                    insideDataSection = true;
                }

                continue;
            }

            if (headers == null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var possibleHeaders = line.Split('\t');

                var fwVoltage =
                    FindColumn(possibleHeaders, "V_FW");

                var fwCurrent =
                    FindColumn(possibleHeaders, "J_FW");

                var rvVoltage =
                    FindColumn(possibleHeaders, "V_RV");

                var rvCurrent =
                    FindColumn(possibleHeaders, "J_RV");

                if ((fwVoltage >= 0 && fwCurrent >= 0) ||
                    (rvVoltage >= 0 && rvCurrent >= 0))
                {
                    headers = possibleHeaders;

                    forwardVoltageIndex = fwVoltage;
                    forwardCurrentIndex = fwCurrent;

                    reverseVoltageIndex = rvVoltage;
                    reverseCurrentIndex = rvCurrent;

                    if (forwardCurrentIndex >= 0)
                    {
                        forwardCurrentScale =
                            GetCurrentDensityScale(
                                headers[forwardCurrentIndex]);
                    }

                    if (reverseCurrentIndex >= 0)
                    {
                        reverseCurrentScale =
                            GetCurrentDensityScale(
                                headers[reverseCurrentIndex]);
                    }
                }

                continue;
            }

            if (trimmedLine.StartsWith("##"))
                break;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            var values = line.Split('\t');

            AddPoint(
                result.Forward,
                values,
                forwardVoltageIndex,
                forwardCurrentIndex,
                forwardCurrentScale);

            AddPoint(
                result.Reverse,
                values,
                reverseVoltageIndex,
                reverseCurrentIndex,
                reverseCurrentScale);
        }

        if (headers == null)
        {
            result.Error =
                "The JV data columns could not be found after ## Data ##.";

            return result;
        }

        if (!result.HasData)
        {
            result.Error =
                "The JV data section does not contain numeric data.";
        }

        return result;
    }

    // Finds a column from the beginning of its header name.
    private static int FindColumn(
        string[] headers,
        string columnName)
    {
        return Array.FindIndex(
            headers,
            header => header
                .Trim()
                .StartsWith(
                    columnName,
                    StringComparison.OrdinalIgnoreCase)
        );
    }

    // Converts current density to mA/cm² for plotting.
    private static double GetCurrentDensityScale(
        string header)
    {
        if (header.Contains(
            "mA/cm",
            StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        if (header.Contains(
            "A/cm",
            StringComparison.OrdinalIgnoreCase))
        {
            return 1000;
        }

        return 1;
    }

    // Adds one point if both voltage and current are valid numbers.
    private static void AddPoint(
        List<JvPlotPoint> points,
        string[] values,
        int voltageIndex,
        int currentIndex,
        double currentScale)
    {
        if (voltageIndex < 0 ||
            currentIndex < 0 ||
            voltageIndex >= values.Length ||
            currentIndex >= values.Length)
        {
            return;
        }

        if (!double.TryParse(
            values[voltageIndex],
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var voltage))
        {
            return;
        }

        if (!double.TryParse(
            values[currentIndex],
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var currentDensity))
        {
            return;
        }

        points.Add(new JvPlotPoint
        {
            X = voltage,
            Y = currentDensity * currentScale
        });
    }
}