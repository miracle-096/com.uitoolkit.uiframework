using UIFramework.Editor.Core;
using UIFramework.Editor.CustomElement.Foldout;
using UnityEngine.UIElements;

namespace UIFramework.GenUICode.Component
{
    public class LabelComponent : TComponent
    {
        private FoldoutHeader panel;

        public FoldoutHeader Panel
        {
            get => panel ??= GetPanel<FoldoutHeader>();
            set => panel = value;
        }


        public override void Start()
        {
            element.RegisterCallback<ClickEvent>(OnLabelClick);
        }

        private void OnLabelClick(ClickEvent evt)
        {
            //选中态->折叠展开
            if (Panel.isChecked)
            {
                Panel.isFold = !Panel.isFold;
                foreach (VisualElement visualElement in Panel.RootContainer.parent.Children())
                {
                    if (visualElement != Panel.RootContainer)
                    {
                        visualElement.style.display = Panel.isFold
                            ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex)
                            : new StyleEnum<DisplayStyle>(DisplayStyle.None);
                    }
                }
            }
            

            //1. 取消所有选中态 2. Label、背景颜色样式
            Panel.ChangeLabelClass("label_focus");
            Panel.SwitchFocusBg(true);
            foreach (FoldoutHeader labelFoldout in Panel.toggleParent.toggleGroup)
            {
                labelFoldout.isChecked = false;
                if (labelFoldout != Panel)
                {
                    labelFoldout.ChangeLabelClass("label_blur");
                    labelFoldout.SwitchFocusBg(false);
                }
            }
            
            Panel.isChecked = true;
        }
    }
}