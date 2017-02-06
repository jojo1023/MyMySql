using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace MyMySql
{
    public partial class Form1 : Form
    {
        string xmlUrl;
        XDocument xdoc;
        Database database;
        public Form1()
        {
            InitializeComponent();
            xmlUrl = "SqlData.xml";
            xdoc = XDocument.Load(xmlUrl);
            database = new Database(xdoc);
            FormClosing += Form1_FormClosing;

        }
        private void executeButton_Click(object sender, EventArgs e)
        {
            OutputInfo outputInfo;
            string selectedText = inputTextBox.SelectedText;
            if (selectedText != "")
            {
                outputInfo = database.DoSQlStuff(selectedText);
            }
            else
            {
                outputInfo = database.DoSQlStuff(inputTextBox.Text);
            }
            errorTextBox.Text = "";
            outputTextBox.Text = outputInfo.Output;
            for(int i = 0; i < outputInfo.Errors.Count; i++)
            {
                errorTextBox.Text += outputInfo.Errors[i];
                if(i + 1 < outputInfo.Errors.Count)
                {
                    errorTextBox.Text += Environment.NewLine;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            database.SaveXML(xdoc, xmlUrl);
        }
    }
}
