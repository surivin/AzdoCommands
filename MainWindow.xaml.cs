using System;
using System.Configuration;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace AzdoCommands
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        XmlDocument doc = new XmlDocument();
        public MainWindow()
        {
            InitializeComponent();
            doc.Load("Commands.xml");
            BindCommandsCmb();
            BindEnvironmentCmb();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            output.Text = "";
            var command = GetCommandFromXML(environmentCmb.SelectedItem.ToString());

            Response result = Shell.Term($"echo hello & pause & echo hello again", Output.Internal);
            if (result.code == 0)
            {
                output.Text = (result.stdout);
            }
            else
            {
                output.Text = (result.stderr);
            }
        }

        private string GetCommandFromXML(string environment)
        {
            foreach (XmlNode env in doc.ChildNodes[0].ChildNodes)
            {
                if (env.Attributes["name"].Value.ToLower() == environment.Split('-')[0].ToLower())
                {
                    foreach (XmlNode command in env.ChildNodes)
                    {
                        if (command.Attributes["name"].Value.ToLower() == commandCmb.Text.ToLower())
                        {
                            return command.InnerText.Replace("[Env]", environment);
                        }
                    }
                }
            }
            return "";
        }

        private void BindEnvironmentCmb()
        {
            var env = ConfigurationManager.AppSettings["Environments"].ToString();

            environmentCmb.DataContext = env.Split(',');            
        }

        private void BindCommandsCmb()
        {
            var commands = ConfigurationManager.AppSettings["Commands"].ToString();

            commandCmb.DataContext = commands.Split(',');
        }
    }
}