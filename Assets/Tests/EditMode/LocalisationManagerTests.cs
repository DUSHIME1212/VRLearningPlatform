using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using VRLearning.Core;

namespace VRLearning.Tests.EditMode
{
    public class LocalisationManagerTests
    {
        private static readonly PropertyInfo InstanceProperty =
            typeof(LocalisationManager).GetProperty(nameof(LocalisationManager.Instance));

        private GameObject _go;
        private LocalisationManager _loc;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("FakeLocalisationManager");
            _loc = _go.AddComponent<LocalisationManager>();

            // AppBootstrap's [RuntimeInitializeOnLoadMethod] instantiates a real, DontDestroyOnLoad
            // "Managers" LocalisationManager whenever a scene load has occurred (e.g. an Editor
            // session that entered Play Mode at some point). If that instance already holds the
            // static Instance slot, Awake()'s self-claim check would self-destruct our fake instead.
            // Force it directly so these tests are hermetic regardless of what else is loaded.
            InstanceProperty.SetValue(null, _loc);
        }

        [TearDown]
        public void TearDown()
        {
            InstanceProperty.SetValue(null, null);
            if (_go != null) Object.DestroyImmediate(_go);
        }

        [Test]
        public void Get_UnknownKey_ReturnsBracketedKeyAsFallback()
        {
            Assert.AreEqual("[some_missing_key]", _loc.Get("some_missing_key"));
        }

        [Test]
        public void SetLanguage_SameLanguageAsCurrent_IsANoOp()
        {
            bool fired = false;
            _loc.OnLanguageChanged += () => fired = true;

            _loc.SetLanguage(_loc.CurrentLanguage);

            Assert.IsFalse(fired, "Setting the already-active language must not raise OnLanguageChanged.");
        }

        [Test]
        public void SetLanguage_DifferentLanguage_FiresChangedEventAndUpdatesCurrentLanguage()
        {
            Language other = _loc.CurrentLanguage == Language.English ? Language.Kinyarwanda : Language.English;
            bool fired = false;
            _loc.OnLanguageChanged += () => fired = true;

            _loc.SetLanguage(other);

            Assert.IsTrue(fired);
            Assert.AreEqual(other, _loc.CurrentLanguage);
        }
    }
}
