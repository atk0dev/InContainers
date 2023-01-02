using System.Collections.Generic;

namespace Globomantics.Core.ClientModel
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; }
        public int NumberOfPointsOfInterest
        {
            get
            {
                return PointsOfInterest.Count;
            }
        }

        public ICollection<PointOfInterest> PointsOfInterest { get; set; }
            = new List<PointOfInterest>();
    }
}
