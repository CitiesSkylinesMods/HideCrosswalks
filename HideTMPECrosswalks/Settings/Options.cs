using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using System.IO;
using HideCrosswalks.Utils;
using ICities;
using UnityEngine;

namespace HideCrosswalks.Settings {
    public class Options {
        public static Options instance = null;
        public static readonly char delemiter = '|';
        static readonly string Path = "HideCrosswalks.Config.txt";

        string loaded_never = "";
        UICheckboxDropDownExt _ui_never;
        public List<string> Never => _ui_never.selectedItems;

        public void Load() {
            try{
                string[] s = File.ReadAllLines(Path);
                loaded_never = s[0];
            }catch(FileNotFoundException e) {
                Log.Info("did not found options configuration file: " + e.Message);
            }catch(Exception e) {
                Log.Error("Loading options configuration file failed: " + e);
            }
        }

        public void Save() {
            loaded_never = string.Join(delemiter.ToString(), Never.ToArray());
            File.WriteAllLines(Path, new[] { loaded_never });
        }

        public Options(UIHelperBase helperBase) :this() {
            MakeSettings(helperBase);
        }

        public Options() {
            Load();
            instance = this;
        }

        private List<string> Split(string s) {
            List<string> ret = s.Split(delemiter).ToList();
            ret.RemoveAll((a)=>a=="");
            return ret;
        }

        public void MakeSettings(UIHelperBase helperBase) {
            UIHelper helper = helperBase as UIHelper;
            UIComponent container = helper.self as UIComponent;

            bool active = Extensions.IsActive;
#if DEBUG
            active = true; // Fast test of options from main menu
#endif

            if (active) {
                _ui_never = container.AddUIComponent<UICheckboxDropDownExt>();
                _ui_never.Title = "Except List";
                _ui_never.tooltip = "TMPE cannot hide crosswalks from roads in this list.\nThis list does not affect NS2 junction markings.";
                _ui_never.selectedItems = Split(loaded_never);

                void HandleAfterDropdownClose(UICheckboxDropDown _) => NetInfoExt.InitNetInfoExtArray();
                _ui_never.eventAfterDropdownClose += HandleAfterDropdownClose;
                NetInfoExt.InitNetInfoExtArray();

                helper.AddButton("Save", Save);
            } else {
                var label = container.AddUIComponent<UILabel>();
                label.text = "Options are only available in game";
            }

        }


        public class UICheckboxDropDownExt : UICheckboxDropDown {
            public string Title = "Title";
            public List<string> selectedItems = new List<string>();


            public override void Start() {
                base.Start();
                atlas = TextureUtils.GetAtlas("InMapEditor");
                size = new Vector2(120f, 30);
                listBackground = "GenericPanelLight";
                itemHeight = 10;
                itemHover = "ListItemHover";
                itemHighlight = "ListItemHighlight";
                normalBgSprite = "ButtonMenu";
                disabledBgSprite = "ButtonMenuDisabled";
                hoveredBgSprite = "ButtonMenuHovered";
                focusedBgSprite = "ButtonMenu";
                listWidth = 500;
                listHeight = 1000;
                foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
                popupColor = new Color32(45, 52, 61, 255);
                popupTextColor = new Color32(170, 170, 170, 255);
                zOrder = 1;
                textScale = 0.65f;
                verticalAlignment = UIVerticalAlignment.Middle;
                horizontalAlignment = UIHorizontalAlignment.Left;
                textFieldPadding = new RectOffset(8, 0, 0, 0);
                itemPadding = new RectOffset(3, 3, 3, 3);
                listPadding = new RectOffset(5, 5, 5, 5);
                uncheckedSprite = "check-unchecked";
                checkedSprite = "check-checked";

                UIButton button = AddUIComponent<UIButton>();
                this.triggerButton = button;
                //button.atlas = // TODO uncomment this to change button atlas
                button.text = Title;
                button.textPadding = new RectOffset(14, 0, -7, 0);
                button.size = this.size;
                button.textVerticalAlignment = UIVerticalAlignment.Middle;
                button.textHorizontalAlignment = UIHorizontalAlignment.Left;
                button.normalBgSprite = "ButtonMenu";
                button.disabledBgSprite = "ButtonMenuDisabled";
                button.hoveredBgSprite = "ButtonMenuHovered";
                button.focusedBgSprite = "ButtonMenuFocused";
                button.pressedBgSprite = "ButtonMenuPressed";
                button.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
                button.horizontalAlignment = UIHorizontalAlignment.Right;
                button.verticalAlignment = UIVerticalAlignment.Middle;
                button.zOrder = 0;
                button.textScale = 0.8f;
                button.relativePosition = new Vector3(0, 0);

                eventVisibilityChanged += (UIComponent component, bool bVisble) => { if (bVisble) Populate(); };
                eventDropdownClose +=
                    (UICheckboxDropDown checkboxdropdown, UIScrollablePanel popup, ref bool overridden) =>
                        GetSelections();
            } // end Start()

            public void GetSelections() {
                selectedItems.Clear();
                for (int i = 0; i < items.Length; ++i) {
                    if (GetChecked(i)) {
                        selectedItems.Add(items[i]);
                    }
                }
            }

            public void Populate() {
                Clear();
                foreach (string item in selectedItems) {
                    AddItem(item, true);
                }
                foreach (string item in RoadUtils.GetRoadNames()) {
                    if (!selectedItems.Contains(item)) {
                        AddItem(item, false);
                    }
                }
            }

        } // end sub class
    } // end class
} // end namespace
