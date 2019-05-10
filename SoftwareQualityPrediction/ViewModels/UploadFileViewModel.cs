using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using SoftwareQualityPrediction.Utils;

namespace SoftwareQualityPrediction.ViewModels
{
    public class UploadFileViewModel: BaseViewModel, IColleague
    {
        public IMediator Mediator { get; set; }

        public UploadFileViewModel()
        {
            _canExecute = true;
            _isSheetEnabled = false;
            _sheets = new List<string>();
        }

        public ICommand UploadFileCommand =>
            _uploadFile ?? (_uploadFile = new CommandHandler(UploadFile, _canExecute));

        
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (value != null)
                {
                    _filePath = value;
                    PopulateSheets();
                    IsSheetEnabled = true;
                    OnPropertyChanged(nameof(FilePath));
                }
            }
        }

        public string SelectedSheet
        {
            get { return _selectedSheet; }
            set
            {
                _selectedSheet = value;
                PopulateDataGrid();
                OnPropertyChanged(nameof(SelectedSheet));
            }
        }

        public bool IsSheetEnabled
        {
            get { return _isSheetEnabled; }
            set
            {
                _isSheetEnabled = value;
                OnPropertyChanged(nameof(IsSheetEnabled));
            }
        }

        public List<string> Sheets
        {
            get { return _sheets; }
            set
            {
                _sheets = value;
                OnPropertyChanged(nameof(Sheets));
            }
        }

        public DataView DataRows
        {
            get { return _dataRows; }
            set
            {
                _dataRows = value;
                OnPropertyChanged(nameof(DataRows));
            }
        }

        public List<string> Columns
        {
            get { return _columns; }
            set
            {
                _columns = value;
                OnPropertyChanged(nameof(Columns));
            }
        }

        public void Send(object message)
        {
            Mediator.Send(message, this);
        }

        public void Receive(object message)
        {
            throw new System.NotImplementedException();
        }

        private void UploadFile()
        {
            var op = new OpenFileDialog();
            op.Title = "Select a file";
            op.Filter = "Excel|*.xls";

            if (op.ShowDialog() == true)
            {
                FilePath = op.FileName;
            }
        }

        private void PopulateSheets()
        {
            var connectionString = $"Provider=Microsoft.Jet.OLEDB.4.0; data source={FilePath}; Extended Properties=Excel 8.0;";
            using (var connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                var dataTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                if(dataTable == null)
                    return;

                var sheets = new List<string>();

                foreach (DataRow row in dataTable.Rows)
                {
                    sheets.Add(row["TABLE_NAME"].ToString());
                }

                Sheets = sheets;

                if (Sheets.Any())
                {
                    SelectedSheet = Sheets.FirstOrDefault();
                }
            }
        }

        private void PopulateDataGrid()
        {
            var connectionString = $"Provider=Microsoft.Jet.OLEDB.4.0; data source={FilePath}; Extended Properties=Excel 8.0;";
            var adapter = new OleDbDataAdapter($"SELECT TOP 7 * FROM [{SelectedSheet}]", connectionString);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);
            DataRows = dataTable.DefaultView;

            var columns = new List<string>();
            foreach (DataColumn column in dataTable.Columns)
            {
                columns.Add(column.ColumnName);
            }

            Columns = columns;
            Send(Columns);
        }

        private bool _canExecute;
        private bool _isSheetEnabled;
        private string _filePath;
        private string _selectedSheet;
        private List<string> _sheets;
        private DataView _dataRows;
        private List<string> _columns;
        private ICommand _uploadFile;
        
    }
}
