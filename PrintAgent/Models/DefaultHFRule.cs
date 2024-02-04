using Common.Api;
using Common.Models;
using DryIoc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintAgent.Models
{
    public class DefaultHFRule : IRule
    {
        private List<HotFolder> _hotFolders = new();
        private List<Rule> _rules = new();

        private readonly string[] COLORS = { "BL", "DK", "LT" };
        private readonly ApiClient _apiClient;

        public DefaultHFRule(ApiClient apiClient)
        {
            _apiClient = apiClient;
            LoadHotFolders();
            LoadRules();
        }

        private void LoadHotFolders()
        {

            // var productTypes = await _apiClient.ListProductTypes();


            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.HotFolderRoot)
                && Directory.Exists(Properties.Settings.Default.HotFolderRoot))
            {
                var dirs = Directory.EnumerateDirectories(Properties.Settings.Default.HotFolderRoot);

                foreach (var dir in dirs)
                {
                    var colorNameParts = Path.GetFileName(dir).Split('-');
                    if (colorNameParts.Length != 2)
                    {
                        continue;
                    }

                    var color = colorNameParts[0].ToUpper();
                    var name = colorNameParts[1];

                    if (COLORS.Contains(color))
                    {
                        _hotFolders.Add(new HotFolder
                        {
                            Color = color,
                            Name = name,
                            Path = dir
                        });
                    }
                }
            }
        }

        private void LoadRules()
        {
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.RulesFile) && File.Exists(Properties.Settings.Default.RulesFile))
            {
                var fileInfo = new FileInfo(Properties.Settings.Default.RulesFile)
                {
                    IsReadOnly = true
                };

                using var excel = new ExcelPackage(fileInfo);
                foreach (var worksheet in excel.Workbook.Worksheets)
                {
                    var workSheetName = worksheet.Name;
                    var numRows = worksheet.Dimension.Rows;
                    var numCols = worksheet.Dimension.Columns;

                    for (int row = 1; row <= numRows; row++)
                    {

                        if (row == 1)
                            continue;

                        var rowData = new string[numCols];

                        for (int col = 1; col <= numCols; col++)
                        {
                            var rowValue = worksheet.Cells[row, col].Value.ToString();
                            rowData[col - 1] = rowValue;
                        }

                        _rules.Add(new Rule
                        {
                            Option = workSheetName,
                            HotFolder = rowData[2],
                            Name = rowData[0],
                            SkuPart = rowData[1]
                        });
                    }
                }
            }
        }

        public string GetHotFolder(PrintRequest printRequest)
        {
            if (_rules == null || !_rules.Any())
            {
                return string.Empty;
            }

            Rule hfProductType = null;
            Rule hfFit = null;

            var color = ParseColor(printRequest.LineItem.Sku);

            var hotFolder = "";
            var hfProductTypes = _rules.Where(r => r.Option == "ProductTypes" && r.Name == printRequest.Variant.Product.ProductType).ToArray();
            var hfColors = _rules.Where(r => r.Option == "Colors" && r.SkuPart == color).ToArray();
            var hfFits = _rules.Where(r => r.Option == "Fits" && r.Name == printRequest.Variant.Option2).ToArray();
            var size = printRequest.Variant.Option3;

            if (!hfProductTypes.Any())
            {
                return string.Empty;
            }
            hfProductType = hfProductTypes.FirstOrDefault();


            if (hfFits.Any())
            {
                hfFit = hfFits.First();
                hotFolder = hfFit.HotFolder;
            }

            if (string.IsNullOrWhiteSpace(hotFolder))
            {
                hotFolder = hfProductType.HotFolder;
            }


            // Special cases
            if (hfProductType.Name == "Kids")
            {
                hotFolder = "MPL";

                if (int.TryParse(size, out var sizeInt))
                {
                    if (sizeInt >= 4)
                    {
                        hotFolder = "MPL";
                    }
                    else
                    {
                        hotFolder = "SHO";
                    }
                }
            }

            if (hfFit.Name == "Maple" || hfFit.Name == "Maple Tee")
            {
                if (size == "XS" || size == "S")
                {
                    hotFolder = "MPL";
                }
                else
                {
                    hotFolder = "TEE";
                }
            }

            if (hfFit.Name == "Crop Jumper")
            {
                if (size == "XS" || size == "S")
                {
                    hotFolder = "SHO";
                }
                else
                {
                    hotFolder = "HOD";
                }
            }

            if (hotFolder.ToLower().Contains("size"))
            {
                return string.Empty;
            }

            return $"{hfColors.FirstOrDefault()?.HotFolder}-{hotFolder}";
        }

        private string ParseColor(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
            {
                return string.Empty;
            }

            var parts = sku.Trim().Split('-');

            var color = parts[parts.Length - 1].Trim();

            if (COLORS.Contains(color.ToUpper()))
            {
                return color.ToUpper();
            }

            return string.Empty;
        }
    }
}
