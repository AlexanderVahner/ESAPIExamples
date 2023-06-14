using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using ScriptArgsNameSpace;

// TODO: Replace the following version attributes by creating AssemblyInfo.cs. You can do this in the properties of the Visual Studio project.
[assembly: AssemblyVersion("1.0.0.1")]
[assembly: AssemblyFileVersion("1.0.0.1")]
[assembly: AssemblyInformationalVersion("1.0")]

// TODO: Uncomment the following line if the script requires write access.
// [assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
  public class Script
  {
    public Script()
    {
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext context/*, System.Windows.Window window, ScriptEnvironment environment*/)
        {
            Run(new ScriptArgs()
            {
                Patient = context.Patient
            });
        }

        public void Run(ScriptArgs args)
        {
            if (args.Patient == null)
            {
                MessageBox.Show("Open a patient!");
                return;
            }

            //MessageBox.Show(args.Patient.FirstName);

            args.Patient.BeginModifications();

            // Finding a StructureSet
            var structureSet = OpenStructureSet(args.Patient, "<Scan Volume>1");

            //Finding structures
            var structurePTVp = structureSet.Structures.Where(s => s.Id == "PTVp").FirstOrDefault();
            var structurePTVn = structureSet.Structures.Where(s => s.Id == "PTVn").FirstOrDefault();
            var structureBody = structureSet.Structures.Where(s => s.DicomType.ToUpper() == "EXTERNAL").FirstOrDefault();

            if (structurePTVp == null || structurePTVn == null || structureBody == null)
            {
                MessageBox.Show($@"Not all structures exist.");
            }

            // Adding PTVeval
            var structurePTVEval = AddStructure(structureSet, "PTVeval");
            /*
             * Operations with structures:
             * And, Not, Or, Sub, Xor, Margin
             * 
             * this is enough for simple preprocessing
             * 
             * For example, let's do the following:
             * PTVeval = (PTVp | PTVn) & (Body - 3mm)
             */
            
            if (structurePTVEval.CanEditSegmentVolume(out string editSegmentVolumeError))
            {
                structurePTVEval.SegmentVolume = 
                    structurePTVp.SegmentVolume.Or(structurePTVn.SegmentVolume) //(PTVp | PTVn)
                    .And(structureBody.SegmentVolume.Margin(-3));// & (Body - 3mm)
            }
            else
            {
                MessageBox.Show(editSegmentVolumeError);
            }


            // Adding a course
            var course = AddNewCourse(args.Patient);

            // Adding a plan
            var plan = AddPlan(course, structureSet);

            // 30 fractions, dose per fraction 2Gy
            plan.SetPrescription(30, new DoseValue(2, DoseValue.DoseUnit.Gy), 1);

            //If you want to save changes, uncomment
            // app.SaveModifications();
            // in Plugin tester project / MainWindow.xaml.cs / Window_Closing method

        }

        public StructureSet OpenStructureSet(Patient patient, string ssId)
        {
            StructureSet result = patient.StructureSets.Where(ss => ss.Id.ToLower() == ssId.ToLower()).FirstOrDefault();
            if (result == null)
            {
                MessageBox.Show($@"StructureSet ""{ssId}"" doesn't exist.");
            }

            return result;
        }

        public Structure AddStructure(StructureSet ss, string id)
        {
            Structure result = null;
            if (ss.CanAddStructure("CONTROL", id))
            {
                result = ss.AddStructure("CONTROL", id);
            }
            else
            {
                MessageBox.Show($@"Cannot add ""{id}"" structure.");
            }
            return result;
        }

        public Course AddNewCourse(Patient patient)
        {
            Course result = null;
            if (patient.CanAddCourse())
            {
                result = patient.AddCourse();
            }
            else
            {
                MessageBox.Show("Can't add a course");
            }
            return result;
        }

        public ExternalPlanSetup AddPlan(Course course, StructureSet structureSet)
        {
            ExternalPlanSetup result = null;
            if (course.CanAddPlanSetup(structureSet))
            {
                result = course.AddExternalPlanSetup(structureSet);
            }
            else
            {
                MessageBox.Show("Can't add a plan");
            }
            return result;
        }
    }
}
