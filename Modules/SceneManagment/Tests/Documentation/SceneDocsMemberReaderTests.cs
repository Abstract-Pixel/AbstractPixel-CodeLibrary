using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AbstractPixel.SceneManagement.Documentation.Tests
{
    /// <summary>
    /// The member tables read each member's kind, modifiers, type, parameters and comment, with one row per overload.
    /// </summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public class SceneDocsMemberReaderTests
    {
        // A type with no source file in the module, so everything comes from reflection.
        public class Sample
        {
            public readonly float Speed = 1f;
            public void Do(int _count) { }
            public void Do(string _name) { }
            public int Count { get; private set; }
            public static event Action Changed;

            public static void Raise()
            {
                Changed?.Invoke();
            }
        }

        private static SceneDocsMember Find(List<SceneDocsMember> _members, string _name)
        {
            return _members.Find(_member => _member.Name == _name);
        }

        [Test]
        public void EachOverloadIsItsOwnRow()
        {
            List<SceneDocsMember> members = SceneDocsMemberReader.Read(typeof(Sample));
            List<SceneDocsMember> overloads = members.FindAll(_member => _member.Name == "Do");
            Assert.AreEqual(2, overloads.Count);
            CollectionAssert.AreEquivalent(new[] { "int _count", "string _name" }, overloads.ConvertAll(_member => _member.Parameters));
        }

        [Test]
        public void APropertyShowsItsAccessorsWithTheirAccess()
        {
            SceneDocsMember count = Find(SceneDocsMemberReader.Read(typeof(Sample)), "Count");
            Assert.AreEqual("property", count.Kind);
            Assert.AreEqual("int", count.TypeText);
            Assert.AreEqual("get; private set;", count.Accessors);
        }

        [Test]
        public void ModifiersComeFromReflectionWhenThereIsNoSource()
        {
            List<SceneDocsMember> members = SceneDocsMemberReader.Read(typeof(Sample));
            CollectionAssert.AreEqual(new[] { "public", "readonly" }, Find(members, "Speed").Modifiers);
            CollectionAssert.AreEqual(new[] { "public", "static" }, Find(members, "Changed").Modifiers);
            Assert.AreEqual("event", Find(members, "Changed").Kind);
        }

        [Test]
        public void ChangeToReadsItsSignatureAndComment()
        {
            SceneDocsMember changeTo = Find(SceneDocsMemberReader.Read(typeof(SceneActions)), "ChangeTo");
            Assert.AreEqual("method", changeTo.Kind);
            CollectionAssert.AreEqual(new[] { "public", "static" }, changeTo.Modifiers);
            Assert.AreEqual("Task<SceneChangeResult>", changeTo.TypeText);
            Assert.AreEqual("SceneGroup _sceneGroup", changeTo.Parameters);
            Assert.IsFalse(changeTo.Summary.Contains(". "), "The summary is one sentence: " + changeTo.Summary);
            Assert.Greater(changeTo.Remarks.Count, 0, "The rest of the comment sits in <remarks> as a list");
        }

        [Test]
        public void EveryMemberOfSceneActionsHasAOneLineSummary()
        {
            List<string> problems = new List<string>();
            foreach (SceneDocsMember member in SceneDocsMemberReader.Read(typeof(SceneActions)))
            {
                bool hasOneLine = member.Summary.Length > 0 && !member.Summary.Contains(". ");
                if (!hasOneLine)
                {
                    problems.Add(member.Name + ": '" + member.Summary + "'");
                }
            }
            CollectionAssert.IsEmpty(problems, "Each member needs a one-sentence <summary>; put the rest in <remarks>");
        }

        [Test]
        public void TheTypeDeclarationKeepsItsModifiers()
        {
            Assert.AreEqual("public static class SceneActions", SceneDocsDeclarations.TypeDeclaration(typeof(SceneActions)));
        }

        [Test]
        public void TypeNamesReadAsCSharp()
        {
            Assert.AreEqual("void", SceneDocsMemberReader.FriendlyName(typeof(void)));
            Assert.AreEqual("Task<SceneChangeResult>", SceneDocsMemberReader.FriendlyName(typeof(Task<SceneChangeResult>)));
            Assert.AreEqual("List<int>", SceneDocsMemberReader.FriendlyName(typeof(List<int>)));
            Assert.AreEqual("string[]", SceneDocsMemberReader.FriendlyName(typeof(string[])));
            Assert.AreEqual("Dictionary<string, bool>", SceneDocsMemberReader.FriendlyName(typeof(Dictionary<string, bool>)));
        }
    }
}
