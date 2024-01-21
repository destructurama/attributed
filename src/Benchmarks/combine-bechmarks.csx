// Copyright 2017 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.IO;
using System.Linq;
using System.Text.Json.Nodes;

string resultsDir = "./BenchmarkDotNet.Artifacts/results";
string resultsFile = "Combined.Benchmarks";
string searchPattern = "*-report-full-compressed.json";

var resultsPath = Path.Combine(resultsDir, resultsFile + ".json");

if (!Directory.Exists(resultsDir))
{
    throw new DirectoryNotFoundException($"Directory not found '{resultsDir}'");
}

if (File.Exists(resultsPath))
{
    File.Delete(resultsPath);
}

var reports = Directory.GetFiles(resultsDir, searchPattern, SearchOption.TopDirectoryOnly).ToArray();
if (!reports.Any())
{
    throw new FileNotFoundException($"Reports not found '{searchPattern}'");
}

var combinedReport = JsonNode.Parse(File.ReadAllText(reports.First()))!;
var title = combinedReport["Title"]!;
var benchmarks = combinedReport["Benchmarks"]!.AsArray();
// Rename title whilst keeping original timestamp
combinedReport["Title"] = $"{resultsFile}{title.GetValue<string>()[^16..]}";

foreach (var report in reports.Skip(1))
{
    var array = JsonNode.Parse(File.ReadAllText(report))!["Benchmarks"]!.AsArray();
    foreach (var benchmark in array)
    {
        // Double parse avoids "The node already has a parent" exception
        benchmarks.Add(JsonNode.Parse(benchmark!.ToJsonString())!);
    }
}

File.WriteAllText(resultsPath, combinedReport.ToString());
