using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInserter.Model
{
    public class MatchingCondition
    {
        public string ExcelPropertyName { get; set; }
        public NodeLevel NodeLevel { get; set; }
        public XmlNodes XmlPropertyName { get; set; }
        public DatabaseTable DatabaseTableName { get; set; }
        public DatabaseFields DatabaseFieldName { get; set; }


        public MatchingCondition(string excelPropertyName, NodeLevel level, XmlNodes xmlPropertyName)
        {
            this.ExcelPropertyName = excelPropertyName;
            this.NodeLevel = level;
            this.XmlPropertyName = xmlPropertyName;

            SetDatabaseTableName();
            SetDatabaseFieldName();
        }

        private void SetDatabaseTableName()
        {
            switch (NodeLevel)
            {
                case NodeLevel.Standard:
                    DatabaseTableName = DatabaseTable.PD_MTRSTANDARDS;
                    break;
                case NodeLevel.Material:
                    DatabaseTableName = DatabaseTable.PD_MATERIALS;
                    break;
                case NodeLevel.Parameter:
                    DatabaseTableName = DatabaseTable.PD_PARAMETERS;
                    break;
                default:
                    break;
            }
        }

        private void SetDatabaseFieldName()
        {
            switch (XmlPropertyName)
            {
                case XmlNodes.StandardName:
                    DatabaseFieldName = DatabaseFields.STANDARDNAME;
                    break;
                case XmlNodes.Version:
                    DatabaseFieldName = DatabaseFields.VERSION;
                    break;
                case XmlNodes.Description:
                    DatabaseFieldName = DatabaseFields.DESCRIPTION;
                    break;
                case XmlNodes.QtyBaseUnit:
                    DatabaseFieldName = DatabaseFields.QTYBASEUNIT;
                    break;
                case XmlNodes.TextTemplate:
                    DatabaseFieldName = DatabaseFields.TEXTTEMPLATE;
                    break;
                case XmlNodes.MassTemplate:
                    DatabaseFieldName = DatabaseFields.MASSTEMPLATE;
                    break;
                case XmlNodes.MassUnit:
                    DatabaseFieldName = DatabaseFields.MASSUNIT;
                    break;
                case XmlNodes.GroupReference:
                    DatabaseFieldName = DatabaseFields.GROUPREFERENCE;
                    break;
                case XmlNodes.DrawingName:
                    DatabaseFieldName = DatabaseFields.DRAWINGNAME;
                    break;
                case XmlNodes.NoPrjSpecFlag:
                    DatabaseFieldName = DatabaseFields.NOPRJSPECFLAG;
                    break;
                case XmlNodes.IdTagTemplate:
                    DatabaseFieldName = DatabaseFields.IDTAGTEMPLATE;
                    break;
                case XmlNodes.Name:
                    DatabaseFieldName = DatabaseFields.MTRNAME;
                    break;
                case XmlNodes.Mass:
                    DatabaseFieldName = DatabaseFields.MASS;
                    break;
                case XmlNodes.IdTag:
                    DatabaseFieldName = DatabaseFields.IDTAG;
                    break;
                case XmlNodes.MaterialCode:
                    DatabaseFieldName = DatabaseFields.MATERIALCODE;
                    break;
                case XmlNodes.Number:
                    DatabaseFieldName = DatabaseFields.PARNUMBER;
                    break;
                case XmlNodes.Context:
                    DatabaseFieldName = DatabaseFields.CONTEXT;
                    break;
                case XmlNodes.ValueType:
                    DatabaseFieldName = DatabaseFields.VALUETYPE;
                    break;
                case XmlNodes.Value:
                    DatabaseFieldName = DatabaseFields.PARVALUE;
                    break;
                case XmlNodes.Unit:
                    DatabaseFieldName = DatabaseFields.PARUNIT;
                    break;
                case XmlNodes.ValueOptions:
                    DatabaseFieldName = DatabaseFields.VALUEOPTIONS;
                    break;
                case XmlNodes.ContextString:
                    DatabaseFieldName = DatabaseFields.CONTEXTSTRING;
                    break;
                case XmlNodes.ExternalName:
                    DatabaseFieldName = DatabaseFields.EXTERNALNAME;
                    break;
                case XmlNodes.GroupName:
                    DatabaseFieldName = DatabaseFields.GROUPNAME;
                    break;
                default:
                    break;
            }
        }
    }
}
