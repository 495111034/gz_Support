using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MyButton = UnityEngine.UI.MyButton;

namespace UnityEditor.UI
{
    static internal class MenuOptions2
    {
        private const string kUILayerName = "UI";
        private const float kWidth = 160f;
        private const float kThickHeight = 30f;
        private const float kThinHeight = 20f;

        private const string kStandardSpritePath = "UI/Skin/UISprite.psd";
        private const string kBackgroundSpriteResourcePath = "UI/Skin/Background.psd";
        private const string kInputFieldBackgroundPath = "UI/Skin/InputFieldBackground.psd";
        private const string kKnobPath = "UI/Skin/Knob.psd";
        private const string kCheckmarkPath = "UI/Skin/Checkmark.psd";
        private const string kDropdownArrowPath = "UI/Skin/DropdownArrow.psd";
        private const string kMaskPath = "UI/Skin/UIMask.psd";

        private static Vector2 s_ThickGUIElementSize = new Vector2(kWidth, kThickHeight);
        private static Vector2 s_ThinGUIElementSize = new Vector2(kWidth, kThinHeight);
        private static Vector2 s_ImageGUIElementSize = new Vector2(100f, 100f);
        private static Color s_DefaultSelectableColor = new Color(1f, 1f, 1f, 1f);
        private static Vector2 s_ThickElementSize = new Vector2(kWidth, kThickHeight);
        private static Vector2 s_ThinElementSize = new Vector2(kWidth, kThinHeight);
      

        [MenuItem("GameObject/UI/MySpriteImage", false, 998)]
        public static void CreateMySpriteImage(MenuCommand menuCommand)
        {
            GameObject go = CreateUIElementRoot("MySprite", menuCommand, s_ImageGUIElementSize);
            go.AddComponent<MySpriteImage>();
            go.GetComponent<MySpriteImage>().raycastTarget = false;
            go.GetComponent<MySpriteImage>().NoTexShow = false;
            go.GetComponent<MySpriteImage>().color = s_DefaultSelectableColor;            
        }
        [MenuItem("GameObject/UI/PolygonImage(多边形)", false, 999)]
        public static void CreatePolygonImage(MenuCommand menuCommand)
        {
            GameObject go = CreateUIElementRoot("PolygonImage", menuCommand, s_ImageGUIElementSize);
            go.AddComponent<PolygonImage>();
            go.GetComponent<PolygonImage>().raycastTarget = false;
            go.GetComponent<PolygonImage>().NoTexShow = true;
        }
        [MenuItem("GameObject/UI/MyAnnularSlider", false, 1000)]
        static public void MyAnnularSlider(MenuCommand menuCommand)
        {
            GameObject go = CreateUIElementRoot("MyAnnularSlider", menuCommand, s_ImageGUIElementSize);
            MyAnnularSlider myslider = go.AddComponent<MyAnnularSlider>();
            MyImage image= go.AddComponent<MyImage>();
            myslider.FillImage = image;
        }

        [MenuItem("GameObject/UI/Image", false, 1000)]
        static public void AddMyImage(MenuCommand menuCommand)
        {
            GameObject go = CreateUIElementRoot("MyImage", menuCommand, s_ImageGUIElementSize);
            go.AddComponent<MyImage>();
            go.GetComponent<MyImage>().raycastTarget = false;
        }


        [MenuItem("GameObject/UI/Text", false, 1001)]
        static public void AddText(MenuCommand menuCommand)
        {
            GameObject go = CreateUIElementRoot("MyText", menuCommand, s_ThickGUIElementSize);

            MyText lbl = go.AddComponent<MyText>();
            lbl.text = "New Text";
            SetDefaultTextValues(lbl);
            lbl.raycastTarget = false;           
            lbl.supportRichText = false;
        }

        private static void SetDefaultTextValues(MyText lbl)
        {
            lbl.color = Color.white;
            lbl.font =  AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/fonts/arial.otf");
            //Log.LogError(lbl.font);
            lbl.raycastTarget = false;
        }



        [MenuItem("GameObject/UI/Raw Image", false, 1002)]
        static public void AddRawImage(MenuCommand menuCommand)
        {
            GameObject go = CreateUIElementRoot("MyRawImage", menuCommand, s_ImageGUIElementSize);
            go.AddComponent<MyRawImage>();
            go.GetComponent<MyRawImage>().raycastTarget = false;
        }

        [MenuItem("GameObject/UI/Button", false, 1003)]
        static public void AddButton(MenuCommand menuCommand)
        {
            GameObject buttonRoot = CreateUIElementRoot("MyButton", menuCommand, new Vector2(129,51));

            GameObject childText = new GameObject("MyText");
            GameObjectUtility.SetParentAndAlign(childText, buttonRoot);

            MySpriteImage image = buttonRoot.AddComponent<MySpriteImage>();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{PathDefs.ASSETS_PATH_GUI_SPRITES}common/button.png");    //AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpriteResourcePath);
            image.SetSprite(sprite,null);
            image.color = s_DefaultSelectableColor;
            image.raycastTarget = true;
            image.NoTexShow = true;

            UnityEngine.UI.MyButton bt = buttonRoot.AddComponent<UnityEngine.UI.MyButton>();
            SetDefaultColorTransitionValues(bt);

            MyText text = childText.AddComponent<MyImageText>();
            text.text = "按钮标题";
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.196f, 0.196f, 0.196f);
            text.raycastTarget = false;
            text.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/fonts/arial.otf");

