using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace noteapp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private DataTable notes;
        private readonly string _saveDir = AppDomain.CurrentDomain.BaseDirectory + @"notes\";

        public MainWindow()
        {
            ConsoleCall.ShowConsoleWindow();

            InitializeComponent();

            if (!Directory.Exists(_saveDir))
            {
                Directory.CreateDirectory(_saveDir);
            }

            InitializeDataTable();

            TitleRTBox.Focus();
            Console.WriteLine(_saveDir);
        }

        private string GenerateRandomName()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void InitializeDataTable()
        {
            notes = new DataTable();
            notes.Columns.Add("Title", typeof(String));
            notes.Columns.Add("Messages", typeof(String));
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
            if (GetDataFromBox(TitleRTBox) != "" || GetDataFromBox(TextRTBox) != "")
            {
                SavePrompt();
            }
            else
            {
                Console.WriteLine("No need to call SavePrompt() ;)");
            }

            TextRTBox.Document.Blocks.Clear();
            TitleRTBox.Document.Blocks.Clear();
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
            
            /*if (File.Exists(filePath))
            {
                RenamePrompt(out filePath);
                WriteFile(filePath);
            }

            try
            {
                File.Create(filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }*/
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
            string title = GetDataFromBox(TitleRTBox);
            if (title != "")
            {
                Save(title);
            }
            else
            {
                SavePrompt();
            }
        }

        private void SavePrompt()
        {
            // TODO: when: clicking on button save, new, delete, picking another note; if: text and title not empty
            // TODO: to ask under what name to save a file
            // TODO: suggest the name typed into TitleRTBox by default
            Console.WriteLine("SavePrompt() called");

            string title = GenerateRandomName() + ".txt";
            Console.WriteLine(title);
            Save(title);
        }

        private void Save(string title)
        {
            Console.WriteLine("Save() called");
            string fullPath = _saveDir + $"{title}.txt";
            if (File.Exists(fullPath))
            {
                WriteFile(fullPath);
            }
            else
            {
                CreateFile(fullPath);
            }
        }

        private void RenamePrompt(out string filePath)
        {
            Console.WriteLine("RenamePrompt() called");

            filePath = "smthrandom";
        }
    }
}