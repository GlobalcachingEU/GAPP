using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.Cachebox
{
    public class Attributes
    {
        public enum attr
        {
            Default, Dogs, Access_or_parking_fee, Climbing_gear, Boat, Scuba_gear, Recommended_for_kids, Takes_less_than_an_hour, Scenic_view, Significant_Hike, Difficult_climbing, May_require_wading, May_require_swimming, Available_at_all_times, Recommended_at_night, Available_during_winter, Cactus, Poison_plants, Dangerous_Animals, Ticks, Abandoned_mines, Cliff_falling_rocks, Hunting, Dangerous_area, Wheelchair_accessible, Parking_available, Public_transportation, Drinking_water_nearby, Public_restrooms_nearby, Telephone_nearby, Picnic_tables_nearby, Camping_available, Bicycles, Motorcycles, Quads, Off_road_vehicles, Snowmobiles, Horses, Campfires, Thorns, Stealth_required, Stroller_accessible, Needs_maintenance, Watch_for_livestock, Flashlight_required, Lost_And_Found_Tour, Truck_Driver, Field_Puzzle, UV_Light_Required, Snowshoes, Cross_Country_Skis, Special_Tool_Required, Night_Cache, Park_and_Grab, Abandoned_Structure, Short_hike, Medium_hike, Long_Hike, Fuel_Nearby, Food_Nearby, Wireless_Beacon, Partnership_Cache, Seasonal_Access, Tourist_Friendly, Tree_Climbing, Front_Yard, Teamwork_Required,
        }

        public Attributes(attr at, bool Negative)
        {
            negative = Negative;
            attribute = at;
        }

        public Attributes(attr at)
        {
            attribute = at;
        }

        private bool negative = false;
        public bool Negative
        {
            get { return negative; }
            set { negative = value; }
        }

        private Attributes.attr attribute;
        public Attributes.attr Attribute
        {
            get { return attribute; }
            set { attribute = value; }
        }

        public static DLong GetAttributeDlong(Attributes.attr attrib)
        {
            if (attributeLookup == null)
                ini();

            int Id = 0;
            attributeLookup.TryGetValue(attrib, out Id);

            return DLong.shift(Id);

        }

        public static Attributes getAttributeEnumByGcComId(int id)
        {
            switch (id)
            {
                case 1:
                    return new Attributes(Attributes.attr.Dogs);
                case 2:
                    return new Attributes(Attributes.attr.Access_or_parking_fee);
                case 3:
                    return new Attributes(Attributes.attr.Climbing_gear);
                case 4:
                    return new Attributes(Attributes.attr.Boat);
                case 5:
                    return new Attributes(Attributes.attr.Scuba_gear);
                case 6:
                    return new Attributes(Attributes.attr.Recommended_for_kids);
                case 7:
                    return new Attributes(Attributes.attr.Takes_less_than_an_hour);
                case 8:
                    return new Attributes(Attributes.attr.Scenic_view);
                case 9:
                    return new Attributes(Attributes.attr.Significant_Hike);
                case 10:
                    return new Attributes(Attributes.attr.Difficult_climbing);
                case 11:
                    return new Attributes(Attributes.attr.May_require_wading);
                case 12:
                    return new Attributes(Attributes.attr.May_require_swimming);
                case 13:
                    return new Attributes(Attributes.attr.Available_at_all_times);
                case 14:
                    return new Attributes(Attributes.attr.Recommended_at_night);
                case 15:
                    return new Attributes(Attributes.attr.Available_during_winter);
                case 16:
                    return new Attributes(Attributes.attr.Cactus);
                case 17:
                    return new Attributes(Attributes.attr.Poison_plants);
                case 18:
                    return new Attributes(Attributes.attr.Dangerous_Animals);
                case 19:
                    return new Attributes(Attributes.attr.Ticks);
                case 20:
                    return new Attributes(Attributes.attr.Abandoned_mines);
                case 21:
                    return new Attributes(Attributes.attr.Cliff_falling_rocks);
                case 22:
                    return new Attributes(Attributes.attr.Hunting);
                case 23:
                    return new Attributes(Attributes.attr.Dangerous_area);
                case 24:
                    return new Attributes(Attributes.attr.Wheelchair_accessible);
                case 25:
                    return new Attributes(Attributes.attr.Parking_available);
                case 26:
                    return new Attributes(Attributes.attr.Public_transportation);
                case 27:
                    return new Attributes(Attributes.attr.Drinking_water_nearby);
                case 28:
                    return new Attributes(Attributes.attr.Public_restrooms_nearby);
                case 29:
                    return new Attributes(Attributes.attr.Telephone_nearby);
                case 30:
                    return new Attributes(Attributes.attr.Picnic_tables_nearby);
                case 31:
                    return new Attributes(Attributes.attr.Camping_available);
                case 32:
                    return new Attributes(Attributes.attr.Bicycles);
                case 33:
                    return new Attributes(Attributes.attr.Motorcycles);
                case 34:
                    return new Attributes(Attributes.attr.Quads);
                case 35:
                    return new Attributes(Attributes.attr.Off_road_vehicles);
                case 36:
                    return new Attributes(Attributes.attr.Snowmobiles);
                case 37:
                    return new Attributes(Attributes.attr.Horses);
                case 38:
                    return new Attributes(Attributes.attr.Campfires);
                case 39:
                    return new Attributes(Attributes.attr.Thorns);
                case 40:
                    return new Attributes(Attributes.attr.Stealth_required);
                case 41:
                    return new Attributes(Attributes.attr.Stroller_accessible);
                case 42:
                    return new Attributes(Attributes.attr.Needs_maintenance);
                case 43:
                    return new Attributes(Attributes.attr.Watch_for_livestock);
                case 44:
                    return new Attributes(Attributes.attr.Flashlight_required);
                case 45:
                    return new Attributes(Attributes.attr.Lost_And_Found_Tour);
                case 46:
                    return new Attributes(Attributes.attr.Truck_Driver);
                case 47:
                    return new Attributes(Attributes.attr.Field_Puzzle);
                case 48:
                    return new Attributes(Attributes.attr.UV_Light_Required);
                case 49:
                    return new Attributes(Attributes.attr.Snowshoes);
                case 50:
                    return new Attributes(Attributes.attr.Cross_Country_Skis);
                case 51:
                    return new Attributes(Attributes.attr.Special_Tool_Required);
                case 52:
                    return new Attributes(Attributes.attr.Night_Cache);
                case 53:
                    return new Attributes(Attributes.attr.Park_and_Grab);
                case 54:
                    return new Attributes(Attributes.attr.Abandoned_Structure);
                case 55:
                    return new Attributes(Attributes.attr.Short_hike);
                case 56:
                    return new Attributes(Attributes.attr.Medium_hike);
                case 57:
                    return new Attributes(Attributes.attr.Long_Hike);
                case 58:
                    return new Attributes(Attributes.attr.Fuel_Nearby);
                case 59:
                    return new Attributes(Attributes.attr.Food_Nearby);
                case 60:
                    return new Attributes(Attributes.attr.Wireless_Beacon);
                case 61:
                    return new Attributes(Attributes.attr.Partnership_Cache);
                case 62:
                    return new Attributes(Attributes.attr.Seasonal_Access);
                case 63:
                    return new Attributes(Attributes.attr.Tourist_Friendly);
                case 64:
                    return new Attributes(Attributes.attr.Tree_Climbing);
                case 65:
                    return new Attributes(Attributes.attr.Front_Yard);
                case 66:
                    return new Attributes(Attributes.attr.Teamwork_Required);
            }
            return new Attributes(Attributes.attr.Default);
        }

        public static Dictionary<Attributes.attr, int> attributeLookup;
        public static void ini()
        {
            attributeLookup = new Dictionary<Attributes.attr, int>();
            attributeLookup.Add(Attributes.attr.Default, 0);
            attributeLookup.Add(Attributes.attr.Dogs, 1);
            attributeLookup.Add(Attributes.attr.Access_or_parking_fee, 2);
            attributeLookup.Add(Attributes.attr.Climbing_gear, 3);
            attributeLookup.Add(Attributes.attr.Boat, 4);
            attributeLookup.Add(Attributes.attr.Scuba_gear, 5);
            attributeLookup.Add(Attributes.attr.Recommended_for_kids, 6);
            attributeLookup.Add(Attributes.attr.Takes_less_than_an_hour, 7);
            attributeLookup.Add(Attributes.attr.Scenic_view, 8);
            attributeLookup.Add(Attributes.attr.Significant_Hike, 9);
            attributeLookup.Add(Attributes.attr.Difficult_climbing, 10);
            attributeLookup.Add(Attributes.attr.May_require_wading, 11);
            attributeLookup.Add(Attributes.attr.May_require_swimming, 12);
            attributeLookup.Add(Attributes.attr.Available_at_all_times, 13);
            attributeLookup.Add(Attributes.attr.Recommended_at_night, 14);
            attributeLookup.Add(Attributes.attr.Available_during_winter, 15);
            attributeLookup.Add(Attributes.attr.Cactus, 16);
            attributeLookup.Add(Attributes.attr.Poison_plants, 17);
            attributeLookup.Add(Attributes.attr.Dangerous_Animals, 18);
            attributeLookup.Add(Attributes.attr.Ticks, 19);
            attributeLookup.Add(Attributes.attr.Abandoned_mines, 20);
            attributeLookup.Add(Attributes.attr.Cliff_falling_rocks, 21);
            attributeLookup.Add(Attributes.attr.Hunting, 22);
            attributeLookup.Add(Attributes.attr.Dangerous_area, 23);
            attributeLookup.Add(Attributes.attr.Wheelchair_accessible, 24);
            attributeLookup.Add(Attributes.attr.Parking_available, 25);
            attributeLookup.Add(Attributes.attr.Public_transportation, 26);
            attributeLookup.Add(Attributes.attr.Drinking_water_nearby, 27);
            attributeLookup.Add(Attributes.attr.Public_restrooms_nearby, 28);
            attributeLookup.Add(Attributes.attr.Telephone_nearby, 29);
            attributeLookup.Add(Attributes.attr.Picnic_tables_nearby, 30);
            attributeLookup.Add(Attributes.attr.Camping_available, 31);
            attributeLookup.Add(Attributes.attr.Bicycles, 32);
            attributeLookup.Add(Attributes.attr.Motorcycles, 33);
            attributeLookup.Add(Attributes.attr.Quads, 34);
            attributeLookup.Add(Attributes.attr.Off_road_vehicles, 35);
            attributeLookup.Add(Attributes.attr.Snowmobiles, 36);
            attributeLookup.Add(Attributes.attr.Horses, 37);
            attributeLookup.Add(Attributes.attr.Campfires, 38);
            attributeLookup.Add(Attributes.attr.Thorns, 39);
            attributeLookup.Add(Attributes.attr.Stealth_required, 40);
            attributeLookup.Add(Attributes.attr.Stroller_accessible, 41);
            attributeLookup.Add(Attributes.attr.Needs_maintenance, 42);
            attributeLookup.Add(Attributes.attr.Watch_for_livestock, 43);
            attributeLookup.Add(Attributes.attr.Flashlight_required, 44);
            attributeLookup.Add(Attributes.attr.Lost_And_Found_Tour, 45);
            attributeLookup.Add(Attributes.attr.Truck_Driver, 46);
            attributeLookup.Add(Attributes.attr.Field_Puzzle, 47);
            attributeLookup.Add(Attributes.attr.UV_Light_Required, 48);
            attributeLookup.Add(Attributes.attr.Snowshoes, 49);
            attributeLookup.Add(Attributes.attr.Cross_Country_Skis, 50);
            attributeLookup.Add(Attributes.attr.Special_Tool_Required, 51);
            attributeLookup.Add(Attributes.attr.Night_Cache, 52);
            attributeLookup.Add(Attributes.attr.Park_and_Grab, 53);
            attributeLookup.Add(Attributes.attr.Abandoned_Structure, 54);
            attributeLookup.Add(Attributes.attr.Short_hike, 55);
            attributeLookup.Add(Attributes.attr.Medium_hike, 56);
            attributeLookup.Add(Attributes.attr.Long_Hike, 57);
            attributeLookup.Add(Attributes.attr.Fuel_Nearby, 58);
            attributeLookup.Add(Attributes.attr.Food_Nearby, 59);
            attributeLookup.Add(Attributes.attr.Wireless_Beacon, 60);
            attributeLookup.Add(Attributes.attr.Partnership_Cache, 61);
            attributeLookup.Add(Attributes.attr.Seasonal_Access, 62);
            attributeLookup.Add(Attributes.attr.Tourist_Friendly, 63);
            attributeLookup.Add(Attributes.attr.Tree_Climbing, 64);
            attributeLookup.Add(Attributes.attr.Front_Yard, 65);
            attributeLookup.Add(Attributes.attr.Teamwork_Required, 66);
        }

        public static List<Attributes> getAttributes(DLong attributesPositive, DLong attributesNegative)
        {
            List<Attributes> ret = new List<Attributes>();
            if (attributesPositive == null || attributesNegative == null)
            {
                return ret;
            }
            if (attributeLookup == null) ini();
            foreach (Attributes.attr attribute in attributeLookup.Keys)
            {
                DLong att = Attributes.GetAttributeDlong(attribute);
                if ((att.BitAndBiggerNull(attributesPositive)))
                {
                    ret.Add(new Attributes(attribute, false));
                }
            }


            foreach (Attributes.attr attribute in attributeLookup.Keys)
            {
                DLong att = Attributes.GetAttributeDlong(attribute);
                if ((att.BitAndBiggerNull(attributesNegative)))
                {
                    ret.Add(new Attributes(attribute, true));
                }
            }

            return ret;
        }

        public String getImageName()
        {
            if (attributeLookup == null) ini();

            int Id = 0;
            attributeLookup.TryGetValue(this.attribute, out Id);

            String ret = "att_" + Id.ToString();

            if (negative)
            {
                ret += "_0";
            }
            else
            {
                ret += "_1";
            }
            return ret;
        }
    }
}
