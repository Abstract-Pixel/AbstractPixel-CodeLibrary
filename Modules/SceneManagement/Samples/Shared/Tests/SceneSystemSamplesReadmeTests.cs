using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace AbstractPixel.SceneManagement.Samples.Tests
{
    /// <summary>
    /// Every sample README follows the template (scene system S, section 6), its code blocks are copied from the
    /// scripts, and the Samples/README.md index lists every sample and covers features F01 to F33.
    /// </summary>
    public class SceneSystemSamplesReadmeTests
    {
        private const int FeatureCount = 33;

        private static readonly string[] Headings =
        {
            "## What you need",
            "## Files",
            "## Try it",
            "## You should see",
            "## The lines that matter",
            "## Windows this sample shows",
            "## How it's set up",
            "## Why it works",
            "## If it goes wrong",
            "## Next steps"
        };

        private static readonly Regex ScriptPathLine = new Regex("^`(Scripts/[^`]+\\.cs)`");
        private static readonly Regex CoverageRow = new Regex("^\\|\\s*(F[0-9]{2})\\s*\\|([^|]*)\\|([^|]*)\\|");

        public static IEnumerable<string> ExpectedSamples()
        {
            return SampleFolders.ExpectedSamples;
        }

        [TestCaseSource(nameof(ExpectedSamples))]
        public void TheReadmeHasTheTemplateHeadingsInOrder(string _sampleFolder)
        {
            string[] lines = ReadReadme(_sampleFolder);

            Assert.IsTrue(lines[0].StartsWith("# " + _sampleFolder), _sampleFolder + ": the first line must be '# " + _sampleFolder + "'");
            int searchFrom = 0;
            foreach (string heading in Headings)
            {
                int found = IndexOfLine(lines, heading, searchFrom);
                Assert.GreaterOrEqual(found, 0, _sampleFolder + ": heading '" + heading + "' is missing or out of order");
                searchFrom = found + 1;
            }
        }

        [TestCaseSource(nameof(ExpectedSamples))]
        public void TheCodeBlocksAreCopiedFromTheScripts(string _sampleFolder)
        {
            string[] lines = ReadReadme(_sampleFolder);
            int sectionStart = IndexOfLine(lines, "## The lines that matter", 0);
            int sectionEnd = IndexOfLine(lines, "## Windows this sample shows", sectionStart + 1);
            Assert.GreaterOrEqual(sectionStart, 0, _sampleFolder + ": no 'The lines that matter' section");

            string lastScriptPath = null;
            for (int index = sectionStart + 1; index < sectionEnd; index++)
            {
                string line = lines[index].Trim();
                Match pathMatch = ScriptPathLine.Match(line);
                if (pathMatch.Success)
                {
                    lastScriptPath = pathMatch.Groups[1].Value;
                    continue;
                }
                if (line.StartsWith("```") == false)
                {
                    continue;
                }
                List<string> block = ReadBlock(lines, index + 1);
                Assert.IsNotNull(lastScriptPath, _sampleFolder + ": a code block at line " + (index + 1) + " has no `Scripts/...cs` line before it");
                AssertBlockIsInScript(_sampleFolder, lastScriptPath, block);
                index += block.Count + 1;
                lastScriptPath = null;
            }
        }

        [TestCaseSource(nameof(ExpectedSamples))]
        public void TheIndexListsTheSample(string _sampleFolder)
        {
            string index = File.ReadAllText(IndexPath());

            StringAssert.Contains(_sampleFolder, index, "Samples/README.md does not list " + _sampleFolder);
        }

        [Test]
        public void TheCoverageTableHasExactlyF01ToF33WithAnExistingSample()
        {
            Assert.IsTrue(File.Exists(IndexPath()), "Samples/README.md is missing");
            List<string> ids = new List<string>();
            foreach (string line in File.ReadAllLines(IndexPath()))
            {
                Match row = CoverageRow.Match(line.Trim());
                if (row.Success == false)
                {
                    continue;
                }
                string id = row.Groups[1].Value;
                string feature = row.Groups[2].Value.Trim();
                string sampleNumber = row.Groups[3].Value.Trim();
                ids.Add(id);
                string sampleFolder = FolderWithNumber(sampleNumber);
                Assert.IsNotNull(sampleFolder, id + ": sample '" + sampleNumber + "' does not exist");
                string readme = File.ReadAllText(ReadmePath(sampleFolder)).ToLowerInvariant();
                StringAssert.Contains(feature.ToLowerInvariant(), readme, id + ": the README of " + sampleFolder + " does not name '" + feature + "'");
            }

            List<string> expected = new List<string>();
            for (int number = 1; number <= FeatureCount; number++)
            {
                expected.Add("F" + number.ToString("00"));
            }
            CollectionAssert.AreEqual(expected, ids, "the coverage table must list F01 to F33 once each, in order");
        }

        private static void AssertBlockIsInScript(string _sampleFolder, string _scriptPath, List<string> _block)
        {
            string scriptFullPath = Path.Combine(SampleFolders.SamplesFullPath(), _sampleFolder, _scriptPath);
            Assert.IsTrue(File.Exists(scriptFullPath), _sampleFolder + ": " + _scriptPath + " does not exist");
            string[] scriptLines = File.ReadAllLines(scriptFullPath);
            bool found = ContainsRun(scriptLines, _block);

            Assert.IsTrue(found, _sampleFolder + ": a code block is not copied verbatim from " + _scriptPath + ":\n" + string.Join("\n", _block));
        }

        // True when the trimmed block lines appear one after another in the trimmed script lines.
        private static bool ContainsRun(string[] _scriptLines, List<string> _block)
        {
            if (_block.Count == 0)
            {
                return false;
            }
            for (int start = 0; start + _block.Count <= _scriptLines.Length; start++)
            {
                bool allMatch = true;
                for (int offset = 0; offset < _block.Count; offset++)
                {
                    if (_scriptLines[start + offset].Trim() != _block[offset].Trim())
                    {
                        allMatch = false;
                        break;
                    }
                }
                if (allMatch)
                {
                    return true;
                }
            }
            return false;
        }

        private static List<string> ReadBlock(string[] _lines, int _firstLine)
        {
            List<string> block = new List<string>();
            for (int index = _firstLine; index < _lines.Length; index++)
            {
                if (_lines[index].Trim().StartsWith("```"))
                {
                    break;
                }
                block.Add(_lines[index]);
            }
            return block;
        }

        private static int IndexOfLine(string[] _lines, string _line, int _from)
        {
            for (int index = _from; index < _lines.Length; index++)
            {
                if (_lines[index].Trim() == _line)
                {
                    return index;
                }
            }
            return -1;
        }

        private static string[] ReadReadme(string _sampleFolder)
        {
            string path = ReadmePath(_sampleFolder);
            Assert.IsTrue(File.Exists(path), _sampleFolder + " has no README.md");
            return File.ReadAllLines(path);
        }

        private static string ReadmePath(string _sampleFolder)
        {
            return Path.Combine(SampleFolders.SamplesFullPath(), _sampleFolder, "README.md");
        }

        private static string IndexPath()
        {
            return Path.Combine(SampleFolders.SamplesFullPath(), "README.md");
        }

        private static string FolderWithNumber(string _number)
        {
            foreach (string folder in SampleFolders.NumberedSampleFolders())
            {
                if (folder.StartsWith(_number + " "))
                {
                    return folder;
                }
            }
            return null;
        }
    }
}
