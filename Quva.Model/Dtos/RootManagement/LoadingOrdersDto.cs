using Quva.Database.Models;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement;

public partial class LoadingOrdersDto : BaseDto<LoadingOrdersDto, LoadingOrders>
{
    public long BelaId { get; set; }

    public string WerkNr { get; set; } = null!;

    public string? BeinNr { get; set; }

    public string? Status { get; set; }

    public decimal? Sollmenge { get; set; }

    public decimal? Istmenge { get; set; }

    public string? GewichtEinheit { get; set; }

    public decimal? TaraGewicht { get; set; }

    public decimal? BruttoGewicht { get; set; }

    public int? TaraGewichtId { get; set; }

    public int? BruttoGewichtId { get; set; }

    public decimal? NettoGewicht { get; set; }

    public string? IdentNr { get; set; }

    public string? MaraNr { get; set; }

    public string? MaraName { get; set; }

    public string? Sprache { get; set; }

    public string? Transportmittel { get; set; }

    public DateTime? Datum { get; set; }

    public string? Zeit { get; set; }

    public int? Silp1Nr { get; set; }

    public int? Silp2Nr { get; set; }

    public int? Silp3Nr { get; set; }

    public decimal? Anteil1 { get; set; }

    public decimal? Anteil2 { get; set; }

    public decimal? Anteil3 { get; set; }

    public decimal? Teilmenge1 { get; set; }

    public decimal? Teilmenge2 { get; set; }

    public decimal? Teilmenge3 { get; set; }

    public decimal? Teilmenge4 { get; set; }

    public decimal? Teilmenge5 { get; set; }

    public decimal? Nachlauf { get; set; }

    public int? Sw1Silp1Nr { get; set; }

    public int? Sw1Silp2Nr { get; set; }

    public int? Sw1Silp3Nr { get; set; }

    public decimal? Sw1Anteil1 { get; set; }

    public decimal? Sw1Anteil2 { get; set; }

    public decimal? Sw1Anteil3 { get; set; }

    public int? Sw2Silp1Nr { get; set; }

    public int? Sw2Silp2Nr { get; set; }

    public int? Sw2Silp3Nr { get; set; }

    public decimal? Sw2Anteil1 { get; set; }

    public decimal? Sw2Anteil2 { get; set; }

    public decimal? Sw2Anteil3 { get; set; }

    public string? Stoerungen { get; set; }

    public string? LfsDruck { get; set; }

    public string? StatusText { get; set; }

    public string? AufkNr { get; set; }

    public int? LfskNr { get; set; }

    public string? Manuell { get; set; }

    public string? OffeneBel { get; set; }

    public string? SpsBel { get; set; }

    public string? KombiKnz { get; set; }

    public string? Verpackart { get; set; }

    public short? SiloNr { get; set; }

    public string? Notbetrieb { get; set; }

    public byte? TeilmengeNr { get; set; }

    public decimal? Teilmenge1Ist { get; set; }

    public decimal? Teilmenge2Ist { get; set; }

    public decimal? Teilmenge3Ist { get; set; }

    public decimal? Teilmenge4Ist { get; set; }

    public decimal? Teilmenge5Ist { get; set; }

    public decimal? Feuchte { get; set; }

    public bool? AnzSw { get; set; }

    public decimal? SperrFeuchte { get; set; }

    public decimal? Maxbrutto { get; set; }

    public string? Quittieren { get; set; }

    public string? Probenahme { get; set; }

    public string? Muster { get; set; }

    public string? MusterNr { get; set; }

    public string? AuftWdhlg { get; set; }

    public decimal? Stillstand { get; set; }

    public decimal? Sw1Nachlauf { get; set; }

    public decimal? Sw1Stillstand { get; set; }

    public decimal? Sw2Nachlauf { get; set; }

    public decimal? Sw2Stillstand { get; set; }

    public string? Analyse { get; set; }

    public int? Silp4Nr { get; set; }

    public int? Silp5Nr { get; set; }

    public int? Silp6Nr { get; set; }

    public int? Silp7Nr { get; set; }

    public decimal? Anteil4 { get; set; }

    public decimal? Anteil5 { get; set; }

    public decimal? Anteil6 { get; set; }

    public decimal? Anteil7 { get; set; }

    public int? Sw1Silp4Nr { get; set; }

    public int? Sw1Silp5Nr { get; set; }

