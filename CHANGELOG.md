# Changelog

## [2.0.0] - 2026-7-30
- 完整重构命名体系，不保留旧类型、旧成员或兼容别名；控制流、绑定顺序、数组索引和池化行为保持不变
- Runtime 类型：`CodeBindAttribute` → `BindingRootAttribute`、`MonoCodeBindAttribute` → `MonoBehaviourBindingAttribute`、`CodeBindNameAttribute` → `BindingTargetTokenAttribute`
- PlainClass 模式：`CSCodeBindMono` → `PlainClassBindingHost`、`ICSCodeBind` → `IPlainClassBinding`、`CSCodeBindPool` → `PlainClassBindingPool`、`CodeBindExtension` → `PlainClassBindingExtensions`
- NamedReference 模式：`ReferenceBindMono` → `NamedReferenceBindingHost`
- PlainClass 生成协议：`BindMono/CachedTransform/InitBind/ClearBind` → `BindingHost/RootTransform/Initialize/Reset`，获取入口改为 `GetPlainClassBinding<T>`
- NamedReference 查询：`GetGameObject/GetAs/GetList` → `GetManualGameObject/GetAutoTarget/GetAutoTargets`
- Editor 扩展 API：`CodeBindData` → `BindingDescriptor`、`ICodeBindCustomizer` → `IBindingCodeCustomizer`、`ICodeBindNameTypeConfig` → `IBindingTargetTokenConfig`
- Editor 核心职责类型改为 `HierarchyBindingProcessor`、`ExistingScriptBindingGenerator`、`NewScriptBindingGenerator`、`BindingSourceBuilder`、各模式 Binder/Generator/Inspector/Window 与 Registry
- Host 序列化字段、Inspector `FindProperty`、生成字段、反射字符串和 Demo Scene YAML 同步迁移；脚本 `.meta` GUID 与对象引用保持不变
- Host Hierarchy token：`CSCodeBindMono` → `PlainClassBindingHost`、`ReferenceBindMono` → `NamedReferenceBindingHost`
- EditorPrefs key 改为 `CodeBind.OutputPath`、`CodeBind.DefaultNamespace`、`CodeBind.NameSeparator`；SessionState key 改为 `CodeBind.PendingBindingSerialization`
- 菜单统一为 MonoBehaviour/Plain Class Binding Generator，以及 Refresh All Binding Sources/Serialized Bindings
- 修正文档中 wildcard、默认 PascalCase 和绑定描述数据的错误说明

## [1.1.0] - 2026-6-15
- 精简ICodeBindCustomizer命名接口：GetFieldName/GetPropertyName直接接收拼好的组合名，数组后缀固定为Array由框架拼接，移除单独的数组命名方法
- 公共字段与属性统一使用大写开头（PascalCase），生成的属性名也保持大写开头，私有字段保持 m_ 前缀
- ICSCodeBind 的 Mono/Transform 重命名为 BindMono/CachedTransform
- 添加ICodeBindCustomizer接口，支持自定义命名风格和额外代码生成，带优先级，高优先级覆盖低优先级
- DefaultCodeBindNameTypeConfig改为ICodeBindNameTypeConfig接口实现，支持多实现按优先级合并覆盖
- 移除CodeBindNameTypeAttribute，由ICodeBindNameTypeConfig替代
- 移除内置STATE_CONTROLLER_CODE_BIND支持，可通过ICodeBindCustomizer自行实现
- 统一日志为英文并添加[CodeBind]前缀

## [1.0.7] - 2025-1-22
- 添加Mono类型自动绑定引用

## [1.0.6] - 2024-5-28
- ReferenceBindMono添加自动数据绑定

## [1.0.5] - 2024-3-15
- 添加STATE_CONTROLLER_CODE_BIND可以支持StateController数据代码生成
- 优化类型收集调用时机，降低ReloadDomain耗时

## [1.0.4] - 2024-3-13
- 支持继承的名称准确识别

## [1.0.3] - 2024-3-9
- fix the problem of nested judgment error

## [1.0.2] - 2024-1-26
- change name style

## [1.0.1] - 2024-1-20
- support bind gameObject
- fix binding partially duplicated components, the name may be modified.
- add bind editor group view.

## [1.0.0] - 2023-10-18