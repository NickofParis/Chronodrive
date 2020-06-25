using System;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zebra.Sdk.Card.Printer.Discovery;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Card.Printer;
using Zebra.Sdk.Card.Containers;
using Zebra.Sdk.Card.Job.Template;
using Zebra.Sdk.Printer.Discovery;
using System.Threading;
using System.Xml;
using System.IO;

namespace Chronodrive
{
    public partial class Form1 : Form
    {
        private const int CARD_FEED_TIMEOUT = 30000;
        static string Value1;
        static string Template;


        public Form1()
        {
            InitializeComponent();
            label1.Visible = false;
            textBoxId.Visible = false;
            textBoxId.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBoxId.Text != "")
            {
                Value1 = textBoxId.Text;
                ZebraCardPrinter zebraCardPrinter = null;
                Connection connection = null;
                String usbAdress = null;
                try
                {
                    foreach (DiscoveredPrinter usbPrinter in UsbDiscoverer.GetZebraUsbPrinters(new ZebraCardPrinterFilter()))
                    {
                        usbAdress = usbPrinter.Address;
                    }
                    connection = new UsbConnection(usbAdress);
                    connection.Open();

                    zebraCardPrinter = ZebraCardPrinterFactory.GetInstance(connection);
                    ZebraTemplate zebraCardTemplate = new ZebraCardTemplate(zebraCardPrinter);
                    //string templateData = GetTemplateData();
                    List<string> templateFields = zebraCardTemplate.GetTemplateDataFields(Template);
                    Dictionary<string, string> fieldData = PopulateTemplateFieldData(templateFields);

                    // Generate template job
                    TemplateJob templateJob = zebraCardTemplate.GenerateTemplateDataJob(Template, fieldData);

                    // Send job
                    int jobId = zebraCardPrinter.PrintTemplate(1, templateJob);

                    // Poll job status
                    JobStatusInfo jobStatus = PollJobStatus(jobId, zebraCardPrinter);
                    //labelStatus.Text = "Impression OK";
                    //Console.WriteLine($"Job {jobId} completed with status '{jobStatus.PrintStatus}'.");
                }
                catch (Exception ev)
                {
                    labelStatus.Text = "Erreur d'impression : " + ev.Message;
                    //Console.WriteLine($"Error printing template: {ev.Message}");
                }
                finally
                {
                    CloseQuietly(connection, zebraCardPrinter);
                }
            }
            else
            {
                MessageBox.Show("Pas de valeur");
            }


        }
        #region Template
        private static string GetTemplateData()
        {
            return "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                   "<template name=\"TemplateExample\" card_type=\"0\" card_thickness=\"40\" delete=\"yes\" source=\"feeder\" destination=\"eject\">\r\n" +
                   "  <fonts>\r\n" +
                   "    <font id=\"1\" name=\"Arial\" size=\"8\" bold=\"no\" italic=\"no\" underline=\"no\"/>\r\n" +
                   "    <font id=\"2\" name=\"Arial\" size=\"6\" bold=\"no\" italic=\"no\" underline=\"no\"/>\r\n" +
                   "    <font id=\"3\" name=\"Arial\" size=\"10\" bold=\"yes\" italic=\"no\" underline=\"no\"/>\r\n" +
                   "  </fonts>\r\n" +
                   "  <sides>\r\n" +
                   "    <side name=\"front\" orientation=\"portrait\" rotation=\"180\" k_mode=\"mixed\">\r\n" +
                   "      <print_types>\r\n" +
                   "        <print_type type=\"monochrome\">\r\n" +
                   "          <text field=\"\" font_id=\"1\" x=\"40\" y=\"20\" width=\"0\" height=\"0\" angle=\"0\" color=\"0x000000\">Votre numero de client:</text>\r\n" +
                   "          <text field=\"\" font_id=\"1\" x=\"40\" y=\"346\" width=\"0\" height=\"0\" angle=\"0\" color=\"0x000000\">Votre numero de client:</text>\r\n" +
                   "          <text field=\"\" font_id=\"1\" x=\"40\" y=\"694\" width=\"0\" height=\"0\" angle=\"0\" color=\"0x000000\">Votre numero de client:</text>\r\n" +
                   "          <text field=\"\" font_id=\"2\" x=\"10\" y=\"276\" width=\"0\" height=\"0\" angle=\"0\" color=\"0x000000\">Ce code barre vous sert pour vous identifier a la borne</text>\r\n" +
                   "          <text field=\"\" font_id=\"2\" x=\"10\" y=\"624\" width=\"0\" height=\"0\" angle=\"0\" color=\"0x000000\">Ce code barre vous sert pour vous identifier a la borne</text>\r\n" +
                   "          <text field=\"\" font_id=\"2\" x=\"10\" y=\"950\" width=\"0\" height=\"0\" angle=\"0\" color=\"0x000000\">Ce code barre vous sert pour vous identifier a la borne</text>\r\n" +
                   "          <text field=\"clientid\" font_id=\"3\" x=\"420\" y=\"20\" width=\"0\" height=\"0\" angle=\"0\" color=\"0x000000\"/>\r\n" +
                   "          <text field=\"clientid\" font_id=\"3\" x=\"420\" y=\"346\" width=\"0\" height=\"0\" angle=\"0\" color=\"0x000000\"/>\r\n" +
                   "          <text field=\"clientid\" font_id=\"3\" x=\"420\" y=\"694\" width=\"0\" height=\"0\" angle=\"0\" color=\"0x000000\"/>\r\n" +
                   "          <barcode field=\"clientid\" code=\"code128\" x=\"140\" y=\"84\" width=\"420\" height=\"150\" bar_height=\"150\" multiplier=\"8\"/>\r\n" +
                   "          <barcode field=\"clientid\" code=\"code128\" x=\"140\" y=\"422\" width=\"420\" height=\"150\" bar_height=\"150\" multiplier=\"8\"/>\r\n" +
                   "          <barcode field=\"clientid\" code=\"code128\" x=\"140\" y=\"770\" width=\"420\" height=\"150\" bar_height=\"150\" multiplier=\"8\"/>\r\n" +
                   "        </print_type>\r\n" +
                   "      </print_types>\r\n" +
                   "    </side>\r\n" +
                   "  </sides>\r\n" +
                   "</template>\r\n";
        }
        // <exception cref="ArgumentException"></exception>
        /// <exception cref="System.IO.IOException"></exception>
        /// <exception cref="Zebra.Sdk.Card.Exceptions.ZebraCardException"></exception>
        /// <exception cref="Zebra.Sdk.Device.ZebraIllegalArgumentException"></exception>
        private static Dictionary<string, string> PopulateTemplateFieldData(List<string> templateFields)
        {
            Dictionary<string, string> fieldData = new Dictionary<string, string>();
            foreach (string fieldName in templateFields)
            {
                string fieldValue = "";
                switch (fieldName)
                {
                    case "employee":
                        fieldValue = Value1;
                        break;

                    case "clientid":
                        fieldValue = Value1;
                        break;

                }

                if (!string.IsNullOrEmpty(fieldValue))
                {
                    if (!fieldData.ContainsKey(fieldName))
                    {
                        fieldData.Add(fieldName, fieldValue);
                    }
                }
            }
            return fieldData;
        }
        #endregion Template