    public int? Sw1Silp6Nr { get; set; }

    public int? Sw1Silp7Nr { get; set; }

    public decimal? Sw1Anteil4 { get; set; }

    public decimal? Sw1Anteil5 { get; set; }

    public decimal? Sw1Anteil6 { get; set; }

    public decimal? Sw1Anteil7 { get; set; }

    public int? Sw2Silp4Nr { get; set; }

    public int? Sw2Silp5Nr { get; set; }

    public int? Sw2Silp6Nr { get; set; }

    public int? Sw2Silp7Nr { get; set; }

    public decimal? Sw2Anteil4 { get; set; }

    public decimal? Sw2Anteil5 { get; set; }

    public decimal? Sw2Anteil6 { get; set; }

    public decimal? Sw2Anteil7 { get; set; }

    public decimal? MischFehler { get; set; }

    public string? MischErgebnis { get; set; }

    public int? Silp8Nr { get; set; }

    public decimal? Anteil8 { get; set; }

    public int? Sw1Silp8Nr { get; set; }

    public decimal? Sw1Anteil8 { get; set; }

    public int? Sw2Silp8Nr { get; set; }

    public decimal? Sw2Anteil8 { get; set; }

    public string? Akz1Nr { get; set; }

    public string? Akz2Nr { get; set; }

    public string? Akz3Nr { get; set; }

    public string? Akz4Nr { get; set; }

    public string? Akz5Nr { get; set; }

    public string? Akz6Nr { get; set; }

    public string? Akz7Nr { get; set; }

    public string? Akz8Nr { get; set; }

    public decimal? Leist1 { get; set; }

    public decimal? Leist2 { get; set; }

    public decimal? Leist3 { get; set; }

    public decimal? Leist4 { get; set; }

    public decimal? Leist5 { get; set; }

    public decimal? Leist6 { get; set; }

    public decimal? Leist7 { get; set; }

    public decimal? Leist8 { get; set; }

    public string? Sw1Akz1Nr { get; set; }

    public string? Sw1Akz2Nr { get; set; }

    public string? Sw1Akz3Nr { get; set; }

    public string? Sw1Akz4Nr { get; set; }

    public string? Sw1Akz5Nr { get; set; }

    public string? Sw1Akz6Nr { get; set; }

    public string? Sw1Akz7Nr { get; set; }

    public string? Sw1Akz8Nr { get; set; }

    public decimal? Sw1Leist1 { get; set; }

    public decimal? Sw1Leist2 { get; set; }

    public decimal? Sw1Leist3 { get; set; }

    public decimal? Sw1Leist4 { get; set; }

    public decimal? Sw1Leist5 { get; set; }

    public decimal? Sw1Leist6 { get; set; }

    public decimal? Sw1Leist7 { get; set; }

    public decimal? Sw1Leist8 { get; set; }

    public string? Sw2Akz1Nr { get; set; }

    public string? Sw2Akz2Nr { get; set; }

    public string? Sw2Akz3Nr { get; set; }

    public string? Sw2Akz4Nr { get; set; }

    public string? Sw2Akz5Nr { get; set; }

    public string? Sw2Akz6Nr { get; set; }

    public string? Sw2Akz7Nr { get; set; }

    public string? Sw2Akz8Nr { get; set; }

    public decimal? Sw2Leist1 { get; set; }

    public decimal? Sw2Leist2 { get; set; }

    public decimal? Sw2Leist3 { get; set; }

    public decimal? Sw2Leist4 { get; set; }

    public decimal? Sw2Leist5 { get; set; }

    public decimal? Sw2Leist6 { get; set; }

    public decimal? Sw2Leist7 { get; set; }

    public decimal? Sw2Leist8 { get; set; }

    public string? SiebVorgabe { get; set; }

    public string? BelaProbNr { get; set; }

    public decimal? BelaProbId { get; set; }

    public string? BelaErgebnis { get; set; }

    public decimal? BelaFehler { get; set; }

    public decimal? BelaMaxabw { get; set; }

    public string? BelaSgabw { get; set; }

    public string? SiebSgu { get; set; }

    public string? SiebSgo { get; set; }

    public decimal? MischMaxabw { get; set; }

    public decimal? IstMaxabw { get; set; }

    public string? MischSgabw { get; set; }

    public string? IstSgabw { get; set; }

