using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace XMLParser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MyItem root = new MyItem { Title = "Menu" };
            root.Attributes.Add("A", "B");
            root.Attributes.Add("C", "D");
            MyItem child1 = new MyItem() { Title = "Child item #1" };
            child1.Items.Add(new MyItem() { Title = "Child item #1.1" });
            child1.Items.Add(new MyItem() { Title = "Child item #1.2" });
            root.Items.Add(child1);
            root.Items.Add(new MyItem() { Title = "Child item #2" });
            elementsTree.Items.Add(root);
        }

        private async Task LerArquivo(string nome)
        {
            rootNode = null;
            actualNode = null;
            elementsTree.Items.Clear();
            using (var fileStream = System.IO.File.OpenText(nome)) {
                XmlReaderSettings settings = new XmlReaderSettings { Async = true };
                using (XmlReader reader = XmlReader.Create(fileStream, settings)) {
                    while (await reader.ReadAsync()) {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                //Create a new element to put into the tree
                                actualNode = new MyItem(actualNode) { Title = reader.Name };
                                while (reader.MoveToNextAttribute())
                                {
                                    actualNode.Attributes.Add(reader.Name, reader.Value);
                                }
                                if (rootNode == null)
                                {
                                    rootNode = actualNode;
                                    elementsTree.Items.Add(rootNode);
                                }
                                break;
                            case XmlNodeType.EndElement:
                                // Element filled, time to insert into the tree
                                if (actualNode.Parent != null)
                                {
                                    actualNode.Parent.Items.Add(actualNode);
                                    actualNode = actualNode.Parent;
                                }
                                break;
                            case XmlNodeType.Text:
                                // Text value
                                actualNode.Text = reader.Value;
                                break;
                        }
                    }
                }
            }
        }

        private MyItem actualNode;
        private MyItem rootNode;

        private async void OpenFile(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (!mainStack.IsVisible)
            {
                mainStack.Visibility = Visibility.Visible;
            }
            if (openFileDialog.ShowDialog() == true)
            {
                txbFileName.Text = openFileDialog.FileName;
                await LerArquivo(openFileDialog.FileName);
            }
        }
    }

    public class MyItem
    {
        public MyItem(MyItem parent = null)
        {
            this.Items = new ObservableCollection<MyItem>();
            this.Attributes = new Dictionary<string, string>();
            this.Parent = parent;
        }
        public string Title { get; set; }
        public string Text { get; set; }
        public ObservableCollection<MyItem> Items { get; set; }
        public Dictionary<string,string> Attributes { get; set; }
        public MyItem Parent { get; }
    }
}
