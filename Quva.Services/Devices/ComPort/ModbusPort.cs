using AutoMapper.Execution;
using Microsoft.Extensions.Logging.Abstractions;
using Quva.Services.Devices.Data;
using Quva.Services.Devices.EasyModbus;
using Quva.Services.Devices.Modbus;
using Quva.Services.Services.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Services.Devices.ComPort;

public class ModbusPort : IComPort
{
    public ModbusParameter ModbusParameter { get; set; }
    public string DeviceCode { get; }
    public PortType PortType { get; } = PortType.Modbus;
    public ComParameter ComParameter { get; set; }
    public bool DirectMode { get; } = false;
    public uint Bcc { get; set; }

    private readonly ByteBuff _inBuff;
    private readonly ILogger _log;
    private readonly ByteBuff _outBuff;
    private readonly ModbusClient _modbusClient;
    public ModbusPort(ComDevice device) : this(device.Code, device.Device.ParamString ?? string.Empty)
    {
    }

    public ModbusPort(string deviceCode, string paramString)
    {
        _log = Log.ForContext<DeviceService>();
        DeviceCode = deviceCode; //for Debug Output
        ComParameter = new ComParameter();
        ModbusParameter = new ModbusParameter();
        _modbusClient = new ModbusClient();
        SetParamString(paramString);
        _inBuff = new ByteBuff(4096);
        _outBuff = new ByteBuff(4096);
    }

    public bool IsConnected()
    {
        return _modbusClient.Connected;
    }

    public async ValueTask DisposeAsync()
    {
        _log.Warning($"[{DeviceCode}] {nameof(ModbusPort)}.DisposeAsync()");
        await DisposeAsyncCore().ConfigureAwait(false);

        GC.SuppressFinalize(this);
    }


    // Setzt Parameter
    // zB "COM1:9600:E:8:1:N[:H]"
    public void SetParamString(string paramstring)
    {
        var SL = paramstring.Split(":");
        if (SL.Length != 2)
            throw new ArgumentException("Wrong Paramstring. Must be Host:Port or Listen:Port", nameof(paramstring));
        ModbusParameter.ParamString = paramstring;
        ModbusParameter.Host = SL[0];
        ModbusParameter.Port = int.Parse(SL[1]);
    }

    public async Task OpenAsync()
    {
        if (IsConnected())
        {
            _log.Warning($"[{DeviceCode}] TCP({ModbusParameter.ParamString}): allready opened");
            return;
        }
        try
        {
            _log.Information($"[{DeviceCode}] ModbusClient.OpenAsync {ModbusParameter.ParamString}");
            _modbusClient.Port = ModbusParameter.Port;
            _modbusClient.IPAddress = ModbusParameter.Host;

            if (ComParameter.TimeoutMs == 0)
                ComParameter.TimeoutMs = 10000; //overwritable in Desc. Bevore: Timeout.Infinite;
            if (ComParameter.Timeout2Ms == 0)
                ComParameter.Timeout2Ms = 500; //overwritable in Desc (T2:500)

            _modbusClient.ConnectionTimeout = ComParameter.TimeoutMs;
            await Task.Run(() =>
            {
                _modbusClient.Connect();
            });
        }
        catch
        {
            throw;
        }
    }

    public async Task CloseAsync()
    {
        if (!IsConnected())
        {
            _log.Warning($"[{DeviceCode}] [{_modbusClient.IPAddress}] allready closed");
            return;
        }

        _log.Debug($"[{DeviceCode}] [{_modbusClient.IPAddress}] CloseAsync()");
        await Task.Run(() =>
        {
            _modbusClient.Disconnect();  //Close connection to Master Device.
        });
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        _log.Warning($"[{DeviceCode}] {nameof(ModbusPort)}.DisposeAsyncCore()");
        await CloseAsync();
    }

    #region input/output

    public async Task ResetAsync()
    {
        if (IsConnected())
            await CloseAsync();
        await OpenAsync();
    }

    //muss vor Read aufgerufen werden. Ruft Flush auf.
    public async Task<int> InCountAsync(int WaitMs)
    {
        //0 important for ComProtocol.ReadDataAsync!
        return await Task.FromResult(0);
    }

