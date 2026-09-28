using System.IO;
using System.Xml;
using UnityEditor.Android;

namespace QuestWorkshop.Editor
{
    /// <summary>
    /// OpenXR 1.13.2's ModifyAndroidManifestMeta unconditionally requests eye tracking.
    /// Quest 3 has no eye tracker and this app never uses it. Explicit manifest merger
    /// rules remove those two declarations without changing the vendor package cache.
    /// </summary>
    public sealed class AndroidManifestPolicy : IPostGenerateGradleAndroidProject
    {
        // Meta's OVRGradleGeneration runs at 99999 and rewrites the same manifest.
        public int callbackOrder => 100000;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string manifestPath = Path.Combine(path, "src/main/AndroidManifest.xml");
            var document = new XmlDocument();
            document.Load(manifestPath);
            var root = document.DocumentElement;
            const string android = "http://schemas.android.com/apk/res/android";
            const string tools = "http://schemas.android.com/tools";
            foreach (var item in new[] {
                (tag: "uses-feature", name: "oculus.software.eye_tracking"),
                (tag: "uses-permission", name: "com.oculus.permission.EYE_TRACKING") })
            {
                foreach (XmlNode existing in root.SelectNodes(item.tag))
                    if (existing.Attributes?["name", android]?.Value == item.name) root.RemoveChild(existing);
                var rule = document.CreateElement(item.tag);
                rule.SetAttribute("name", android, item.name);
                rule.SetAttribute("node", tools, "remove");
                root.AppendChild(rule);
            }
            document.Save(manifestPath);
        }
    }
}
