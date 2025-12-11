using BloodShadow.Core.Operations;
using Course31_trpo.Sources.Structures;
using ObservableCollections;
using OfficeOpenXml;

namespace Course31_trpo.Sources.LoadModules
{
    public class ExcelImportModule : IImportModule
    {
        private static readonly string[] fileTypes = [".xls", ".xlsx"];

        public string Name => "Excel";
        public IReadOnlyObservableList<Report> LoadedItems => _loadedItems;

        private readonly ObservableList<Report> _loadedItems = [];

        public ActionOperation Load(bool useDefaultDir = false)
        {
            return new(async () =>
            {
                List<string> filesToLoad = [];
                if (useDefaultDir)
                {
                    if (!Directory.Exists(MauiProgram.SettingsVM.CurrentValue.DefaultPath.CurrentValue))
                    { Directory.CreateDirectory(MauiProgram.SettingsVM.CurrentValue.DefaultPath.CurrentValue); }
                    foreach (string file in Directory.EnumerateFiles(MauiProgram.SettingsVM.CurrentValue.DefaultPath.CurrentValue, "*", SearchOption.AllDirectories))
                    {
                        foreach (string fileType in fileTypes)
                        {
                            if (file.EndsWith(fileType, StringComparison.OrdinalIgnoreCase))
                            {
                                filesToLoad.Add(file);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    IEnumerable<FileResult?> files = await FilePicker.PickMultipleAsync(new PickOptions()
                    {
                        PickerTitle = MauiProgram.LocalizationManager.Localize<string>(LocalizationKeys.SELECTFILE),
                        FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>()
                        {
                            { DevicePlatform.Android, fileTypes },
                            { DevicePlatform.iOS, fileTypes },
                            { DevicePlatform.macOS, fileTypes },
                            { DevicePlatform.MacCatalyst, fileTypes },
                            { DevicePlatform.tvOS, fileTypes },
                            { DevicePlatform.Tizen, fileTypes },
                            { DevicePlatform.WinUI, fileTypes },
                            { DevicePlatform.watchOS, fileTypes },
                            { DevicePlatform.Unknown, fileTypes },
                        })
                    });
                    foreach (FileResult file in files.Where(x => x != null).Select<FileResult?, FileResult>(x => x)) { filesToLoad.Add(file.FullPath); }
                }
                List<Task> tasks = [];
                foreach (string file in filesToLoad) { tasks.Add(ParseFile(file, _loadedItems)); }
                await Task.WhenAll(tasks);
            });
        }

        private static async Task ParseFile(string filePath, IList<Report> listToAdd)
        {
            using ExcelPackage ep = new(filePath);
            List<Task> tasks = [];
            foreach (ExcelWorksheet worksheet in ep.Workbook.Worksheets) { tasks.Add(ParseSheet(filePath, worksheet, listToAdd)); }
            await Task.WhenAll(tasks);
        }

        private static async Task ParseSheet(string filePath, ExcelWorksheet worksheet, IList<Report> listToAdd)
        {
            List<Task> tasks = [];
            for (int i = 1; ; i += 4)
            {
                int col = i;
                if (worksheet.Cells[1, i].Value != null) { tasks.Add(ParseColumn(filePath, worksheet, listToAdd, col)); }
                else { break; }
            }
            await Task.WhenAll(tasks);
        }

        private static async Task ParseColumn(string filePath, ExcelWorksheet worksheet, IList<Report> listToAdd, int col)
        {
            WriteableReport? currentReport = null;
            for (int row = 1; ; row++)
            {
                if (worksheet.Cells[row, col].Value != null &&
                    worksheet.Cells[row, col + 1].Value != null &&
                    worksheet.Cells[row, col + 2].Value == null &&
                    worksheet.Cells[row, col + 3].Value == null)
                {
                    if (currentReport != null)
                    {
                        listToAdd.Add((Report)currentReport);
                        currentReport = null;
                    }
                    if (!DateTime.TryParseExact(worksheet.Cells[row, col].Value.ToString(), "MMMM", MauiProgram.CurrentCultureInfo,
                        System.Globalization.DateTimeStyles.None, out DateTime dt)) { break; }
                    if (!int.TryParse(worksheet.Cells[row, col + 1].Value.ToString(), out int year)) { break; }

                    currentReport = new()
                    {
                        FilePath = $"{filePath}::{worksheet.Name}:{col}",
                        DateOfSale = new(year, dt.Month, 1),
                        Transfers = []
                    };
                }
                else if (worksheet.Cells[row, col].Value != null &&
                    worksheet.Cells[row, col + 1].Value != null &&
                    worksheet.Cells[row, col + 2].Value != null &&
                    worksheet.Cells[row, col + 3].Value != null)
                {
                    if (currentReport == null) { continue; }
                    if (!int.TryParse(worksheet.Cells[row, col].Value.ToString(), out int transfer)) { break; }
                    if (!byte.TryParse(worksheet.Cells[row, col + 3].Value.ToString(), out byte day)) { break; }
                    currentReport.Value.Transfers.Add(new()
                    {
                        Amount = transfer,
                        Company = worksheet.Cells[row, col + 1].Value.ToString(),
                        Descrition = worksheet.Cells[row, col + 2].Value.ToString(),
                        Day = day
                    });
                }
                else { break; }
                await Task.Yield();
            }
            if (currentReport != null) { listToAdd.Add((Report)currentReport); }
        }

        private struct WriteableReport
        {
            public DateOnly DateOfSale;
            public List<WriteableTransfer> Transfers;
            public string FilePath;

            public struct WriteableTransfer
            {
                public int Amount;
                public string Company;
                public string Descrition;
                public byte Day;
            }

            public static implicit operator Report(WriteableReport wr)
                => new(wr.DateOfSale, [.. wr.Transfers.Select(x => new Transfer(x.Amount, x.Company, x.Descrition, x.Day))], wr.FilePath);
        }
    }
}
