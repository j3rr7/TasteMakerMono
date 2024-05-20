using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TasteMakerMono
{
    public class Cheat : MonoBehaviour
    {
        public static Rect Window = new Rect(10, 10, 250, 200);
        public static Rect Window1 = new Rect(10, 50, 400, 200);
        public static Rect Window2 = new Rect(10, 90, 400, 200);

        private bool windowVisible = true;
        private bool window1Visible = true;
        private bool window2Visible = true;

        private readonly List<(string label, IngredientType type)> ingredients = new List<(string label, IngredientType type)>
        {
            ("Add Alcohol", IngredientType.Alcohol),
            ("Add Avocado", IngredientType.Avocado),
            ("Add Berries", IngredientType.Berries),
            ("Add Chicken", IngredientType.Chicken),
            ("Add Chocolate", IngredientType.Chocolate),
            ("Add CoffeeBean", IngredientType.CoffeeBean),
            ("Add Flour", IngredientType.Flour),
            ("Add Milk", IngredientType.Milk),
            ("Add Patatoes", IngredientType.Patatoes),
            ("Add Steak", IngredientType.Steak),
            ("Add Tomatoes", IngredientType.Tomatoes),
            ("Add Wine", IngredientType.Wine)
        };
        private readonly List<(string label, TablewareType type)> tableware = new List<(string label, TablewareType type)>
        {
            ("Add Bowl", TablewareType.Bowl),
            ("Add Glass", TablewareType.Glass),
            ("Add Mug", TablewareType.Mug),
            ("Add Pan", TablewareType.Pan),
            ("Add Plate", TablewareType.Plate),
            ("Add WineGlass", TablewareType.WineGlass)
        };

        private Dictionary<Type, object> managers = new Dictionary<Type, object>();

        private HotkeyManager hotkeyManager = new HotkeyManager();
        private Scene activeSceneRef = default;

        private bool waiterNoRest = false;
        private bool removeWaitTime = false;

        public void Start()
        {
            UnityEngine.Debug.unityLogger.logEnabled = true;

            //if (CustomConsole.Instance != null)
            //    CustomConsole.Instance.Initialize();

            activeSceneRef = SceneManager.GetActiveScene();
            SceneManager.activeSceneChanged += UpdateActiveSceneCallback;
            FillImportantObjects();

            hotkeyManager.RegisterHotkey(
                KeyCode.Insert, () =>
                {
                    this.windowVisible = !this.windowVisible;
                    this.window1Visible = !this.window1Visible;
                    this.window2Visible = !this.window2Visible;
                }
            );

            hotkeyManager.RegisterHotkey(KeyCode.Backslash, () =>
            {
                var mm = GetManager<MoneyManager>();
                var g = GetManager<Grid>();
                if (mm != null && g != null)
                {
                    mm.ReceiveMoney(g.MouseWorldPosition, 1000);
                }
            });
        }

        private void UpdateActiveSceneCallback(Scene previousScene, Scene newScene)
        {
            activeSceneRef = newScene;
            FillImportantObjects();
        }

        public void OnDestroy()
        {
            UnityEngine.Debug.unityLogger.logEnabled = false;
        }

        private void FindManager<T>() where T : Component
        {
            if (!managers.TryGetValue(typeof(T), out object manager) || manager == null)
            {
                StartCoroutine(
                    Utils.FindObjectOfTypeWithRetries<T>(3, 2, onFound: (foundManager) =>
                    {
                        managers[typeof(T)] = foundManager;
                    })
                );
            }
        }

        //public static T GetManager<T>(this Dictionary<Type, object> managers) where T : class
        //{
        //    if (managers.TryGetValue(typeof(T), out object manager))
        //    {
        //        return (T)manager;
        //    }
        //    throw new KeyNotFoundException($"Manager of type {typeof(T)} not found.");
        //}

        public T GetManager<T>() where T : class
        {
            if (this.managers.TryGetValue(typeof(T), out object manager))
            {
                return (T)manager;
            }
            throw new KeyNotFoundException($"Manager of type {typeof(T)} not found.");
        }

        private void FillImportantObjects()
        {
            // Clear Dict
            managers.Clear();

            FindManager<GameManager>();
            FindManager<BladderManager>();
            FindManager<FoodOrderManager>();
            FindManager<GuestManager>();
            FindManager<HappinessManager>();
            FindManager<HygeneManager>();
            FindManager<KitchenManager>();
            FindManager<MoneyManager>();
            FindManager<OrderManager>();
            FindManager<ScenarioManager>();
            FindManager<ObjectManager>();
            FindManager<WaiterManager>();
            FindManager<StorageManager>();
            FindManager<Grid>();
        }

        public void Update()
        {
            if (hotkeyManager != null)
                hotkeyManager.HandleHotkeys();
        }

        public void FixedUpdate()
        {

        }

        public void LateUpdate()
        {
            if (waiterNoRest)
            {
                var wm = GetManager<WaiterManager>();
                if (wm != null)
                {
                    foreach (Waiter waiter in wm.waiters)
                    {
                        if (!waiter.fired)
                        {
                            waiter.lastRestTime = 0;
                        }
                    }
                }
            }
            if (removeWaitTime)
            {
                var sm = GetManager<StorageManager>();
                if (sm != null)
                {
                    if (sm.startedIngredientDeliveryTimer)
                        sm.DeliveryTimeSeconds = 0f;
                    if (sm.startedUtillityDeliveryTimer)
                        sm.DeliveryTimeUtillitySeconds = 0f;
                }
            }
        }

        public void OnGUI()
        {
            if (windowVisible)
            {
                Window = GUILayout.Window(0, Window, (GUI.WindowFunction)WindowContent, "Main Window", new GUILayoutOption[0]);
            }
            if (window1Visible)
            {
                Window1 = GUILayout.Window(1, Window1, (GUI.WindowFunction)WindowContent, "Trainer Winow", new GUILayoutOption[0]);
            }
            if (window2Visible)
            {
                Window2 = GUILayout.Window(2, Window2, (GUI.WindowFunction)WindowContent, "Inventory", new GUILayoutOption[0]);
            }
        }

        public void WindowContent(int windowId)
        {
            switch (windowId)
            {
                case 2:
                    DrawWindow2();
                    break;
                case 1:
                    DrawWindow1();
                    break;
                case 0:
                default:
                    DrawMainWindow();
                    break;
            }

            GUI.DragWindow();
        }

        private void DrawMainWindow()
        {
            if (GetManager<GameManager>() == null)
            {
                GUILayout.Label("Please wait...");
            }

            GUILayout.Label("Show / Hide menu (Insert)");
            if (GUILayout.Button("GC Collect"))
            {
                GC.Collect();
            }
            if (GUILayout.Button("Reload all objects"))
            {
                FillImportantObjects();
            }
        }

        private void DrawWindow1()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Force Add Money"))
            {
                var mm = GetManager<MoneyManager>();
                if (mm != null)
                {
                    mm.money += 1000f;
                    Singleton<MessageBox>.Instance.ForceDisplayMessage("+Money", MessageType.Info);
                }
            }

            if (GUILayout.Button("Recieve Money"))
            {
                var mm = GetManager<MoneyManager>();
                var g = GetManager<Grid>();
                if (mm != null && g != null)
                {
                    mm.ReceiveMoney(g.MouseWorldPosition, 1000);
                }
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Make Happy"))
            {
                var gm = GetManager<GuestManager>();
                if (gm != null)
                {
                    var guests = Utils.GetPrivateFieldValue<List<Guest>>(gm, "guests");
                    foreach (Guest guest in guests)
                    {
                        guest.stats.GeneralHappiness.Value = 1000.0f;
                        guest.stats.Price.Value = 1000.0f;
                        guest.stats.FoodQuallity.Value = 1000.0f;
                        guest.stats.Decor.Value = 1000.0f;
                        guest.stats.Hygene.Value = 1000.0f;
                        guest.stats.Sound.Value = 1000.0f;
                        guest.stats.WaitingTime.Value = 1000.0f;
                        guest.stats.Bladder.Value = 1000.0f;                
                        guest.stats.Menu.Value = 1000.0f;
                    }
                }
            }

            if (GUILayout.Button("Quick Deliver Goods"))
            {
                var sm = GetManager<StorageManager>();
                if (sm != null)
                {
                    if (sm.startedIngredientDeliveryTimer)
                        sm.DeliveryTimeSeconds = 0f;
                    if (sm.startedUtillityDeliveryTimer)
                        sm.DeliveryTimeUtillitySeconds = 0f;
                    sm.DeliveryTimeSeconds = 0f;
                    sm.DeliveryTimeUtillitySeconds = 0f;
                }
            }

            if (GUILayout.Button("Complete Scenario"))
            {
                var sm = GetManager<ScenarioManager>();
                if (sm != null)
                {
                    if (sm.currentScenario != null)
                    {
                        for (int i = 0; i < sm.currentScenario.challanges.Count; i++)
                        {
                            if (sm.currentScenario.challanges[i].state == ChallangeState.InProgress)
                            {
                                sm.currentScenario.challanges[i].state = ChallangeState.Completed;
                                PlayerPrefs.SetInt(sm.currentScenario.name + i, 1);
                            }
                        }
                    }
                }
            }

            if (GUILayout.Button("Free Waiters Wage"))
            {
                var wm = GetManager<WaiterManager>();
                if (wm != null)
                {
                    wm.AssistantWage = 0;
                    wm.CatWage = 0;
                    wm.chefWage = 0;
                    wm.dealerWage = 0;
                    wm.generalWage = 0;
                    wm.janitorWage = 0;
                    wm.waiterWage = 0;

                    foreach (Waiter waiter in wm.waiters)
                    {
                        if (!waiter.fired)
                        {
                            waiter.stats.wage = 0;
                        }
                    }
                }
            }

            waiterNoRest = GUILayout.Toggle(waiterNoRest, "Waiter No Rest", new GUILayoutOption[0]);

            if (GUILayout.Button("Waiter Max Stats"))
            {
                var wm = GetManager<WaiterManager>();
                if (wm != null)
                {
                    foreach (Waiter waiter in wm.waiters)
                    {
                        if (!waiter.fired)
                        {
                            waiter.stats.clumsyness = 0f;
                            waiter.stats.cooking = 1000f;
                            waiter.stats.speed = 1000f;
                            waiter.stats.wage = 0;
                        }
                    }
                }
            }
        }

        private void DrawWindow2()
        {
            removeWaitTime = GUILayout.Toggle(removeWaitTime, "Remove Wait Time", new GUILayoutOption[0]);

            GUILayout.BeginHorizontal();
            CreateButtonSM(ingredients[0].label, ingredients[0].type, (sm, it) => sm.AddIngredient(it));
            CreateButtonSM(ingredients[1].label, ingredients[1].type, (sm, it) => sm.AddIngredient(it));
            CreateButtonSM(ingredients[2].label, ingredients[2].type, (sm, it) => sm.AddIngredient(it));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            CreateButtonSM(ingredients[3].label, ingredients[3].type, (sm, it) => sm.AddIngredient(it));
            CreateButtonSM(ingredients[4].label, ingredients[4].type, (sm, it) => sm.AddIngredient(it));
            CreateButtonSM(ingredients[5].label, ingredients[5].type, (sm, it) => sm.AddIngredient(it));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            CreateButtonSM(ingredients[6].label, ingredients[6].type, (sm, it) => sm.AddIngredient(it));
            CreateButtonSM(ingredients[7].label, ingredients[7].type, (sm, it) => sm.AddIngredient(it));
            CreateButtonSM(ingredients[8].label, ingredients[8].type, (sm, it) => sm.AddIngredient(it));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            CreateButtonSM(ingredients[9].label, ingredients[9].type, (sm, it) => sm.AddIngredient(it));
            CreateButtonSM(ingredients[10].label, ingredients[10].type, (sm, it) => sm.AddIngredient(it));
            CreateButtonSM(ingredients[11].label, ingredients[11].type, (sm, it) => sm.AddIngredient(it));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            CreateButtonSM(tableware[0].label, tableware[0].type, (sm, it) => sm.AddTableware(it, true));
            CreateButtonSM(tableware[1].label, tableware[1].type, (sm, it) => sm.AddTableware(it, true));
            CreateButtonSM(tableware[2].label, tableware[2].type, (sm, it) => sm.AddTableware(it, true));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            CreateButtonSM(tableware[3].label, tableware[3].type, (sm, it) => sm.AddTableware(it, true));
            CreateButtonSM(tableware[4].label, tableware[4].type, (sm, it) => sm.AddTableware(it, true));
            CreateButtonSM(tableware[5].label, tableware[5].type, (sm, it) => sm.AddTableware(it, true));
            GUILayout.EndHorizontal();
        }

        #region Usefull Function / Methods
        private void CreateButtonSM<T>(string buttonLabel, T parameter, Action<StorageManager, T> action)
        {
            if (GUILayout.Button(buttonLabel))
            {
                var sm = GetManager<StorageManager>();
                if (sm != null)
                    action(sm, parameter);
                else
                    Singleton<MessageBox>.Instance.ForceDisplayMessage("No Storage Manager Found.", MessageType.Warning);
            }
        }
        #endregion
    }
}
