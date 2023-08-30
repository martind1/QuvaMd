namespace Quva.Services.Enums;

public enum OrderDebitorRole
{
    GoodsRecipient = 0,
    InvoiceRecipient = 1,
    ForwardingAgent = 2
}

public enum DebitorRoleValues
{
    GoodsRecipient = 0,
    InvoiceRecipient = 1,
    ForwardingAgent = 2,
    Unknown = 99
}

public enum DeliveryStateValues
{
    Entered = 0,
    Scheduled = 1,
    Yardlist = 2,
    Loaded = 3,
    Weighed = 4,
    ExportedSAP = 5,
    Cancelled = 6
}

public enum ScaleUnit
{
    Nothing = 0,
    Ton = 1,
    Kilogram = 2,
    Gram = 3
}

/*
  TransferType                    SapExportState
  0,1   (3) SHP__SLS QUVA -> ___  1 -> 2(WaitForResponce)
  2   + (4) ALEAUD01 ___ -> QUVA  2,4 -> 3(CorrectlyConfirmed) oder 4(IncorrectlyConfirmed)
  3,4 + (5) DELVRY07 QUVA -> ___  3 -> 5(WaitForResponce2)
 *5   + (6) ALEAUD01 ___ -> QUVA  5,7,8,9 -> 6(WaitForDesadv) oder 7(IncorrectlyConfirmed2) 
 *                                         oder 8->10(Exported) oder 9->11(ExportedWithDifference)
  6   * (7) DESADV01 ___ -> QUVA  5,6,7 -> 8(CorrectlyConfirmed2) oder 9(Confirmed2WithDifference) 
                                           oder 10(Exported) oder 11(ExportedWithDifference)
  7,8 * (8) SYSTAT01 QUVA -> ___  keine Statusänderung

 * kein Trigger
*/

public static class IdocTypes
{
    // Delivery Out:
    public const string SHP_OBDLV_CREATE_SLS01 = "SHP_OBDLV_CREATE_SLS01";
    public const string DELVRY07 = "DELVRY07";
    // Out:
    public const string SYSTAT01 = "SYSTAT01";

    // Delivery In:
    public const string ALEAUD = "ALEAUD";
    public const string ZDESADV = "ZDESADV";
    // In:
    public const string MATMAS = "MATMAS";
    public const string DEBMAS = "DEBMAS";
    public const string CLFMAS = "CLFMAS";
    public const string ZORDRSP = "ZORDRSP";
}

public enum SapExportStateValues
{
    NoExport = 0,
    WaitForExport = 1,
    WaitForResponce = 2,
    CorrectlyConfirmed = 3,
    IncorrectlyConfirmed = 4,

    WaitForResponce2 = 5,
    WaitForDesadv = 6,
    IncorrectlyConfirmed2 = 7,

    CorrectlyConfirmed2 = 8,
    Confirmed2WithDifference = 9,
    Exported = 10,
    ExportedWithDifference = 11,

    IdocOutError = 12

}

public enum TransferTypeValues
{
    SentToQueue = 0,
    SentPerHttp = 1,
    ResponseSap = 2,

    SentToQueue2 = 3,
    SentPerHttp2 = 4,
    ResponseSap2 = 5,
    DesadvFromSap = 6,
    SentToQueue3 = 7,
    SentPerHttp3 = 8,

    DispoFromSap = 9,
    ImportFromRadlader = 10,
    ExportToBde = 11
}

public enum TransportTypeValues
{
    All = 0,
    Truck = 1,
    Rail = 2
}

public enum PackagingTypeValues
{
    All = 0,
    Bulk = 1,
    Packaged = 2
}

