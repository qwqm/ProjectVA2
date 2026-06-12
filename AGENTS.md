# AGENTS.md

本文件是给后续开发 agent 使用的项目索引。适用范围为仓库根目录及全部子目录。

## 项目概览

- 项目类型：Unity 2D 客户端游戏，Vampire Survivors 类移动/PC 生存射击玩法。
- Unity 版本：`2022.3.62f3c1`，见 `ProjectSettings/ProjectVersion.txt`。
- 主要代码程序集：默认 `Assembly-CSharp` / `Assembly-CSharp-Editor`，当前没有项目自定义 `.asmdef`。
- 主要命名空间：大多数运行时代码在 `Vampire` 命名空间下；`SpatialHashGrid`、`ISpatialHashGridClient`、`ScaleToScreen`、`PhysicsConstants`、`MiscTesting` 当前在全局命名空间。新增运行时代码优先使用 `Vampire`，除非是在扩展现有全局类。
- 输入：使用 Unity Input System 包，`Assets/Input/InputActions.inputactions` 定义 `Player` 与 `UI` action map；`ProjectSettings/ProjectSettings.asset` 中 `activeInputHandler: 2`。
- 本地化：Unity Localization 包，资源位于 `Assets/Localization`，文本表包括 `Ability Text`、`Character Text`、`Game Text`、`UI Text`、`Upgradeable Values`。
- Addressables：项目启用了 Addressables 与 Localization 相关分组，配置在 `Assets/AddressableAssetsData`。
- 当前没有发现 `Assets/Tests` 或自定义测试程序集。验证以 Unity Editor 编译、Play Mode 冒烟测试和目标场景手测为主。

## 不要编辑的生成目录

除非任务明确要求，不要修改或提交这些目录/文件：

- `Library/`
- `Temp/`
- `obj/`
- `Logs/`
- `.vs/`
- `Assembly-CSharp.csproj`
- `Assembly-CSharp-Editor.csproj`
- `ProjectVAI.sln`

新增、移动或删除 Unity 资产时必须保留对应 `.meta` 文件。优先通过 Unity Editor 处理 prefab、scene、asset 的结构性变更；直接手改 YAML 前要确认 GUID、fileID 和序列化字段名。

## 场景与运行入口

构建场景配置在 `ProjectSettings/EditorBuildSettings.asset`：

1. `Assets/Scenes/Game/Main Menu.unity`
2. `Assets/Scenes/Game/Level 1.unity`

辅助场景：

- `Assets/Scenes/Character Set Generation/Character Set Generation.unity`：与角色截图/图集生成工具相关。

主流程：

- 菜单入口：`Assets/Scripts/Main Menu/MainMenu.cs`
- 角色选择：`Assets/Scripts/Main Menu/CharacterSelector.cs`
- 跨场景选择数据：`Assets/Scripts/Main Menu/CrossSceneData.cs`
- 局内入口：`Assets/Scripts/Gameplay/LevelManager.cs`

菜单中 `CharacterSelector.StartGame` 会把选中的 `CharacterBlueprint` 写入 `CrossSceneData.CharacterBlueprint`，然后 `SceneManager.LoadScene(1)` 进入 `Level 1`。局内 `Character.Awake` 从 `CrossSceneData.CharacterBlueprint` 读取角色蓝图。如果直接从 `Level 1` 场景 Play，必须保证场景或调试流程给了可用角色蓝图，否则容易遇到空引用。

## 目录索引

