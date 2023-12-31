﻿using ScriptArgsNameSpace;
using System;
using System.Linq;
using System.Windows;
using VMS.TPS;
using VMS.TPS.Common.Model.API;
using ESAPI = VMS.TPS.Common.Model.API;

namespace PluginTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ESAPI.Application app;
        public MainWindow()
        {
            InitializeComponent();

            try
            {
                app = ESAPI.Application.CreateApplication();
                //------------------------------------------------------------------------------------
                // Write your Test Patient Id here
                Patient patient = app.OpenPatientById("EC-046");
                //Course course = patient?.Courses.FirstOrDefault(c => c.Id == "CV");
                //ExternalPlanSetup plan = course?.ExternalPlanSetups.FirstOrDefault(p => p.Id == "Fields");

                Script script = new Script();
                script.Run(new ScriptArgs()
                {
                    Patient = patient
                });

                //------------------------------------------------------------------------------------

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //app.SaveModifications();
            app?.Dispose();
        }
    }
}
