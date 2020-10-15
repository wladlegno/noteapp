using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace noteapp
{
    public class Note
    {
        public string Title { get; set; }
        public string Body { get; set; }
    }

    public partial class MainWindow
    {
        private DataTable notes;
        private readonly string notesPath = AppDomain.CurrentDomain.BaseDirectory + @"notes\";
        private bool changesPending;

        public MainWindow()
        {
            ConsoleCall.ShowConsoleWindow();

            InitializeComponent();

            InitializeDirectory();

            InitializeDataTable();

            TitleRTBox.Focus();
        }

        private static string GenerateRandomName()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static string GetNameFromPath(FileInfo path)
        {
            return path.Name.Substring(0, path.Name.Length - path.Extension.Length);
        }

        private string GetPathFromName(string name)
        {
            return $"{notesPath + name}.txt";
        }

        private void InitializeDirectory()
        {
            Directory.CreateDirectory(notesPath);
        }

        private void InitializeDataTable()
        {
            notes = new DataTable();
            notes.Columns.Add("Title", typeof(String));
            DataRow row;
            GetNotesList().ToList().ForEach(x =>
            {
                row = notes.NewRow();
                row[0] = x;
                Console.WriteLine(row.ItemArray[0]);
                notes.Rows.Add(row);
            });

            NotesDGrid.DataContext = notes.DefaultView;
            NotesDGrid.CanUserResizeRows = false;

            NotesDGrid.ColumnWidth = new DataGridLength(210);
            NotesDGrid.BorderThickness = new Thickness(0);
            NotesDGrid.Background = new SolidColorBrush(Colors.White);
        }

        private void LoadNote(string noteTitle)
        {
            changesPending = false;
            string path = GetPathFromName(noteTitle);
            string
                fileTitle = noteTitle,
                fileBody = File.ReadAllText(path);
            ClearContents();
            TitleRTBox.Document.Blocks.Add(new Paragraph(new Run(fileTitle)));
            TextRTBox.Document.Blocks.Add(new Paragraph(new Run(fileBody)));
        }

        private void MainGrid_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
            }
        }

        private void NewNoteButton_OnClick(object sender, RoutedEventArgs e)
        {
            NewNote();
        }

        private static string GetDataFromBox(RichTextBox rtb)
        {
            TextRange textRange = new TextRange(
                rtb.Document.ContentStart,
                rtb.Document.ContentEnd
            );
            return textRange.Text.Trim();
        }

        private void CreateFile(string filePath)
        {
            Console.WriteLine("CreateFile() called");
            
            if (File.Exists(filePath))
            {
                WriteFile(filePath);
                return;
            }

            try
            {
                File.Create(filePath).Close();
                CreateFile(filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void WriteFile(string filePath)
        {
            Console.WriteLine("WriteFile() called");
            var dataToWrite = GetDataFromBox(TextRTBox);

            try
            {
                File.WriteAllText(filePath, dataToWrite);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SaveNoteButton_OnClick(object sender, RoutedEventArgs e)
        {
            SaveNote();
        }

        private void SaveNote()
        {
            if (!changesPending) return;
            string title = GetDataFromBox(TitleRTBox);
            if (title == "")
            {
                SaveNotePrompt();
            }
            else
            {
                SaveNote(title);
            }
        }

        private void SaveNote(string title)
        {
            Console.WriteLine("Save() called");
            string fullPath = notesPath + $"{title}.txt";
            CreateFile(fullPath);
        }

        private void SaveNotePrompt()
        {
            // TODO: when: clicking on button save, new, delete, picking another note; if: text and title not empty
            // TODO: to ask under what name to save a file
            // TODO: suggest the name typed into TitleRTBox by default
            Console.WriteLine("SavePrompt() called");

            string title = $"{GenerateRandomName()}";
            SaveNote(title);
        }

        private void NewNote()
        {
            if (changesPending)
            {
                SaveNote();
                return;
            }

            if (GetDataFromBox(TextRTBox) == "") return;
            ClearContents();
        }

        private void RenamePrompt(out string filePath)
        {
            Console.WriteLine("RenamePrompt() called");
        }

        private void ClearContents()
        {
            TextRTBox.Document.Blocks.Clear();
            TitleRTBox.Document.Blocks.Clear();
        }

        private IEnumerable<string> GetNotesList()
        {
            IEnumerable<FileInfo> noteList = new DirectoryInfo(notesPath).EnumerateFiles();
            List<string> nl = new List<string>();
            noteList.ToList().ForEach(x => { nl.Add(GetNameFromPath(x)); });
            return nl;
        }

        private void Row_OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            DataRowView dataRowView = (DataRowView) NotesDGrid.SelectedItem;
            string rowTitle = dataRowView["Title"].ToString();
            LoadNote(rowTitle);
        }
    }
}