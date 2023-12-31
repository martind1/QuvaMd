﻿using Quva.Services.Devices.Card;
using Quva.Services.Devices.Display;
using Quva.Services.Devices.Modbus;
using Quva.Services.Devices.Scale;
using Quva.Services.Devices.Sps;
using Quva.Services.Interfaces.Shared;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace QuvaMd.Services.Tests;

internal class TestDeviceService
{
    private readonly IDeviceService _deviceService;
    private readonly ILogger _log;

    public TestDeviceService(IDeviceService deviceService)
    {
        _log = Log.ForContext(GetType());
        _deviceService = deviceService;
    }

    private string _oldDisplay = string.Empty;
    private ScaleStatus _oldScaleStatus;
    private long _oldSecond;

    public void Menu()
    {
        while (true)
        {
            Console.WriteLine("### TestDeviceService ###");
            Console.WriteLine("1 = Test Positionierung");
            Console.WriteLine("2 = Test Modbus");
            Console.WriteLine("3 = Callback Scale async Test");
            Console.WriteLine("4 = Callback Display Test");
            Console.WriteLine("5 = Camera Test");
            Console.WriteLine("6 = Test TST.TRANSP1");
            Console.WriteLine("7 = Test TST.BARCODE1");
            Console.WriteLine("8 = Test TST.SPS_LKW");
            Console.WriteLine("sonst = Ende");
            ConsoleKeyInfo key = Console.ReadKey(); //warten auf Taste
            Console.WriteLine("");
            if (key.KeyChar == '1')
            {
                TestPosition().GetAwaiter().GetResult();
            }
            else if (key.KeyChar == '2')
            {
                TestModbus().GetAwaiter().GetResult();
            }
            else if (key.KeyChar == '3')
            {
                TestScaleStatus().GetAwaiter().GetResult();
            }
            else if (key.KeyChar == '4')
            {
                TestDisplayShowScale().GetAwaiter().GetResult();
            }
            else if (key.KeyChar == '5')
            {
                TestCam().GetAwaiter().GetResult();
            }
            else if (key.KeyChar == '6')
            {
                TestTransp1().GetAwaiter().GetResult();
            }
            else if (key.KeyChar == '7')
            {
                TestBarcode1().GetAwaiter().GetResult();
            }
            else if (key.KeyChar == '8')
            {
                TestSps().GetAwaiter().GetResult();
            }
            else
            {
                break;
            }
        }
    }

    public async Task TestModbus()
    {
        _log.Information("testsvc.TestModbus");

        //var T1 = _deviceService.ModbusWrite("TST.WAGO", "BulbEntryGreen", "1");
        //var T2 = _deviceService.ModbusWrite("TST.WAGO", "BulbExitGreen", "1");
        //var T3 = _deviceService.ModbusWrite("TST.WAGO", "BulbExitRed", "1");
        //await Task.WhenAll(T1, T2, T3);
        //var data1 = await T1;
        //var data2 = await T2;
        //var data3 = await T3;
        //_log.Information($"ModbusWrite Err1:{data1.ErrorNr} {data1.ErrorText} Err2:{data2.ErrorNr} {data2.ErrorText} Err3:{data3.ErrorNr} {data3.ErrorText}");


        //var T1 = _deviceService.ModbusCommand("TST.WAGO", ModbusCommands.ReadBlock.ToString(), "Inputs", "1");
        //var data1 = await T1;
        //_log.Information($"ModbusWrite Err1:{data1.ErrorNr} {data1.ErrorText}");
        //string v1 = _deviceService.GetModbusValue("TST.WAGO", "EntryIsOccupied");
        //string v2 = _deviceService.GetModbusValue("TST.WAGO", "ExitIsOccupied");
        //_log.Information($"ModbusRead V1.3:{v1} V2.4:{v2}");

        var T1 = _deviceService.ModbusReadStart("TST.WAGO", MyModbusRead);
        var T2 = _deviceService.ModbusReadStart("TST.WAGO", MyModbusRead);  //test doppelter Aufruf
        var data1 = await T1;
        var data2 = await T2;
        _log.Error($"ModbusReadStart1:{data1}");
        _log.Error($"ModbusReadStart2:{data2}");
    }

    private void MyModbusRead(ModbusData Data)
    {
        if (Data.changedBlocks.Contains("Inputs"))
        {
            Data.changedBlocks.Remove("Inputs");
            string v1 = _deviceService.GetModbusValue("TST.WAGO", "EntryIsOccupied");
            string v2 = _deviceService.GetModbusValue("TST.WAGO", "ExitIsOccupied");
            _log.Error($"### MyModbus 3:{v1} 4:{v2}");
        }
    }

