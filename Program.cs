//
// (C) Copyright 2014 by Autodesk, Inc.
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.
//

//todo: new input: standard. check for standard in result set of nozzle query

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;
using System.IO;

// Platform
//
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Windows;

// Plant
//
using Autodesk.ProcessPower.PnP3dObjects;
using Autodesk.ProcessPower.PlantInstance;
using Autodesk.ProcessPower.ProjectManager;
using Autodesk.ProcessPower.DataLinks;
using Autodesk.ProcessPower.DataObjects;
using Autodesk.ProcessPower.PnP3dDataLinks;
using Autodesk.ProcessPower.PartsRepository;
using Autodesk.ProcessPower.PnP3dEquipment;
using Autodesk.ProcessPower.ACPUtils;


using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using PlantApp = Autodesk.ProcessPower.PlantInstance.PlantApplication;
using System.Security.Cryptography;
using System.Diagnostics.Metrics;

[assembly: Autodesk.AutoCAD.Runtime.ExtensionApplication(null)]
[assembly: Autodesk.AutoCAD.Runtime.CommandClass(typeof(EquipmentSample.Program))]

namespace EquipmentSample
{
    public class Program
    {
        // Currently selected equipment
        //
        static EquipmentType s_CurrentEquipmentType = null;
        static String s_CurrentDwgName = null;
        static double s_CurrentDwgScale = 1.0;
        static ObjectId s_CurrentEquipmentId = ObjectId.Null;
        static int numberOfNozzles = 0;



        [CommandMethod("NozzleArray")]
        public static void NozzleArray()
        {
            Editor ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;

            EquipmentLoadEntity();

            PromptStringOptions pStrOpts1 = new PromptStringOptions("\n" + "please input (sample: \"StraightNozzle,Radial,100,FL,mm,10,C,substrLongDesc,H=100,L=75,A=0,O=0,I=0,N=0,T=0\")");
            PromptStringOptions pStrOpts2 = new PromptStringOptions("\n" + "please input the number of nozzles to be created");
            PromptStringOptions pStrOpts3 = new PromptStringOptions("\n" + "please input the distance between the nozzles");

            string inputString = ed.GetString(pStrOpts1).StringResult.Trim();
            if (inputString.Length == 0) { return; }
            string tmpString = ed.GetString(pStrOpts2).StringResult.Trim();
            if (tmpString.Length == 0) { return; }
            numberOfNozzles = Int32.Parse(tmpString);
            tmpString = ed.GetString(pStrOpts3).StringResult.Trim();
            if (tmpString.Length == 0) { return; }
            double distance = Double.Parse(tmpString);

            string[] inArr = inputString.Split(new char[] { ',' });

            if (!inArr[0].Trim().Equals("StraightNozzle") || !inArr[1].Trim().Equals("Radial"))
            {
                ed.WriteMessage("\nOnly NozzleType=StraightNozzle and NozzleLocation=Radial currently supported by this script.");
                return;
            }

            for (int i = 0; i < numberOfNozzles; i++)
            {
                double addition = i * distance;
                
                EquipmentAddNozzle("StraightNozzle", "Radial",
                    Double.Parse(inArr[2].Trim()),
                    inArr[3].Trim(),
                    inArr[4].Trim(),
                    inArr[5].Trim(),
                    inArr[6].Trim(),
                    inArr[7].Trim(),
                    (Double.Parse(inArr[8].Trim().Split(new char[] { '=' })[1].Trim()) + addition).ToString(),                    
                    inArr[9].Trim().Split(new char[] { '=' })[1].Trim(),
                    inArr[10].Trim().Split(new char[] { '=' })[1].Trim(),
                    inArr[11].Trim().Split(new char[] { '=' })[1].Trim(),
                    inArr[12].Trim().Split(new char[] { '=' })[1].Trim(),
                    inArr[13].Trim().Split(new char[] { '=' })[1].Trim(),
                    inArr[14].Trim().Split(new char[] { '=' })[1].Trim(),
                    i
                    );
            }

            //attempts for a different approach, extract information from a given nozzle, problem: cannot extract full nozzleinfo, designparameters..
            /*using (Transaction tr = AcadApp.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                if (s_CurrentEquipmentId.ObjectClass.Name.Equals("AcPpDb3dEquipment"))
                {
                    Autodesk.ProcessPower.PnP3dObjects.Equipment Eqp = tr.GetObject(s_CurrentEquipmentId, OpenMode.ForRead) as Autodesk.ProcessPower.PnP3dObjects.Equipment;

                    if (Eqp != null)
                    {
                        foreach (Autodesk.ProcessPower.PnP3dObjects.NozzleSubPart sp in Eqp.AllSubParts)
                        {//NozzleSubPartCollection
                            PartSizeProperties spprops = sp.PartSizeProperties;
                            if(sourceNozzleNumber == sp.Index)
                            {
                                ed.WriteMessage("\n index: " + sp.Index);
                                //"StraightNozzle" = PartSubType_StraightNozzle, nd = Size_100, "FL", pressureClass, facing, L = CutLength_76.2
                                //Nozzle, flanged, 4" ND, RF, 300, ASME B16.5
                                //metric/mixedmetric project:
                                //sample input: "StraightNozzle,Radial,100,FL,mm,10,C,DIN 2632,H=100,L=75,A=0,O=0,I=0,N=0,T=0"
                                //sample input: "StraightNozzle,Radial,4,FL,in,300,RF,substrLongDesc,H=100,L=75,A=0,O=0,I=0,N=0,T=0"
                                //imperial project:
                                //sample input: "StraightNozzle,Radial,100,FL,mm,10,C,DIN 2632,H=4,L=4,A=0,O=0,I=0,N=0,T=0"
                                //sample input: "StraightNozzle,Radial,4,FL,in,300,RF,substrLongDesc,H=4,L=4,A=0,O=0,I=0,N=0,T=0"
                                for (int i = 0; i < spprops.PropCount; i++)
                                {
                                    try
                                    {
                                        ed.WriteMessage("\n" + spprops.PropValue("Tag") + "_" + spprops.PropNames[i] + "_" + spprops.PropValue(spprops.PropNames[i]).ToString());
                                    }
                                    catch (System.Exception) { }
                                }

                                ed.WriteMessage("\n objid: " + sp.Id);
                            }

                        }
                    }
                }
                tr.Commit();
            }*/

        }




