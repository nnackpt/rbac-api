using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using RBACapi.Data;
using RBACapi.Services.Interfaces;
using RBACapi.Models.Dtos;

namespace RBACapi.Services
{
    public class UserReviewFormService : IUserReviewFormService
    {
        private readonly ApplicationDbContext _context;

        public UserReviewFormService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateUserReviewFormAsync(string? applicationName = null, string? roleName = null)
        {
            var query = _context.UsersAuthorize
                .Include(ua => ua.Application)
                .Include(ua => ua.Role)
                .Where(ua => ua.ACTIVE == true);

            // Filters by application if specified
            if (!string.IsNullOrEmpty(applicationName))
            {
                query = query.Where(ua => ua.Application != null && ua.Application.TITLE == applicationName);
            }

            // Filters by role if specified
            if (!string.IsNullOrEmpty(roleName))
            {
                query = query.Where(ua => ua.Role != null && ua.Role.NAME == roleName);
            }

            var authUsers = await query
                .Select(ua => new
                {
                    ApplicationTitle = ua.Application != null ? ua.Application.TITLE ?? "N/A" : "N/A",
                    RoleName = ua.Role != null ? ua.Role.NAME ?? "N/A" : "N/A",
                    UserId = ua.USERID ?? "",
                    FirstName = ua.FNAME ?? "",
                    LastName = ua.LNAME ?? "",
                    Department = ua.ORG ?? "",
                    SiteFacility = FormatSiteFacility(ua.SITE_CODE, ua.FACT_CODE)
                })
                .OrderBy(ua => ua.ApplicationTitle)
                .ThenBy(ua => ua.RoleName)
                .ThenBy(ua => ua.UserId)
                .ToListAsync();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("User Review Form");
            worksheet.View.ShowGridLines = false;

            worksheet.Row(1).Height = 42.6;
            worksheet.Row(2).Height = 49.2;
            worksheet.Row(3).Height = 34.2;
            worksheet.Row(4).Height = 24;

            var currentRow = 1;

            // Header section
            SetCellValue(worksheet, currentRow, 1, "Report review Application authorized user");
            // MergeCells(worksheet, currentRow, 1, currentRow, 7);
            SetMainHeaderStyle(worksheet, currentRow, 1, currentRow, 7);
            SetFontArial(worksheet, currentRow, 1, currentRow, 7);
            currentRow++;

            var appName = !string.IsNullOrEmpty(applicationName) ? applicationName : "ALL_APPLICATIONS";
            SetCellValue(worksheet, currentRow, 1, "Application");
            SetCellValue(worksheet, currentRow, 2, appName);
            SetCellValue(worksheet, currentRow, 5, "Manager Approve :");

            // set font size for Application and appName
            worksheet.Cells[currentRow, 1].Style.Font.Name = "Arial";
            worksheet.Cells[currentRow, 1].Style.Font.Size = 14;
            worksheet.Cells[currentRow, 2].Style.Font.Name = "Arial";
            worksheet.Cells[currentRow, 2].Style.Font.Size = 14;

            // set font and alignment for "Manager Approve :"
            worksheet.Cells[currentRow, 5].Style.Font.Name = "Arial";
            worksheet.Cells[currentRow, 5].Style.Font.Size = 11;
            worksheet.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            var managerCell = worksheet.Cells[currentRow, 6, currentRow, 7];
            managerCell.Merge = true;
            managerCell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            currentRow++;

            // Date review section
            SetCellValue(worksheet, currentRow, 1, "Instruction: The report list all active user under role and site can access.");
            SetCellValue(worksheet, currentRow, 5, "Date review :");
            // MergeCells(worksheet, currentRow, 1, currentRow, 4);

            SetInstructionStyle(worksheet, currentRow, 1, currentRow, 4);
            // worksheet.Cells[currentRow, 2, currentRow, 4].Style.Font.Size = 11;
            // worksheet.Cells[currentRow, 2, currentRow, 4].Style.Font.Name = "Arial";
            // worksheet.Cells[currentRow, 2, currentRow, 4].Style.Font.Color.SetColor(Color.Black);

            // Config Date review : cells
            var dateReviewRange = worksheet.Cells[currentRow, 5, currentRow, 7];
            dateReviewRange.Style.Font.Name = "Arial";
            dateReviewRange.Style.Font.Size = 11;
            dateReviewRange.Style.Font.Color.SetColor(Color.Black);
            dateReviewRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            dateReviewRange.Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;

            var dateCell = worksheet.Cells[currentRow, 6, currentRow, 7];
            dateCell.Merge = true;
            dateCell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            currentRow++;

            SetCellValue(worksheet, currentRow, 1, "Under review column :");
            SetInstructionStyle(worksheet, currentRow, 1, currentRow, 1);
            // worksheet.Cells[currentRow, 2, currentRow, 7].Style.Font.Size = 11;
            // worksheet.Cells[currentRow, 2, currentRow, 7].Style.Font.Name = "Arial";
            // worksheet.Cells[currentRow, 2, currentRow, 7].Style.Font.Color.SetColor(Color.Black);
            currentRow++;

            SetCellValue(worksheet, currentRow, 1, "Select 'Active' if user is in your organize and you consider that he/she need to access application by this role.");
            // MergeCells(worksheet, currentRow, 1, currentRow, 7);
            SetInstructionStyle(worksheet, currentRow, 1, currentRow, 1);
            // worksheet.Cells[currentRow, 2, currentRow, 7].Style.Font.Size = 11;
            // worksheet.Cells[currentRow, 2, currentRow, 7].Style.Font.Name = "Arial";
            // worksheet.Cells[currentRow, 2, currentRow, 7].Style.Font.Color.SetColor(Color.Black);
            currentRow++;

            SetCellValue(worksheet, currentRow, 1, "Select 'Inactive' if user is not in your organize or you consider that he/she not need to access the application by this role.");
            // MergeCells(worksheet, currentRow, 1, currentRow, 7);
            SetInstructionStyle(worksheet, currentRow, 1, currentRow, 1);
            // worksheet.Cells[currentRow, 2, currentRow, 7].Style.Font.Size = 11;
            // worksheet.Cells[currentRow, 2, currentRow, 7].Style.Font.Name = "Arial";
            // worksheet.Cells[currentRow, 2, currentRow, 7].Style.Font.Color.SetColor(Color.Black);
            currentRow++;

            // Empty rows
            currentRow += 2;

            // Group by role for better organization
            var groupedUsers = authUsers.GroupBy(u => u.RoleName);

            foreach (var roleGroup in groupedUsers)
            {
                // Role Header
                SetCellValue(worksheet, currentRow, 1, "Role :");
                SetCellValue(worksheet, currentRow, 2, roleGroup.Key);
                SetCellValue(worksheet, currentRow, 6, "Review");
                MergeCells(worksheet, currentRow, 6, currentRow, 7);
                SetRoleHeaderStyle(worksheet, currentRow, 1, currentRow, 7);
                SetFontArial(worksheet, currentRow, 1, currentRow, 7);

                // Set font size 
                worksheet.Cells[currentRow, 1].Style.Font.Size = 11;
                worksheet.Cells[currentRow, 2].Style.Font.Size = 11;
                worksheet.Cells[currentRow, 2].Style.Font.Bold = false;
                worksheet.Cells[currentRow, 6].Style.Font.Size = 9;
                worksheet.Cells[currentRow, 6].Style.Font.Bold = false;

                // Set font size for others in this row (not bold)
                // for (int col = 2; col <= 7; col++)
                // {
                //     worksheet.Cells[currentRow, col].Style.Font.Size = 9;
                //     worksheet.Cells[currentRow, col].Style.Font.Bold = false;
                // }

                var reviewHeaderRange = worksheet.Cells[currentRow, 6, currentRow, 7];
                reviewHeaderRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                reviewHeaderRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                reviewHeaderRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                reviewHeaderRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                reviewHeaderRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                reviewHeaderRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                reviewHeaderRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                reviewHeaderRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                currentRow++;

                // column Header
                SetCellValue(worksheet, currentRow, 1, "User ID");
                SetCellValue(worksheet, currentRow, 2, "First Name");
                SetCellValue(worksheet, currentRow, 3, "Last Name");
                SetCellValue(worksheet, currentRow, 4, "Department");
                SetCellValue(worksheet, currentRow, 5, "Site/Facility");
                SetCellValue(worksheet, currentRow, 6, "Active");
                SetCellValue(worksheet, currentRow, 7, "Inactive");
                SetColumnHeaderStyle(worksheet, currentRow, 1, currentRow, 7);
                SetFontArial(worksheet, currentRow, 1, currentRow, 7);

                // Set font size 9, not bold
                for (int col = 1; col <= 7; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Font.Size = 9;
                    worksheet.Cells[currentRow, col].Style.Font.Bold = false;
                }

                currentRow++;

                // User rows
                foreach (var user in roleGroup)
                {
                    SetCellValue(worksheet, currentRow, 1, user.UserId);
                    SetCellValue(worksheet, currentRow, 2, user.FirstName);
                    SetCellValue(worksheet, currentRow, 3, user.LastName);
                    SetCellValue(worksheet, currentRow, 4, user.Department);
                    SetCellValue(worksheet, currentRow, 5, user.SiteFacility);
                    SetDataRowStyle(worksheet, currentRow, 1, currentRow, 7);
                    SetFontArial(worksheet, currentRow, 1, currentRow, 7);
                    currentRow++;
                }

                currentRow++;
            }

            // Auto-fit column
            // worksheet.Cells.AutoFitColumns();
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // worksheet.Column(1).Width = 16;
            // worksheet.Column(2).Width = 11;
            // worksheet.Column(3).Width = 13;
            // worksheet.Column(4).Width = 10;
            // worksheet.Column(5).Width = 12.5;
            // worksheet.Column(6).Width = 7;
            // worksheet.Column(7).Width = 6.6;

            worksheet.Column(1).Width = 22;
            worksheet.Column(2).Width = 15;
            worksheet.Column(3).Width = 17.78;
            worksheet.Column(4).Width = 13.56;
            worksheet.Column(5).Width = 17.11;
            worksheet.Column(6).Width = 9.11;
            worksheet.Column(7).Width = 9.11;

            // set minimum column widths
            // for (int col = 1; col <= 7; col++)
            // {
            //     if (worksheet.Column(col).Width < 12)
            //         worksheet.Column(col).Width = 12;
            // }

            worksheet.PrinterSettings.FitToPage = true;
            worksheet.PrinterSettings.FitToWidth = 1;
            worksheet.PrinterSettings.FitToHeight = 0;
            // worksheet.PrinterSettings.TopMargin = 2.5;
            // worksheet.PrinterSettings.BottomMargin = 2.5;
            // worksheet.PrinterSettings.LeftMargin = 2.2;
            // worksheet.PrinterSettings.RightMargin = .2;

            return package.GetAsByteArray();
        }

