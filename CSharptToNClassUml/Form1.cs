using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace CSharptToNClassUml
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox2.Text = Parser.CodeToXElement(richTextBox1.Text).ToString();
            try
            {
                //richTextBox2.Text = Parser.CodeToXElement(richTextBox1.Text).ToString();
            }
            catch(Exception ex)
            {
                richTextBox2.Text = ex.Message;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Программа предназначена для конвертирования исходного кода c# в xml для программы NClass."+
                "\nПрограмма написанна на скорую руку за один вечер." +
                "\nОна умеет работать только с полями, свойствами, методами."+
                "\nПроверяйте сгенерирванный код перед вставкой."+
                "\nНе рекомендуется для коммерческого использования.");
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open code File";
            theDialog.Filter = "cs files|*.cs";
            theDialog.Multiselect = true;
            //theDialog.InitialDirectory = @"C:\";
            if (theDialog.ShowDialog() == DialogResult.OK)
            {
                List<string> codePages = new List<string>();
                foreach(string fname in theDialog.FileNames)
                    codePages.Add(File.ReadAllText(fname));
                try
                {
                    richTextBox2.Text = Parser.CodeToXElement(codePages).ToString();
                }
                catch (Exception ex)
                {
                    richTextBox2.Text = ex.Message;
                }
            }
        }
    }
}
