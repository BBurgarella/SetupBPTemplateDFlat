using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Security.Principal;
using System.Drawing;
using System.Text;


public class ModNameInputForm : Form
{
    private Label label;
    private TextBox textBox;
    private Button buttonOk;
    private Button buttonCancel;
    private string modName;

    public ModNameInputForm()
    {
        // Set the title of the form
        this.Text = "Enter Mod Name";

        // Create a new Label control
        label = new Label();
        label.Left = 50;
        label.Top = 20;
        label.Text = "Please enter your mod name:";

        // Create a new TextBox control
        textBox = new TextBox();
        textBox.Left = 50;
        textBox.Top = 50;
        textBox.Width = 200;

        // Create a new OK Button control
        buttonOk = new Button();
        buttonOk.Text = "OK";
        buttonOk.Left = 50;
        buttonOk.Width = 70;
        buttonOk.Top = 80;
        buttonOk.Click += (sender, e) =>
        {
            modName = textBox.Text;
            this.Close();
        };

        // Create a new Cancel Button control
        buttonCancel = new Button();
        buttonCancel.Text = "Cancel";
        buttonCancel.Left = 130;
        buttonCancel.Width = 70;
        buttonCancel.Top = 80;
        buttonCancel.Click += (sender, e) =>
        {
            this.Close();
        };

        // Add the controls to the form
        this.Controls.Add(label);
        this.Controls.Add(textBox);
        this.Controls.Add(buttonOk);
        this.Controls.Add(buttonCancel);
    }

    public string GetModName()
    {
        this.ShowDialog();
        return modName;
    }
}

public class LogWindow : Form
{
    /**
    The LogWindow class creates a new window that contains a TextBox and a ProgressBar control.
    The TextBox control is used to display logs to the user while the program is running.
    The ProgressBar control is used to display the progress of a task being executed by the program.
    */

    private TextBox logTextBox;
    private ProgressBar progressBar;

    public LogWindow()
    {
        // Set the title of the form
        this.Text = "Logs";

        // Set the size of the form
        this.Size = new Size(400, 400);

        // Create a new TextBox control
        logTextBox = new TextBox();
        logTextBox.Multiline = true;
        logTextBox.ScrollBars = ScrollBars.Vertical;
        logTextBox.Dock = DockStyle.Fill;

        // Add the TextBox control to the form
        this.Controls.Add(logTextBox);

        // Create a new ProgressBar control
        progressBar = new ProgressBar();
        progressBar.Dock = DockStyle.Bottom;
        progressBar.Style = ProgressBarStyle.Continuous;

        // Add the ProgressBar control to the form
        this.Controls.Add(progressBar);
    }

    public void AddLogs(string Text_to_log)
    {
        //This method adds the provided text to the log text box control.
        this.logTextBox.AppendText(Text_to_log + Environment.NewLine);
    }

    public void UpdateProgress(int value)
    {
        // Updates the progess bar, in debug it was quite slow so I added this
        // for User feedback
        this.progressBar.Value = value;
    }
}

// The main program class.
class Program
{
    // This method prompts the user to select a folder using a dialog box.
    // It takes two parameters: a description of the folder to select (used as a prompt)
    // and an optional default directory to start in.
    static string GetFolder(string desc, string dir = "")
    {
        // Create a new FolderBrowserDialog and set its properties.
        var foldername = new FolderBrowserDialog();
        foldername.Description = desc;
        foldername.RootFolder = Environment.SpecialFolder.MyComputer;
        foldername.SelectedPath = dir;

        string folder = "";
        // If the user selects a folder and clicks OK, set the folder variable to the selected path.
        if (foldername.ShowDialog() == DialogResult.OK)
        {
            folder += foldername.SelectedPath;
        }
        return folder;
    }

    static void CopyDirectory(string sourceDir, string targetDir, LogWindow TgtLogs)
    {
        Directory.CreateDirectory(targetDir);

        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string destFile = Path.Combine(targetDir, Path.GetFileName(file));
            File.Copy(file, destFile, true);
            TgtLogs.AddLogs("Copied file:" + file + " into " + destFile);
        }

