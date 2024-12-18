using System;
using UnityEngine.Serialization;

namespace UnityEngine.EventSystems
{
    public class MyPointerEventData : PointerEventData
    {
        public MyPointerEventData(EventSystem eventSystem) :base(eventSystem)
        {

        }
    }

    [AddComponentMenu("Event/MyStandaloneInputModule")]
    public class MyStandaloneInputModule : PointerInputModule
    {
        //触发长按事件
        public static float LONG_PRESS_TIME = 0.25f;

        //双击事件间隔
        public static float DOUBLE_CLICK_TIME = 0.3f;

        /// <summary>
        /// 当前gameobject是否允许响应触摸屏或鼠标事件,在热更dll中处理
        /// </summary>
        public static Func<GameObject, bool> CanResponseInput;
        /// <summary>
        /// 当前焦点对象
        /// </summary>
        public static GameObject CurrentFocusedGameObject { get { return m_CurrentFocusedGameObject; } }
        public static GameObject CurrentDragGameObject { get { return m_CurrentDragGameObject; } }

        public Action<PointerEventData> OnClickEvent;       
        public Action<PointerEventData> OnLongPressEvent;

        private float m_PrevActionTime;
        private Vector2 m_LastMoveVector;
        private int m_ConsecutiveMoveCount = 0;
        private Vector2 m_LastMousePosition;
        private Vector2 m_MousePosition;
        private static GameObject m_CurrentFocusedGameObject, m_CurrentDragGameObject;

        [SerializeField]
        private string m_HorizontalAxis = "Horizontal";
        [SerializeField]
        private string m_VerticalAxis = "Vertical";
        [SerializeField]
        private string m_SubmitButton = "Submit";
        [SerializeField]
        private string m_CancelButton = "Cancel";
        [SerializeField]
        private float m_InputActionsPerSecond = 10f;
        [SerializeField]
        private float m_RepeatDelay = 0.5f;
        [FormerlySerializedAs("m_AllowActivationOnMobileDevice"), SerializeField]
        private bool m_ForceModuleActive;


