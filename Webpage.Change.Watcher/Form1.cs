using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Net.Mail;


namespace WebpageChangeWatcher
{
    public partial class Form1 : Form
    {
        private Timer timer1;
        private string urlForCheck = null;
        NotifyIcon mynotifyicon = new NotifyIcon();
        private int optionID;
        private string stringToCheck = null;

        public Form1()
        {
            
            InitializeComponent();
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            label6.Hide();
            textBox2.Hide();
            button1.Left = (this.ClientSize.Width - button1.Width) / 2; ;
            button2.Hide();
            button3.Hide();
            button2.Left = (this.ClientSize.Width - button2.Width) / 2;
            button3.Left = (this.ClientSize.Width - button3.Width) / 2;
            label1.Left = button2.Left - 20;
            label2.Left = button2.Left - 20;
            label3.Left = button2.Left - 20;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            urlForCheck = textBox1.Text;
            string dialogMessage;
            DialogResult dialogResult = DialogResult.No;

            if (Uri.IsWellFormedUriString(urlForCheck, UriKind.Absolute))
            {
                if (optionID == 1)
                {
                    if (textBox2.Text.Length > 5)
                    {
                        stringToCheck = textBox2.Text;
                        dialogMessage = string.Format("This will create a monitor for:\r\n{0} \r\n Are you sure?", urlForCheck);
                        dialogResult = MessageBox.Show(dialogMessage, "Settings Confirmation", MessageBoxButtons.YesNo);
                    }
                    else
                    {
                        dialogMessage = "String to search is too short!";
                        dialogResult = MessageBox.Show(dialogMessage, "Settings Confirmation", MessageBoxButtons.YesNo);
                        return;
                    } 
                }
                else if (optionID == 2)
                {
                    dialogMessage = string.Format("This will create a monitor for:\r\n{0} \r\n Are you sure?", urlForCheck);
                    dialogResult = MessageBox.Show(dialogMessage, "Settings Confirmation", MessageBoxButtons.YesNo);
                }
                
            }
            else
            {
                dialogMessage = "Illegal URL format!";
                MessageBox.Show(dialogMessage, "Alert", MessageBoxButtons.OK);
                return;
            }

            if (dialogResult == DialogResult.Yes)
            {
                InitTimer();
                label1.ForeColor = Color.Green;
                label1.Text = "Monitoring has been started.";
                UpdateTimingLabel();
                label4.Hide();
                textBox1.Hide();
                button3.Show();
                button1.Hide();
                button2.Show();
                label5.Hide();
                label6.Hide();
                comboBox1.Hide();
                textBox2.Hide();
                timer1_Tick(sender, e);
            }
            else 
            {
                return;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            StopTimer();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            timer1_Tick(sender, e);
        }

        public void InitTimer()
        {
            timer1 = new Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            Random randTimeInMinutes = new Random();
            timer1.Interval = randTimeInMinutes.Next(45, 60)*60*1000; //Interval is in miliseconds. 3600000 for 1 hour
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            CheckURL_ForUpdate(urlForCheck, optionID);
        }


        private void StopTimer()
        {
            if (timer1 != null)
            {
                timer1.Dispose();
                label1.ForeColor = Color.Red;
                label1.Text = "Monitoring has been stopped.";
                label2.Text = "";
                label3.Text = "";
            }
            else
            {
                label1.ForeColor = Color.Red;
                label1.Text = "Try starting the monitoring first ;)";
                return;
            }
        }

        private void UpdateTimingLabel()
        {
            label2.Text = "Last check:\r\n" + DateTime.Now + ".";
            if (timer1 != null)
            {
                label2.Text += "\r\n\r\nNext check cycle will be at:\r\n" + DateTime.Now.AddMilliseconds(timer1.Interval);
            }
            
        }

        private void CheckURL_ForUpdate(string url, int optionID)
        {
            UpdateTimingLabel();
            switch (optionID) {
                case 1:
                    checkByString(stringToCheck, url);
                    break;
                case 2:
                    checkByContentLength();
                    break;
            }

            return;
        }

        private void checkByString(string stringToCheck, string url)
        {
            WebClient client = new WebClient();
            string downloadString = client.DownloadString(url);

            if (!(downloadString.Contains(stringToCheck)))
            {
                MessageBox.Show("Item is back in stock!");
                label3.BackColor = Color.Black;
                label3.ForeColor = Color.LawnGreen;
                label3.Text = "***Item is back in stock***";
                //sendSuccessEmail("tomer.cohen@buzzr.biz");
                return;
            }
            else
            {
                label3.ForeColor = Color.Firebrick;
                label3.Text = "Item is still out of stock.";
                return;
            }
        }

        private void checkByContentLength()
        {

        }

        public bool sendSuccessEmail(string recipient)
        {

            try
            {
                MailMessage mail = new MailMessage("reports@buzzr.biz", recipient);
                SmtpClient mailClient = new SmtpClient();
                mailClient.Port = 587; //for gmail: 587
                mailClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                mailClient.UseDefaultCredentials = false;
                mailClient.Host = "za-smtp-outbound-1.mimecast.co.za";
                mailClient.Credentials = new System.Net.NetworkCredential("ramint\reports", "R3ports123");
                mail.Subject = "This is a test email subject";
                mail.Body = "This is a test email body";
                mailClient.Send(mail);
                return true;
            }
            
            catch (Exception e)
            {
                throw e;
            }
        }



        private void form1_Resize(object sender, EventArgs e)
        {            
            mynotifyicon.Click += new System.EventHandler(showWindow);
            mynotifyicon.Icon = new Icon(@"../../Oxygen-Icons.org-Oxygen-Places-certificate-server.ico");
            if (FormWindowState.Minimized == this.WindowState)
            {
                mynotifyicon.Visible = true;
                this.Hide();
            }

            else if (FormWindowState.Normal == this.WindowState)
            {
                mynotifyicon.Visible = false;
            }

        }

        private void showWindow(object sender, System.EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            mynotifyicon.Visible = false;
        }


        private void textBox1_KeyDown(object sender, KeyEventArgs e)
      {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(this, new EventArgs());
            }
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            var selectedItem = comboBox1.SelectedItem.ToString();
            if (selectedItem == "String")
            {
                optionID = 1;
                label6.Show();
                textBox2.Show();
            }

            else if (selectedItem == "ContentLength")
            {
                optionID = 2;
                label6.Hide();
                textBox2.Hide();
            }
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(this, new EventArgs());
            }
        }



    }
}