    // read with timeout. into Buffer offset 0; max buffer.Cnt bytes. Returns 0 when no data available
    public async Task<int> ReadAsync(ByteBuff buffer)
    {
        // Responce is already in _inBuff
        buffer.CopyFrom(_inBuff);
        _log.Debug($"[{DeviceCode}] ModbusPort.ReadAsync {buffer.Cnt} {ModbusParameter.Host}");
        return await Task.FromResult(buffer.Cnt);
    }

    /// <summary>
    ///     write only to internal buffer. Write to network later in flush.
    /// </summary>
    /// <returns>new Count in internal buffer</returns>
    public async Task<bool> WriteAsync(ByteBuff buffer)
    {
        // calling Modbus function (readx, writex)
        // buffer: <function:1>|<Adress:2>|<Count:1>[|<values as byte array>]
        ModbusFunction modbusFunction = (ModbusFunction)buffer.Buff[0];
        int startingAddress = BitConverter.ToInt16(buffer.Buff, 1);
        int quantity = buffer.Buff[3];
        // TODO: move Modbus responce to _inBuff
        bool[]? boolValues = null;
        int[]? intValues = null;
        switch (modbusFunction)
        {
            case ModbusFunction.ReadCoils:
                boolValues = _modbusClient.ReadCoils(startingAddress, quantity);
                break;
            case ModbusFunction.ReadDiscreteInputs:
                boolValues = _modbusClient.ReadDiscreteInputs(startingAddress, quantity); 
                break;
            case ModbusFunction.ReadHoldingRegisters:
                intValues =_modbusClient.ReadHoldingRegisters(startingAddress, quantity); 
                break;
            case ModbusFunction.ReadInputRegisters:
                intValues = _modbusClient.ReadInputRegisters(startingAddress, quantity); 
                break;
            case ModbusFunction.WriteSingleCoil: 
                _modbusClient.WriteSingleCoil(startingAddress, buffer.Buff[4] != 0); 
                break;
            case ModbusFunction.WriteSingleRegister: 
                _modbusClient.WriteSingleRegister(startingAddress, BitConverter.ToInt16(buffer.Buff, 4)); 
                break;
            case ModbusFunction.WriteMultipleCoils:
                boolValues = new bool[buffer.Cnt - 4];
                for (int i = 0; i < boolValues.Length; i++)
                {
                    boolValues[i] = buffer.Buff[4 + i] != 0;
                }
                _modbusClient.WriteMultipleCoils(startingAddress, boolValues); 
                break;
            case ModbusFunction.WriteMultipleRegisters:
                intValues = new int[(buffer.Cnt - 4) / 2];
                for (int i = 0; i < intValues.Length; i++)
                {
                    intValues[i] = BitConverter.ToInt16(buffer.Buff, 4 + 2 * i);
                }
                _modbusClient.WriteMultipleRegisters(startingAddress, intValues); 
                break;
        }
        if (boolValues != null && boolValues.Length > 0)
        {
            _inBuff.Cnt = boolValues.Length;
            for (int i = 0; i < boolValues.Length; i++)
            {
                _inBuff.Buff[i] = (byte)(boolValues[i] ? 1 : 0);
            }
        }
        else if (intValues != null && intValues.Length > 0)
        {
            _inBuff.Cnt = intValues.Length * 2;
            for (int i = 0; i < intValues.Length; i++)
            {
                byte[] shortBytes = BitConverter.GetBytes((short)intValues[i]);
                _inBuff.Buff[i * 2] = shortBytes[0];
                _inBuff.Buff[i * 2 + 1] = shortBytes[1];
            }
        }
        else
        {
            throw new Exception("_inBuff is undefined");
        }
        return await Task.FromResult(true);
    }

    //muss vor Read und vor InCount und am Telegram Ende aufgerufen werden
    public async Task FlushAsync()
    {
        //nothing to flush
        await Task.Delay(0);
    }
}
#endregion input/output

public class ModbusParameter
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }

    public string ParamString { get; set; } = string.Empty;
}