using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Security.Principal;
using System.Drawing;


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
            DialogResult result = MessageBox.Show("You are not running the program as Administrator, it might work but you might face bugs", "Continue or Stop", MessageBoxButtons.OKCancel);

            if (result == DialogResult.Yes)
            {
                // User clicked "Continue"
            }
            else if (result == DialogResult.No)
            {
                return;
            }    
        }

            LogWindow logWindow = new LogWindow();
            logWindow.Show();


            // Warn the user
            string Warn = "Warning, when you will click OK. this programm will force-kill any Unity process running, make sure to save your work before clicking OK";
            MessageBox.Show(Warn, "Save Unity Progress", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // Then kill any existing Unity processes.
            KillUnityProcesses();

            // Prompt the user to select the Wrath install directory and set the "WrathPath" environment variable.
            string wrathPath = GetFolder("Select your Wrath install directory (contains Wrath.exe)", @"C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Second Adventure");
            logWindow.AddLogs("Wrath install directory chosen: " + wrathPath + "Setting the environement variable, please wait...");
            Environment.SetEnvironmentVariable("WrathPath", wrathPath, EnvironmentVariableTarget.User);
            logWindow.AddLogs("Envrionement variable set");

            // Get the current directory and create a new directory for the mod project.
            string currentDir = GetFolder("Select the BasicTemplate directory");
            string modProject = GetFolder("Select your mod project directory", currentDir);
            logWindow.AddLogs("Mod directory chosen: " + currentDir);
            Directory.CreateDirectory(modProject);

            // Copy the files from the BasicTemplate directory to the mod project directory.
            string projectFiles = Path.Combine(currentDir, "BasicTemplate");
            string[] files = Directory.GetFiles(projectFiles, "*", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                string destFile = Path.Combine(modProject, Path.GetFileName(file));
                logWindow.AddLogs("Copying file: " + file + " to " + destFile);

                // Calculate the progress percentage
                int progress = (int)(((float)i / (float)files.Length) * 100);

                // Update the progress bar
                logWindow.UpdateProgress(progress);

                File.Copy(file, destFile, true);

                logWindow.AddLogs("");
            }

            // Set the progress bar to 100% 
            logWindow.UpdateProgress(100);

            // Prompt the user to select the Unity project directory and copy the Unity files to that directory.
            string unityProject = GetFolder("Select your Unity project directory", currentDir);
            string unityFiles = Path.Combine(currentDir, "Assets");

            files = Directory.GetFiles(unityFiles, "*", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                string destFile = Path.Combine(unityProject, Path.GetFileName(file));
                logWindow.AddLogs("Copying file: " + file + " to " + destFile);

                // Calculate the progress percentage
                int progress = (int)(((float)i / (float)files.Length) * 100);

                // Update the progress bar
                logWindow.UpdateProgress(progress);

                File.Copy(file, destFile, true);

                logWindow.AddLogs("");
            }

            // Set the progress bar to 100% when the task is complete
            logWindow.UpdateProgress(100);

            // Ask the user for the name of the mod and replace the "BasicTemplate" string with the mod name in the project files.
            string modName = "";
            using (var form = new Form())
            {
                form.Text = "Enter Mod Name";
                var label = new Label() { Left = 50, Top = 20, Text = "Please enter your mod name:" };
                var textBox = new TextBox() { Left = 50, Top = 50, Width = 200 };
                var buttonOk = new Button() { Text = "OK", Left = 50, Width = 70, Top = 80 };
                var buttonCancel = new Button() { Text = "Cancel", Left = 130, Width = 70, Top = 80 };
                buttonOk.Click += (sender, e) => { modName = textBox.Text; form.Close(); };
                buttonCancel.Click += (sender, e) => { form.Close(); };
                form.Controls.Add(label);
                form.Controls.Add(textBox);
                form.Controls.Add(buttonOk);
                form.Controls.Add(buttonCancel);
                form.ShowDialog();
            }

            int numFiles = CountFiles(modProject);
            int count = 0;

            foreach (string file in Directory.GetFiles(modProject, "*", SearchOption.AllDirectories))
            {
                string destFile = file.Replace("BasicTemplate", modName);

                if (file.Contains("BasicTemplate"))
                {
                    logWindow.AddLogs("Renaming file: " + file + " to " + destFile);
                    File.Move(file, destFile);
                    logWindow.AddLogs("");
                }
                else
                {
                    string content = File.ReadAllText(file);
                    logWindow.AddLogs("Looking for and replacing 'BasicTemplate' in the file : " + count + "/" + numFiles);
                    content = content.Replace("BasicTemplate", modName);
                    File.WriteAllText(file, content);
                    logWindow.AddLogs("");
                }

                // Update the progress bar
                int progress = (int)(((float)count / (float)numFiles) * 100);
                logWindow.UpdateProgress(progress);

                count++;
            }

            // Set the progress bar to 100% 
            logWindow.UpdateProgress(100);

            // Replace the "BasicTemplate" string with the mod name in the Unity files.
            numFiles = CountFiles(unityProject);
            count = 0;
            foreach (string file in Directory.GetFiles(unityProject, "*", SearchOption.AllDirectories))
            {
                string content = File.ReadAllText(file);
                content = content.Replace("BasicTemplate", modName);
                File.WriteAllText(file, content);
                int progress = (int)(((float)count / (float)numFiles) * 100);
                logWindow.UpdateProgress(progress);
                if (count % 100 == 0)
                {
                    logWindow.AddLogs("Looking for and replacing 'BasicTemplate' in the file : " + count + "/" + numFiles);
                    logWindow.AddLogs("");
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
