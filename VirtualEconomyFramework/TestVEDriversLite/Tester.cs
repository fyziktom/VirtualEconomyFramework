using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TestVEDriversLite.Properties;

namespace TestVEDriversLite
{
    public partial class Tester : Form
    {
        Dictionary<string, MethodInfo> m_tests;

        public Tester()
        {
            InitializeComponent();
            var combo = ((DataGridViewComboBoxColumn)Tests.Columns[0]).Items;
            fillCombo(combo);
            FillConnections();
            System.Console.SetOut(new TextBoxWriter(Console));
            //Console.AddContextMenu();
            try
            {
                int count = Settings.Default.Functions?.Count ?? 0;
                if (count > 0) Tests.Rows.Add(count);
                for (int i = 0; i < count; i++)
                {
                    var row = Tests.Rows[i];

                    if (combo.Contains(Settings.Default.Functions[i]))
                    {
                        row.Cells[0].Value = Settings.Default.Functions[i];
                        row.Cells[1].Value = Settings.Default.Parameters[i];
                    }
                }
                if (!Settings.Default.Position.IsEmpty)
                {
                    DesktopBounds = Settings.Default.Position;
                }
                if (Settings.Default.Split > 0)
                {
                    this.split.SplitterDistance = Settings.Default.Split;
                }

            }
            catch (Exception) { }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
        }

        private void Tester_Shown(object sender, EventArgs e)
        {
            try
            {
                if (!Settings.Default.Position.IsEmpty)
                {
                    this.SetDesktopLocation(Settings.Default.Position.Left, Settings.Default.Position.Top);
                }
            }
            catch (Exception) { }
        }

        private void fillCombo(IList combo)
        {
            m_tests = new Dictionary<string, MethodInfo>();
            Assembly a = Assembly.GetExecutingAssembly();
            List<string> data = new List<string>();
            foreach (Type t in a.GetTypes())
            {
                foreach (MethodInfo method in t.GetMethods())
                {
                    if (!method.IsStatic) continue;
                    bool found = false;
                    foreach (Attribute attr in Attribute.GetCustomAttributes(method))
                    {
                        if (attr is TestEntry) found = true;
                    }
                    if (!found) continue;
                    data.Add(method.Name);
                    m_tests.Add(method.Name, method);
                }
            }
            data.Sort();
            data.ForEach(d => combo.Add(d));
        }

        private void FillConnections()
        {
            foreach (var connection in Settings.Default.Connections)
            {
                cboConnection.Items.Add(connection);

            }
            if (Settings.Default.LastConnection > cboConnection.Items.Count - 1)
                cboConnection.SelectedIndex = 0;
            else
                cboConnection.SelectedIndex = Settings.Default.LastConnection;
        }

