using ContactsManager.Core.Domain.RepositoryContracts;

using Microsoft.Extensions.Logging;

using OfficeOpenXml;

using Serilog;

using ServiceContracts.DTO;

namespace Services
{
    public class PersonsGetterServiceChild : PersonsGetterService
    {
        public PersonsGetterServiceChild(
            IPersonsRepository personsRepository,
            ILogger<PersonsGetterService> logger,
            IDiagnosticContext diagnosticContext) : base(personsRepository, logger, diagnosticContext)
        {
        }

        public override async Task<MemoryStream> GetPersonsExcel()
        {
            {
                MemoryStream memoryStream = new MemoryStream();
                using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
                {
                    ExcelWorksheet workSheet = excelPackage.Workbook.Worksheets.Add("PersonsSheet");
                    workSheet.Cells["A1"].Value = nameof(PersonResponse.PersonName);
                    workSheet.Cells["B1"].Value = nameof(PersonResponse.Age);
                    workSheet.Cells["C1"].Value = nameof(PersonResponse.Gender);

                    using (ExcelRange headerCells = workSheet.Cells["A1:C1"])
                    {
                        headerCells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        headerCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                        headerCells.Style.Font.Bold = true;
                    }

                    int row = 2;
                    List<PersonResponse> persons = await GetAllPersons();

                    if (persons.Count == 0)
                    {
                        throw new InvalidOperationException("No persons data");
                    }

                    foreach (PersonResponse person in persons)
                    {
                        workSheet.Cells[row, 1].Value = person.PersonName;
                        workSheet.Cells[row, 2].Value = person.Age;
                        workSheet.Cells[row, 3].Value = person.Gender;

                        row++;
                    }

                    workSheet.Cells[$"A1:C{row}"].AutoFitColumns();

                    await excelPackage.SaveAsync();

                    memoryStream.Position = 0;
                    return memoryStream;
                }
            }
        }
    }
}