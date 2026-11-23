using System.Collections.Generic;
using NUnit.Framework;

namespace AbstractPixel.SceneManagement.Documentation.Tests
{
    /// <summary>The Markdown parser: every element and directive the scene documentation uses.</summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public class MarkdownParserTests
    {
        private static readonly HashSet<string> Known = new HashSet<string> { "level", "action", "done-when", "timeline", "marks", "members" };

        private static DocsPage Parse(string _text)
        {
            return MarkdownParser.Parse(_text, "guide/test.md", Known);
        }

        [Test]
        public void TheTitleTakesItsPill()
        {
            DocsPage page = Parse("# Change scene <!-- level 1 -->\n\nLede.");
            Assert.AreEqual("Change scene", page.Title);
            Assert.AreEqual(1, page.TitlePill);
        }

        [Test]
        public void ASectionHeadingHasAnIdAndAPill()
        {
            DocsPage page = Parse("# T\n\n## Minimum time <!-- level 2 -->\n");
            DocsBlock heading = page.Blocks.Find(_block => _block.Kind == DocsBlockKind.Heading && _block.Level == 2);
            Assert.AreEqual("minimum-time", heading.Id);
            Assert.AreEqual(2, heading.Pill);
            Assert.IsTrue(page.HasHeading("minimum-time"));
        }

        [Test]
        public void TwoHeadingsWithOneNameGetDifferentIds()
        {
            DocsPage page = Parse("# T\n\n## Try it\n\n## Try it\n");
            Assert.AreEqual("try-it", page.Headings[0].Id);
            Assert.AreEqual("try-it-1", page.Headings[1].Id);
        }

        [Test]
        public void LinesOfOneParagraphJoin()
        {
            DocsPage page = Parse("# T\n\nOne line\nand the next.\n\nSecond.");
            List<DocsBlock> paragraphs = page.Blocks.FindAll(_block => _block.Kind == DocsBlockKind.Paragraph);
            Assert.AreEqual(2, paragraphs.Count);
            Assert.AreEqual("One line and the next.", paragraphs[0].Text);
        }

        [Test]
        public void ABulletListNestsTwoLevels()
        {
            DocsPage page = Parse("# T\n\n- One\n  - One a\n- Two\n");
            DocsBlock list = page.Blocks.Find(_block => _block.Kind == DocsBlockKind.List);
            Assert.IsFalse(list.Ordered);
            Assert.AreEqual(2, list.Items.Count);
            Assert.AreEqual("One a", list.Items[0].NestedList.Items[0].Text);
        }

        [Test]
        public void ANumberedStepKeepsItsDoneWhen()
        {
            DocsPage page = Parse("# T\n\n1. Press Play. <!-- done-when OnGameStarted -->\n2. Click it. <!-- done-when screen:Hidden -->\n");
            DocsBlock list = page.Blocks.Find(_block => _block.Kind == DocsBlockKind.List);
            Assert.IsTrue(list.Ordered);
            Assert.AreEqual("Press Play.", list.Items[0].Text);
            Assert.AreEqual("OnGameStarted", list.Items[0].DoneWhen);
            Assert.AreEqual("screen:Hidden", list.Items[1].DoneWhen);
        }

        [Test]
        public void ANumberedListAfterABulletListIsANewList()
        {
            DocsPage page = Parse("# T\n\n- A\n- B\n\n1. One\n2. Two\n");
            List<DocsBlock> lists = page.Blocks.FindAll(_block => _block.Kind == DocsBlockKind.List);
            Assert.AreEqual(2, lists.Count);
            Assert.IsFalse(lists[0].Ordered);
            Assert.IsTrue(lists[1].Ordered);
        }

        [Test]
        public void AnActionOnTheLineUnderAnItemBelongsToTheItem()
        {
            DocsPage page = Parse("# T\n\n1. Select the button.\n   <!-- action select \"Assets/A.asset\" \"Select it\" -->\n2. Next.\n");
            DocsBlock list = page.Blocks.Find(_block => _block.Kind == DocsBlockKind.List);
            Assert.AreEqual(2, list.Items.Count);
            Assert.AreEqual("Select the button.", list.Items[0].Text);
            Assert.AreEqual("action", list.Items[0].Directives[0].Name);
        }

        [Test]
        public void ATableKeepsItsRowsWithoutTheDivider()
        {
            DocsPage page = Parse("# T\n\n| A | B |\n|---|---|\n| 1 | 2 |\n");
            DocsBlock table = page.Blocks.Find(_block => _block.Kind == DocsBlockKind.Table);
            Assert.AreEqual(2, table.Rows.Count);
            Assert.AreEqual("B", table.Rows[0][1]);
            Assert.AreEqual("2", table.Rows[1][1]);
        }

        [TestCase("NOTE", "note")]
        [TestCase("WARNING", "warning")]
        [TestCase("TIP", "tip")]
        public void ACalloutKnowsItsKind(string _marker, string _kind)
        {
            DocsPage page = Parse("# T\n\n> [!" + _marker + "]\n> Body text.\n");
            DocsBlock callout = page.Blocks.Find(_block => _block.Kind == DocsBlockKind.Callout);
            Assert.AreEqual(_kind, callout.Info);
            Assert.AreEqual("Body text.", callout.Children[0].Text);
        }

        [Test]
        public void APlainQuoteIsAQuote()
        {
            DocsPage page = Parse("# T\n\n> Just a quote.\n");
            Assert.IsNotNull(page.Blocks.Find(_block => _block.Kind == DocsBlockKind.Quote));
        }

        [Test]
        public void AFenceKeepsItsInfoAndLines()
        {
            DocsPage page = Parse("# T\n\n```cs source=Scripts/A.cs\nint a = 1;\n\nint b = 2;\n```\n");
            DocsBlock code = page.Blocks.Find(_block => _block.Kind == DocsBlockKind.Code);
            Assert.AreEqual("cs source=Scripts/A.cs", code.Info);
            Assert.AreEqual("int a = 1;\n\nint b = 2;", code.Text);
        }

        [Test]
        public void AFoldoutHoldsItsBlocks()
        {
            DocsPage page = Parse("# T\n\n<details><summary>Why it works</summary>\n\nBecause.\n\n- A\n\n</details>\n");
            DocsBlock details = page.Blocks.Find(_block => _block.Kind == DocsBlockKind.Details);
            Assert.AreEqual("Why it works", details.Title);
            Assert.AreEqual(2, details.Children.Count);
        }

        [Test]
        public void AnActionKeepsItsQuotedArguments()
        {
            DocsPage page = Parse("# T\n\n<!-- action ping \"Assets/My Group.asset\" \"Ping the group\" -->\n");
            DocsBlock directive = page.Blocks.Find(_block => _block.Kind == DocsBlockKind.Directive);
            Assert.AreEqual("action", directive.Directive.Name);
            CollectionAssert.AreEqual(new[] { "ping", "Assets/My Group.asset", "Ping the group" }, directive.Directive.Arguments);
        }

        [Test]
        public void AnUnknownDirectiveIsReportedWithPageAndLine()
        {
            DocsPage page = Parse("# T\n\n<!-- sparkle now -->\n");
            Assert.AreEqual(1, page.Problems.Count);
            StringAssert.Contains("guide/test.md:3", page.Problems[0]);
            StringAssert.Contains("sparkle", page.Problems[0]);
        }

        [Test]
        public void AnImageTakesTheMarksUnderIt()
        {
            DocsPage page = Parse("# T\n\n![The picker](../Images/a.png)\n<!-- marks 1@0.12,0.30 \"Scenes tab\"; 2@0.55,0.3 \"Search\" -->\n");
            DocsBlock image = page.Blocks.Find(_block => _block.Kind == DocsBlockKind.Image);
            Assert.AreEqual("../Images/a.png", image.ImagePath);
            Assert.AreEqual("The picker", image.Text);
            Assert.AreEqual(2, image.Marks.Count);
            Assert.AreEqual(0.55f, image.Marks[1].X, 0.0001f);
            Assert.AreEqual("Search", image.Marks[1].Label);
            Assert.AreEqual(0, page.Problems.Count);
        }

        [TestCase("1@1.2,0.3 \"Off the right\"")]
        [TestCase("1@0.2,-0.1 \"Above the top\"")]
        [TestCase("1@0.2 \"No y\"")]
        public void AMarkOutsideTheImageIsRejected(string _marks)
        {
            DocsPage page = Parse("# T\n\n![A](a.png)\n<!-- marks " + _marks + " -->\n");
            DocsBlock image = page.Blocks.Find(_block => _block.Kind == DocsBlockKind.Image);
            Assert.AreEqual(0, image.Marks.Count);
            Assert.AreEqual(1, page.Problems.Count);
        }

        [TestCase("Minimum time", "minimum-time")]
        [TestCase("Start-up and Play from any scene", "start-up-and-play-from-any-scene")]
        [TestCase("What's \"Reset shared scenes\"?", "whats-reset-shared-scenes")]
        [TestCase("Build Settings & Addressables", "build-settings--addressables")]
        [TestCase("  Trim me  ", "trim-me")]
        public void SlugsFollowGitHub(string _heading, string _slug)
        {
            Assert.AreEqual(_slug, DocsSlug.From(_heading));
        }
    }
}
