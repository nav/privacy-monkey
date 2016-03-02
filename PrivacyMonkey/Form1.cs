using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;
using System.Threading;
using ImageMagick;


namespace PrivacyMonkey
{
    public partial class Form1 : Form
    {
        OpenFileDialog ofd = new OpenFileDialog();
        String bank, selectedFile, selectedSafeFile;
        List<String> outputfiles = new List<String>();
        static String fromEmail = "onewebdev@gmail.com";
        static String toEmail = "nav@navaulakh.com";

        SmtpClient smtp = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromEmail, "xxx")
        };

        public Form1()
        {
            InitializeComponent();
            txtSelectedFile.Text = "";
        }


        private void ConversionRoutine()
        {
            List<String> outputfiles_inner = new List<String>();
            MagickReadSettings settings = new MagickReadSettings();

            settings.Density = new PointD(300, 300);
            String outputfile;
            System.Diagnostics.ProcessStartInfo procInfo = new System.Diagnostics.ProcessStartInfo();
            procInfo.FileName = ("mspaint.exe");

            using (MagickImageCollection images = new MagickImageCollection())
            {
                images.Read(selectedFile, settings);
                int page = 1;
                foreach (MagickImage image in images)
                {
                    outputfile = String.Format(@"{0}\output-page-{1}.png", Path.GetDirectoryName(selectedFile), page);
                    image.Write(outputfile);
                    //this.Invoke(new Action(() => outputfiles.Add(outputfile)));
                    outputfiles.Add(outputfile);
                    procInfo.Arguments = outputfile;
                    System.Diagnostics.Process.Start(procInfo);
                    page++;
                }
            }

            this.Invoke((MethodInvoker)delegate {
                btnConvert.Enabled = true;
                lstOutputFiles.Items.Clear();
                for (int i = 0; i < outputfiles.Count; i++)
                {
                    lstOutputFiles.Items.Add(outputfiles[i]);
                }
            });
        }


        private void btnSelect_Click(object sender, EventArgs e)
        {
            ofd.Filter = "PDF|*.pdf";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                selectedFile = ofd.FileName;
                selectedSafeFile = ofd.SafeFileName;
                txtSelectedFile.Text = selectedFile;
                btnConvert.Enabled = true;
            }
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            bank = txtBank.Text;
            if (bank == "" || selectedFile == null)
            {
                MessageBox.Show(
                    "Please enter bank name and statement file",
                    "Missing Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            btnConvert.Enabled = false;
            ThreadStart threadStart = new ThreadStart(ConversionRoutine);
            Thread thread = new Thread(threadStart);
            thread.Start();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = "A kind person has sent you a statement",
                Body = "Financial Institution: " + bank
            })
            {
                btnSend.Enabled = false;
                btnSendOutlook.Enabled = false;
                for (int i = 0; i < outputfiles.Count; i++)
                {
                    message.Attachments.Add(new Attachment(outputfiles[i]));
                }
                smtp.Send(message);
                MessageBox.Show("Email sent successfully. Thanks for your help!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnSend.Enabled = true;
                btnSendOutlook.Enabled = true;
            }
        }

        private void btnSendOutlook_Click(object sender, EventArgs e)
        {
            int count = outputfiles.Count;
            int remaining = count - 1;

            if (count > 1)
            {
                MessageBox.Show(
                    "Since there are " + count + " files generated, outlook can only attach one file automatically. Please attach the remaining " + remaining + " file(s) manually.",
                    "Missing Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            System.Diagnostics.ProcessStartInfo procInfo = new System.Diagnostics.ProcessStartInfo();
            procInfo.FileName = ("outlook.exe");
            procInfo.Arguments = "/a " + outputfiles[0] + " /c ipm.note /m \"nav@navaulakh.com&subject=Here's a sample statement\"";
            System.Diagnostics.Process.Start(procInfo);
        }

        private void txtBank_TextChanged(object sender, EventArgs e)
        {
            btnConvert.Enabled = txtBank.Text != "" && selectedFile != null;
        }

        private void chkConfirm_CheckedChanged(object sender, EventArgs e)
        {
            btnSend.Enabled = chkConfirm.Checked && outputfiles.Count > 0;
            btnSendOutlook.Enabled = chkConfirm.Checked && outputfiles.Count > 0;
        }
    }
}
