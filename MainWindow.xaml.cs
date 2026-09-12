using System.IO;
using System.Windows;


namespace TypstWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Controls.TreeView file_explorer = new System.Windows.Controls.TreeView();
        System.Windows.Controls.RichTextBox text_block = new System.Windows.Controls.RichTextBox();
        public MainWindow()
        { 
            InitializeComponent();

            file_explorer = (System.Windows.Controls.TreeView)FindName("FileExplorerView");
            text_block = (System.Windows.Controls.RichTextBox)FindName("DocumentBlock");
        }

        private void OpenFolder(object sender, RoutedEventArgs e)
        {
            

            var folder_dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Select Directory",
                DefaultDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),
                Multiselect = false
            };

            if (folder_dialog.ShowDialog() == true) {
                this.file_explorer.Items.Clear();  // clear all exists items

                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(folder_dialog.FolderName);
                // System.Windows.MessageBox.Show(dir_name, Constants.Infos.DEBUG);
                this.set_dir_treeview(dir);
            }
        }

        private void set_dir_treeview(System.IO.DirectoryInfo dir)
        {
            
            try
            {
                
                foreach (System.IO.FileInfo file in dir.GetFiles())
                {
                    System.Windows.Controls.TreeViewItem item = new System.Windows.Controls.TreeViewItem();
                    item.Header = file.Name;
                    item.Tag = file.FullName;
                    item.MouseDoubleClick += TreeViewItemClick;
                    this.file_explorer.Items.Add(item);
                }
                foreach (System.IO.DirectoryInfo d in dir.GetDirectories())
                {
                    this.file_explorer.Items.Add(this.create_directory_entry_item(d));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            } 
        }

        private System.Windows.Controls.TreeViewItem create_directory_entry_item(System.IO.DirectoryInfo directory)
        {
            /**
             * create sub treeview item within specific directory
             */
            System.Windows.Controls.TreeViewItem root_item = new System.Windows.Controls.TreeViewItem
            {
                Header = directory.Name,
                Tag = directory.FullName
            };

            try
            {
                foreach (System.IO.DirectoryInfo sub_dir in directory.GetDirectories())
                {
                    System.Windows.Controls.TreeViewItem sub_item = create_directory_entry_item(sub_dir);
                    root_item.Items.Add(sub_item);
                }

                foreach(System.IO.FileInfo fp in directory.GetFiles())
                {
                    System.Windows.Controls.TreeViewItem file_item = new System.Windows.Controls.TreeViewItem
                    {
                        Header = fp.Name,
                        Tag = fp.FullName
                    };
                    file_item.MouseDoubleClick += TreeViewItemClick;
                    root_item.Items.Add(file_item);
                }
            }
            catch (UnauthorizedAccessException ex)
            {

            }
            catch (IOException ex)
            {

            }

            return root_item;
        }

        private void TreeViewItemClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Controls.TreeViewItem curr_control = (System.Windows.Controls.TreeViewItem)sender;
                string file_name = curr_control.Tag.ToString();
                System.IO.FileAttributes file_attr = System.IO.File.GetAttributes(file_name);
                if (!file_attr.HasFlag(System.IO.FileAttributes.Directory))
                {
                    this.text_block.Document.Blocks.Clear();

                    string content = System.IO.File.ReadAllText(file_name, System.Text.Encoding.UTF8);

                    this.text_block.AppendText(content);
                    this.text_block.AppendText("\r\n");
                }
                
            }
            catch (Exception ex)
            {
                
            }
            
        }

        private void ExportFile(object sender, RoutedEventArgs e)
        {
            var file_dialog = new Microsoft.Win32.SaveFileDialog();
            file_dialog.Filter = Constants.Export.EXPORT_FILTER;
            file_dialog.DefaultExt = Constants.Export.EXPORT_EXT;
            file_dialog.DefaultDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            file_dialog.FileName = Constants.Export.EXPORT_FILE;

            if (file_dialog.ShowDialog() == true)
            {
                if (this.file_explorer.Items.Count > 0)
                {
                    System.Windows.Controls.TreeViewItem item = (System.Windows.Controls.TreeViewItem)this.file_explorer.Items.GetItemAt(0);
                    string project_folder = System.IO.Path.GetDirectoryName(item.Tag.ToString()) ?? System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
                    string main_content = System.IO.File.ReadAllText($"{project_folder}/{Constants.Infos.MAIN_FILE}");

                    var compiler_option = new Typst.NET.TypstCompilerOptions
                    {
                        WorkspaceRoot = project_folder
                    };
                    var compiler = new Typst.NET.TypstCompiler(compiler_option);
                    var result = compiler.Compile(main_content);

                    if (result.Success)
                    {
                        var pdf = result.Document.RenderToPdf();
                        System.IO.File.WriteAllBytes(file_dialog.FileName, pdf);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Something Error happened", Constants.Infos.WARNING);
                    }
                }
            }
        }
    }
}