using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ICities;
using ColossalFramework;
using ColossalFramework.UI;

using System.IO;
// Add references to Soap and Binary formatters.
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace HideTMPECrosswalks.Settiongs {

    public static class Options {
        public static TF Always = new TF("Always  remove crosswalks:");
        public static TF Never = new TF("Never  remove crosswalks:");

        public static void MakeSettings(UIHelperBase container) {
            Always.AddUI(container);
            Never.AddUI(container);
        }

        public class TF {
            UITextField Element;
            public void AddUI(UIHelperBase container) {
                UIHelper helper = container as UIHelper;
                Element = helper.AddTextfield(Label, DefaultText, null, null) as UITextField;
            }

            readonly string Label;
            readonly string DefaultText;
            public TF(string label, string defaultText = "") {
                Label = label;
                DefaultText = defaultText;
            }

            public static implicit operator string(TF obj) => obj.Element.text;
            public static implicit operator string[](TF obj) => obj.Element.text.Split('|');
        }
    }
}
