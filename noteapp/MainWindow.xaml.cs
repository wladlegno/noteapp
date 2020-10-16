using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace noteapp
{
    public class Note
    {
        public string Path { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }

    public partial class MainWindow
    {
        private DataTable notes;
        private Note currentNote = new Note {Body = "", Title = ""};
        private readonly string notesPath = AppDomain.CurrentDomain.BaseDirectory + @"notes\";
        private DispatcherTimer timer1;

        public MainWindow()
        {
            ConsoleCall.ShowConsoleWindow();

            InitializeComponent();

            InitializeDirectory();

            InitializeDataTable();

            TitleRTBox.Focus();

            SaveNotePromptWindow dialog = new SaveNotePromptWindow();
            // dialog.ShowDialog();
        }

        private void somefun(object sender, EventArgs eventArgs)
        {
            Console.WriteLine(ChangesPendingState());
            Console.WriteLine(currentNote.Title);
        }

        #region event handlers

        private void OnChanges(object sender, KeyEventArgs e)
        {
            ChangesPendingState();
        }

        private void SaveNoteButton_OnClick(object sender, RoutedEventArgs e)
        {
            SaveNote();
        }

        private void NewNoteButton_OnClick(object sender, RoutedEventArgs e)
        {
            NewNote();
        }

        private void Row_OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            DataRowView dataRowView = (DataRowView) NotesDGrid.SelectedItem;
            string rowTitle = dataRowView["Title"].ToString();
            LoadNote(rowTitle);
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
        
        private void DeleteNoteButton_OnMouseDown(object sender, RoutedEventArgs routedEventArgs)
        {
            DeleteNote(currentNote.Path);
        }

        #endregion

        #region methods

        #region initializers

        private void InitializeDirectory()
        {
            Directory.CreateDirectory(notesPath);
        }

        private void InitializeDataTable()
        {
            UpdateDataTable();

            NotesDGrid.CanUserResizeRows = false;
            NotesDGrid.ColumnWidth = new DataGridLength(210);
            NotesDGrid.BorderThickness = new Thickness(0);
            NotesDGrid.Background = new SolidColorBrush(Colors.White);
        }

        private void UpdateDataTable()
        {
            notes = new DataTable();
            DataRow row;
            notes.Columns.Add("Title", typeof(string));
            GetNotesList().ToList().ForEach(x =>
            {
                row = notes.NewRow();
                row[0] = x;
                notes.Rows.Add(row);
            });
            NotesDGrid.DataContext = notes.DefaultView;
        }

        #endregion

        #region file manipulation

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
            string dataToWrite = GetDataFromBox(BodyRTBox);

            try
            {
                File.WriteAllText(filePath, dataToWrite);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region note manipulation

        #region new note

        private void NewNote()
        {
            if (GetDataFromBox(TitleRTBox) == "" || GetDataFromBox(BodyRTBox) == "") return;
            SaveNote();
            ClearContents();
        }

        #endregion

        #region save note

        private void SaveNote()
        {
            if (!ChangesPendingState())
            {
                return;
            }

            ;
            string title = GetDataFromBox(TitleRTBox);
            if (title == "")
                SaveNotePrompt();
            SaveNote(title);
        }

        private void SaveNote(string title)
        {
            Console.WriteLine("Save() called");
            string fullPath = notesPath + $"{title}.txt";
            CreateFile(fullPath);
            UpdateDataTable();
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

        #endregion

        #region load note

        private void LoadNote(string noteTitle)
        {
            if (ChangesPendingState())
            {
                SaveNote();
            }
            else
            {
                ClearContents();
                string path = GetPathFromName(noteTitle);
                currentNote = new Note {Path = path, Title = noteTitle, Body = File.ReadAllText(path)};
                TitleRTBox.Document.Blocks.Add(new Paragraph(new Run(currentNote.Title)));
                BodyRTBox.Document.Blocks.Add(new Paragraph(new Run(currentNote.Body)));
            }
        }

        #endregion

        #region delete note

        private void DeleteNote(string path)
        {
            File.Delete(path);
            currentNote = new Note();
            UpdateDataTable();
        }
        
        #endregion

        #region change note

        private void RenamePrompt(out string filePath)
        {
            Console.WriteLine("RenamePrompt() called");
            filePath = "";
        }

        #endregion

        #endregion

        #endregion

        #region helper methods

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

        private static string GetDataFromBox(RichTextBox rtb)
        {
            TextRange textRange = new TextRange(
                rtb.Document.ContentStart,
                rtb.Document.ContentEnd
            );
            return textRange.Text.Trim();
        }

        private void ClearContents()
        {
            BodyRTBox.Document.Blocks.Clear();
            TitleRTBox.Document.Blocks.Clear();
        }

        private bool ChangesPendingState()
        {
            string
                title = GetDataFromBox(TitleRTBox),
                body = GetDataFromBox(BodyRTBox);
            string[]
                inputData = {title, body},
                noteData = {currentNote.Title, currentNote.Body};
            bool changesPending = !inputData.SequenceEqual(noteData);
            return changesPending;
        }

        private List<string> GetNotesList()
        {
            /*IEnumerable<FileInfo> noteList = new DirectoryInfo(notesPath).EnumerateFiles();
            List<string> nl = new List<string>();
            noteList.ToList().ForEach(x => { nl.Add(GetNameFromPath(x)); });*/
            DirectoryInfo dir = new DirectoryInfo(notesPath);
            List<string> files = new List<string>();
            dir.GetFiles().OrderBy(p => p.LastWriteTime).ToList().ForEach(x => { files.Add(GetNameFromPath(x)); });
            return files;
        }

        #endregion
    }
}