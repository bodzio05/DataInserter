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

        private Status _status;
        public Status Status
        {
            get => _status;
            set
            {
                _status = value;
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
            this.Status = new Status(StatusEnum.Waiting);
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
            this.Status.CurrentStatus = StatusEnum.InProgress;

            this.excelReaderResult = materials;

            if (CanBeExecuted())
            {
                RunEditor();
            }
            else
            {
                System.Windows.MessageBox.Show("Can't run program. Check the settings and try again.");
            }

            this.Status.CurrentStatus = StatusEnum.Ended;
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
            foreach (var xml in XMLFiles)
            {
                XDocument document = XDocument.Load(xml);
                var standardNodes = document.Root.Elements();
                var materials = document.Root.Elements().Where(e => e.Name == "Material").ToList();

                foreach (var data in excelReaderResult)
                {                  
                    switch (data.RootCondition.NodeLevel)
                    {
                        case NodeLevel.Standard:
                            var standardName = document.Root.Element("StandardName").Value;
                            data.StdVersion = standardNodes.FirstOrDefault(n=>n.Name == "Version").Value;
                            var node = standardNodes.Where(x => x.Name.ToString() == data.RootCondition.XmlPropertyName.ToString()).ToList();

                            if (node.Count == 1 && data.StandardName == standardName)
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

                            foreach (var material in materials)
                            {
                                if (material.Element("Name").Value == data.MaterialName)
                                {
                                    var materialNode = material.Elements().Where(x => x.Name.ToString() == data.RootCondition.XmlPropertyName.ToString()).ToList();
                                    data.MtrVersion = material.Elements().FirstOrDefault(n => n.Name == "Version").Value;
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
                            
                            var templateNode = document.Root.Element("Template");
                            var allTemplateParameters =  templateNode.Elements().Where(x => x.Name.ToString() == "Parameter").ToList();
                            int biggestParameterNumber = 10000;

                            foreach (var parameter in allTemplateParameters)
                            {
                                int parameterNumber = Int32.Parse(parameter.Elements().FirstOrDefault(p => p.Name.ToString() == "Number").Value);
                                if (parameterNumber > biggestParameterNumber)
                                    biggestParameterNumber = parameterNumber + 1;
                            }

                            if (templateNode != null)
                            {
                                var parameterInTemplate = allTemplateParameters.FirstOrDefault(p => p.Element("Name").Value.ToString() == data.RootCondition.ExcelPropertyName);

                                if (parameterInTemplate != null)
                                {
                                    //parameterInTemplate.Elements().FirstOrDefault(e => e.Name.ToString() == "Value").Value = data.PropertyValue;
                                    parameterInTemplate.Elements().FirstOrDefault(e => e.Name.ToString() == "Description").Value = data.RootCondition.Parameter.Description;
                                    parameterInTemplate.Elements().FirstOrDefault(e => e.Name.ToString() == "Context").Value = data.RootCondition.Parameter.Context.ToString();

                                    if (parameterInTemplate.Elements().Any(p => p.Name.ToString() == "ValueType"))
                                    {
                                        parameterInTemplate.Elements().FirstOrDefault(e => e.Name.ToString() == "ValueType").Value = data.RootCondition.Parameter.ValueType;
                                    }
                                }                              
                                else if (CreateNodeIfNotExists)
                                {
                                    XElement newParameter = new XElement("Parameter");

                                    string name = data.RootCondition.Parameter.Name;
                                    string number = biggestParameterNumber.ToString();
                                    string context = data.RootCondition.Parameter.Context.ToString();
                                    string description = data.RootCondition.Parameter.Description;
                                    string valueType = data.RootCondition.Parameter.ValueType;
                                    //string value = data.PropertyValue;
                                    string unit = data.RootCondition.Parameter.Unit;
                                    string valueOptions = data.RootCondition.Parameter.ValueOptions;

                                    XElement newParameterName = new XElement("Name", name);
                                    XElement newParameterNumber = new XElement("Number", number);
                                    XElement newParameterContext = new XElement("Context", context);
                                    XElement newParameterDescription = new XElement("Description", description);
                                    XElement newParameterValueType = new XElement("ValueType", valueType);
                                    XElement newParameterValue = new XElement("Value", "");
                                    XElement newParameterUnit = new XElement("Unit", unit);
                                    XElement newParameterValueOptions = new XElement("ValueOptions", valueOptions);

                                    newParameter.Add(
                                        newParameterName,
                                        newParameterNumber,
                                        newParameterContext,
                                        newParameterDescription,
                                        newParameterValueType,
                                        newParameterValue,
                                        newParameterUnit,
                                        newParameterValueOptions
                                        );

                                    templateNode.Elements().LastOrDefault(n => n.Name == "Parameter").AddAfterSelf(newParameter);
                                }
                            }

                            foreach (var material in materials)
                            {
                                if (material.Element("Name").Value == data.MaterialName)
                                {
                                    var allParameters = material.Elements().Where(x => x.Name.ToString() == "Parameter").ToList();
                                    var existingParameter = allParameters.FirstOrDefault(p => p.Element("Name").Value.ToString() == data.RootCondition.ExcelPropertyName);

                                    if (existingParameter != null)
                                    {
                                        existingParameter.Elements().FirstOrDefault(e => e.Name.ToString() == "Value").Value = data.PropertyValue;
                                        existingParameter.Elements().FirstOrDefault(e => e.Name.ToString() == "Description").Value = data.RootCondition.Parameter.Description;
                                        existingParameter.Elements().FirstOrDefault(e => e.Name.ToString() == "Context").Value = data.RootCondition.Parameter.Context.ToString();

                                        if (existingParameter.Elements().ToList().Any(p=>p.Name.ToString() == "ValueType"))
                                        {
                                            existingParameter.Elements().FirstOrDefault(e => e.Name.ToString() == "ValueType").Value = data.RootCondition.Parameter.ValueType;
                                        }
                                    }
                                    else if (CreateNodeIfNotExists)
                                    {
                                        XElement newParameter = new XElement("Parameter");

                                        string name = data.RootCondition.Parameter.Name;
                                        string number = biggestParameterNumber.ToString();
                                        string context = data.RootCondition.Parameter.Context.ToString();
                                        string description = data.RootCondition.Parameter.Description;
                                        string valueType = data.RootCondition.Parameter.ValueType;
                                        string value = data.PropertyValue;
                                        string unit = data.RootCondition.Parameter.Unit;
                                        string valueOptions = data.RootCondition.Parameter.ValueOptions;

                                        XElement newParameterName = new XElement("Name", name);
                                        XElement newParameterNumber = new XElement("Number", number);
                                        XElement newParameterContext = new XElement("Context", context);
                                        XElement newParameterDescription = new XElement("Description", description);
                                        XElement newParameterValueType = new XElement("ValueType", valueType);
                                        XElement newParameterValue = new XElement("Value", value);
                                        XElement newParameterUnit = new XElement("Unit", unit);
                                        XElement newParameterValueOptions = new XElement("ValueOptions", valueOptions);

                                        newParameter.Add(
                                            newParameterName,
                                            newParameterNumber,
                                            newParameterContext,
                                            newParameterDescription,
                                            newParameterValueType,
                                            newParameterValue,
                                            newParameterUnit,
                                            newParameterValueOptions
                                            );

                                        material.Elements().LastOrDefault(n => n.Name == "Parameter").AddAfterSelf(newParameter);
                                    }

                                    break;
                                }
                            }
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
