using System.Globalization;
using System.Text;

namespace Cicci.SampleManager.Measurements.Common;

public sealed class ArkeoFile
{
    public Dictionary<string, Dictionary<string, string>> Header { get; init; } = new();

    public string[] ColumnHeaders { get; init; } = [];

    // Column-oriented:
    // Data[0] = entire first column
    // Data[1] = entire second column
    public double[][] Data { get; init; } = [];
}

public static class ArkeoFileReader
{
    private const string HeaderMarker = "## Header ##";
    private const string DataMarker = "## Data ##";

    public static async Task<ArkeoFile> ReadAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(
                "ARKEO data file was not found.",
                filePath);

        string[] lines = await File.ReadAllLinesAsync(
            filePath,
            Encoding.Latin1);

        var header = new Dictionary<string, Dictionary<string, string>>(
            StringComparer.OrdinalIgnoreCase);

        string[] columnHeaders = [];
        var dataRows = new List<double[]>();

        bool inHeader = false;
        bool inData = false;
        bool dataHeaderRead = false;

        string? currentSection = null;

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            if (line.Length == 0)
                continue;

            if (line.Equals(
                HeaderMarker,
                StringComparison.OrdinalIgnoreCase))
            {
                inHeader = true;
                inData = false;
                continue;
            }

            if (line.Equals(
                DataMarker,
                StringComparison.OrdinalIgnoreCase))
            {
                inHeader = false;
                inData = true;
                continue;
            }

            if (inHeader)
            {
                if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    currentSection = line[1..^1].Trim();

                    header[currentSection] =
                        new Dictionary<string, string>(
                            StringComparer.OrdinalIgnoreCase);

                    continue;
                }

                if (currentSection is null)
                    throw new FormatException(
                        $"Header value found before a section: '{line}'.");

                int separatorIndex = rawLine.IndexOf('\t');

                if (separatorIndex < 0)
                    throw new FormatException(
                        $"Invalid header line: '{rawLine}'.");

                string key = rawLine[..separatorIndex].Trim();
                string value = rawLine[(separatorIndex + 1)..].Trim();

                header[currentSection][key] = value;

                continue;
            }

            if (inData)
            {
                string[] values = rawLine.Split(
                    '\t',
                    StringSplitOptions.None);

                if (!dataHeaderRead)
                {
                    columnHeaders = values
                        .Select(value => value.Trim())
                        .ToArray();

                    dataHeaderRead = true;
                    continue;
                }

                if (values.Length != columnHeaders.Length)
                {
                    throw new FormatException(
                        $"Data row contains {values.Length} values, " +
                        $"but {columnHeaders.Length} columns were expected.");
                }

                var row = new double[values.Length];

                for (int column = 0; column < values.Length; column++)
                {
                    row[column] = double.Parse(
                        values[column],
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture);
                }

                dataRows.Add(row);
            }
        }

        if (header.Count == 0)
            throw new FormatException(
                "No ARKEO header was found.");

        if (columnHeaders.Length == 0)
            throw new FormatException(
                "No ARKEO data section was found.");

        var data = new double[columnHeaders.Length][];

        //transpose
        for (int column = 0; column < columnHeaders.Length; column++)
        {
            data[column] = new double[dataRows.Count];

            for (int row = 0; row < dataRows.Count; row++)
            {
                data[column][row] = dataRows[row][column];
            }
        }

        return new ArkeoFile
        {
            Header = header,
            ColumnHeaders = columnHeaders,
            Data = data
        };
    }
}