using DataInserter.Model;
using DataInserter.ViewModel.Commands;
using DataInserter.ViewModel.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace DataInserter.ViewModel
{
    public class XMLManipulatorViewModel : ViewModelBase, IViewModel
    {
        #region Properties
        private bool? _isRunnable;
        public bool? IsRunnable
        {
            get => _isRunnable;
            set
            {
                _isRunnable = value;
                NotifyPropertyChanged();
            }
        }

        private bool _editXmlFiles;
        public bool EditXmlFiles
        {
            get => _editXmlFiles;
            set
            {
                _editXmlFiles = value;
                NotifyPropertyChanged();
            }
        }

        private bool _createNodeIfNotExists;
        public bool CreateNodeIfNotExists
        {
            get => _createNodeIfNotExists;
            set
            {
                _createNodeIfNotExists = value;
                NotifyPropertyChanged();
            }
        }

        private string _xmlFolderPath;
        public string XmlFolderPath
        {
            get => _xmlFolderPath;
            set
            {
                _xmlFolderPath = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region Fields
        private readonly IMainViewModel mainViewModel;
        private List<MatchedData> excelReaderResult = new List<MatchedData>();
        #endregion

        #region Commands
        public ICommand SearchForExcelFileCommand { get { return new RelayCommand(SearchForXmlFolder, AlwaysTrue); } }
        private bool AlwaysTrue() { return true; }
        private bool AlwaysFalse() { return false; }
        #endregion

        #region Constructors
        public XMLManipulatorViewModel(IMainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
        }
        #endregion

        #region UI Methods
        private void SearchForXmlFolder()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                this.XmlFolderPath = dialog.SelectedPath;
            }
        }

        private bool CanBeExecuted()
        {
            if (Directory.Exists(this.XmlFolderPath) && excelReaderResult.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void RunXmlEditor(List<MatchedData> materials)
        {
            this.excelReaderResult = materials;

            if (CanBeExecuted())
            {
                RunEditor();
            }
            else
            {
                System.Windows.MessageBox.Show("Can't run program. Check the settings and try again.");
            }
        }
        #endregion

        #region Editor Methods
        private void RunEditor()
        {
            GetFiles(this.XmlFolderPath);
            EditFiles();
        }

        private List<string> XMLFiles;

        private void EditFiles()
        {
            foreach (var data in excelReaderResult)
            {
                foreach (var xml in XMLFiles)
                {
                    XDocument document = XDocument.Load(xml);

                    switch (data.RootCondition.NodeLevel)
                    {
                        case NodeLevel.Standard:
                            var standardNodes = document.Root.Elements();
                            var node = standardNodes.Where(x => x.Name.ToString() == data.RootCondition.XmlPropertyName.ToString()).ToList();

                            if (node.Count == 1)
                            {
                                node[0].Value = data.PropertyValue;
                            }
                            else if (node.Count == 0 && CreateNodeIfNotExists)
                            {
                                XElement newNode = new XElement(data.RootCondition.XmlPropertyName.ToString(), data.PropertyValue);
                                standardNodes.LastOrDefault().AddAfterSelf(newNode);
                            }

                            break;

                        case NodeLevel.Material:
                            var materials = document.Root.Elements().Where(e => e.Name == "Material").ToList();

                            foreach (var material in materials)
                            {
                                if (material.Element("Name").Value == data.MaterialName)
                                {
                                    var materialNode = material.Elements().Where(x => x.Name.ToString() == data.RootCondition.XmlPropertyName.ToString()).ToList();

                                    if (materialNode.Count == 1)
                                    {
                                        materialNode[0].Value = data.PropertyValue;
                                    }
                                    else if (materialNode.Count == 0 && CreateNodeIfNotExists)
                                    {
                                        XElement newNode = new XElement(data.RootCondition.XmlPropertyName.ToString(), data.PropertyValue);
                                        material.Elements().LastOrDefault().AddAfterSelf(newNode);
                                    }

                                    break;
                                }
                            }

                            break;

                        case NodeLevel.Parameter:
                            break;
                        default:
                            break;
                    }

                    document.Save(xml);
                }
            }
        }

        private string SetVersion(XElement material)
        {
            return material.Descendants("Version").FirstOrDefault().Value;
        }

        private void GetFiles(string path)
        {
            XMLFiles = Directory.GetFiles(path, "*.xml").ToList();
        }
        #endregion
    }
}
