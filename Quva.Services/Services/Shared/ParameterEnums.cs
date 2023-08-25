using Quva.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Services.Services.Shared;

public enum TransportType
{
    All = 0,
    Truck = 1,
    Rail = 2
}

public enum PackagingType
{
    All = 0,
    Bulk = 1,
    Packaged = 2
}

public enum DataTypeValues
{
    t_string = 0,
    t_int = 1,
    t_bool = 2,
    t_float = 3,
    t_date = 4,
    t_blob = 5
}

public enum OrderDebitorRole
{
    GoodsRecipient = 0,
    InvoiceRecipient = 1,
    ForwardingAgent = 2
}

public static class TypeAgreementOptionCode
{
    public const string ShippingInstruction_Message = "ShippingInstruction_Message"; // Versandanweisung Meldung
    public const string FEUCHTE_CHECK = "FEUCHTE_CHECK"; // Feuchtewert überprüfen
    public const string FEUCHTE_PRINT = "FEUCHTE_PRINT"; // Feuchtewert drucken
    public const string WARN_FEUCHTE = "WARN_FEUCHTE"; // Warnen ab[%]
    public const string SPERR_FEUCHTE = "SPERR_FEUCHTE"; // Sperren ab[%]
    public const string VB_QUITT = "VB_QUITT"; // Beladung quittieren
    public const string KONT_PFLICHT = "KONT_PFLICHT"; // Kontingent Pflicht
    public const string PROBE_SW = "PROBE_SW"; // Probe nur bei Silowechsel
    public const string ANZ_LFS = "ANZ_LFS"; // Anzahl Lieferscheine
    public const string VERSANDAVIS = "VERSANDAVIS"; // Versandavis
    public const string VERSANDAVIS_EMAIL = "VERSANDAVIS_EMAIL"; // Versandavis E-Mail Adressen
    public const string VERSANDAVIS_LFS_ANHANG = "VERSANDAVIS_LFS_ANHANG"; // Versandavis Lieferschein im Anhang
    public const string KUMULIERT = "KUMULIERT"; // Kumuliert
    public const string PROBE = "PROBE"; // Probe für Fahrer
    public const string LAB_WZ_FREQ = "LAB_WZ_FREQ"; // Werkszeugniss Häufigkeit(0=nie, 1, 2, 3)
    public const string Lab_Message = "Lab_Message"; // Labor Besonderheiten
    public const string MK_SGU = "MK_SGU"; // Mittlere Korngröße SGU
    public const string MK_SGO = "MK_SGO"; // Mittlere Korngröße SGO
    public const string MAX_BRUTTO = "MAX_BRUTTO"; // Maximales Bruttogewicht für Lieferschein[t]
    public const string MAX_NETTO = "MAX_NETTO"; // Maximale Ausladung für Lieferschein[t]
    public const string SCHIFFCONTAINER = "SCHIFFCONTAINER"; // Schiff Container
    public const string MITAUFLIEGER = "MITAUFLIEGER"; // Auflieger Eingabepflicht
}