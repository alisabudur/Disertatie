using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using SoftwareQualityPrediction.Models;

namespace SoftwareQualityPrediction.Services
{
    public class ExcelService
    {
        public ExcelService(string filePath, 
            string sheet, string idColumn, 
            List<string> inputVariables, 
            List<string> outputVariables)
        {
            _filePath = filePath;
            _sheet = sheet;
            _idColumn = idColumn;
            _inputVariables = inputVariables;
            _outputVariables = outputVariables;
        }

        public IEnumerable<TrainingRow> GetAllRows()
        {
            var rows = new List<TrainingRow>();
            var connectionString = $"Provider=Microsoft.Jet.OLEDB.4.0; data source={_filePath}; Extended Properties=Excel 8.0;";

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                OleDbCommand command = new OleDbCommand(GetSelectStatement(), connection);
                connection.Open();
                OleDbDataReader reader = command.ExecuteReader();

                while (reader != null && reader.Read())
                {
                    var row = new TrainingRow();

                    var idColumn = reader[_idColumn];

                    if (idColumn != DBNull.Value)
                    {
                        row.IdColumn = idColumn.ToString();
                        row.Input = GetArrayOfVariables(reader, _inputVariables);
                        row.Output = GetArrayOfVariables(reader, _outputVariables);
                        rows.Add(row);
                    }
                }

                reader?.Close();
            }

            return rows;
        }

        private string GetSelectStatement()
        {
            var stringBuilder = new StringBuilder();
            var inputVariables = _inputVariables.Select(x => $"[{x}]").ToList();
            var outputVariables = _outputVariables.Select(x => $"[{x}]").ToList();
            stringBuilder.AppendLine(
                $"SELECT [{_idColumn}], {string.Join(", ", inputVariables)}, {string.Join(", ", outputVariables)}");
            stringBuilder.AppendLine($"FROM [{_sheet}]");

            return stringBuilder.ToString();
        }

        private double[] GetArrayOfVariables(OleDbDataReader reader, List<string> variables)
        {
            var list = new List<double>();
            foreach (var variable in variables)
            {
                var value = reader[variable];

                if (value != DBNull.Value)
                    list.Add(Convert.ToDouble(value));
                else
                    list.Add(0);
            }

            return list.ToArray();
        }

        private string _filePath;
        private string _sheet;
        private string _idColumn;
        private List<string> _inputVariables;
        private List<string> _outputVariables;
    }
}