        /// <summary>
        ///   <para>Force this module to be active.</para>
        /// </summary>
        public bool forceModuleActive
        {
            get
            {
                return this.m_ForceModuleActive;
            }
            set
            {
                this.m_ForceModuleActive = value;
            }
        }
        /// <summary>
        ///   <para>Number of keyboard / controller inputs allowed per second.</para>
        /// </summary>
        public float inputActionsPerSecond
        {
            get
            {
                return this.m_InputActionsPerSecond;
            }
            set
            {
                this.m_InputActionsPerSecond = value;
            }
        }
        /// <summary>
        ///   <para>Delay in seconds before the input actions per second repeat rate takes effect.</para>
        /// </summary>
        public float repeatDelay
        {
            get
            {
                return this.m_RepeatDelay;
            }
            set
            {
                this.m_RepeatDelay = value;
            }
        }
        /// <summary>
        ///   <para>Input manager name for the horizontal axis button.</para>
        /// </summary>
        public string horizontalAxis
        {
            get
            {
                return this.m_HorizontalAxis;
            }
            set
            {
                this.m_HorizontalAxis = value;
            }
        }
        /// <summary>
        ///   <para>Input manager name for the vertical axis.</para>
        /// </summary>
        public string verticalAxis
        {
            get
            {
                return this.m_VerticalAxis;
            }
            set
            {
                this.m_VerticalAxis = value;
            }
        }
        /// <summary>
        ///   <para>Maximum number of input events handled per second.</para>
        /// </summary>
        public string submitButton
        {
            get
            {
                return this.m_SubmitButton;
            }
            set
            {
                this.m_SubmitButton = value;
            }
        }
        /// <summary>
        ///   <para>Input manager name for the 'cancel' button.</para>
        /// </summary>
        public string cancelButton
        {
            get
            {
                return this.m_CancelButton;
            }
            set
            {
                this.m_CancelButton = value;
            }
        }
        protected MyStandaloneInputModule()
        {

        }
        private bool ShouldIgnoreEventsOnNoFocus()
        {
            bool result;
            switch (SystemInfo.operatingSystemFamily)
            {
                case OperatingSystemFamily.MacOSX:
                case OperatingSystemFamily.Windows:
                case OperatingSystemFamily.Linux:
#if UNITY_EDITOR
                    result = !UnityEditor.EditorApplication.isRemoteConnected;
#else
                    result = true;
#endif
                    break;
                default:
                    result = false;
                    break;
            }
            return result;
        }
        /// <summary>
        ///   <para>See BaseInputModule.</para>
        /// </summary>
        public override void UpdateModule()
        {
            if (base.eventSystem.isFocused || !this.ShouldIgnoreEventsOnNoFocus())
            {
                this.m_LastMousePosition = this.m_MousePosition;
                this.m_MousePosition = base.input.mousePosition;
            }
        }
        /// <summary>
        ///   <para>See BaseInputModule.</para>
        /// </summary>
        /// <returns>
        ///   <para>Supported.</para>
        /// </returns>
        public override bool IsModuleSupported()
        {
            return this.m_ForceModuleActive || base.input.mousePresent || base.input.touchSupported;
        }
        /// <summary>
        ///   <para>See BaseInputModule.</para>
        /// </summary>
        /// <returns>
        ///   <para>Should activate.</para>
        /// </returns>
        public override bool ShouldActivateModule()
        {
            bool result;
            if (!base.ShouldActivateModule())
            {
                result = false;
            }
            else
            {
                bool flag = this.m_ForceModuleActive;
                flag |= base.input.GetButtonDown(this.m_SubmitButton);
                flag |= base.input.GetButtonDown(this.m_CancelButton);
                flag |= !Mathf.Approximately(base.input.GetAxisRaw(this.m_HorizontalAxis), 0f);
                flag |= !Mathf.Approximately(base.input.GetAxisRaw(this.m_VerticalAxis), 0f);
                flag |= ((this.m_MousePosition - this.m_LastMousePosition).sqrMagnitude > 0f);
                flag |= base.input.GetMouseButtonDown(0);
                if (base.input.touchCount > 0)
                {
                    flag = true;
                }
                result = flag;
            }
            return result;
        }
        /// <summary>
        ///   <para>See BaseInputModule.</para>
        /// </summary>
        public override void ActivateModule()
        {
            if (base.eventSystem.isFocused || !this.ShouldIgnoreEventsOnNoFocus())
            {
                base.ActivateModule();
                this.m_MousePosition = base.input.mousePosition;
                this.m_LastMousePosition = base.input.mousePosition;
                GameObject gameObject = base.eventSystem.currentSelectedGameObject;
                if (gameObject == null)
                {
                    gameObject = base.eventSystem.firstSelectedGameObject;
                }
                base.eventSystem.SetSelectedGameObject(gameObject, this.GetBaseEventData());
            }
        }
        /// <summary>
        ///   <para>See BaseInputModule.</para>
        /// </summary>
        public override void DeactivateModule()
        {
            base.DeactivateModule();
            base.ClearSelection();
        }

