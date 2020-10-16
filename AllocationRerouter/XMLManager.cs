using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace AllocationRerouter
{
    public static class XMLManager
    {
        public static IEnumerable<KeyValuePair<string, string>> ReadFlows(IEnumerable<string> paths)
        {
            var seenKeys = new HashSet<string>();

            foreach (var path in paths.Distinct())
            {
                using (var xmlReader = XmlReader.Create(path))
                {
                    while (xmlReader.Read())
                    {
                        if (xmlReader.NodeType != XmlNodeType.Element) continue;

                        var name = xmlReader.Name;

                        if (name == "Data" || name == "Objects") continue;

                        if (name != "PmFlow")
                        {
                            xmlReader.Skip();
                            continue;
                        }

                        var e = XElement.Parse(xmlReader.ReadOuterXml());

                        var idAttribute = e.Attribute("ExternalId");
                        var id = idAttribute.Value;

                        if (idAttribute == null) continue;

                        var partId = e.Element("parts")?.Element("item")?.Value;

                        if (partId != null && seenKeys.Add(id))
                        {
                            yield return new KeyValuePair<string, string>(id, partId);
                        }
                    }
                }
            }
        }

        public static Dictionary<string, Item> GetItems(Stream stream)
        {
            var compoundParts = new Dictionary<string, CompoundPart>();
            var partInstances = new Dictionary<string, PartInstance>();
            var catalogNumbers = new Dictionary<string, string>();
            var locations = new Dictionary<string, string>();

            using (var xmlReader = XmlReader.Create(stream))
            {
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType != XmlNodeType.Element) continue;

                    var name = xmlReader.Name;

                    if (name == "Data" || name == "Objects") continue;

                    if (name == "PmCompoundPart" || name == "PmPartInstance" || name == "PmLayout" || name == "PmPartPrototype")
                    {
                        var e = XElement.Parse(xmlReader.ReadOuterXml());

                        var idAttribute = e.Attribute("ExternalId");

                        if (idAttribute == null) continue;

                        var id = idAttribute.Value;

                        switch (name)
                        {
                            case "PmCompoundPart":
                                if (!compoundParts.ContainsKey(id) && !partInstances.ContainsKey(id))
                                {
                                    var number = e.Element("number")?.Value.Trim();

                                    if (!string.IsNullOrEmpty(number))
                                        compoundParts[id] = new CompoundPart()
                                        {
                                            Number = number,
                                            ChildrenIds = e.Element("children")?.Elements("item").Select(ie => ie.Value).ToArray(),
                                            LayoutId = e.Element("layout")?.Value
                                        };
                                }
                                break;

                            case "PmPartInstance":
                                if (!compoundParts.ContainsKey(id) && !partInstances.ContainsKey(id))
                                {
                                    var prototypeId = e.Element("prototype")?.Value;

                                    if (prototypeId != null)
                                        partInstances[id] = new PartInstance()
                                        {
                                            PrototypeId = prototypeId,
                                            LayoutId = e.Element("layout")?.Value
                                        };
                                }
                                break;

                            case "PmLayout":
                                if (!locations.ContainsKey(id))
                                {
                                    var locationValues = e.Element("NodeInfo")?.Element("absoluteLocation")?.Elements().Where(absoluteLocationElement =>
                                    {
                                        switch (absoluteLocationElement.Name.LocalName)
                                        {
                                            case "x":
                                            case "y":
                                            case "z":
                                                return true;

                                            default:
                                                return false;
                                        }
                                    }).Select(ie => ie.Value).ToArray();

                                    if (locationValues != null && locationValues.Length == 3)
                                        locations[id] = string.Join(";", locationValues);
                                }
                                break;

                            case "PmPartPrototype":
                                if (!catalogNumbers.ContainsKey(id))
                                {
                                    var catalogNumber = e.Element("catalogNumber")?.Value.Trim();

                                    if (!string.IsNullOrEmpty(catalogNumber))
                                        catalogNumbers[id] = catalogNumber;
                                }
                                break;
                        }
                    }

                    else
                        xmlReader.Skip();
                }
            }

            var items = partInstances.Keys.Concat(compoundParts.Keys).Select(id => new { id, item = new Item() }).ToDictionary(p => p.id, p => p.item);

            foreach (var pair in items)
            {
                var id = pair.Key;
                var item = pair.Value;

                if (partInstances.ContainsKey(id))
                {
                    var partInstance = partInstances[id];

                    item.Location = locations.TryGetValue(partInstance.LayoutId, out string location) ? location : "0;0;0";
                    item.Number = catalogNumbers.TryGetValue(partInstance.PrototypeId, out string catalogNumber) ? catalogNumber : null;
                }

                else if (compoundParts.ContainsKey(id))
                {
                    var compoundPart = compoundParts[id];

                    item.Location = locations.TryGetValue(compoundPart.LayoutId, out string location) ? location : "0;0;0";
                    item.Number = compoundPart.Number;
                    item.Children = compoundPart.ChildrenIds.Select(cid =>
                    {
                        if (items.TryGetValue(cid, out Item childItem))
                        {
                            childItem.Parents = (childItem.Parents ?? new Item[0]).Append(item).ToArray();
                            return childItem;
                        }

                        return null;
                    }).Where(ci => ci != null).ToArray();
                }
            }

            var itemsWithoutNumber = items.Where(p => p.Value.Number == null);

            foreach (var pair in itemsWithoutNumber)
            {
                items.Remove(pair.Key);
                var itemWithoutNumber = pair.Value;


                foreach (var parent in itemWithoutNumber.Parents ?? new Item[0])
                {
                    parent.Children = parent.Children.Where(c => c != itemWithoutNumber).ToArray();
                }
            }

            return items;
        }
    }
}