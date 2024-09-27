using System;
using System.Collections.Generic;
using UIFramework.Editor.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIFramework.Core
{
    /// <summary>
    /// 编辑器ui panel容器
    /// </summary>
    public abstract class UIWindow : EditorWindow
    {
        public virtual Vector2 MIN_SIZE => new Vector2(0, 0);
        public static Action<KeyCode> OnKeyDown;
        public static Action<KeyCode> OnKeyUp;
        public static Action<string> OnCommand;

        public static Dictionary<VisualElement, VisualObject>
            AllObjects = new Dictionary<VisualElement, VisualObject>();

        private static Dictionary<Type, Action<EComponent, VisualElement>> eventHandlerFactory;

        public static Dictionary<Type, Action<EComponent, VisualElement>> EventHandlerFactory
        {
            get
            {
                eventHandlerFactory ??= new Dictionary<Type, Action<EComponent, VisualElement>>();
                var ass = typeof(DragState).Assembly;
                var types = ass.GetTypes();
                foreach (var item in types)
                {
                    if (!item.IsInterface || item.GetInterface("IUIEvent") == null ||
                        eventHandlerFactory.ContainsKey(item)) continue;
                    if (item == typeof(IDoubleClickUIEvent))
                        eventHandlerFactory.Add(item,
                            (component, target) =>
                            {
                                DoubleClickHandler.RegisterDoubleClickEvent(component as IDoubleClickUIEvent, target,
                                    300);
                            });
                    else if (item == typeof(IReceiveDragUIEvent))
                        eventHandlerFactory.Add(item,
                            (component, target) =>
                            {
                                ReceiveDragEventHandler.RegisterReceiveDragEvent(component as IReceiveDragUIEvent,
                                    target);
                            });
                    else if (item == typeof(IDraggableUIEvent))
                        eventHandlerFactory.Add(item,
                            (component, target) =>
                            {
                                DraggableEventHandler.RegisterDragEvent(component as IDraggableUIEvent, target);
                            });
                }

                return eventHandlerFactory;
            }
        }

        public EPanel rootEPanel { get; private set; }

        protected virtual void OnEnable()
        {
            WindowManager.RegisterWindow(GetType(), this);
            titleContent = new GUIContent(GetType().Name);
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void OnDestroy()
        {
            if (rootEPanel != null)
            {
                rootEPanel.OnDestroy();
                EPanel.Destroy(rootEPanel);
                rootEPanel = null;
            }

            //To-do: Same as the above.
            WindowManager.UnRegisterWindow(GetType());
        }

        public static VisualObject Find(VisualElement element)
        {
            if (AllObjects.ContainsKey(element)) return AllObjects[element];
            var vo = new VisualObject();
            vo.element = element;
            AllObjects.Add(element, vo);
            return vo;
        }

        public void OpenView(params object[] objs)
        {
            var view = MakeView(objs);
            view.Window = this;
            rootEPanel = view;
        }
        protected abstract EPanel MakeView(params object[] objs);

        protected virtual void OnGUI()
        {
            var currentEvent = Event.current;
            if (currentEvent != null)
            {
                if (currentEvent.type == EventType.ValidateCommand)
                {
                    OnCommand?.Invoke(currentEvent.commandName);
                }

                var keyCode = currentEvent.keyCode;
                if (keyCode != KeyCode.None)
                {
                    if (currentEvent.type == EventType.KeyDown)
                    {
                        OnKeyDown?.Invoke(keyCode);
                    }
                    else if (currentEvent.type == EventType.KeyUp)
                    {
                        OnKeyUp?.Invoke(keyCode);
                    }
                }
            }

            foreach (var visualObject in AllObjects)
            {
                if (rootVisualElement.Contains(visualObject.Key))
                {
                    foreach (var component in visualObject.Value.AllComponents)
                    {
                        component.Value.OnGUI();
                    }
                }
            }
        }

        protected virtual void Update()
        {
            foreach (var visualObject in AllObjects)
            {
                if (rootVisualElement.Contains(visualObject.Key))
                {
                    foreach (var component in visualObject.Value.AllComponents)
                    {
                        component.Value.Update();
                    }
                }
            }
        }
    }
}