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
            _connectionString = $"Provider=Microsoft.Jet.OLEDB.4.0; data source={_filePath}; Extended Properties=Excel 8.0;";
        }

        public IEnumerable<TrainingRow> GetAllRows()
        {
            var rows = new List<TrainingRow>();

            using (OleDbConnection connection = new OleDbConnection(_connectionString))
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

        public void UpdateRow(TrainingRow row)
        {
            var updateStatement = GetUpdateStatement(row);

            using (OleDbConnection connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                OleDbCommand command = new
                    OleDbCommand(updateStatement, connection);
                command.ExecuteNonQuery();
            }
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

        private string GetUpdateStatement(TrainingRow entity)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"UPDATE [{_sheet}]");
            stringBuilder.AppendLine($"SET {GetSetStatement(entity)}");
            stringBuilder.AppendLine($"WHERE {_idColumn} = '{entity.IdColumn}'");
            return stringBuilder.ToString();
        }

        private string GetSetStatement(TrainingRow entity)
        {
            var setList = new List<string>();

            if(_outputVariables.Count != entity.Output.Length)
                throw new ArgumentException("Number of output columns different than the number of output variables. ");

            for (var i = 0; i < _outputVariables.Count; i++)
            {
                var variable = _outputVariables[i];
                var value = entity.Output[i];

                setList.Add($"[{variable}] = {value}");
            }

            return string.Join($",{Environment.NewLine}", setList);
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
        private string _connectionString;
        private List<string> _inputVariables;
        private List<string> _outputVariables;
    }
}