    public async Task TestPosition()
    {
        _log.Information("Test IT9000 + Modbus Wago Positioning");
        var T1 = await _deviceService.ScaleStatusStart("TST.FW1", MyScalePosition);

    }
    private void MyScalePosition(ScaleData scaleData)
    {
        if (_oldDisplay != scaleData.Display || _oldScaleStatus != scaleData.Status)
        {
            _log.Error($"### Scale {scaleData.Display} * Status {scaleData.Status:F} ###");
            _oldDisplay = scaleData.Display;
            _oldScaleStatus = scaleData.Status;
        }
        if (_oldSecond != DateTime.Now.ToFileTime() / 100000000)
        {
            _oldSecond = DateTime.Now.ToFileTime() / 100000000;
            var T2 = _deviceService.ScaleRegister("TST.FW1");
            ScaleData data = T2.GetAwaiter().GetResult();
            _log.Information($"### Register:{data.ErrorNr} {data.Display} C:{data.CalibrationNumber} S:{data.Status:F}");
        }
    }


    public async Task TestCam()
    {
        _log.Information("testsvc.TestCam");

        //var data1 = await _deviceService.ScaleStatus("TST.FW1");
        //_log.Information($"Status Err:{data1.ErrorNr} Display:{data1.Display} Weight:{data1.Weight} Unit:{data1.Unit}");

        //var data2 = await _deviceService.ScaleStatus("TST.FW2");
        //_log.Information($"Status Err:{data2.ErrorNr} Display:{data2.Display} Weight:{data2.Weight} Unit:{data2.Unit}");

        //var data12 = await _deviceService.ScaleRegister("TST.FW1");
        //_log.Information($"Register Err:{data12.ErrorNr} Display:{data12.Display} Eichnr:{data12.CalibrationNumber} Weight:{data12.Weight} Unit:{data12.Unit}");


        //var message = DateTime.Now.ToString("G", CultureInfo.GetCultureInfo("de-DE"));
        ////var data4 = await _deviceService.DisplayShow("TST.DISP1", message);
        //var data4 = await _deviceService.DisplayShow("TST.DISP2", message);
        //_log.Information($"Display Err:{data4.ErrorNr} {data4.ErrorText} Msg:{data4.Message}");

        //var data5 = await _deviceService.CamLoad("TST.CAMS", 0);

        var T1 = _deviceService.CamLoad("TST.CAMS", 0);
        var T2 = _deviceService.CamLoad("TST.CAMS", 1);
        var T3 = _deviceService.CamLoad("TST.CAMS", 2);
        var T4 = _deviceService.CamLoad("TST.CAMS", 3);  //nicht vorhanden
        await Task.WhenAll(T1, T2, T3, T4);
        var data1 = await T1;
        var data2 = await T2;
        var data3 = await T3;
        //var data4 = await T4;
        _log.Information($"CamLoad0 Err:{data1.ErrorNr} {data1.ErrorText} Size:{data1.ImageSize}");
        //ArgumentNullException.ThrowIfNull(data1.ImageBytes);
        if (data1.ImageBytes != null)
            File.WriteAllBytes(@$"C:\Temp\angular\logs\CAM0.{data1.ImageFormat}", data1.ImageBytes);

        _log.Information($"CamLoad1 Err:{data2.ErrorNr} {data2.ErrorText} Size:{data2.ImageSize}");
        //ArgumentNullException.ThrowIfNull(data2.ImageBytes);
        if (data2.ImageBytes != null)
            File.WriteAllBytes(@$"C:\Temp\angular\logs\CAM1.{data2.ImageFormat}", data2.ImageBytes);

        _log.Information($"CamLoad2 Err:{data3.ErrorNr} {data3.ErrorText} Size:{data3.ImageSize}");
        //ArgumentNullException.ThrowIfNull(data3.ImageBytes);
        if (data3.ImageBytes != null)
            File.WriteAllBytes($@"C:\Temp\angular\logs\CAM2.{data3.ImageFormat}", data3.ImageBytes);

        //_log.Information($"CamLoad3 Err:{data4.ErrorNr} {data4.ErrorText} Size:{data4.ImageSize}");
        //ArgumentNullException.ThrowIfNull(data4.ImageBytes);
        //File.WriteAllBytes(@$"C:\Temp\angular\logs\CAM3.{data4.ImageFormat}", data4.ImageBytes); 

    }

    /// <summary>
    ///     Dauerschleife: Status, wenn 10++t dann Register
    /// </summary>
    public void Test2()
    {
        _log.Information("testsvc.Test2");
        ScaleData d1;
        var registerTrigger = 10;
        do
        {
            var T1 = _deviceService.ScaleStatus("TST.FW1");
            d1 = T1.Result;
            Console.Write($"\r{d1.Display}          ");
            if (d1.Weight == registerTrigger)
            {
                registerTrigger++;
                var T2 = _deviceService.ScaleRegister("TST.FW1");
                var d2 = T2.Result;
                Console.Write($"\r\n{d2.Display}\r\n");
            }
        } while (d1.Weight != 99);
    }

