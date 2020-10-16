using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AllocationRerouter
{
    public class OldItem
    {
        public string Number { get; set; }
        public string CatalogNumber { get; set; }

        public string[] ChildrenIds { get; set; }

        public string Prototype { get; set; }
        public string Layout { get; set; }

        public string Location { get; set; }
    }

    public class Allocation
    {
        public string PartId { get; private set; }
        public string[] FlowIds { get; private set; }

        public Allocation(string partId, string[] flowIds)
        {
            PartId = partId;
            FlowIds = flowIds;
        }
    }

    public class Item
    {
        readonly Regex dsRegex = new Regex(@"^DS\s*-.*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string Number { get; set; }

        public Item[] Parents { get; set; }
        public Item[] Children { get; set; }

        public string Location { get; set; }

        private string[] dss = null;
        public string[] GetDSs(bool refreshCache = false)
        {
            if (refreshCache) dss = null;

            if (dss == null)
                dss = (Parents ?? new Item[0]).Select(p =>
                {
                    if (p == null) return null;

                    if (dsRegex.IsMatch(p.Number)) return new[] { p.Number };

                    return p.GetDSs(refreshCache);
                }).Where(s => s != null).SelectMany(s => s).ToArray();

            return dss;
        }

        public IEnumerable<string> GetLookUps(bool refreshCache = false)
        {
            var dsSet = GetDSs(refreshCache);

            return (dsSet == null || dsSet.Length == 0 ? new string[] { null } : dsSet).Select(ds => string.Join(" ", new[] { ds, Number, Location }.Where(s => s != null)));
        }
    }

    public abstract class BasePart
    {
        public string LayoutId { get; set; }
    }

    public class CompoundPart : BasePart
    {
        public string Number { get; set; }

        public string[] ChildrenIds { get; set; }
    }

    public class PartInstance : BasePart
    {
        public string PrototypeId { get; set; }
    }

    /*public class Layout
    {
        public string Location { get; set; }
    }

    public class PartPrototype
    {
        public string CatalogNumber { get; set; }
    }*/
}