        public static float ProcessEnd;
        public static float ProcessCost;
        //public static bool HasTouchGameObject = false;
        /// <summary>
        ///   <para>See BaseInputModule.</para>
        /// </summary>
        public override void Process()
        {
            MyTask.Last_opcode = "MyStandaloneInputModule.Process";
            //UnityEngine.Debug.Log($"{Time.frameCount} Process {Time.realtimeSinceStartup}");
            var start_time = Time.realtimeSinceStartup;
            m_CurrentDragGameObject = m_CurrentFocusedGameObject = null;
            //HasTouchGameObject = false;
            //Log.LogInfo($"reset HasTouchGameObject={m_CurrentFocusedGameObject}");
            if (base.eventSystem.isFocused || !this.ShouldIgnoreEventsOnNoFocus())
            {
                bool flag = this.SendUpdateEventToSelectedObject();
                if (base.eventSystem.sendNavigationEvents)
                {
                    if (!flag)
                    {
                        flag |= this.SendMoveEventToSelectedObject();
                    }
                    if (!flag)
                    {
                        this.SendSubmitEventToSelectedObject();
                    }
                }
                if (!ProcessTouchEvents() && input.mousePresent)
                {
                    ProcessMouseEvent();
                }
            }

            ProcessCost = (ProcessEnd = Time.realtimeSinceStartup) - start_time;
            if (ProcessCost > 0.02f)
            {
                if (!BuilderConfig.IsDebugBuild)
                {
                    Log.Log2File($"Process slow, cost={(int)(ProcessCost * 1000)}ms, drag={m_CurrentDragGameObject?.GetLocation()}, focus={m_CurrentFocusedGameObject?.GetLocation()}");
                }
            }
            MyTask.Last_opcode = "MyStandaloneInputModule.Process Done";
        }
        private bool ProcessTouchEvents()
        {
           // string info = $"ProcessTouchEvents:touchCount={input.touchCount}";
           
            for (int i = 0; i < input.touchCount; i++)
            {               
                Touch touch = input.GetTouch(i);
              //  info += $"\n{i}:{touch.type}";
                if (touch.type != TouchType.Indirect)
                {
                    bool pressed;
                    bool released;
                    PointerEventData buttonData = GetTouchPointerEventData(touch, out pressed, out released);
                    if (!m_CurrentFocusedGameObject)
                    {
                        m_CurrentFocusedGameObject = buttonData.pointerCurrentRaycast.gameObject;
                    }
                    if (!m_CurrentDragGameObject)
                    {
                        m_CurrentDragGameObject = buttonData.pointerDrag;
                    }
                    ProcessTouchPress(buttonData, pressed, released);
                    if (!released)
                    {
                        ProcessMove(buttonData);
                        ProcessDrag(buttonData);
                    }
                    else
                    {
                        RemovePointerData(buttonData);
                    }
                }
            }
           // App.ShowAttackInfo(info, Vector2.zero, Color.black, 15);
            return base.input.touchCount > 0;
        }

        protected new PointerEventData GetTouchPointerEventData(Touch input, out bool pressed, out bool released)
        {
            PointerEventData pointerEventData;
            bool pointerData = this.GetPointerData(input.fingerId, out pointerEventData, true);
            pointerEventData.Reset();
            pressed = (pointerData || input.phase == 0);
            released = (input.phase == TouchPhase.Canceled || input.phase == TouchPhase.Ended);
            if (pointerData)
            {
                pointerEventData.position = input.position;
            }
            if (pressed)
            {
                pointerEventData.delta = Vector2.zero;
            }
            else
            {
                pointerEventData.delta = input.position - pointerEventData.position;
            }
            pointerEventData.position = input.position;
            pointerEventData.button = PointerEventData.InputButton.Left;
            eventSystem.RaycastAll(pointerEventData, m_RaycastResultCache);
            RaycastResult pointerCurrentRaycast = BaseInputModule.FindFirstRaycast(m_RaycastResultCache);

            var b = CanResponseInput?.Invoke(pointerCurrentRaycast.gameObject);
            if (b.HasValue && !b.Value)
            {
                pointerCurrentRaycast.gameObject = null;
            }

            pointerEventData.pointerCurrentRaycast = pointerCurrentRaycast;
            m_RaycastResultCache.Clear();
            return pointerEventData;
        }