        private void Tests_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2)
            {
                var row = this.Tests.Rows[e.RowIndex];
                var function = row.Cells[0].Value;
                if (function != null)
                {
                    MethodInfo test = null;
                    if (m_tests.TryGetValue(row.Cells[0].Value.ToString(), out test))
                    {
                        var cmdline = row.Cells[1].Value;
                        saveTests();
                        System.Console.WriteLine("Invoking {0}.{1}(\"{2}\")", test.ReflectedType.FullName, test.Name, cmdline);
                        try
                        {
                            if (test.GetParameters().Length == 3) // SqlConnection, param, object
                            {
                                var url = this.SelectedUrl();
                                new Thread((ThreadStart)delegate
                                {
                                    try
                                    {
                                        System.Console.WriteLine(DateTime.Now + " Start: " + test.Name);
                                        //using (var site = new NpgsqlConnection(url))
                                        {
                                            test.Invoke(null, new object[] { null, cmdline, null });
                                        }
                                        System.Console.WriteLine(DateTime.Now + " Finish: " + test.Name);
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Console.WriteLine("Crash");
                                        System.Console.WriteLine(ex.InnerException);
                                    }
                                })
                                { IsBackground = true }.Start();
                            }
                            else if (test.GetParameters().Length == 2)
                            {
                                if (test.GetParameters()[0].ParameterType.Name == "DbConnection") // SqlConnection, param) 
                                {
                                    //using (var site = new NpgsqlConnection(this.SelectedUrl()))
                                    {
                                        test.Invoke(null, new object[] { null, cmdline });
                                    }
                                }
                                else //param, object
                                {
                                    new Thread((ThreadStart)delegate
                                    {
                                        try
                                        {
                                            System.Console.WriteLine(DateTime.Now + " Start: " + test.Name);
                                            test.Invoke(null, new object[] { cmdline, null });
                                            System.Console.WriteLine(DateTime.Now + " Finish: " + test.Name);
                                        }
                                        catch (Exception ex)
                                        {
                                            System.Console.WriteLine("Crash");
                                            System.Console.WriteLine(ex.InnerException);
                                        }
                                    })
                                    { IsBackground = true }.Start();
                                }
                            }
                            else // param
                            {
                                test.Invoke(null, new object[] { cmdline });
                            }
                            System.Console.WriteLine("OK");
                        }
                        catch (Exception ex)
                        {
                            System.Console.WriteLine("Crash");
                            System.Console.WriteLine(ex.InnerException);
                        }
                    }
                }
            }
        }

        private void Tester_FormClosing(object sender, FormClosingEventArgs e)
        {
            saveTests();
        }

        private void saveTests()
        {
            var s1 = Settings.Default.Functions = new System.Collections.Specialized.StringCollection();
            var s2 = Settings.Default.Parameters = new System.Collections.Specialized.StringCollection();

            foreach (DataGridViewRow row in Tests.Rows)
            {
                if (row.Cells[0].Value != null)
                {
                    s1.Add((string)row.Cells[0].Value);
                    s2.Add((string)row.Cells[1].Value);
                }
            }
            Settings.Default.Position = DesktopBounds;
            Settings.Default.Split = split.SplitterDistance;
            Settings.Default.LastConnection = cboConnection.SelectedIndex;
            Settings.Default.Save();
        }


        private string SelectedUrl()
        {
            int index = this.cboConnection.SelectedIndex;
            return Settings.Default.Connections[index];
        }


        /*public static void InjectConnection(SqlConnection conn, string name)
        {
            var connectionString = conn.ConnectionString;

            try
            {
                typeof(ConfigurationElementCollection)
                    .GetField("bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(ConfigurationManager.ConnectionStrings, false);
                ConfigurationManager.ConnectionStrings.Remove(name);
            }
            catch { }

            typeof(ConfigurationElementCollection)
                .GetField("bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(ConfigurationManager.ConnectionStrings, false);
            ConfigurationManager.ConnectionStrings.Add(new ConnectionStringSettings(name, connectionString, "System.Data.SqlClient"));
        }

        public static void InjectSection(string section, string property, string value)
        {
            var collection = (NameValueCollection)ConfigurationManager.GetSection(section);
            var field = collection.GetType().GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
            var old = field.GetValue(collection);
            field.SetValue(collection, false);
            collection[property] = value;
            field.SetValue(collection, old);
        }*/

        private void Tester_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.A)
            {
                this.Console.SelectAll();
            }
        }
    }

    public class TestEntry : System.Attribute { }

    public class TextBoxWriter : TextWriter
    {
        private TextBox output;
        [ThreadStatic]
        static StringBuilder line;
        public TextBoxWriter(TextBox textbox)
        {
            output = textbox;
        }

        delegate void InvokeDelegate(string txt);
        public override void Write(char value)
        {
            if (line == null) line = new StringBuilder();
            base.Write(value);
            line.Append(value);
            if (value == '\n')
            {
                this.output.BeginInvoke((InvokeDelegate)delegate (string txt) { output.AppendText(txt); }, line.ToString());
                line.Clear();
            }
        }
        public override void Close()
        {
            base.Close();
            this.output.AppendText(line.ToString());
        }
        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}
