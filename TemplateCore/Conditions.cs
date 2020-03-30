﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SmartExportTemplates.Utils;
using SmartExportTemplates.TemplateCore;
using System.Diagnostics;

namespace SmartExportTemplates
{
    class Conditions
    {
        TemplateParser templateParser = null;
        SmartExportTemplates.SmartExport ExportCore = (SmartExportTemplates.SmartExport)Globals.Instance.GetData(Constants.GE_EXPORT_CORE);

        public Conditions()
        {
            this.templateParser = (TemplateParser)Globals.Instance.GetData(Constants.GE_TEMPLATE_PARSER);
        }

        public void EvaluateCondition(XmlNode ConditionNode)
        {
            Stopwatch sw = Stopwatch.StartNew();

            bool ConditionEvaluated = false;

            try
            {

                //Evaluate the IF
                string CondText = ((XmlElement)ConditionNode).GetAttribute(Constants.SE_ATTRIBUTE_COND_TEST);
                ConditionEvaluation conditionEvaluation = new ConditionEvaluation(CondText);

                if (conditionEvaluation.CanEvaluate())
                {
                    processChildNodes(ConditionNode);
                    ConditionEvaluated = true;
                }
            
                // Evaluate the ELSIFs if IF has not satisfied
                if (!ConditionEvaluated)
                {
                    XmlNodeList elseIfNodeList = ConditionNode.ChildNodes;
                    foreach (XmlNode elseIfNode in elseIfNodeList)
                    {
                        if (elseIfNode.Name == Constants.NodeTypeString.SE_ELSIF)
                        {
                            CondText = ((XmlElement)elseIfNode).GetAttribute(Constants.SE_ATTRIBUTE_COND_TEST);
                            conditionEvaluation = new ConditionEvaluation(CondText);
                            if (conditionEvaluation.CanEvaluate())
                            {
                                processChildNodes(elseIfNode);
                                ConditionEvaluated = true;
                                break;
                            }
                            // reaching else node indicates there are no more nodes with node name ELSIF
                            if (elseIfNode.Name == Constants.NodeTypeString.SE_ELSE)
                            {
                                break;
                            }
                        }
                    }
                }

                // Evaluate the Else
                if (!ConditionEvaluated)
                {
                    XmlNodeList elseNodeList = ConditionNode.ChildNodes;
                    foreach (XmlNode elseNode in elseNodeList)
                    {
                        if ( elseNode.Name == Constants.NodeTypeString.SE_ELSE)
                        {
                            processChildNodes(elseNode);
                            ConditionEvaluated = true;
                            break;
                        }

                    }

                }
                if (!ConditionEvaluated)
                {
                    ExportCore.WriteLog("None of the conditions evaluated for the Node with test: " + CondText);
                }
            }
            catch (System.Exception exp)
            {
                string message = exp.Message;
                //if the problem was already caught at the child node level the line number
                // information would be already present in the exception message
                if (!message.Contains("Problem found at line number"))
                {
                    TemplateParser templateParser = (TemplateParser)Globals.Instance.GetData(Constants.GE_TEMPLATE_PARSER);
                    message = "Problem found at line number : " + templateParser.GetLineNumberForNode(ConditionNode) + "\n" + exp.Message;
                }
                throw new SmartExportException(message);
            }
            ExportCore.WriteDebugLog(" EvaluateCondition("+ConditionNode+") completed in " + sw.ElapsedMilliseconds + " ms.");

            sw.Stop();
        }

        private void processChildNodes(XmlNode parentNode)
        {
            DataElement dataElement = new DataElement();

            XmlNodeList childNodes = parentNode.ChildNodes;
            foreach (XmlNode childNode in childNodes)
            {
                if (childNode.Name == Constants.NodeTypeString.SE_DATA)
                    dataElement.EvaluateData(childNode);
                else if (childNode.Name == Constants.NodeTypeString.SE_IF)
                    new Conditions().EvaluateCondition(childNode);
                else if (childNode.Name == Constants.NodeTypeString.SE_FOREACH)
                    new Loops().EvaluateLoop(childNode);
                else if (childNode.Name == Constants.NodeTypeString.SE_ROWS)
                    new Tables().FetchTable(childNode);
            }

        }
    }
}
