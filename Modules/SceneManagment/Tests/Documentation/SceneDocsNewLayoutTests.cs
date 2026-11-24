using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace AbstractPixel.SceneManagement.Documentation.Tests
{
    /// <summary>
    /// The new page layout: the opt-in line, inline code that links to type pages, fact rows, flows and design
    /// decision status.
    /// </summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public class SceneDocsNewLayoutTests
    {
        private static readonly Regex CodePattern = new Regex(@"`([^`]+)`");

        private static DocsPage Parse(string _text)
        {
            return MarkdownParser.Parse(_text, "guide/test.md", SceneDocsDirectives.Known);
        }

        [Test]
        public void TheLayoutLineOptsAPageIn()
        {
            DocsPage page = Parse("# T\n\n<!-- layout new -->\n\nLede.");
            Assert.AreEqual("new", page.Layout);
            Assert.IsFalse(page.Blocks.Exists(_block => _block.Kind == DocsBlockKind.Directive), "The layout line draws nothing");
            Assert.IsNull(Parse("# T\n\nLede.").Layout);
        }

        [Test]
        public void InlineCodeLinksWhenTheResolverKnowsIt()
        {
            string linked = DocsInline.ToRichText("Call `SceneActions` now.", _code => _code == "SceneActions" ? "type:SceneActions" : null);
            StringAssert.Contains("<link=\"type:SceneActions\">", linked);
            StringAssert.DoesNotContain("<link", DocsInline.ToRichText("Call `SceneActions` now."));
            StringAssert.DoesNotContain("<link", DocsInline.ToRichText("Call `Other` now.", _code => null));
        }

        [Test]
        public void CodeSpansResolveToTypesAndMembers()
        {
            Assert.AreEqual("type:SceneActions", SceneDocsTypeIndex.TargetFor("SceneActions"));
            Assert.AreEqual("type:SceneActions.LoadingProgress", SceneDocsTypeIndex.TargetFor("SceneActions.LoadingProgress"));
            Assert.AreEqual("type:SceneActions.ChangeTo", SceneDocsTypeIndex.TargetFor("SceneActions.ChangeTo(group)"));
            Assert.AreEqual("type:ILoadingProgressDisplay.PlayFull", SceneDocsTypeIndex.TargetFor("ILoadingProgressDisplay.PlayFull"));
            Assert.IsNull(SceneDocsTypeIndex.TargetFor("SceneActions.NoSuchMember"));
            Assert.IsNull(SceneDocsTypeIndex.TargetFor("MMProgressBar.UpdateBar01"));
            Assert.IsNull(SceneDocsTypeIndex.TargetFor("Modules/SceneManagment/SceneScreens"));
        }

        [Test]
        public void ATypeLinkOpensTheTypePageAtTheMember()
        {
            SceneDocsHost host = new SceneDocsHost(_topic => { });
            Assert.AreEqual("types/sceneactions", host.TopicOfTarget("type:SceneActions", "guide/loading-scene.md"));
            Assert.AreEqual("types/sceneactions#member-loadingprogress", host.TopicOfTarget("type:SceneActions.LoadingProgress", "guide/loading-scene.md"));
            StringAssert.StartsWith("How far", host.TooltipFor("type:SceneActions.LoadingProgress"));
        }

        [Test]
        public void ABulletListOfLabelledItemsIsFactRows()
        {
            DocsBlock facts = Parse("# T\n\n- **Speed:** how fast.\n- **Curve:** optional.\n").Blocks[0];
            DocsBlock plain = Parse("# T\n\n- **Speed:** how fast.\n- Just a line.\n").Blocks[0];
            Assert.IsTrue(DocsFactRows.Applies(facts));
            Assert.IsFalse(DocsFactRows.Applies(plain));
            string label;
            string value;
            DocsFactRows.Split(facts.Items[0].Text, out label, out value);
            Assert.AreEqual("Speed", label);
            Assert.AreEqual("how fast.", value);
        }

        [Test]
        public void ANumberedListWithNoDoneWhenIsAFlow()
        {
            DocsBlock flow = Parse("# T\n\n1. Hide screen\n2. Show screen\n").Blocks[0];
            DocsBlock steps = Parse("# T\n\n1. Press Play. <!-- done-when OnGameStarted -->\n").Blocks[0];
            Assert.IsTrue(DocsFlow.Applies(flow));
            Assert.IsFalse(DocsFlow.Applies(steps));
        }

        [Test]
        public void ADecisionsStatusComesFromItsText()
        {
            Assert.AreEqual("Accepted", SceneDocsReference.StatusOf("# A\n\nText.\n\n## Consequences\n\nMore."));
            Assert.AreEqual("Amended", SceneDocsReference.StatusOf("# A\n\nText.\n\n## Amended by the 4C grill, 2026-11-21\n"));
        }

        [Test]
        public void EveryCodeSpanNamingADocumentedTypeResolvesOnNewLayoutPages()
        {
            List<string> broken = new List<string>();
            foreach (string topic in SceneDocsReferenceTests.AllTopics())
            {
                DocsPage page = SceneDocsPageCache.Get(topic);
                if (page == null)
                {
                    continue;
                }
                string text = SceneDocsPaths.Read(SceneDocsTopics.FileFor(topic)) ?? "";
                foreach (Match code in CodePattern.Matches(text))
                {
                    string span = code.Groups[1].Value;
                    string typeName = span.Split('.', '(')[0];
                    bool namesADocumentedType = SceneDocsTypeIndex.TargetFor(typeName) != null;
                    if (namesADocumentedType && SceneDocsTypeIndex.TargetFor(span) == null)
                    {
                        broken.Add(topic + ": `" + span + "`");
                    }
                }
            }
            CollectionAssert.IsEmpty(broken, "These code spans name a documented type but a member it does not have");
        }
    }
}
