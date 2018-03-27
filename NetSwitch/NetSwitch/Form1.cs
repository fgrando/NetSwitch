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
using System.IO;

namespace NetSwitch
{
    public partial class Form1 : Form
    {
        private bool printedInterfacesStartup = false;
        private bool toogleConfigActive = false;
        private bool allowshowdisplay = false;
        private const string deviceDisabled = "22";
        private const string menuExitText = "Exit";
        private const string menuToggleText = "Toggle";
        private const string toggleFileConfig = "toggle.conf";
        private List<string> toggleToDeactivate = new List<string>();
        private string toggleToActivate = "";
        private List<ManagementObject> devList = new List<ManagementObject>();
        private ToolStripMenuItem menuExit = new ToolStripMenuItem(menuExitText);

        public Form1()
        {
            InitializeComponent();

            // parse toggle options, if configured:
            // loads the content of the file. Each line must contain the exact name of the interface. Only one of the interfaces is activated each toggle.
            if (File.Exists(toggleFileConfig))
            {
                List<string> interfaces = new List<string>();
                string[] lines = System.IO.File.ReadAllLines(toggleFileConfig);
                foreach(string line in lines)
                {
                    if (line.Count() > 0)
                        interfaces.Add(line);
                }
                if(interfaces.Count() > 1)
                {
                    Console.WriteLine("toggle configuration found");
                    toogleConfigActive = true;
                    toggleToActivate = interfaces.ElementAt(0);
                    interfaces.Remove(toggleToActivate);
                    toggleToDeactivate = interfaces;
                }
            }

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
            string NamespacePath = "\\\\.\\ROOT\\cimv2";
            string ClassName = "Win32_NetworkAdapter";
            
            // this message is useful to copy interface names and configuring toggle 
            if (printedInterfacesStartup == false)
                Console.WriteLine("add two or more of the following names to configure " + toggleFileConfig + " and then activate the Toggle function:");

            devList.Clear();
            ManagementClass oClass = new ManagementClass(NamespacePath + ":" + ClassName);
            foreach (ManagementObject oObject in oClass.GetInstances())
            {
                if (IsNetworkDevice(oObject))
                {
                    devList.Add(oObject);
                    // this message is useful to copy interface names and configuring toggle.
                    if (printedInterfacesStartup == false)
                        Console.WriteLine(Uid(oObject));
                }
            }
            // display only once the interface names
            printedInterfacesStartup = true;
        }

        private Boolean IsNetworkDevice(ManagementObject obj)
        {
            return (obj["NetEnabled"] != null);
        }

        private Boolean IsDisabled(ManagementObject obj)
        {
            if (obj["ConfigManagerErrorCode"] != null)
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

            // add the toggle option to toggle interfaces defined by user in configuration
            
            ToolStripMenuItem toggleOption = new ToolStripMenuItem(menuToggleText)
            {
                Enabled = toogleConfigActive
            };

            this.contextMenuStrip1.Items.Add(toggleOption);

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
            else if (menuToggleText == e.ClickedItem.Text)
            {
                ToggleUserInterfaces();
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

        private void ToggleUserInterfaces()
        {
            string aux = toggleToActivate;

            // if there is no element to deactivate, do not toggle
            if (toggleToDeactivate.Count() < 1)
                return;

            toggleToActivate = toggleToDeactivate.ElementAt(0);
            toggleToDeactivate.Remove(toggleToActivate);
            toggleToDeactivate.Add(aux); // add in the end

            Console.WriteLine("activating " + toggleToActivate);
            foreach(string str in toggleToDeactivate)
            {
                Console.WriteLine("deactive " + str);
            }

            foreach (ManagementObject oObject in devList)
            {
                // disable all interfaces and enable only one of the user set (toggle mode)
                foreach (string iface in toggleToDeactivate)
                {
                    if (iface == Uid(oObject))
                    {
                        string method = "Disable";
                        oObject.InvokeMethod(method, null);
                    }
                }

                if (toggleToActivate == Uid(oObject))
                {
                    string method = "Enable";
                    oObject.InvokeMethod(method, null);
                }

                UpdateMenu();
            }
        }
    }
}
