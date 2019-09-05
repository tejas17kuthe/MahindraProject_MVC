﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MMi_BIS_PA.Models;
using CoreScanner;
using System.Xml;

namespace MMi_BIS_PA.Controllers
{
    public class CurrentDataPageController : Controller
    {
        static CCoreScannerClass ccs;
        static string barcodeData;
        LoginData id;
        static ProcessStartInfo myProcessStartInfo;
        PiechartData p;
        static Barcode b;
        int i;
        static string barcode = "";
        static int barcodeEventCallCount;

        bool initializeFlag;

        public CurrentDataPageController()
        {

            string python = @"C:\ProgramData\Anaconda3\python.exe";
            myProcessStartInfo = new ProcessStartInfo(python);
            p = new PiechartData();
            b = new Barcode();
            i = 0;
          
        }

        [Route("CurrentDataPage/CurrentDataPage")]
        [HttpGet]
        public ActionResult CurrentDataPage(LoginData id)
        {
            InitializeBarcodeReader();
            barcodeEventCallCount = 0;
            initializeFlag = true;
            barcode = "initial QR Code";
            UpdatePieChart();
            this.id = id;
            barcodeData = "Please Scan the QR Code";
            b.Data = barcodeData;
            //b.Data = "No Barcode Scanned";
            //ViewBag.Barcode = b;
            //ViewBag.userName = this.id.userName;
            //ViewBag.password = this.id.password;
            return View();
        }

        [HttpPost]
        public ActionResult CurrentDataPage(string qr)
        {
            InitializeBarcodeReader();
            try
            {
                return View();
            }
            catch (Exception e)
            {
                return Content("There is some problem with the driver please check connection");
            }
        }

        [Route("CurrentDataPage/Delete")]
        public ActionResult Delete()
        {

            new MySqlDatabaseInteraction().RemoveTableData();
            return RedirectToAction("CurrentDataPage", "CurrentDataPage");

        }


        public void AddDataIntoCurrentTable()
        {
            MySqlDatabaseInteraction sql = new MySqlDatabaseInteraction();
            List<TableData> i = sql.GetTableData();
            ViewBag.Barcode = ++this.i;
            if (i.Count == 4)
            {
                #region Creating CurrentData object
                currentdata c = new currentdata();

                //row 1 data collection
                c.qr1 = i[0].qrcode;
                c.c11 = i[0].c1;
                c.c12 = i[0].c2;
                c.r1 = i[0].r;
                c.w1 = i[0].w;

                //row 2 data collection
                c.qr2 = i[1].qrcode;
                c.c21 = i[1].c1;
                c.c22 = i[1].c2;
                c.r2 = i[1].r;
                c.w2 = i[1].w;

                //row 3 data collection
                c.qr3 = i[2].qrcode;
                c.c31 = i[2].c1;
                c.c32 = i[2].c2;
                c.r3 = i[2].r;
                c.w3 = i[2].w;

                //row 4 data collection
                c.qr4 = i[3].qrcode;
                c.c41 = i[3].c1;
                c.c42 = i[3].c2;
                c.r4 = i[3].r;
                c.w4 = i[3].w;

                c.wd = i[3].wd;

                if (c.c11 != 0 && c.c12 != 0 && c.r1 != 0 && c.c21 != 0 && c.c22 != 0 && c.r2 != 0 && c.c31 != 0 && c.c32 != 0 && c.r3 != 0 && c.c41 != 0 && c.c42 != 0 && c.r4 != 0 && c.wd < i[3].set_point)
                {
                    c.status = 1;
                }
                else
                {
                    c.status = 0;
                }

                var shift = new MySqlDatabaseInteraction().getshiftid(DateTime.Now.ToString("HH:mm:ss")).ToList();

                c.shiftid = shift[0].shift_id;
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                string time = DateTime.Now.ToString("HH:mm:ss");
                string dateT = date + " " + time;
                c.date_time = DateTime.Parse(dateT);
                #endregion

                #region currentShiftData
                currentshiftdata c2 = new currentshiftdata();

                //row 1 data collection
                c2.qr1 = i[0].qrcode;
                c2.c11 = i[0].c1;
                c2.c12 = i[0].c2;
                c2.r1 = i[0].r;
                c2.w1 = i[0].w;

                //row 2 data collection
                c2.qr2 = i[1].qrcode;
                c2.c21 = i[1].c1;
                c2.c22 = i[1].c2;
                c2.r2 = i[1].r;
                c2.w2 = i[1].w;

                //row 3 data collection
                c2.qr3 = i[2].qrcode;
                c2.c31 = i[2].c1;
                c2.c32 = i[2].c2;
                c2.r3 = i[2].r;
                c2.w3 = i[2].w;

                //row 4 data collection
                c2.qr4 = i[3].qrcode;
                c2.c41 = i[3].c1;
                c2.c42 = i[3].c2;
                c2.r4 = i[3].r;
                c2.w4 = i[3].w;

                c2.wd = i[3].wd;

                if (c2.c11 != 0 && c2.c12 != 0 && c2.r1 != 0 && c2.c21 != 0 && c2.c22 != 0 && c2.r2 != 0 && c2.c31 != 0 && c2.c32 != 0 && c2.r3 != 0 && c2.c41 != 0 && c2.c42 != 0 && c2.r4 != 0 && c2.wd < i[3].set_point)
                {
                    c2.status = 1;
                }
                else
                {
                    c2.status = 0;
                }

                var currentShift = new MySqlDatabaseInteraction().getshiftid(DateTime.Now.ToString("HH:mm:ss")).ToList();

                c2.shiftid = shift[0].shift_id;
                c2.date_time = DateTime.Parse(dateT);
                #endregion


                if (barcodeEventCallCount == 1)
                {
                    new MySqlDatabaseInteraction().AddCurrentData(c);
                    new MySqlDatabaseInteraction().AddCurrentShiftData(c2,shift[0].shift_id);
                    new MySqlDatabaseInteraction().RemoveTableData();
                }
            }

        }

