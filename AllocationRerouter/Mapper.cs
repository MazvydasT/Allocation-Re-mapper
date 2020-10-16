using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Shell;
using System.Xml;

namespace AllocationRerouter
{
    public static class Mapper
    {
        public static Exception Map(List<string> operationPaths, List<string> oldEBOMPaths, List<string> newEBOMPaths, Stream outputStream)
        {
            if (operationPaths == null || operationPaths.Count < 1)
                return new Exception("No operation files provided.");

            if (oldEBOMPaths == null || oldEBOMPaths.Count < 1)
                return new Exception("No old EBOM files provided.");

            if (newEBOMPaths == null || newEBOMPaths.Count < 1)
                return new Exception("No new EBOM files provided.");

            if (outputStream == null)
                return new Exception("No output stream provided.");

            if (!outputStream.CanWrite)
                return new Exception("Provided output stream is not writable.");

            var stats = MappingStats.Stats;
            stats.Reset();

            var progress = ProgressTracker.Progress;

            progress.Max = operationPaths.Count + oldEBOMPaths.Count + newEBOMPaths.Count;
            progress.Value = 0;
            progress.State = TaskbarItemProgressState.Normal;

            Dictionary<string, string[]> flowIdsByPartId;

            try
            {
                flowIdsByPartId = XMLManager.ReadFlows(operationPaths)
                    .Select(p =>
                    {
                        stats.Flows++;
                        return p;
                    })
                    .GroupBy(p => p.Value, p => p.Key)
                    .ToDictionary(g => g.Key, g => g.ToArray());
            }

            catch (Exception e)
            {
                return e;
            }

            progress.Value += operationPaths.Count;

            var allocationsByLookup = new Dictionary<string, Allocation[]>(flowIdsByPartId.Count);

            foreach (var path in oldEBOMPaths)
            {
                Dictionary<string, Item> items;

                using (var stream = File.OpenRead(path))
                {
                    try
                    {
                        items = XMLManager.GetItems(stream);
                    }

                    catch (Exception e)
                    {
                        return e;
                    }
                }

                foreach (var pair in items)
                {
                    var id = pair.Key;

                    if (!flowIdsByPartId.ContainsKey(id)) continue;

                    var flowIds = flowIdsByPartId[id];
                    flowIdsByPartId.Remove(id);

                    stats.FlowsWithIdentifiedParts += flowIds.Length;

                    var item = pair.Value;

                    foreach (var lookUp in item.GetLookUps())
                    {
                        if (!allocationsByLookup.ContainsKey(lookUp))
                            allocationsByLookup[lookUp] = new[] { new Allocation(id, flowIds) };

                        else
                            allocationsByLookup[lookUp] = allocationsByLookup[lookUp].Append(new Allocation(id, flowIds)).ToArray();
                    }

                    if (flowIdsByPartId.Count == 0)
                        goto AllFlowIdsHaveLookups;
                }

                progress.Value++;
            }

        AllFlowIdsHaveLookups:

            //stats.FlowsWithUnidentifiedParts = flowIdsByPartId.Values.SelectMany(s => s).Count();

            progress.Value = operationPaths.Count + oldEBOMPaths.Count;

            var moreThanOneMatch = new List<string>();
            var partIdHasNotChanged = new List<string>();

            using (var xmlWriter = XmlWriter.Create(outputStream, new XmlWriterSettings() { Indent = true, IndentChars = "\t" }))
            {
                xmlWriter.WriteStartDocument(true);

                xmlWriter.WriteStartElement("Data");
                xmlWriter.WriteStartElement("Objects");

                foreach (var path in newEBOMPaths)
                {
                    Dictionary<string, Item> items;

                    using (var stream = File.OpenRead(path))
                    {
                        try
                        {
                            items = XMLManager.GetItems(stream);
                        }

                        catch (Exception e)
                        {
                            return e;
                        }
                    }

                    var partIdsByLookup = items.AsParallel().Select(p => p.Value.GetLookUps().Select(l => new { Lookup = l, Id = p.Key })).SelectMany(s => s).ToLookup(p => p.Lookup, p => p.Id);

                    foreach (var partIdGroup in partIdsByLookup)
                    {
                        var lookUp = partIdGroup.Key;
                        var partIds = partIdGroup.ToArray();

                        if (!allocationsByLookup.ContainsKey(lookUp)) continue;

                        var allocations = allocationsByLookup[lookUp];
                        allocationsByLookup.Remove(lookUp);

                        if (partIds.Length > 1)
                        {
                            var flowIdsWithMoreThanOneMatch = allocations.Select(a => a.FlowIds).SelectMany(s => s).ToList();

                            stats.FlowsWithMultipleMatches += flowIdsWithMoreThanOneMatch.Count;

                            moreThanOneMatch.AddRange(flowIdsWithMoreThanOneMatch);

                            continue;
                        }

                        var newPartId = partIds[0];

                        foreach (var allocation in allocations)
                        {
                            if (allocation.PartId == newPartId)
                            {
                                partIdHasNotChanged.AddRange(allocation.FlowIds);

                                stats.FlowsWithUnchangedPartIds += allocation.FlowIds.Length;
                            }

                            else
                                foreach (var flowId in allocation.FlowIds)
                                {
                                    stats.FlowsWithSingleMatch++;

                                    xmlWriter.WriteStartElement("PmFlow");
                                    xmlWriter.WriteAttributeString("ExternalId", flowId);

                                    xmlWriter.WriteStartElement("parts");

                                    xmlWriter.WriteElementString("item", newPartId);

                                    xmlWriter.WriteEndElement();

                                    xmlWriter.WriteEndElement();
                                }
                        }

                        if (allocationsByLookup.Count == 0)
                            goto AllFlowsHaveNewPartIds;
                    }

                    progress.Value++;
                }

                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
            }

        AllFlowsHaveNewPartIds:

            progress.Value = progress.Max;

            //var noMatch = allocationsByLookup.Values.SelectMany(a => a).SelectMany(a => a.FlowIds).ToList();

            //stats.FlowsWithNoMatches = noMatch.Count;

            return null;
        }
    }
}