        foreach (string subDir in Directory.GetDirectories(sourceDir))
        {
            string destSubDir = Path.Combine(targetDir, Path.GetFileName(subDir));
            CopyDirectory(subDir, destSubDir, TgtLogs);
        }
    }

    static int CountFiles(string directory)
    {
        // Get the number of files in the specified directory
        int count = Directory.GetFiles(directory).Length;

        // Recursively count the number of files in each subdirectory
        foreach (string subDir in Directory.GetDirectories(directory))
        {
            count += CountFiles(subDir);
        }

        // Return the total number of files
        return count;
    }

    // This method kills all processes with "Unity" in their names.
    static void KillUnityProcesses()
    {
        Process[] unityProcesses = Process.GetProcesses().Where(p => p.ProcessName.Contains("unity")).ToArray();
        foreach (Process process in unityProcesses)
        {
            process.Kill();
        }
        unityProcesses = Process.GetProcesses().Where(p => p.ProcessName.Contains("Unity")).ToArray();
        foreach (Process process in unityProcesses)
        {
            process.Kill();
        }
    }

    public static bool IsAdministrator()
    {
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    // The main method of the program.
    [STAThread]
    static void Main(string[] args)
    {
        try
        {

        //Request administrator privileges if not already running as admin.
        if (!IsAdministrator())
        {
            DialogResult result = MessageBox.Show("You are not running the program as Administrator, it is adviced to run this program with Admin rights", "Continue or Stop", MessageBoxButtons.OKCancel);

            if (result == DialogResult.Yes)
            {
                // User clicked "Continue"
            }
            else if (result == DialogResult.No)
            {
                    Application.Exit();
                    return;
            }    
        }

            LogWindow logs = new LogWindow();
            logs.Show();


            // Warn the user
            string Warn = "Warning, when you will click OK. this programm will force-kill any Unity process running, make sure to save your work before clicking OK";
            MessageBox.Show(Warn, "Save Unity Progress", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // Then kill any existing Unity processes.
            KillUnityProcesses();

            // Prompt the user to select the Wrath install directory and set the "WrathPath" environment variable.
            string wrathPath = GetFolder("Select your Wrath install directory (contains Wrath.exe)", @"C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Second Adventure");
            logs.AddLogs("Wrath install directory chosen: " + wrathPath + "Setting the environement variable, please wait...");
            Environment.SetEnvironmentVariable("WrathPath", wrathPath, EnvironmentVariableTarget.User);
            logs.AddLogs("Envrionement variable set");

            // Get the current directory and create a new directory for the mod project.
            string currentDir = GetFolder("Select the BasicTemplate directory");
            string modProject = GetFolder("Select your mod project directory", currentDir);
            logs.AddLogs("Mod directory chosen: " + currentDir);
            Directory.CreateDirectory(modProject);

            // Copy the files from the BasicTemplate directory to the mod project directory.
            string projectFiles = Path.Combine(currentDir, "BasicTemplate");
            string[] files = Directory.GetFiles(projectFiles, "*", SearchOption.AllDirectories);

            CopyDirectory(projectFiles, modProject, logs);

            // Set the progress bar to 100% 
            logs.UpdateProgress(100);

            // Prompt the user to select the Unity project directory and copy the Unity files to that directory.
            string unityProject = GetFolder("Select your Unity project directory", currentDir);
            string unityProject_assets = Path.Combine(unityProject, "Assets");
            string unityFiles = Path.Combine(currentDir, "Assets");

            logs.UpdateProgress(100);
            CopyDirectory(unityFiles, unityProject_assets, logs);

            // Set the progress bar to 100% when the task is complete
            logs.UpdateProgress(100);

            // Ask the user for the name of the mod and replace the "BasicTemplate" string with the mod name in the project files.
            ModNameInputForm modNameInputForm = new ModNameInputForm();
            string modName = modNameInputForm.GetModName();

            int numFiles = CountFiles(modProject);
            int count = 0;

            foreach (string file in Directory.GetFiles(modProject, "*", SearchOption.AllDirectories))
            {
                string destFile = file.Replace("BasicTemplate", modName);

                if (file.Contains("BasicTemplate"))
                {
                    logs.AddLogs("Renaming file: " + file + " to " + destFile);
                    File.Move(file, destFile);
                    logs.AddLogs("");
                }
                else
                {
                    string content = File.ReadAllText(file);
                    logs.AddLogs("Looking for and replacing 'BasicTemplate' in the file : " + count + "/" + numFiles);
                    content = content.Replace("BasicTemplate", modName);
                    File.WriteAllText(file, content);
                    logs.AddLogs("");
                }

                // Update the progress bar
                int progress = (int)(((float)count / (float)numFiles) * 100);
                logs.UpdateProgress(progress);

                count++;
            }

            // Set the progress bar to 100% 
            logs.UpdateProgress(100);

           //Replace the "BasicTemplate" string with the mod name in the Unity files.
           numFiles = CountFiles(unityProject);
           count = 0;
           foreach (string file in Directory.GetFiles(unityProject, "*", SearchOption.AllDirectories))
           {
               //if (file.Contains("Library"))
               //{
               //    continue;
               //}
               string content = File.ReadAllText(file, System.Text.Encoding.UTF8);
               if (content.Contains("basictemplate"))
                {
                    logs.AddLogs("Found 'basictemplate' in" + file);
                    content = content.Replace("basictemplate", modName.ToLower());
                    File.WriteAllText(file, content, new UTF8Encoding(false));
                }
               int progress = (int)(((float)count / (float)numFiles) * 100);
               logs.UpdateProgress(progress);
               if (count % 100 == 0)
               {
                   logs.AddLogs("Looking for and replacing 'BasicTemplate' in the file : " + count + "/" + numFiles);
                   logs.AddLogs("");
               }
               count++;
           }
            
            // Show a message box to indicate that the operation is complete.
            MessageBox.Show("Done", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