        public PartialViewResult UpdateBarcode()
        {
            b.Data = barcodeData;
            ViewBag.Barcode = b;
            return PartialView("_SearchBar");
        }

        public PartialViewResult UpdateTable()
        {
            MySqlDatabaseInteraction sql = new MySqlDatabaseInteraction();
            List<TableData> i = sql.GetTableData();
            UpdatePieChart();
            return PartialView("_DataCardContent", i);
        }



        public void UpdatePieChart()
        {
            List<currentdata> d = new MySqlDatabaseInteraction().UpdatePieChart();
            int c1 = 0;
            int c2 = 0;
            int r = 0;
            int w = 0;
            foreach (var data in d)
            {
                if (data.c11 == 0 || data.c21 == 0 || data.c31 == 0 || data.c41 == 0)
                {
                    c1 += 1;
                }




                if (data.c12 == 0 || data.c22 == 0 || data.c32 == 0 || data.c42 == 0)
                {
                    c2 += 1;
                }



                if (data.r1 == 0 || data.r2 == 0 || data.r3 == 0 || data.r4 == 0)
                {
                    r += 1;
                }

                if (data.wd > new MySqlDatabaseInteraction().GetWeightDifferenceSetPoint())
                {
                    w += 1;
                }
            }


            p.clip1 = c1;
            p.clip2 = c2;
            p.ring = r;
            p.weight = w;
            p.TotalSuccessfulCycles = new MySqlDatabaseInteraction().SuccessfulCycleCount();
            p.Barcode = barcodeData;


            ViewBag.pieData = p;

        }