        // Load equipment from entity
        //
        [CommandMethod("EquipmentLoadEntity")]
        public static void EquipmentLoadEntity()
        {
            try
            {
                Editor ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;

                // Select entity
                //
                ObjectId eqId;
                while (true)
                {
                    PromptEntityResult res = ed.GetEntity("\nSelect equipment entity: ");
                    if (res.Status == PromptStatus.OK)
                    {
                        // Equipment ?
                        //
                        if (res.ObjectId.ObjectClass.IsDerivedFrom(RXObject.GetClass(typeof(Equipment))))
                        {
                            // Yes
                            //
                            eqId = res.ObjectId;
                            break;
                        }
                    }
                    else
                    if (res.Status == PromptStatus.Cancel)
                    {
                        return;
                    }
                }

                // Use helper class
                //
                using (EquipmentHelper eqHelper = new EquipmentHelper())
                {
                    // Load
                    //
                    s_CurrentEquipmentType = eqHelper.RetrieveEquipmentFromInstance(eqId);
                    s_CurrentDwgName = null;
                    s_CurrentEquipmentId = eqId;
                }
            }
            catch (System.Exception)
            {
            }
        }





// Add nozzle
//
[CommandMethod("EquipmentAddNozzle")]
        public static void EquipmentAddNozzle(string nozzleType, 
    string nozzleLocation, 
    double nd_double, 
    string endcode, 
    string units, 
    string pressureClass, 
    string facing, 
    string substrLongDesc,
    string H, string L, string A, string O, string I, string N, string T,
    int callNo)
        {
            try
            {
                Editor ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;
                Database db = AcadApp.DocumentManager.MdiActiveDocument.Database;

                if (s_CurrentEquipmentType == null)
                {
                    ed.WriteMessage("\nNo current equipment type loaded");
                    return;
                }

                bool bFabricated = false;
                if (s_CurrentEquipmentType.IsImportedEquipment())
                {
                    // Need an entity to edit
                    //
                    if (s_CurrentEquipmentId.IsNull)
                    {
                        ed.WriteMessage("\nSelect equipment entity to edit imported equipment.");
                        return;
                    }
                }
                else
                if (s_CurrentEquipmentType.IsParametricEquipment())
                {
                    // Can't add nozzle to parametric
                    //
                    ed.WriteMessage("\nCan't add nozzle to parametic equipment.");
                    return;
                }
                else
                {
                    bFabricated = true;
                }

                // Use helper class
                //
                using (EquipmentHelper eqHelper = new EquipmentHelper())
                {
                    // New Nozzle
                    //
                    int newIndex = eqHelper.NewNozzleIndex(s_CurrentEquipmentType);
                    ed.WriteMessage("\nnozzindex: " + newIndex);
                    String nozName = "New Nozzle " + newIndex.ToString();
                    NozzleInfo ni = eqHelper.NewNozzle(s_CurrentEquipmentType, newIndex, nozName);

                    // Project units
                    //
                    Units nUnit = (Units)Autodesk.ProcessPower.AcPp3dObjectsUtils.ProjectUnits.CurrentNDUnit;
                    Units lUnit = (Units)Autodesk.ProcessPower.AcPp3dObjectsUtils.ProjectUnits.CurrentLinearUnit;

                    // Find nozzles
                    //
                    NominalDiameter nd = new NominalDiameter();
                    if(units.Equals("mm")) nd.EUnits = Units.Mm;//Undefined, Inch, Mm
                    else if (units.Equals("in")) nd.EUnits = Units.Inch;
                    else
                    {
                        ed.WriteMessage("\nunit input not valid. can be only mm or in, using project nd unit");
                        nd.EUnits = nUnit;
                    }
                    nd.Value = nd_double;
                    /*if (nUnit == Units.Inch)
                    {
                        // Something hardcoded imperial
                        //
                        nd.Value = 4;
                        pressureClass = "300";
                        facing = "RF";
                    }
                    else
                    {
                        // Metric
                        //
                        nd.Value = 100;
                        pressureClass = "10";
                        facing = "C";
                    }*/

                    // For example, straight, flanged
                    //
                    PnPRow[] rows = NozzleInfo.SelectFromNozzleCatalog(eqHelper.NozzleRepository, "StraightNozzle", nd, endcode, pressureClass, facing);
                    if (rows.Length == 0)
                    {
                        ed.WriteMessage("\nNo nozzles found in the catalog.");
                        return;
                    }

                    // Take the first
                    // Its guid
                    //
                    String guid = String.Empty;
                    if (substrLongDesc.Equals("substrLongDesc") || substrLongDesc.Length == 0)
                    {
                        guid = rows[0][PartsRepository.PartGuid].ToString();
                    }
                    else
                    { //or take the one specified by substrLongDesc
                        foreach (var row in rows) {
                            if (row[PartsRepository.LongDescription].ToString().Contains(substrLongDesc)) { 
                                guid = row[PartsRepository.PartGuid].ToString();
                                break;
                            }
                        }
                    }

                    if (guid.Equals(String.Empty))
                    {
                        ed.WriteMessage("\nNo nozzles found in the catalog.");
                        return;
                    }
                    // Assign nozzle part
                    //
                    eqHelper.SetNozzlePart(s_CurrentEquipmentType, ni, guid);

                    if (bFabricated)
                    {
                        // TODO: we may ask for all these params

                        // For example, radial
                        //
                        eqHelper.SetNozzleLocation(s_CurrentEquipmentType, ni, (int)NozzleLocation.eRadial);

                        // Set some nozzle length: 1' / 300mm
                        //
                        ParameterInfo pa = ni.Parameters["L"];
                        if (pa != null)
                        {
                            if (lUnit == Units.Inch) pa.Value = L;
                            else pa.Value = L;
                        }

                        // And some height: 6"/150mm
                        //
                        pa = ni.Parameters["H"];
                        if (pa != null)
                        {
                            if (lUnit == Units.Inch) pa.Value = H;
                            else pa.Value = H;
                        }

                        pa = ni.Parameters["A"];
                        if (pa != null)
                        {
                            if (lUnit == Units.Inch) pa.Value = A;
                            else pa.Value = A;
                        }

                        pa = ni.Parameters["O"];
                        if (pa != null)
                        {
                            if (lUnit == Units.Inch) pa.Value = O;
                            else pa.Value = O;
                        }

                        pa = ni.Parameters["I"];
                        if (pa != null)
                        {
                            if (lUnit == Units.Inch) pa.Value = I;
                            else pa.Value = I;
                        }

                        pa = ni.Parameters["N"];
                        if (pa != null)
                        {
                            if (lUnit == Units.Inch) pa.Value = N;
                            else pa.Value = N;
                        }

                        pa = ni.Parameters["T"];
                        if (pa != null)
                        {
                            if (lUnit == Units.Inch) pa.Value = T;
                            else pa.Value = T;
                        }
                    }

                    // Add new nozzle
                    //
                    s_CurrentEquipmentType.Nozzles.Add(ni);

                    // Update entity
                    //
                    if (!s_CurrentEquipmentId.IsNull && callNo == numberOfNozzles-1)
                    {
                        eqHelper.UpdateEquipmentEntity(s_CurrentEquipmentId, s_CurrentEquipmentType, s_CurrentDwgName, s_CurrentDwgScale);
                    }
                }
            }
            catch (System.Exception ex)
            {
                string errorstack = "";
                System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(ex, true);
                errorstack += trace.ToString() + "\n";
                errorstack += "Line: " + trace.GetFrame(0).GetFileLineNumber() + "\n";
                errorstack += "message: " + ex.Message + "\n";
                AcadApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage(errorstack);
            }
        }