            RectTransform textRectTransform = childText.GetComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.sizeDelta = Vector2.zero;
        }

        [MenuItem("GameObject/UI/MyToggle", false, 1004)]
        static public void AddToggle(MenuCommand menuCommand)
        {
            // Set up hierarchy
            GameObject toggleRoot = CreateUIElementRoot("MyToggle", menuCommand, new Vector2(104,44));

            GameObject background = CreateUIObject("Background", toggleRoot);
            GameObject trueObj = CreateUIObject("trueObj", toggleRoot);
            GameObject falseObj = CreateUIObject("falseObj", toggleRoot);
            GameObject trueLable = CreateUIObject("trueLable", trueObj);
            GameObject falseLable = CreateUIObject("falseLable", falseObj);

            // Set up components
            MyToggle toggle = toggleRoot.AddComponent<MyToggle>();
            toggle.isOn = true;

            EmptyImage bgImage = background.AddComponent<EmptyImage>();
            bgImage.raycastTarget = true;            

            MySpriteImage checkmarkImage = trueObj.AddComponent<MySpriteImage>();
            var sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpriteResourcePath);
            checkmarkImage.SetSprite(sprite,null);
            checkmarkImage.raycastTarget = false;            

            MyText trueLbl = trueLable.AddComponent<MyImageText>();
            trueLbl.text = "已选";
            trueLbl.fontSize = 14;
            trueLbl.alignment = TextAnchor.UpperLeft;
            trueLbl.AutoSize = true;
            trueLbl.MaxWidth = 104;
            SetDefaultTextValues(trueLbl);
            trueLbl.color = Color.white;
            trueLbl.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/fonts/arial.otf");


