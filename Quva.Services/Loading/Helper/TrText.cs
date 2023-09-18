using Quva.Services.Enums;

namespace Quva.Services.Loading.Helper;

public static class TrText
{
    public static string Tr(LanguageEnum language, string code)
    {
        string result;
        if (language == LanguageEnum.EN)
        {
            result = code switch
            {
                TrCode.LoadingService.NoSiloMaterial => "No Silo for Material:{0} BasicType:{1}.Mix{2} Point:{3}",
                TrCode.LoadingService.WrongMixindex => "Wrong MixIndex {0}. Must be 0 when no Mix. BasicType:{1}",
                TrCode.LoadingService.WrongMixindex0 => "Wrong MixIndex 0. Must be greater 0 when Mix. BasicType:{0}",
                TrCode.LoadingService.NoTrueBasicType => "Contingent Material no True Basic Type; MaterialId:{0}",
                TrCode.LoadingService.NoContingentSilos => "Contingent Siloset no Silos; Set:{0} Id:{1}",
                TrCode.LoadingService.DeliveryNotFound => "Delivery not found ID:{0}",
                TrCode.LoadingService.DebitorNotFound => "Debitor not found: {0} in DeliveryId {1}",
                TrCode.LoadingService.LocationNull => "Location is null; in DeliveryId {0}",
                TrCode.LoadingService.MaterialNotFound => "Material not found; in DeliveryId Mat:{0} DelId:{1}",
                TrCode.LoadingService.ContingentRequired => "no valid contingents but required; IdDebitor:{0} IdMaterial:{1}",

                TrCode.LoadingService.NoLoadingPoints => "No Loading Points for IdDelivery:({0})",
                TrCode.LoadingService.NoMainPositions => "No Main Positions for IdDelivery:({0})",
                TrCode.LoadingService.LoadorderExists => "Active Loadorder already exists. Point({0}) Load.ID({1})",

                TrCode.LoadingService.LockBigbag => "Locked for BigBag; Silos:{0}",
                TrCode.LoadingService.LockTruck2 => "Locked for Truck2; Silos:{0}",
                TrCode.LoadingService.LockTruck => "Locked for Truck; Silos:{0}",
                TrCode.LoadingService.LockRail => "Locked for Rail; Silos:{0}",
                TrCode.LoadingService.LockLaboratory => "Locked by Laboratory; Silos:{0}",
                TrCode.LoadingService.LockProduction => "Locked for Production; Silos:{0}",
                TrCode.LoadingService.LockForSensitiveCustomer => "Locked for Sensitive Customer; Silos:{0}",
                TrCode.LoadingService.CheckSilolevel => "Silo Level to low; Silo:{0}",

                _ => code
            };
        }
        else  //if (language == LanguageEnum.DE)
        {
            result = code switch
            {
                TrCode.LoadingService.NoSiloMaterial => "Kein Silo für Material:{0} Grundsorte:{1}.Mix{2} Stelle:{3}",
                TrCode.LoadingService.WrongMixindex => "Falscher MixIndex {0}. Muss 0 sein wenn kein Mix. Grundsorte:{1}",
                TrCode.LoadingService.WrongMixindex0 => "Falscher MixIndex 0. Muss >0 sein bei Mix. Grundsorte:{0}",
                TrCode.LoadingService.NoTrueBasicType => "Kontingent: Material ist keine Grundsorte; MaterialId:{0}",
                TrCode.LoadingService.NoContingentSilos => "Kontingent Siloset.{0} keine Silos; Id:{1}",
                TrCode.LoadingService.DeliveryNotFound => "Lieferschein nicht gefunden ID:{0}",
                TrCode.LoadingService.DebitorNotFound => "Kunde {0} nicht gefunden in Lieferschein Id {1}",
                TrCode.LoadingService.LocationNull => "Location ist null; in DeliveryId {0}",
                TrCode.LoadingService.MaterialNotFound => "Material nicht gefunden; in DeliveryId Mat:{0} DelId:{1}",
                TrCode.LoadingService.ContingentRequired => "keine gültigen Kontingente trotz Kontingentpflicht; IdDebitor:{0} IdMaterial:{1}",

                TrCode.LoadingService.NoLoadingPoints => "Keine Beladestellen für Lieferschein Id:({0})",
                TrCode.LoadingService.NoMainPositions => "Keine Hauptpositionen für Lieferschein Id:({0})",
                TrCode.LoadingService.LoadorderExists => "Es gibt bereits einen aktiven Beladeauftrag. Stelle({0}) Bel.ID({1})",

                TrCode.LoadingService.LockBigbag => "Gesperrt für BigBag; Silos:{0}",
                TrCode.LoadingService.LockTruck2 => "Gesperrt für Lkw2; Silos:{0}",
                TrCode.LoadingService.LockTruck => "Gesperrt für Lkw; Silos:{0}",
                TrCode.LoadingService.LockRail => "Gesperrt für Bahn; Silos:{0}",
                TrCode.LoadingService.LockLaboratory => "Gesperrt von Labor; Silos:{0}",
                TrCode.LoadingService.LockProduction => "Locked für Produktion; Silos:{0}",
                TrCode.LoadingService.LockForSensitiveCustomer => "Gesperrt für sensible Kunden; Silos:{0}",
                TrCode.LoadingService.CheckSilolevel => "Silostand zu niedrig; Silo:{0}",

                _ => code
            };
        }

        return result;
    }
}