    /// <summary>
    ///     mehrere async Tasks starten und später beenden
    /// </summary>
    public async Task Test3()
    {
        _log.Information("testsvc.Test3");

        var T1 = _deviceService.ScaleStatus("TST.FW1");

        var T2 = _deviceService.ScaleStatus("TST.FW2");

        var T12 = _deviceService.ScaleRegister("TST.FW1");


        await Task.WhenAll(T1, T2, T12);

        var data1 = await T1;
        _log.Information(
            $"Status 1 Err:{data1.ErrorNr} Display:{data1.Display} Weight:{data1.Weight} Unit:{data1.Unit}");
        var data2 = await T2;
        _log.Information(
            $"Status 2 Err:{data2.ErrorNr} Display:{data2.Display} Weight:{data2.Weight} Unit:{data2.Unit}");
        var data12 = await T12;
        _log.Information(
            $"Register 1 Err:{data12.ErrorNr} Display:{data12.Display} Eichnr:{data12.CalibrationNumber} Weight:{data12.Weight} Unit:{data12.Unit}");
        await _deviceService.DisposeAsync();
    }

    // Callback Card async Test
    public async Task TestTransp1()
    {
        _log.Information("testsvc.TestTransp1");
        var result = await _deviceService.CardReadStart("TST.TRANSP1", MyCardRead);
        _log.Information("testsvc.TestTransp1 Started");
    }

    private void MyCardRead(CardData cardData)
    {
        _log.Information($"### CardRead {cardData.CardNumber} ###");
        if (cardData.CardNumber == "q") _ = _deviceService.CloseDevice(cardData.DeviceCode);
    }

    // Callback Barcode async Test
    public async Task TestBarcode1()
    {
        _log.Information("testsvc.TestBarcode1");
        var result = await _deviceService.CardReadStart("TST.BARCODE1", MyBarcodeRead);
        _log.Information("testsvc.TestBarcode1 Started");
    }

    private void MyBarcodeRead(CardData cardData)
    {
        _log.Information($"### BarcodeRead {cardData.CardNumber} ###");
        if (cardData.CardNumber == "q") _ = _deviceService.CloseDevice(cardData.DeviceCode);
    }

    // Callback Scale async Test
    public async Task TestScaleStatus()
    {
        _log.Information("testsvc.TestScaleStatus");
        var result = await _deviceService.ScaleStatusStart("TST.FW1", MyScaleStatus);
        _log.Information("testsvc.TestScaleStatus Started");
    }

    private async void MyScaleStatus(ScaleData scaleData)
    {
        if (_oldDisplay != scaleData.Display)
        {
            if (scaleData.ErrorNr == 0)
                _oldDisplay = scaleData.Display;
            _log.Error($"### ScaleStatus {scaleData.Display} ###");
        }
        if (scaleData.Weight == 5)
        {
            _log.Error($"### ScaleStatus 5 close {scaleData.DeviceCode} ###");
            _ = _deviceService.CloseDevice(scaleData.DeviceCode);
            await Task.Delay(100);

            _log.Error("### ScaleStatus 5 close TST.DISP1 ###");
            _ = _deviceService.CloseDevice("TST.DISP1");
        }
    }

    // Callback Sps async Test
    public async Task TestSps()
    {
        _log.Information("testsvc.TestSps");
        var result = await _deviceService.SpsReadStart("TST.SPS_LKW", MySpsRead);
        _log.Information("testsvc.TestSpsStatus Started");
    }

    private async void MySpsRead(SpsData spsData)
    {
        _log.Error($"### SpsRead {spsData.IdLoadorder} {spsData.LoadorderState} {spsData.ActualQuantity} t ###");
        await Task.Delay(100);
    }



    /// <summary>
    ///     Callback Display Test
    /// </summary>
    public async Task TestDisplayShowScale()
    {
        _log.Information("testsvc.TestDisplayShowScale");

        // Voraussetzung für Gewicht
        await TestScaleStatus();

        //Microsoft.AspNetCore.Http.IResult result = await _deviceService.DisplayShowStart("TST.DISP2", MyShow);

        //ScaleDisplay:
        var result = await _deviceService.DisplayShowScale("TST.DISP1", "TST.FW1");
        if (result == null)
        {
        }

        _log.Information("testsvc.TestDisplayShowScale Started");
    }

    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    private static void MyShow(DisplayData displayData)
    {
        displayData.Message = DateTime.Now.ToString("G", CultureInfo.GetCultureInfo("de-DE"));
    }
    //_log.Error($"### DisplayShow {ShowData.Message} ###");
    //aktuelle Uhrzeit
}