        // Dragger classes
        //
        public class EqDragPoint : EntityJig
        {
            Point3d m_CurrentPos;
            JigPromptPointOptions m_Opts;

            public EqDragPoint(Equipment eqEnt) : base(eqEnt)
            {
                m_CurrentPos = eqEnt.Position;

                m_Opts = new JigPromptPointOptions("\nSelect point");
                m_Opts.UserInputControls = (UserInputControls.Accept3dCoordinates | UserInputControls.NoZeroResponseAccepted | UserInputControls.NoNegativeResponseAccepted);
            }

            protected override SamplerStatus Sampler(JigPrompts prompts)
            {
                // Ask for a point
                //
                PromptPointResult res = prompts.AcquirePoint(m_Opts);
                switch (res.Status)
                {
                    case PromptStatus.OK:
                        if (m_CurrentPos == res.Value)
                        {
                            // The same point. Don't redraw
                            //
                            return SamplerStatus.NoChange;
                        }
                        else
                        {
                            // Take new point
                            //
                            m_CurrentPos = res.Value;
                            return SamplerStatus.OK;
                        }

                    default:
                        return SamplerStatus.Cancel;
                }
            }

            protected override bool Update()
            {
                // Set new point
                //
                ((Equipment)Entity).Position = m_CurrentPos;
                return true;
            }
        }

