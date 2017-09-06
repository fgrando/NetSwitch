using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Management.Instrumentation;

namespace NetSwitch
{
    public partial class Form1 : Form
    {
        private bool allowshowdisplay = false;
        private const string deviceDisabled = "22"; 
        private const string menuExitText = "Exit";
        private List<ManagementObject> devList = new List<ManagementObject>();
        private ToolStripMenuItem menuExit = new ToolStripMenuItem(menuExitText);

        public Form1()
        {
            InitializeComponent();

            Hide(); // will be only in notification area
            UpdateItems();
        }

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(allowshowdisplay ? value : allowshowdisplay);
        }

        private string Uid(ManagementObject obj)
        {
            // create the interface name to be displayed 
            string sName = obj["Index"].ToString() + ": " + obj["Name"].ToString();
            return sName;
        }

        private void UpdateItems()
        {
            UpdateInterfaces();
            UpdateMenu();
        }

        private void UpdateInterfaces()
        {
            devList.Clear();

            string NamespacePath = "\\\\.\\ROOT\\cimv2";
            string ClassName = "Win32_NetworkAdapter";
            ManagementClass oClass = new ManagementClass(NamespacePath + ":" + ClassName);
            foreach (ManagementObject oObject in oClass.GetInstances())
            {
                if (IsNetworkDevice(oObject))
                {
                    devList.Add(oObject);
                }
            }
        }

        private Boolean IsNetworkDevice(ManagementObject obj)
        {
            return (obj["NetEnabled"] != null);
        }

        private Boolean IsDisabled(ManagementObject obj)
        {
            if(obj["ConfigManagerErrorCode"] != null)
            {
                return (obj["ConfigManagerErrorCode"].ToString() == "22");
            }

            return false;
        }

        private void UpdateMenu()
        {
            // erase all entries in menu
            contextMenuStrip1.Items.Clear();

            foreach (ManagementObject oObject in devList)
            {
                // set as checked the enabled devices 
                ToolStripMenuItem m = new ToolStripMenuItem(Uid(oObject))
                {
                    Checked = IsDisabled(oObject) ? false : true
                };

                // add to menu
                contextMenuStrip1.Items.Add(m);
            }
            
            // remember to add the exit option or user will be sad...
            this.contextMenuStrip1.Items.Add(menuExit);
        }

        private void ContextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            // if the item is the exit, bye bye
            if (menuExitText == e.ClickedItem.Text)
            {
                Close();
            }

            // find the interface in or device list 
            foreach (ManagementObject oObject in devList)
            {
                if (e.ClickedItem.Text.ToString() == Uid(oObject))
                {
                    //command in powershell: get-netadapter -ifIndex 7 | Disable-NetAdapter -Confirm:$false
                    string method = IsDisabled(oObject) ? "Enable" : "Disable";
                    oObject.InvokeMethod(method, null);
                }
            }

            // refresh all 
            UpdateItems();
        }
    }
}