- `Assets/Scripts/Character`：玩家角色、移动、触屏摇杆。
- `Assets/Scripts/Character/Abilities`：技能基类、武器技能、投掷物技能、被动升级技能。
- `Assets/Scripts/Monsters`：怪物基类、普通怪、远程怪、投掷怪、回旋怪、Boss、刷怪表。
- `Assets/Scripts/Monsters/Boss Abilities`：Boss 行为模块。
- `Assets/Scripts/Gameplay`：局内管理、宝箱、计时、属性统计、经验/生命条。
- `Assets/Scripts/Gameplay/Pools`：各类对象池封装。
- `Assets/Scripts/Gameplay/Inventory`：可存储拾取物的快捷栏和槽位。
- `Assets/Scripts/Collectables`：金币、经验、血包、磁铁、炸弹、红药等拾取物。
- `Assets/Scripts/Projectiles`：弹体、爆炸弹体、重力弹体。
- `Assets/Scripts/Throwables`：手雷、燃烧瓶、重力井等投掷物。
- `Assets/Scripts/ScriptableObjects`：角色、关卡、宝箱、金币、经验宝石和怪物蓝图。
- `Assets/Scripts/Spatial`：空间哈希网格，用于怪物附近查询。
- `Assets/Scripts/UI`：升级选择、暂停、结算弹窗。
- `Assets/Scripts/Localization`：语言下拉与字体本地化事件。
- `Assets/Scripts/Utilities`：动画、排序、屏幕缩放、掉落表、升级数值、协程队列等通用工具。
- `Assets/Editor/Scripts`：自定义 Inspector drawer；注意 `LootTableDrawer.cs` 当前整体被注释。
- `Assets/Blueprints`：ScriptableObject 配置实例，是设计数据主入口。
- `Assets/Prefabs`：技能、怪物、弹体、UI、拾取物 prefab。
- `Assets/Materials`、`Assets/Shaders`、`Assets/Sprites`、`Assets/Textures`：渲染与美术资源。

## 核心运行链

`LevelManager.Init` 是局内初始化中心：

- 初始化 `EntityManager`
- 初始化 `AbilityManager`
- 初始化 `AbilitySelectionDialog`
- 初始化玩家 `Character`
- 初始生成经验宝石与宝箱
- 初始化无限背景和背包

`LevelManager.Update` 负责局内时间推进：

- 更新时间 UI
- 按 `LevelBlueprint.monsterSpawnTable` 计算刷怪频率、怪物权重和 HP 倍率
- 到达时间点生成 mini boss
- 到达关卡结束时间生成 final boss，并监听 final boss 死亡触发通关
- 按间隔生成宝箱

`EntityManager` 是实体和对象池中心：

- 初始化怪物、弹体、投掷物、回旋镖、经验、金币、宝箱、伤害文本池
- 负责屏幕外刷怪点、宝箱生成点、全屏伤害、全屏拾取
- 维护 `FastList<Monster>`、`FastList<Collectable>` 和 `SpatialHashGrid`
- 动态按 prefab 创建 projectile/throwable/boomerang pool

`AbilityManager` 是技能选择与升级中心：

- 从玩家蓝图实例化 starting abilities，直接 `Select`
- 从关卡蓝图实例化可掉落技能池
- 通过反射搜集 `Ability` 子类字段里的 `IUpgradeableValue`
- `SelectAbilities` 从已拥有技能和新技能中按 rarity/drop weight 抽选 3 到 4 个选项

## 数据驱动入口

常用配置资产：

- 角色：`Assets/Blueprints/Characters/*.asset`，类型 `CharacterBlueprint`
- 关卡：`Assets/Blueprints/Levels/*.asset`，类型 `LevelBlueprint`
- 怪物：`Assets/Blueprints/Monsters/**/*.asset`，类型 `MonsterBlueprint` 及子类
- 宝箱：`Assets/Blueprints/Chests/*.asset`，类型 `ChestBlueprint`
- 拾取物类型：`Assets/Blueprints/CollectableTypes/*.asset`，类型 `CollectableType`
- 经验/金币外观枚举数据：`Assets/Blueprints/Exp Gem`、`Assets/Blueprints/Coin`

重要数据结构：

- `LevelBlueprint.abilityPrefabs`：关卡可出现的技能 prefab 列表。
- `LevelBlueprint.monsters`：怪物 pool prefab 与蓝图列表。`MonsterIndexMap` 把刷怪表中的扁平怪物索引映射回 pool index 和 blueprint index。
- `LevelBlueprint.monsterSpawnTable`：归一化时间 `t` 下的刷怪频率、怪物概率和 HP 加成。
- `LootTable<T>` / `Loot<T>`：怪物、宝箱掉落权重。
- `EnumDataContainer<TEnum, TValue>`：按枚举存储图像/数值等 Inspector 数据。

## 常见开发任务

### 新增技能或武器

1. 在 `Assets/Scripts/Character/Abilities` 选择合适基类：
   - 弹体武器：继承 `ProjectileAbility`
   - 投掷物：继承 `ThrowableAbility`
   - 近战：继承 `MeleeAbility`、`SlashAbility` 或 `StabAbility`
   - 被动升级：继承 `FloatUpgradeAbility<T>` 或 `IntUpgradeAbility<T>`