            MySpriteImage falseImg = falseObj.AddComponent<MySpriteImage>();
            sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpriteResourcePath);
            falseImg.SetSprite(sprite, null);
            falseImg.raycastTarget = false;

            MyText falseLbl = falseLable.AddComponent<MyImageText>();
            falseLbl.text = "未选";
            falseLbl.fontSize = 14;
            falseLbl.AutoSize = true;
            falseLbl.MaxWidth = 104;
            falseLbl.alignment = TextAnchor.UpperLeft;           
            SetDefaultTextValues(falseLbl);
            falseLbl.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/fonts/arial.otf");

            toggle.graphic = trueObj;
            toggle.falseGraphic = falseObj;
            toggle.targetGraphic = bgImage;
            SetDefaultColorTransitionValues(toggle);

            RectTransform bgRect = bgImage.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.5f, 0.5f);
            bgRect.anchorMax = new Vector2(0.5f, 0.5f);
            bgRect.anchoredPosition = Vector2.zero;
            bgRect.sizeDelta = new Vector2(104, 44);

            RectTransform checkmarkRect = trueObj.GetComponent<RectTransform>();
            checkmarkRect.anchorMin = new Vector2(0.5f, 0.5f);
            checkmarkRect.anchorMax = new Vector2(0.5f, 0.5f);
            checkmarkRect.anchoredPosition = Vector2.zero;
            checkmarkRect.sizeDelta = new Vector2(104, 44);

            RectTransform labelRect = trueLbl.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = new Vector2(104, 44);

            checkmarkRect = falseObj.GetComponent<RectTransform>();
            checkmarkRect.anchorMin = new Vector2(0.5f, 0.5f);
            checkmarkRect.anchorMax = new Vector2(0.5f, 0.5f);
            checkmarkRect.anchoredPosition = Vector2.zero;
            checkmarkRect.sizeDelta = new Vector2(104, 44);

            labelRect = falseLbl.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = new Vector2(104, 44);
        }

        [MenuItem("GameObject/UI/Slider", false, 1005)]
        static public void AddSlider(MenuCommand menuCommand)
        {
            // Create GOs Hierarchy
            GameObject root = CreateUIElementRoot("MySlider", menuCommand, s_ThinGUIElementSize);

            GameObject background = CreateUIObject("Background", root);
            GameObject fillArea = CreateUIObject("Fill Area", root);
            GameObject fill = CreateUIObject("Fill", fillArea);
            GameObject handleArea = CreateUIObject("Handle Slide Area", root);
            GameObject handle = CreateUIObject("Handle", handleArea);

            // Background
            MyImage backgroundImage = background.AddComponent<MyImage>();
            backgroundImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpriteResourcePath);
            backgroundImage.type = Image.Type.Sliced;
            backgroundImage.color = s_DefaultSelectableColor;
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0, 0.25f);
            backgroundRect.anchorMax = new Vector2(1, 0.75f);
            backgroundRect.sizeDelta = new Vector2(0, 0);

            // Fill Area
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1, 0.75f);
            fillAreaRect.anchoredPosition = new Vector2(-5, 0);
            fillAreaRect.sizeDelta = new Vector2(-20, 0);

            // Fill
            MyImage fillImage = fill.AddComponent<MyImage>();
            fillImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(kStandardSpritePath);
            fillImage.type = Image.Type.Sliced;
            fillImage.color = s_DefaultSelectableColor;

            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.sizeDelta = new Vector2(10, 0);

            // Handle Area
            RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.sizeDelta = new Vector2(-20, 0);
            handleAreaRect.anchorMin = new Vector2(0, 0);
            handleAreaRect.anchorMax = new Vector2(1, 1);

            // Handle
            MyImage handleImage = handle.AddComponent<MyImage>();
            handleImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(kKnobPath);
            handleImage.color = s_DefaultSelectableColor;

            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 0);

            // Setup slider component
            MySlider slider = root.AddComponent<MySlider>();
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            SetDefaultColorTransitionValues(slider);
        }

        [MenuItem("GameObject/UI/Input Field", false, 1006)]
        public static void AddInputField(MenuCommand menuCommand)
        {
            GameObject root = CreateUIElementRoot("MyInputField", menuCommand, s_ThickGUIElementSize);

            GameObject childPlaceholder = CreateUIObject("Placeholder", root);
            GameObject childText = CreateUIObject("MyText", root);

            MySpriteImage image = root.AddComponent<MySpriteImage>();
            image.SetSprite(AssetDatabase.LoadAssetAtPath<Sprite>($"{PathDefs.ASSETS_PATH_GUI_SPRITES}common/case.png"),null);//AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpriteResourcePath);
            image.color = s_DefaultSelectableColor;
            image.raycastTarget = true;


            MyInputField inputField = root.AddComponent<MyInputField>();
            SetDefaultColorTransitionValues(inputField);

            MyImageText text = childText.AddComponent<MyImageText>();
            text.text = "";
            text.supportRichText = false;
            text.alignment = TextAnchor.UpperLeft;
            text.raycastTarget = false;            
            SetDefaultTextValues(text);
            text.color = Color.white;

            MyImageText placeholder = childPlaceholder.AddComponent<MyImageText>();
            //placeholder.SetLanguageID("common_pleaseinput");
            placeholder.alignment = TextAnchor.UpperLeft;
            placeholder.fontStyle = FontStyle.Italic;            
            Color placeholderColor = text.color;
            placeholderColor.a *= 0.5f;
            placeholder.color = placeholderColor;
            placeholder.raycastTarget = false;
            placeholder.supportRichText = false;
            SetDefaultTextValues(placeholder);

            RectTransform textRectTransform = childText.GetComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.sizeDelta = Vector2.zero;
            textRectTransform.offsetMin = new Vector2(10, 6);
            textRectTransform.offsetMax = new Vector2(-10, -7);

            RectTransform placeholderRectTransform = childPlaceholder.GetComponent<RectTransform>();
            placeholderRectTransform.anchorMin = Vector2.zero;
            placeholderRectTransform.anchorMax = Vector2.one;
            placeholderRectTransform.sizeDelta = Vector2.zero;
            placeholderRectTransform.offsetMin = new Vector2(10, 6);
            placeholderRectTransform.offsetMax = new Vector2(-10, -7);

            inputField.textComponent = text;
            inputField.placeholder = placeholder;
        }

        [MenuItem("GameObject/UI/Scroll View", false, 1019)]
        static public void AddScrollView(MenuCommand menuCommand)
        {
            GameObject root = CreateUIElementRoot("ScrollRect", menuCommand, new Vector2(200, 200));

            GameObject viewport = CreateUIObject("MyViewport", root);
            GameObject content = CreateUIObject("Content", viewport);


            // Setup RectTransforms.

            // Make viewport fill entire scroll view.
            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.sizeDelta = Vector2.zero;
            viewportRT.pivot = Vector2.up;

            // Make context match viewpoprt width and be somewhat taller.
            // This will show the vertical scrollbar and not the horizontal one.
            RectTransform contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = Vector2.up;
            contentRT.anchorMax = Vector2.one;
            contentRT.sizeDelta = new Vector2(0, 300);
            contentRT.pivot = Vector2.up;

            // Setup UI components.

            ScrollRect scrollRect = root.AddComponent<ScrollRect>();
            scrollRect.content = contentRT;
            scrollRect.viewport = viewportRT;
            scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.horizontalScrollbarSpacing = -3;
            scrollRect.verticalScrollbarSpacing = -3;

            MySpriteImage rootImage = root.AddComponent<MySpriteImage>();
            rootImage.NoTexShow = true;
            rootImage.color = new Color(0, 0, 0, 0.4f);


            Mask viewportMask = viewport.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;
            rootImage.NoTexShow = true;

            MySpriteImage viewportImage = viewport.AddComponent<MySpriteImage>();
            viewportImage.NoTexShow = true;
        }

        [MenuItem("GameObject/UI/Dropdown", false, 1007)]
        static public void AddDropdown(MenuCommand menuCommand)
        {
            GameObject root = CreateUIElementRoot("My Dropdown", menuCommand, s_ThickElementSize);

            GameObject label = CreateUIObject("Label", root);
            GameObject arrow = CreateUIObject("Arrow", root);
            GameObject template = CreateUIObject("Template", root);
            GameObject viewport = CreateUIObject("Viewport", template);
            GameObject content = CreateUIObject("Content", viewport);
            GameObject item = CreateUIObject("Item", content);
            GameObject itemBackground = CreateUIObject("Item Background", item);
            GameObject itemCheckmark = CreateUIObject("Item Checkmark", item);
            GameObject itemLabel = CreateUIObject("Item Label", item);

            // Sub controls.           

            // Setup item UI components.

            MyImageText itemLabelText = itemLabel.AddComponent<MyImageText>();
            SetDefaultTextValues(itemLabelText);
            itemLabelText.alignment = TextAnchor.MiddleLeft;

            MySpriteImage itemBackgroundImage = itemBackground.AddComponent<MySpriteImage>();
            itemBackgroundImage.color = new Color32(245, 245, 245, 255);
            itemBackgroundImage.NoTexShow = true;
            itemBackgroundImage.color = new Color(0, 0, 0, 0.4f);

            MySpriteImage itemCheckmarkImage = itemCheckmark.AddComponent<MySpriteImage>();
            itemCheckmarkImage.color = s_DefaultSelectableColor;
            itemCheckmarkImage.NoTexShow = true;
            itemCheckmarkImage.SetSprite(AssetDatabase.LoadAssetAtPath<Sprite>($"{PathDefs.ASSETS_PATH_GUI_SPRITES}common/check.png"),null);//AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpriteResourcePath);

            MyToggle itemToggle = item.AddComponent<MyToggle>();
            itemToggle.targetGraphic = itemBackgroundImage;
            itemToggle.graphic = itemCheckmarkImage.gameObject;
            itemToggle.isOn = true;

            // Setup template UI components.

            MySpriteImage templateImage = template.AddComponent<MySpriteImage>();
            templateImage.color = s_DefaultSelectableColor;
            templateImage.NoTexShow = true;
            templateImage.color = new Color(0, 0, 0, 0.4f);


            Mask scrollRectMask = viewport.AddComponent<Mask>();
            scrollRectMask.showMaskGraphic = false;
            MySpriteImage viewportImage = viewport.AddComponent<MySpriteImage>();
            viewportImage.color = s_DefaultSelectableColor;
            viewportImage.NoTexShow = true;

            // Setup dropdown UI components.

            MyImageText labelText = label.AddComponent<MyImageText>();
            SetDefaultTextValues(labelText);
            labelText.text = "选项 A";
            labelText.alignment = TextAnchor.MiddleLeft;

            MySpriteImage arrowImage = arrow.AddComponent<MySpriteImage>();
            arrowImage.SetSprite(AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpriteResourcePath),null);

            MySpriteImage backgroundImage = root.AddComponent<MySpriteImage>();
            backgroundImage.NoTexShow = true;
            backgroundImage.color = s_DefaultSelectableColor;
            backgroundImage.SetSprite(AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpriteResourcePath),null);

            MyDropdown dropdown = root.AddComponent<MyDropdown>();
            dropdown.targetGraphic = backgroundImage;
            SetDefaultColorTransitionValues(dropdown);
            dropdown.template = template.GetComponent<RectTransform>();
            dropdown.captionText = labelText;
            dropdown.itemText = itemLabelText;

            // Setting default Item list.
            itemLabelText.text = "选项 A";
            dropdown.UseLangugaeID = true;
            dropdown.LanguageList = new List<string>() { };

            // Set up RectTransforms.

            RectTransform labelRT = label.GetComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(10, 6);
            labelRT.offsetMax = new Vector2(-25, -7);

            RectTransform arrowRT = arrow.GetComponent<RectTransform>();
            arrowRT.anchorMin = new Vector2(1, 0.5f);
            arrowRT.anchorMax = new Vector2(1, 0.5f);
            arrowRT.sizeDelta = new Vector2(20, 20);
            arrowRT.anchoredPosition = new Vector2(-15, 0);

            RectTransform templateRT = template.GetComponent<RectTransform>();
            templateRT.anchorMin = new Vector2(0, 0);
            templateRT.anchorMax = new Vector2(1, 0);
            templateRT.pivot = new Vector2(0.5f, 1);
            templateRT.anchoredPosition = new Vector2(0, 2);
            templateRT.sizeDelta = new Vector2(0, 150);

            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            viewportRT.anchorMin = new Vector2(0, 0);
            viewportRT.anchorMax = new Vector2(1, 1);
            viewportRT.sizeDelta = new Vector2(0, 0);
            viewportRT.pivot = new Vector2(0, 1);

            RectTransform contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0f, 1);
            contentRT.anchorMax = new Vector2(1f, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.anchoredPosition = new Vector2(0, 0);
            contentRT.sizeDelta = new Vector2(0, 28);

            RectTransform itemRT = item.GetComponent<RectTransform>();
            itemRT.anchorMin = new Vector2(0, 0.5f);
            itemRT.anchorMax = new Vector2(1, 0.5f);
            itemRT.sizeDelta = new Vector2(0, 20);

            RectTransform itemBackgroundRT = itemBackground.GetComponent<RectTransform>();
            itemBackgroundRT.anchorMin = Vector2.zero;
            itemBackgroundRT.anchorMax = Vector2.one;
            itemBackgroundRT.sizeDelta = Vector2.zero;

            RectTransform itemCheckmarkRT = itemCheckmark.GetComponent<RectTransform>();
            itemCheckmarkRT.anchorMin = new Vector2(0, 0.5f);
            itemCheckmarkRT.anchorMax = new Vector2(0, 0.5f);
            itemCheckmarkRT.sizeDelta = new Vector2(20, 20);
            itemCheckmarkRT.anchoredPosition = new Vector2(10, 0);

            RectTransform itemLabelRT = itemLabel.GetComponent<RectTransform>();
            itemLabelRT.anchorMin = Vector2.zero;
            itemLabelRT.anchorMax = Vector2.one;
            itemLabelRT.offsetMin = new Vector2(20, 1);
            itemLabelRT.offsetMax = new Vector2(0, -2);

            template.SetActive(false);
            
        }

        [MenuItem("GameObject/UI/My3DRoomImage", false, 1009)]
        static public void Add3DRoomImage(MenuCommand menuCommand)
        {
            GameObject go = CreateUIElementRoot("My3DRoomImage", menuCommand, s_ImageGUIElementSize);
            go.AddComponent<RawImage>();
            go.AddComponent<My3DRoomImage>();
        }

        [MenuItem("GameObject/UI/Canvas", false, 1008)]
        static public void AddCanvas(MenuCommand menuCommand)
        {
            var go = CreateNewUI();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            if (go.transform.parent as RectTransform)
            {
                RectTransform rect = go.transform as RectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = Vector2.zero;
            }
            Selection.activeGameObject = go;
        }

        [MenuItem("GameObject/UI/Panel", false, 1009)]
        static public void AddPanel(MenuCommand menuCommand)
        {
            GameObject panelRoot = CreateUIElementRoot("Panel", menuCommand, s_ThickElementSize);

            // Set RectTransform to stretch
            RectTransform rectTransform = panelRoot.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;

            MySpriteImage image = panelRoot.AddComponent<MySpriteImage>();
            image.SetSprite(AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpriteResourcePath),null);
            image.color = Color.white;
            image.raycastTarget = true;
            image.NoTexShow = true;

            // Panel is special, we need to ensure there's no padding after repositioning.
            RectTransform rect = panelRoot.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }


        [MenuItem("GameObject/UI/Event System", false, 1010)]
        public static void CreateEventSystem(MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            CreateEventSystem(true, parent);
        }

        [MenuItem("GameObject/UI/3d UI Object", false, 1011)]
        public static void Create3Dobj(MenuCommand menuCommand)
        {
            GameObject go = CreateUIElementRoot("MyUI3dObject", menuCommand, s_ImageGUIElementSize);
            go.AddComponent<MyUI3DObject>();          
        }

        [MenuItem("GameObject/UI/Empty Image(空图)", false, 1012)]
        public static void CreateEmptyImage(MenuCommand menuCommand)
        {
            GameObject go = CreateUIElementRoot("EmptyImage", menuCommand, s_ImageGUIElementSize);
            go.AddComponent<EmptyImage>();
            go.GetComponent<EmptyImage>().raycastTarget = false;
        }


        [MenuItem("GameObject/UI/MulitImageSilder(多层进度条)", false, 1013)]
        public static void CreateMultiImageSlider(MenuCommand menuCommand)
        {
            GameObject go = CreateUIElementRoot("MulitImageSilder", menuCommand, new Vector2(100,19));
            go.AddComponent<MyMultiImageSlider>();
            go.GetComponent<MyMultiImageSlider>().raycastTarget = false;
            go.GetComponent<MyMultiImageSlider>().SetSpritePacker(AssetDatabase.LoadAssetAtPath<MySpritePacker>($"{PathDefs.PREFAB_PATH_UI_PACKERS}bosshead/bosshead.prefab"),null);
            go.GetComponent<MyMultiImageSlider>().SetSpriteList(new List<string>() { "blood_02" });
            go.GetComponent<MyMultiImageSlider>().BackGroupImageID = "blood_case_02";
            go.GetComponent<MyMultiImageSlider>().BGImageOffset = new RectOffset(1, 1, 2, 2);
        }

        [MenuItem("GameObject/UI/MySpriteFrameSeq(帧动画)", false, 1014)]
        public static void CreateMySpriteFrameSeq(MenuCommand menuCommand)
        {
            GameObject go = CreateUIElementRoot("MySpriteFrameSeq", menuCommand, s_ImageGUIElementSize);
            go.AddComponent<MySpriteFrameSeq>();
            go.GetComponent<MySpriteFrameSeq>().raycastTarget = false;
            go.GetComponent<MySpriteFrameSeq>().UseAllFrame = false;
            go.GetComponent<MySpriteFrameSeq>().CompleteEvent = false;
            go.GetComponent<MySpriteFrameSeq>().AutoSize = false;
            go.GetComponent<MySpriteFrameSeq>().SetSpritePacker(AssetDatabase.LoadAssetAtPath<MySpritePacker>($"{PathDefs.PREFAB_PATH_UI_PACKERS}guinumber/guinumber.prefab"),null);
            go.GetComponent<MySpriteFrameSeq>().SetSpriteList(new List<string>() { "gold_01_0", "gold_01_1", "gold_01_2", "gold_01_3", "gold_01_4", "gold_01_5" });
        }

        [MenuItem("GameObject/UI/SpriteList(图片列表)", false, 1015)]
        public static void CreateSpriteList(MenuCommand menuCommand)
        {
            GameObject go = CreateUIElementRoot("SpriteList", menuCommand, s_ImageGUIElementSize);
            go.AddComponent<SpriteList>();
            go.GetComponent<SpriteList>().raycastTarget = false;
            go.GetComponent<SpriteList>().SetSpritePacker(AssetDatabase.LoadAssetAtPath<MySpritePacker>($"{PathDefs.PREFAB_PATH_UI_PACKERS}guinumber/guinumber.prefab"),null);
            go.GetComponent<SpriteList>().SetSpriteList(new List<string>() { "gold_01_0", "gold_01_1", "gold_01_2", "gold_01_3", "gold_01_4", "gold_01_5" });
        }
        [MenuItem("GameObject/UI/MyImageText(图文混排)", false, 1016)]
        public static void CreateMyImageText(MenuCommand menuCommand)
        {
            GameObject go = CreateUIElementRoot("ImageText", menuCommand, new Vector2(500,250));
            go.AddComponent<MyImageText>();
            go.GetComponent<MyImageText>().raycastTarget = false;
            go.GetComponent<MyImageText>().text = $"这是图文混排的文字," +
                $"<quad name=activity/ac_commmon_rank size={go.GetComponent<MyImageText>().fontSize} anim=0 />呵呵," +
                $"<a href=mytest111>我是可点的</a>,<color=red>我是可变色的</color><size=50>大号字</size><i>斜体</i><b>粗体</b>,<quad name=guinumber size=40 anim=1 />" +
                $"，再多的空格都只显示一个，          ,猜猜前面有几个空格" +
                $"，&nbsp&nbsp&nbsp&nbsp&nbsp,多个空格请用转义符" +
                $"<br/>换行也要用转义符";
        }

        [MenuItem("GameObject/UI/环形部局", false, 1020)]
        static public void AddCircleRange(MenuCommand menuCommand)
        {
            GameObject root = CreateUIElementRoot("My Circle LayoutGroup", menuCommand, new Vector2(200, 200));
            GameObject viewport = CreateUIObject("MyViewport", root);
            GameObject content = CreateUIObject("Content", viewport);

            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            viewportRT.sizeDelta = new Vector2(200, 200);

            RectTransform contentRT = content.GetComponent<RectTransform>();           
            contentRT.sizeDelta = new Vector2(200, 200);          


            CircleLayoutGroup circleRect = root.AddComponent<CircleLayoutGroup>();
            circleRect.Content = content.GetRectTransform();          

            MySpriteImage rootImage = root.AddComponent<MySpriteImage>();
            rootImage.NoTexShow = true;
            rootImage.color = new Color(0, 0, 0, 0.4f);


            Mask viewportMask = viewport.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;
            rootImage.NoTexShow = true;

            MySpriteImage viewportImage = viewport.AddComponent<MySpriteImage>();
            viewportImage.NoTexShow = true;
            viewportImage.raycastTarget = true;
        }

        [MenuItem("GameObject/UI/弹出式菜单", false, 1021)]
        static public void AddMenu(MenuCommand menuCommand)
        {
            GameObject root = CreateUIElementRoot("MyMenu", menuCommand, new Vector2(10,10));

            GameObject template = CreateUIObject("Template", root);
            GameObject viewport = CreateUIObject("Viewport", template);
            GameObject content = CreateUIObject("Content", viewport);
            GameObject item = CreateUIObject("menu_btn", content);
            GameObject itemBackground = CreateUIObject("menu_btnBackground", item);           
            GameObject itemLabel = CreateUIObject("menu_btnLabel", item);


            MySpriteImage templateImage = template.AddComponent<MySpriteImage>();
            templateImage.color = s_DefaultSelectableColor;
            templateImage.NoTexShow = true;
            templateImage.color = new Color(0, 0, 0, 0.4f);

            Mask scrollRectMask = viewport.AddComponent<Mask>();
            scrollRectMask.showMaskGraphic = false;

            MySpriteImage viewportImage = viewport.AddComponent<MySpriteImage>();
            viewportImage.color = s_DefaultSelectableColor;
            viewportImage.NoTexShow = true;



            MyImageText itemLabelText = itemLabel.AddComponent<MyImageText>();
            SetDefaultTextValues(itemLabelText);
            itemLabelText.alignment = TextAnchor.MiddleCenter;
            itemLabelText.text = "测试标签";

            MySpriteImage itemBackgroundImage = itemBackground.AddComponent<MySpriteImage>();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{PathDefs.ASSETS_PATH_GUI_SPRITES}common/common_bg_btn_02.png");//AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpriteResourcePath);
            itemBackgroundImage.SetSprite(sprite,null);
            itemBackgroundImage.color = s_DefaultSelectableColor;
            itemBackgroundImage.raycastTarget = true;            

            MyButton itemBtn = item.AddComponent<MyButton>();
            itemBtn.targetGraphic = itemBackgroundImage;


            var menu = root.AddComponent<MyMenu>();
            RectTransform templateRT = menu.template = template.GetComponent<RectTransform>();

            //RectTransform templateRT = template.GetComponent<RectTransform>();
            templateRT.anchorMin = new Vector2(0, 0);
            templateRT.anchorMax = new Vector2(0, 0);
            templateRT.pivot = new Vector2(0.5f, 1f);
            templateRT.anchoredPosition = new Vector2(0, 0);
            templateRT.sizeDelta = new Vector2(100, 150);

            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            viewportRT.anchorMin = new Vector2(0, 0);
            viewportRT.anchorMax = new Vector2(1, 1);           
            viewportRT.pivot = new Vector2(0, 1);
            viewportRT.SetSize(new Vector2(100, 140));
            viewportRT.anchoredPosition = new Vector2(0, -5);

            RectTransform contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0f, 1);
            contentRT.anchorMax = new Vector2(1f, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.anchoredPosition = new Vector2(0, 0);
            contentRT.sizeDelta = new Vector2(0, 50);

            RectTransform itemRT = item.GetComponent<RectTransform>();
            itemRT.anchorMin = new Vector2(0, 0.5f);
            itemRT.anchorMax = new Vector2(1, 0.5f);
            itemRT.SetSize(new Vector2(90, 50));

            RectTransform itemBackgroundRT = itemBackground.GetComponent<RectTransform>();
            itemBackgroundRT.anchorMin = Vector2.zero;
            itemBackgroundRT.anchorMax = Vector2.one;
            itemBackgroundRT.sizeDelta = Vector2.zero;         

            RectTransform itemLabelRT = itemLabel.GetComponent<RectTransform>();
            itemLabelRT.anchorMin = Vector2.zero;
            itemLabelRT.anchorMax = Vector2.one;
            itemLabelRT.offsetMin = new Vector2(0, 0);
            itemLabelRT.offsetMax = new Vector2(0, 0);


            menu.UseLangugaeID = true;
            menu.LanguageList = new List<string>() { "btn_ok", "btn_confirm", "btn_cancel" };

        }

        [MenuItem("GameObject/UI/MyWareImage（摆动的图片）", false, 1022)]
        public static void CreateMyWareImage(MenuCommand menuCommand)
        {
            GameObject go = CreateUIElementRoot("MyWareSprite", menuCommand, s_ImageGUIElementSize);
            go.AddComponent<MyWareImage>();
            go.GetComponent<MyWareImage>().raycastTarget = false;          
            go.GetComponent<MyWareImage>().color = s_DefaultSelectableColor;           
        }

        private static void CreateEventSystem(bool select)
        {
            CreateEventSystem(select, null);
        }
       

        public static GameObject CreateScrollbar(MenuCommand menuCommand)
        {
            // Create GOs Hierarchy
            GameObject scrollbarRoot = CreateUIElementRoot("Scrollbar", menuCommand, s_ThinElementSize);

            GameObject sliderArea = CreateUIObject("Sliding Area", scrollbarRoot);
            GameObject handle = CreateUIObject("Handle", sliderArea);

            MySpriteImage bgImage = scrollbarRoot.AddComponent<MySpriteImage>();
            bgImage.SetSprite(AssetDatabase.LoadAssetAtPath<Sprite>($"{PathDefs.ASSETS_PATH_GUI_SPRITES}common/case_c_01.png"),null);
            bgImage.color = s_DefaultSelectableColor;
            bgImage.NoTexShow = true;
            bgImage.raycastTarget = true;

            MySpriteImage handleImage = handle.AddComponent<MySpriteImage>();
            handleImage.SetSprite(AssetDatabase.LoadAssetAtPath<Sprite>($"{PathDefs.ASSETS_PATH_GUI_SPRITES}common/case_c_02.png"),null);
            handleImage.color = s_DefaultSelectableColor;
            handleImage.NoTexShow = true;
            handleImage.raycastTarget = true;

            RectTransform sliderAreaRect = sliderArea.GetComponent<RectTransform>();
            sliderAreaRect.sizeDelta = new Vector2(-20, -20);
            sliderAreaRect.anchorMin = Vector2.zero;
            sliderAreaRect.anchorMax = Vector2.one;

            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 20);

            Scrollbar scrollbar = scrollbarRoot.AddComponent<Scrollbar>();
            scrollbar.handleRect = handleRect;
            scrollbar.targetGraphic = handleImage;
            SetDefaultColorTransitionValues(scrollbar);

            return scrollbarRoot;
        }


        private static GameObject CreateUIElementRoot(string name, MenuCommand menuCommand, Vector2 size)
        {
            GameObject parent = menuCommand.context as GameObject;
            if (parent == null || FindInParents<Canvas>(parent) == null)
            {
                parent = GetParentActiveCanvasInSelection(true);
            }
            GameObject child = new GameObject(name);

            Undo.RegisterCreatedObjectUndo(child, "Create " + name);
            Undo.SetTransformParent(child.transform, parent.transform, "Parent " + child.name);
            GameObjectUtility.SetParentAndAlign(child, parent);

            RectTransform rectTransform = child.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            if (parent != menuCommand.context) // not a context click, so center in sceneview
            {
                SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(), rectTransform);
            }
            Selection.activeGameObject = child;
            return child;
        }

        private static void SetDefaultColorTransitionValues(Selectable slider)
        {
            ColorBlock colors = slider.colors;
            colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
            colors.pressedColor = new Color(0.698f, 0.698f, 0.698f);
            colors.disabledColor = new Color(0.521f, 0.521f, 0.521f);
        }

        static public T FindInParents<T>(GameObject go) where T : Component
        {
            if (go == null)
                return null;

            T comp = null;
            Transform t = go.transform;
            while (t != null && comp == null)
            {
                comp = t.GetComponent<T>();
                t = t.parent;
            }
            return comp;
        }

        static public GameObject GetParentActiveCanvasInSelection(bool createIfMissing)
        {
            GameObject go = Selection.activeGameObject;

            // Try to find a gameobject that is the selected GO or one if ots parents
            Canvas p = (go != null) ? FindInParents<Canvas>(go) : null;
            // Only use active objects
            if (p != null && p.gameObject.activeInHierarchy)
                go = p.gameObject;

            // No canvas in selection or its parents? Then use just any canvas.
            if (go == null)
            {
                Canvas canvas = Object.FindObjectOfType(typeof(Canvas)) as Canvas;
                if (canvas != null)
                    go = canvas.gameObject;
            }

            // No canvas present? Create a new one.
            if (createIfMissing && go == null)
                go = MenuOptions2.CreateNewUI();

            return go;
        }

        static public GameObject CreateNewUI()
        {
            GameObject go = null;
            Camera _camera = null;
            var eventobj = GameObject.FindObjectOfType<MyStandaloneInputModule>();

            if (eventobj && eventobj.transform.parent && eventobj.transform.parent.gameObject.name == "UI Root")
            {
                go = eventobj.transform.parent.gameObject;
                var camears = go.FindsInChild<Camera>();
                if(camears.Count > 0)
                    for(int i = 0; i < camears.Count; ++i)
                    {
                        if(camears[i].orthographic && camears[i].cullingMask == 1 << LayerMask.NameToLayer(kUILayerName))
                        {
                            _camera = camears[i];
                            break;
                        }
                    }

                // if there is no event system add one...
                CreateEventSystem(true, go);
            }
            else
            {
                // Root for the UI
                go = new GameObject("UI Root", typeof(RectTransform));
                //go.transform.position = _root_pos;
                go.layer = LayerMask.NameToLayer(kUILayerName);

                _camera = GameObjectUtils.AddChild<Camera>(go);
                _camera.gameObject.name = "UICamera";
                _camera.gameObject.layer = LayerMask.NameToLayer(kUILayerName);
                _camera.cullingMask = 1 << LayerMask.NameToLayer(kUILayerName);
                _camera.clearFlags = CameraClearFlags.Depth;
                _camera.depth = UIDefs.CAMERA_DEPTH_UI;
                _camera.nearClipPlane = UIDefs.CAMERA_NEAR;
                _camera.farClipPlane = UIDefs.CAMERA_FAR;
                _camera.orthographic = true;
                _camera.orthographicSize = UIDefs.CAMERA_SIZE;

                if (_camera.gameObject.GetComponent<AudioListener>())
                {
                    GameObject.DestroyImmediate(_camera.gameObject.GetComponent<AudioListener>());
                }

                CreateEventSystem(true, go);
            }

            var root = new GameObject("UICanvas");
            root.transform.SetParent(go.transform);
            root.layer = LayerMask.NameToLayer(kUILayerName);
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = _camera;
            var scale = root.AddComponent<CanvasScaler>();
            scale.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scale.matchWidthOrHeight = 1;
            scale.referenceResolution = new Vector2(MyUITools.RefScreenWidth, MyUITools.RefScreenHeight);
            root.AddMissingComponent<GraphicRaycaster>();
            root.AddMissingComponent<CanvasGroup>();
            MyComponent myComponent = root.AddMissingComponent<MyComponent>();
            myComponent.editorModel = 1;
            var json = new MyComponentEditor.FieldPackJson();
            json.list.Add(new MyComponentEditor.FieldJson() 
            {
                c = "UI",
                n = "f12",
                v = "0"
            });
            json.list.Add(new MyComponentEditor.FieldJson()
            {
                c = "UI",
                n = "f14",
                v = "0"
            });
            myComponent.configJson = JsonUtility.ToJson(json);
            Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);

           
            return root;
        }

  
        private static void CreateEventSystem(bool select, GameObject parent)
        {
            var esys = Object.FindObjectOfType<EventSystem>();
            if (esys == null)
            {
                var eventSystem = new GameObject("MyEventSystem");
                GameObjectUtility.SetParentAndAlign(eventSystem, parent);
                esys = eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<MyStandaloneInputModule>();

                Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
            }

            if (select && esys != null)
            {
                Selection.activeGameObject = esys.gameObject;
            }
        }

        private static void SetPositionVisibleinSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
        {
            // Find the best scene view
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null && SceneView.sceneViews.Count > 0)
                sceneView = SceneView.sceneViews[0] as SceneView;

            // Couldn't find a SceneView. Don't set position.
            if (sceneView == null || sceneView.camera == null)
                return;

            // Create world space Plane from canvas position.
            Vector2 localPlanePosition;
            Camera camera = sceneView.camera;
            Vector3 position = Vector3.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera, out localPlanePosition))
            {
                // Adjust for canvas pivot
                localPlanePosition.x = localPlanePosition.x + canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
                localPlanePosition.y = localPlanePosition.y + canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;

                localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRTransform.sizeDelta.x);
                localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRTransform.sizeDelta.y);

                // Adjust for anchoring
                position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
                position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;

                Vector3 minLocalPosition;
                minLocalPosition.x = canvasRTransform.sizeDelta.x * (0 - canvasRTransform.pivot.x) + itemTransform.sizeDelta.x * itemTransform.pivot.x;
                minLocalPosition.y = canvasRTransform.sizeDelta.y * (0 - canvasRTransform.pivot.y) + itemTransform.sizeDelta.y * itemTransform.pivot.y;

                Vector3 maxLocalPosition;
                maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1 - canvasRTransform.pivot.x) - itemTransform.sizeDelta.x * itemTransform.pivot.x;
                maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1 - canvasRTransform.pivot.y) - itemTransform.sizeDelta.y * itemTransform.pivot.y;

                position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
                position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
            }

            itemTransform.anchoredPosition = position;
            itemTransform.localRotation = Quaternion.identity;
            itemTransform.localScale = Vector3.one;
        }

        static GameObject CreateUIObject(string name, GameObject parent)
        {
            GameObject go = new GameObject(name);
            go.AddComponent<RectTransform>();
            GameObjectUtility.SetParentAndAlign(go, parent);
            return go;
        }

        private static void SetParentAndAlign(GameObject child, GameObject parent)
        {
            if (parent == null)
                return;

            child.transform.SetParent(parent.transform, false);
            SetLayerRecursively(child, parent.layer);
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }

    }
}
