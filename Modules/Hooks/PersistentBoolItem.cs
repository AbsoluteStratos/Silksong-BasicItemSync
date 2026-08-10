namespace BasicItemSync.Modules.Hooks;

using System;
using UnityEngine;

// Token: 0x020007D3 RID: 2003
public abstract class PersistentItemMock : MonoBehaviour
{
    public event SetValueEvent? OnSetSaveState;

    public event GetValueEvent? OnGetSaveState;

    public event Action? SemiPersistentReset;

    public PersistentItemData<bool> ItemData
    {
        get
        {
            EnsureSetup();
            return itemData;
        }
    }

    protected bool DefaultValue => false;

    protected PersistentItemData<bool> SerializedItemData => itemData;

    public bool HasLoadedValue { get; private set; }

    public bool LoadedValue { get; private set; }

    protected virtual void Awake()
    {
        itemData ??= new PersistentItemData<bool>();
        saveCondition ??= new PlayerDataTest();
        
        gm = GameManager.instance;
        gm.SavePersistentObjects += SaveState;

        if (disablePrefabIfActivated)
        {
            OnSetSaveState += (value) =>
            {
                if (value) disablePrefabIfActivated.SetActive(false);
            };
        }
        if (disableIfActivated)
        {
            OnSetSaveState += (value) =>
            {
                if (value) gameObject.SetActive(false);
            };
        }
    }

    private void OnEnable()
    {
        if (SerializedItemData.IsSemiPersistent)
        {
            gm.ResetSemiPersistentObjects += ResetState;
        }

        if (OnGetSaveState == null)
        {
            fsm = LookForMyFSM();
        }
    }

    private void OnDisable()
    {
        if (gm != null)
        {
            gm.ResetSemiPersistentObjects -= ResetState;
        }
    }

    private void OnDestroy()
    {
        if (gm != null)
        {
            gm.SavePersistentObjects -= SaveState;
        }
    }

    private void Start()
    {
        started = true;
        EnsureSetup();
        CheckIsValid();

        PersistentItemData<bool> persistentItemData;
        var found = false;
        if (SceneData.instance.PersistentBools.TryGetValue(itemData.SceneName, itemData.ID, out persistentItemData))
        {
            itemData.Value = persistentItemData.Value;
            found = true;
        }


        if (found)
        {
            OnSetSaveState?.Invoke(itemData.Value);

            if (fsm == null) fsm = LookForMyFSM();
            if (fsm != null) fsm.FsmVariables.FindFsmBool("Activated").Value = itemData.Value; // SetValueOnFSM

            HasLoadedValue = true;
            LoadedValue = itemData.Value;
            return;
        }

        UpdateValue();
    }

    private void UpdateValue()
    {
        if (isValueOverridden) return;

        if (OnGetSaveState != null)
        {
            OnGetSaveState(out ItemData.Value);
            return;
        }
        UpdateActivatedFromFSM();
    }

    //public void SetValueOverride(bool value)
    //{
    //    ItemData.Value = value;
    //    isValueOverridden = true;
    //}
    //public bool GetCurrentValue()
    //{
    //    UpdateValue();
    //    return ItemData.Value;
    //}

    private void CheckIsValid()
    {
        Type type = GetType();
        if (GetComponents(type).Length > 1)
        {
            Debug.LogError(string.Format("There is more than one component of type: <b>{0}</b> on <b>{1}</b>, please remove one!", type, gameObject.name), this);
        }
    }

    public void SaveState()
    {
        if (saveCondition.IsDefined && !saveCondition.IsFulfilled) return;

        SaveStateNoCondition();
    }

    public void SaveStateNoCondition()
    {
        EnsureSetup();

        if (!isValueOverridden)
        {
            if (OnGetSaveState != null)
            {
                OnGetSaveState(out itemData.Value);
            }
            else
            {
                UpdateActivatedFromFSM();
            }
        }

        HasLoadedValue = true;
        LoadedValue = itemData.Value;

        if (dontSave) return;

        SceneData.instance.PersistentBools.SetValue(itemData); // SaveValue
    }

    private void ResetState()
    {
        if (!itemData.IsSemiPersistent) return;

        SaveState();
        if (itemData.Value.Equals(DefaultValue)) return;

        itemData.Value = DefaultValue;
        SemiPersistentReset?.Invoke();

        if (fsm != null) fsm.SendEvent("RESET");
    }

    private void EnsureSetup()
    {
        if (hasSetup) return;
        
        hasSetup = true;
        itemData = SerializedItemData;

        if (string.IsNullOrEmpty(itemData.ID))
        {
            itemData.ID = name;
        }
        if (string.IsNullOrEmpty(itemData.SceneName))
        {
            itemData.SceneName = GameManager.GetBaseSceneName(gameObject.scene.name);
        }
    }

    private void UpdateActivatedFromFSM()
    {
        if (fsm != null)
        {
            itemData.Value = fsm.FsmVariables.FindFsmBool("Activated").Value; // GetValueFromFSM
            return;
        }

        fsm = LookForMyFSM();
    }

    protected virtual PlayMakerFSM LookForMyFSM()
    {
        PlayMakerFSM[] components = GetComponents<PlayMakerFSM>();
        if (components == null) return null;

        return FSMUtility.FindFSMWithPersistentBool(components);
    }



    [SerializeField]
    private PlayerDataTest saveCondition;

    private bool dontSave;

    private bool hasSetup;

    private bool isValueOverridden;

    private GameManager gm;

    private PlayMakerFSM fsm;

    private PersistentItemData<bool> itemData;

    private bool started;

    public delegate void SetValueEvent(bool value);

    public delegate void GetValueEvent(out bool value);

    private bool disableIfActivated;

    private GameObject disablePrefabIfActivated;
}