2. 如需新弹体/投掷物，新增对应 prefab 和脚本，并确保由 `EntityManager.AddPoolForProjectile` / `AddPoolForThrowable` 管理。
3. 新建或复制 `Assets/Prefabs/Abilities/...` 下的 ability prefab，填好 sprite、本地化名称/描述、rarity、升级数组和依赖 prefab/layer。
4. 把 ability prefab 加入目标 `LevelBlueprint.abilityPrefabs`。
5. 补齐 `Assets/Localization/Tables/Ability Text` 和 `Upgradeable Values` 中相关文案。
6. 在 `Main Menu` 正常选角色进入 `Level 1`，验证升级弹窗能抽到、选择后能生效、升级描述正确。

### 新增怪物

1. 从 `Monster`、`MeleeMonster`、`RangedMonster`、`ThrowingMonster`、`BoomerangMonster` 或 `BossMonster` 选择基类。
2. 新增怪物 prefab 到 `Assets/Prefabs/Monsters`，确认 Rigidbody2D、Collider2D、SpriteAnimator、材质和图层配置。
3. 新增对应 `MonsterBlueprint` 或子类实例到 `Assets/Blueprints/Monsters`。
4. 在目标 `LevelBlueprint.monsters` 中添加 prefab/blueprint，并同步检查 `monsterSpawnTable.spawnChanceKeyframes` 和 `hpMultiplierKeyframes` 数组长度。这里的数组长度必须覆盖扁平怪物索引数量。
5. 如果是 boss，配置 `miniBosses` 或 `finalBoss`，并确认 boss prefab 使用 `BossMonster` 和所需 `BossAbility`。

### 新增拾取物或道具

1. 新增 `CollectableType` 子类和 `Assets/Blueprints/CollectableTypes` 实例；`inventoryStackSize > 0` 表示进入背包槽。
2. 新增 `Collectable` 子类，实现 `OnCollected`。
3. 新增 prefab 到 `Assets/Prefabs/Chest` 或相应目录，确保 Collider2D、sprite、type、pool/init 依赖正确。
4. 如果需要可存储，在 `Inventory` 场景对象的 `inventorySlots` 中绑定相同 `CollectableType`。
5. 更新宝箱 `ChestBlueprint.lootTable` 或怪物掉落配置。

### 修改关卡节奏

- 优先改 `Assets/Blueprints/Levels/Level 1.asset`。
- `levelTime` 决定 final boss 出现时间。
- `monsterSpawnTable.spawnRateKeyframes` 控制刷怪速率。
- `spawnChanceKeyframes` 的概率建议每个 keyframe 合计约为 1。
- `hpMultiplierKeyframes` 的每个数组长度要与可刷怪物总数一致。
- `miniBosses[0]` 当前被 `LevelManager.Update` 直接索引，删除或清空 mini boss 配置前必须先改代码防空。

### 修改输入

- 键鼠/手柄绑定在 `Assets/Input/InputActions.inputactions`。
- 玩家移动回调：`Character.SetMoveDirection`。
- 触屏摇杆：`TouchJoystick` 通过 UnityEvent 把方向传给角色移动；当前 `SendValueToControl` 逻辑被注释。
- 物品快捷键：`InputActions` 中 `North/East/South/West Powerup` 绑定 `Z/X/C/V` 与 `1/2/3/4`，场景中应连接到对应 `InventorySlot.UseItem`。

### 修改 UI 和暂停

- 升级弹窗：`AbilitySelectionDialog`，打开时 `Time.timeScale = 0`，关闭恢复为 1。
- 暂停：`PauseMenu`，用 `TimeIsFrozen` 避免升级弹窗期间暂停按钮恢复时间。
- 结算：`GameOverDialog`，由 `LevelManager.GameOver` / `LevelPassed` 打开。
- 金币持久化：`PlayerPrefs` key 为 `Coins`。

### 修改本地化

- 文本表在 `Assets/Localization/Tables`。
- 技能名和描述使用 `LocalizedString`，见 `Ability`、`AbilityCard`。
- 升级数值名称来自 `LocalizationSettings.StringDatabase.GetLocalizedString("Upgradeable Values", "...")`。
- 字体本地化通过 `LocalizeFontEvent` 和 `Assets/Localization/Tables/Fonts`。
- 终端里部分历史中文注释可能显示乱码，编辑脚本时尽量保持文件编码稳定，不要批量重编码无关文件。