        public async Task<FormOptionsDto> GetFormOptionsAsync()
        {
            var applications = await _context.Applications
                .Where(a => a.ACTIVE == true)
                .Select(a => a.TITLE)
                .Distinct()
                .OrderBy(title => title)
                .ToListAsync();

            var roles = await _context.AppRoles
                .Where(r => r.ACTIVE == true)
                .Select(r => r.NAME)
                .Distinct()
                .OrderBy(name => name)
                .ToListAsync();

            return new FormOptionsDto
            {
                Applications = applications ?? new List<string>(),
                Roles = roles ?? new List<string>()
            };
        }

        private static void SetCellValue(ExcelWorksheet worksheet, int row, int col, string value)
        {
            worksheet.Cells[row, col].Value = value;
        }

        private static void MergeCells(ExcelWorksheet worksheet, int fromRow, int fromCol, int toRow, int toCol)
        {
            worksheet.Cells[fromRow, fromCol, toRow, toCol].Merge = true;
        }

        private static void SetMainHeaderStyle(ExcelWorksheet worksheet, int fromRow, int fromCol, int toRow, int toCol)
        {
            var range = worksheet.Cells[fromRow, fromCol, toRow, toCol];
            range.Style.Font.Bold = false;
            range.Style.Font.Size = 22;
            // range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            // range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
            // range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        }

        // private static void SetSubHeaderStyle(ExcelWorksheet worksheet, int fromRow, int fromCol, int toRow, int toCol)
        // {
        //     var range = worksheet.Cells[fromRow, fromCol, toRow, toCol];
        //     range.Style.Font.Name = "Arial";
        //     range.Style.Font.Size = 14;
        //     range.Style.Font.Bold = true;
        //     range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        //     range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        // }

        // private static void SetHeaderStyle(ExcelWorksheet worksheet, int fromRow, int fromCol, int toRow, int toCol)
        // {
        //     var range = worksheet.Cells[fromRow, fromCol, toRow, toCol];
        //     range.Style.Font.Size = 11;
        //     range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        //     range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        // }

        private static void SetInstructionStyle(ExcelWorksheet worksheet, int fromRow, int fromCol, int toRow, int toCol)
        {
            var range = worksheet.Cells[fromRow, fromCol, toRow, toCol];
            range.Style.Font.Name = "Arial";
            range.Style.Font.Size = 9;
            range.Style.Font.Color.SetColor(Color.Gray);
            // range.Style.Fill.PatternType = ExcelFillStyle.None;
            // range.Style.Fill.BackgroundColor.SetColor(Color.Gray);
            // range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            range.Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
        }

        private static void SetRoleHeaderStyle(ExcelWorksheet worksheet, int fromRow, int fromCol, int toRow, int toCol)
        {
            var range = worksheet.Cells[fromRow, fromCol, toRow, toCol];
            // range.Style.Font.Bold = true;
            range.Style.Font.Size = 11;
            // range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            // range.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
            // range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            worksheet.Cells[fromRow, 6].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[fromRow, 6].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[fromRow, 6].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[fromRow, 6].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[fromRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        private static void SetColumnHeaderStyle(ExcelWorksheet worksheet, int fromRow, int fromCol, int toRow, int toCol)
        {
            var range = worksheet.Cells[fromRow, fromCol, toRow, toCol];
            // range.Style.Font.Bold = true;
            range.Style.Font.Size = 10;
            range.Style.Font.Color.SetColor(Color.Black);
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            // range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            // range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            for (int col = fromCol; col <= toCol; col++)
            {
                var cell = worksheet.Cells[fromRow, col];
                cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }
        }

        private static void SetDataRowStyle(ExcelWorksheet worksheet, int fromRow, int fromCol, int toRow, int toCol)
        {
            for (int col = fromCol; col <= toCol; col++)
            {
                var cell = worksheet.Cells[fromRow, col];
                cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cell.Style.Font.Size = 9;
                cell.Style.Font.Bold = false;
            }
        }

        private static void SetFontArial(ExcelWorksheet worksheet, int fromRow, int fromCol, int toRow, int toCol)
        {
            var range = worksheet.Cells[fromRow, fromCol, toRow, toCol];
            range.Style.Font.Name = "Arial";
        }

        private static string FormatSiteFacility(string? siteCode, string? factCode)
        {
            if (string.IsNullOrEmpty(siteCode) && string.IsNullOrEmpty(factCode))
                return "";

            var result = siteCode ?? "";
            if (!string.IsNullOrEmpty(factCode))
            {
                result += $" [{factCode}]";
            }
            return result;
        }
    }
}