    public decimal? Teilmenge6Ist { get; set; }

    public decimal? Teilmenge7Ist { get; set; }

    public decimal? Teilmenge8Ist { get; set; }

    public decimal? Teilmenge9Ist { get; set; }

    public decimal? Teilmenge10Ist { get; set; }

    public string? KunwNr { get; set; }

    public string? ChargeNr { get; set; }

    public int? AufpNr { get; set; }

    public decimal? IstFehler { get; set; }

    public string? IstErgebnis { get; set; }

    public decimal? Silp1Ist { get; set; }

    public decimal? Silp2Ist { get; set; }

    public decimal? Silp3Ist { get; set; }

    public decimal? Silp4Ist { get; set; }

    public decimal? Silp5Ist { get; set; }

    public decimal? Silp6Ist { get; set; }

    public decimal? Silp7Ist { get; set; }

    public decimal? Silp8Ist { get; set; }

    public string? BelaSektion { get; set; }

    public decimal? Silostand1 { get; set; }

    public decimal? Silostand2 { get; set; }

    public decimal? Silostand3 { get; set; }

    public decimal? Silostand4 { get; set; }

    public decimal? Silostand5 { get; set; }

    public decimal? Silostand6 { get; set; }

    public decimal? Silostand7 { get; set; }

    public decimal? Silostand8 { get; set; }

    public decimal? LeistIstGesamt { get; set; }

    public decimal? LeistIstSieb { get; set; }

    public string? Mr3Knz { get; set; }

    public int? QualKuveId { get; set; }

    public decimal? Teilmenge22Ist { get; set; }

    public decimal? Teilmenge11Ist { get; set; }

    public decimal? Teilmenge13Ist { get; set; }

    public decimal? Teilmenge14Ist { get; set; }

    public decimal? Teilmenge17Ist { get; set; }

    public decimal? Teilmenge18Ist { get; set; }

    public decimal? Teilmenge16Ist { get; set; }

    public decimal? Teilmenge23Ist { get; set; }

    public decimal? Teilmenge24Ist { get; set; }

    public decimal? Teilmenge12Ist { get; set; }

    public decimal? Teilmenge20Ist { get; set; }

    public decimal? Teilmenge21Ist { get; set; }

    public decimal? Teilmenge19Ist { get; set; }

    public decimal? Teilmenge15Ist { get; set; }

    public string? Probe1Nr { get; set; }

    public string? Probe2Nr { get; set; }

    public string? Probe3Nr { get; set; }

    public string? Probe4Nr { get; set; }

    public string? Probe5Nr { get; set; }

    public string? Probe6Nr { get; set; }

    public string? Probe7Nr { get; set; }

    public string? Probe8Nr { get; set; }

    public string? Verladesperre { get; set; }

    public string? Grundsorte1 { get; set; }

    public string? Grundsorte2 { get; set; }

    public string? Grundsorte3 { get; set; }

    public string? Grundsorte4 { get; set; }

    public string? Grundsorte5 { get; set; }

    public string? Grundsorte6 { get; set; }

    public string? Grundsorte7 { get; set; }

    public string? Grundsorte8 { get; set; }

    public string? Sw1Grundsorte1 { get; set; }

    public string? Sw1Grundsorte2 { get; set; }

    public string? Sw1Grundsorte3 { get; set; }

    public string? Sw1Grundsorte4 { get; set; }

    public string? Sw1Grundsorte5 { get; set; }

    public string? Sw1Grundsorte6 { get; set; }

    public string? Sw1Grundsorte7 { get; set; }

    public string? Sw1Grundsorte8 { get; set; }

    public string? Sw2Grundsorte1 { get; set; }

    public string? Sw2Grundsorte2 { get; set; }

    public string? Sw2Grundsorte3 { get; set; }

    public string? Sw2Grundsorte4 { get; set; }

    public string? Sw2Grundsorte5 { get; set; }

    public string? Sw2Grundsorte6 { get; set; }

    public string? Sw2Grundsorte7 { get; set; }

    public string? Sw2Grundsorte8 { get; set; }

    public decimal? NettoGewichtSand { get; set; }

    public int? NettoGewichtSandId { get; set; }

    public decimal? NettoGewichtKies { get; set; }

    public int? NettoGewichtKiesId { get; set; }

    public int? NettoGewichtId { get; set; }

    public string? AnlageNr { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }
}