## 代码约定与易踩点

- 运行时代码大多用 public field 或 `[SerializeField] protected/private` 暴露给 Unity Inspector。重命名序列化字段会破坏 prefab/scene/asset 引用，除非同步处理迁移。
- `Ability.Init` 用反射查找当前类字段中的 `IUpgradeableValue`。新增可升级字段时必须是字段，不要只写属性。
- `UpgradeableValue<T>.Upgrade` 对 float 是乘法倍率，对 int 是加法。配置升级数组时要按这个含义填写。
- `Ability.RequirementsMet` 依赖 `maxLevel`。没有 upgradeable values 的技能需要自行确认 `maxLevel` 行为，避免永远不可选或异常。
- `EntityManager` 的动态 pool 按 prefab 去重。不要绕过它直接频繁 `Instantiate/Destroy` 战斗对象。
- `FastList<T>` 要求元素唯一；重复 Add 或移除不存在元素会输出错误。
- `SpatialHashGrid.UpdateClient` 需要调用者在对象移动时触发。新增依赖空间查询的对象时确认是否插入、更新、移除。
- `Monster.Killed` 会 `OnKilled.Invoke` 后 `RemoveAllListeners` 并释放回池。复用对象时不要假设旧监听仍存在。
- `Projectile.Setup` 每次重建 `OnHitDamageable`，监听应在每次 spawn 后重新添加。
- `Chest.Open` 中已有注释说明逻辑脆弱；改宝箱流程时重点测 ability chest、failsafe chest、普通 loot chest。
- 直接从 `Level 1` Play 可能没有 `CrossSceneData.CharacterBlueprint`。调试时优先从 `Main Menu` 进入，或临时在场景/代码中注入默认角色。
- `LevelManager` 直接访问 `miniBosses[0]`。关卡蓝图必须至少有一个 mini boss，除非先加防御代码。
- `MonsterSpawnTable.SelectMonsterWithHPMultiplier` 在找不到匹配时返回 `(-1, 0)`；改刷怪表时要防止概率/时间 keyframe 不完整导致索引错误。
- `ProjectSettings/ProjectSettings.asset` 中产品名是 `project_mgd_vampire`，Android 包名是 `com.DefaultCompany.project_mgd_vampire`。

## 验证建议

首选 Unity Editor 验证：

1. 使用 Unity `2022.3.62f3c1` 打开项目，等待脚本编译无错误。
2. 打开 `Assets/Scenes/Game/Main Menu.unity`，Play 后选择角色进入 `Level 1`。
3. 验证 WASD/方向键/触屏摇杆移动。
4. 拾取经验触发升级弹窗，选择新技能和升级项。
5. 验证宝箱、金币、背包道具、暂停、结算弹窗。
6. 切换语言，确认 UI 和技能文案不缺 key。

## 快速定位表

- 局内初始化/胜负/刷怪计时：`Assets/Scripts/Gameplay/LevelManager.cs`
- 实体生成/对象池/空间网格：`Assets/Scripts/Monsters/EntityManager.cs`
- 玩家生命、经验、移动：`Assets/Scripts/Character/Character.cs`
- 技能抽取与升级：`Assets/Scripts/Character/Abilities/AbilityManager.cs`
- 技能基类与本地化描述：`Assets/Scripts/Character/Abilities/Ability.cs`
- 可升级数值类型：`Assets/Scripts/Utilities/UpgradeableValues.cs`
- 怪物基类与掉落：`Assets/Scripts/Monsters/Monster.cs`
- 刷怪表算法：`Assets/Scripts/Monsters/MonsterSpawnTable.cs`
- 弹体生命周期：`Assets/Scripts/Projectiles/Projectile.cs`
- 拾取物生命周期：`Assets/Scripts/Collectables/Collectable.cs`
- 宝箱逻辑：`Assets/Scripts/Gameplay/Chest.cs`
- 背包逻辑：`Assets/Scripts/Gameplay/Inventory/Inventory.cs`
- 升级弹窗：`Assets/Scripts/UI/AbilitySelectionDialog.cs`
- 角色选择与跨场景数据：`Assets/Scripts/Main Menu/CharacterSelector.cs`、`Assets/Scripts/Main Menu/CrossSceneData.cs`
- 输入配置：`Assets/Input/InputActions.inputactions`
- 构建场景顺序：`ProjectSettings/EditorBuildSettings.asset`
