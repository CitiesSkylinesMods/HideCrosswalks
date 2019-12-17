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

    [Serializable]
    public class Options {
        public static TF Always;
        public static TF Never;

        public static IFormatter Formatter => new BinaryFormatter();
        public static string FileName => "HideTMPECrosswalks.xml";

        public Options() {

        }

        public static Options Create(UIHelperBase container) {
            FileStream stream = new FileStream(FileName, FileMode.Open);
            Formatter.Deserialize(s);
        }

        public void Save() {
            FileStream stream = new FileStream(FileName, FileMode.Create);
            Formatter.Serialize(stream, this);
        }
        public void MakeSettings(UIHelperBase container) {
            Always.AddUI(container);
            Never.AddUI(container);
            container.AddButton("Save", Save);
        }

        [Serializable]
        public class TFAlways : TF, ISerializable {
            public TFAlways() : base() { }
            public override string Label => "Always remove crosswalks:";
            public override string Key => "tf always";

            public void GetObjectData(SerializationInfo info, StreamingContext c) {
                info.AddValue(Key, Element.text, typeof(string));
            }
            public TFAlways(SerializationInfo info, StreamingContext _) : this() {
                Element.text = (string)info.GetValue(Key, typeof(string));
            }

        }

        [Serializable]
        public class TFNever : TF, ISerializable {
            public TFNever() : base() { }
            public override string Label => "Never remove crosswalks:";
            public override string Key => "tf never";

            // TODO move this to base class?
            public void GetObjectData(SerializationInfo info, StreamingContext c) {
                info.AddValue(Key, Element.text, typeof(string));
            }
            public TFNever(SerializationInfo info, StreamingContext _) : this() {
                Element.text = (string)info.GetValue(Key, typeof(string));
            }

        }

        public abstract class TF {
            protected UITextField Element;
            public void AddUI(UIHelperBase container) {
                UIHelper helper = container as UIHelper;
                Element = helper.AddTextfield(Label, DefaultText, null, null) as UITextField;
            }

            public abstract string Label { get; }
            public virtual string DefaultText => "";
            public abstract string Key { get; }


            public TF() { }

            public static implicit operator string(TF obj) => obj.Element.text;
            public static implicit operator string[](TF obj) => obj.Element.text.Split('|');
        }
    }
}
