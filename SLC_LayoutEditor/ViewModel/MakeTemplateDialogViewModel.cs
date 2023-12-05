using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Enum;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace SLC_LayoutEditor.ViewModel
{
    public class MakeTemplateDialogViewModel : AddDialogViewModel
    {
        private bool mKeepWalls = true;
        private int wallsCount;
        private bool mKeepPassengerDoors = true;
        private int passengerDoorsCount;
        private bool mKeepLoadingBays = true;
        private int loadingBaysCount;
        private bool mKeepCateringDoors = true;
        private int cateringDoorsCount;
        private bool mKeepCockpits = true;
        private int cockpitsCount;
        private bool mKeepToilets = true;
        private int toiletsCount;
        private bool mKeepIntercoms = true;
        private int intercomsCount;
        private bool mKeepStairways = true;
        private int stairwaysCount;
        private bool mKeepKitchens;
        private int kitchensCount;
        private bool mKeepGalleys;
        private int galleysCount;
        private bool mKeepServicePoints;
        private int servicePointsCount;
        private bool mKeepBusinessClass;
        private int businessClassCount;
        private bool mKeepEconomyClass;
        private int economyClassCount;
        private bool mKeepFirstClass;
        private int firstClassCount;
        private bool mKeepPremiumClass;
        private int premiumClassCount;
        private bool mKeepSupersonicClass;
        private int supersonicClassCount;
        private bool mKeepUnavailableSeats;
        private int unavailableSeatsCount;

        public MakeTemplateDialogViewModel()
            : base("A template with this name exists already!")
        {

        }

        #region Properties
        public bool KeepWalls
        {
            get => mKeepWalls;
            set
            {
                mKeepWalls = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(KeepAllBasics));
            }
        }

        public string WallsCount => wallsCount > 0 ? wallsCount.ToString() : "-";

        public bool KeepPassengerDoors
        {
            get => mKeepWalls;
            set
            {
                mKeepWalls = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(KeepAllBasics));
            }
        }

        public string PassengerDoorsCount => passengerDoorsCount > 0 ? passengerDoorsCount.ToString() : "-";

        public bool KeepLoadingBays
        {
            get => mKeepLoadingBays;
            set
            {
                mKeepLoadingBays = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(KeepAllBasics));
            }
        }

        public string LoadingBaysCount => loadingBaysCount> 0 ? loadingBaysCount.ToString() : "-";

        public bool KeepCateringDoors
        {
            get => mKeepCateringDoors;
            set
            {
                mKeepCateringDoors = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(KeepAllBasics));
            }
        }

        public string CateringDoorsCount => cateringDoorsCount  > 0 ? cateringDoorsCount.ToString() : "-";

        public bool KeepCockpits
        {
            get => mKeepCockpits;
            set
            {
                mKeepCockpits = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(KeepAllBasics));
            }
        }

        public string CockpitsCount => cockpitsCount > 0 ? cockpitsCount.ToString() : "-";

        public bool KeepGalleys
        {
            get => mKeepGalleys;
            set
            {
                mKeepGalleys = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(KeepAllSeats));
            }
        }

        public string GalleysCount => galleysCount > 0 ? galleysCount.ToString() : "-";

        public bool KeepToilets
        {
            get => mKeepToilets;
            set
            {
                mKeepToilets = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(KeepAllServices));
            }
        }

        public string ToiletsCount => toiletsCount > 0 ? toiletsCount.ToString() : "-";

        public bool KeepKitchens
        {
            get => mKeepKitchens;
            set
            {
                mKeepKitchens = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(KeepAllServices));
            }
        }

        public string KitchensCount => kitchensCount > 0 ? kitchensCount.ToString() : "-";

        public bool KeepIntercoms
        {
            get => mKeepIntercoms;
            set
            {
                mKeepIntercoms = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(KeepAllServices));
            }
        }

        public string IntercomsCount => intercomsCount > 0 ? intercomsCount.ToString() : "-";

        public bool KeepStairways
        {
            get => mKeepStairways;
            set
            {
                mKeepStairways = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(KeepAllBasics));
            }
        }

        public string StairwaysCount => stairwaysCount > 0 ? stairwaysCount.ToString() : "-";

        public bool KeepBusinessClass
        {
            get => mKeepBusinessClass;
            set
            {
                mKeepBusinessClass = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(KeepAllSeats));
            }
        }

        public string BusinessClassCount => businessClassCount > 0 ? businessClassCount.ToString() : "-";

        public bool KeepEconomyClass
        {
            get => mKeepEconomyClass;
            set
            {
                mKeepEconomyClass = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(KeepAllSeats));
            }
        }

        public string EconomyClassCount => economyClassCount > 0 ? economyClassCount.ToString() : "-";

        public bool KeepFirstClass
        {
            get => mKeepFirstClass;
            set
            {
                mKeepFirstClass = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(KeepAllSeats));
            }
        }

        public string FirstClassCount => firstClassCount > 0 ? firstClassCount.ToString() : "-";

        public bool KeepPremiumClass
        {
            get => mKeepPremiumClass;
            set
            {
                mKeepPremiumClass = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(KeepAllSeats));
            }
        }

        public string PremiumClassCount => premiumClassCount > 0 ? premiumClassCount.ToString() : "-";

        public bool KeepSupersonicClass
        {
            get => mKeepSupersonicClass;
            set
            {
                mKeepSupersonicClass = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(KeepAllSeats));
            }
        }

        public string SupersonicClassCount => supersonicClassCount > 0 ? supersonicClassCount.ToString() : "-";

        public bool KeepUnavailableSeats
        {
            get => mKeepUnavailableSeats;
            set
            {
                mKeepUnavailableSeats = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(KeepAllSeats));
            }
        }

        public string UnavailableSeatsCount => unavailableSeatsCount > 0 ? unavailableSeatsCount.ToString() : "-"   ;

        public bool KeepServicePoints
        {
            get => mKeepServicePoints;
            set
            {
                mKeepServicePoints = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(KeepAllServices));
            }
        }

        public string ServicePointsCount => servicePointsCount > 0 ? servicePointsCount.ToString() : "-";

        public bool? KeepAllBasics
        {
            get
            {
                if (mKeepWalls && mKeepPassengerDoors && mKeepLoadingBays && mKeepCateringDoors && mKeepCockpits && mKeepStairways)
                {
                    return true;
                }
                else if (!mKeepWalls && !mKeepPassengerDoors && !mKeepLoadingBays && !mKeepCateringDoors && !mKeepCockpits && !mKeepStairways)
                {
                    return false;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value.HasValue)
                {
                    mKeepWalls = value.Value;
                    mKeepPassengerDoors = value.Value;
                    mKeepLoadingBays = value.Value;
                    mKeepCateringDoors = value.Value;
                    mKeepCockpits = value.Value;
                    mKeepStairways = value.Value;

                    InvokePropertyChanged(nameof(KeepWalls));
                    InvokePropertyChanged(nameof(KeepPassengerDoors));
                    InvokePropertyChanged(nameof(KeepLoadingBays));
                    InvokePropertyChanged(nameof(KeepCateringDoors));
                    InvokePropertyChanged(nameof(KeepCockpits));
                    InvokePropertyChanged(nameof(KeepStairways));
                }

                InvokePropertyChanged();
            }
        }

        public string TotalBasicSlots
        {
            get
            {
                int totalBasicSlots = wallsCount + passengerDoorsCount + loadingBaysCount + cateringDoorsCount + cockpitsCount + stairwaysCount;
                return totalBasicSlots > 0 ? totalBasicSlots.ToString() : "";
            }
        }

        public bool? KeepAllServices
        {
            get
            {
                if (mKeepToilets && mKeepIntercoms && mKeepKitchens && mKeepServicePoints)
                {
                    return true;
                }
                else if (!mKeepToilets && !mKeepIntercoms && !mKeepKitchens && !mKeepServicePoints)
                {
                    return false;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value.HasValue)
                {
                    mKeepToilets = value.Value;
                    mKeepIntercoms = value.Value;
                    mKeepKitchens = value.Value;
                    mKeepServicePoints = value.Value;

                    InvokePropertyChanged(nameof(KeepToilets));
                    InvokePropertyChanged(nameof(KeepIntercoms));
                    InvokePropertyChanged(nameof(KeepKitchens));
                    InvokePropertyChanged(nameof(KeepServicePoints));
                }

                InvokePropertyChanged();
            }
        }

        public string TotalServiceSlots
        {
            get
            {
                int totalServiceSlots = toiletsCount + intercomsCount + kitchensCount + servicePointsCount;
                return totalServiceSlots > 0 ? totalServiceSlots.ToString() : "";
            }
        }

        public bool? KeepAllSeats
        {
            get
            {
                if (mKeepGalleys && mKeepEconomyClass && mKeepBusinessClass && mKeepPremiumClass && mKeepFirstClass && mKeepSupersonicClass && mKeepUnavailableSeats)
                {
                    return true;
                }
                else if (!mKeepGalleys && !mKeepEconomyClass && !mKeepBusinessClass && !mKeepPremiumClass && !mKeepFirstClass && !mKeepSupersonicClass && !mKeepUnavailableSeats)
                {
                    return false;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value.HasValue)
                {
                    mKeepGalleys = value.Value;
                    mKeepEconomyClass = value.Value;
                    mKeepBusinessClass = value.Value;
                    mKeepPremiumClass = value.Value;
                    mKeepFirstClass = value.Value;
                    mKeepSupersonicClass = value.Value;
                    mKeepUnavailableSeats = value.Value;

                    InvokePropertyChanged(nameof(KeepGalleys));
                    InvokePropertyChanged(nameof(KeepEconomyClass));
                    InvokePropertyChanged(nameof(KeepBusinessClass));
                    InvokePropertyChanged(nameof(KeepPremiumClass));
                    InvokePropertyChanged(nameof(KeepFirstClass));
                    InvokePropertyChanged(nameof(KeepSupersonicClass));
                    InvokePropertyChanged(nameof(KeepUnavailableSeats));
                }

                InvokePropertyChanged();
            }
        }

        public string TotalSeatSlots
        {
            get
            {
                int totalSeatSlots = galleysCount + economyClassCount + businessClassCount + premiumClassCount +
                                        firstClassCount + supersonicClassCount + unavailableSeatsCount;
                return totalSeatSlots > 0 ? totalSeatSlots.ToString() : "";
            }
        }
        #endregion

        public IEnumerable<CabinSlotType> GetKeptSlotTypes()
        {
            List<CabinSlotType> keptSlotTypes = new List<CabinSlotType>() {
                CabinSlotType.Aisle
            };

            #region Basic slots
            if (KeepAllBasics == true)
            {
                keptSlotTypes.AddRange(
                    new List<CabinSlotType>()
                    {
                        CabinSlotType.Wall,
                        CabinSlotType.Door,
                        CabinSlotType.LoadingBay,
                        CabinSlotType.CateringDoor,
                        CabinSlotType.Cockpit,
                        CabinSlotType.Stairway
                    });
            }
            else if (KeepAllBasics != false)
            {
                if (mKeepWalls)
                {
                    keptSlotTypes.Add(CabinSlotType.Wall);
                }
                if (mKeepPassengerDoors)
                {
                    keptSlotTypes.Add(CabinSlotType.Door);
                }
                if (mKeepLoadingBays)
                {
                    keptSlotTypes.Add(CabinSlotType.LoadingBay);
                }
                if (mKeepCateringDoors)
                {
                    keptSlotTypes.Add(CabinSlotType.CateringDoor);
                }
                if (mKeepCockpits)
                {
                    keptSlotTypes.Add(CabinSlotType.Cockpit);
                }
                if (mKeepStairways)
                {
                    keptSlotTypes.Add(CabinSlotType.Stairway);
                }
            }
            #endregion
            #region Service slots
            if (KeepAllServices == true)
            {
                keptSlotTypes.AddRange(
                    new List<CabinSlotType>()
                    {
                        CabinSlotType.Toilet,
                        CabinSlotType.Intercom,
                        CabinSlotType.Kitchen,
                        CabinSlotType.ServiceEndPoint,
                        CabinSlotType.ServiceStartPoint
                    });
            }
            else if (KeepAllServices != false)
            {
                if (mKeepToilets)
                {
                    keptSlotTypes.Add(CabinSlotType.Toilet);
                }
                if (mKeepIntercoms)
                {
                    keptSlotTypes.Add(CabinSlotType.Intercom);
                }
                if (mKeepKitchens)
                {
                    keptSlotTypes.Add(CabinSlotType.Kitchen);
                }
                if (mKeepServicePoints)
                {
                    keptSlotTypes.Add(CabinSlotType.ServiceStartPoint);
                    keptSlotTypes.Add(CabinSlotType.ServiceEndPoint);
                }
            }
            #endregion
            #region Seat slots
            if (KeepAllSeats == true)
            {
                keptSlotTypes.AddRange(
                    new List<CabinSlotType>()
                    {
                        CabinSlotType.Galley,
                        CabinSlotType.EconomyClassSeat,
                        CabinSlotType.BusinessClassSeat,
                        CabinSlotType.PremiumClassSeat,
                        CabinSlotType.FirstClassSeat,
                        CabinSlotType.SupersonicClassSeat,
                        CabinSlotType.UnavailableSeat
                    });
            }
            else if (KeepAllSeats != false)
            {
                if (mKeepGalleys)
                {
                    keptSlotTypes.Add(CabinSlotType.Galley);
                }
                if (mKeepEconomyClass)
                {
                    keptSlotTypes.Add(CabinSlotType.EconomyClassSeat);
                }
                if (mKeepBusinessClass)
                {
                    keptSlotTypes.Add(CabinSlotType.BusinessClassSeat);
                }
                if (mKeepPremiumClass)
                {
                    keptSlotTypes.Add(CabinSlotType.PremiumClassSeat);
                }
                if (mKeepFirstClass)
                {
                    keptSlotTypes.Add(CabinSlotType.FirstClassSeat);
                }
                if (mKeepSupersonicClass)
                {
                    keptSlotTypes.Add(CabinSlotType.SupersonicClassSeat);
                }
                if (mKeepUnavailableSeats)
                {
                    keptSlotTypes.Add(CabinSlotType.UnavailableSeat);
                }
            }
            #endregion

            return keptSlotTypes;
        }

        public void CalculateSlotsCount(CabinLayout source)
        {
            IEnumerable<CabinSlot> cabinSlots = source.CabinDecks.SelectMany(x => x.CabinSlots);
            wallsCount = cabinSlots.CountSlots(CabinSlotType.Wall);
            passengerDoorsCount = cabinSlots.CountSlots(CabinSlotType.Door);
            loadingBaysCount = cabinSlots.CountSlots(CabinSlotType.LoadingBay);
            cateringDoorsCount = cabinSlots.CountSlots(CabinSlotType.CateringDoor);
            cockpitsCount = cabinSlots.CountSlots(CabinSlotType.Cockpit);
            toiletsCount = cabinSlots.CountSlots(CabinSlotType.Toilet);
            intercomsCount = cabinSlots.CountSlots(CabinSlotType.Intercom);
            stairwaysCount = cabinSlots.CountSlots(CabinSlotType.Stairway);
            kitchensCount = cabinSlots.CountSlots(CabinSlotType.Kitchen);
            galleysCount = cabinSlots.CountSlots(CabinSlotType.Galley);
            servicePointsCount = cabinSlots.CountSlots(CabinSlotType.ServiceStartPoint, CabinSlotType.ServiceEndPoint);
            businessClassCount = cabinSlots.CountSlots(CabinSlotType.BusinessClassSeat);
            economyClassCount = cabinSlots.CountSlots(CabinSlotType.EconomyClassSeat);
            firstClassCount = cabinSlots.CountSlots(CabinSlotType.FirstClassSeat);
            premiumClassCount = cabinSlots.CountSlots(CabinSlotType.PremiumClassSeat);
            supersonicClassCount = cabinSlots.CountSlots(CabinSlotType.SupersonicClassSeat);
            unavailableSeatsCount = cabinSlots.CountSlots(CabinSlotType.UnavailableSeat);

            InvokePropertyChanged(nameof(WallsCount));
            InvokePropertyChanged(nameof(PassengerDoorsCount));
            InvokePropertyChanged(nameof(LoadingBaysCount));
            InvokePropertyChanged(nameof(CateringDoorsCount));
            InvokePropertyChanged(nameof(CockpitsCount));
            InvokePropertyChanged(nameof(ToiletsCount));
            InvokePropertyChanged(nameof(IntercomsCount));
            InvokePropertyChanged(nameof(StairwaysCount));
            InvokePropertyChanged(nameof(KitchensCount));
            InvokePropertyChanged(nameof(GalleysCount));
            InvokePropertyChanged(nameof(ServicePointsCount));
            InvokePropertyChanged(nameof(BusinessClassCount));
            InvokePropertyChanged(nameof(EconomyClassCount));
            InvokePropertyChanged(nameof(FirstClassCount));
            InvokePropertyChanged(nameof(PremiumClassCount));
            InvokePropertyChanged(nameof(SupersonicClassCount));
            InvokePropertyChanged(nameof(UnavailableSeatsCount));

            InvokePropertyChanged(nameof(TotalBasicSlots));
            InvokePropertyChanged(nameof(TotalSeatSlots));
            InvokePropertyChanged(nameof(TotalServiceSlots));
        }
    }
}
