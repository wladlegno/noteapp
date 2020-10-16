using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace noteapp
{
    /// <summary>
    /// Interaction logic for SaveNotePromptWindow.xaml
    /// </summary>
    public partial class SaveNotePromptWindow : Window
    {
        public SaveNotePromptWindow()
        {
            InitializeComponent();
            // Owner = App.Current.MainWindow;
        }
    }
}