        private void InitializeBarcodeReader()
        {

            //Instantiate CoreScanner Class
            ccs = new CCoreScannerClass();
            //Call Open API
            short[] scannerTypes = new short[1];//Scanner Types you are interested in
            scannerTypes[0] = 1; // 1 for all scanner types
            short numberOfScannerTypes = 1; // Size of the scannerTypes array
            int status; // Extended API return code
            ccs.Open(0, scannerTypes, numberOfScannerTypes, out status);
            // Subscribe for barcode events in cCoreScannerClass
            ccs.BarcodeEvent += new _ICoreScannerEvents_BarcodeEventEventHandler(OnBarcodeEvent);
            // Let's subscribe for events
            int opcode = 1001; // Method for Subscribe events
            string outXML; // XML Output
            string inXML = "<inArgs>" +
             "<cmdArgs>" +
             "<arg-int>1</arg-int>" + // Number of events you want to subscribe
             "<arg-int>1</arg-int>" + // Comma separated event IDs
             "</cmdArgs>" +
             "</inArgs>";
            ccs.ExecCommand(opcode, ref inXML, out outXML, out status);
            string text = "initialized " + status.ToString();
            //using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"E:\logs\mahindra.txt"))
            //{
            //    file.WriteLine(text);
            //}

            //System.IO.File.WriteAllText(@"E:\logs\mahindra.txt", text);



        }

        public void OnBarcodeEvent(short eventType, ref string pscanData)
        {

            // ++barcodeEventCallCount;
            string hashcode = "";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(pscanData);

            XmlElement root = doc.DocumentElement;
            XmlNodeList elemList = root.GetElementsByTagName("datalabel");
            if (elemList.Count > 0)
            {
                for (int i = 0; i < elemList.Count; i++)
                {
                    hashcode = elemList[i].InnerXml;
                }
            }
            string temp = getbarcode(hashcode);
            bool countFlag = temp.Equals(barcode);
            if (!countFlag)
            {
                if (barcodeEventCallCount == 0)
                {
                    barcodeEventCallCount = 1;
                    barcodeData = temp;
                    barcode = barcodeData;
                    AddDataIntoCurrentTable();
                    CallPythonDriver(barcodeData);
                }
                else
                {
                    barcodeData = "Driver is running Please wait";
                }
            }
            else
            {
                barcodeEventCallCount = 0;
               // barcodeData = "Same QR Code Scanned";
            }
            //if (barcodeEventCallCount == 1)
            //{

            //    //b.Data = barcode;
            //    //ViewBag.Barcode = b;
            //    string textbar = "Barcode " + barcodeData;
            //    bool flag = barcodeData.Equals(barcode);
            //    if (!flag)
            //    {

            //    }

            //}

            //this.Invoke((MethodInvoker)delegate { txtBarcode.Text = barcode; });
        }

        string getbarcode(string b)
        {
            try
            {
                string hex = string.Empty;
                string ascii = string.Empty;

                for (int i = 0; i < b.Length; i += 4)
                {
                    String hs = string.Empty;

                    hs = b.Substring(i, 4);

                    uint decval = System.Convert.ToUInt32(hs, 16);
                    char character = System.Convert.ToChar(decval);
                    ascii += character;
                    i++;
                }

                return ascii;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return string.Empty;
        }







        private void CallPythonDriver(string qr)
        {
            if (barcodeEventCallCount == 1)
            {
                try
                {
                    string fname1 = qr;

                    // python app to call 
                    string myPythonApp = "C:\\ProgramData\\Anaconda3\\driver.py";

                    // dummy parameters to send Python script 
                    string x = @fname1;



                    myProcessStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    myProcessStartInfo.CreateNoWindow = true;


                    // make sure we can read the output from stdout 
                    myProcessStartInfo.UseShellExecute = false;
                    myProcessStartInfo.RedirectStandardOutput = true;

                    // start python app with 3 arguments  
                    // 1st arguments is pointer to itself,  
                    // 2nd and 3rd are actual arguments we want to send 
                    myProcessStartInfo.Arguments = myPythonApp + " " + x;

                    Process myProcess = new Process();
                    // Process[] pname = Process.GetProcessesByName("notepad");

                    //if (pname.Length < 2)

                    //{
                    // assign start information to the process 
                    myProcess.StartInfo = myProcessStartInfo;

                    // Console.WriteLine("Calling Python script with arguments {0} ", x);
                    // start the process

                    myProcess.Start();
                    myProcess.WaitForExit();
                    myProcess.Close();
                    barcodeEventCallCount = 0;
                    barcodeData = "Please Scan the QR Code";
                    // }

                }
                catch (Exception e)
                {
                    // return Content("There is some problem with the driver please check connection");
                }
            }
        }
    }
}