        #region JobStatus
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ConnectionException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="OverflowException"></exception>
        /// <exception cref="Zebra.Sdk.Settings.SettingsException"></exception>
        /// <exception cref="Zebra.Sdk.Card.Exceptions.ZebraCardException"></exception>
        private static JobStatusInfo PollJobStatus(int jobId, ZebraCardPrinter zebraCardPrinter)
        {
            JobStatusInfo jobStatusInfo = new JobStatusInfo();
            bool isFeeding = false;

            long start = Math.Abs(Environment.TickCount);
            while (true)
            {
                jobStatusInfo = zebraCardPrinter.GetJobStatus(jobId);

                if (!isFeeding)
                {
                    start = Math.Abs(Environment.TickCount);
                }

                isFeeding = jobStatusInfo.CardPosition.Contains("feeding");

                string alarmDesc = jobStatusInfo.AlarmInfo.Value > 0 ? $" ({jobStatusInfo.AlarmInfo.Description})" : "";
                string errorDesc = jobStatusInfo.ErrorInfo.Value > 0 ? $" ({jobStatusInfo.ErrorInfo.Description})" : "";

                //Console.WriteLine($"Job {jobId}: status:{jobStatusInfo.PrintStatus}, position:{jobStatusInfo.CardPosition}, alarm:{jobStatusInfo.AlarmInfo.Value}{alarmDesc}, error:{jobStatusInfo.ErrorInfo.Value}{errorDesc}");

                if (jobStatusInfo.PrintStatus.Contains("done_ok"))
                {
                    break;
                }
                else if (jobStatusInfo.PrintStatus.Contains("error") || jobStatusInfo.PrintStatus.Contains("cancelled"))
                {
                    //Console.WriteLine($"The job encountered an error [{jobStatusInfo.ErrorInfo.Description}] and was cancelled.");
                    break;
                }
                else if (jobStatusInfo.ErrorInfo.Value > 0)
                {
                    //Console.WriteLine($"The job encountered an error [{jobStatusInfo.ErrorInfo.Description}] and was cancelled.");
                    zebraCardPrinter.Cancel(jobId);
                }
                else if (jobStatusInfo.PrintStatus.Contains("in_progress") && isFeeding)
                {
                    if (Math.Abs(Environment.TickCount) > start + CARD_FEED_TIMEOUT)
                    {
                        //Console.WriteLine("The job timed out waiting for a card and was cancelled.");
                        zebraCardPrinter.Cancel(jobId);
                    }
                }

                Thread.Sleep(2000);
            }
            return jobStatusInfo;
        }
        #endregion JobStatus

        #region CleanUp
        private static void CloseQuietly(Connection connection, ZebraCardPrinter zebraCardPrinter)
        {
            try
            {
                if (zebraCardPrinter != null)
                {
                    zebraCardPrinter.Destroy();
                }
            }
            catch { }

            try
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }
            catch { }
        }
        #endregion CleanUp

        private void buttonSelectTemplate_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialogTemplate = new OpenFileDialog {
            InitialDirectory = "C:\\Zebra\\ZC\\",
            CheckFileExists = true,
            CheckPathExists = true,
            DefaultExt = "xml",
            Filter = "xml files (*.xml)|*.xml",
            FilterIndex = 2,
            RestoreDirectory = true
            };

            if (openFileDialogTemplate.ShowDialog() == DialogResult.OK)
            {
                labelTemplateName.Text = openFileDialogTemplate.SafeFileName;
                label1.Visible = true;
                textBoxId.Visible = true;
                textBoxId.Enabled = true;
            }

            Template = File.ReadAllText(openFileDialogTemplate.FileName, Encoding.UTF8);
            
            if(openFileDialogTemplate.SafeFileName == "Employee.xml")
            {
                label1.Text = "Employé :";
                textBoxId.Width = 300;
                textBoxId.Left = 149;
            }
            
        }
    }
}
