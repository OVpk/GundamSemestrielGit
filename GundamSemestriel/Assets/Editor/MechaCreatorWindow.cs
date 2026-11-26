using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MechaCreatorWindow : EditorWindow
{
    // ----- Data -----
    List<MechaBaseData> _mechasBase = new List<MechaBaseData>();
    List<ModuleData> _moduleDatas = new List<ModuleData>();

    MechaBaseData _selectedMechaBase;
    List<ModuleData> _selectedModules = new List<ModuleData>(5);

    // Modules grouped by set, rebuilt in Refresh()
    class ModuleGroup
    {
        public string title;
        public List<ModuleData> modules = new List<ModuleData>();
    }
    List<ModuleGroup> _moduleGroups = new List<ModuleGroup>();

    // Icons found in IconsFolder, by (normalized) file name
    Dictionary<string, Texture2D> _icons = new Dictionary<string, Texture2D>();

    // ----- Calculated stats -----
    EntityRuntimeData _currentStats;
    bool _hasStats;
    List<KeyValuePair<string, string>> _statLines = new List<KeyValuePair<string, string>>();
    List<KeyValuePair<string, string>> _activeSetLines = new List<KeyValuePair<string, string>>();

    Vector2 scrollPos = Vector2.zero;

    // ----- Mecha preview layout -----
    const float DesignWidth = 250f;
    const float DesignHeight = 300f;
    const float PreviewMargin = 10f;
    const float ListWidth = 300f;       // fixed width of the Bases/Modules column
    const float ColumnGap = 100f;       // space between the lists and the mecha draw
    const float MinPreviewWidth = 250f; // the draw never gets smaller than this
    const float StatsWidth = 200f;      // fixed width of the stats column (right of the draw)
    const float StatsGap = 10f;         // space between the draw and the stats column

    // ----- List rows -----
    const float IconSize = 22f;
    const float EditButtonWidth = 40f;
    static readonly Color IconBackground = new Color(0.78f, 0.78f, 0.78f); // the icons are black, they need a light background

    const string NoSetTitle = "No Set";

    // ----- Project paths -----
    const string IconsFolder = "Assets/Textures/UI/Inons";
    const string MechaPrefabPath = "Assets/Prefabs/Mecha.prefab";   // prefab the variants are made from
    const string DefaultSaveFolder = "Assets/Prefabs";

    // ----- Naming convention -----
    const string BasePrefix = "Base_";

    static readonly Color SelectedColor = new Color(0.2f, 0.75f, 0.3f);

    struct PartSlot
    {
        public string name;
        public string prefix;   // asset name prefix that belongs to this slot
        public string iconName; // icon file name (no extension), null = no icon
        public Rect rect;

        public PartSlot(string name, string prefix, string iconName, float x, float y, float w, float h)
        {
            this.name = name;
            this.prefix = prefix;
            this.iconName = iconName;
            rect = new Rect(x, y, w, h);
        }
    }

    // Slots sizes. The order of the non-base slots is also the order of the
    // modules saved in MechaTestProto.modules (Head, L_Arm, R_Arm, L_Leg, R_Leg).
    static readonly PartSlot[] _slots =
    {
        new PartSlot("Head",  "Mod_Head_", "Tête",          107f,  11f,  36f,  36f),
        new PartSlot("Torso", BasePrefix,  null,             81f,  54f,  88f, 125f),
        new PartSlot("L_Arm", "Mod_Arm_L_", "Bras_Gauche",   39f,  59f,  29f, 123f),
        new PartSlot("R_Arm", "Mod_Arm_R_", "Bras_Droit",   182f,  59f,  29f, 123f),
        new PartSlot("L_Leg", "Mod_Leg_L_", "Jambe_Gauche",  78f, 191f,  27f,  96f),
        new PartSlot("R_Leg", "Mod_Leg_R_", "Jambe_Droite", 145f, 191f,  27f,  96f),
    };

    // Cached styles (created lazily, they need EditorStyles/GUI.skin to be ready)
    GUIStyle _entryStyle;
    GUIStyle _entrySelectedStyle;
    GUIStyle _partLabelStyle;

    [MenuItem("Tools/Mecha Creator")]
    static void Open()
    {
        MechaCreatorWindow customWindow = GetWindow<MechaCreatorWindow>("MechaCreator");
        customWindow.minSize = new Vector2(ListWidth + ColumnGap + MinPreviewWidth + StatsGap + StatsWidth, 400);
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void OnProjectChange()
    {
        Refresh();
        Repaint();
    }

    private void Refresh()
    {
        _mechasBase.Clear();
        foreach (string GUID in AssetDatabase.FindAssets("t:MechaBaseData"))
        {
            string Path = AssetDatabase.GUIDToAssetPath(GUID);
            MechaBaseData mechaBase = AssetDatabase.LoadAssetAtPath<MechaBaseData>(Path);

            _mechasBase.Add(mechaBase);
        }

        _moduleDatas.Clear();
        foreach (string GUID in AssetDatabase.FindAssets("t:ModuleData"))
        {
            string Path = AssetDatabase.GUIDToAssetPath(GUID);
            ModuleData module = AssetDatabase.LoadAssetAtPath<ModuleData>(Path);

            _moduleDatas.Add(module);
        }

        BuildModuleGroups();
        LoadIcons();

        // Drop selections whose asset has been deleted from the project.
        _selectedModules.RemoveAll(m => m == null);
        if (_selectedMechaBase == null)
            _selectedMechaBase = null;

        // Assets may have been edited, so the stats are recomputed too.
        RecalculateStats();
    }

    // Groups the modules by set: groups sorted by set name, modules sorted by name,
    // modules without a set go in a last group.
    private void BuildModuleGroups()
    {
        _moduleGroups.Clear();

        Dictionary<SetData, ModuleGroup> bySet = new Dictionary<SetData, ModuleGroup>();
        ModuleGroup noSet = null;

        foreach (ModuleData module in _moduleDatas)
        {
            if (module == null) continue;

            if (module.Set == null)
            {
                if (noSet == null)
                    noSet = new ModuleGroup { title = NoSetTitle };
                noSet.modules.Add(module);
            }
            else
            {
                if (!bySet.TryGetValue(module.Set, out ModuleGroup group))
                {
                    group = new ModuleGroup { title = module.Set.name };
                    bySet.Add(module.Set, group);
                }
                group.modules.Add(module);
            }
        }

        _moduleGroups.AddRange(bySet.Values);
        _moduleGroups.Sort((a, b) => string.Compare(a.title, b.title, StringComparison.OrdinalIgnoreCase));

        if (noSet != null)
            _moduleGroups.Add(noSet);

        foreach (ModuleGroup group in _moduleGroups)
            group.modules.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
    }

    // Loads every texture of IconsFolder, indexed by its name.
    private void LoadIcons()
    {
        _icons.Clear();

        if (!AssetDatabase.IsValidFolder(IconsFolder))
        {
            Debug.LogWarning($"[MechaCreator] Icons folder '{IconsFolder}' not found.");
            return;
        }

        foreach (string GUID in AssetDatabase.FindAssets("t:Texture2D", new[] { IconsFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(GUID);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            if (texture != null)
                _icons[NormalizeName(texture.name)] = texture;
        }
    }

    // Accented names ("Tête") can be stored in different Unicode forms depending on the OS.
    private static string NormalizeName(string name)
    {
        return name.Normalize(NormalizationForm.FormC);
    }

    private void OnGUI()
    {
        EnsureStyles();

        EditorGUILayout.BeginHorizontal();

        DrawModuleLists();      // left
        GUILayout.Space(ColumnGap);
        DrawMechaPreview();     // middle
        GUILayout.Space(StatsGap);
        DrawStats();            // right

        EditorGUILayout.EndHorizontal();
    }

    private void EnsureStyles()
    {
        if (_entryStyle == null)
        {
            _entryStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft };
            _entrySelectedStyle = new GUIStyle(_entryStyle) { fontStyle = FontStyle.Bold };
        }

        if (_partLabelStyle == null)
        {
            _partLabelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                clipping = TextClipping.Clip
            };
            _partLabelStyle.normal.textColor = Color.white;
        }
    }

    #region Display Mecha Rect

    private void DrawMechaPreview()
    {
        // No fixed width: this column takes ALL the space left by the other columns
        // and the gaps, so the draw is rescaled whenever the window is resized.
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));

        // Reserve the space in the layout (must run on Layout AND Repaint events).
        Rect area = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        if (Event.current.type == EventType.Repaint)
        {
            area.x += PreviewMargin;
            area.y += PreviewMargin;
            area.width -= PreviewMargin * 2f;
            area.height -= PreviewMargin * 2f;

            if (area.width > 1f && area.height > 1f)
            {
                float scale = Mathf.Min(area.width / DesignWidth, area.height / DesignHeight);
                float panelWidth = DesignWidth * scale;
                float panelHeight = DesignHeight * scale;

                Rect cardRect = new Rect(
                    area.x + (area.width - panelWidth) / 2f,
                    area.y + (area.height - panelHeight) / 2f,
                    panelWidth,
                    panelHeight);

                EditorGUI.DrawRect(cardRect, Color.darkGray);

                _partLabelStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(5f * scale), 9, 14);

                foreach (PartSlot slot in _slots)
                {
                    Rect partRect = Part(cardRect, scale, slot.rect);
                    string selectedName = GetSelectedName(slot);

                    if (selectedName != null)
                    {
                        EditorGUI.DrawRect(partRect, SelectedColor);
                        GUI.Label(partRect, DisplayName(selectedName, slot.prefix), _partLabelStyle);
                    }
                    else
                    {
                        EditorGUI.DrawRect(partRect, Color.crimson);
                    }
                }
            }
        }

        GUILayout.Space(4f);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Clear", GUILayout.Height(24f)))
        {
            ClearSelection();
        }

        // The save button only exists once the mecha is complete (base + every module slot).
        if (IsMechaComplete())
        {
            if (GUILayout.Button(new GUIContent("Save", "Save this mecha as a prefab variant"), GUILayout.Height(24f)))
            {
                SaveMechaAsPrefabVariant();
                GUIUtility.ExitGUI(); // a modal dialog was opened from inside OnGUI
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    static Rect Part(Rect cardRect, float scale, Rect designRect)
    {
        return Part(cardRect, scale, designRect.x, designRect.y, designRect.width, designRect.height);
    }

    static Rect Part(Rect cardRect, float scale, float x, float y, float w, float h)
    {
        return new Rect(
            cardRect.x + x * scale,
            cardRect.y + y * scale,
            w * scale,
            h * scale
        );
    }

    #endregion

    #region Display Stats

    private void DrawStats()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(StatsWidth));

        EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);

        if (_selectedMechaBase == null)
        {
            EditorGUILayout.HelpBox("Select a base to see the stats.", MessageType.Info);
        }
        else if (!_hasStats)
        {
            EditorGUILayout.HelpBox("Stats could not be calculated.", MessageType.Warning);
        }
        else
        {
            foreach (KeyValuePair<string, string> line in _statLines)
            {
                EditorGUILayout.LabelField(line.Key, line.Value);
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Active sets", EditorStyles.boldLabel);

        if (_activeSetLines.Count == 0)
        {
            EditorGUILayout.LabelField("None", EditorStyles.miniLabel);
        }
        else
        {
            foreach (KeyValuePair<string, string> line in _activeSetLines)
            {
                EditorGUILayout.LabelField(line.Key, line.Value);
            }
        }

        EditorGUILayout.EndVertical();
    }

    // Runs the stat calculator and caches the result as (label, value) lines so
    // OnGUI only has to display strings. Called whenever the selection changes.
    private void RecalculateStats()
    {
        RecalculateActiveSets();

        _statLines.Clear();
        _hasStats = false;

        if (_selectedMechaBase == null)
            return;

        _currentStats = MechaStatCalculator.CalculateRuntimeData(_selectedMechaBase, _selectedModules);

        object boxed = _currentStats;
        if (boxed == null)
            return;

        // Lists every public field / property of EntityRuntimeData, so the panel
        // doesn't depend on its member names.
        Type type = boxed.GetType();
        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

        foreach (FieldInfo field in type.GetFields(flags))
        {
            _statLines.Add(new KeyValuePair<string, string>(
                ObjectNames.NicifyVariableName(field.Name),
                FormatValue(field.GetValue(boxed))));
        }

        foreach (PropertyInfo property in type.GetProperties(flags))
        {
            if (!property.CanRead || property.GetIndexParameters().Length > 0)
                continue;

            _statLines.Add(new KeyValuePair<string, string>(
                ObjectNames.NicifyVariableName(property.Name),
                FormatValue(property.GetValue(boxed))));
        }

        _hasStats = true;
    }

    // Same rule as MechaStatCalculator: a set bonus is active when the number of
    // equipped modules of that set >= bonus.RequiredSetNb.
    private void RecalculateActiveSets()
    {
        _activeSetLines.Clear();

        Dictionary<SetData, int> setCounts = new Dictionary<SetData, int>();
        foreach (ModuleData module in _selectedModules)
        {
            if (module == null || module.Set == null) continue;

            setCounts.TryGetValue(module.Set, out int count);
            setCounts[module.Set] = count + 1;
        }

        List<KeyValuePair<SetData, int>> sets = new List<KeyValuePair<SetData, int>>(setCounts);
        sets.Sort((a, b) => string.Compare(a.Key.name, b.Key.name, StringComparison.OrdinalIgnoreCase));

        foreach (KeyValuePair<SetData, int> setAndCount in sets)
        {
            SetData setData = setAndCount.Key;
            int equippedCount = setAndCount.Value;

            if (setData.SetBonus == null) continue;

            List<KeyValuePair<string, string>> bonusLines = new List<KeyValuePair<string, string>>();

            foreach (ConditionnalSubstat bonus in setData.SetBonus)
            {
                if (equippedCount < bonus.RequiredSetNb) continue;

                SubStat subStat = bonus.SubStat;
                string value = subStat.StatValue.ToString("+0.##;-0.##");
                if (subStat.AdditiveType == SubStatType.Percent)
                    value += "%";

                bonusLines.Add(new KeyValuePair<string, string>(
                    $"   {bonus.RequiredSetNb} pieces",
                    $"{ObjectNames.NicifyVariableName(subStat.Stat.ToString())} {value}"));
            }

            // Only sets with at least one active bonus are listed.
            if (bonusLines.Count == 0) continue;

            _activeSetLines.Add(new KeyValuePair<string, string>(setData.name, $"{equippedCount} pieces"));
            _activeSetLines.AddRange(bonusLines);
        }
    }

    private static string FormatValue(object value)
    {
        if (value == null) return "-";
        if (value is float f) return f.ToString("0.##");
        return value.ToString();
    }

    #endregion

    #region Display Modules

    private void DrawModuleLists()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(ListWidth));
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.LabelField("Bases", EditorStyles.boldLabel);
        if (_mechasBase.Count == 0)
        {
            EditorGUILayout.HelpBox("No mechaBase was found in the project", MessageType.Info);
        }
        else
        {
            foreach (MechaBaseData mechaBase in _mechasBase)
            {
                if (mechaBase == null) continue;

                // Bases have no icon.
                if (DrawEntry(mechaBase, mechaBase == _selectedMechaBase, false, null))
                    ToggleBase(mechaBase);
            }
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Modules", EditorStyles.boldLabel);
        if (_moduleGroups.Count == 0)
        {
            EditorGUILayout.HelpBox("No mecha's module was found in the project", MessageType.Info);
        }
        else
        {
            foreach (ModuleGroup group in _moduleGroups)
            {
                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField(group.title, EditorStyles.boldLabel);

                foreach (ModuleData module in group.modules)
                {
                    if (module == null) continue;

                    if (DrawEntry(module, _selectedModules.Contains(module), true, GetModuleIcon(module.name)))
                        ToggleModule(module);
                }
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    // Draws one row: [icon] [name button] [check mark] [Edit button when selected].
    // Returns true when the name was clicked.
    private bool DrawEntry(UnityEngine.Object asset, bool selected, bool showIcon, Texture2D icon)
    {
        bool clicked;

        EditorGUILayout.BeginHorizontal();

        if (showIcon)
            DrawIcon(icon);

        clicked = GUILayout.Button(asset.name, selected ? _entrySelectedStyle : _entryStyle);

        // Always reserve the check mark space so rows stay aligned.
        GUIContent check = selected ? EditorGUIUtility.IconContent("TestPassed") : GUIContent.none;
        GUILayout.Label(check, GUILayout.Width(20f), GUILayout.Height(18f));

        if (selected)
        {
            if (GUILayout.Button("Edit", GUILayout.Width(EditButtonWidth)))
                OpenInInspector(asset);
        }
        else
        {
            GUILayout.Space(EditButtonWidth + 4f); // same width as the button
        }

        EditorGUILayout.EndHorizontal();

        return clicked;
    }

    // The icons are black silhouettes, so they are drawn on a light square.
    private static void DrawIcon(Texture2D icon)
    {
        Rect rect = GUILayoutUtility.GetRect(IconSize, IconSize, GUILayout.Width(IconSize), GUILayout.Height(IconSize));

        if (Event.current.type != EventType.Repaint)
            return;

        EditorGUI.DrawRect(rect, IconBackground);

        if (icon != null)
        {
            Rect inner = new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f);
            GUI.DrawTexture(inner, icon, ScaleMode.ScaleToFit, true);
        }
    }

    // Selects the asset (main Inspector) and opens it in its own Inspector window.
    private static void OpenInInspector(UnityEngine.Object asset)
    {
        Selection.activeObject = asset;
        EditorUtility.OpenPropertyEditor(asset);
    }

    private Texture2D GetModuleIcon(string moduleName)
    {
        foreach (PartSlot slot in _slots)
        {
            if (slot.iconName == null) continue;

            if (moduleName.StartsWith(slot.prefix, StringComparison.Ordinal))
            {
                _icons.TryGetValue(NormalizeName(slot.iconName), out Texture2D icon);
                return icon;
            }
        }
        return null;
    }

    #endregion

    #region Selection

    private void ToggleBase(MechaBaseData mechaBase)
    {
        _selectedMechaBase = (_selectedMechaBase == mechaBase) ? null : mechaBase;
        RecalculateStats();
    }

    private void ToggleModule(ModuleData module)
    {
        // Already selected -> deselect
        if (_selectedModules.Remove(module))
        {
            RecalculateStats();
            return;
        }

        string prefix = GetSlotPrefix(module.name);
        if (prefix == null)
        {
            Debug.LogWarning($"[MechaCreator] '{module.name}' doesn't match any slot prefix (Mod_Head_, Mod_Arm_L_, Mod_Arm_R_, Mod_Leg_L_, Mod_Leg_R_).");
            return;
        }

        // One module per slot: replace the one already in that slot.
        _selectedModules.RemoveAll(m => m.name.StartsWith(prefix, StringComparison.Ordinal));
        _selectedModules.Add(module);
        RecalculateStats();
    }

    private void ClearSelection()
    {
        _selectedMechaBase = null;
        _selectedModules.Clear();
        RecalculateStats();
    }

    // Returns the slot prefix an asset name belongs to, or null if none matches.
    private static string GetSlotPrefix(string assetName)
    {
        foreach (PartSlot slot in _slots)
        {
            if (slot.prefix != BasePrefix && assetName.StartsWith(slot.prefix, StringComparison.Ordinal))
                return slot.prefix;
        }
        return null;
    }

    // Module currently selected for this (non-base) slot, or null if empty.
    private ModuleData GetSelectedModule(PartSlot slot)
    {
        foreach (ModuleData module in _selectedModules)
        {
            if (module != null && module.name.StartsWith(slot.prefix, StringComparison.Ordinal))
                return module;
        }
        return null;
    }

    // Name of the asset currently selected for this slot, or null if empty.
    private string GetSelectedName(PartSlot slot)
    {
        if (slot.prefix == BasePrefix)
            return _selectedMechaBase != null ? _selectedMechaBase.name : null;

        ModuleData module = GetSelectedModule(slot);
        return module != null ? module.name : null;
    }

    // Return assetName directly instead if you'd rather show the full name.
    private static string DisplayName(string assetName, string prefix)
    {
        string shortName = assetName.Substring(prefix.Length);
        return string.IsNullOrEmpty(shortName) ? assetName : shortName;
    }

    // Complete = a base and one module in every non-base slot.
    private bool IsMechaComplete()
    {
        if (_selectedMechaBase == null)
            return false;

        foreach (PartSlot slot in _slots)
        {
            if (slot.prefix != BasePrefix && GetSelectedModule(slot) == null)
                return false;
        }
        return true;
    }

    #endregion

    #region Save

    // Creates a prefab variant of the Mecha prefab whose MechaTestProto holds the
    // current base and modules.
    private void SaveMechaAsPrefabVariant()
    {
        GameObject sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MechaPrefabPath);
        if (sourcePrefab == null)
        {
            Debug.LogError($"[MechaCreator] Mecha prefab not found at '{MechaPrefabPath}'.");
            return;
        }

        string path = EditorUtility.SaveFilePanelInProject(
            "Save mecha", "Mecha_New", "prefab",
            "Choose where to save the new mecha prefab variant.", DefaultSaveFolder);

        if (string.IsNullOrEmpty(path))
            return; // cancelled

        if (path == MechaPrefabPath)
        {
            Debug.LogError("[MechaCreator] Choose a different file: this one is the source prefab.");
            return;
        }

        // Modules in slot order (Head, L_Arm, R_Arm, L_Leg, R_Leg)
        List<ModuleData> modules = new List<ModuleData>();
        foreach (PartSlot slot in _slots)
        {
            if (slot.prefix != BasePrefix)
                modules.Add(GetSelectedModule(slot));
        }

        // The variant is built from an instance, which lives in a preview scene so the
        // open scene is left untouched.
        Scene previewScene = EditorSceneManager.NewPreviewScene();
        try
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefab, previewScene);
            MechaTestProto proto = instance.GetComponent<MechaTestProto>();

            if (proto == null)
            {
                Debug.LogError("[MechaCreator] The Mecha prefab has no MechaTestProto component.");
                return;
            }

            SerializedObject serializedProto = new SerializedObject(proto);
            SerializedProperty baseProperty = serializedProto.FindProperty("baseData");
            SerializedProperty modulesProperty = serializedProto.FindProperty("modules");

            if (baseProperty == null || modulesProperty == null)
            {
                Debug.LogError("[MechaCreator] Fields 'baseData' / 'modules' not found on MechaTestProto.");
                return;
            }

            baseProperty.objectReferenceValue = _selectedMechaBase;

            modulesProperty.arraySize = modules.Count;
            for (int i = 0; i < modules.Count; i++)
                modulesProperty.GetArrayElementAtIndex(i).objectReferenceValue = modules[i];

            serializedProto.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.RecordPrefabInstancePropertyModifications(proto);

            // Saving an instance of a prefab as a new asset creates a Prefab Variant.
            PrefabUtility.SaveAsPrefabAsset(instance, path, out bool success);

            if (success)
            {
                Debug.Log($"[MechaCreator] Mecha saved as prefab variant: {path}");
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(path));
            }
            else
            {
                Debug.LogError($"[MechaCreator] Could not save the prefab variant at '{path}'.");
            }
        }
        finally
        {
            EditorSceneManager.ClosePreviewScene(previewScene);
        }
    }

    #endregion
}