        /// <summary>
        ///   <para>This method is called by Unity whenever a touch event is processed. Override this method with a custom implementation to process touch events yourself.</para>
        /// </summary>
        /// <param name="pointerEvent">Event data relating to the touch event, such as position and ID to be passed to the touch event destination object.</param>
        /// <param name="pressed">This is true for the first frame of a touch event, and false thereafter. This can therefore be used to determine the instant a touch event occurred.</param>
        /// <param name="released">This is true only for the last frame of a touch event.</param>
        protected void ProcessTouchPress(PointerEventData pointerEvent, bool pressed, bool released)
        {
            GameObject currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;
            if (pressed)
            {
                pointerEvent.eligibleForClick = true;
                pointerEvent.delta = Vector2.zero;
                pointerEvent.dragging = false;
                pointerEvent.useDragThreshold = true;
                pointerEvent.pressPosition = pointerEvent.position;
                pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;
                base.DeselectIfSelectionChanged(currentOverGo, pointerEvent);
                if (pointerEvent.pointerEnter != currentOverGo)
                {
                    base.HandlePointerExitAndEnter(pointerEvent, currentOverGo);
                    pointerEvent.pointerEnter = currentOverGo;
                }
                GameObject newPressed = ExecuteEvents.ExecuteHierarchy<IPointerDownHandler>(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);
                if (newPressed == null)
                {
                    newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
                }
                float unscaledTime = Time.unscaledTime;
                if (newPressed == pointerEvent.lastPress)
                {
                    float num = unscaledTime - pointerEvent.clickTime;
                    if (num < DOUBLE_CLICK_TIME)
                    {
                        pointerEvent.clickCount++;
                    }
                    else
                    {
                        pointerEvent.clickCount = 1;
                    }
                    pointerEvent.clickTime = unscaledTime;
                }
                else
                {
                    pointerEvent.clickCount = 1;
                }
                pointerEvent.pointerPress = newPressed;
                pointerEvent.rawPointerPress = currentOverGo;
                pointerEvent.clickTime = unscaledTime;
                pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);
                if (pointerEvent.pointerDrag != null)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);                    
                }
            }
            if (released)
            {
                ExecuteEvents.Execute<IPointerUpHandler>(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
                GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
                if (pointerEvent.pointerPress == eventHandler && pointerEvent.eligibleForClick)
                {
                    OnClickEvent?.Invoke(pointerEvent);
                    ExecuteEvents.Execute<IPointerClickHandler>(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);                   
                }
                else
                {
                    if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                    {
                        ExecuteEvents.ExecuteHierarchy<IDropHandler>(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
                    }                   
                }

                if(ExecuteEvents.GetEventHandler<IBlackRangeClickHandler>(currentOverGo) == pointerEvent.pointerPress)
                {
                    ExecuteEvents.Execute<IBlackRangeClickHandler>(pointerEvent.pointerPress, pointerEvent, ExecuteEvents2.pointerBlackRangeClickHandler);
                }

                pointerEvent.eligibleForClick = false;
                pointerEvent.pointerPress = null;
                pointerEvent.rawPointerPress = null;
                if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                {
                    ExecuteEvents.Execute<IEndDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
                }

               

                pointerEvent.dragging = false;
                pointerEvent.pointerDrag = null;
                ExecuteEvents.ExecuteHierarchy<IPointerExitHandler>(pointerEvent.pointerEnter, pointerEvent, ExecuteEvents.pointerExitHandler);
                pointerEvent.pointerEnter = null;

                
            }

            if (currentOverGo && pointerEvent.eligibleForClick && ExecuteEvents.GetEventHandler<IPointerLongpressHandler>(currentOverGo) == pointerEvent.pointerPress && pointerEvent.clickCount < 10 && (Time.unscaledTime - pointerEvent.clickTime) >= LONG_PRESS_TIME)
            {
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents2.pointerLongpressHandler);
                if(pointerEvent.pointerPress)
                    OnLongPressEvent?.Invoke(pointerEvent);
                pointerEvent.eligibleForClick = false;
                pointerEvent.clickCount = 10;
                //pointerEvent.pointerPress = null;
                pointerEvent.rawPointerPress = null;
            }
        }
        /// <summary>
        ///   <para>Calculate and send a submit event to the current selected object.</para>
        /// </summary>
        /// <returns>
        ///   <para>If the submit event was used by the selected object.</para>
        /// </returns>
        protected bool SendSubmitEventToSelectedObject()
        {
            bool result;
            if (base.eventSystem.currentSelectedGameObject == null)
            {
                result = false;
            }
            else
            {
                BaseEventData data = this.GetBaseEventData();
                if (base.input.GetButtonDown(this.m_SubmitButton))
                {
                    ExecuteEvents.Execute<ISubmitHandler>(base.eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);
                }
                if (base.input.GetButtonDown(this.m_CancelButton))
                {
                    ExecuteEvents.Execute<ICancelHandler>(base.eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);
                }
                result = data.used;
            }
            return result;
        }
        private Vector2 GetRawMoveVector()
        {
            Vector2 move = Vector2.zero;
            move.x = base.input.GetAxisRaw(this.m_HorizontalAxis);
            move.y = base.input.GetAxisRaw(this.m_VerticalAxis);
            if (base.input.GetButtonDown(this.m_HorizontalAxis))
            {
                if (move.x < 0f)
                {
                    move.x = -1f;
                }
                if (move.x > 0f)
                {
                    move.x = 1f;
                }
            }
            if (base.input.GetButtonDown(this.m_VerticalAxis))
            {
                if (move.y < 0f)
                {
                    move.y = -1f;
                }
                if (move.y > 0f)
                {
                    move.y = 1f;
                }
            }
            return move;
        }
        /// <summary>
        ///   <para>Calculate and send a move event to the current selected object.</para>
        /// </summary>
        /// <returns>
        ///   <para>If the move event was used by the selected object.</para>
        /// </returns>
        protected bool SendMoveEventToSelectedObject()
        {
            float time = Time.unscaledTime;
            Vector2 movement = this.GetRawMoveVector();
            bool result;
            if (Mathf.Approximately(movement.x, 0f) && Mathf.Approximately(movement.y, 0f))
            {
                this.m_ConsecutiveMoveCount = 0;
                result = false;
            }
            else
            {
                bool allow = base.input.GetButtonDown(this.m_HorizontalAxis) || base.input.GetButtonDown(this.m_VerticalAxis);
                bool similarDir = Vector2.Dot(movement, this.m_LastMoveVector) > 0f;
                if (!allow)
                {
                    if (similarDir && this.m_ConsecutiveMoveCount == 1)
                    {
                        allow = (time > this.m_PrevActionTime + this.m_RepeatDelay);
                    }
                    else
                    {
                        allow = (time > this.m_PrevActionTime + 1f / this.m_InputActionsPerSecond);
                    }
                }
                if (!allow)
                {
                    result = false;
                }
                else
                {
                    AxisEventData axisEventData = this.GetAxisEventData(movement.x, movement.y, 0.6f);
                    if (axisEventData.moveDir != MoveDirection.None)
                    {
                        ExecuteEvents.Execute<IMoveHandler>(base.eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
                        if (!similarDir)
                        {
                            this.m_ConsecutiveMoveCount = 0;
                        }
                        this.m_ConsecutiveMoveCount++;
                        this.m_PrevActionTime = time;
                        this.m_LastMoveVector = movement;
                    }
                    else
                    {
                        this.m_ConsecutiveMoveCount = 0;
                    }
                    result = axisEventData.used;
                }
            }
            return result;
        }
        /// <summary>
        ///   <para>Iterate through all the different mouse events.</para>
        /// </summary>
        /// <param name="id">The mouse pointer Event data id to get.</param>
        protected void ProcessMouseEvent()
        {
            ProcessMouseEvent(0);
        }
        [Obsolete("This method is no longer checked, overriding it with return true does nothing!")]
        protected virtual bool ForceAutoSelect()
        {
            return false;
        }
        /// <summary>
        ///   <para>Iterate through all the different mouse events.</para>
        /// </summary>
        /// <param name="id">The mouse pointer Event data id to get.</param>
        protected void ProcessMouseEvent(int id)
        {
            //
            MouseState mousePointerEventData = GetMousePointerEventData(id);
            //
            MouseButtonEventData eventData = mousePointerEventData.GetButtonState(PointerEventData.InputButton.Left).eventData;
            var buttonData = eventData.buttonData;
            if (!m_CurrentFocusedGameObject)
            {
                m_CurrentFocusedGameObject = buttonData.pointerCurrentRaycast.gameObject;
            }
            if (!m_CurrentDragGameObject)
            {
                m_CurrentDragGameObject = buttonData.pointerDrag;
            }

            ProcessMousePress(eventData);
            ProcessMove(buttonData);
            ProcessDrag(buttonData);

            eventData = mousePointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData;
            buttonData = eventData.buttonData;
            if (!m_CurrentFocusedGameObject)
            {
                m_CurrentFocusedGameObject = buttonData.pointerCurrentRaycast.gameObject;
            }
            if (!m_CurrentDragGameObject) 
            {
                m_CurrentDragGameObject = buttonData.pointerDrag;
            }
            ProcessMousePress(eventData);
            ProcessDrag(buttonData);

            eventData = mousePointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData;
            buttonData = eventData.buttonData;
            if (!m_CurrentFocusedGameObject)
            {
                m_CurrentFocusedGameObject = buttonData.pointerCurrentRaycast.gameObject;
            }
            if (!m_CurrentDragGameObject)
            {
                m_CurrentDragGameObject = buttonData.pointerDrag;
            }
            ProcessMousePress(eventData);
            ProcessDrag(buttonData);
            
            if (!Mathf.Approximately(eventData.buttonData.scrollDelta.sqrMagnitude, 0f))
            {
                GameObject eventHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(eventData.buttonData.pointerCurrentRaycast.gameObject);
                ExecuteEvents.ExecuteHierarchy(eventHandler, eventData.buttonData, ExecuteEvents.scrollHandler);
            }
        }
        private readonly MouseState m_MouseState = new MouseState();
        protected override MouseState GetMousePointerEventData(int id)
        {
            PointerEventData pointerEventData;
            bool pointerData = GetPointerData(-1, out pointerEventData, true);
            pointerEventData.Reset();
            if (pointerData)
            {
                pointerEventData.position = input.mousePosition;
            }
            Vector2 mousePosition = input.mousePosition;
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                pointerEventData.position = new Vector2(-1f, -1f);
                pointerEventData.delta = Vector2.zero;
            }
            else
            {
                pointerEventData.delta = mousePosition - pointerEventData.position;
                pointerEventData.position = mousePosition;
            }
            pointerEventData.scrollDelta = input.mouseScrollDelta;
            pointerEventData.button = PointerEventData.InputButton.Left;

            eventSystem.RaycastAll(pointerEventData, m_RaycastResultCache);
            RaycastResult pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);

            var b = CanResponseInput?.Invoke(pointerCurrentRaycast.gameObject);
            if(b.HasValue && !b.Value)
            {
                pointerCurrentRaycast.gameObject = null;
            }
            

            pointerEventData.pointerCurrentRaycast = pointerCurrentRaycast;
            m_RaycastResultCache.Clear();
            PointerEventData pointerEventData2;
            GetPointerData(-2, out pointerEventData2, true);
            CopyFromTo(pointerEventData, pointerEventData2);
            pointerEventData2.button = PointerEventData.InputButton.Right;
            PointerEventData pointerEventData3;
            GetPointerData(-3, out pointerEventData3, true);
            CopyFromTo(pointerEventData, pointerEventData3);

            pointerEventData3.button = PointerEventData.InputButton.Middle;
            m_MouseState.SetButtonState(PointerEventData.InputButton.Left, StateForMouseButton(0), pointerEventData);
            m_MouseState.SetButtonState(PointerEventData.InputButton.Right, StateForMouseButton(1), pointerEventData2);
            m_MouseState.SetButtonState(PointerEventData.InputButton.Middle, StateForMouseButton(2), pointerEventData3);

            return m_MouseState;
        }

        /// <summary>
        ///   <para>Send a update event to the currently selected object.</para>
        /// </summary>
        /// <returns>
        ///   <para>If the update event was used by the selected object.</para>
        /// </returns>
        protected bool SendUpdateEventToSelectedObject()
        {
            bool result;
            if (base.eventSystem.currentSelectedGameObject == null)
            {
                result = false;
            }
            else
            {
                BaseEventData data = this.GetBaseEventData();
                ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
                result = data.used;
            }
            return result;
        }
        /// <summary>
        /// Process the current mouse press.
        /// </summary>
        protected void ProcessMousePress(MouseButtonEventData data)
        {
            PointerEventData pointerEvent = data.buttonData;
            GameObject currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;
            if (data.PressedThisFrame())
            {
                pointerEvent.eligibleForClick = true;
                pointerEvent.delta = Vector2.zero;
                pointerEvent.dragging = false;
                pointerEvent.useDragThreshold = true;
                pointerEvent.pressPosition = pointerEvent.position;
                pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;
                base.DeselectIfSelectionChanged(currentOverGo, pointerEvent);
                GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy<IPointerDownHandler>(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);
                if (gameObject2 == null)
                {
                    gameObject2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
                }
                float unscaledTime = Time.unscaledTime;
                if (gameObject2 == pointerEvent.lastPress)
                {
                    float num = unscaledTime - pointerEvent.clickTime;
                    if (num < DOUBLE_CLICK_TIME)
                    {
                        pointerEvent.clickCount++;
                    }
                    else
                    {
                        pointerEvent.clickCount = 1;
                    }
                    pointerEvent.clickTime = unscaledTime;
                }
                else
                {
                    pointerEvent.clickCount = 1;
                }
                pointerEvent.pointerPress = gameObject2;
                pointerEvent.rawPointerPress = currentOverGo;
                pointerEvent.clickTime = unscaledTime;
                pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);
                if (pointerEvent.pointerDrag != null)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);                    
                }
            }
            if (data.ReleasedThisFrame())
            {
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
                GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
                if (pointerEvent.pointerPress == eventHandler && pointerEvent.eligibleForClick)
                {
                    OnClickEvent?.Invoke(pointerEvent);
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
                }
                else
                {
                    if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                    {
                        ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
                    }
                }
                if (ExecuteEvents.GetEventHandler<IBlackRangeClickHandler>(currentOverGo) == pointerEvent.pointerPress)
                {
                    ExecuteEvents.Execute<IBlackRangeClickHandler>(pointerEvent.pointerPress, pointerEvent, ExecuteEvents2.pointerBlackRangeClickHandler);
                }
                pointerEvent.eligibleForClick = false;
                pointerEvent.pointerPress = null;
                pointerEvent.rawPointerPress = null;
                if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
                }
                pointerEvent.dragging = false;
                pointerEvent.pointerDrag = null;
                if (currentOverGo != pointerEvent.pointerEnter)
                {
                    base.HandlePointerExitAndEnter(pointerEvent, null);
                    base.HandlePointerExitAndEnter(pointerEvent, currentOverGo);
                }

                
            }

            if (currentOverGo && pointerEvent.eligibleForClick && ExecuteEvents.GetEventHandler<IPointerLongpressHandler>(currentOverGo) == pointerEvent.pointerPress && pointerEvent.clickCount < 10 &&  (Time.unscaledTime - pointerEvent.clickTime) >= LONG_PRESS_TIME)
            {
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents2.pointerLongpressHandler);
                if(pointerEvent.pointerPress)
                    OnLongPressEvent?.Invoke(pointerEvent);
                pointerEvent.eligibleForClick = false;
                pointerEvent.clickCount = 10;
               // pointerEvent.pointerPress = null;
                pointerEvent.rawPointerPress = null;                
            }
        }

    }
}
