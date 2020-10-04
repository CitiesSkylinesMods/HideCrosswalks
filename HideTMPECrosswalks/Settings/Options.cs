using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using System.IO;
using HideCrosswalks.Utils; using KianCommons;
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
            Log.Info("Called Options.Load()");
            try {
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

            bool active = HelpersExtensions.InGameOrEditor;
#if DEBUG
            active = true; // Fast test of options from main menu
#endif
            void RefreshPrefabs() {
                if (PrefabUtils.PrefabsLoaded) {
                    NetInfoExt.InitNetInfoExtArray();
                }
            }

            if (active) {
                _ui_never = container.AddUIComponent<UICheckboxDropDownExt>();
                _ui_never.Title = "Except List";
                _ui_never.tooltip = "TMPE cannot hide crosswalks from roads in this list.\nThis list does not affect NS2 junction markings.";
                _ui_never.selectedItems = Split(loaded_never);

                void HandleAfterDropdownClose(UICheckboxDropDown _) => RefreshPrefabs();
                _ui_never.eventAfterDropdownClose += HandleAfterDropdownClose;
                RefreshPrefabs();

                helper.AddButton("Save", Save);
            } else {
                var label = container.AddUIComponent<UILabel>();
                label.text = "Options are only available in game";
            }
        }

        public class UICheckboxDropDownExt : UICheckboxDropDown {
            public string Title = "Title";
            public List<string> selectedItems = new List<string>();

            public override void Awake() {
                base.Awake();
                size = new Vector2(150f, 30);
                verticalAlignment = UIVerticalAlignment.Middle;
                horizontalAlignment = UIHorizontalAlignment.Left;
                builtinKeyNavigation = true;

                popupColor = new Color32(45, 52, 61, 255);
                popupTextColor = new Color32(170, 170, 170, 255);
                atlas = TextureUtils.GetAtlas("InMapEditor");
                uncheckedSprite = "check-unchecked";
                checkedSprite = "check-checked";

                textFieldPadding = new RectOffset(12, 0, 5, 0);
                itemHeight = 25;
                itemHover = "ListItemHover";
                itemHighlight = "ListItemHighlight";
                foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
                itemPadding = new RectOffset(3, 3, 3, 3);

                listBackground = "GenericPanelLight";
                listWidth = 500;
                listHeight = 800;
                clampListToScreen = true;
                listPadding = new RectOffset(5, 5, 5, 5);
                listPosition = UICheckboxDropDown.PopupListPosition.Automatic;

                UIButton button = AddUIComponent<UIButton>();
                triggerButton = button;
                button.textPadding = new RectOffset(2, 21, 0, 0);
                button.size = size;
                button.textVerticalAlignment = UIVerticalAlignment.Middle;
                button.textHorizontalAlignment = UIHorizontalAlignment.Center;
                button.normalFgSprite = "IconDownArrow";
                button.hoveredFgSprite = "IconDownArrowHovered";
                button.pressedFgSprite = "IconDownArrowPressed";
                button.normalBgSprite = "TextFieldPanel";
                button.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
                button.horizontalAlignment = UIHorizontalAlignment.Right;
                button.verticalAlignment = UIVerticalAlignment.Middle;
                button.relativePosition = new Vector3(0, 0);

                // Scrollbar
                listScrollbar = AddUIComponent<UIScrollbar>();
                listScrollbar.height = listHeight;
                listScrollbar.orientation = UIOrientation.Vertical;
                listScrollbar.pivot = UIPivotPoint.TopRight;
                listScrollbar.thumbPadding = new RectOffset(5, 5, 5, 5);
                listScrollbar.minValue = 0;
                listScrollbar.value = 0;
                listScrollbar.incrementAmount = 60;
                listScrollbar.AlignTo(this, UIAlignAnchor.TopRight);
                listScrollbar.autoHide = true;
                listScrollbar.isVisible = false;

                //UISlicedSprite thumbSprite = listScrollbar.AddUIComponent<UISlicedSprite>();
                //thumbSprite.fillDirection = UIFillDirection.Vertical;
                //thumbSprite.autoSize = true;
                //listScrollbar.width = thumbSprite.width = thumbSprite.parent.width = 20;
                //thumbSprite.atlas = TextureUtils.GetAtlas("Ingame");
                //thumbSprite.spriteName = "ScrollbarThumb";
                //listScrollbar.thumbObject = thumbSprite;
            }

            public override void Start() {
                base.Start();
                (triggerButton as UIButton).text = Title;

                eventVisibilityChanged += (UIComponent component, bool bVisble) => { if (bVisble) Populate(); };
                eventDropdownClose +=
                    (UICheckboxDropDown checkboxdropdown, UIScrollablePanel popup, ref bool overridden) =>
                        GetSelections();

                Populate();
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