        public class EqDragAngle : EntityJig
        {
            Double m_CurrentAngle;
            Double m_DefaultAngle;
            Point3d m_Position;
            Vector3d m_ZAxis;
            Matrix3d m_Trans;
            JigPromptAngleOptions m_Opts;

            public EqDragAngle(Equipment eqEnt) : base(eqEnt)
            {
                m_CurrentAngle = m_DefaultAngle = eqEnt.Rotation;
                m_Position = eqEnt.Position;
                m_ZAxis = eqEnt.ZAxis;
                m_Trans = Matrix3d.Identity;

                m_Opts = new JigPromptAngleOptions("\nSelect rotation angle");
                m_Opts.UserInputControls = UserInputControls.NullResponseAccepted;
                m_Opts.Cursor |= CursorType.RubberBand;
                m_Opts.BasePoint = eqEnt.Position;
                m_Opts.UseBasePoint = true;
                m_Opts.DefaultValue = m_DefaultAngle;
            }

            protected override SamplerStatus Sampler(JigPrompts prompts)
            {
                // Ask for an angle
                //
                PromptDoubleResult res = prompts.AcquireAngle(m_Opts);
                switch (res.Status)
                {
                    case PromptStatus.OK:
                        if (m_CurrentAngle == res.Value)
                        {
                            // The same angle. Don't redraw
                            //
                            return SamplerStatus.NoChange;
                        }
                        else
                        {
                            // Rotate around equipment Z on the angle difference
                            //
                            m_Trans = Matrix3d.Rotation(res.Value - m_CurrentAngle, m_ZAxis, m_Position);
                            m_CurrentAngle = res.Value;
                        }
                        break;

                    default:
                        // Restore default
                        //
                        m_Trans = Matrix3d.Rotation(m_DefaultAngle - m_CurrentAngle, m_ZAxis, m_Position);
                        m_CurrentAngle = m_DefaultAngle;
                        break;
                }

                // Transform here since Update isn't called on None or Cancel
                // And return NoChange to avoid second transform
                //
                Entity.TransformBy(m_Trans);
                return SamplerStatus.NoChange;
            }

            protected override bool Update()
            {
                // We transform in sampler()
                //
                return true;
            }
        }
    }
}
